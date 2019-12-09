//
// X509PalImpl.Btls.cs
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
#if MONO_FEATURE_BTLS
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32.SafeHandles;

namespace Mono.Btls
{
	class X509PalImplBtls : X509PalImpl
	{
		public X509PalImplBtls (MonoTlsProvider provider)
		{
			Provider = (MonoBtlsProvider)provider;
		}

		MonoBtlsProvider Provider {
			get;
		}

		public override X509CertificateImpl Import (byte[] data)
		{
			return Provider.GetNativeCertificate (data, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		public override X509Certificate2Impl Import (
			byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags)
		{
			return Provider.GetNativeCertificate (data, password, keyStorageFlags);
		}

		public override X509Certificate2Impl Import (X509Certificate cert)
		{
			return Provider.GetNativeCertificate (cert);
		}
	}
}
#endif
