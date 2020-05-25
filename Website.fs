namespace CloudClamp

// external
open Amazon.S3
open Amazon.Runtime
open System

// internal
open Aws
open AwsS3
open Config
open Utils

module Website =

  // Utils

  let awsProfileCredentials (awsProfileName:string) =
    getAwsCredentials (SharedFile(fileName = None, profileName = awsProfileName))

  let createS3Client awsRegion awsProfileName =
    getAwsS3Client 
      (getOkValue (awsProfileCredentials awsProfileName)) 
      (getOkValue (createAwsS3Config awsRegion))

  // Create

  let createS3Buckets (amazonS3client:AmazonS3Client) allowedAwsRegions =
     
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
      match bucketRegionString, allowedAwsRegions with
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
      match bucketRegionString, allowedAwsRegions with
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
  

  let getS3Buckets (amazonS3client:AmazonS3Client) = 
    ()

  let planS3Buckets (amazonS3client:AmazonS3Client) allowedAwsRegions = 
    ()

  // ######################################################################################
  // ##################################### Commands #######################################
  // ######################################################################################

  let executeCommand command stage =
    
    if stage = "prod" then

      Console.WriteLine("Deploying serive: Website stage: prod")

      let jsonConfig      = jsonConfig "prod"
      let awsRegion       = jsonConfig.AwsRegion
      let awsProfileName  = jsonConfig.AwsProfileName

      let s3ClientMaybe = createS3Client awsRegion awsProfileName
      match s3ClientMaybe, command with
        | Ok s3Client, "deploy" -> 
            createS3Buckets s3Client jsonConfig.AllowedAwsRegions
        | Ok s3Client, "show" -> 
            getS3Buckets s3Client
        | Ok s3Client, "plan" -> 
            planS3Buckets s3Client jsonConfig.AllowedAwsRegions
        | Ok _, _ -> 
            Console.Error.WriteLine("Unsupported command")
            Environment.Exit(1)
        | Error err, _ -> 
            Console.Error.WriteLine("{0}", err)
            Environment.Exit(1)
    else
      Console.Error.WriteLine("The only supported stage is prod for Website")
      Environment.Exit(1)

  let show stage = 
    executeCommand "show" stage
    
  let plan stage =
    executeCommand "plan" stage
    
  let deploy (stage:string) =
    executeCommand "deploy" stage