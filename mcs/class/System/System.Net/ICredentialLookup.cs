//
// System.Net.ICredential.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//

namespace System.Net {

	// <remarks>
	//   Base authentication interface for Web clients.
	// </remarks>
	public interface ICredential {
		
		NetworkCredential GetCredential (string uri, string AuthType);
	}
}
