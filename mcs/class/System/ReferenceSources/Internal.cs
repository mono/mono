//
// Internal.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace System.Net.Security
{
	//From Schannel.h
	[Flags]
	internal enum SchProtocols
	{
		Zero = 0,
		PctClient = 0x00000002,
		PctServer = 0x00000001,
		Pct = (PctClient | PctServer),
		Ssl2Client = 0x00000008,
		Ssl2Server = 0x00000004,
		Ssl2 = (Ssl2Client | Ssl2Server),
		Ssl3Client = 0x00000020,
		Ssl3Server = 0x00000010,
		Ssl3 = (Ssl3Client | Ssl3Server),
		Tls10Client = 0x00000080,
		Tls10Server = 0x00000040,
		Tls10 = (Tls10Client | Tls10Server),
		Tls11Client = 0x00000200,
		Tls11Server = 0x00000100,
		Tls11 = (Tls11Client | Tls11Server),
		Tls12Client = 0x00000800,
		Tls12Server = 0x00000400,
		Tls12 = (Tls12Client | Tls12Server),
		Ssl3Tls = (Ssl3 | Tls10),
		UniClient = unchecked((int)0x80000000),
		UniServer = 0x40000000,
		Unified = (UniClient | UniServer),
		ClientMask = (PctClient | Ssl2Client | Ssl3Client | Tls10Client | Tls11Client | Tls12Client | UniClient),
		ServerMask = (PctServer | Ssl2Server | Ssl3Server | Tls10Server | Tls11Server | Tls12Server | UniServer)
	}

	//From Schannel.h
	[StructLayout (LayoutKind.Sequential)]
	internal class SslConnectionInfo
	{
		public readonly int Protocol;
		public readonly int DataCipherAlg;
		public readonly int DataKeySize;
		public readonly int DataHashAlg;
		public readonly int DataHashKeySize;
		public readonly int KeyExchangeAlg;
		public readonly int KeyExchKeySize;

		internal SslConnectionInfo (int protocol)
		{
			Protocol = protocol;
		}
	}
}
