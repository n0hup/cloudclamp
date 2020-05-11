namespace CloudClamp

// Let the type system do its job

module AwsS3Bucket =

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


