namespace CloudClamp

// external
open System.Text.Json
open Fleece.SystemTextJson
open Fleece.SystemTextJson.Operators
open FSharpPlus
// internal

module AwsRoute53Resource =

  // type ResourceRecordType =
  //   A | AAAA | CNAME | DNSKEY | MX | NS | PTR | RRSIG | SOA | TXT

  type AliasTarget = {
    DNSName               : string
    EvaluateTargetHealth  : bool
    HostedZoneId          : string
  }
  with
    static member JsonObjCodec =
      fun d e h -> { DNSName = d; EvaluateTargetHealth = e; HostedZoneId = h }
      |> withFields
      |> jfield    "DNSName"                (fun d -> d.DNSName)
      |> jfield    "EvaluateTargetHealth"   (fun e -> e.EvaluateTargetHealth)
      |> jfield    "HostedZoneId"           (fun h -> h.HostedZoneId)

  type ResourceRecord =
    { Value : string }
    with
      static member JsonObjCodec =
        fun r -> { Value = r }
        |> withFields
        |> jfield "Value" (fun r -> r.Value)

  type ResourceRecords =
    List<ResourceRecord>

  type ResourceRecordsOrAlias =
    | AliasTarget of
        AliasTarget
    | ResourceRecords of
        ResourceRecords
    with
      static member JsonObjCodec =
        (
          ( AliasTarget     <!> jreq "AliasTarget"      (function AliasTarget a     -> Some a | _ -> None) )
        <|>
          ( ResourceRecords <!> jreq "ResourceRecords"  (function ResourceRecords r -> Some r | _ -> None) )
        )

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

  type Color = Red | Blue | White

  type Car = {
      Id    : string
      Color : Color
      Kms   : int }

  let colorDecoder = function
      | JString "red"   -> Decode.Success Red
      | JString "blue"  -> Decode.Success Blue
      | JString "white" -> Decode.Success White
      | JString  x as v -> Decode.Fail.invalidValue v ("Wrong color: " + x)
      | x               -> Decode.Fail.strExpected  x

  let colorEncoder = function
      | Red   -> JString "red"
      | Blue  -> JString "blue"
      | White -> JString "white"

  let colorCodec = colorDecoder, colorEncoder

  let [<GeneralizableValue>]carCodec<'t> =
      fun i c k -> { Id = i; Color = c; Kms = k }
      |> withFields
      |> jfieldWith JsonCodec.string "id"    (fun x -> x.Id)
      |> jfieldWith colorCodec       "color" (fun x -> x.Color)
      |> jfieldWith JsonCodec.int    "kms"   (fun x -> x.Kms)
      |> Codec.compose jsonObjToValueCodec

  //
  // Helpers
  //



  let validResourceRecordType s =
      match s with
        | "A" | "AAAA" | "CNAME" | "DNSKEY"
        | "MX" | "NS"  | "PTR" | "RRSIG"
        | "SOA" | "TXT" ->
          Ok s
        | _ ->
          Error "not supported ResouceRecord Type"