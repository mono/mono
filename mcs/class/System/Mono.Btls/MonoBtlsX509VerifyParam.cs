//
// MonoBtlsX509VerifyParam.cs
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

namespace Mono.Btls
{
	class MonoBtlsX509VerifyParam : MonoBtlsObject
	{
		internal class BoringX509VerifyParamHandle : MonoBtlsHandle
		{
			public BoringX509VerifyParamHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_btls_x509_verify_param_free (handle);
				return true;
			}
		}

		new internal BoringX509VerifyParamHandle Handle {
			get { return (BoringX509VerifyParamHandle)base.Handle; }
		}

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_verify_param_new ();

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_verify_param_copy (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_verify_param_lookup (IntPtr name);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_can_modify (IntPtr param);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_set_name (IntPtr handle, IntPtr name);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_set_host (IntPtr handle, IntPtr name, int namelen);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_add_host (IntPtr handle, IntPtr name, int namelen);

		[DllImport (BTLS_DYLIB)]
		extern static ulong mono_btls_x509_verify_param_get_flags (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_set_flags (IntPtr handle, ulong flags);

		[DllImport (BTLS_DYLIB)]
		extern static MonoBtlsX509VerifyFlags mono_btls_x509_verify_param_get_mono_flags (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_set_mono_flags (IntPtr handle, MonoBtlsX509VerifyFlags flags);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_set_purpose (IntPtr handle, MonoBtlsX509Purpose purpose);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_get_depth (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_set_depth (IntPtr handle, int depth);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_verify_param_set_time (IntPtr handle, long time);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_verify_param_get_peername (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_x509_verify_param_free (IntPtr handle);

		internal MonoBtlsX509VerifyParam ()
			: base (new BoringX509VerifyParamHandle (mono_btls_x509_verify_param_new ()))
		{
		}

		internal MonoBtlsX509VerifyParam (BoringX509VerifyParamHandle handle)
			: base (handle)
		{
		}

		public MonoBtlsX509VerifyParam Copy ()
		{
			var copy = mono_btls_x509_verify_param_copy (Handle.DangerousGetHandle ());
			CheckError (copy != IntPtr.Zero);
			return new MonoBtlsX509VerifyParam (new BoringX509VerifyParamHandle (copy));
		}

		public static MonoBtlsX509VerifyParam GetSslClient ()
		{
			return Lookup ("ssl_client", true);
		}

		public static MonoBtlsX509VerifyParam GetSslServer ()
		{
			return Lookup ("ssl_server", true);
		}

		public static MonoBtlsX509VerifyParam Lookup (string name, bool fail = false)
		{
			IntPtr namePtr = IntPtr.Zero;
			IntPtr handle = IntPtr.Zero;

			try {
				namePtr = Marshal.StringToHGlobalAnsi (name);
				handle = mono_btls_x509_verify_param_lookup (namePtr);
				if (handle == IntPtr.Zero) {
					if (!fail)
						return null;
					throw new MonoBtlsException ("X509_VERIFY_PARAM_lookup() could not find '{0}'.", name);
				}

				return new MonoBtlsX509VerifyParam (new BoringX509VerifyParamHandle (handle));
			} finally {
				if (namePtr != IntPtr.Zero)
					Marshal.FreeHGlobal (namePtr);
			}
		}

		public bool CanModify {
			get {
				return mono_btls_x509_verify_param_can_modify (Handle.DangerousGetHandle ()) != 0;
			}
		}

		void WantToModify ()
		{
			if (!CanModify)
				throw new MonoBtlsException ("Attempting to modify read-only MonoBtlsX509VerifyParam instance.");
		}

		public void SetName (string name)
		{
			WantToModify ();
			IntPtr namePtr = IntPtr.Zero;
			try {
				namePtr = Marshal.StringToHGlobalAnsi (name);
				var ret = mono_btls_x509_verify_param_set_name (
					Handle.DangerousGetHandle (), namePtr);
				CheckError (ret);
			} finally {
				if (namePtr != IntPtr.Zero)
					Marshal.FreeHGlobal (namePtr);
			}
		}

		public void SetHost (string name)
		{
			WantToModify ();
			IntPtr namePtr = IntPtr.Zero;
			try {
				namePtr = Marshal.StringToHGlobalAnsi (name);
				var ret = mono_btls_x509_verify_param_set_host (
					Handle.DangerousGetHandle (), namePtr, name.Length);
				CheckError (ret);
			} finally {
				if (namePtr != IntPtr.Zero)
					Marshal.FreeHGlobal (namePtr);
			}
		}

		public void AddHost (string name)
		{
			WantToModify ();
			IntPtr namePtr = IntPtr.Zero;
			try {
				namePtr = Marshal.StringToHGlobalAnsi (name);
				var ret = mono_btls_x509_verify_param_add_host (
					Handle.DangerousGetHandle (), namePtr, name.Length);
				CheckError (ret);
			} finally {
				if (namePtr != IntPtr.Zero)
					Marshal.FreeHGlobal (namePtr);
			}
		}

		public ulong GetFlags ()
		{
			return mono_btls_x509_verify_param_get_flags (Handle.DangerousGetHandle ());
		}

		public void SetFlags (ulong flags)
		{
			WantToModify ();
			var ret = mono_btls_x509_verify_param_set_flags (
				Handle.DangerousGetHandle (), flags);
			CheckError (ret);
		}

		public MonoBtlsX509VerifyFlags GetMonoFlags ()
		{
			return mono_btls_x509_verify_param_get_mono_flags (
				Handle.DangerousGetHandle ());
		}

		public void SetMonoFlags (MonoBtlsX509VerifyFlags flags)
		{
			WantToModify ();
			var ret = mono_btls_x509_verify_param_set_mono_flags (
				Handle.DangerousGetHandle (), flags);
			CheckError (ret);
		}

		public void SetPurpose (MonoBtlsX509Purpose purpose)
		{
			WantToModify ();
			var ret = mono_btls_x509_verify_param_set_purpose (
				Handle.DangerousGetHandle (), purpose);
			CheckError (ret);
		}

		public int GetDepth ()
		{
			return mono_btls_x509_verify_param_get_depth (Handle.DangerousGetHandle ());
		}

		public void SetDepth (int depth)
		{
			WantToModify ();
			var ret = mono_btls_x509_verify_param_set_depth (
				Handle.DangerousGetHandle (), depth);
			CheckError (ret);
		}

		public void SetTime (DateTime time)
		{
			WantToModify ();
			var epoch = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var ticks = (long)time.Subtract (epoch).TotalSeconds;
			var ret = mono_btls_x509_verify_param_set_time (
				Handle.DangerousGetHandle (), ticks);
			CheckError (ret);
		}

		public string GetPeerName ()
		{
			var peer = mono_btls_x509_verify_param_get_peername (Handle.DangerousGetHandle ());
			if (peer == IntPtr.Zero)
				return null;
			return Marshal.PtrToStringAnsi (peer);
		}
	}
}
#endif
