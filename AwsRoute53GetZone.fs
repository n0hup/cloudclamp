namespace CloudClamp

open Amazon.Route53
open Amazon.Route53.Model
open System

open HttpStatus

module AwsRoute53GetZone =

  let getHostedZone (amazonRoute53client:AmazonRoute53Client) (id:string) =
    try
      let getHostedZoneRequest = 
        GetHostedZoneRequest(
          Id = id
        )
      let task = amazonRoute53client.GetHostedZoneAsync(getHostedZoneRequest)
      task.Wait()
      if task.IsCompletedSuccessfully && isHttpSuccess task.Result.HttpStatusCode then
        Some task.Result.HostedZone     
      else
        None 
    with ex ->
      Console.Error.WriteLine("{0} : {1}", ex.Message, ex.InnerException.Message)
      None
