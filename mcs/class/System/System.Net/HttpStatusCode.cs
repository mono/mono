// HttpStatusCode.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net {


	/// <summary>
	/// <para>Contains the values of status codes defined for the Hypertext Transfer Protocol (HTTP).</para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration is used by <see cref="!:System.Net.HTTPWebReponse" />.</para>
	/// <block subset="none" type="note">
	/// <para>The <see cref="T:System.Net.HttpStatusCode" /> enumeration contains the values of the status codes
	///    defined in IETF RFC 2068 - HTTP/1.1. </para>
	/// <para>The status of an HTTP request is contained in the <see cref="P:System.Net.HttpWebResponse.StatusCode" qualify="true" /> property. </para>
	/// </block>
	/// </remarks>
	/// <example>
	/// <para> The following example compares the status returned by a
	///    <see cref="T:System.Net.HttpWebResponse" /> with a <see cref="T:System.Net.HttpStatusCode" /> 
	///    value to determine the status of the response.</para>
	/// <code lang="C#">using System;
	/// using System.Net;
	/// 
	/// public class HttpStatusCodeExample {
	/// 
	///    public static void Main() {
	/// 
	///       HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create("http://www.contoso.com");
	///       httpReq.AllowAutoRedirect = false;
	/// 
	///       HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse();
	/// 
	///       if (httpRes.StatusCode==HttpStatusCode.Moved) 
	///       {
	///          // Code for moved resources goes here. 
	///       }
	///    }
	/// }
	/// </code>
	/// </example>
	public enum HttpStatusCode {

		/// <summary><para>Equivalent to HTTP status 100. Indicates that the client is allowed to continue with the request.</para><para><block subset="none" type="note">For a detailed description of HTTP status code 100, see
		///       Section 10.1.1 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		Continue = 100,

		/// <summary><para>
		///       Equivalent to HTTP status 101. Indicates that the server understands
		///       and will comply with the client's request to switch the protocol
		///       being used by the current connection to the protocols defined by the response's Upgrade header.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 101, see
		///       Section 10.1.2 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		SwitchingProtocols = 101,

		/// <summary><para> Equivalent to HTTP status 200. Indicates that the request
		///       succeeded. The method used by the request determines the information returned with the
		///       response according to the following table.</para><para><list type="termdef"><item><term>GET</term><description>Returns the entity that corresponds to the 
		///             requested resource.</description></item><item><term>HEAD</term><description>Returns the entity-header fields that 
		///             corresponds to the requested resource, but does not sent the
		///             message-body.</description></item><item><term>POST </term><description>Returns an entity that contains or describes 
		///             the result of the action.</description></item><item><term>TRACE</term><description>Returns an entity that contains the request 
		///             message received by the server. </description></item></list></para><para><block subset="none" type="note">For a detailed description of the HTTP status code 200, see
		///    Section 10.2.1 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		OK = 200,

		/// <summary><para> 
		///       Equivalent to HTTP status 201. Indicates
		///       that the request has been fulfilled, resulting in the creation of a new resource. The most specific URI for this resource is contained by
		///       the Location header field in the response.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 201, see
		///       Section 10.2.2 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		Created = 201,

		/// <summary><para> Equivalent
		///       to HTTP status 202. Indicates that the request has been
		///       accepted but not
		///       yet processed.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 202, see
		///       Section 10.2.3 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		Accepted = 202,

		/// <summary><para> 
		///       Equivalent to HTTP status 203. Indicates
		///       that a local or a third-party copy
		///       rather than the origin server provided the the metainformation returned in the entity-header.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 203, see
		///       Section 10.2.4 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		NonAuthoritativeInformation = 203,

		/// <summary><para>Equivalent to HTTP status 204. Indicates that the request has been fulfilled by
		///       the server and no entity-body need be returned.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 204, see
		///       Section 10.2.5 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		NoContent = 204,

		/// <summary><para> Equivalent to HTTP status 205. Indicates that
		///       the server has fulfilled the request and the document view that yielded the request should be reset
		///       by the user.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 205, see
		///       Section 10.2.6 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		ResetContent = 205,

		/// <summary><para>Equivalent to HTTP status 206. Indicates that the server has
		///       fulfilled a partial GET request for the resource. The request is required to have
		///       included a Range header field that indicates the desired range.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 206, see
		///       Section 10.2.7 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		PartialContent = 206,

		/// <summary><para> Equivalent
		///       to HTTP status 300. Indicates that multiple representations, each with a specific location,
		///       correspond to the requested resource. Agent-driven negotiation information is provided
		///       so that the request may be redirected by the user (or user agent) to the location of the preferred representation.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />,
		/// <see cref="F:System.Net.HttpStatusCode.MultipleChoices" /> will cause an exception to be thrown.</para><block subset="none" type="note"><para>The default action is to treat this status as a redirect and follow the
		///       contents of the Location header associated with this response.</para><para><see cref="F:System.Net.HttpStatusCode.MultipleChoices" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.Ambiguous" />.</para><para>For a detailed description of the HTTP status code 300, see Section 10.3.1 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		MultipleChoices = 300,

		/// <summary><para>Equivalent to HTTP status 300. Indicates that multiple 
		///       representations, each with a specific location, correspond to the requested
		///       resource. Agent-driven negotiation information is provided so that the
		///       request may be redirected by the user (or user agent) to the location of the
		///       preferred representation.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />,
		/// <see cref="F:System.Net.HttpStatusCode.Ambiguous" /> will cause an exception to be 
		///    thrown.</para><block subset="none" type="note"><para>The default action is to treat this status as a redirect
		///       and follow the contents of the Location header associated with this
		///       response.</para><para><see cref="F:System.Net.HttpStatusCode.Ambiguous" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.MultipleChoices" />.</para><para>For a detailed description of the HTTP status code 300, see Section 10.3.1 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		Ambiguous = 300,

		/// <summary><para>Equivalent to HTTP status 301. Indicates that a new, permanent URI has been assigned to the requested resource. All future references should use one of the returned URIs.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />, <see cref="F:System.Net.HttpStatusCode.MovedPermanently" /> will cause an exception to be
		///    thrown.</para><block subset="none" type="note"><para>The default action when this status is received is to follow the Location
		///       header associated with the response.</para><para><see cref="F:System.Net.HttpStatusCode.MovedPermanently" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.Moved" />.</para><para>For a detailed description of the HTTP status code 301, see Section 10.3.2 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		MovedPermanently = 301,

		/// <summary><para>Equivalent to HTTP status 301. Indicates that a new, permanent URI has been 
		///       assigned to the requested resource. All future references should use one of the
		///       returned URIs.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />,
		/// <see cref="F:System.Net.HttpStatusCode.Moved" /> will cause an exception to be 
		///    thrown.</para><block subset="none" type="note"><para>The default action when this status is received is to follow the Location
		///       header associated with the response. When the original request method was POST,
		///       the redirected request will use the GET method.</para><para><see cref="F:System.Net.HttpStatusCode.Moved" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.MovedPermanently" />.</para><para>For a detailed description of the HTTP status code 301, see Section 10.3.2 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		Moved = 301,

		/// <summary><para> Equivalent to HTTP status 302. Indicates
		///       that the requested resource is temporarily located on a different URI.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />,
		/// <see cref="F:System.Net.HttpStatusCode.Found" /> will cause an exception to be
		///    thrown.</para><block subset="none" type="note"><para><see cref="F:System.Net.HttpStatusCode.Found" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.Redirect" />.</para><para> The default action when this status is received is to
		///    follow the Location header associated with the response. When the original
		///    request method was POST, the redirected request will use the GET method.</para><para>For a detailed description of the HTTP status code 302, see Section 10.3.3 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		Found = 302,

		/// <summary><para>Equivalent to HTTP status 302. Indicates that the requested resource is 
		///       temporarily located on a different URI.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />,
		/// <see cref="F:System.Net.HttpStatusCode.Redirect" /> will cause an exception to be 
		///    thrown.</para><block subset="none" type="note"><para>The default action when this status is received is to follow the Location
		///       header associated with the response. When the original request method was POST,
		///       the redirected request will use the GET method.</para><para><see cref="F:System.Net.HttpStatusCode.Redirect" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.Found" />.</para><para>For a detailed description of the HTTP status code 302, see Section 10.3.3 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		Redirect = 302,

		/// <summary><para>Equivalent to HTTP status 303. Automatically redirects the client to the URI
		///       specified in the Location header as the result of a POST.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />,
		/// <see cref="F:System.Net.HttpStatusCode.SeeOther" /> will cause an exception to be 
		///    thrown.</para><block subset="none" type="note"><para>The request to the resource specified by the Location header will be made
		///       with a GET.</para><para><see cref="F:System.Net.HttpStatusCode.SeeOther" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.RedirectMethod" />.</para><para>For a detailed description of the HTTP status code 303, see Section 10.3.4 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		SeeOther = 303,

		/// <summary><para>Equivalent to HTTP status 303.
		///       Automatically
		///       redirects the client to the URI specified in the Location header as the result
		///       of a POST.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />,
		/// <see cref="F:System.Net.HttpStatusCode.RedirectMethod" /> will cause an exception to be thrown.</para><block subset="none" type="note"><para>The request to the resource specified by the Location header will be made
		///       with a GET.</para><para><see cref="F:System.Net.HttpStatusCode.RedirectMethod" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.SeeOther" />.</para><para>For a detailed description of the HTTP status code 303, see Section 10.3.4 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		RedirectMethod = 303,

		/// <summary><para>Equivalent to HTTP status 304. Indicates that the
		///       client has preformed a conditional GET request and access is allowed, but the document has not been modified.</para><para><block subset="none" type="note">For a detailed description ofthe HTTP status code 304, see Section 10.3.5
		///       of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		NotModified = 304,

		/// <summary><para>Equivalent to HTTP status 305. Indicates that the requested
		///       resource is required to be accessed through the proxy given by the Location header field.</para><para><block subset="none" type="note">For a detailed description of the HTTP status
		///       code 305, see Section 10.3.6 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		UseProxy = 305,

		/// <summary><para> Equivalent to HTTP status 306.</para><block subset="none" type="note"><para>This status code is not used in HTTP/1.1.</para><para>For a detailed description of the HTTP status code 306, see Section 10.3.7 of IETF RFC 2068 -
		///          HTTP/1.1.</para></block></summary>
		Unused = 306,

		/// <summary><para>Equivalent to HTTP status 307. Indicates that the requested resource is temporarily
		///       located under a different URI.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />,
		/// <see cref="F:System.Net.HttpStatusCode.TemporaryRedirect" /> will cause an exception to be 
		///    thrown.</para><block subset="none" type="note"><para>The default action when this status is received is to follow the Location
		///       header associated with the response. When the original request method was POST,
		///       the redirected request will also use the POST method.</para><para><see cref="F:System.Net.HttpStatusCode.TemporaryRedirect" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.RedirectKeepVerb" />.</para><para>For a detailed description of the HTTP status code 307, see Section 10.3.8 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		TemporaryRedirect = 307,

		/// <summary><para>Equivalent to HTTP status 307. Indicates that the requested resource is 
		///       temporarily located under a different URI.</para><para>If the <see cref="P:System.Net.HttpWebRequest.AllowAutoRedirect" qualify="true" /> property is <see langword="false" />,
		/// <see cref="F:System.Net.HttpStatusCode.RedirectKeepVerb" /> will cause an exception to be 
		///    thrown.</para><block subset="none" type="note"><para>The default action when this status is received is to follow the Location
		///       header associated with the response. When the original request method was POST,
		///       the redirected request will also use the POST method.</para><para><see cref="F:System.Net.HttpStatusCode.RedirectKeepVerb" /> is a synonym for <see cref="F:System.Net.HttpStatusCode.TemporaryRedirect" />.</para><para>For a detailed description of the HTTP status code 307, see Section 10.3.8 of IETF RFC 2068 -
		///    HTTP/1.1.</para></block></summary>
		RedirectKeepVerb = 307,

		/// <summary><para> Equivalent to HTTP status 400.
		///       Indicates that improper syntax prevented the server from understanding the request .</para><block subset="none" type="note"><para>For a detailed description of the HTTP status code 400, see Section 10.4.1 of IETF RFC 2068 -
		///          HTTP/1.1.</para></block></summary>
		BadRequest = 400,

		/// <summary><para> Equivalent to HTTP status 401. Indicates that user authentication is required
		///       for the request.</para><block subset="none" type="note"><para>The WWW-Authenticate header contains the details of how to perform the
		///          authentication.</para><para>For a detailed description of the HTTP status code 401, see Section 10.4.2 of IETF RFC 2068 -
		///          HTTP/1.1.</para></block></summary>
		Unauthorized = 401,

		/// <summary><para>Equivalent to HTTP status 402.
		///       <see cref="F:System.Net.HttpStatusCode.PaymentRequired" />is reserved for future use.</para></summary>
		PaymentRequired = 402,

		/// <summary><para> Equivalent to HTTP status 403. Indicates that the server understood but refuses to fulfill the request.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 403, see
		///       Section 10.4.4 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		Forbidden = 403,

		/// <summary><para> 
		///       Equivalent to HTTP status 404. Indicates that the server has not found a resource that matches
		///       the requested URI.</para><para><block subset="none" type="note">For adetailed descriptionof the HTTP status code 404, see
		///       Section 10.4.5 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		NotFound = 404,

		/// <summary><para> Equivalent to HTTP status 405. Indicates that the method specified in the Request-Line
		///       is not allowed for the requested resource.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 405, see
		///       Section 10.4.6 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		MethodNotAllowed = 405,

		/// <summary><para>Equivalent to HTTP status 406. Indicates that the only response entities that can be generated by the requested resource have content
		///       characteristics that are not acceptable according to the accept headers sent in the request.</para><para><block subset="none" type="note">For adetailed descriptionof the HTTP status code 406, see
		///       Section 10.4.7 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		NotAcceptable = 406,

		/// <summary><para> Equivalent to HTTP status 407.
		///       Indicates that the client must authenticate itself with the
		///       proxy before proceeding.</para><block subset="none" type="note"><para>The Proxy-authenticate header contains the details of how to perform the
		///          authentication.</para><para>For a detailed description of the HTTP status code 407, see Section 10.4.8 of IETF RFC 2068 -
		///          HTTP/1.1.</para></block></summary>
		ProxyAuthenticationRequired = 407,

		/// <summary><para> Equivalent to HTTP status 408. Indicates that the
		///       server timed out before the client produced a request.</para><para><block subset="none" type="note">For adetailed descriptionof the HTTP status code 408, see
		///       Section 10.4.9 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		RequestTimeout = 408,

		/// <summary><para> Equivalent to HTTP status 409. Indicates that a conflict with the current resource
		///       state prevented the completion of the request.</para><para><block subset="none" type="note">For adetailed descriptionof the HTTP status code 409, see
		///       Section 10.4.10 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		Conflict = 409,

		/// <summary><para>Equivalent to HTTP status 410. Indicates both that the
		///       requested resource is no longer available
		///       on the server and no forwarding address is known.</para><para><block subset="none" type="note">For adetailed descriptionof the HTTP status code 410, see
		///       Section 10.4.11 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		Gone = 410,

		/// <summary><para>Equivalent to HTTP status 411. Indicates that the server refuses to accept the request because its Content-length header is undefined.</para><para><block subset="none" type="note">For adetailed descriptionof the HTTP status code 411, see
		///       Section 10.4.12 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		LengthRequired = 411,

		/// <summary><para> Equivalent to HTTP status 412. Indicates
		///       that a precondition given in one or more of the request-header fields
		///       was tested on the server but evaluated to
		///       false.</para><block subset="none" type="note"><para>Conditions are set with conditional request headers such as If-Match,
		///          If-None-Match, or If-Unmodified-Since.</para><para>For a detailed description of the HTTP status code 412, see Section 10.4.13 of IETF RFC 2068 -
		///          HTTP/1.1.</para></block></summary>
		PreconditionFailed = 412,

		/// <summary><para> Equivalent to HTTP status 413. Indicates that the
		///       request entity is larger that the server is willing or able to process, so the server is not
		///       processing the request.</para><para><block subset="none" type="note">For a detailed description of the HTTP
		///       status code 413, see Section 10.4.14 of IETF RFC 2068 -
		///       HTTP/1.1.</block></para></summary>
		RequestEntityTooLarge = 413,

		/// <summary><para> 
		///       Equivalent to HTTP status 414. Indicates
		///       that the Request-URI is longer than the server will interpret, so the server is not servicing the
		///       request.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 414, see
		///       Section 10.4.15 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		RequestUriTooLong = 414,

		/// <summary><para> Equivalent to HTTP status
		///       415. Indicates that the format of the entity of the request is not supported by the requested resource, so the server is not servicing the request.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 415, see
		///       Section 10.4.16 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		UnsupportedMediaType = 415,

		/// <summary><para> Equivalent to HTTP status
		///       416. Indicates that none of the values specified by the Range request-header field
		///       overlap the current extent of the
		///       selected resource, and no If-Range request-header field was contained by the request.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 416, see
		///       Section 10.4.17 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		RequestedRangeNotSatisfiable = 416,

		/// <summary><para> 
		///       Equivalent to HTTP status 417. Indicates that the Expect request-header field condition could
		///       not be met by the server, or the server is a proxy and has unambiguous evidence
		///       that the next-hop server cannot meet the condition.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 417, see
		///       Section 10.4.18 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		ExpectationFailed = 417,

		/// <summary><para>Equivalent to HTTP status 500. Indicates that the request could not be fulfilled by the server due to an unexpected condition.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 500, see
		///       Section 10.5.1 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		InternalServerError = 500,

		/// <summary><para> Equivalent to HTTP status 501. Indicates that
		///       the functionality required to fulfill the request is not supported by the server.
		///       This is appropriate, for example, if the server does not recognize the request method and cannot support it for any resource.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 501, see
		///       Section 10.5.2 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		NotImplemented = 501,

		/// <summary><para> Equivalent to HTTP status 502. Indicates that the
		///       server, acting as a gateway or proxy, received an invalid response from the upstream
		///       server that was accessed while attempting to fulfill the request.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 502, see
		///       Section 10.5.3 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		BadGateway = 502,

		/// <summary><para> Equivalent to HTTP status 503. Indicates that a temporary overloading or maintenance of
		///       the server is preventing it from handling the request.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 503, see
		///       Section 10.5.4 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		ServiceUnavailable = 503,

		/// <summary><para> 
		///       Equivalent to HTTP status 504. Indicates that the server, acting as a gateway or proxy, timed out while waiting for a response from an
		///       upstream server accessed in an attempt to fulfill the request.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 504, see
		///       Section 10.5.5 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		GatewayTimeout = 504,

		/// <summary><para> Equivalent to HTTP
		///       status 505. Indicates that the HTTP protocol version used by the request is not
		///       supported by the server.</para><para><block subset="none" type="note">For a detailed description of the HTTP status code 505, see
		///       Section 10.5.6 of IETF RFC 2068 - HTTP/1.1.</block></para></summary>
		HttpVersionNotSupported = 505,
	} // HttpStatusCode

} // System.Net
