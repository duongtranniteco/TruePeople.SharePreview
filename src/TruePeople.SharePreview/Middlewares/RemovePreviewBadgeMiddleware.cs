using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Extensions;

namespace TruePeople.SharePreview.Middlewares
{
    internal class RemovePreviewBadgeMiddleware
    {
        private readonly RequestDelegate _next;

        public RemovePreviewBadgeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Value.StartsWith("/umbraco/sharepreview/"))
            {
                await _next(context);
                return;
            }

            // Set the body to our stream
            var originalBody = context.Response.Body;
            var newBody = new MemoryStream();
            context.Response.Body = newBody;

            // Execute other middlewares so we have the full output
            await _next(context);

            // Reset to 0
            newBody.Position = 0;

            // Read all content and replace
            var content = await new StreamReader(newBody).ReadToEndAsync();
            var regex = @"(?s)<div[^>]*id=""umbracoPreviewBadge"".*<\/div>";
            content = Regex.Replace(content, regex, "");
            var updatedStream = GenerateStreamFromString(content);
            await updatedStream.CopyToAsync(originalBody);

            context.Response.Body = originalBody;

            //In New Umbraco, somehow it reponses with a 400 even if there is a location header
            //So if there is a location header, we need to set the status code to 302
            if (!context.Response.Headers["location"].ToString().IsNullOrWhiteSpace())
            {
                context.Response.StatusCode = 302;
            }
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}