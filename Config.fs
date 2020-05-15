namespace CloudClamp

open System.IO
open FSharp.Data

module Config =

  type JsonConfig =
    JsonProvider<"./config.prod.json">

  let config =
    JsonConfig.Parse(File.ReadAllText("./config.prod.json"))