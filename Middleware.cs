using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
//todo: replace code somewhere else
public class ErrorHandler
{
    public bool IsRequestFailed { get { return m_result != null; } }
    public HttpResult result { get { return m_result; } }
    public void SetErrorData(HttpResult result) { m_result = result; }
    public void SetErrorData(int code, string message) { m_result = new HttpResult(code, message); }

    private HttpResult m_result;
}

class ErrorHandlerMiddleware
{
    private RequestDelegate m_next;
    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        m_next = next;
    }

    public async Task Invoke(HttpContext context, ErrorHandler errorHandler)
    {
        await m_next.Invoke(context);

        if (errorHandler.IsRequestFailed)
        {
            context.Response.StatusCode = errorHandler.result.code;
            context.Response.ContentLength = errorHandler.result.message.Length;
            context.Response.WriteAsync(errorHandler.result.message);
        }
    }
}