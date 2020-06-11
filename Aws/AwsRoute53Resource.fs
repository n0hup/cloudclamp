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

  let fromStringToResourceRecordType =
    function
      | "A"       -> A
      | "AAAA"    -> AAAA
      | "CNAME"   -> CNAME
      | "DNSKEY"  -> DNSKEY
      | "MX"      -> MX
      | "NS"      -> NS
      | "PTR"     -> PTR
      | "RRSIG"   -> RRSIG
      | "SOA"     -> SOA
      | "TXT"     -> TXT
      | x         -> failwith (sprintf "ResourceRecordType not found: %s" x)


  type Alias =
    {
      Name            : string
      Type            : ResourceRecordType
      AliasTarget     : AliasTarget
    }

  type Record =
    {
      Name            : string
      Type            : ResourceRecordType
      ResourceRecords : List<string>
      TTL             : uint32
    }

  type ResourceRecordSet =
    | Alias of
        Alias
    | Record of
        Record

  type DnsResource = {
    Name               : string
    HostedZoneId       : Option<string>
    ResourceRecordSets : List<ResourceRecordSet>
  }