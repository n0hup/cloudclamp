namespace CloudClamp

// external
open Amazon
open Amazon.S3
open Amazon.S3.Model
open Amazon.Runtime.CredentialManagement
open System
open System.Collections.Generic

// internal
open AwsS3Bucket
open Config


module Main =

  let getAwsProfileCredentials profileName  =
    try
      let sharedFile = SharedCredentialsFile();
      let success1, basicProfile = sharedFile.TryGetProfile(profileName)
      let success2, awsCredentials = AWSCredentialsFactory.TryGetAWSCredentials(basicProfile, sharedFile)
      if success1 && success2 then
        Console.WriteLine(
          "CredentialDescription : {0}  credentials: {1}",
          basicProfile.CredentialDescription,
          awsCredentials.GetCredentials
        )
        Some(awsCredentials)
      else
        None
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
      None

  let createAwsS3Config awsRegionName =
    let region = RegionEndpoint.GetBySystemName(awsRegionName)
    let config = AmazonS3Config()
    config.MaxConnectionsPerServer <- new Nullable<int>(64)
    config.RegionEndpoint <- region
    config

  let getAwsS3Client awsCredentials (awsS3Config:AmazonS3Config) =
    try
      Console.WriteLine("Connecting to S3...")
      Console.WriteLine(
        "AWS S3 Config: RegionEndpoint :: {0} MaxConnectionsPerServer :: {1} BufferSize :: {2} ServiceVersion :: {3}",
        awsS3Config.RegionEndpoint,
        awsS3Config.MaxConnectionsPerServer,
        awsS3Config.BufferSize,
        awsS3Config.ServiceVersion
      )
      Some(new AmazonS3Client(awsCredentials, awsS3Config))
    with ex ->
      Console.Error.WriteLine("Connecting to S3 has failed")
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
      None

  //  We need to figure out how to structure this
  let createS3Buckets (amazonS3client:AmazonS3Client) =

    // Tags

    let websiteTags = 
      [   ("Name", "l1x.be"); ("Environment", "website"); 
          ("Scope", "global"); ("Stage", "prod");         ]

    let websiteAwsS3Tags = convertToS3Tags websiteTags

    // dev.l1x.be

    let websiteDocuments : WebsiteDocuments = 
      { IndexDocument = "index.html"; ErrorDocument = "error.html"; }

    let devl1xbeBucketConfig = 
      createWebsiteBucketConfig "dev.l1x.be" EUW1 websiteDocuments websiteAwsS3Tags

    let devl1xbeBucket = 
      createWebsiteBucket devl1xbeBucketConfig

    match createS3bucket amazonS3client devl1xbeBucket websiteAwsS3Tags with 
      | response -> Console.WriteLine("O hai, {0}", response)

    // redirect l1x.be -> dev.l1x.be

    let l1xbeBucketConfig = 
      createRedirectOnlyBucketConfig "l1x.be" EUW1 { RedirectTo = "dev.l1x.be" } websiteAwsS3Tags 

    let l1xbeBucket = 
      createRedirectOnlyBucket l1xbeBucketConfig 

    match createS3bucket amazonS3client l1xbeBucket websiteAwsS3Tags with 
      | response -> Console.WriteLine("O hai, {0}", response)


  [<EntryPoint>]
  let main argv =
    let awsProfileCredentials = getAwsProfileCredentials jsonConfig.AwsProfileName
    if awsProfileCredentials.IsSome then
      Console.WriteLine("AWS Credentials are [OK]")
    else
      Console.WriteLine("AWS Credentials could not be created [ERR]")
      Environment.Exit(1)

    let awsS3Config = createAwsS3Config jsonConfig.AwsRegion
    let awsS3Client = getAwsS3Client awsProfileCredentials.Value awsS3Config

    if awsS3Client.IsSome then
      createS3Buckets awsS3Client.Value
    else
      Console.Error.WriteLine("Could not connect to AWS S3")
    
    //return
    0

  // REDIRECT ONLY


  // resource "aws_s3_bucket" "s3-bucket" {
  //   bucket = var.bucket-name
  //   acl    = var.acl
  //   region = var.region

  //   website {
  //     redirect_all_requests_to = var.redirect-all-requests-to
  //   }

  //   tags = {
  //     Name        = format("%s-s3-bucket", var.bucket-name)
  //     Environment = var.environment
  //     Stage       = var.stage
  //     Scope       = var.scope
  //   }
  // }

  // module "static-website-apex" {
  //   source                   = "./modules/static-website-redirect-only/"
  //   acl                      = "public-read"
  //   bucket-name              = "lambdainsight.com"
  //   environment              = "website"
  //   redirect-all-requests-to = "www.lambdainsight.com"
  //   region                   = "eu-central-1"
  //   scope                    = "global"
  //   stage                    = "prod"
  // }

  // WWW

  // resource "aws_s3_bucket" "s3-bucket" {
  //   bucket = var.bucket-name
  //   acl    = var.acl
  //   region = var.region

  //   website {
  //     index_document           = length(var.redirect-all-requests-to) > 0 ? null : "index.html"
  //     error_document           = length(var.redirect-all-requests-to) > 0 ? null : "error.html"
  //     redirect_all_requests_to = length(var.redirect-all-requests-to) > 0 ? var.redirect-all-requests-to : null
  //   }

  //   tags = {
  //     Name        = format("%s-s3-bucket", var.bucket-name)
  //     Environment = var.environment
  //     Stage       = var.stage
  //     Scope       = var.scope
  //   }
  // }

  // resource "aws_s3_bucket_policy" "s3-bucket-policy" {
  //   bucket = aws_s3_bucket.s3-bucket.id
  //   policy = data.aws_iam_policy_document.allow-cloudfront-read.json
  // }

  // data "aws_iam_policy_document" "allow-cloudfront-read" {

  //   statement {
  //     sid = "PublicReadGetObject"

  //     principals {
  //       type        = "*"
  //       identifiers = ["*"]
  //     }

  //     resources = [
  //       format("arn:aws:s3:::%s", var.bucket-name),
  //       format("arn:aws:s3:::%s/*", var.bucket-name)
  //     ]

  //     actions = ["s3:GetObject"]

  //     condition {
  //       test     = "IpAddress"
  //       variable = "aws:SourceIp"
  //       # https://www.cloudflare.com/ips-v4
  //       # https://www.cloudflare.com/ips-v6
  //       values = [
  //         "173.245.48.0/20",
  //         "103.21.244.0/22",
  //         "103.22.200.0/22",
  //         "103.31.4.0/22",
  //         "141.101.64.0/18",
  //         "108.162.192.0/18",
  //         "190.93.240.0/20",
  //         "188.114.96.0/20",
  //         "197.234.240.0/22",
  //         "198.41.128.0/17",
  //         "162.158.0.0/15",
  //         "104.16.0.0/12",
  //         "172.64.0.0/13",
  //         "131.0.72.0/22",
  //         "2400:cb00::/32",
  //         "2606:4700::/32",
  //         "2803:f800::/32",
  //         "2405:b500::/32",
  //         "2405:8100::/32",
  //         "2a06:98c0::/29",
  //         "2c0f:f248::/32",
  //       ]
  //     }
  //   }
  // }

  // module "static-website-www" {
  //   source = "./modules/static-website/"
  //   # S3 - Static Website
  //   acl                      = "private"
  //   bucket-name              = "www.lambdainsight.com"
  //   environment              = "website"
  //   redirect-all-requests-to = ""
  //   region                   = "eu-central-1"
  //   scope                    = "global"
  //   stage                    = "prod"
  // }



