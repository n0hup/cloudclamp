namespace CloudClamp

// external
open ZeroLog
open ZeroLog.Appenders
open ZeroLog.Config
open System.Collections.Generic
open System
open System.IO

// internal

module Logging =

  type Logger(name) = 
    
    let currentTime (tw:TextWriter) = 
        tw.Write("{0:s}",DateTime.Now)

    let logEvent level msg = 
        printfn "%t %s [%s] %s" currentTime level name msg

    member this.LogInfo msg = 
        logEvent "INFO" msg

    member this.LogError msg = 
        logEvent "ERROR" msg

    static member CreateLogger name = 
      Logger(name)