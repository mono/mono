// 
// System.Web.Services.Protocols.SoapHttpClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Net;
using System.Web;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class SoapHttpClientProtocol : HttpWebClientProtocol {

		#region Constructors

		public SoapHttpClientProtocol () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		protected IAsyncResult BeginInvoke (string methodName, object[] parameters, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Discover ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object[] EndInvoke (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		protected override WebRequest GetWebRequest (Uri uri)
		{
			return WebRequest.Create (uri);
		}

		[MonoTODO]
		protected object[] Invoke (string methodName, object[] parameters)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
