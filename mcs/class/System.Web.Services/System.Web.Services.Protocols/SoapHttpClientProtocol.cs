// 
// System.Web.Services.Protocols.SoapHttpClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
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

		SoapClientMessage CreateMessage (string method_name, object [] parameters)
		{
			//SoapClientMessage message = new SoapClientMessage (this);

			return null;
		}
		
		void SendMessage (WebRequest request, SoapClientMessage message)
		{
			
		}
		
		protected object[] Invoke (string method_name, object[] parameters)
		{
			SoapClientMessage message = CreateMessage (method_name, parameters);
			WebRequest request = GetWebRequest (uri);
			Stream s = request.GetRequestStream ();
			
			try {
				SendMessage (request, message);
			} finally {
				s.Close ();
			}

			return null;
		}

		#endregion // Methods
	}
}
