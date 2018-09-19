//
// MonoOpenSSLX509LookupMono.cs
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
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.OpenSSL
{
	abstract class MonoOpenSSLX509LookupMono : MonoOpenSSLObject
	{
		internal class OpenSSLX509LookupMonoHandle : MonoOpenSSLHandle
		{
			public OpenSSLX509LookupMonoHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_openssl_x509_lookup_mono_free (handle);
				return true;
			}
		}

		new internal OpenSSLX509LookupMonoHandle Handle {
			get { return (OpenSSLX509LookupMonoHandle)base.Handle; }
		}

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_lookup_mono_new ();

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_x509_lookup_mono_init (
			IntPtr handle, IntPtr instance, IntPtr by_subject_func);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_lookup_mono_free (IntPtr handle);

		delegate int BySubjectFunc (IntPtr instance, IntPtr name, out IntPtr x509_ptr);

		GCHandle gch;
		IntPtr instance;
		BySubjectFunc bySubjectFunc;
		IntPtr bySubjectFuncPtr;
		MonoOpenSSLX509Lookup lookup;

		internal MonoOpenSSLX509LookupMono ()
			: base (new OpenSSLX509LookupMonoHandle (mono_openssl_x509_lookup_mono_new ()))
		{
			gch = GCHandle.Alloc (this);
			instance = GCHandle.ToIntPtr (gch);
			bySubjectFunc = OnGetBySubject;
			bySubjectFuncPtr = Marshal.GetFunctionPointerForDelegate (bySubjectFunc);
			mono_openssl_x509_lookup_mono_init (Handle.DangerousGetHandle (), instance, bySubjectFuncPtr);
		}

		internal void Install (MonoOpenSSLX509Lookup lookup)
		{
			if (this.lookup != null)
				throw new InvalidOperationException ();
			this.lookup = lookup;
		}

		protected void AddCertificate (MonoOpenSSLX509 certificate)
		{
			lookup.AddCertificate (certificate);
		}

		protected abstract MonoOpenSSLX509 OnGetBySubject (MonoOpenSSLX509Name name);

		[Mono.Util.MonoPInvokeCallback (typeof (BySubjectFunc))]
		static int OnGetBySubject (IntPtr instance, IntPtr name_ptr, out IntPtr x509_ptr)
		{
			try {
				MonoOpenSSLX509LookupMono obj;
				MonoOpenSSLX509Name.OpenSSLX509NameHandle name_handle = null;
				try {
					obj = (MonoOpenSSLX509LookupMono)GCHandle.FromIntPtr (instance).Target;
					name_handle = new MonoOpenSSLX509Name.OpenSSLX509NameHandle (name_ptr, false);
					MonoOpenSSLX509Name name_obj = new MonoOpenSSLX509Name (name_handle);
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
