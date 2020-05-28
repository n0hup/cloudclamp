namespace CloudClamp

// external
open System.Net

module HttpStatus =

  let httpSuccessCodes =
    Set.ofSeq [ HttpStatusCode.OK; HttpStatusCode.NoContent; HttpStatusCode.Created; HttpStatusCode.Accepted; ]

  let isHttpSuccess (httpSuccessCode:HttpStatusCode) =
    httpSuccessCodes.Contains httpSuccessCode