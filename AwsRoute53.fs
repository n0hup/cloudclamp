namespace CloudClamp

open Amazon
open Amazon.Route53
open Amazon.Route53.Model
open System
open System.Collections.Generic

open AwsRoute53GetZone

module AwsRoute53 =

  let createAwsRoute53Config awsRegionName =
    try
      let region = RegionEndpoint.GetBySystemName(awsRegionName)
      let config = AmazonRoute53Config()
      config.MaxConnectionsPerServer <- new Nullable<int>(64)
      config.RegionEndpoint <- region
      Ok config
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message);
      Environment.Exit(1);
      Error (String.Format("{0} : {1}", ex.Message, ex.InnerException.Message))

  let getAwsRoute53Client awsCredentials (awsS3Config:AmazonRoute53Config) =
    try
      Console.WriteLine("Connecting to Route53...")
      Console.WriteLine(
        "AWS Route53 Config: RegionEndpoint :: {0} MaxConnectionsPerServer :: {1} BufferSize :: {2} ServiceVersion :: {3}",
        awsS3Config.RegionEndpoint,
        awsS3Config.MaxConnectionsPerServer,
        awsS3Config.BufferSize,
        awsS3Config.ServiceVersion
      )
      Ok(new AmazonRoute53Client(awsCredentials, awsS3Config))
    with ex ->
      Console.Error.WriteLine("Connecting to Route53 has failed")
      let err = String.Format("{0} : {1}", ex.Message, ex.InnerException.Message)
      Console.Error.WriteLine err
      Error err

  let convertToRoute53Tags (tags:list<string * string>) =
    let awsRoute53Tags = List<Tag>()
    for tag in tags do
      let tba : Tag =  Tag()
      tba.Key   <- fst tag
      tba.Value <- snd tag
      awsRoute53Tags.Add(tba)
    Some awsRoute53Tags

  let getRoute53HostedZone (amazonRoute53client:AmazonRoute53Client) (id:string) =
    getHostedZone amazonRoute53client id
    