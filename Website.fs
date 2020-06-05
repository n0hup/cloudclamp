namespace CloudClamp

// external
open Amazon.S3
open System

// internal
open Aws
open AwsS3
open Config
open Logging
open Utils
open System.IO

module Website =

  // Utils

  let loggerWebsite = Logger.CreateLogger "Website" loggingConfig.LogLevel

  let awsProfileCredentials (awsProfileName:string) =
    loggerWebsite.LogInfo (String.Format("{0}", awsProfileName))
    getAwsCredentials (SharedFile(fileName = None, profileName = awsProfileName))

  let createS3Client awsRegion awsProfileName =
    getAwsS3Client 
      (getOkValue (awsProfileCredentials awsProfileName)) 
      (getOkValue (createAwsS3Config awsRegion))

  let getAwsDetails (stage:string) =
    let config          = jsonConfig "prod"
    let awsRegion       = config.AwsRegion
    let awsProfileName  = config.AwsProfileName
    loggerWebsite.LogInfo(sprintf "awsRegion: %A :: awsProfileName: %A" awsRegion awsProfileName)
    (awsRegion, awsProfileName)

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
      { RedirectTo = "dev.l1x.be"; Protocol = "https" }

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

  type AliasTarget = {
    DNSName: string
    EvaluateTargetHealth: Boolean
    HostedZoneId: string
  }

  type ResourceRecordType = 
    A | AAAA | CNAME | DNSKEY | MX | NS | PTR | RRSIG | SOA | TXT 

  type ResourceRecord = {
    Value: string
  }

  type ResourceRecordSet = {
    Name            : string
    ResourceRecords : List<ResourceRecord>
    TTL             : Option<int32>
    Type            : ResourceRecordType
    AliasTarget     : Option<AliasTarget>
  }

  type Dns = {
    Id                  : string
    ResourceRecordSets  : List<ResourceRecordSet>
  }

  let rr (str) : ResourceRecord = 
    { Value= str }

  type Certificate = {
    Id  : string
  }

  type Bucket = {
    Name : string
  }

  type Stack = {
      Dns         : Dns
      // Certificate : Certificate
      // Bucket      : Bucket
  }

  let dns =    
    let ns : ResourceRecordSet  = {
        Name            = "l1x.be." 
        ResourceRecords = [
          { Value = "ns-583.awsdns-08.net."    };
          { Value = "ns-504.awsdns-63.com."    };
          { Value = "ns-1708.awsdns-21.co.uk." };
          { Value = "ns-1176.awsdns-19.org."   };
        ]
        Type            = NS
        TTL             = Some 172800
        AliasTarget     = None
      }
    
    let soa : ResourceRecordSet = {
      Name            = "l1x.be." 
      ResourceRecords = [
        { Value = "ns-583.awsdns-08.net. awsdns-hostmaster.amazon.com. 1 7200 900 1209600 86400" };
      ]
      Type            = SOA
      TTL             = Some 900
      AliasTarget     = None
    }

    let resourceRecordSets = [ns; soa;] 
    { Id = "/hostedzone/Z04374393O7HMC10LT1Q2"; ResourceRecordSets = resourceRecordSets} 

  let stack () =
    let ns : ResourceRecordSet  = {
        Name            = "l1x.be." 
        ResourceRecords = [{Value = "ns-583.awsdns-08.net."}]
        Type            = NS
        TTL             = Some 172800
        AliasTarget     = None
      }
    let resourceRecordSets = [ns]
    let dns : Dns = 
      { Id = "/hostedzone/Z04374393O7HMC10LT1Q2"; ResourceRecordSets = resourceRecordSets}
    
    {Dns = dns;}

  let showStack stage awsRegion awsProfileName =
    
    let state = File.ReadAllLines(Path.Combine("state", stage, "website.yaml"))

    ()

  // ######################################################################################
  // ##################################### Commands #######################################
  // ######################################################################################

  let executeCommand command stage =
      
    loggerWebsite.LogInfo(String.Format("{0} :: {1}", command, stage)) 

    match command, stage with
      | "showStack", "prod" ->
          let (awsRegion, awsProfileName) = getAwsDetails "prod"
          showStack "prod"
          ()
      | "refreshState", "prod" ->
          let (awsRegion, awsProfileName) = getAwsDetails "prod"
          ()
      | "deployStack", "prod" -> 
          let (awsRegion, awsProfileName) = getAwsDetails "prod"
          ()
      | "importResource", "prod" ->
          let (awsRegion, awsProfileName) = getAwsDetails "prod"
          ()
      | "destroyStack", _ ->
          loggerWebsite.LogError("destroyStack is not allowed for this stack")
      | command, stage ->
          loggerWebsite.LogError( sprintf "Unsupported command %A and stage %A" command stage)
          Environment.Exit(1)

  