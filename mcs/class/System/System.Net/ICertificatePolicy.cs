//
// System.Net.ICertificatePolicy.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System.Security.Cryptography.X509Certificates;

namespace System.Net {

	// <remarks>
	// </remarks>
	public interface ICertificatePolicy {
		bool CheckValidationResult (
				ServicePoint srvPoint,
				X509Certificate certificate,
		   		WebRequest request,
		   		int certificateProblem
		);		
	}
}
