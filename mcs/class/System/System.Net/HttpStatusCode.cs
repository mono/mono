// HttpStatusCode.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:32:05 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


namespace System.Net {


	/// <summary>
	/// </summary>
	public enum HttpStatusCode {

		/// <summary>
		/// </summary>
		Continue = 100,

		/// <summary>
		/// </summary>
		SwitchingProtocols = 101,

		/// <summary>
		/// </summary>
		OK = 200,

		/// <summary>
		/// </summary>
		Created = 201,

		/// <summary>
		/// </summary>
		Accepted = 202,

		/// <summary>
		/// </summary>
		NonAuthoritativeInformation = 203,

		/// <summary>
		/// </summary>
		NoContent = 204,

		/// <summary>
		/// </summary>
		ResetContent = 205,

		/// <summary>
		/// </summary>
		PartialContent = 206,

		/// <summary>
		/// </summary>
		MultipleChoices = 300,

		/// <summary>
		/// </summary>
		Ambiguous = 300,

		/// <summary>
		/// </summary>
		MovedPermanently = 301,

		/// <summary>
		/// </summary>
		Moved = 301,

		/// <summary>
		/// </summary>
		Found = 302,

		/// <summary>
		/// </summary>
		Redirect = 302,

		/// <summary>
		/// </summary>
		SeeOther = 303,

		/// <summary>
		/// </summary>
		RedirectMethod = 303,

		/// <summary>
		/// </summary>
		NotModified = 304,

		/// <summary>
		/// </summary>
		UseProxy = 305,

		/// <summary>
		/// </summary>
		Unused = 306,

		/// <summary>
		/// </summary>
		TemporaryRedirect = 307,

		/// <summary>
		/// </summary>
		RedirectKeepVerb = 307,

		/// <summary>
		/// </summary>
		BadRequest = 400,

		/// <summary>
		/// </summary>
		Unauthorized = 401,

		/// <summary>
		/// </summary>
		PaymentRequired = 402,

		/// <summary>
		/// </summary>
		Forbidden = 403,

		/// <summary>
		/// </summary>
		NotFound = 404,

		/// <summary>
		/// </summary>
		MethodNotAllowed = 405,

		/// <summary>
		/// </summary>
		NotAcceptable = 406,

		/// <summary>
		/// </summary>
		ProxyAuthenticationRequired = 407,

		/// <summary>
		/// </summary>
		RequestTimeout = 408,

		/// <summary>
		/// </summary>
		Conflict = 409,

		/// <summary>
		/// </summary>
		Gone = 410,

		/// <summary>
		/// </summary>
		LengthRequired = 411,

		/// <summary>
		/// </summary>
		PreconditionFailed = 412,

		/// <summary>
		/// </summary>
		RequestEntityTooLarge = 413,

		/// <summary>
		/// </summary>
		RequestUriTooLong = 414,

		/// <summary>
		/// </summary>
		UnsupportedMediaType = 415,

		/// <summary>
		/// </summary>
		RequestedRangeNotSatisfiable = 416,

		/// <summary>
		/// </summary>
		ExpectationFailed = 417,

		/// <summary>
		/// </summary>
		InternalServerError = 500,

		/// <summary>
		/// </summary>
		NotImplemented = 501,

		/// <summary>
		/// </summary>
		BadGateway = 502,

		/// <summary>
		/// </summary>
		ServiceUnavailable = 503,

		/// <summary>
		/// </summary>
		GatewayTimeout = 504,

		/// <summary>
		/// </summary>
		HttpVersionNotSupported = 505,
	} // HttpStatusCode

} // System.Net
