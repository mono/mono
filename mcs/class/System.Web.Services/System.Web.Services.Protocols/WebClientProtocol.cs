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

		public string ConnectionGroupName {
			get { return connectionGroupName; }
			set { connectionGroupName = value; }
		}

		public ICredentials Credentials {
			get { return credentials; }
			set { credentials = value; }
		}

		public bool PreAuthenticate {
			get { return preAuthenticate; }
			set { preAuthenticate = value; }
		}

		public Encoding RequestEncoding {
			get { return requestEncoding; }
			set { requestEncoding = value; }
		}

		public int Timeout {
			get { return timeout; }
			set { timeout = value; }
		}

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
