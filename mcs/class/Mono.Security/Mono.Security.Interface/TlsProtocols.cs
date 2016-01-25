//
// TlsProtocols.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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

using System;

namespace Mono.Security.Interface
{
	[Flags]
	// Keep in sync with SchProtocols / native SChannel.h
	// Unfortunately, the definition in System.dll is not public, so we need to duplicate it here.
	public enum TlsProtocols {
		Zero                = 0,
		Tls10Client         = 0x00000080,
		Tls10Server         = 0x00000040,
		Tls10               = (Tls10Client | Tls10Server),
		Tls11Client         = 0x00000200,
		Tls11Server         = 0x00000100,
		Tls11               = (Tls11Client | Tls11Server),
		Tls12Client         = 0x00000800,
		Tls12Server         = 0x00000400,
		Tls12               = (Tls12Client | Tls12Server),
		ClientMask          = (Tls10Client | Tls11Client | Tls12Client),
		ServerMask          = (Tls10Server | Tls11Server | Tls12Server)
	};
}

