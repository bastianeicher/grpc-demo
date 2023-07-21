using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Net.Http.Headers;

namespace Identity.Service;

/// <summary>
/// Returns valid gRPC responses for authorization failures.
/// </summary>
public class GrpcAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler {
    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<GrpcMethodMetadata>() != null)
        {
            void SetStatusCode(StatusCode statusCode)
            {
                context.Response.Headers.ContentType = "application/grpc+proto";
                context.Response.Headers[HeaderNames.GrpcStatus] = statusCode.ToString("D");
            }

            if (authorizeResult.Challenged)
                SetStatusCode(StatusCode.Unauthenticated);
            else if (authorizeResult.Forbidden)
                SetStatusCode(StatusCode.PermissionDenied);
        }

        return Task.CompletedTask;
    }
}
