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
    static member ToJson (a: AliasTarget) =
        jobj [
            "DNSName"               .= a.DNSName
            "EvaluateTargetHealth"  .= a.EvaluateTargetHealth
            "HostedZoneId"          .= a.HostedZoneId
        ]
    static member OfJson json =
        match json with
        | JObject o ->
            monad {
                let! dnsName              = o .@ "DNSName"
                let! evaluateTargetHealth = o .@ "EvaluateTargetHealth"
                let! hostedZoneId         = o .@ "HostedZoneId"
                return {
                    DNSName = dnsName
                    EvaluateTargetHealth = evaluateTargetHealth
                    HostedZoneId = hostedZoneId
                }
            }
        | x -> Decode.Fail.objExpected x

  let aliasTargetToJSON (a:AliasTarget) : string =
    sprintf "%s" (string (toJson a))

  type ResourceRecordType =
      A | AAAA | CNAME | DNSKEY | MX | NS | PTR | RRSIG | SOA | TXT

  let resourceRecordTypeDecoder s  =
    match s with
      | JString "A"       -> Decode.Success A
      | JString "AAAA"    -> Decode.Success AAAA
      | JString "CNAME"   -> Decode.Success CNAME
      | JString "DNSKEY"  -> Decode.Success DNSKEY
      | JString "MX"      -> Decode.Success MX
      | JString "NS"      -> Decode.Success NS
      | JString "PTR"     -> Decode.Success PTR
      | JString "RRSIG"   -> Decode.Success RRSIG
      | JString "SOA"     -> Decode.Success SOA
      | JString "TXT"     -> Decode.Success TXT
      | JString  x as v   -> Decode.Fail.invalidValue v ("Wrong ResourceRecordType: " + x)
      | x                 -> Decode.Fail.strExpected  x

  let resourceRecordTypeEncoder r =
    match r with
      | A       -> JString "A"
      | AAAA    -> JString "AAAA"
      | CNAME   -> JString "CNAME"
      | DNSKEY  -> JString "DNSKEY"
      | MX      -> JString "MX"
      | NS      -> JString "NS"
      | PTR     -> JString "PTR"
      | RRSIG   -> JString "RRSIG"
      | SOA     -> JString "SOA"
      | TXT     -> JString "TXT"

  let colorCodec = resourceRecordTypeDecoder, resourceRecordTypeEncoder

  type ResourceRecordSet =
    | Alias of
        Name : string *
        Type : ResourceRecordType *
        AliasTarget : AliasTarget
    | Record of
        Name : string *
        Type : ResourceRecordType *
        ResourceRecords : List<string> *
        TTL : uint32

  type DnsResource = {
    Name               : string
    HostedZoneId       : Option<string>
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