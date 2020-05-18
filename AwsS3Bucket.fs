namespace CloudClamp


module AwsS3Bucket =

  open Amazon.S3
  open System
  open Amazon.S3.Model
  open System.Collections.Generic

  // We only allow these
  
  type AllowedAwsRegions = 
   | EUC1
   | EUW1

  let allowedAwsRegionsToS3region (region) =
    match region with
    | EUW1 -> S3Region.EUW1
    | EUC1 -> S3Region.EUC1

  let awsRegionToString (region:AllowedAwsRegions):string =
    match region with
    | EUC1  -> "eu-central-1"
    | EUW1  -> "eu-west-1"

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
      tba.Key <- fst tag
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
    Region  : AllowedAwsRegions;
    Website : RedirectOnly;
    Tags    : Tags ;
  }

  type WebsiteConfig = {
    Bucket  : string;
    Acl     : PublicReadAcl;
    Region  : AllowedAwsRegions;
    Website : WebsiteDocuments;
    Tags    : Tags ;
  }

  type PrivateBucketConfig = {
    Bucket  : string;
    Acl     : PrivateAcl;
    Region  : AllowedAwsRegions;
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

  let createWebsiteBucketConfig (bucketName:string) (region:AllowedAwsRegions) (websiteDocuments:WebsiteDocuments) (tags:Tags) : WebsiteConfig  =
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

  let inline putBucket (amazonS3client:AmazonS3Client) bucket region acl =
    try
      let putBucketRequest = 
        PutBucketRequest(
          BucketName = bucket, 
          BucketRegion = allowedAwsRegionsToS3region(region),
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

  let inline createAwsS3Bucket (amazonS3client:AmazonS3Client) (bucket:string) (region:AllowedAwsRegions) (acl) (tags:Tags) =
  
    let putBucketResponseResult = putBucket amazonS3client bucket region acl

    let putBucketTaggingResult =
      match putBucketResponseResult, tags with
      | _, _ -> 1
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

