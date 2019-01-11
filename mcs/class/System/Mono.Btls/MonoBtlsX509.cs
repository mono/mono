//
// MonoBtlsX509.cs
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
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace Mono.Btls
{
	class MonoBtlsX509 : MonoBtlsObject
	{
		internal class BoringX509Handle : MonoBtlsHandle
		{
			public BoringX509Handle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				if (handle != IntPtr.Zero)
					mono_btls_x509_free (handle);
				return true;
			}

			public IntPtr StealHandle ()
			{
				var retval = Interlocked.Exchange (ref handle, IntPtr.Zero);
				return retval;
			}
		}

		new internal BoringX509Handle Handle {
			get { return (BoringX509Handle)base.Handle; }
		}

		internal MonoBtlsX509 (BoringX509Handle handle) 
			: base (handle)
		{
		}

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_up_ref (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_from_data (IntPtr data, int len, MonoBtlsX509Format format);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_get_subject_name (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_get_issuer_name (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_subject_name_string (IntPtr handle, IntPtr buffer, int size);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_issuer_name_string (IntPtr handle, IntPtr buffer, int size);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_raw_data (IntPtr handle, IntPtr bio, MonoBtlsX509Format format);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_cmp (IntPtr a, IntPtr b);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_hash (IntPtr handle, out IntPtr data);

		[DllImport (BTLS_DYLIB)]
		extern static long mono_btls_x509_get_not_before (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static long mono_btls_x509_get_not_after (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_public_key (IntPtr handle, IntPtr bio);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_serial_number (IntPtr handle, IntPtr data, int size, int mono_style);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_version (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_signature_algorithm (IntPtr handle, IntPtr buffer, int size);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_public_key_asn1 (IntPtr handle, IntPtr oid, int oid_size, out IntPtr data, out int size);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_public_key_parameters (IntPtr handle, IntPtr oid, int oid_size, out IntPtr data, out int size);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_get_pubkey (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_get_subject_key_identifier (IntPtr handle, out IntPtr data, out int size);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_print (IntPtr handle, IntPtr bio);

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_x509_free (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_dup (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_add_trust_object (IntPtr handle, MonoBtlsX509Purpose purpose);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_add_reject_object (IntPtr handle, MonoBtlsX509Purpose purpose);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_add_explicit_trust (IntPtr handle, MonoBtlsX509TrustKind kind);

		internal MonoBtlsX509 Copy ()
		{
			var copy = mono_btls_x509_up_ref (Handle.DangerousGetHandle ());
			CheckError (copy != IntPtr.Zero);
			return new MonoBtlsX509 (new BoringX509Handle (copy));
		}

		// This will actually duplicate the underlying 'X509 *' object instead of
		// simply increasing the reference count.
		internal MonoBtlsX509 Duplicate ()
		{
			var copy = mono_btls_x509_dup (Handle.DangerousGetHandle ());
			CheckError (copy != IntPtr.Zero);
			return new MonoBtlsX509 (new BoringX509Handle (copy));
		}

		public static MonoBtlsX509 LoadFromData (byte[] buffer, MonoBtlsX509Format format)
		{
			var data = Marshal.AllocHGlobal (buffer.Length);
			if (data == IntPtr.Zero)
				throw new OutOfMemoryException ();

			try {
				Marshal.Copy (buffer, 0, data, buffer.Length);
				var x509 = mono_btls_x509_from_data (data, buffer.Length, format);
				if (x509 == IntPtr.Zero)
					throw new MonoBtlsException ("Failed to read certificate from data.");

				return new MonoBtlsX509 (new BoringX509Handle (x509));
			} finally {
				Marshal.FreeHGlobal (data);
			}
		}

		public MonoBtlsX509Name GetSubjectName ()
		{
			var handle = mono_btls_x509_get_subject_name (Handle.DangerousGetHandle ());
			CheckError (handle != IntPtr.Zero);
			return new MonoBtlsX509Name (new MonoBtlsX509Name.BoringX509NameHandle (handle, false));
		}

		public string GetSubjectNameString ()
		{
			const int size = 4096;
			var data = Marshal.AllocHGlobal (size);
			try {
				var ret = mono_btls_x509_get_subject_name_string (
					Handle.DangerousGetHandle (), data, size);
				CheckError (ret);
				return Marshal.PtrToStringAnsi (data);
			} finally {
				Marshal.FreeHGlobal (data);
			}
		}

		public long GetSubjectNameHash ()
		{
			CheckThrow ();
			using (var subject = GetSubjectName ())
				return subject.GetHash ();
		}

		public MonoBtlsX509Name GetIssuerName ()
		{
			var handle = mono_btls_x509_get_issuer_name (Handle.DangerousGetHandle ());
			CheckError (handle != IntPtr.Zero);
			return new MonoBtlsX509Name (new MonoBtlsX509Name.BoringX509NameHandle (handle, false));
		}

		public string GetIssuerNameString ()
		{
			const int size = 4096;
			var data = Marshal.AllocHGlobal (size);
			try {
				var ret = mono_btls_x509_get_issuer_name_string (
					Handle.DangerousGetHandle (), data, size);
				CheckError (ret);
				return Marshal.PtrToStringAnsi (data);
			} finally {
				Marshal.FreeHGlobal (data);
			}
		}

		public byte[] GetRawData (MonoBtlsX509Format format)
		{
			using (var bio = new MonoBtlsBioMemory ()) {
				var ret = mono_btls_x509_get_raw_data (
					Handle.DangerousGetHandle (),
					bio.Handle.DangerousGetHandle (),
					format);
				CheckError (ret);
				return bio.GetData ();
			}
		}

		public void GetRawData (MonoBtlsBio bio, MonoBtlsX509Format format)
		{
			CheckThrow ();
			var ret = mono_btls_x509_get_raw_data (
				Handle.DangerousGetHandle (),
				bio.Handle.DangerousGetHandle (),
				format);
			CheckError (ret);
		}

		public static int Compare (MonoBtlsX509 a, MonoBtlsX509 b)
		{
			return mono_btls_x509_cmp (
				a.Handle.DangerousGetHandle (),
				b.Handle.DangerousGetHandle ());
		}

		public byte[] GetCertHash ()
		{
			IntPtr data;
			var ret = mono_btls_x509_get_hash (Handle.DangerousGetHandle (), out data);
			CheckError (ret > 0);
			var buffer = new byte [ret];
			Marshal.Copy (data, buffer, 0, ret);
			return buffer;
		}

		public DateTime GetNotBefore ()
		{
			var ticks = mono_btls_x509_get_not_before (Handle.DangerousGetHandle ());
			return new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds (ticks);
		}

		public DateTime GetNotAfter ()
		{
			var ticks = mono_btls_x509_get_not_after (Handle.DangerousGetHandle ());
			return new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds (ticks);
		}

		public byte[] GetPublicKeyData ()
		{
			using (var bio = new MonoBtlsBioMemory ()) {
				var ret = mono_btls_x509_get_public_key (
					Handle.DangerousGetHandle (),
					bio.Handle.DangerousGetHandle ());
				CheckError (ret > 0);
				return bio.GetData ();
			}
		}

		public byte[] GetSerialNumber (bool mono_style)
		{
			int size = 256;
			IntPtr data = Marshal.AllocHGlobal (size);
			try {
				var ret = mono_btls_x509_get_serial_number (
					Handle.DangerousGetHandle (), data,
					size, mono_style ? 1 : 0);
				CheckError (ret > 0);
				var buffer = new byte [ret];
				Marshal.Copy (data, buffer, 0, ret);
				return buffer;
			} finally {
				if (data != IntPtr.Zero)
					Marshal.FreeHGlobal (data);
			}
		}

		public int GetVersion ()
		{
			return mono_btls_x509_get_version (Handle.DangerousGetHandle ());
		}

		public string GetSignatureAlgorithm ()
		{
			int size = 256;
			IntPtr data = Marshal.AllocHGlobal (size);
			try {
				var ret = mono_btls_x509_get_signature_algorithm (
					Handle.DangerousGetHandle (), data, size);
				CheckError (ret > 0);
				return Marshal.PtrToStringAnsi (data);
			} finally {
				Marshal.FreeHGlobal (data);
			}
		}

		public AsnEncodedData GetPublicKeyAsn1 ()
		{
			int size;
			IntPtr data;

			int oidSize = 256;
			var oidData = Marshal.AllocHGlobal (256);
			string oid;

			try {
				var ret = mono_btls_x509_get_public_key_asn1 (
					Handle.DangerousGetHandle (), oidData, oidSize,
					out data, out size);
				CheckError (ret);
				oid = Marshal.PtrToStringAnsi (oidData);
			} finally {
				Marshal.FreeHGlobal (oidData);
			}

			try {
				var buffer = new byte[size];
				Marshal.Copy (data, buffer, 0, size);
				return new AsnEncodedData (oid.ToString (), buffer);
			} finally {
				if (data != IntPtr.Zero)
					FreeDataPtr (data);
			}
		}

		public AsnEncodedData GetPublicKeyParameters ()
		{
			int size;
			IntPtr data;

			int oidSize = 256;
			var oidData = Marshal.AllocHGlobal (256);
			string oid;

			try {
				var ret = mono_btls_x509_get_public_key_parameters (
					Handle.DangerousGetHandle (), oidData, oidSize,
					out data, out size);
				CheckError (ret);
				oid = Marshal.PtrToStringAnsi (oidData);
			} finally {
				Marshal.FreeHGlobal (oidData);
			}

			try {
				var buffer = new byte[size];
				Marshal.Copy (data, buffer, 0, size);
				return new AsnEncodedData (oid.ToString (), buffer);
			} finally {
				if (data != IntPtr.Zero)
					FreeDataPtr (data);
			}
		}

		public byte[] GetSubjectKeyIdentifier ()
		{
			int size;
			IntPtr data = IntPtr.Zero;

			try {
				var ret = mono_btls_x509_get_subject_key_identifier (
					Handle.DangerousGetHandle (), out data, out size);
				CheckError (ret);
				var buffer = new byte[size];
				Marshal.Copy (data, buffer, 0, size);
				return buffer;
			} finally {
				if (data != IntPtr.Zero)
					FreeDataPtr (data);
			}
		}

		public MonoBtlsKey GetPublicKey ()
		{
			var handle = mono_btls_x509_get_pubkey (Handle.DangerousGetHandle ());
			CheckError (handle != IntPtr.Zero);
			return new MonoBtlsKey (new MonoBtlsKey.BoringKeyHandle (handle));
		}

		public void Print (MonoBtlsBio bio)
		{
			var ret = mono_btls_x509_print (
				Handle.DangerousGetHandle (),
				bio.Handle.DangerousGetHandle ());
			CheckError (ret);
		}

		public void ExportAsPEM (MonoBtlsBio bio, bool includeHumanReadableForm)
		{
			GetRawData (bio, MonoBtlsX509Format.PEM);

			if (!includeHumanReadableForm)
				return;

			Print (bio);

			var hash = GetCertHash ();
			var output = new StringBuilder ();
			output.Append ("SHA1 Fingerprint=");
			for (int i = 0; i < hash.Length; i++) {
				if (i > 0)
					output.Append (":");
				output.AppendFormat ("{0:X2}", hash [i]);
			}
			output.AppendLine ();
			var outputData = Encoding.ASCII.GetBytes (output.ToString ());
			bio.Write (outputData, 0, outputData.Length);
		}

		public void AddTrustObject (MonoBtlsX509Purpose purpose)
		{
			CheckThrow ();
			var ret = mono_btls_x509_add_trust_object (
				Handle.DangerousGetHandle (), purpose);
			CheckError (ret);
		}

		public void AddRejectObject (MonoBtlsX509Purpose purpose)
		{
			CheckThrow ();
			var ret = mono_btls_x509_add_reject_object (
				Handle.DangerousGetHandle (), purpose);
			CheckError (ret);
		}

		public void AddExplicitTrust (MonoBtlsX509TrustKind kind)
		{
			CheckThrow ();
			var ret = mono_btls_x509_add_explicit_trust (
				Handle.DangerousGetHandle (), kind);
			CheckError (ret);
		}
	}
}
#endif
