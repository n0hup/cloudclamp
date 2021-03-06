namespace CloudClamp

// external
open Amazon.Runtime.CredentialManagement
open System

// internal
open Config
open Logging

module Aws =

  // This has to be a match on different options
  // Not sure how the SDK handles this, it seems to be all over the place
  // SharedCredentialsFile is for credential files
  // this handles the case when there is a role to assume 
  // I could not find the other popular one with instance profiles

  // It might be the case that instane role credentials do not need this functionality
  // https://aws.amazon.com/blogs/security/a-new-and-standardized-way-to-manage-credentials-in-the-aws-sdks/
  // To be investigated further


  let loggerAws = Logger.CreateLogger "Aws" loggingConfig.LogLevel

  type AwsCredentialsProvider =
    | SharedFile of fileName : Option<string> * profileName : string
    | InstanceProfile

  let getAwsCredentialsFromSharedFile fileNameOption profileName  =
    try
      let sharedFile = 
        match fileNameOption with
          | None -> SharedCredentialsFile()
          | Some fileName -> SharedCredentialsFile(fileName)

      let success1, basicProfile = sharedFile.TryGetProfile(profileName)
      let success2, awsCredentials = AWSCredentialsFactory.TryGetAWSCredentials(basicProfile, sharedFile)

      if success1 && success2 then
        Ok awsCredentials
      else
        Error "Could not get AWS credentials"
    with ex ->
      loggerAws.LogError(String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))
      Environment.Exit(1)
      Error "never reached"

  let getAwsCredentialsFromInstanceProfile =
    Environment.Exit(1)
    Error "not implemented"

  let getAwsCredentials (awsCredentialsProvider:AwsCredentialsProvider) =
    match awsCredentialsProvider with
      | SharedFile (fileName, profileName) -> getAwsCredentialsFromSharedFile fileName profileName
      | InstanceProfile -> getAwsCredentialsFromInstanceProfile


 