//
// System.Net.ICredential.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//

namespace System.Net {

	// <remarks>
	//   Base authentication interface for Web clients.
	// </remarks>
	public interface ICredentials 
	{
		NetworkCredential GetCredential (Uri uri, string authType);
	}
}
