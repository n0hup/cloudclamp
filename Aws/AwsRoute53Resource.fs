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
    with
      static member JsonObjCodec =
        fun d e h -> { DNSName = d; EvaluateTargetHealth = e; HostedZoneId = h }
        |> withFields
        |> jfield    "DNSName"                (fun d -> d.DNSName)
        |> jfield    "EvaluateTargetHealth"   (fun e -> e.EvaluateTargetHealth)
        |> jfield    "HostedZoneId"           (fun h -> h.HostedZoneId)

  type ResourceRecord =
    {
      Value : string
    }
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

  let jfrom c f =
    Codec.compose (Codec.ofConcrete c) (Ok, f) |> Codec.toConcrete

  type ResourceRecordSet =
    {
      Name            : string
      RecordsOrAlias  : ResourceRecordsOrAlias
      TTL             : Option<uint32>
      Type            : string
    }
    static member JsonObjCodec =
      fun n r tt ty -> { Name = n; RecordsOrAlias = r; TTL =tt; Type = ty}
      <!> jreq "Name"                               (fun n  -> Some n.Name)
      <*> jfrom ResourceRecordsOrAlias.JsonObjCodec (fun r  -> r.RecordsOrAlias)
      <*> jopt "TTL"                                (fun tt -> tt.TTL)
      <*> jreq "Type"                               (fun ty -> Some ty.Type)

  type DnsResource =
    {
      Name               : string
      HostedZoneId       : Option<string>
      ResourceRecordSets : List<ResourceRecordSet>
    }
