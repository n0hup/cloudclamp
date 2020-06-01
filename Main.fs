namespace CloudClamp

// external
open System
open System.Reflection

// internal
open Cli
open Stage
open Command
open Logging

module Main =

  let loggerMain = Logger.CreateLogger("Main")

  let callModuleFunction (moduleName:string) (functionName:string) (stage:string) = 
    let asm = Assembly.GetExecutingAssembly()
    for t in asm.GetTypes() do
      for m in t.GetMethods() do
        if t.FullName = moduleName && m.IsStatic && m.Name = functionName then 
          loggerMain.LogInfo(String.Format("Invoking: {0}.{1}", t.FullName, m.Name))
          m.Invoke(null, [|stage|]) |> ignore

  [<EntryPoint>]
  let main argv =

    loggerMain.LogInfo("Main is starting")

    let commandLineArgumentsParsed = parseCommandLine (Array.toList argv)
       
    let stage = fromStageToString commandLineArgumentsParsed.Stage
    let command = fromCommandToString commandLineArgumentsParsed.Command

    if commandLineArgumentsParsed.Service = "noop" then
      callModuleFunction "CloudClamp.Website" "show" "prod"
    else
      callModuleFunction commandLineArgumentsParsed.Service command stage 

    //return
    0