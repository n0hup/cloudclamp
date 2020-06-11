namespace CloudClamp

// external
open System.Text.Json
open Fleece.SystemTextJson
open Fleece.SystemTextJson.Operators
open FSharpPlus
// internal

module AwsRoute53Resource =

  type AliasTarget =
    {
      DNSName               : string
      EvaluateTargetHealth  : bool
      HostedZoneId          : string
    }

  type ResourceRecordType =
    A | AAAA | CNAME | DNSKEY | MX | NS | PTR | RRSIG | SOA | TXT


  // type Alias =
  //   {
  //       Name            : string
  //       Type            : ResourceRecordType
  //       AliasTarget     : AliasTarget
  //   }

  // type Record =
  //   {
  //       Name            : string
  //       Type            : ResourceRecordType
  //       ResourceRecords : List<string>
  //       TTL             : uint32
  //   }

  // type ResourceRecordSet2 =
  //   | Alias of Alias
  //   | Record of Record


  type ResourceRecordSet =
    | Alias of
        Name            : string *
        Type            : ResourceRecordType *
        AliasTarget     : AliasTarget
    | Record of
        Name            : string *
        Type            : ResourceRecordType *
        ResourceRecords : List<string> *
        TTL             : uint32

  type DnsResource = {
    Name               : string
    HostedZoneId       : Option<string>
    ResourceRecordSets : List<ResourceRecordSet>
  }