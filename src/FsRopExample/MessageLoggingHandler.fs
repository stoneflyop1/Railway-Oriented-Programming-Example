namespace FsRopExample

//open System
//open System.Diagnostics
//open System.Net.Http
//open System.Threading

///// Logging code 
//type MessageLoggingHandler() =
    //inherit MessageProcessingHandler()

    //override this.ProcessRequest(request:HttpRequestMessage , _:CancellationToken) =
    //    let correlationId = sprintf "%i%i" DateTime.Now.Ticks Thread.CurrentThread.ManagedThreadId
    //    let requestInfo = sprintf "%O %O" request.Method  request.RequestUri
    //    let message = request.Content.ReadAsStringAsync().Result
    //    Debug.WriteLine("[HTTP]Request: {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, message)
    //    request

    //override this.ProcessResponse(response:HttpResponseMessage, _:CancellationToken) =
        //let correlationId = sprintf "%i%i" DateTime.Now.Ticks Thread.CurrentThread.ManagedThreadId
        //let requestInfo = sprintf "%O %O" response.RequestMessage.Method  response.RequestMessage.RequestUri
        //let message = if response.Content <> null then response.Content.ReadAsStringAsync().Result else "[no body]"
        //Debug.WriteLine("[HTTP]Response: {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, message)
        //response

open System
open System.Threading
open Microsoft.AspNetCore.Http
open System.IO

type LoggingMiddleware(next:RequestDelegate) = 
    member this.Invoke (context: HttpContext) = 
        context.Request.EnableBuffering()
        let correlationId = sprintf "%i%i" DateTime.Now.Ticks Thread.CurrentThread.ManagedThreadId
        let requestInfo = sprintf "%O %O" context.Request.Method  context.Request.Path
        let len = 
            if context.Request.ContentLength.HasValue then (int context.Request.ContentLength.Value)
            else 0

        let buffer: byte array = Array.zeroCreate len

        context.Request.Body.Read(buffer, 0, buffer.Length) |> ignore

        let message = System.Text.Encoding.UTF8.GetString(buffer)

        context.Request.Body.Seek(0L, SeekOrigin.Begin) |> ignore

        System.Console.WriteLine("[HTTP]Request: {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, message)

        let originalBodyStream = context.Response.Body

        use responseBody = new MemoryStream ()
        context.Response.Body <- responseBody

        let task = next.Invoke(context)

        context.Response.Body.Seek(0L, SeekOrigin.Begin)|> ignore

        use sr = new StreamReader(context.Response.Body)
        let text = sr.ReadToEnd()
        context.Response.Body.Seek(0L, SeekOrigin.Begin) |> ignore
        let msg = sprintf "%i %O" context.Response.StatusCode  text

        System.Console.WriteLine("[HTTP]Response: {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, msg)

        responseBody.CopyTo(originalBodyStream)

        task