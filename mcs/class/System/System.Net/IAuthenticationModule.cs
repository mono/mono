//
// System.Net.IAuthenticationModule.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Net {

	// <remarks>
	//   Authentication interface for Web client authentication modules.
	// </remarks>
	public interface IAuthenticationModule {
		string AuthenticationType {
			get;
		}

		bool CanPreAuthenticate {
			get;
		}

		bool CanRespond (string challenge);

		Authorization PreAuthenticate (WebRequest request, ICredentialLookup credentials);

		Authorization Respond (string challenge, WebRequest request, ICredentialLookup credentials);
	}
}
