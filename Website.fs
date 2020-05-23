namespace CloudClamp

// external
open Amazon.S3
open System

// internal
open AwsS3Bucket
open Config

module Website =

  // Common parameters

  // Stage Specific Parameters

  let prod x =
    ()

  let dev x =
    ()

  let qa x =
    ()

  let createS3Buckets (amazonS3client:AmazonS3Client) =
    
    let jsonConfig = (jsonConfig "prod")
    
    // Tags

    let websiteTags = 
      [   ("Name", "l1x.be"); ("Environment", "website"); 
          ("Scope", "global"); ("Stage", "prod");         ]

    let websiteAwsS3Tags = convertToS3Tags websiteTags

    // dev.l1x.be

    let websiteDocuments : WebsiteDocuments = 
      { IndexDocument = "index.html"; ErrorDocument = "error.html"; }

    let bucketRegionString = "eu-west-1"

    let devl1xbeBucketConfig =
      match bucketRegionString, jsonConfig.AllowedAwsRegions with
        | AllowedS3Region region  -> 
          Some (createWebsiteBucketConfig "dev.l1x.be" region websiteDocuments websiteAwsS3Tags)
        | _ ->
          Console.Error.WriteLine("Unsupported region: {0}", bucketRegionString);
          Environment.Exit 1;
          None
    
    let devl1xbeBucket = 
      createWebsiteBucket devl1xbeBucketConfig.Value

    match createS3bucket amazonS3client devl1xbeBucket websiteAwsS3Tags with 
      | response -> Console.WriteLine("O hai, {0}", response)

    // redirect l1x.be -> dev.l1x.be

    let l1xbeBucketConfig =
      match bucketRegionString, jsonConfig.AllowedAwsRegions with
        | AllowedS3Region region  -> 
          Some (createRedirectOnlyBucketConfig "l1x.be" region { RedirectTo = "dev.l1x.be" } websiteAwsS3Tags)
        | _ ->
          Console.Error.WriteLine("Unsupported region: {0}", bucketRegionString);
          Environment.Exit 1;
          None
    
    let l1xbeBucket = 
      createRedirectOnlyBucket l1xbeBucketConfig.Value

    match createS3bucket amazonS3client l1xbeBucket websiteAwsS3Tags with 
      | response -> Console.WriteLine("O hai, {0}", response)