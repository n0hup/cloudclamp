namespace CloudClamp

// external
open System
open System.IO

// internal

module Logging =


  type LogLevel = Info | Debug

  type Logger(name, logLevel) = 

    let logLevelActual logLevel =
      match logLevel with
      | "info"  -> Info
      | "debug" -> Debug
      | _       -> failwith "Not supported loglevel"

    let currentTime (tw:TextWriter) = 
      tw.Write("{0:s}",DateTime.Now)

    let logEvent level msg = 
      printfn "%t %s [%s] %s" currentTime level name msg

    member this.LogInfo msg = 
      logEvent "INFO" msg

    member this.LogError msg = 
      logEvent "ERROR" msg

    member this.LogDebug msg =
      if logLevelActual logLevel = Debug then
        logEvent "DEBUG" msg

    static member CreateLogger name logLevel= 
      Logger(name, logLevel)