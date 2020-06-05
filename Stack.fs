namespace CloudClamp

module Stack =

  // type ResourceMap = 
  //   Map<string, List<obj>>

  // type StateDiff = 
  //   Map<string, List<obj>>

  type IStack =
    abstract member ShowStack       : string -> unit // Gets of all the resources that are managed in the module    
    abstract member RefreshState    : string -> unit
    abstract member DeployStack     : string -> unit
    abstract member DestroyStack    : string -> unit
    abstract member ImportResource  : string -> unit
