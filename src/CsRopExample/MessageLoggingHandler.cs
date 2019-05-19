using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CsRopExample
{
    // /// <summary>
    // /// Logging code 
    // /// </summary>
    // public class MessageLoggingHandler : MessageProcessingHandler
    // {

    //     protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    //     {
    //         var correlationId = string.Format("{0}{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);
    //         var requestInfo = string.Format("{0} {1}", request.Method, request.RequestUri);
    //         var message = request.Content.ReadAsStringAsync().Result;
    //         Debug.WriteLine("[HTTP]Request: {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, message);
    //         return request;
    //     }

    //     protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    //     {
    //         var correlationId = string.Format("{0}{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);
    //         var requestInfo = string.Format("{0} {1}", response.RequestMessage.Method, response.RequestMessage.RequestUri);
    //         var message = response.Content != null 
    //             ? response.Content.ReadAsStringAsync().Result 
    //             : "[no body]";
    //         Debug.WriteLine("[HTTP]Response: {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, message);
    //         return response;
    //     }
    // }
    // https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
    // https://devblogs.microsoft.com/aspnet/re-reading-asp-net-core-request-bodies-with-enablebuffering/
    public class LogginMiddleware
    {
        private readonly RequestDelegate _next;

        public LogginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            if (!response.Body.CanRead || !response.Body.CanSeek)
            {
                return "Resonpse cannot be read";
            }
            //We need to read the response stream from the beginning...
            response.Body.Seek(0, SeekOrigin.Begin);

            //...and copy it into a string
            var sr = new StreamReader(response.Body);
            {
                string text = await sr.ReadToEndAsync();

                //We need to reset the reader for the response so that the client can read it.
                response.Body.Seek(0, SeekOrigin.Begin);

                //Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
                return $"{response.StatusCode}: {text}";
            }
            
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            request.EnableBuffering();
            var correlationId = string.Format("{0}{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);
            var requestInfo = string.Format("{0} {1}", request.Method, request.Path);

            
            {
                //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];

                //...Then we copy the entire request stream into the new buffer.
                await request.Body.ReadAsync(buffer, 0, buffer.Length);

                //We convert the byte[] into a string using UTF8 encoding...
                var message = System.Text.Encoding.UTF8.GetString(buffer);
                // Reset the request body stream position so the next middleware can read it
                request.Body.Position = 0;
                
                Console.WriteLine("[HTTP]Request: {0}, {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, message);
            }
            //Copy a pointer to the original response body stream
            var originalBodyStream = context.Response.Body;
            //Create a new memory stream...
            using (var responseBody = new MemoryStream())
            {
                //...and use that for the temporary response body
                context.Response.Body = responseBody;

                //Continue down the Middleware pipeline, eventually returning to this class
                await _next(context);

                //Format the response from the server
                var response = context.Response;

                //We need to read the response stream from the beginning...
                response.Body.Seek(0, SeekOrigin.Begin);

                //...and copy it into a string
                using(var sr = new StreamReader(response.Body))
                {
                    string text = await sr.ReadToEndAsync();

                    //We need to reset the reader for the response so that the client can read it.
                    response.Body.Seek(0, SeekOrigin.Begin);

                    //Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
                    var message = $"{response.StatusCode}: {text}";
                    //TODO: Save log to chosen datastore
                    Console.WriteLine("[HTTP]Response: {0}, {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, message);

                    //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                    await responseBody.CopyToAsync(originalBodyStream);
                }
                
            }
        }
    }
}