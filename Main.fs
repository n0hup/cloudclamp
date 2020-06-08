namespace CloudClamp

// external
open System
open System.Reflection

// internal
open Config
open Cli
open Stage
open Command
open Logging

module Main =

  let loggerMain = Logger.CreateLogger "Main" loggingConfig.LogLevel

  let callModuleFunction (moduleName:string) (functionName:string) (command:string) (stage:string) =
    loggerMain.LogDebug(sprintf "{moduleName.functionName: %s.%s, command: %s, stage: %s}" moduleName functionName command stage)
    let asm = Assembly.GetExecutingAssembly()
    for t in asm.GetTypes() do
      for m in t.GetMethods() do
        if t.FullName = moduleName && m.IsStatic && m.Name = functionName then
          loggerMain.LogInfo(sprintf "Invoking: %A.%A" t.FullName m.Name)
          m.Invoke(null, [|command;stage|]) |> ignore

  [<EntryPoint>]
  let main argv =

    loggerMain.LogInfo("Main is starting")

    let commandLineArgumentsParsed = parseCommandLine (Array.toList argv)

    let stage = fromStageToString commandLineArgumentsParsed.Stage
    let command = fromCommandToMethodName commandLineArgumentsParsed.Command

    if commandLineArgumentsParsed.Stack = "noop" then
      callModuleFunction "CloudClamp.Blog" "executeCommand" "showStack" "prod"
    else
      loggerMain.LogInfo(sprintf "%s %s %s %s" commandLineArgumentsParsed.Stack "executeCommand" command stage)
      callModuleFunction commandLineArgumentsParsed.Stack "executeCommand" command stage

    //return
    0