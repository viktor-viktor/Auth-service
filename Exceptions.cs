using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web;

namespace CSharp
{
    class HttpBadRequestException: HttpException
    {
        public HttpBadRequestException(string message) : base(400, message) { }
    }

    class HttpUnAuthorizedException : HttpException
    {
        public HttpUnAuthorizedException(string message) : base(401, message) { }
    }
    class HttpForbiddenException : HttpException
    {
        public HttpForbiddenException(string message) : base(403, message) { }
    }
    class HttpNotFoundException : HttpException
    {
        public HttpNotFoundException(string message) : base(404, message) { }
    }
    class HttpMethodNotAllowedException : HttpException
    {
        public HttpMethodNotAllowedException(string message) : base(405, message) { }
    }
    class HttpInternalErrorException : HttpException
    {
        public HttpInternalErrorException(string message) : base(500, message) { }
    }
}
