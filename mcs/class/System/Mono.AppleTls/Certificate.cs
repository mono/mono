// 
// Certificate.cs: Implements the managed SecCertificate wrapper.
//
// Authors: 
//	Miguel de Icaza
//  Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2010 Novell, Inc
// Copyright 2012-2013 Xamarin Inc.
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

#if SECURITY_DEP && MONO_FEATURE_APPLETLS

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Mono.Net;

using ObjCRuntimeInternal;

namespace Mono.AppleTls {

	partial class SecCertificate : INativeObject, IDisposable {
		internal IntPtr handle;
		
		internal SecCertificate (IntPtr handle, bool owns = false)
		{
			if (handle == IntPtr.Zero)
				throw new Exception ("Invalid handle");

			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}
		
		[DllImport (AppleTlsContext.SecurityLibrary, EntryPoint="SecCertificateGetTypeID")]
		public extern static IntPtr GetTypeID ();
			
		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static IntPtr SecCertificateCreateWithData (IntPtr allocator, IntPtr cfData);

		public SecCertificate (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			handle = certificate.Impl.GetNativeAppleCertificate ();
			if (handle != IntPtr.Zero) {
				CFObject.CFRetain (handle);
				return;
			}

			using (CFData cert = CFData.FromData (certificate.GetRawCertData ())) {
				Initialize (cert);
			}
		}

		internal SecCertificate (X509CertificateImpl impl)
		{
			handle = impl.GetNativeAppleCertificate ();
			if (handle != IntPtr.Zero) {
				CFObject.CFRetain (handle);
				return;
			}

			using (CFData cert = CFData.FromData (impl.GetRawCertData ())) {
				Initialize (cert);
			}
		}

		void Initialize (CFData data)
		{
			handle = SecCertificateCreateWithData (IntPtr.Zero, data.Handle);
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("Not a valid DER-encoded X.509 certificate");
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static IntPtr SecCertificateCopySubjectSummary (IntPtr cert);

		public string SubjectSummary {
			get {
				if (handle == IntPtr.Zero)
					throw new ObjectDisposedException ("SecCertificate");
				
				IntPtr subjectSummaryHandle = IntPtr.Zero;
				try {
					subjectSummaryHandle = SecCertificateCopySubjectSummary (handle);
					CFString subjectSummary = CFString.AsString (subjectSummaryHandle);
					return subjectSummary;
				}
				finally {
					if (subjectSummaryHandle != IntPtr.Zero)
						CFObject.CFRelease (subjectSummaryHandle);
				}
			}
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static /* CFDataRef */ IntPtr SecCertificateCopyData (/* SecCertificateRef */ IntPtr cert);

		public CFData DerData {
			get {
				if (handle == IntPtr.Zero)
					throw new ObjectDisposedException ("SecCertificate");

				IntPtr data = SecCertificateCopyData (handle);
				if (data == IntPtr.Zero)
					throw new ArgumentException ("Not a valid certificate");
				return new CFData (data, true);
			}
		}

		public X509Certificate ToX509Certificate ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecCertificate");

			return new X509Certificate (handle);
		}

		internal static bool Equals (SecCertificate first, SecCertificate second)
		{
			/*
			 * This is a little bit expensive, but unfortunately there is no better API to compare two
			 * SecCertificateRef's for equality.
			 */
			if (first == null)
				throw new ArgumentNullException ("first");
			if (second == null)
				throw new ArgumentNullException ("second");
			if (first.Handle == second.Handle)
				return true;

			using (var firstData = first.DerData)
			using (var secondData = second.DerData) {
				if (firstData.Handle == secondData.Handle)
					return true;

				if (firstData.Length != secondData.Length)
					return false;
				IntPtr length = (IntPtr)firstData.Length;
				for (long i = 0; i < (long)length; i++) {
					if (firstData [i] != secondData [i])
						return false;
				}

				return true;
			}
		}

		~SecCertificate ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}
	}

	partial class SecIdentity : INativeObject, IDisposable {
		 
		static readonly CFString ImportExportPassphase;
		static readonly CFString ImportItemIdentity;
		static readonly CFString ImportExportAccess;
		static readonly CFString ImportExportKeychain;
		
		static SecIdentity ()
		{
			var handle = CFObject.dlopen (AppleTlsContext.SecurityLibrary, 0);
			if (handle == IntPtr.Zero)
				return;

			try {		
				ImportExportPassphase = CFObject.GetStringConstant (handle, "kSecImportExportPassphrase");
				ImportItemIdentity = CFObject.GetStringConstant (handle, "kSecImportItemIdentity");
				ImportExportAccess = CFObject.GetStringConstant (handle, "kSecImportExportAccess");
				ImportExportKeychain = CFObject.GetStringConstant (handle, "kSecImportExportKeychain");
			} finally {
				CFObject.dlclose (handle);
			}
		}

		internal IntPtr handle;
		
		internal SecIdentity (IntPtr handle, bool owns = false)
		{
			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

		[DllImport (AppleTlsContext.SecurityLibrary, EntryPoint="SecIdentityGetTypeID")]
		public extern static IntPtr GetTypeID ();

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static /* OSStatus */ SecStatusCode SecIdentityCopyCertificate (/* SecIdentityRef */ IntPtr identityRef,  /* SecCertificateRef* */ out IntPtr certificateRef);

		public SecCertificate Certificate {
			get {
				if (handle == IntPtr.Zero)
					throw new ObjectDisposedException ("SecIdentity");
				IntPtr cert;
				SecStatusCode result = SecIdentityCopyCertificate (handle, out cert);
				if (result != SecStatusCode.Success)
					throw new InvalidOperationException (result.ToString ());
				return new SecCertificate (cert, true);
			}
		}

		internal class ImportOptions
		{
#if !MONOTOUCH
			public SecAccess Access {
				get; set;
			}
			public SecKeyChain KeyChain {
				get; set;
			}
#endif
		}

		static CFDictionary CreateImportOptions (CFString password, ImportOptions options = null)
		{
			if (options == null)
				return CFDictionary.FromObjectAndKey (password.Handle, ImportExportPassphase.Handle);

			var items = new List<Tuple<IntPtr, IntPtr>> ();
			items.Add (new Tuple<IntPtr, IntPtr> (ImportExportPassphase.Handle, password.Handle));

#if !MONOTOUCH
			if (options.KeyChain != null)
				items.Add (new Tuple<IntPtr, IntPtr> (ImportExportKeychain.Handle, options.KeyChain.Handle));
			if (options.Access != null)
				items.Add (new Tuple<IntPtr, IntPtr> (ImportExportAccess.Handle, options.Access.Handle));
#endif

			return CFDictionary.FromKeysAndObjects (items);
		}

		public static SecIdentity Import (byte[] data, string password, ImportOptions options = null)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (string.IsNullOrEmpty (password)) // SecPKCS12Import() doesn't allow empty passwords.
				throw new ArgumentException ("password");
			using (var pwstring = CFString.Create (password))
			using (var optionDict = CreateImportOptions (pwstring, options)) {
				CFDictionary [] array;
				SecStatusCode result = SecImportExport.ImportPkcs12 (data, optionDict, out array);
				if (result != SecStatusCode.Success)
					throw new InvalidOperationException (result.ToString ());

				return new SecIdentity (array [0].GetValue (ImportItemIdentity.Handle));
			}
		}

		public static SecIdentity Import (X509Certificate2 certificate, ImportOptions options = null)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			if (!certificate.HasPrivateKey)
				throw new InvalidOperationException ("Need X509Certificate2 with a private key.");

			/*
			 * SecPSK12Import does not allow any empty passwords, so let's generate
			 * a semi-random one here.
			 */
			var password = Guid.NewGuid ().ToString ();
			var pkcs12 = certificate.Export (X509ContentType.Pfx, password);
			return Import (pkcs12, password, options);
		}

		~SecIdentity ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}
	}

	partial class SecKey : INativeObject, IDisposable {
		internal IntPtr handle;
		internal IntPtr owner;
		
		public SecKey (IntPtr handle, bool owns = false)
		{
			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

		/*
		 * SecItemImport() returns a SecArrayRef.  We need to free the array, not the items inside it.
		 * 
		 */
		internal SecKey (IntPtr handle, IntPtr owner)
		{
			this.handle = handle;
			this.owner = owner;
			CFObject.CFRetain (owner);
		}

		[DllImport (AppleTlsContext.SecurityLibrary, EntryPoint="SecKeyGetTypeID")]
		public extern static IntPtr GetTypeID ();
		
		~SecKey ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (owner != IntPtr.Zero) {
				CFObject.CFRelease (owner);
				owner = handle = IntPtr.Zero;
			} else if (handle != IntPtr.Zero) {
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}
	}

#if !MONOTOUCH
	class SecAccess : INativeObject, IDisposable {
		internal IntPtr handle;

		public SecAccess (IntPtr handle, bool owns = false)
		{
			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

		~SecAccess ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static /* OSStatus */ SecStatusCode SecAccessCreate (/* CFStringRef */ IntPtr descriptor,  /* CFArrayRef */ IntPtr trustedList, /* SecAccessRef _Nullable * */ out IntPtr accessRef);

		public static SecAccess Create (string descriptor)
		{
			var descriptorHandle = CFString.Create (descriptor);
			if (descriptorHandle == null)
				throw new InvalidOperationException ();

			try {
				IntPtr accessRef;
				var result = SecAccessCreate (descriptorHandle.Handle, IntPtr.Zero, out accessRef);
				if (result != SecStatusCode.Success)
					throw new InvalidOperationException (result.ToString ());

				return new SecAccess (accessRef, true);
			} finally {
				descriptorHandle.Dispose ();
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}
	}
#endif
}
#endif
