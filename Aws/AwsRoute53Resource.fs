namespace CloudClamp

// external
open Thoth.Json.Net
// internal

module AwsRoute53Resource =

  type ResourceRecordType =
    A | AAAA | CNAME | DNSKEY | MX | NS | PTR | RRSIG | SOA | TXT
    with
      static member Decoder : Decoder<ResourceRecordType> =
        Decode.string
          |> Decode.andThen
            (function
              | "A"     -> Decode.succeed A
              | "AAAA"  -> Decode.succeed AAAA
              | invalid -> Decode.fail (sprintf "Failed to decode `%s`" invalid))
      static member Encoder (resourceRecordType : ResourceRecordType) =
        Encode.object
            [ "type", Encode.string (sprintf "%A" resourceRecordType) ]

  type AliasTarget = {
    DNSName               : string
    EvaluateTargetHealth  : bool
    HostedZoneId          : string
  }
  with
    static member Decoder : Decoder<AliasTarget> =
      Decode.object
          (fun get ->
              { DNSName               = get.Required.Field "DNSName"              Decode.string
                EvaluateTargetHealth  = get.Required.Field "EvaluateTargetHealth" Decode.bool
                HostedZoneId          = get.Required.Field "HostedZoneId"         Decode.string } )

  type ResourceRecord =
    { Value : string }


  type ResourceRecords =
    List<ResourceRecord>

  type ResourceRecordsOrAlias =
    | AliasTarget of
        AliasTarget
    | ResourceRecords of
        ResourceRecords

  type ResourceRecordSet =
    {
      Name            : string
      RecordsOrAlias  : ResourceRecordsOrAlias
      TTL             : Option<uint32>
      Type            : string
    }

  type DnsResource = {
    Name               : string
    HostedZoneId       : Option<string>
    ResourceRecordSets : List<ResourceRecordSet>
  }
