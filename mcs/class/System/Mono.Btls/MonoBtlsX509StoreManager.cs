//
// MonoBtlsX509StoreManager.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
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
#if SECURITY_DEP && MONO_FEATURE_BTLS
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
using MX = MonoSecurity::Mono.Security.X509;
#else
using Mono.Security.Interface;
using MX = Mono.Security.X509;
#endif

namespace Mono.Btls
{
	static class MonoBtlsX509StoreManager
	{
		static bool initialized;
#if !MONODROID
		static string machineTrustedRootPath;
		static string machineIntermediateCAPath;
		static string machineUntrustedPath;
		static string userTrustedRootPath;
		static string userIntermediateCAPath;
		static string userUntrustedPath;
#endif

		static void Initialize ()
		{
			if (initialized)
				return;

			try {
				DoInitialize ();
			} catch (Exception ex) {
				Console.Error.WriteLine ("MonoBtlsX509StoreManager.Initialize() threw exception: {0}", ex);
			} finally {
				initialized = true;
			}
		}

		static void DoInitialize ()
		{
#if !MONODROID
			var userPath = MX.X509StoreManager.NewCurrentUserPath;
			userTrustedRootPath = Path.Combine (userPath, MX.X509Stores.Names.TrustedRoot);
			userIntermediateCAPath = Path.Combine (userPath, MX.X509Stores.Names.IntermediateCA);
			userUntrustedPath = Path.Combine (userPath, MX.X509Stores.Names.Untrusted);

			var machinePath = MX.X509StoreManager.NewLocalMachinePath;
			machineTrustedRootPath = Path.Combine (machinePath, MX.X509Stores.Names.TrustedRoot);
			machineIntermediateCAPath = Path.Combine (machinePath, MX.X509Stores.Names.IntermediateCA);
			machineUntrustedPath = Path.Combine (machinePath, MX.X509Stores.Names.Untrusted);
#endif
		}

		public static bool HasStore (MonoBtlsX509StoreType type)
		{
#if MONODROID
			return false;
#else
			var path = GetStorePath (type);
			return path != null && Directory.Exists (path);
#endif
		}

		public static string GetStorePath (MonoBtlsX509StoreType type)
		{
#if MONODROID
			throw new NotSupportedException ();
#else
			Initialize ();
			switch (type) {
			case MonoBtlsX509StoreType.MachineTrustedRoots:
				return machineTrustedRootPath;
			case MonoBtlsX509StoreType.MachineIntermediateCA:
				return machineIntermediateCAPath;
			case MonoBtlsX509StoreType.MachineUntrusted:
				return machineUntrustedPath;
			case MonoBtlsX509StoreType.UserTrustedRoots:
				return userTrustedRootPath;
			case MonoBtlsX509StoreType.UserIntermediateCA:
				return userIntermediateCAPath;
			case MonoBtlsX509StoreType.UserUntrusted:
				return userUntrustedPath;
			default:
				throw new NotSupportedException ();
			}
#endif
		}
	}
}
#endif
