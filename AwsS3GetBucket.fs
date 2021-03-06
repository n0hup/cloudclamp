namespace CloudClamp

// external
open Amazon.S3
open Amazon.S3.Model
open System
open System.Net

// internal
open HttpStatus

module AwsS3GetBucket =

  // #############################  GET BUCKET ##################################

  let getBucketPolicy (amazonS3client:AmazonS3Client) (bucket:string) =
    try
      let getBucketPolicyRequest = 
        GetBucketPolicyRequest(
          BucketName = bucket
        )
      let task = amazonS3client.GetBucketPolicyAsync(getBucketPolicyRequest)
      task.Wait()
      if task.IsCompletedSuccessfully && isHttpSuccess task.Result.HttpStatusCode then
        Some task.Result.Policy      
      else
        None 
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
      None

  let getBucketWebsite (amazonS3client:AmazonS3Client) (bucket:string) =
    try
      let getBucketWebsiteRequest = 
        GetBucketWebsiteRequest(
          BucketName = bucket
        )
      let task = amazonS3client.GetBucketWebsiteAsync(getBucketWebsiteRequest)
      task.Wait()
      if task.IsCompletedSuccessfully && isHttpSuccess task.Result.HttpStatusCode then
        Some task.Result.WebsiteConfiguration      
      else
        None
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
      None

  let getBucketTagging (amazonS3client:AmazonS3Client) (bucket:string) =
    try
      let getBucketTaggingRequest = 
        GetBucketTaggingRequest(
          BucketName = bucket
        )
      let task = amazonS3client.GetBucketTaggingAsync(getBucketTaggingRequest)
      task.Wait()
      if task.IsCompletedSuccessfully && isHttpSuccess task.Result.HttpStatusCode then
        Some task.Result.TagSet
      else
        None
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
      None
  
  let getBucketAcl (amazonS3client:AmazonS3Client) (bucket:string) =
    try
      let getBucketAclRequest = 
        GetACLRequest(
          BucketName = bucket
        )
      let task = amazonS3client.GetACLAsync(getBucketAclRequest)
      task.Wait()
      if task.IsCompletedSuccessfully && isHttpSuccess task.Result.HttpStatusCode then
        Some task.Result.AccessControlList
      else
        None    
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
      None