//
// System.Net.Sockets.ProtocolFamily.cs
//
// Author:
//   Andrew Sutton
//
// (C) Andrew Sutton
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Net.Sockets
{
	public enum ProtocolFamily
	{
		Unknown = -1,
		Unspecified = 0,
		Unix,
		InterNetwork,
		ImpLink,
		Pup,
		Chaos,
		Ipx,
		Iso,
		Ecma,
		DataKit,
		Ccitt,
		Sna,
		DecNet,
		DataLink,
		Lat,
		HyperChannel,
		AppleTalk,
		NetBios,
		VoiceView,
		FireFox,
		Banyan = 0x15,
		Atm,
		InterNetworkV6,
		Cluster,
		Ieee12844,
		Irda,
		NetworkDesigners = 0x1c,
		Max,

		NS = Ipx,
		Osi = Iso,
	}
}
