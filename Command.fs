namespace CloudClamp

// external

// internal

module Command =    

  type Command = Plan | Deploy | Show

  let fromStringToCommand (s:string) : Result<Command,string> =
    match s with
      | "plan"     -> Ok Plan
      | "deploy"   -> Ok Deploy
      | "show"     -> Ok Show
      | _         -> Error "Unsupported command"

  let fromCommandToString (s:Command) : string =
    match s with
      | Plan      -> "plan"
      | Deploy    -> "deploy"
      | Show      -> "show"
