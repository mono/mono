// 
// Trust.cs: Implements the managed SecTrust wrapper.
//
// Authors: 
//	Miguel de Icaza
//  Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2010 Novell, Inc
// Copyright 2012-2014 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using ObjCRuntimeInternal;
using Mono.Net;

namespace Mono.AppleTls {
	partial class SecTrust : INativeObject, IDisposable {
		IntPtr handle;

		internal SecTrust (IntPtr handle, bool owns = false)
		{
			if (handle == IntPtr.Zero)
				throw new Exception ("Invalid handle");

			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static SecStatusCode SecTrustCreateWithCertificates (
			/* CFTypeRef */            IntPtr certOrCertArray,
			/* CFTypeRef __nullable */ IntPtr policies,
			/* SecTrustRef *__nonull */ out IntPtr sectrustref);
		

		public SecTrust (X509CertificateCollection certificates, SecPolicy policy)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			var array = new SafeSecCertificateHandle [certificates.Count];
			int i = 0;
			foreach (var certificate in certificates)
				array [i++] = MonoCertificatePal.FromOtherCertificate (certificate);
			Initialize (array, policy);
			for (i = 0; i < array.Length; i++)
				array [i].Dispose ();
		}

		void Initialize (SafeSecCertificateHandle[] array, SecPolicy policy)
		{
			var handles = new IntPtr [array.Length];
			for (int i = 0; i < array.Length; i++)
				handles [i] = array [i].DangerousGetHandle ();
			using (var certs = CFArray.CreateArray (handles)) {
				Initialize (certs.Handle, policy);
			}
		}

		void Initialize (IntPtr certHandle, SecPolicy policy)
		{
			SecStatusCode result = SecTrustCreateWithCertificates (certHandle, policy == null ? IntPtr.Zero : policy.Handle, out handle);
			if (result != SecStatusCode.Success)
				throw new ArgumentException (result.ToString ());
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static SecStatusCode /* OSStatus */ SecTrustEvaluate (IntPtr /* SecTrustRef */ trust, out /* SecTrustResultType */ SecTrustResult result);

		public SecTrustResult Evaluate ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");

			SecTrustResult trust;
			SecStatusCode result = SecTrustEvaluate (handle, out trust);
			if (result != SecStatusCode.Success)
				throw new InvalidOperationException (result.ToString ());
			return trust;
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static IntPtr /* CFIndex */ SecTrustGetCertificateCount (IntPtr /* SecTrustRef */ trust);

		public int Count {
			get {
				if (handle == IntPtr.Zero)
					return 0;
				return (int) SecTrustGetCertificateCount (handle);
			}
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static IntPtr /* SecCertificateRef */ SecTrustGetCertificateAtIndex (IntPtr /* SecTrustRef */ trust, IntPtr /* CFIndex */ ix);

		internal X509Certificate2 GetCertificate (int index)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException ("index");

			var ptr = SecTrustGetCertificateAtIndex (handle, (IntPtr)index);
			var impl = new X509CertificateImplApple (ptr, false);
			return new X509Certificate2 (impl);
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static SecStatusCode /* OSStatus */ SecTrustSetAnchorCertificates (IntPtr /* SecTrustRef */ trust, IntPtr /* CFArrayRef */ anchorCertificates);

		public SecStatusCode SetAnchorCertificates (X509CertificateCollection certificates)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");
			if (certificates == null)
				return SecTrustSetAnchorCertificates (handle, IntPtr.Zero);

			var array = new SafeSecCertificateHandle [certificates.Count];
			int i = 0;
			foreach (var certificate in certificates)
				array [i++] = MonoCertificatePal.FromOtherCertificate (certificate);
			return SetAnchorCertificates (array);
		}

		public SecStatusCode SetAnchorCertificates (SafeSecCertificateHandle[] array)
		{
			if (array == null)
				return SecTrustSetAnchorCertificates (handle, IntPtr.Zero);
			var handles = new IntPtr [array.Length];
			for (int i = 0; i < array.Length; i++)
				handles [i] = array [i].DangerousGetHandle ();
			using (var certs = CFArray.CreateArray (handles)) {
				return SecTrustSetAnchorCertificates (handle, certs.Handle);
			}
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static SecStatusCode /* OSStatus */ SecTrustSetAnchorCertificatesOnly (IntPtr /* SecTrustRef */ trust, bool anchorCertificatesOnly);

		public SecStatusCode SetAnchorCertificatesOnly (bool anchorCertificatesOnly)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");

			return SecTrustSetAnchorCertificatesOnly (handle, anchorCertificatesOnly);
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static SecStatusCode /* OSStatus */ SecTrustSetVerifyDate (IntPtr /* SecTrustRef */ trust, IntPtr /* CFDateRef */ date);

		public SecStatusCode SetVerifyDate (DateTime date)
		{
			using (var nativeDate = CFDate.Create (date))
				return SecTrustSetVerifyDate (handle, nativeDate.Handle);
		}

		~SecTrust ()
		{
			Dispose (false);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public IntPtr Handle {
			get { return handle; }
		}
	}
}
