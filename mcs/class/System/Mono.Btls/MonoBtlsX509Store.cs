//
// MonoBtlsX509Store.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

namespace Mono.Btls
{
	class MonoBtlsX509Store : MonoBtlsObject
	{
		internal class BoringX509StoreHandle : MonoBtlsHandle
		{
			public BoringX509StoreHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_btls_x509_store_free (handle);
				return true;
			}
		}

		new internal BoringX509StoreHandle Handle {
			get { return (BoringX509StoreHandle)base.Handle; }
		}

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_new ();

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_from_ctx (IntPtr ctx);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_store_from_ssl_ctx (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_store_load_locations (IntPtr handle, IntPtr file, IntPtr path);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_store_set_default_paths (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_store_add_cert (IntPtr handle, IntPtr x509);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_store_get_count (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_x509_store_free (IntPtr handle);

		Dictionary<IntPtr,MonoBtlsX509Lookup> lookupHash;

		public void LoadLocations (string file, string path)
		{
			IntPtr filePtr = IntPtr.Zero;
			IntPtr pathPtr = IntPtr.Zero;
			try {
				if (file != null)
					filePtr = Marshal.StringToHGlobalAnsi (file);
				if (path != null)
					pathPtr = Marshal.StringToHGlobalAnsi (path);
				var ret = mono_btls_x509_store_load_locations (
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
			var ret = mono_btls_x509_store_set_default_paths (Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		static BoringX509StoreHandle Create_internal ()
		{
			var handle = mono_btls_x509_store_new ();
			if (handle == IntPtr.Zero)
				throw new MonoBtlsException ();
			return new BoringX509StoreHandle (handle);
		}

		static BoringX509StoreHandle Create_internal (IntPtr store_ctx)
		{
			var handle = mono_btls_x509_store_from_ssl_ctx (store_ctx);
			if (handle == IntPtr.Zero)
				throw new MonoBtlsException ();
			return new BoringX509StoreHandle (handle);
		}

		static BoringX509StoreHandle Create_internal (MonoBtlsSslCtx.BoringSslCtxHandle ctx)
		{
			var handle = mono_btls_x509_store_from_ssl_ctx (ctx.DangerousGetHandle ());
			if (handle == IntPtr.Zero)
				throw new MonoBtlsException ();
			return new BoringX509StoreHandle (handle);
		}

		internal MonoBtlsX509Store ()
			: base (Create_internal ())
		{
		}

		internal MonoBtlsX509Store (IntPtr store_ctx)
			: base (Create_internal (store_ctx))
		{
		}

		internal MonoBtlsX509Store (MonoBtlsSslCtx.BoringSslCtxHandle ctx)
			: base (Create_internal (ctx))
		{
		}

		public void AddCertificate (MonoBtlsX509 x509)
		{
			var ret = mono_btls_x509_store_add_cert (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public int GetCount ()
		{
			return mono_btls_x509_store_get_count (Handle.DangerousGetHandle ());
		}

		internal void AddTrustedRoots ()
		{
			MonoBtlsProvider.SetupCertificateStore (this, MonoTlsSettings.DefaultSettings, false);
		}

		public MonoBtlsX509Lookup AddLookup (MonoBtlsX509LookupType type)
		{
			if (lookupHash == null)
				lookupHash = new Dictionary<IntPtr,MonoBtlsX509Lookup> ();

			/*
			 * X509_STORE_add_lookup() returns the same 'X509_LOOKUP *' for each
			 * unique 'X509_LOOKUP_METHOD *' (which is supposed to be a static struct)
			 * and we want to use the same managed object for each unique 'X509_LOOKUP *'.
			*/
			var lookup = new MonoBtlsX509Lookup (this, type);
			var nativeLookup = lookup.GetNativeLookup ();
			if (lookupHash.ContainsKey (nativeLookup)) {
				lookup.Dispose ();
				lookup = lookupHash [nativeLookup];
			} else {
				lookupHash.Add (nativeLookup, lookup);
			}

			return lookup;
		}

		public void AddDirectoryLookup (string dir, MonoBtlsX509FileType type)
		{
			var lookup = AddLookup (MonoBtlsX509LookupType.HASH_DIR);
			lookup.AddDirectory (dir, type);
		}

		public void AddFileLookup (string file, MonoBtlsX509FileType type)
		{
			var lookup = AddLookup (MonoBtlsX509LookupType.FILE);
			lookup.LoadFile (file, type);
		}

		public void AddCollection (X509CertificateCollection collection, MonoBtlsX509TrustKind trust)
		{
			var monoLookup = new MonoBtlsX509LookupMonoCollection (collection, trust);
			var lookup = new MonoBtlsX509Lookup (this, MonoBtlsX509LookupType.MONO);
			lookup.AddMono (monoLookup);
		}

#if MONODROID
		public void AddAndroidLookup ()
		{
			var androidLookup = new MonoBtlsX509LookupAndroid ();
			var lookup = new MonoBtlsX509Lookup (this, MonoBtlsX509LookupType.MONO);
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
