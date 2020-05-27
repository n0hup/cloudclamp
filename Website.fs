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
      [   ("Name", "l1x.be");   ("Environment", "website"); 
          ("Scope", "global");  ("Stage", "prod");         ]

    // dev.l1x.be

    let websiteDocuments : WebsiteDocuments = 
      { IndexDocument = "index.html"; ErrorDocument = "error.html"; }

    let s3BucketWithConfigDev = 
      createWebsiteBucketConfig "dev.l1x.be" "eu-west-1" "prod" websiteDocuments websiteTags

    createS3bucket amazonS3client s3BucketWithConfigDev |> ignore
    
    // redirect l1x.be -> dev.l1x.be

    let redirectTo : RedirectOnly = { RedirectTo = "dev.l1x.be" }

    let s3BucketWithConfigApex = 
      createRedirectBucketConfig "l1x.be" "eu-west-1" "prod" redirectTo websiteTags
    
    createS3bucket amazonS3client s3BucketWithConfigApex |> ignore
    
    // end 
  
  // Get bucket info for show
  let getS3Buckets (amazonS3client:AmazonS3Client) allowedAwsRegions = 
    // "dev.l1x.be"
    // "l1x.be"
    ()
  
  // Determine 
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
            getS3Buckets s3Client jsonConfig.AllowedAwsRegions
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

  let show (stage:string) = 
    executeCommand "show" stage
    
  let plan (stage:string) =
    executeCommand "plan" stage
    
  let deploy (stage:string) =
    executeCommand "deploy" stage