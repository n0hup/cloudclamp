namespace CloudClamp

open Amazon
open Amazon.S3
open Amazon.S3.Model
open Amazon.S3.Util
open System
open System.Collections.Generic

open Config
open AwsS3Acl
open AwsS3GetBucket
open AwsS3PutBucket
open AwsS3DeleteBucket

module AwsS3 =

  let createAwsS3Config awsRegionName =
    try
      let region = RegionEndpoint.GetBySystemName(awsRegionName)
      let config = AmazonS3Config()
      config.MaxConnectionsPerServer <- new Nullable<int>(64)
      config.RegionEndpoint <- region
      Ok config
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message);
      Environment.Exit(1);
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  let getAwsS3Client awsCredentials (awsS3Config:AmazonS3Config) =
    try
      Console.WriteLine("Connecting to S3...")
      Console.WriteLine(
        "AWS S3 Config: RegionEndpoint :: {0} MaxConnectionsPerServer :: {1} BufferSize :: {2} ServiceVersion :: {3}",
        awsS3Config.RegionEndpoint,
        awsS3Config.MaxConnectionsPerServer,
        awsS3Config.BufferSize,
        awsS3Config.ServiceVersion
      )
      Ok(new AmazonS3Client(awsCredentials, awsS3Config))
    with ex ->
      Console.Error.WriteLine("Connecting to S3 has failed")
      let err = String.Format("{0} : {1}", ex.Message, ex.InnerException.Message)
      Console.Error.WriteLine err
      Error err

  let (|AllowedS3Region|_|) (region:string, allowedAwsRegions) =
    match region, Array.contains region allowedAwsRegions with
      | "eu-central-1", true  -> Some S3Region.EUC1
      // Europe (Ireland) eu-west-1
      | "eu-west-1", true     -> Some S3Region.EUW1
      // Europe (London) eu-west-2
      | "eu-west-2", true     -> Some S3Region.EUW2
      // Europe (Paris) eu-west-3
      | "eu-west-3", true     -> Some S3Region.EUW3
      // Europe (Stockholm) eu-north-1
      | "eu-north-1", true    -> Some S3Region.EUN1
      | _, _ -> None

  let convertToS3Tags (tags:list<string * string>) =
    let awsS3Tags = List<Tag>()
    for tag in tags do
      let tba : Tag =  Tag()
      tba.Key   <- fst tag
      tba.Value <- snd tag
      awsS3Tags.Add(tba)
    Some awsS3Tags

  type WebsiteDocuments = {
    IndexDocument : string ;
    ErrorDocument : string ;
  }

  type RedirectOnly = {
    RedirectTo : string
  }

  type RedirectOnlyConfig = {
    Bucket  : string;
    Acl     : PublicReadAcl;
    Region  : S3Region;
    Website : RedirectOnly;
    Tags    : Option<List<Tag>> ;
    Policy  : Option<string>;
  }

  type WebsiteConfig = {
    Bucket  : string;
    Acl     : PublicReadAcl;
    Region  : S3Region;
    Website : WebsiteDocuments;
    Tags    : Option<List<Tag>> ;
    Policy  : Option<string>;
  }

  type PrivateBucketConfig = {
    Bucket  : string;
    Acl     : PrivateAcl;
    Region  : S3Region;
    Tags    : Option<List<Tag>> ;
    Policy  : Option<string>;
  }

  type S3BucketConfig =
    | Website of config   : WebsiteConfig
    | Private of config   : PrivateBucketConfig
    | Redirect of config  : RedirectOnlyConfig

  type Event = 
    | GetState 
    | PutBucket 
    | DeleteBucket 
    | PutBucketTagging
    | PutBucketWebsite
    | PutBucketPolicy
    | DeleteBucketTagging
    | DeleteBucketWebsite
    | DeleteBucketPolicy

  type State = 
    | Initial
    | Err
    | NonExistent
    | Created of
        name    : string *
        acl     : S3AccessControlList *
        tags    : Option<List<Tag>> * 
        website : Option<WebsiteConfiguration> * 
        policy  : Option<string>

  let getState (amazonS3client:AmazonS3Client) (bucket:string) : State =
    try
      let task = AmazonS3Util.DoesS3BucketExistV2Async(amazonS3client, bucket)
      task.Wait()
      if task.IsCompletedSuccessfully then
        if task.Result then
          // basic
          let bucketAcl     = getBucketAcl      amazonS3client bucket
          // extra
          let bucketTags    = getBucketTagging  amazonS3client bucket
          let bucketWesite  = getBucketWebsite  amazonS3client bucket
          let bucketPolicy  = getBucketPolicy   amazonS3client bucket
          
          // TODO nul pointer!!! None.Value
          Created(name=bucket, acl=bucketAcl.Value, tags=bucketTags, website=bucketWesite, policy=bucketPolicy)
          
        else
          NonExistent
      else
        Console.Error.WriteLine(String.Format("Could not get bucket state: {0}", bucket))
        Err
    with ex ->
      Console.Error.WriteLine(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
      Err

  let transition (amazonS3client:AmazonS3Client) basicConfig websiteConfig redirectConfig (state:State) (event:Event) =

    match state, event, basicConfig, websiteConfig, redirectConfig with
      | Initial, GetState, (bucket, _, _, _,_), _, _     -> getState amazonS3client bucket
      // from nonexistent to created
      | NonExistent, PutBucket, (bucket, acl, region, _,_), _, _   -> 
          match putBucket amazonS3client bucket region acl with
            | None ->
                Err
            | Some _v ->  
              match getBucketAcl amazonS3client bucket with
                | Some acl ->  Created (name = bucket, acl = acl, tags=None, website=None, policy=None)
                | None ->
                    Err
      
      // from created to created   - Put                  

      | Created (name, acl, _, website, policy), PutBucketTagging, (bucket, _, _, Some tags,_), _, _ ->      
          match putBucketTagging amazonS3client bucket tags with
            | None -> Err
            | Some _ -> Created (name=name, acl=acl, tags=Some tags, website=website, policy=policy)

      | Created (name, acl, tags, _, policy), PutBucketWebsite, (bucket, _, _, _, _), Some websiteConfig, None ->      
          match putBucketWebsite amazonS3client bucket websiteConfig with
            | None -> Err
            | Some _ -> Created (name=name, acl=acl, tags=tags, website=Some websiteConfig, policy=policy)

      | Created (name, acl, tags, _, policy), PutBucketWebsite, (bucket, _, _, _, _), None, Some websiteConfig ->      
          match putBucketWebsite amazonS3client bucket websiteConfig with
            | None -> Err
            | Some _ -> Created (name=name, acl=acl, tags=tags, website=Some websiteConfig, policy=policy)                        

      | Created (name, acl, tags, website, _), PutBucketPolicy, (bucket, _, _, _, Some policy), _, _ ->      
          match putBucketPolicy amazonS3client bucket policy with
            | None -> Err
            | Some _ -> Created (name=name, acl=acl, tags=tags, website=website, policy=Some policy)
      
      // from created to created   - Delete                    

      | Created (name, acl, _, website, policy), DeleteBucketTagging, (bucket, _, _, Some tags,_), _, _ ->      
          match putBucketTagging amazonS3client bucket tags with
            | None -> Err
            | Some _ -> Created (name=name, acl=acl, tags=Some tags, website=website, policy=policy)

      | Created (name, acl, _, website, policy), DeleteBucketPolicy, (bucket, _, _, Some tags,_), _, _ ->      
          match putBucketTagging amazonS3client bucket tags with
            | None -> Err
            | Some _ -> Created (name=name, acl=acl, tags=Some tags, website=website, policy=policy)

      | Created (name, acl, _, website, policy), DeleteBucketWebsite, (bucket, _, _, Some tags,_), _, _ ->      
          match putBucketTagging amazonS3client bucket tags with
            | None -> Err
            | Some _ -> Created (name=name, acl=acl, tags=Some tags, website=website, policy=policy)

      // from created to nonexistent

      | Created (_), DeleteBucket, (bucket, _, _, _,_), _, _ ->      
          match deleteBucket amazonS3client bucket with
            | None -> Err
            | Some _ -> NonExistent

      | _, _, (_), _, _ ->
        Console.Error.WriteLine("Not implemented State / Event combination")
        Err

  let checkAllowedRegion region allowedAwsRegions =
    match region, allowedAwsRegions with
      | AllowedS3Region region  -> 
          Some region
      | _ ->
        Console.Error.WriteLine("Unsupported region: {0}", region);
        Environment.Exit 1
        None

  let createWebsiteBucketConfig 
    (bucketName: string) (region: string) (stage: string) 
      (websiteDocuments: WebsiteDocuments) (tags: list<string * string>) (policy) =
    
    let config = jsonConfig stage

    let awsRegion = checkAllowedRegion region config.AllowedAwsRegions
   
    let config = { 
      Bucket = bucketName; 
      Acl = PublicReadAcl; 
      Region = awsRegion.Value;
      Website = websiteDocuments;
      Tags = (convertToS3Tags tags) ;
      Policy = policy;
    }
    // return
    Website(config = config)


  let createRedirectBucketConfig 
    (bucketName:string) (region:string) (stage:string)
      (redirect:RedirectOnly) (tags:list<string * string>) (policy)  =
    
    let config = jsonConfig stage

    let awsRegion = checkAllowedRegion region config.AllowedAwsRegions
   
    let config : RedirectOnlyConfig  = { 
      Bucket = bucketName; 
      Acl = PublicReadAcl; 
      Region = awsRegion.Value;
      Website = redirect;
      Tags = (convertToS3Tags tags) ;
      Policy = policy;
    }
    // return
    Redirect(config = config)

  let lofasz (state:State) (desiredState:State) =
    let rec lofasz2 state desiredState acc =
      match state, desiredState with
        | NonExistent, Created (name, acl, None, None, None)  -> PutBucket :: acc
        | NonExistent, Created (name, acl, Some v, _, _)      -> PutBucketTagging :: acc
        | NonExistent, Created (name, acl, _, Some v, _)      -> PutBucketWebsite :: acc
        | NonExistent, Created (name, acl, _, _, Some v)      -> PutBucketPolicy :: acc
        | _,_ -> acc
    lofasz2 state desiredState []

  let createS3Bucket (amazonS3client:AmazonS3Client) (s3BucketConfig:S3BucketConfig) =
    try
      let basicConfig = 
        match s3BucketConfig with 
          | Private config  ->  (config.Bucket, aclToS3CannedAcl(config.Acl), config.Region, config.Tags, config.Policy)
          | Website config  ->  (config.Bucket, aclToS3CannedAcl(config.Acl), config.Region, config.Tags, config.Policy)
          | Redirect config ->  (config.Bucket, aclToS3CannedAcl(config.Acl), config.Region, config.Tags, config.Policy)

      let websiteConfig = 
        match s3BucketConfig with 
          | Website config  ->
              let webSiteConfiguration = WebsiteConfiguration()
              webSiteConfiguration.ErrorDocument        <- config.Website.ErrorDocument
              webSiteConfiguration.IndexDocumentSuffix  <- config.Website.IndexDocument
              Some webSiteConfiguration
          | _               
              ->  None

      let redirectConfig =
        match s3BucketConfig with
          | Redirect config ->  
              let webSiteConfiguration  = WebsiteConfiguration()     
              let routingRuleRedirect   = RoutingRuleRedirect()
              routingRuleRedirect.HostName                <- config.Website.RedirectTo
              webSiteConfiguration.RedirectAllRequestsTo  <- routingRuleRedirect
              Some webSiteConfiguration
          | _               
              ->  None

      
      


      let currentState = transition amazonS3client basicConfig websiteConfig redirectConfig Initial GetState

      let miez = 
        match currentState with
          | NonExistent -> 
            match transition amazonS3client basicConfig websiteConfig redirectConfig Initial PutBucket with
              | Err -> Err
              | Created (name, acl, None, None, None) -> transition amazonS3client basicConfig websiteConfig redirectConfig Initial PutBucket
          | _ -> Err

        // match s3BucketConfig with
        //   | Website config -> (config.Bucket, aclToS3CannedAcl(config.Acl), config.Region, config.Tags, config.Policy)

        // match currentState with
        //   | NonExistent -> 
        //       transition amazonS3client s3BucketConfig Initial PutBucket
      Err              
    with ex ->
      Err











































    // try      
    //   let putBucketResult = putBucket amazonS3client config.Bucket config.Region config.Acl

    //   let webSiteConfiguration = WebsiteConfiguration()
    //   webSiteConfiguration.ErrorDocument        <- config.Website.ErrorDocument
    //   webSiteConfiguration.IndexDocumentSuffix  <- config.Website.IndexDocument

    //   Ok ()
    // with ex ->
    //   Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  // let createS3BucketRedirect (amazonS3client:AmazonS3Client) (config:RedirectOnlyConfig) =
  //   try
  //     // let putBucketResult = putBucket amazonS3client config.Bucket config.Region config.Acl
  //     // let webSiteConfiguration  = WebsiteConfiguration()     
  //     // let routingRuleRedirect   = RoutingRuleRedirect()
  //     // routingRuleRedirect.HostName                <- config.Website.RedirectTo
  //     // webSiteConfiguration.RedirectAllRequestsTo  <- routingRuleRedirect  
  //     NonExistent
  //   with ex ->
  //     // Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
  //     Err

  // let createS3BucketPrivate (amazonS3client:AmazonS3Client) (config:PrivateBucketConfig) =
  //   try
      
  //     let putBucketResult = putBucket amazonS3client config.Bucket config.Region config.Acl
  //     NonExistent
  //   with ex ->
  //     // Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
  //     Err


      




  // let createRedirectOnlyBucketConfig (bucketName) (region) (websiteToRedirectTo) (tags):RedirectOnlyConfig  =
  //   { 
  //     Bucket = bucketName; 
  //     Acl = PublicReadAcl; 
  //     Region = region;
  //     Website = websiteToRedirectTo;
  //     Tags = tags ;
  //   }

  // let createRedirectOnlyBucket (config:RedirectOnlyConfig) : AwsS3Bucket =
  //   Redirect(config = config)

  // // Website


  


  // // AWS CODE PATH

  // // Creating a bucket

  // let putBucket (amazonS3client:AmazonS3Client) bucket (region:S3Region) acl =
  //   try
  //     let putBucketRequest = 
  //       PutBucketRequest(
  //         BucketName = bucket, 
  //         BucketRegion = region,
  //         CannedACL = aclToS3CannedAcl(acl)
  //       )
  //     let task = amazonS3client.PutBucketAsync(putBucketRequest)
  //     task.Wait()
  //     if task.IsCompletedSuccessfully then
  //       Ok task.Result
  //     else
  //       Error (String.Format("Could not create bucket: {0}", bucket))
  //   with ex ->
  //     Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  // let putBucketTagging (amazonS3client:AmazonS3Client) (bucket:string) (tags:List<Tag>) =
  //   try
  //     let putBucketTaggingRequest = 
  //       PutBucketTaggingRequest(
  //         BucketName = bucket, 
  //         TagSet =  tags
  //       )
  //     let task = amazonS3client.PutBucketTaggingAsync(putBucketTaggingRequest)
  //     task.Wait()   
  //     if task.IsCompletedSuccessfully then
  //       Ok task.Result
  //     else
  //       Error "Could not add tags to bucket"
  //   with ex ->
  //     Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  // let putBucketWebsite (amazonS3client:AmazonS3Client) (bucket:string) (webSiteConfiguration:WebsiteConfiguration) =
  //   try
  //     let putBucketWebsiteRequest = 
  //       PutBucketWebsiteRequest(
  //         BucketName = bucket,
  //         WebsiteConfiguration = webSiteConfiguration
  //       )
  //     let task = amazonS3client.PutBucketWebsiteAsync(putBucketWebsiteRequest)
  //     task.Wait()
  //     if task.IsCompletedSuccessfully then
  //       Ok task.Result
  //     else
  //       Error "Could not add website configuration to bucket"
  //   with ex ->
  //     Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))



  // // TODO Move this code to StateMachine
  // // that would simplify show plan and create too

  // let createAwsS3BucketPrivate (amazonS3client:AmazonS3Client) (region:S3Region) (bucket:string) (acl) (tags:Tags) =
  
  //   let putBucketResult = putBucket amazonS3client bucket region acl

  //   let putBucketTaggingResult =
  //     match putBucketResult, tags with
  //     | Ok _, Some tags -> putBucketTagging amazonS3client bucket tags
  //     | Ok _, None      -> Error "No tags were supplied"
  //     | Error _, _    -> Error "Bucket creation error"
    
  //   match putBucketResult, putBucketTaggingResult with
  //     | Ok pbr, Ok pbt  -> 
  //         Console.WriteLine(
  //           "Bucket has been successfully created: put bucket request id: {0} put tagging request id {1}", 
  //           pbr.ResponseMetadata.RequestId, 
  //           pbt.ResponseMetadata.RequestId)
  //     | Ok pbr, Error pbt -> 
  //         Console.WriteLine(
  //           "Bucket has been successfully created: put bucket request id: {0} put tagging could NOT be completed {1}", 
  //           pbr.ResponseMetadata.RequestId, 
  //           pbt)
  //     | Error pbr, _  -> 
  //         Console.WriteLine(
  //           "Bucket could NOT be created: {0}", 
  //           pbr)


  