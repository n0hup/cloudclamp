namespace CloudClamp

// external

// internal


module BlogStack =
  
  type DnsResource = AwsRoute53Resource 

  type CertificateResource = AwsAcmResource 

  type BucketResource = {
    //    mandatory
    Name      : string
    Location  : AwsS3Resource.BucketLocation
    Acl       : AwsS3Resource.BucketAcl
    Tagging   : AwsS3Resource.BucketTagging
    //    optional
    Policy    : Option<AwsS3Resource.BucketPolicy>
    Logging   : Option<AwsS3Resource.BucketLogging>
    Website   : Option<AwsS3Resource.BucketWebsite>

  } 

  type BlogStack = {
    Dns         : List<DnsResource>
    Certificate : List<CertificateResource>
    Bucket      : List<BucketResource>
  }