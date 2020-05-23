namespace CloudClamp

// external
open Amazon
open Amazon.S3
open Amazon.Runtime.CredentialManagement
open System

// internal
// config values
open Config
// infrastucture
open Website
// open Hadoop
// open Elk

module Main =
  
  let getOkValue (Ok v) =
    v

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
        Ok awsCredentials
      else
        Error "Could not get AWS credentials"
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  let createAwsS3Config awsRegionName =
    try
      let region = RegionEndpoint.GetBySystemName(awsRegionName)
      let config = AmazonS3Config()
      config.MaxConnectionsPerServer <- new Nullable<int>(64)
      config.RegionEndpoint <- region
      Ok config
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))


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
      Ok(new AmazonS3Client(awsCredentials, awsS3Config))
    with ex ->
      Console.Error.WriteLine("Connecting to S3 has failed")
      let err = String.Format("{0} : {1}", ex.Message, ex.InnerException.Message)
      Console.Error.WriteLine err
      Error err

  [<EntryPoint>]
  let main argv =
    let jsonConfig = (jsonConfig "prod")
    
    let awsProfileCredentials = 
      match getAwsProfileCredentials jsonConfig.AwsProfileName with
      | Ok creds -> 
          Console.WriteLine("AWS Credentials are ok");
          Ok creds
      | Error err ->
          Console.Error.WriteLine("AWS Credentials could not be created");
          Console.Error.WriteLine(err);
          Environment.Exit(1);
          Error "This will be never reached"

    let awsS3Config = 
      match createAwsS3Config jsonConfig.AwsRegion with
        | Ok config -> 
            Ok config
        | Error err -> 
            Console.Error.WriteLine("AWS Config could not be created");
            Console.Error.WriteLine(err);
            Environment.Exit(1);
            Error "This will be never reached"

    let awsS3ClientMaybe = 
      getAwsS3Client (getOkValue awsProfileCredentials) (getOkValue awsS3Config)

    match awsS3ClientMaybe with
      | Ok awsS3Client -> createS3Buckets awsS3Client
      | Error err -> Console.Error.WriteLine(err)

  // let fromStringToStage (s) : Stage =
  //    "dev" -> Dev

  // type Stage = Dev | Qa | Prod | Test
  // -stage qa -service hadoop
  // match stage, service
  // 










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



