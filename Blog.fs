namespace CloudClamp

// external
// internal
open AwsRoute53Resource
open Logging
open Config

module Blog =

  let loggerBlog = Logger.CreateLogger "Website" loggingConfig.LogLevel

  let ns : ResourceRecordSet  =
    Record {
      Name = "l1x.be."
      Type = A
      ResourceRecords = [
        "ns-583.awsdns-08.net."
        "ns-504.awsdns-63.com."
        "ns-1708.awsdns-21.co.uk."
        "ns-1176.awsdns-19.org."
      ]
      TTL =  172800u
    }

  let soa : ResourceRecordSet =
   Record {
      Name = "l1x.be."
      Type =  SOA
      ResourceRecords = [
        "ns-583.awsdns-08.net. awsdns-hostmaster.amazon.com. 1 7200 900 1209600 86400"
      ]
      TTL = 900u
   }

  let resourceRecordSets = [
    ns
    soa
    Alias {
      Name = "dev.l1x.be."
      AliasTarget = {
        DNSName = "dbrgct5gwrbsd.cloudfront.net."
        EvaluateTargetHealth = false
        HostedZoneId = "Z2FDTNDATAQYW2"
      }
      Type = A
    }
    Alias {
      Name = "dev.l1x.be."
      AliasTarget = {
        DNSName = "dbrgct5gwrbsd.cloudfront.net."
        EvaluateTargetHealth = false
        HostedZoneId = "Z2FDTNDATAQYW2" }
      Type = AAAA
    }
    Record {
      Name = "_0a772de2cfcacc285d64f76d53afb931.dev.l1x.be."
      Type = CNAME
      ResourceRecords = ["_031a48dd8cb38a7723ba15eeef0ae2b2.tfmgdnztqk.acm-validations.aws."]
      TTL = 300u
    }
  ]

  let dnsResource =
    { Name = "l1x.be"; HostedZoneId = Some "szopki"; ResourceRecordSets = resourceRecordSets }

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
    loggerBlog.LogInfo log






