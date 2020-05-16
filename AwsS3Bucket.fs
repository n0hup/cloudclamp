namespace CloudClamp
open Amazon.S3.Model

module AwsS3Bucket =

  open Amazon.S3

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
    | EUW1     -> "eu-west-1"

  type PrivateAcl = 
    PrivateAcl

  type PublicReadAcl = 
    PublicReadAcl

  let aclToS3CannedAcl (acl:obj) =
    match acl with
    | :? PrivateAcl     -> S3CannedACL.Private
    | :? PublicReadAcl  -> S3CannedACL.PublicRead
    | _                 -> S3CannedACL.NoACL

  type Tag = string * string

  type Tags = Option<List<Tag>>

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

  let createWebsiteBucketConfig (bucketName) (region) (websiteDocuments:WebsiteDocuments) (tags):WebsiteConfig  =
    { 
      Bucket = bucketName; 
      Acl = PublicReadAcl; 
      Region = region;
      Website = websiteDocuments;
      Tags = tags ;
    }
  
  let createWebsiteBucket(config:WebsiteConfig):AwsS3Bucket =
    Website(config = config)

  // Creating the bucket

  let createAwsS3Bucket (amazonS3client:AmazonS3Client) (bucket) (region) (acl) =
    
    let putBucketRequest = 
      PutBucketRequest(
        BucketName = bucket, 
        BucketRegion = allowedAwsRegionsToS3region(region),
        CannedACL = aclToS3CannedAcl(acl)
      )
    
    let task = amazonS3client.PutBucketAsync(putBucketRequest)
    task.Wait()
    if task.IsCompletedSuccessfully then
      Some task.Result
    else
      None

  let createS3bucket (amazonS3client:AmazonS3Client) (bucket:AwsS3Bucket) =
    match bucket with 
      | Website config -> createAwsS3Bucket amazonS3client config.Bucket config.Region config.Acl
      | Redirect config -> createAwsS3Bucket amazonS3client config.Bucket config.Region config.Acl
      | Private config -> createAwsS3Bucket amazonS3client config.Bucket config.Region config.Acl

