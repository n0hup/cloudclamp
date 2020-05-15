namespace CloudClamp

module AwsS3Bucket =

  open Amazon.S3

  // We only allow these
  
  type AwsRegion = 
   | EuCentral1
   | EuWest1

  let awsRegionToString (region:AwsRegion):string =
    match region with
    | EuCentral1  -> "eu-central-1"
    | EuWest1     -> "eu-west-1"

  type PrivateAcl = 
    PrivateAcl

  type PublicReadAcl = 
    PublicReadAcl

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
    Region  : AwsRegion;
    Website : RedirectOnly;
    Tags    : Tags ;
  }

  type WebsiteConfig = {
    Bucket  : string;
    Acl     : PublicReadAcl;
    Region  : AwsRegion;
    Website : WebsiteDocuments;
    Tags    : Tags ;
  }

  type PrivateBucketConfig = {
    Bucket  : string;
    Acl     : PrivateAcl;
    Region  : AwsRegion;
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

  let createAwsS3Bucket (amazonS3client:AmazonS3Client) (bucket) (region) =
    ()

  let createS3bucket (amazonS3client:AmazonS3Client) (bucket:AwsS3Bucket) =
    match bucket with 
      | Website config -> createAwsS3Bucket amazonS3client config.Bucket config.Region
      | Redirect config -> createAwsS3Bucket amazonS3client config.Bucket config.Region
      | Private config -> createAwsS3Bucket amazonS3client config.Bucket config.Region

