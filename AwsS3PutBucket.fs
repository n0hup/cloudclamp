namespace CloudClamp

// external
open Amazon.S3
open Amazon.S3.Model
open System
open System.Collections.Generic

// internal
open HttpStatus
open AwsS3Printable

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
      if task.IsCompletedSuccessfully && isHttpSuccess task.Result.HttpStatusCode then
        Console.WriteLine("Successfully added bucket: {0}", bucket)
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
      if task.IsCompletedSuccessfully && isHttpSuccess task.Result.HttpStatusCode then
        Console.WriteLine("Successfully added tagging to bucket: {0} tagging: {1}", bucket, (getTagsString (Some tags)))
        Some task.Result
      else
        Console.Error.WriteLine("Could not put bucket tagging: {0} tagging: {1}", bucket, (getTagsString (Some tags)))
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
      task.Wait()
      
      let webSiteConfigurationString = getWebsiteConfigString (Some webSiteConfiguration)
        
      if task.IsCompletedSuccessfully && isHttpSuccess task.Result.HttpStatusCode  then
        Console.WriteLine("Successfully added website to bucket: {0}", webSiteConfigurationString)
        Some task.Result
      else
        Console.Error.WriteLine("Could not put bucket website bucket: {0} website config: {1}", bucket, webSiteConfigurationString)
        None
    with ex ->
      Console.Error.WriteLine("{0}", ex)
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
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
      if task.IsCompletedSuccessfully && isHttpSuccess task.Result.HttpStatusCode then
        Console.WriteLine("Successfully added policy to bucket: {0} policy: {1}", bucket, policy)
        Some task.Result
      else
        Console.Error.WriteLine("Could not put bucket policy bucket: {0} policy: {1}", bucket, policy)
        None
    with ex ->
      Console.Error.WriteLine(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
      None