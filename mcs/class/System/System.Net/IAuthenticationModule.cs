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
	public interface IAuthenticationModule
	{
		Authorization Authenticate (string challenge, WebRequest request, ICredentials credentials);
		Authorization PreAuthenticate (WebRequest request, ICredentials credentials);
		string AuthenticationType { get; }
		bool CanPreAuthenticate { get; }
	}
}
