namespace CloudClamp

// external

// internal
open AwsRoute53Resource
open Logging
open Config
open Fleece.SystemTextJson

module Blog =

  let loggerBlog = Logger.CreateLogger "Website" loggingConfig.LogLevel

  let ns : ResourceRecordSet  =
    {
      Name = "l1x.be."
      Type =  "NS"
      RecordsOrAlias =
        ResourceRecords [
          { Value = "ns-583.awsdns-08.net."}
          { Value = "ns-504.awsdns-63.com."}
          { Value = "ns-1708.awsdns-21.co.uk."}
          { Value = "ns-1176.awsdns-19.org."}
        ]
      TTL = Some 172800u
    }

  let soa : ResourceRecordSet =
   {
      Name = "l1x.be."
      Type =  "SOA"
      RecordsOrAlias =
        ResourceRecords [
          {Value = "ns-583.awsdns-08.net. awsdns-hostmaster.amazon.com. 1 7200 900 1209600 86400"}
        ]
      TTL = Some 900u
   }

  let resourceRecordSets = [
    ns
    soa
    { Name = "dev.l1x.be."
      RecordsOrAlias =
        AliasTarget {
          DNSName = "dbrgct5gwrbsd.cloudfront.net."
          EvaluateTargetHealth = false
          HostedZoneId = "Z2FDTNDATAQYW2" }
      Type = "A"
      TTL = None
    };
    { Name = "dev.l1x.be.";
      RecordsOrAlias =
        AliasTarget {
          DNSName = "dbrgct5gwrbsd.cloudfront.net."
          EvaluateTargetHealth = false
          HostedZoneId = "Z2FDTNDATAQYW2" }
      Type = "AAAA"
      TTL = None
    };
    {
      Name = "_0a772de2cfcacc285d64f76d53afb931.dev.l1x.be.";
      Type = "CNAME";
      RecordsOrAlias = ResourceRecords [{ Value = "_031a48dd8cb38a7723ba15eeef0ae2b2.tfmgdnztqk.acm-validations.aws."}];
      TTL = Some 300u
    };
  ]

  let dnsResource =
    { Name = "l1x.be"; HostedZoneId = None; ResourceRecordSets = resourceRecordSets }

  type Blog =
    {
      DnsZones  : List<AwsRoute53Resource.DnsResource>
    }

  let blog : Blog =
    {
      DnsZones = [dnsResource]
    }



  let executeCommand command stage =
    let log = sprintf "command: %s stage: %s" command stage
    // let outp = aliasTargetToJSON {
    //   DNSName = "dbrgct5gwrbsd.cloudfront.net.";
    //   EvaluateTargetHealth = false;
    //   HostedZoneId = "Z2FDTNDATAQYW2" }
    // loggerBlog.LogInfo outp
    // let aliasJson =
    //   """{"DNSName":"dbrgct5gwrbsd.cloudfront.net.","EvaluateTargetHealth":false,"HostedZoneId":"Z2FDTNDATAQYW2"}"""
    // let alias : AliasTarget ParseResult = parseJson aliasJson
    // // loggerBlog.LogInfo (sprintf "%A" alias)s
    // let a : ResourceRecordsOrAlias =
    //   AliasTarget
    //     { DNSName = "dbrgct5gwrbsd.cloudfront.net.";
    //     EvaluateTargetHealth = false;
    //     HostedZoneId = "Z2FDTNDATAQYW2" }
    // printfn "%s" (string (toJson a))
    // let b : ResourceRecordsOrAlias =
    //   ResourceRecords [{ Value = "_031a48dd8cb38a7723ba15eeef0ae2b2.tfmgdnztqk.acm-validations.aws."}]

    let lofasz =
      [ AliasTarget
          { DNSName = "dbrgct5gwrbsd.cloudfront.net.";
          EvaluateTargetHealth = false;
          HostedZoneId = "Z2FDTNDATAQYW2" }
        ResourceRecords [{ Value = "_031a48dd8cb38a7723ba15eeef0ae2b2.tfmgdnztqk.acm-validations.aws."}]
      ]
    printfn "%s" (string (toJson lofasz))
    loggerBlog.LogInfo log

