namespace CloudClamp

// external
open Amazon.S3

module AwsS3Acl =

  type PrivateAcl = 
    PrivateAcl

  type PublicReadAcl = 
    PublicReadAcl

  let aclToS3CannedAcl (acl:obj) =
    match acl with
    | :? PrivateAcl     -> S3CannedACL.Private
    | :? PublicReadAcl  -> S3CannedACL.PublicRead
    | _                 -> S3CannedACL.NoACL
