//
// MonoBtlsX509Lookup.cs
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
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Mono.Btls
{
	class MonoBtlsX509Lookup : MonoBtlsObject
	{
		internal class BoringX509LookupHandle : MonoBtlsHandle
		{
			public BoringX509LookupHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_btls_x509_lookup_free (handle);
				return true;
			}
		}

		new internal BoringX509LookupHandle Handle {
			get { return (BoringX509LookupHandle)base.Handle; }
		}

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_lookup_new (IntPtr store, MonoBtlsX509LookupType type);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_lookup_load_file (IntPtr handle, IntPtr file, MonoBtlsX509FileType type);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_lookup_add_dir (IntPtr handle, IntPtr dir, MonoBtlsX509FileType type);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_lookup_add_mono (IntPtr handle, IntPtr monoLookup);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_lookup_init (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_lookup_shutdown (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_lookup_by_subject (IntPtr handle, IntPtr name);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_lookup_by_fingerprint (IntPtr handle, IntPtr bytes, int len);

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_x509_lookup_free (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_lookup_peek_lookup (IntPtr handle);

		MonoBtlsX509Store store;
		MonoBtlsX509LookupType type;
		List<MonoBtlsX509LookupMono> monoLookups;

		static BoringX509LookupHandle Create_internal (MonoBtlsX509Store store, MonoBtlsX509LookupType type)
		{
			var handle = mono_btls_x509_lookup_new (
				store.Handle.DangerousGetHandle (), type);
			if (handle == IntPtr.Zero)
				throw new MonoBtlsException ();
			return new BoringX509LookupHandle (handle);
		}

		internal MonoBtlsX509Lookup (MonoBtlsX509Store store, MonoBtlsX509LookupType type)
			: base (Create_internal (store, type))
		{
			this.store = store;
			this.type = type;
		}

		internal IntPtr GetNativeLookup ()
		{
			return mono_btls_x509_lookup_peek_lookup (Handle.DangerousGetHandle ());
		}

		public void LoadFile (string file, MonoBtlsX509FileType type)
		{
			IntPtr filePtr = IntPtr.Zero;
			try {
				if (file != null)
					filePtr = Marshal.StringToHGlobalAnsi (file);
				var ret = mono_btls_x509_lookup_load_file (
					Handle.DangerousGetHandle (), filePtr, type);
				CheckError (ret);
			} finally {
				if (filePtr != IntPtr.Zero)
					Marshal.FreeHGlobal (filePtr);
			}
		}

		public void AddDirectory (string dir, MonoBtlsX509FileType type)
		{
			IntPtr dirPtr = IntPtr.Zero;
			try {
				if (dir != null)
					dirPtr = Marshal.StringToHGlobalAnsi (dir);
				var ret = mono_btls_x509_lookup_add_dir (
					Handle.DangerousGetHandle (), dirPtr, type);
				CheckError (ret);
			} finally {
				if (dirPtr != IntPtr.Zero)
					Marshal.FreeHGlobal (dirPtr);
			}
		}

		// Takes ownership of the 'monoLookup'.
		internal void AddMono (MonoBtlsX509LookupMono monoLookup)
		{
			if (type != MonoBtlsX509LookupType.MONO)
				throw new NotSupportedException ();
			var ret = mono_btls_x509_lookup_add_mono (
				Handle.DangerousGetHandle (), monoLookup.Handle.DangerousGetHandle ());
			CheckError (ret);
			monoLookup.Install (this);

			if (monoLookups == null)
				monoLookups = new List<MonoBtlsX509LookupMono> ();
			monoLookups.Add (monoLookup);
		}

		public void Initialize ()
		{
			var ret = mono_btls_x509_lookup_init (Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public void Shutdown ()
		{
			var ret = mono_btls_x509_lookup_shutdown (Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public MonoBtlsX509 LookupBySubject (MonoBtlsX509Name name)
		{
			var handle = mono_btls_x509_lookup_by_subject (
				Handle.DangerousGetHandle (),
				name.Handle.DangerousGetHandle ());
			if (handle == IntPtr.Zero)
				return null;
			return new MonoBtlsX509 (new MonoBtlsX509.BoringX509Handle (handle));
		}

		public MonoBtlsX509 LookupByFingerPrint (byte[] fingerprint)
		{
			var bytes = Marshal.AllocHGlobal (fingerprint.Length);
			try {
				Marshal.Copy (fingerprint, 0, bytes, fingerprint.Length);
				var handle = mono_btls_x509_lookup_by_fingerprint (
					Handle.DangerousGetHandle (),
					bytes, fingerprint.Length);
				if (handle == IntPtr.Zero)
					return null;
				return new MonoBtlsX509 (new MonoBtlsX509.BoringX509Handle (handle));
			} finally {
				if (bytes != IntPtr.Zero)
					Marshal.FreeHGlobal (bytes);
			}
		}

		internal void AddCertificate (MonoBtlsX509 certificate)
		{
			store.AddCertificate (certificate);
		}

		protected override void Close ()
		{
			try {
				if (monoLookups != null) {
					foreach (var monoLookup in monoLookups)
						monoLookup.Dispose ();
				monoLookups = null;
				}
			} finally {
				base.Close ();
			}
		}
	}
}
#endif
