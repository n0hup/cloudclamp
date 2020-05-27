namespace CloudClamp

// external
open Amazon.S3
open Amazon.S3.Model
open System
open System.Net

module AwsS3DeleteBucket =

  // #############################  DELETE BUCKET ##################################

  let deleteBucket (amazonS3client:AmazonS3Client) (bucket:string) =
    try
      let deleteBucketRequest = 
        DeleteBucketRequest(
          BucketName = bucket
        )
      let task = amazonS3client.DeleteBucketAsync(deleteBucketRequest)
      task.Wait()
      if task.IsCompletedSuccessfully && task.Result.HttpStatusCode = HttpStatusCode.OK then
        Some task.Result
      else
        Console.Error.WriteLine(String.Format("Could not delete bucket: {0}", bucket))
        None 
    with ex ->
      Console.Error.WriteLine(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
      None

  