//
// System.Net.IWebProxy.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;

namespace System.Net {

	// <remarks>
	// </remarks>
	public interface IWebProxy {
		ICredentials Credentials {
			get; 
			set;
		}

		Uri GetProxy (Uri destination);
		
		bool IsBypassed (Uri host);
	}
}
