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
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using XamCore.ObjCRuntime;
using XamCore.CoreFoundation;
using XamCore.Foundation;

namespace XamCore.Security {
	public partial class SecTrust : INativeObject, IDisposable {
		IntPtr handle;

		public SecTrust (IntPtr handle) 
			: this (handle, false)
		{
		}

		[Preserve (Conditional=true)]
		internal SecTrust (IntPtr handle, bool owns)
		{
			if (handle == IntPtr.Zero)
				throw new Exception ("Invalid handle");

			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

#if !COREBUILD

		[DllImport (Constants.SecurityLibrary, EntryPoint="SecTrustGetTypeID")]
		public extern static nint GetTypeID ();

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecTrustCreateWithCertificates (
			/* CFTypeRef */            IntPtr certOrCertArray,
			/* CFTypeRef __nullable */ IntPtr policies,
			/* SecTrustRef *__nonull */ out IntPtr sectrustref);
		

		public SecTrust (X509Certificate certificate, SecPolicy policy)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			using (SecCertificate cert = new SecCertificate (certificate)) {
				Initialize (cert.Handle, policy);
			}
		}

		public SecTrust (X509Certificate2 certificate, SecPolicy policy)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			using (SecCertificate cert = new SecCertificate (certificate)) {
				Initialize (cert.Handle, policy);
			}
		}

		public SecTrust (X509CertificateCollection certificates, SecPolicy policy)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			SecCertificate[] array = new SecCertificate [certificates.Count];
			int i = 0;
			foreach (var certificate in certificates)
				array [i++] = new SecCertificate (certificate);
			Initialize (array, policy);
		}

		public SecTrust (X509Certificate2Collection certificates, SecPolicy policy)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			SecCertificate[] array = new SecCertificate [certificates.Count];
			int i = 0;
			foreach (var certificate in certificates)
				array [i++] = new SecCertificate (certificate);
			Initialize (array, policy);
		}

		void Initialize (SecCertificate[] array, SecPolicy policy)
		{
			using (var certs = CFArray.FromNativeObjects (array)) {
				Initialize (certs.Handle, policy);
			}
		}

		void Initialize (IntPtr certHandle, SecPolicy policy)
		{
			SecStatusCode result = SecTrustCreateWithCertificates (certHandle, policy == null ? IntPtr.Zero : policy.Handle, out handle);
			if (result != SecStatusCode.Success)
				throw new ArgumentException (result.ToString ());
		}

		[DllImport (Constants.SecurityLibrary)]
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

		[DllImport (Constants.SecurityLibrary)]
		extern static nint /* CFIndex */ SecTrustGetCertificateCount (IntPtr /* SecTrustRef */ trust);

		public int Count {
			get {
				if (handle == IntPtr.Zero)
					return 0;
				return (int) SecTrustGetCertificateCount (handle);
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr /* SecCertificateRef */ SecTrustGetCertificateAtIndex (IntPtr /* SecTrustRef */ trust, nint /* CFIndex */ ix);

		public SecCertificate this [nint index] {
			get {
				if (handle == IntPtr.Zero)
					throw new ObjectDisposedException ("SecTrust");
				if ((index < 0) || (index >= Count))
					throw new ArgumentOutOfRangeException ("index");

				return new SecCertificate (SecTrustGetCertificateAtIndex (handle, index));
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr /* SecKeyRef */ SecTrustCopyPublicKey (IntPtr /* SecTrustRef */ trust);

		public SecKey GetPublicKey ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");

			return new SecKey (SecTrustCopyPublicKey (handle), true);
		}

		[Mac (10,9)]
		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr /* CFDataRef */ SecTrustCopyExceptions (IntPtr /* SecTrustRef */ trust);

		[Mac (10,9)]
		public NSData GetExceptions ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");

			return new NSData (SecTrustCopyExceptions (handle), false); // inverted boolean?
		}

		[Mac (10,9)]
		[DllImport (Constants.SecurityLibrary)]
		extern static bool SecTrustSetExceptions (IntPtr /* SecTrustRef */ trust, IntPtr /* __nullable CFDataRef */ exceptions);

		[Mac (10,9)]
		public bool SetExceptions (NSData data)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");

			IntPtr p = data == null ? IntPtr.Zero : data.Handle;
			return SecTrustSetExceptions (handle, p);
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static double /* CFAbsoluteTime */ SecTrustGetVerifyTime (IntPtr /* SecTrustRef */ trust);

		public double GetVerifyTime ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");

			return SecTrustGetVerifyTime (handle);
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode /* OSStatus */ SecTrustSetVerifyDate (IntPtr /* SecTrustRef */ trust, IntPtr /* CFDateRef */ verifyDate);

		public SecStatusCode SetVerifyDate (DateTime date)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");

			// CFDateRef amd NSDate are toll-freee bridged
			using (NSDate d = (NSDate) date) {
				return SecTrustSetVerifyDate (handle, d.Handle);
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode /* OSStatus */ SecTrustSetAnchorCertificates (IntPtr /* SecTrustRef */ trust, IntPtr /* CFArrayRef */ anchorCertificates);

		public SecStatusCode SetAnchorCertificates (X509CertificateCollection certificates)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");
			if (certificates == null)
				return SecTrustSetAnchorCertificates (handle, IntPtr.Zero);

			SecCertificate[] array = new SecCertificate [certificates.Count];
			int i = 0;
			foreach (var certificate in certificates)
				array [i++] = new SecCertificate (certificate);
			return SetAnchorCertificates (array);
		}

		public SecStatusCode SetAnchorCertificates (X509Certificate2Collection certificates)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");
			if (certificates == null)
				return SecTrustSetAnchorCertificates (handle, IntPtr.Zero);

			SecCertificate[] array = new SecCertificate [certificates.Count];
			int i = 0;
			foreach (var certificate in certificates)
				array [i++] = new SecCertificate (certificate);
			return SetAnchorCertificates (array);
		}

		public SecStatusCode SetAnchorCertificates (SecCertificate[] array)
		{
			if (array == null)
				return SecTrustSetAnchorCertificates (handle, IntPtr.Zero);
			using (var certs = CFArray.FromNativeObjects (array)) {
				return SecTrustSetAnchorCertificates (handle, certs.Handle);
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode /* OSStatus */ SecTrustSetAnchorCertificatesOnly (IntPtr /* SecTrustRef */ trust, bool anchorCertificatesOnly);

		public SecStatusCode SetAnchorCertificatesOnly (bool anchorCertificatesOnly)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecTrust");

			return SecTrustSetAnchorCertificatesOnly (handle, anchorCertificatesOnly);
		}
#endif


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
