//
// System.Net.HttpContinueDelegate.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

namespace System.Net
{
	public delegate void HttpContinueDelegate (
			int StatusCode,
	   		WebHeaderCollection httpHeaders);
}
