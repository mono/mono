//
// MonoBtlsX509Name.cs
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
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.Btls
{
	class MonoBtlsX509Name : MonoBtlsObject
	{
		internal class BoringX509NameHandle : MonoBtlsHandle
		{
			bool dontFree;

			internal BoringX509NameHandle (IntPtr handle, bool ownsHandle)
				: base (handle, ownsHandle)
			{
				this.dontFree = !ownsHandle;
			}

			protected override bool ReleaseHandle ()
			{
				if (!dontFree)
					mono_btls_x509_name_free (handle);
				return true;
			}
		}

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_name_print_bio (IntPtr handle, IntPtr bio);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_name_print_string (IntPtr handle, IntPtr buffer, int size);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_name_get_raw_data (IntPtr handle, out IntPtr buffer, int use_canon_enc);

		[DllImport (BTLS_DYLIB)]
		extern static long mono_btls_x509_name_hash (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static long mono_btls_x509_name_hash_old (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_name_get_entry_count (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static MonoBtlsX509NameEntryType mono_btls_x509_name_get_entry_type (IntPtr name, int index);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_name_get_entry_oid (IntPtr name, int index, IntPtr buffer, int size);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_name_get_entry_oid_data (IntPtr name, int index, out IntPtr data);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_name_get_entry_value (IntPtr name, int index, out int tag, out IntPtr str);

		[DllImport (BTLS_DYLIB)]
		extern unsafe static IntPtr mono_btls_x509_name_from_data (void* data, int len, int use_canon_enc);

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_x509_name_free (IntPtr handle);

		new internal BoringX509NameHandle Handle {
			get { return (BoringX509NameHandle)base.Handle; }
		}

		internal MonoBtlsX509Name (BoringX509NameHandle handle)
			: base (handle)
		{
		}

		public string GetString ()
		{
			const int size = 4096;
			var data = Marshal.AllocHGlobal (size);
			try {
				var ret = mono_btls_x509_name_print_string (
					Handle.DangerousGetHandle (), data, size);
				CheckError (ret);
				return Marshal.PtrToStringAnsi (data);
			} finally {
				Marshal.FreeHGlobal (data);
			}
		}

		public void PrintBio (MonoBtlsBio bio)
		{
			var ret = mono_btls_x509_name_print_bio (
				Handle.DangerousGetHandle (),
				bio.Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public byte[] GetRawData (bool use_canon_enc)
		{
			IntPtr data;
			var ret = mono_btls_x509_name_get_raw_data (
				Handle.DangerousGetHandle (),
				out data, use_canon_enc ? 1 : 0);
			CheckError (ret > 0);
			var buffer = new byte [ret];
			Marshal.Copy (data, buffer, 0, ret);
			FreeDataPtr (data);
			return buffer;
		}

		public long GetHash ()
		{
			return mono_btls_x509_name_hash (Handle.DangerousGetHandle ());
		}

		public long GetHashOld ()
		{
			return mono_btls_x509_name_hash_old (Handle.DangerousGetHandle ());
		}

		public int GetEntryCount ()
		{
			return mono_btls_x509_name_get_entry_count (Handle.DangerousGetHandle ());
		}

		public MonoBtlsX509NameEntryType GetEntryType (int index)
		{
			if (index >= GetEntryCount ())
				throw new ArgumentOutOfRangeException ();
			return mono_btls_x509_name_get_entry_type (
				Handle.DangerousGetHandle (), index);
		}

		public string GetEntryOid (int index)
		{
			if (index >= GetEntryCount ())
				throw new ArgumentOutOfRangeException ();

			const int size = 4096;
			var data = Marshal.AllocHGlobal (size);
			try {
				var ret = mono_btls_x509_name_get_entry_oid (
					Handle.DangerousGetHandle (),
					index, data, size);
				CheckError (ret > 0);
				return Marshal.PtrToStringAnsi (data);
			} finally {
				Marshal.FreeHGlobal (data);
			}
		}

		public byte[] GetEntryOidData (int index)
		{
			IntPtr data;
			var ret = mono_btls_x509_name_get_entry_oid_data (
				Handle.DangerousGetHandle (), index, out data);
			CheckError (ret > 0);

			var bytes = new byte[ret];
			Marshal.Copy (data, bytes, 0, ret);
			return bytes;
		}

		public unsafe string GetEntryValue (int index, out int tag)
		{
			if (index >= GetEntryCount ())
				throw new ArgumentOutOfRangeException ();
			IntPtr data;
			var ret = mono_btls_x509_name_get_entry_value (
				Handle.DangerousGetHandle (), index, out tag, out data);
			if (ret <= 0)
				return null;
			try {
				return new UTF8Encoding ().GetString ((byte*)data, ret);
			} finally {
				if (data != IntPtr.Zero)
					FreeDataPtr (data);
			}
		}

		public static unsafe MonoBtlsX509Name CreateFromData (byte[] data, bool use_canon_enc)
		{
			fixed (void *ptr = data) {
				var handle = mono_btls_x509_name_from_data (ptr, data.Length, use_canon_enc ? 1 : 0);
				if (handle == IntPtr.Zero)
					throw new MonoBtlsException ("mono_btls_x509_name_from_data() failed.");
				return new MonoBtlsX509Name (new BoringX509NameHandle (handle, false));
			}
		}
	}
}
#endif
