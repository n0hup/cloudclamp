namespace CloudClamp

open Amazon
open Amazon.S3
open Amazon.S3.Model
open System
open System.Collections.Generic

module AwsS3 =

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


  let (|AllowedS3Region|_|) (region:string, allowedAwsRegions) =
    match region, Array.contains region allowedAwsRegions with
      | "eu-central-1", true  -> Some S3Region.EUC1
      // Europe (Ireland) eu-west-1
      | "eu-west-1", true     -> Some S3Region.EUW1
      // Europe (London) eu-west-2
      | "eu-west-2", true     -> Some S3Region.EUW2
      // Europe (Milan) eu-south-1
      // Europe (Paris) eu-west-3
      | "eu-west-3", true     -> Some S3Region.EUW3
      // Europe (Stockholm) eu-north-1
      | "eu-north-1", true    -> Some S3Region.EUN1
      | _, _ -> None

  // let allowedRegion (region:string) (allowedRegions) : AllowedS3Region =
  //   match region, (Array.contains region allowedRegions) with
  //     // US
  //     // US East (N. Virginia) us-east-1
  //     | "us-east-1", true  -> Allowed S3Region.US // Not sure about this
  //     // US East (Ohio) us-east-2
  //     | "us-east-2", true  -> Allowed S3Region.USE2
  //     // US West (N. California) us-west-1
  //     | "us-west-1", true  -> Allowed S3Region.USW1
  //     // US West (Oregon) us-west-2
  //     | "us-west-2", true  -> Allowed S3Region.USW2
  //     // Africa
  //     // Africa (Cape Town) af-south-1
  //     // Asia
  //     // Asia Pacific (Hong Kong) ap-east-1
  //     // Asia Pacific (Mumbai) ap-south-1
  //     // Asia Pacific (Osaka-Local) ap-northeast-3
  //     // Asia Pacific (Seoul) ap-northeast-2
  //     // Asia Pacific (Singapore) ap-southeast-1
  //     // Asia Pacific (Sydney) ap-southeast-2
  //     // Asia Pacific (Tokyo) ap-northeast-1
  //     // Canada
  //     // Canada (Central) ca-central-1
  //     // China
  //     // China (Beijing) cn-north-1
  //     // China (Ningxia) cn-northwest-1
  //     // Europe
  //     // Europe (Frankfurt) eu-central-1
  //     | "eu-central-1", true  -> Allowed S3Region.EUC1
  //     // Europe (Ireland) eu-west-1
  //     | "eu-west-1", true     -> Allowed S3Region.EUW1
  //     // Europe (London) eu-west-2
  //     | "eu-west-2", true     -> Allowed S3Region.EUW2
  //     // Europe (Milan) eu-south-1
  //     // Europe (Paris) eu-west-3
  //     | "eu-west-3", true     -> Allowed S3Region.EUW3
  //     // Europe (Stockholm) eu-north-1
  //     | "eu-north-1", true     -> Allowed S3Region.EUN1
  //     // Middle East
  //     // Middle East (Bahrain) me-south-1
  //     // South America
  //     // South America (São Paulo) sa-east-1
  //     | _, _                -> NotAllowed

  type PrivateAcl = 
    PrivateAcl

  type PublicReadAcl = 
    PublicReadAcl

  let aclToS3CannedAcl (acl:obj) =
    match acl with
    | :? PrivateAcl     -> S3CannedACL.Private
    | :? PublicReadAcl  -> S3CannedACL.PublicRead
    | _                 -> S3CannedACL.NoACL

  type Tags = Option<List<Tag>>

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
    Tags    : Tags ;
  }

  type WebsiteConfig = {
    Bucket  : string;
    Acl     : PublicReadAcl;
    Region  : S3Region;
    Website : WebsiteDocuments;
    Tags    : Tags ;
  }

  type PrivateBucketConfig = {
    Bucket  : string;
    Acl     : PrivateAcl;
    Region  : S3Region;
    Tags    : Tags ;
  }

  type AwsS3Bucket =
    | Website of config   : WebsiteConfig
    | Private of config   : PrivateBucketConfig
    | Redirect of config  : RedirectOnlyConfig

  type CreateS3bucketResponse = {
    PutBucketResponseResult : Option<PutBucketResponse>;
    PutBucketTaggingResult : Option<PutBucketTaggingResponse>;
  }

  // RedirectOnly

  let createRedirectOnlyBucketConfig (bucketName) (region) (websiteToRedirectTo) (tags):RedirectOnlyConfig  =
    { 
      Bucket = bucketName; 
      Acl = PublicReadAcl; 
      Region = region;
      Website = websiteToRedirectTo;
      Tags = tags ;
    }

  let createRedirectOnlyBucket(config:RedirectOnlyConfig):AwsS3Bucket =
    Redirect(config = config)

  // Website

  let createWebsiteBucketConfig (bucketName:string) (region:S3Region) (websiteDocuments:WebsiteDocuments) (tags:Tags) : WebsiteConfig  =
    { 
      Bucket = bucketName; 
      Acl = PublicReadAcl; 
      Region = region;
      Website = websiteDocuments;
      Tags = tags ;
    }
  
  let createWebsiteBucket(config:WebsiteConfig):AwsS3Bucket =
    Website(config = config)

  // AWS CODE PATH

  // Creating a bucket

  let putBucket (amazonS3client:AmazonS3Client) bucket (region:S3Region) acl =
    try
      let putBucketRequest = 
        PutBucketRequest(
          BucketName = bucket, 
          BucketRegion = region,
          CannedACL = aclToS3CannedAcl(acl)
        )
      let task = amazonS3client.PutBucketAsync(putBucketRequest)
      task.Wait()
      if task.IsCompletedSuccessfully then
        Ok task.Result
      else
        Error (String.Format("Could not create bucket: {0}", bucket))
    with ex ->
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  let putBucketTagging (amazonS3client:AmazonS3Client) bucket tags =
    try
      let putBucketTaggingRequest = 
        PutBucketTaggingRequest(
          BucketName = bucket, 
          TagSet =  tags
        )
      let task = amazonS3client.PutBucketTaggingAsync(putBucketTaggingRequest)
      task.Wait()   
      if task.IsCompletedSuccessfully then
        Ok task.Result
      else
        Error "Could not add tags to bucket"
    with ex ->
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))


  // TODO ITT

  let putBucketWebsite (amazonS3client:AmazonS3Client) bucket =
    try
      // this is garbage

      let redirectRule = RoutingRuleRedirect()
      redirectRule.HostName <- "HostName"

      let webSiteConfiguration = WebsiteConfiguration()
      webSiteConfiguration.ErrorDocument <- "lofasz"
      webSiteConfiguration.IndexDocumentSuffix <- "bofasz"
      
      let rrrule = RoutingRule()
      rrrule.Redirect <- redirectRule
      let lofasz = List<RoutingRule>()

      lofasz.Add(rrrule)

      webSiteConfiguration.RoutingRules <- lofasz

      webSiteConfiguration.RedirectAllRequestsTo <- redirectRule
      
      // end garbage

      let putBucketWebsiteRequest = 
        PutBucketWebsiteRequest(
          BucketName = bucket,
          WebsiteConfiguration = webSiteConfiguration
        )
      // let lofaz = amazonS3client.Add
      Ok 1
    with ex ->
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  let createAwsS3Bucket (amazonS3client:AmazonS3Client) (bucket:string) (region:S3Region) (acl) (tags:Tags) =
  
    let putBucketResponseResult = putBucket amazonS3client bucket region acl

    let putBucketTaggingResult =
      match putBucketResponseResult, tags with
      | Ok v, Some tags -> 1
      | Ok v, None      -> 1
      | Error err, _    -> 1
  
    1
    //   if putBucketResponseResult.IsSome && tags.IsSome then
    //     putBucketTagging (amazonS3client:AmazonS3Client) bucket tags.Value
    //   else
    //     None
    // // return
    // { PutBucketResponseResult = putBucketResponseResult; 
    //   PutBucketTaggingResult  = putBucketTaggingResult;   }

  let createS3bucket (amazonS3client:AmazonS3Client) (bucket:AwsS3Bucket) (tags) =
    match bucket with 
      | Website config -> createAwsS3Bucket amazonS3client config.Bucket config.Region config.Acl tags
      | Redirect config -> createAwsS3Bucket amazonS3client config.Bucket config.Region config.Acl tags
      | Private config -> createAwsS3Bucket amazonS3client config.Bucket config.Region config.Acl tags

