
namespace CloudClamp

// external
open Amazon.S3
open Amazon.S3.Model
open System

// internal
open Aws
open AwsRoute53
open Config
open Logging
open Utils

module Dns =

  // Utils

  let awsProfileCredentials (awsProfileName:string) =
    getAwsCredentials (SharedFile(fileName = None, profileName = awsProfileName))

  let createRoute53Client awsRegion awsProfileName =
    getAwsRoute53Client 
      (getOkValue (awsProfileCredentials awsProfileName)) 
      (getOkValue (createAwsRoute53Config awsRegion))

  // Create

  let executeCommand command stage =
    
    if stage = "prod" then

      Console.WriteLine("Deploying serive: Website stage: prod")

      let jsonConfig      = jsonConfig "prod"
      let awsRegion       = jsonConfig.AwsRegion
      let awsProfileName  = jsonConfig.AwsProfileName

      let s3ClientMaybe = createRoute53Client awsRegion awsProfileName

      match s3ClientMaybe, command with
        | Ok s3Client, "deploy" -> 
            ()
        | Ok s3Client, "show" -> 
            ()
        | Ok s3Client, "plan" -> 
            ()
        | Ok _, _ -> 
            Console.Error.WriteLine("Unsupported command")
            Environment.Exit(1)
        | Error err, _ -> 
            Console.Error.WriteLine("{0}", err)
            Environment.Exit(1)
    else
      Console.Error.WriteLine("The only supported stage is prod for Website")
      Environment.Exit(1)

  let list (stage:string) =
    executeCommand "list" stage

  let show (stage:string) = 
    executeCommand "show" stage
    
  let plan (stage:string) =
    executeCommand "plan" stage
    
  let deploy (stage:string) =
    executeCommand "deploy" stage