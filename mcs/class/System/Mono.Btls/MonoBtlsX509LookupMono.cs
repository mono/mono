//
// MonoBtlsX509LookupMono.cs
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
	abstract class MonoBtlsX509LookupMono : MonoBtlsObject
	{
		internal class BoringX509LookupMonoHandle : MonoBtlsHandle
		{
			public BoringX509LookupMonoHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_btls_x509_lookup_mono_free (handle);
				return true;
			}
		}

		new internal BoringX509LookupMonoHandle Handle {
			get { return (BoringX509LookupMonoHandle)base.Handle; }
		}

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_lookup_mono_new ();

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_x509_lookup_mono_init (
			IntPtr handle, IntPtr instance, IntPtr by_subject_func);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_lookup_mono_free (IntPtr handle);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate int BySubjectFunc (IntPtr instance, IntPtr name, out IntPtr x509_ptr);

		GCHandle gch;
		IntPtr instance;
		BySubjectFunc bySubjectFunc;
		IntPtr bySubjectFuncPtr;
		MonoBtlsX509Lookup lookup;

		internal MonoBtlsX509LookupMono ()
			: base (new BoringX509LookupMonoHandle (mono_btls_x509_lookup_mono_new ()))
		{
			gch = GCHandle.Alloc (this);
			instance = GCHandle.ToIntPtr (gch);
			bySubjectFunc = OnGetBySubject;
			bySubjectFuncPtr = Marshal.GetFunctionPointerForDelegate (bySubjectFunc);
			mono_btls_x509_lookup_mono_init (Handle.DangerousGetHandle (), instance, bySubjectFuncPtr);
		}

		internal void Install (MonoBtlsX509Lookup lookup)
		{
			if (this.lookup != null)
				throw new InvalidOperationException ();
			this.lookup = lookup;
		}

		protected void AddCertificate (MonoBtlsX509 certificate)
		{
			lookup.AddCertificate (certificate);
		}

		protected abstract MonoBtlsX509 OnGetBySubject (MonoBtlsX509Name name);

		[Mono.Util.MonoPInvokeCallback (typeof (BySubjectFunc))]
		static int OnGetBySubject (IntPtr instance, IntPtr name_ptr, out IntPtr x509_ptr)
		{
			try {
				MonoBtlsX509LookupMono obj;
				MonoBtlsX509Name.BoringX509NameHandle name_handle = null;
				try {
					obj = (MonoBtlsX509LookupMono)GCHandle.FromIntPtr (instance).Target;
					name_handle = new MonoBtlsX509Name.BoringX509NameHandle (name_ptr, false);
					MonoBtlsX509Name name_obj = new MonoBtlsX509Name (name_handle);
					var x509 = obj.OnGetBySubject (name_obj);
					if (x509 != null) {
						x509_ptr = x509.Handle.StealHandle ();
						return 1;
					} else {
						x509_ptr = IntPtr.Zero;
						return 0;
					}
				} finally {
					if (name_handle != null)
						name_handle.Dispose ();
				}
			} catch (Exception ex) {
				Console.WriteLine ("LOOKUP METHOD - GET BY SUBJECT EX: {0}", ex);
				x509_ptr = IntPtr.Zero;
				return 0;
			}
		}

		protected override void Close ()
		{
			try {
				if (gch.IsAllocated)
					gch.Free ();
			} finally {
				instance = IntPtr.Zero;
				bySubjectFunc = null;
				bySubjectFuncPtr = IntPtr.Zero;
				base.Close ();
			}
		}
	}
}
#endif
