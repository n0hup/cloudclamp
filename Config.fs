namespace CloudClamp

open System.IO
open FSharp.Data

module Config =

  type JsonConfig =
    JsonProvider<"./config.prod.json">

  let jsonConfig =
    JsonConfig.Parse(File.ReadAllText("./config.prod.json"))