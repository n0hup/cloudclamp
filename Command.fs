namespace CloudClamp

// external

// internal

module Command =    

  type Command = ShowStack | RefreshState | DeployStack | DestroyStack | ImportResource

  let fromStringToCommand (s:string) : Result<Command,string> =
    match s with
      | "show-stack"        -> Ok ShowStack
      | "refresh-state"     -> Ok RefreshState
      | "deploy-stack"      -> Ok DeployStack
      | "destroy-stack"     -> Ok DestroyStack
      | "import-resource"   -> Ok ImportResource
      | _                   -> Error "Unsupported command"

  let fromCommandToMethodName (s:Command) : string =
    match s with
      | ShowStack       -> "showStack"
      | RefreshState    -> "refreshState"
      | DeployStack     -> "deployStack"
      | DestroyStack    -> "destroyStack"
      | ImportResource  -> "importResource"
