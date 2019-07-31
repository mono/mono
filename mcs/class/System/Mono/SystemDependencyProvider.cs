//
// SystemDependencyProvider.cs
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
using System;
using System.Threading;

namespace Mono
{
	/*
	 * The purpose of this class is to allow code in `corlib.dll` to access `System.dll` APIs.
	 */
	class SystemDependencyProvider : ISystemDependencyProvider
	{
		static SystemDependencyProvider instance;
		static object syncRoot = new object ();

		public static SystemDependencyProvider Instance {
			get {
				Initialize ();
				return instance;
			}
		}

		internal static void Initialize ()
		{
			lock (syncRoot) {
				if (instance != null)
					return;

				instance = new SystemDependencyProvider ();
			}
		}

		ISystemCertificateProvider ISystemDependencyProvider.CertificateProvider => CertificateProvider;

		public SystemCertificateProvider CertificateProvider {
			get;
		}

		public X509PalImpl X509Pal => CertificateProvider.X509Pal;

		SystemDependencyProvider ()
		{
			CertificateProvider = new SystemCertificateProvider ();

			/*
			 * Register ourselves with corlib's `DependencyInjector`.
			 */
			DependencyInjector.Register (this);
		}
	}
}
