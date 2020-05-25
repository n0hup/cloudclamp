namespace CloudClamp

// external
open System
open System.Text.RegularExpressions

// internal
open Stage
open Command

module Cli =    

  let isValidServiceName s =
    Regex(@"^[a-zA-Z0-9-_\.]+$").Match(s).Success

  // --stage qa --service hadoop --command deploy
  
  type CommandLineOptions = {
    Stage : Stage;
    Command : Command;
    Service : string;
  }
  
  // create the "helper" recursive function
  let rec parseCommandLineRec args optionsSoFar = 
      match args with 
      // empty list means we're done.
      | [] -> 
          optionsSoFar  

      // match stage flag
      | "--stage"::xs -> 
        match xs with
          | stageString::xss ->
            match fromStringToStage stageString with
              | Ok stage -> 
                  parseCommandLineRec xss { optionsSoFar with Stage=stage}
              | Error err -> 
                  Console.Error.WriteLine("Unsupported stage: {0}", stageString)
                  Environment.Exit 1 
                  parseCommandLineRec xss optionsSoFar // never reach
          | [] ->
            Console.Error.WriteLine("Stage cannot be empty")
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
                  Console.Error.WriteLine("Unsupported command: {0}", commandString)
                  Environment.Exit 1 
                  parseCommandLineRec xss optionsSoFar // never reach
          | [] ->
            Console.Error.WriteLine("Command cannot be empty")
            Environment.Exit 1
            parseCommandLineRec xs optionsSoFar // never reach

      // match service flag - this must be in last position, free text
      | "--service"::xs -> 
        match xs with
          | serviceString::xss ->
            match isValidServiceName serviceString with
              | true -> 
                  parseCommandLineRec xss { optionsSoFar with Service=serviceString}
              | false -> 
                  Console.Error.WriteLine("Unsupported service name: {0}", serviceString)
                  Environment.Exit 1 
                  parseCommandLineRec xss optionsSoFar // never reach
          | [] ->
            Console.Error.WriteLine("Service cannot be empty")
            Environment.Exit 1
            parseCommandLineRec xs optionsSoFar // never reach
      // handle unrecognized option and keep looping
      | x::xs -> 
          printfn "Option '%s' is unrecognized" x
          parseCommandLineRec xs optionsSoFar 

  // create the "public" parse function
  let parseCommandLine args = 
    // create the defaults
    let defaultOptions = {
      Stage = Dev;
      Command = Plan;
      Service = "noop"
    }
    // call the recursive one with the initial options
    parseCommandLineRec args defaultOptions


// END