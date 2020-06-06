namespace CloudClamp

// external

// internal

module AwsRoute53Resource =

  type ResourceRecordType = 
      A | AAAA | CNAME | DNSKEY | MX | NS | PTR | RRSIG | SOA | TXT 

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

  type AliasTarget = {
    DNSName               : string
    EvaluateTargetHealth  : bool
    HostedZoneId          : string
  }

  type ResourceRecordSet = {
    Name            : string
    ResourceRecords : List<string>
    TTL             : Option<int32>
    Type            : ResourceRecordType
    AliasTarget     : Option<AliasTarget>
  }