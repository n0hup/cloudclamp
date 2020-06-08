namespace CloudClamp

// external
// open Newtonsoft.Json
open Thoth.Json

// internal
open AwsRoute53Resource
open Logging
open Config
open System.IO

module Blog =

  let loggerBlog = Logger.CreateLogger "Website" loggingConfig.LogLevel

  let ns : ResourceRecordSet  =
    Record(
      "l1x.be."
      , NS
      ,[  "ns-583.awsdns-08.net."
          "ns-504.awsdns-63.com."
          "ns-1708.awsdns-21.co.uk."
          "ns-1176.awsdns-19.org." ]
      , 172800u
    )

  let soa : ResourceRecordSet =
    Record(
      "l1x.be."
      , SOA
      , ["ns-583.awsdns-08.net. awsdns-hostmaster.amazon.com. 1 7200 900 1209600 86400"]
      , 900u
    )

  let resourceRecordSets = [
    ns;
    soa;
    Alias(
      "dev.l1x.be.",
      A,
      { DNSName = "dbrgct5gwrbsd.cloudfront.net.";
        EvaluateTargetHealth = false;
        HostedZoneId = "Z2FDTNDATAQYW2" }
    );
    Alias(
      "dev.l1x.be.",
      AAAA,
      { DNSName = "dbrgct5gwrbsd.cloudfront.net.";
        EvaluateTargetHealth = false;
        HostedZoneId = "Z2FDTNDATAQYW2" }
    );
    Record(
      "_0a772de2cfcacc285d64f76d53afb931.dev.l1x.be."
      , CNAME
      , ["_031a48dd8cb38a7723ba15eeef0ae2b2.tfmgdnztqk.acm-validations.aws."]
      , 300u
    );
  ]

  let dnsResource =
    { Name = "l1x.be"; HostedZoneId = None; ResourceRecordSets = resourceRecordSets }

  type Blog = {
    DnsZones  : List<AwsRoute53Resource.DnsResource>
  }

  let blog : Blog =
    {
      DnsZones = [dnsResource]
    }

  // type BlogState =
  //   JsonProvider<"state/prod/blog.json">

  // let blogState (stage:string) =
  //   BlogState.Parse(File.ReadAllText(sprintf "state/%s/blog.json" stage))

  let executeCommand command stage =
    let log = sprintf "command: %s stage: %s" command stage
    // let options = JsonSerializerOptions()
    // options.Converters.Add(JsonFSharpConverter())
    // let soap = JsonSerializer.Serialize(soa, options)
    loggerBlog.LogInfo log
    // loggerBlog.LogInfo (sprintf "%A" soap)

