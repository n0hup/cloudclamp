namespace CloudClamp

// external

// internal

module AwsRoute53Resource =


  type AliasTarget = {
    DNSName               : string
    EvaluateTargetHealth  : bool
    HostedZoneId          : string
  }

  type ResourceRecordType =
      A | AAAA | CNAME | DNSKEY | MX | NS | PTR | RRSIG | SOA | TXT

  type ResourceRecordSet =
    | Alias of Name : string * Type : ResourceRecordType * AliasTarget : AliasTarget
    | Record of Name : string * Type : ResourceRecordType *  ResourceRecords : List<string> * TTL : uint32

  type DnsResource = {
    HostedZoneId       : string
    ResourceRecordSets : List<ResourceRecordSet>
  }

  //
  // Helpers
  //

  let rrTypeToString (rrType:ResourceRecordType) : string =
    match rrType with
      | A         -> "A"
      | AAAA      -> "AAAA"
      | CNAME     -> "CNAME"
      | DNSKEY    -> "DNSKEY"
      | MX        -> "MX"
      | NS        -> "NS"
      | PTR       -> "PTR"
      | RRSIG     -> "RRSIG"
      | SOA       -> "SOA"
      | TXT       -> "TXT"