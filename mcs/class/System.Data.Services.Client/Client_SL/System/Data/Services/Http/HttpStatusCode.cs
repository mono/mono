//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Http
{
    internal enum HttpStatusCodeRange : int
    {
        MaxValue = 599,

        MinValue = 100
    }

    internal enum HttpStatusCode
    {
        Accepted = 202,

        Ambiguous = 300,

        BadGateway = 502,

        BadRequest = 400,

        Conflict = 409,

        Continue = 100,

        Created = 201,

        ExpectationFailed = 417,

        Forbidden = 403,

        Found = 302,

        GatewayTimeout = 504,

        Gone = 410,

        HttpVersionNotSupported = 505,

        InternalServerError = 500,

        LengthRequired = 411,

        MethodNotAllowed = 405,

        Moved = 301,

        MovedPermanently = 301,

        MultipleChoices = 300,

        NoContent = 204,

        NonAuthoritativeInformation = 203,

        NotAcceptable = 406,

        NotFound = 404,

        NotImplemented = 501,

        NotModified = 304,

        OK = 200,

        PartialContent = 206,

        PaymentRequired = 402,

        PreconditionFailed = 412,

        ProxyAuthenticationRequired = 407,

        Redirect = 302,

        RedirectKeepVerb = 307,

        RedirectMethod = 303,

        RequestedRangeNotSatisfiable = 416,

        RequestEntityTooLarge = 413,

        RequestTimeout = 408,

        RequestUriTooLong = 414,

        ResetContent = 205,

        SeeOther = 303,

        ServiceUnavailable = 503,

        SwitchingProtocols = 101,

        TemporaryRedirect = 307,

        Unauthorized = 401,

        UnsupportedMediaType = 415,

        Unused = 306,

        UseProxy = 305
    }
}
