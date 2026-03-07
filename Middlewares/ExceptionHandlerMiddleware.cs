using System.Net;

namespace backend_trial.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly ILogger<ExceptionHandlerMiddleware> logger;
        private readonly RequestDelegate next;

        public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger, RequestDelegate next)
        {
            this.logger = logger;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // execution of end points
                await next(httpContext);
            }
            catch(Exception ex)
            {
                // generating new id
                var errorId = Guid.NewGuid(); 

                // Logging the exception with time
                logger.LogError(ex, $"{errorId} : {ex.Message}" , DateTime.Now);

                // Createing a custom Error response
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "application/json";

                var error = new
                {
                    Id = errorId,
                    ErrorMessage = "Something went wrong! We are looking into resolving this. "
                };
                // Returning back the response
                await httpContext.Response.WriteAsJsonAsync(error);
            }
        }
    }
}
