namespace CloudClamp

open System
open System.IO
open FSharp.Data

module Config =

  type JsonConfig =
    JsonProvider<"./config.template.json">

  let jsonConfig (stage:string) =
    JsonConfig.Parse(File.ReadAllText(String.Format("./config.{0}.json", stage)))

  type LoggingConfig =
    JsonProvider<"./logging.json">

  let loggingConfig =
    LoggingConfig.Parse(File.ReadAllText(String.Format("./logging.json")))
  