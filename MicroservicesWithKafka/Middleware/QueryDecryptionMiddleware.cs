using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using MicroservicesWithKafka.Services;

namespace MicroservicesWithKafka.Middleware
{
    public class QueryDecryptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEncryptionService _encryptionService;
        private readonly string _encryptedFieldParam = "field";

        public QueryDecryptionMiddleware(RequestDelegate next, IEncryptionService encryptionService)
        {
            _next = next;
            _encryptionService = encryptionService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request has an encrypted field parameter
            if (context.Request.Query.TryGetValue(_encryptedFieldParam, out StringValues encryptedFieldValue))
            {
                try
                {
                    string decryptedField = _encryptionService.Decrypt(encryptedFieldValue.ToString());

                    var queryCollection = new Dictionary<string, StringValues>(context.Request.Query);
                    queryCollection.Remove(_encryptedFieldParam);
                    queryCollection.Add("field", new StringValues(decryptedField));

                    // Replace the request's query collection with our modified one
                    var requestFeature = context.Features.Get<IHttpRequestFeature>();
                    requestFeature.QueryString = QueryString.Create(queryCollection).ToString();

                    Serilog.Log.Information($"Decrypted field parameter: {decryptedField}");
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"Failed to decrypt field parameter: {ex.Message}");
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid encrypted field parameter");
                    return;
                }
            }

            await _next(context);
        }
    }

    // Extension method to make it easier to add the middleware to the pipeline
    public static class QueryDecryptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseQueryDecryption(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<QueryDecryptionMiddleware>();
        }
    }
}