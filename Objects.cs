
public class HttpResult
{
    public HttpResult() { code = 200; message = ""; }
    public HttpResult(int _code, string _message = "") { message = _message; code = _code; }

    public int code { get; } 
    public string message { get; }
}
