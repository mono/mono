//
// Microsoft.Web.Services.Messaging.ISoapTransport.cs
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// (C) Ximian, Inc. 2003.
//

using System;
using System.IO;
using System.Net;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Messaging {

	public interface ISoapTransport {

		ICredentials Credentials { get; set; }

		int IdleTimeout { get; set; }
		
		//string Scheme { get; }
		
		IAsyncResult BeginSend ( 
			SoapEnvelope envelope,
			Uri destination,
			AsyncCallback callback,
			object state);
		
		void EndSend (IAsyncResult result);

		void RegisterPort (Uri to, Type port);

		void RegisterPort (Uri to, SoapReceiver receiver);

		void Send (SoapEnvelope envelop, Uri destination);
		
		void UnregisterAll ();

		void UnregisterPort (Uri to);

	}
}







