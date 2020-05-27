namespace CloudClamp

// external
open Amazon.S3
open Amazon.S3.Model
open System
open System.Collections.Generic

// internal
open AwsS3Acl
open System.Net

module AwsS3PutBucket =

  // #############################  PUT BUCKET ##################################

  let putBucket (amazonS3client:AmazonS3Client) bucket (region:S3Region) acl =
    try
      let putBucketRequest = 
        PutBucketRequest(
          BucketName = bucket, 
          BucketRegion = region,
          CannedACL = acl
        )
      let task = amazonS3client.PutBucketAsync(putBucketRequest)
      task.Wait()
      if task.IsCompletedSuccessfully && task.Result.HttpStatusCode = HttpStatusCode.OK then
        Some task.Result
      else
        Console.Error.WriteLine("Could not put bucket: {0}", bucket)
        None
    with ex ->
      Console.Error.WriteLine(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
      None

  let putBucketTagging (amazonS3client:AmazonS3Client) (bucket:string) (tags:List<Tag>) =
    try
      let putBucketTaggingRequest = 
        PutBucketTaggingRequest(
          BucketName = bucket, 
          TagSet =  tags
        )
      let task = amazonS3client.PutBucketTaggingAsync(putBucketTaggingRequest)
      task.Wait()   
      if task.IsCompletedSuccessfully && task.Result.HttpStatusCode = HttpStatusCode.OK then
        Some task.Result
      else
        Console.Error.WriteLine("Could not put bucket tagging: {0} tagging: {1}", bucket, tags)
        None
    with ex ->
      Console.Error.WriteLine(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
      None

  let putBucketWebsite (amazonS3client:AmazonS3Client) (bucket:string) (webSiteConfiguration:WebsiteConfiguration) =
    try
      let putBucketWebsiteRequest = 
        PutBucketWebsiteRequest(
          BucketName = bucket,
          WebsiteConfiguration = webSiteConfiguration
        )
      let task = amazonS3client.PutBucketWebsiteAsync(putBucketWebsiteRequest)
      if task.IsCompletedSuccessfully && task.Result.HttpStatusCode = HttpStatusCode.OK then
        Some task.Result
      else
        Console.Error.WriteLine("Could not put bucket website bucket: {0} website: {1}", bucket, webSiteConfiguration)
        None
    with ex ->
      Console.Error.WriteLine(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
      None

  let putBucketPolicy (amazonS3client:AmazonS3Client) (bucket:string) (policy:string) =
    try
      let putBucketPolicyRequest = 
        PutBucketPolicyRequest(
          BucketName = bucket,
          Policy = policy
        )
      let task = amazonS3client.PutBucketPolicyAsync(putBucketPolicyRequest)
      task.Wait()
      if task.IsCompletedSuccessfully && task.Result.HttpStatusCode = HttpStatusCode.OK then
        Some task.Result
      else
        Console.Error.WriteLine("Could not put bucket policy bucket: {0} policy: {1}", bucket, policy)
        None
    with ex ->
      Console.Error.WriteLine(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
      None