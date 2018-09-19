//
// MonoOpenSSLX509Store.cs
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
#if SECURITY_DEP && MONO_FEATURE_OPENSSL
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

namespace Mono.OpenSSL
{
	class MonoOpenSSLX509Store : MonoOpenSSLObject
	{
		internal class OpenSSLX509StoreHandle : MonoOpenSSLHandle
		{
			public OpenSSLX509StoreHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_openssl_x509_store_free (handle);
				return true;
			}
		}

		new internal OpenSSLX509StoreHandle Handle {
			get { return (OpenSSLX509StoreHandle)base.Handle; }
		}

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_new ();

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_from_ctx (IntPtr ctx);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_store_from_ssl_ctx (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_store_load_locations (IntPtr handle, IntPtr file, IntPtr path);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_store_set_default_paths (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_store_add_cert (IntPtr handle, IntPtr x509);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_store_get_count (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_x509_store_free (IntPtr handle);

		Dictionary<IntPtr,MonoOpenSSLX509Lookup> lookupHash;

		public void LoadLocations (string file, string path)
		{
			IntPtr filePtr = IntPtr.Zero;
			IntPtr pathPtr = IntPtr.Zero;
			try {
				if (file != null)
					filePtr = Marshal.StringToHGlobalAnsi (file);
				if (path != null)
					pathPtr = Marshal.StringToHGlobalAnsi (path);
				var ret = mono_openssl_x509_store_load_locations (
					Handle.DangerousGetHandle (), filePtr, pathPtr);
				CheckError (ret);
			} finally {
				if (filePtr != IntPtr.Zero)
					Marshal.FreeHGlobal (filePtr);
				if (pathPtr != IntPtr.Zero)
					Marshal.FreeHGlobal (pathPtr);
			}
		}

		public void SetDefaultPaths ()
		{
			var ret = mono_openssl_x509_store_set_default_paths (Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		static OpenSSLX509StoreHandle Create_internal ()
		{
			var handle = mono_openssl_x509_store_new ();
			if (handle == IntPtr.Zero)
				throw new MonoOpenSSLException ();
			return new OpenSSLX509StoreHandle (handle);
		}

		static OpenSSLX509StoreHandle Create_internal (IntPtr store_ctx)
		{
			var handle = mono_openssl_x509_store_from_ssl_ctx (store_ctx);
			if (handle == IntPtr.Zero)
				throw new MonoOpenSSLException ();
			return new OpenSSLX509StoreHandle (handle);
		}

		static OpenSSLX509StoreHandle Create_internal (MonoOpenSSLSslCtx.OpenSSLCtxHandle ctx)
		{
			var handle = mono_openssl_x509_store_from_ssl_ctx (ctx.DangerousGetHandle ());
			if (handle == IntPtr.Zero)
				throw new MonoOpenSSLException ();
			return new OpenSSLX509StoreHandle (handle);
		}

		internal MonoOpenSSLX509Store ()
			: base (Create_internal ())
		{
		}

		internal MonoOpenSSLX509Store (IntPtr store_ctx)
			: base (Create_internal (store_ctx))
		{
		}

		internal MonoOpenSSLX509Store (MonoOpenSSLSslCtx.OpenSSLCtxHandle ctx)
			: base (Create_internal (ctx))
		{
		}

		public void AddCertificate (MonoOpenSSLX509 x509)
		{
			var ret = mono_openssl_x509_store_add_cert (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public int GetCount ()
		{
			return mono_openssl_x509_store_get_count (Handle.DangerousGetHandle ());
		}

		internal void AddTrustedRoots ()
		{
			MonoOpenSSLProvider.SetupCertificateStore (this, MonoTlsSettings.DefaultSettings, false);
		}

		public MonoOpenSSLX509Lookup AddLookup (MonoOpenSSLX509LookupType type)
		{
			if (lookupHash == null)
				lookupHash = new Dictionary<IntPtr,MonoOpenSSLX509Lookup> ();

			/*
			 * X509_STORE_add_lookup() returns the same 'X509_LOOKUP *' for each
			 * unique 'X509_LOOKUP_METHOD *' (which is supposed to be a static struct)
			 * and we want to use the same managed object for each unique 'X509_LOOKUP *'.
			*/
			var lookup = new MonoOpenSSLX509Lookup (this, type);
			var nativeLookup = lookup.GetNativeLookup ();
			if (lookupHash.ContainsKey (nativeLookup)) {
				lookup.Dispose ();
				lookup = lookupHash [nativeLookup];
			} else {
				lookupHash.Add (nativeLookup, lookup);
			}

			return lookup;
		}

		public void AddDirectoryLookup (string dir, MonoOpenSSLX509FileType type)
		{
			var lookup = AddLookup (MonoOpenSSLX509LookupType.HASH_DIR);
			lookup.AddDirectory (dir, type);
		}

		public void AddFileLookup (string file, MonoOpenSSLX509FileType type)
		{
			var lookup = AddLookup (MonoOpenSSLX509LookupType.FILE);
			lookup.LoadFile (file, type);
		}

		public void AddCollection (X509CertificateCollection collection, MonoOpenSSLX509TrustKind trust)
		{
			var monoLookup = new MonoOpenSSLX509LookupMonoCollection (collection, trust);
			var lookup = new MonoOpenSSLX509Lookup (this, MonoOpenSSLX509LookupType.MONO);
			lookup.AddMono (monoLookup);
		}

#if MONODROID
		public void AddAndroidLookup ()
		{
			var androidLookup = new MonoOpenSSLX509LookupAndroid ();
			var lookup = new MonoOpenSSLX509Lookup (this, MonoOpenSSLX509LookupType.MONO);
			lookup.AddMono (androidLookup);
		}
#endif

		protected override void Close ()
		{
			try {
				if (lookupHash != null) {
					foreach (var lookup in lookupHash.Values)
						lookup.Dispose ();
					lookupHash = null;
				}
			} finally {
				base.Close ();
			}
		}
	}
}
#endif
