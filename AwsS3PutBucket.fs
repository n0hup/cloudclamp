namespace CloudClamp

// external
open Amazon.S3
open Amazon.S3.Model
open System
open System.Collections.Generic

// internal
open AwsS3Acl

module AwsS3PutBucket =


  // ############################################################################
  // ########################  LOW LEVEL AWS CALLS ##############################
  // ############################################################################

  // #############################  PUT BUCKET ##################################

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

  let putBucketTagging (amazonS3client:AmazonS3Client) (bucket:string) (tags:List<Tag>) =
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
        Error "Could not add tagging to bucket"
    with ex ->
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  let putBucketWebsite (amazonS3client:AmazonS3Client) (bucket:string) (webSiteConfiguration:WebsiteConfiguration) =
    try
      let putBucketWebsiteRequest = 
        PutBucketWebsiteRequest(
          BucketName = bucket,
          WebsiteConfiguration = webSiteConfiguration
        )
      let task = amazonS3client.PutBucketWebsiteAsync(putBucketWebsiteRequest)
      task.Wait()
      if task.IsCompletedSuccessfully then
        Ok task.Result
      else
        Error "Could not add website configuration to bucket"
    with ex ->
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  let putBucketPolicy (amazonS3client:AmazonS3Client) (bucket:string) (policy:string) =
    try
      let putBucketPolicyRequest = 
        PutBucketPolicyRequest(
          BucketName = bucket,
          Policy = policy
        )
      let task = amazonS3client.PutBucketPolicyAsync(putBucketPolicyRequest)
      task.Wait()
      if task.IsCompletedSuccessfully then
        Ok task.Result
      else
        Error "Could not add policy to bucket"
    with ex ->
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))