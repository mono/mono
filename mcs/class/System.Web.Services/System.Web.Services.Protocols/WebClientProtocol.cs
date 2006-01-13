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
	public abstract class WebClientProtocol : Component {

		#region Fields

		string connectionGroupName;
		ICredentials credentials;
		bool preAuthenticate;
		Encoding requestEncoding;
		int timeout;
		string url;
		bool abort;

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
			url = String.Empty;
			abort = false;
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
			get { return url; }
			set {
				url = value;
				uri = new Uri (url);
			}
		}
#if NET_2_0
		[MonoTODO]
		public bool UseDefaultCredentials {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif

		#endregion // Properties

		#region Methods

		public virtual void Abort ()
		{
			if (current_request != null){
				current_request.Abort ();
				current_request = null;
			}
			abort = true;
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

			current_request = WebRequest.Create (uri);
			current_request.Timeout = timeout;
			current_request.PreAuthenticate = preAuthenticate;
			current_request.ConnectionGroupName = connectionGroupName;

			if (credentials != null)
				current_request.Credentials = credentials;

			return current_request;
		}

		protected virtual WebResponse GetWebResponse (WebRequest request)
		{
			if (abort)
				throw new WebException ("The operation has been aborted.", WebExceptionStatus.RequestCanceled);

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
			if (abort)
				throw new WebException ("The operation has been aborted.", WebExceptionStatus.RequestCanceled);

			return request.EndGetResponse (result);
		}

		#endregion // Methods
	}
}
