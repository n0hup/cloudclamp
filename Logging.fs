namespace CloudClamp

// external
open ZeroLog
open ZeroLog.Appenders
open ZeroLog.Config
open System.Collections.Generic
// internal

module Logging =  

  let logManager =
    let appender = new ConsoleAppender()
    let defaultAppenderConfig = DefaultAppenderConfig()
    defaultAppenderConfig.PrefixPattern <- "[%level] @ %time - %logger :: "
    appender.Configure(defaultAppenderConfig)
    let appenders =  new List<IAppender>(capacity=1)
    appenders.Add(appender)
    BasicConfigurator.Configure(appenders)  
  
  let getLogger (name:string) =
    LogManager.GetLogger(name)

  let testLogging () =
    let logger = getLogger "Main"
    logger.Info("test")

    