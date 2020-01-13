﻿
public class HttpResult
{
    public HttpResult() { code = 200; message = ""; }
    public HttpResult(int _code, string _message = "") { message = _message; code = _code; }

    public int code { get; } 
    public string message { get; }
}

public class Role
{
    private Role(string value) { Value = value; }
    public string Value { get; }

    public static Role User { get { return new Role("user"); } }
    public static Role Dev { get { return new Role("dev"); } }
    public static Role Admin { get { return new Role("admin"); } }
}
