// 
// System.Web.Services.Protocols.WebClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Net;
using System.Text;
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

		#endregion

		#region Constructors

		protected WebClientProtocol () 
		{
			connectionGroupName = String.Empty;
			credentials = null;
			preAuthenticate = false;
			requestEncoding = null;
			timeout = 100000;
			url = String.Empty;
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

		[DefaultValue ("")]
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
			set { url = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public virtual void Abort ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static void AddToCache (Type type, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static object GetFromCache (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual WebRequest GetWebRequest (Uri uri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual WebResponse GetWebResponse (WebRequest request)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual WebResponse GetWebResponse (WebRequest request, IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
