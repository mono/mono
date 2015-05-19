using System;
using System.Net;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Contains the HTTP response data after executing
	/// <see cref="WebTest.Run"/>
	/// </summary>
	/// <seealso cref="WebTest.Run"/>
	[Serializable]
	public class Response
	{
		string _body;
		HttpStatusCode _statusCode;
		string _statusDescription;
		/// <summary>
		/// Get the response body.
		/// </summary>
		public string Body
		{
			get { return _body; }
			internal
			set { _body = value; }
		}

		/// <summary>
		/// Get the HTTP status code of the response
		/// </summary>
		public HttpStatusCode StatusCode
		{
			get { return _statusCode; }
			internal
			set { _statusCode = value; }
		}

		/// <summary>
		/// Get the HTTP status description of the response
		/// </summary>
		public string StatusDescription
		{
			get { return _statusDescription; }
			internal
			set { _statusDescription = value; }
		}
	}
}
