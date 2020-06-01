namespace CloudClamp

// external
open Amazon
open Amazon.S3
open Amazon.S3.Model
open Amazon.S3.Util
open System
open System.Collections.Generic

// internal
open Config
open AwsS3Acl
open AwsS3GetBucket
open AwsS3PutBucket
open AwsS3DeleteBucket
open AwsS3Printable
open Logging

module AwsS3 =

  let loggerAwsS3 = Logger.CreateLogger("AwsS3")

  let createAwsS3Config awsRegionName =
    try
      let region = RegionEndpoint.GetBySystemName(awsRegionName)
      let config = AmazonS3Config()
      config.MaxConnectionsPerServer <- new Nullable<int>(64)
      config.RegionEndpoint <- region
      Ok config
    with ex ->
      loggerAwsS3.LogError(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
      Environment.Exit(1);
      Error "never reached"

  let getAwsS3Client awsCredentials (awsS3Config:AmazonS3Config) =
    try
      loggerAwsS3.LogInfo("Connecting to S3...")
      loggerAwsS3.LogInfo(
        String.Format(
          "AWS S3 Config: RegionEndpoint :: {0} MaxConnectionsPerServer :: {1} BufferSize :: {2} ServiceVersion :: {3}",
          awsS3Config.RegionEndpoint,
          awsS3Config.MaxConnectionsPerServer,
          awsS3Config.BufferSize,
          awsS3Config.ServiceVersion))
      Ok(new AmazonS3Client(awsCredentials, awsS3Config))
    with ex ->
      loggerAwsS3.LogError("Connecting to S3 has failed")
      let err = String.Format("{0} : {1}", ex.Message, ex.InnerException.Message)
      loggerAwsS3.LogError(err)
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

  type BucketLoggingConfig = {
    TargetBucketName : string;
    TargetPrefix : string;
  }

  let getS3BucketLoggingConfig (bucketLoggingConfig:BucketLoggingConfig) =
    let s3BucketLoggingConfig = S3BucketLoggingConfig()
    s3BucketLoggingConfig.TargetBucketName <- bucketLoggingConfig.TargetBucketName
    s3BucketLoggingConfig.TargetPrefix <- bucketLoggingConfig.TargetPrefix
    s3BucketLoggingConfig

  type WebsiteDocuments = {
    IndexDocument : string ;
    ErrorDocument : string ;
  }

  type RedirectOnly = {
    RedirectTo  : string;
    Protocol    : string;
  }

  type RedirectOnlyConfig = {
    Bucket      : string;
    Acl         : PublicReadAcl;
    Region      : S3Region;
    Website     : RedirectOnly;
    Tags        : Option<List<Tag>> ;
    Policy      : Option<string>;
    Logging     : Option<BucketLoggingConfig>;
  }

  type WebsiteConfig = {
    Bucket  : string;
    Acl     : PublicReadAcl;
    Region  : S3Region;
    Website : WebsiteDocuments;
    Tags    : Option<List<Tag>> ;
    Policy  : Option<string>;
    Logging : Option<BucketLoggingConfig>;
  }

  type PrivateBucketConfig = {
    Bucket  : string;
    Acl     : PrivateAcl;
    Region  : S3Region;
    Tags    : Option<List<Tag>> ;
    Policy  : Option<string>;
    Logging : Option<BucketLoggingConfig>;
  }

  type S3BucketConfig =
    | Website of config   : WebsiteConfig
    | Private of config   : PrivateBucketConfig
    | Redirect of config  : RedirectOnlyConfig

  let getS3BucketConfigString config =
    match config with
      | Website config -> 
          String.Format("Bucket: {0} Region: {1} Tags: {2} Website: {3} Policy: {4}", config.Bucket, config.Region, 
            (getTagsString config.Tags), (config.Website.IndexDocument + ":" + config.Website.ErrorDocument), 
            (getPolicyString config.Policy))
      | Private config ->
          String.Format("Bucket: {0} Region: {1} Tags: {2} Policy: {3}", config.Bucket, config.Region, 
            (getTagsString config.Tags), (getPolicyString config.Policy))
      | Redirect config ->
          String.Format("Bucket: {0} Region: {1} Tags: {2} Website: {3} Policy: {4}", config.Bucket, config.Region, 
            (getTagsString config.Tags), (config.Website.RedirectTo), 
            (getPolicyString config.Policy))
 

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

  let stateToString (s:State) =
    match s with
      | Initial     -> "Initial"
      | Err         -> "Err"
      | NonExistent -> "NonExistent"
      | Created (name, acl, tags, website, policy) -> 
          String.Format("Created name: {0} - Acl: {1} - tags: {2} - website: {3} - policy: {4}", 
            name, 
            ("ACL :: Owner:" + acl.Owner.DisplayName + " Grants: " + acl.Grants.Capacity.ToString()),
            (getTagsString tags), 
            (getWebsiteConfigString website), 
            (getPolicyString policy))

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
          // TODO null pointer!!! None.Value
          Created(name=bucket, acl=bucketAcl.Value, tags=bucketTags, website=bucketWesite, policy=bucketPolicy)
        else
          NonExistent
      else
        loggerAwsS3.LogError(String.Format("Could not get bucket state: {0}", bucket))
        Err
    with ex ->
      loggerAwsS3.LogError(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
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
        loggerAwsS3.LogError("Not implemented State / Event combination")
        Err

  let checkAllowedRegion region allowedAwsRegions =
    match region, allowedAwsRegions with
      | AllowedS3Region region  -> 
          Some region
      | _ ->
        loggerAwsS3.LogError(String.Format("Unsupported region: {0}", region))
        Environment.Exit 1
        None

  let createPrivateBucketConfig 
    (bucketName: string) (region: string) (stage: string) 
      (tags: list<string * string>) (policy) (logging) =
    
    let config = jsonConfig stage

    let awsRegion = checkAllowedRegion region config.AllowedAwsRegions
   
    let config = { 
      Bucket = bucketName; 
      Acl = PrivateAcl; 
      Region = awsRegion.Value;
      Tags = (convertToS3Tags tags) ;
      Policy = policy;
      Logging = logging;
    }
    // return
    Private(config = config)

  let createWebsiteBucketConfig 
    (bucketName: string) (region: string) (stage: string) 
      (websiteDocuments: WebsiteDocuments) (tags: list<string * string>) (policy) (logging) =
    
    let config = jsonConfig stage

    let awsRegion = checkAllowedRegion region config.AllowedAwsRegions
   
    let config = { 
      Bucket = bucketName; 
      Acl = PublicReadAcl; 
      Region = awsRegion.Value;
      Website = websiteDocuments;
      Tags = (convertToS3Tags tags) ;
      Policy = policy;
      Logging = logging;
    }
    // return
    Website(config = config)


  let createRedirectBucketConfig 
    (bucketName:string) (region:string) (stage:string)
      (redirect:RedirectOnly) (tags:list<string * string>) (policy) (logging)  =
    
    let config = jsonConfig stage

    let awsRegion = checkAllowedRegion region config.AllowedAwsRegions
   
    let config : RedirectOnlyConfig  = { 
      Bucket = bucketName; 
      Acl = PublicReadAcl; 
      Region = awsRegion.Value;
      Website = redirect;
      Tags = (convertToS3Tags tags) ;
      Policy = policy;
      Logging = logging;
    }
    // return
    Redirect(config = config)

  let getEvents (currentState) (additionalProperties) =
    match currentState, additionalProperties with

      | NonExistent, (None, None, None)   -> [ PutBucket ]

      | NonExistent, (Some _, None, None) -> [ PutBucket; PutBucketTagging ]
      | NonExistent, (None, Some _, None) -> [ PutBucket; PutBucketWebsite ]
      | NonExistent, (None, None, Some _) -> [ PutBucket; PutBucketPolicy  ]
 
      | NonExistent, (Some _, Some _, None) -> [ PutBucket; PutBucketTagging; PutBucketWebsite; ]
      | NonExistent, (Some _, None, Some _) -> [ PutBucket; PutBucketTagging; PutBucketPolicy;  ]
      | NonExistent, (None, Some _, Some _) -> [ PutBucket; PutBucketWebsite; PutBucketPolicy;  ]
 
      | NonExistent, (Some _, Some _, Some _) -> [ PutBucket; PutBucketTagging; PutBucketWebsite; PutBucketPolicy; ]

      | Created (_), (Some _, None, None) -> [ PutBucketTagging ]
      | Created (_), (None, Some _, None) -> [ PutBucketWebsite ]
      | Created (_), (None, None, Some _) -> [ PutBucketPolicy  ]
 
      | Created (_), (Some _, Some _, None) -> [ PutBucketTagging; PutBucketWebsite; ]
      | Created (_), (Some _, None, Some _) -> [ PutBucketTagging; PutBucketPolicy;  ]
      | Created (_), (None, Some _, Some _) -> [ PutBucketWebsite; PutBucketPolicy;  ]
 
      | Created (_), (Some _, Some _, Some _) -> [ PutBucketTagging; PutBucketWebsite; PutBucketPolicy; ]

      | _, _ -> []

  let rec walkEvents amazonS3client basicConfig websiteConfig redirectConfig currentState events =
    match currentState, events with
      | currentState, []      -> currentState
      | currentState, hd::tl  -> 
          walkEvents amazonS3client basicConfig websiteConfig redirectConfig (transition amazonS3client basicConfig websiteConfig redirectConfig currentState hd) tl
  
  let createS3Bucket (amazonS3client:AmazonS3Client) (s3BucketConfig:S3BucketConfig) =
    try
      loggerAwsS3.LogInfo(String.Format("s3BucketConfig: {0}", getS3BucketConfigString s3BucketConfig))
      let basicConfig = 
        match s3BucketConfig with 
          | Private config  ->
            (config.Bucket, aclToS3CannedAcl(config.Acl), config.Region, config.Tags, config.Policy)
          | Website config  ->
            (config.Bucket, aclToS3CannedAcl(config.Acl), config.Region, config.Tags, config.Policy)
          | Redirect config ->
            (config.Bucket, aclToS3CannedAcl(config.Acl), config.Region, config.Tags, config.Policy)

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
              routingRuleRedirect.Protocol                <- config.Website.Protocol
              webSiteConfiguration.RedirectAllRequestsTo  <- routingRuleRedirect
              Some webSiteConfiguration
          | _               
              ->  None
              
      let currentState = transition amazonS3client basicConfig websiteConfig redirectConfig Initial GetState
      loggerAwsS3.LogError(String.Format("Current state: {0}", (stateToString currentState)))

      let (_,_,_, tags, policy) = basicConfig

      let website =
        if websiteConfig.IsSome || redirectConfig.IsSome then
          Some ""
        else
          None
      
      let events = getEvents currentState (tags, website, policy)
      
      loggerAwsS3.LogInfo(String.Format("The following chain of events will be processed: {0}", events))

      let finalState = walkEvents amazonS3client basicConfig websiteConfig redirectConfig currentState events

      loggerAwsS3.LogInfo(String.Format("Final state: {0}", (stateToString finalState)))

      finalState     
    with ex ->
      loggerAwsS3.LogError(String.Format("{0}", ex))
      Err

  let getS3Bucket (amazonS3client:AmazonS3Client) (bucket:string) =
    let currentState = getState amazonS3client bucket
    loggerAwsS3.LogInfo(String.Format("Current state: {0}", (stateToString currentState)))
    currentState