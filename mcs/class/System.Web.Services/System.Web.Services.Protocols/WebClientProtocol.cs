// 
// System.Web.Services.Protocols.WebClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Services;

namespace System.Web.Services.Protocols {
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public abstract class WebClientProtocol : Component {

		#region Fields

		string connectionGroupName;
		ICredentials credentials;
		bool preAuthenticate;
		Encoding requestEncoding;
		int timeout;

		//
		// Used by SoapHttpClientProtocol, use this to avoid creating a new Uri on each invocation.
		//
		internal Uri uri;
			
		//
		// Points to the current request, so we can call Abort() on it
		//
		WebRequest current_request;
		
#if !TARGET_JVM
		static HybridDictionary cache;
#else
		static HybridDictionary cache {
			get {
				return (HybridDictionary)AppDomain.CurrentDomain.GetData("WebClientProtocol.cache");
			}
			set {
				AppDomain.CurrentDomain.SetData("WebClientProtocol.cache", value);
			}
		}
#endif
		#endregion

		#region Constructors

		static WebClientProtocol ()
		{
			cache = new HybridDictionary ();
		}

		protected WebClientProtocol () 
		{
			connectionGroupName = String.Empty;
			credentials = null;
			preAuthenticate = false;
			requestEncoding = null;
			timeout = 100000;
		}
		
		#endregion // Constructors

		#region Properties

		[DefaultValue ("")]
		public string ConnectionGroupName {
			get { return connectionGroupName; }
			set { connectionGroupName = value; }
		}

		public ICredentials Credentials {
			get { return credentials; }
			set { credentials = value; }
		}

		[DefaultValue (false)]
		[WebServicesDescription ("Enables pre authentication of the request.")]
		public bool PreAuthenticate {
			get { return preAuthenticate; }
			set { preAuthenticate = value; }
		}

		[DefaultValue (null)]
		[RecommendedAsConfigurable (true)]
		[WebServicesDescription ("The encoding to use for requests.")]
		public Encoding RequestEncoding {
			get { return requestEncoding; }
			set { requestEncoding = value; }
		}

		[DefaultValue (100000)]
		[RecommendedAsConfigurable (true)]
		[WebServicesDescription ("Sets the timeout in milliseconds to be used for synchronous calls.  The default of -1 means infinite.")]
		public int Timeout {
			get { return timeout; }
			set { timeout = value; }
		}

		[DefaultValue ("")]
		[RecommendedAsConfigurable (true)]
		[WebServicesDescription ("The base URL to the server to use for requests.")]
		public string Url {
			get { return uri == null ? String.Empty : uri.AbsoluteUri; }
			set { uri = new Uri (value); }
		}
#if NET_2_0
		public bool UseDefaultCredentials {
			get { return CredentialCache.DefaultCredentials == Credentials; }
			set { Credentials = value ? CredentialCache.DefaultCredentials : null; }
		}
#endif

		#endregion // Properties

		#region Methods

		public virtual void Abort ()
		{
			WebRequest request = current_request;
			current_request = null;
			if (request != null) 
				request.Abort ();
		}

		protected static void AddToCache (Type type, object value)
		{
			cache [type] = value;
		}

		protected static object GetFromCache (Type type)
		{
			return cache [type];
		}

		protected virtual WebRequest GetWebRequest (Uri uri)
		{
			if (uri == null)
				throw new InvalidOperationException ("uri is null");

			WebRequest request = WebRequest.Create (uri);
			request.Timeout = timeout;
			request.PreAuthenticate = preAuthenticate;
			request.ConnectionGroupName = connectionGroupName;

			if (credentials != null)
				request.Credentials = credentials;

			current_request = request;
			return request;
		}

		protected virtual WebResponse GetWebResponse (WebRequest request)
		{
			WebResponse response = null;
			try {
				request.Timeout = timeout;
				response = request.GetResponse ();
			} catch (WebException e) {
				response = e.Response;
				if (response == null)
					throw;
			}

			return response;
		}

		protected virtual WebResponse GetWebResponse (WebRequest request, IAsyncResult result)
		{
			return request.EndGetResponse (result);
		}

		#endregion // Methods
	}
}
