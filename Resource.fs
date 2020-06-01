namespace CloudClamp

open System

module Resource =

  type IResource<'T> =
    abstract member List    : string -> List<'T>
    abstract member Show    : string -> 'T
    abstract member Plan    : string -> 'T
    abstract member Deploy  : string -> 'T
    abstract member Destroy : string -> 'T
    abstract member Refresh : string -> 'T


    // update 

    // - plan
    // - deploy
    // - destroy

    // view

    // - show
    // - list

    // state (local)

    // - refresh
