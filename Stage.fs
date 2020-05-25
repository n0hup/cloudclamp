namespace CloudClamp

// external

// internal

module Stage =    

  type Stage = Dev | Test | Qa | PreProd | Prod 

  let fromStringToStage (s:string) : Result<Stage,string> =
    match s with
      | "dev"     -> Ok Dev
      | "test"    -> Ok Test
      | "qa"      -> Ok Qa
      | "preprod" -> Ok PreProd
      | "prod"    -> Ok Prod
      | _         -> Error "Unsupported stage"

  let fromStageToString (s:Stage) : string =
    match s with
      | Dev     -> "dev"
      | Test    -> "test"
      | Qa      -> "qa"
      | PreProd -> "preprod"
      | Prod    -> "prod"