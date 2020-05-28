namespace CloudClamp

// external
open Amazon.S3
open Amazon.S3.Model
open System

// internal
open Aws
open AwsS3
open Config
open Logging
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

  let createS3Buckets (amazonS3client:AmazonS3Client) =
     
    // Tags

    let websiteTags = 
      [   ("Name", "l1x.be");   ("Environment", "website"); 
          ("Scope", "global");  ("Stage", "prod");         ]

    // logs.l1x.be

    let s3BucketWithConfigLogs = 
      createPrivateBucketConfig 
        "logs.l1x.be"     // name
        "eu-west-1"       // region
        "prod"            // stage
        websiteTags       // tagging
        None              // policy
        None              // logging
    
    createS3Bucket amazonS3client s3BucketWithConfigLogs |> ignore
    
    // dev.l1x.be

    let websiteDocuments : WebsiteDocuments = 
      { IndexDocument = "index.html"; ErrorDocument = "error.html"; }  

    let loggingConfig : BucketLoggingConfig =
      {  TargetBucketName = "logs.l1x.be"; TargetPrefix = "dev.l1x.be" }

    let s3BucketWithConfigDev = 
      createWebsiteBucketConfig 
        "dev.l1x.be"        // name
        "eu-west-1"         // region
        "prod"              // stage
        websiteDocuments    // website
        websiteTags         // tagging
        None                // policy
        None                // logging

    createS3Bucket amazonS3client s3BucketWithConfigDev |> ignore
    
    // redirect l1x.be -> dev.l1x.be

    let redirectTo : RedirectOnly = 
      { RedirectTo = "dev.l1x.be" }

    let s3BucketWithConfigApex = 
      createRedirectBucketConfig 
        "l1x.be"          // name
        "eu-west-1"       // region
        "prod"            // stage
        redirectTo        // website
        websiteTags       // tagging
        None              // policy
        None              // logging
   
    createS3Bucket amazonS3client s3BucketWithConfigApex |> ignore

    ()
      
  let getS3Buckets (amazonS3client:AmazonS3Client) = 
    getS3Bucket amazonS3client "logs.l1x.be" |> ignore
    getS3Bucket amazonS3client "dev.l1x.be" |> ignore
    getS3Bucket amazonS3client "l1x.be" |> ignore
    ()
  
  let planS3Buckets (amazonS3client:AmazonS3Client) = 
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
            createS3Buckets s3Client
        | Ok s3Client, "show" -> 
            getS3Buckets s3Client
        | Ok s3Client, "plan" -> 
            planS3Buckets s3Client
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