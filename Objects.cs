﻿
namespace AuthService
{
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
        public string Value { get; set; }

        public static Role[] GetRolesList() { return new Role[] { User, Dev, Admin }; }
        public static Role GerRoleFromString(string role)
        {
            switch (role)
            {
                case "user": return Role.User;
                case "dev": return Role.Dev;
                case "admin": return Role.Admin;
                default: return null;
            }
        }

        public static Role User { get { return new Role("user"); } }
        public static Role Dev { get { return new Role("dev"); } }
        public static Role Admin { get { return new Role("admin"); } }
    }

    public class CustomClaimTypes
    {
        private CustomClaimTypes(string value) { Value = value; }
        public string Value { get; }

        public static CustomClaimTypes Name { get { return new CustomClaimTypes("name"); } }
        public static CustomClaimTypes Role { get { return new CustomClaimTypes("role"); } }
        public static CustomClaimTypes UserId { get { return new CustomClaimTypes("userId"); } }
    }
}