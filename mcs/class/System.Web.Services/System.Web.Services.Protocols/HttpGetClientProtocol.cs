// 
// System.Web.Services.Protocols.HttpGetClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Net;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class HttpGetClientProtocol : HttpSimpleClientProtocol {

		#region Constructors

		public HttpGetClientProtocol () 
		{
			TypeStub = (HttpSimpleTypeStubInfo) TypeStubManager.GetTypeStub (GetType(), "HttpGet");
		}
		
		#endregion // Constructors

		#region Methods

		protected override WebRequest GetWebRequest (Uri uri)
		{
			if (uri == null)
				throw new InvalidOperationException ("The uri parameter is null.");
			if (uri.ToString () == String.Empty)
				throw new InvalidOperationException ("The uri parameter has a length of zero.");
				
			WebRequest request = WebRequest.Create (uri);
			request.Method = "GET";
			return request;
		}

		#endregion // Methods
	}
}
