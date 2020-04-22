using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace cw_3.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();

            if (httpContext.Request != null)
            {
                var list = new List<string>();
                list.Add(httpContext.Request.Method.ToString());
                list.Add(httpContext.Request.Path);
                string bodyStr = "";
                using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    list.Add(bodyStr);
                    httpContext.Request.Body.Position = 0;
                }
                list.Add(httpContext.Request?.QueryString.ToString());

                var jsonString = JsonSerializer.Serialize(list);
                File.WriteAllText("requestsLog.txt", jsonString);
            }
            await _next(httpContext);
        }
    }
}
