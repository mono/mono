// 
// System.Web.Services.Protocols.HttpSimpleClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class HttpSimpleClientProtocol : HttpWebClientProtocol {

		#region Fields

		IAsyncResult result;

		#endregion // Fields

		#region Constructors

		protected HttpSimpleClientProtocol () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		protected IAsyncResult BeginInvoke (string methodName, string requestUrl, object[] parameters, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object EndInvoke (IAsyncResult asyncResult)
		{
			if (asyncResult != result)
				throw new ArgumentException ("asyncResult is not the return value from BeginInvoke");
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object Invoke (string methodName, string requestUrl, object[] parameters)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
