namespace CloudClamp

// external

// internal

module AwsS3Resource =

  //
  // BucketAccelerateConfiguration
  //

  type BucketAccelerateConfiguration = {
    Name : string
  }

  //
  // BucketAcl
  //

  type Owner = {
    DisplayName : string
    ID          : string
  }

  type Permission = FullControl | Read

  type GranteeType = CanonicalUser | Group

  type Grantee = {
    DisplayName : string
    ID          : string
    GranteeType : GranteeType
  }

  type Grant = {
    Permission : Permission
    Grantee : Grantee
  }

  type BucketAcl = {
    Owner   : Owner
    Grants  : List<Grant>
  }

  //
  // BucketAnalyticsConfiguration
  //

  type BucketAnalyticsConfiguration = {
    Name : string
  }

  //
  // BucketCors
  //

  type BucketCors = {
    Name : string
  }

  //
  // BucketEncryption
  //

  type BucketEncryption = {
    Name : string
  }

  //
  // BucketInventoryConfiguration
  //

  type BucketInventoryConfiguration = {
    Name : string
  }

  //
  // BucketLifecycleConfiguration
  //

  type BucketLifecycleConfiguration = {
    Name : string
  }

  //
  // BucketLocation
  //

  type BucketLocation = {
    LocationConstraint : string
  }

  //
  // BucketLogging
  //

  type BucketLogging = {
    TargetBucket : string
    TargetPrefix : string
  }

  //
  // BucketMetricsConfiguration
  //

  type BucketMetricsConfiguration = {
    Name : string
  }

  //
  // BucketNotificationConfiguration
  //

  type BucketNotificationConfiguration = {
    Name : string
  }

  //
  // BucketPolicy
  //

  type BucketPolicy = {
    Name : string
  }

  //
  // BucketPolicyStatus
  //

  type BucketPolicyStatus = {
    Name : string
  }

  //
  // BucketReplication
  //

  type BucketReplication = {
    Name : string
  }

  //
  // BucketRequestPayment
  //

  type BucketRequestPayment = {
    Name : string
  }

  //
  // BucketTagging
  //

  type Tag = {
    Key   : string
    Value : string
  }

  type BucketTagging = {
    TagSet : List<Tag>
  }

  //
  // BucketVersioning
  //

  type BucketVersioning = {
    Name : string
  }

  //
  // BucketWebsite
  //

  type Website =
    | Documents of IndexDocument : string * ErrorDocument : string
    | Redirect of RedirectAllRequestsTo : string

  type BucketWebsite = {
    Website : Website
  }

  //
  // BucketPublicAccessBlock
  //

  type BucketPublicAccessBlock = {
    Name : string
  }


  type BucketResource = {
    Name      : string
    Location  : BucketLocation
    Acl       : BucketAcl
    Tagging   : BucketTagging
    Policy    : Option<BucketPolicy>
    Logging   : Option<BucketLogging>
    Website   : Option<BucketWebsite>
  }