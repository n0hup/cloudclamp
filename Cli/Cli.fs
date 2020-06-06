namespace CloudClamp

// external
open System
open System.Text.RegularExpressions

// internal
open Stage
open Command
open Config
open Logging

module Cli =

  let loggerCli = Logger.CreateLogger "Cli" loggingConfig.LogLevel

  let isValidStackName s =
    Regex(@"^[a-zA-Z0-9-_\.]+$").Match(s).Success

  // --stage qa --service hadoop --command deploy
  
  [<StructuredFormatDisplay("Stage {Stage} :: Command {Command} :: DryRun {DryRun} :: Stack {Stack}")>]
  type CommandLineOptions = {
    Stage   : Stage;
    Command : Command;
    DryRun  : Boolean;
    Stack   : string;
  }
  
  
  // create the "helper" recursive function
  let rec parseCommandLineRec args optionsSoFar =       
      //loggerCli.LogInfo(args)
      match args with 
      // empty list means we're done.
      | [] ->
          loggerCli.LogInfo(sprintf "optionsSoFar %A" optionsSoFar)
          optionsSoFar  

      // match stage flag
      | "--stage"::xs -> 
        match xs with
          | stageString::xss ->
            match fromStringToStage stageString with
              | Ok stage -> 
                  parseCommandLineRec xss { optionsSoFar with Stage=stage}
              | Error err -> 
                  loggerCli.LogError(String.Format("Unsupported stage: {0}", stageString))
                  Environment.Exit 1 
                  parseCommandLineRec xss optionsSoFar // never reach
          | [] ->
            loggerCli.LogError(String.Format("Stage cannot be empty"))
            Environment.Exit 1
            parseCommandLineRec xs optionsSoFar // never reach

      // match command flag
      | "--command"::xs -> 
        match xs with
          | commandString::xss ->
            match fromStringToCommand commandString with
              | Ok command -> 
                  parseCommandLineRec xss { optionsSoFar with Command=command}
              | Error _err ->
                  loggerCli.LogError(String.Format("Unsupported command: {0}", commandString))
                  Environment.Exit 1 
                  parseCommandLineRec xss optionsSoFar // never reach
          | [] ->
            loggerCli.LogError(String.Format("Command cannot be empty"))
            Environment.Exit 1
            parseCommandLineRec xs optionsSoFar // never reach

      // match command flag
      | "--dry-run"::xs -> 
        match xs with
          | dryRunString::xss ->
            match dryRunString with
              | "yes" -> 
                  parseCommandLineRec xss { optionsSoFar with DryRun=true}
              | "no" -> 
                  parseCommandLineRec xss { optionsSoFar with DryRun=false}
              | _ ->
                  loggerCli.LogError(String.Format("Unsupported dry-run: {0}", dryRunString))
                  Environment.Exit 1 
                  parseCommandLineRec xss optionsSoFar // never reach
          | [] ->
            loggerCli.LogError(String.Format("DryRun cannot be empty"))
            Environment.Exit 1
            parseCommandLineRec xs optionsSoFar // never reach

      // match stack flag - this must be in last position, free text
      | "--stack"::xs -> 
        match xs with
          | stackString::xss ->
            match isValidStackName stackString with
              | true -> 
                  parseCommandLineRec xss { optionsSoFar with Stack=stackString}
              | false -> 
                  loggerCli.LogError(String.Format("Unsupported stack name: {0}", stackString))
                  Environment.Exit 1 
                  parseCommandLineRec xss optionsSoFar // never reach
          | [] ->
            loggerCli.LogError(String.Format("Stack cannot be empty"))
            Environment.Exit 1
            parseCommandLineRec xs optionsSoFar // never reach
      // handle unrecognized option and keep looping
      | x::xs -> 
          loggerCli.LogError(String.Format("Option {0} is unrecognized", x))
          parseCommandLineRec xs optionsSoFar 

  // create the "public" parse function
  let parseCommandLine args = 
    // create the defaults
    let defaultOptions = {
      Stage = Dev;
      Command = ShowStack;
      DryRun = true;
      Stack = "noop"
    }
    // call the recursive one with the initial options
    parseCommandLineRec args defaultOptions


// END