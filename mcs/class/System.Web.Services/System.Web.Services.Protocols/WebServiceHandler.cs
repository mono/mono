// 
// System.Web.Services.Protocols.WebServiceHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	internal class WebServiceHandler {

		#region Fields

		ServerProtocol protocol;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public WebServiceHandler (ServerProtocol protocol)
		{
			this.protocol = protocol;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		protected IAsyncResult BeginCoreProcessRequest (AsyncCallback callback, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void CoreProcessRequest ()
		{
			Invoke ();
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void EndCoreProcessRequest (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void Invoke ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void WriteReturns (object[] returnValues)
		{
			//protocol.WriteReturns (returnValues, outputStream);
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
