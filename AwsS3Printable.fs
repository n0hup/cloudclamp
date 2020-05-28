namespace CloudClamp

open Amazon.S3.Model
open System
open System.Collections.Generic

module AwsS3Printable =

  let getTagsString (tags:Option<List<Tag>>) =
    match tags with
      | Some t  -> "Some :: [" + String.Join("; ", (List.map (fun (x:Tag) -> x.Key + ":" + x.Value) (Seq.toList t))) +  "]"
      | None    -> "None"

  let getWebsiteConfigString (websiteConfig:Option<WebsiteConfiguration>)  =
    match websiteConfig with
      | Some w  -> 
          if (isNull websiteConfig.Value.RedirectAllRequestsTo) then
            "Some :: Index: " + w.IndexDocumentSuffix + " Error: " + w.ErrorDocument
          else
            "Some :: RedirectTo: " + w.RedirectAllRequestsTo.HostName 
      | None    -> "None"

  let getPolicyString (policy:Option<string>) =
    match policy with
      | Some p  -> "Some " + p
      | None    -> "None"