//
// X509PalImpl.Apple.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin, Inc.
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
#if MONO_FEATURE_APPLETLS || MONO_FEATURE_APPLE_X509
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32.SafeHandles;
using XamMac.CoreFoundation;

namespace Mono.AppleTls
{
	class X509PalImplApple : X509PalImpl
	{
		public override X509CertificateImpl Import (byte[] data)
		{
			data = ConvertData (data);

			var handle = CFHelpers.CreateCertificateFromData (data);
			if (handle != IntPtr.Zero)
				return new X509CertificateImplApple (handle, true);

			return null;
		}

		public override X509Certificate2Impl Import (
			byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags)
		{
			return null;
		}

		public override X509Certificate2Impl Import (X509Certificate cert)
		{
			return null;
		}
	}
}
#endif
