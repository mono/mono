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

#if XAMARIN_APPLETLS || __WATCHOS__
#define NATIVE_APPLE_CERTIFICATE
#endif

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using XamCore.ObjCRuntime;
using XamCore.CoreFoundation;
using XamCore.Foundation;

namespace XamCore.Security {

	public partial class SecCertificate : INativeObject, IDisposable {
		internal IntPtr handle;
		
		// invoked by marshallers
		public SecCertificate (IntPtr handle)
			: this (handle, false)
		{
		}
		
		[Preserve (Conditional = true)]
		internal SecCertificate (IntPtr handle, bool owns)
		{
			if (handle == IntPtr.Zero)
				throw new Exception ("Invalid handle");

			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}
#if !COREBUILD
		[DllImport (Constants.SecurityLibrary, EntryPoint="SecCertificateGetTypeID")]
		public extern static nint GetTypeID ();
			
		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr SecCertificateCreateWithData (IntPtr allocator, IntPtr cfData);

		public SecCertificate (NSData data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			Initialize (data);
		}

		public SecCertificate (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			using (NSData cert = NSData.FromArray (data)) {
				Initialize (cert);
			}
		}

		public SecCertificate (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

#if NATIVE_APPLE_CERTIFICATE
			/*
			 * This requires a recent Mono runtime which has the lazily-initialized
			 * certifciates in mscorlib.dll, so we can't use it on XM classic.
			 *
			 * Using 'XAMARIN_APPLETLS' as a conditional because 'XAMCORE_2_0' is
			 * defined for tvos and watch, which have a recent-enough runtime.
			 */
			handle = certificate.Impl.GetNativeAppleCertificate ();
			if (handle != IntPtr.Zero) {
				CFObject.CFRetain (handle);
				return;
			}
#endif

			using (NSData cert = NSData.FromArray (certificate.GetRawCertData ())) {
				Initialize (cert);
			}
		}

#if NATIVE_APPLE_CERTIFICATE
		internal SecCertificate (X509CertificateImpl impl)
		{
			handle = impl.GetNativeAppleCertificate ();
			if (handle != IntPtr.Zero) {
				CFObject.CFRetain (handle);
				return;
			}

			using (NSData cert = NSData.FromArray (impl.GetRawCertData ())) {
				Initialize (cert);
			}
		}
#endif

		public SecCertificate (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

#if NATIVE_APPLE_CERTIFICATE
			handle = certificate.Impl.GetNativeAppleCertificate ();
			if (handle != IntPtr.Zero) {
				CFObject.CFRetain (handle);
				return;
			}
#endif

			using (NSData cert = NSData.FromArray (certificate.RawData)) {
				Initialize (cert);
			}
		}

		void Initialize (NSData data)
		{
			handle = SecCertificateCreateWithData (IntPtr.Zero, data.Handle);
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("Not a valid DER-encoded X.509 certificate");
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr SecCertificateCopySubjectSummary (IntPtr cert);

		public string SubjectSummary {
			get {
				if (handle == IntPtr.Zero)
					throw new ObjectDisposedException ("SecCertificate");
				
				return CFString.FetchString (SecCertificateCopySubjectSummary (handle), releaseHandle: true);
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static /* CFDataRef */ IntPtr SecCertificateCopyData (/* SecCertificateRef */ IntPtr cert);

		public NSData DerData {
			get {
				if (handle == IntPtr.Zero)
					throw new ObjectDisposedException ("SecCertificate");

				IntPtr data = SecCertificateCopyData (handle);
				if (data == IntPtr.Zero)
					throw new ArgumentException ("Not a valid certificate");
				return new NSData (data, true);
			}
		}

		byte[] GetRawData ()
		{
			using (NSData data = DerData) {
				int len = (int)data.Length;
				byte[] raw = new byte [len];
				Marshal.Copy (data.Bytes, raw, 0, len);
				return raw;
			}
		}

		public X509Certificate ToX509Certificate ()
		{
#if NATIVE_APPLE_CERTIFICATE
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecCertificate");

			return new X509Certificate (handle);
#else
			return new X509Certificate (GetRawData ());
#endif
		}

		public X509Certificate2 ToX509Certificate2 ()
		{
			return new X509Certificate2 (GetRawData ());
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
				nint length = (nint)firstData.Length;
				for (nint i = 0; i < length; i++) {
					if (firstData [i] != secondData [i])
						return false;
				}

				return true;
			}
		}

#if MONOMAC
		/* Only available on OS X v10.7 or later */
		[DllImport (Constants.SecurityLibrary)]
		extern static /* CFDictionaryRef */ IntPtr SecCertificateCopyValues (/* SecCertificateRef */ IntPtr certificate, /* CFArrayRef */ IntPtr keys, /* CFErrorRef _Nullable * */ IntPtr error);

		public NSData GetPublicKey ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecCertificate");

			IntPtr result;
			using (var oids = NSArray.FromIntPtrs (new IntPtr[] { SecCertificateOIDs.SubjectPublicKey })) {
				result = SecCertificateCopyValues (handle, oids.Handle, IntPtr.Zero);
				if (result == IntPtr.Zero)
					throw new ArgumentException ("Not a valid certificate");
			}

			using (var dict = new NSDictionary (result, true)) {
				var ptr = dict.LowlevelObjectForKey (SecCertificateOIDs.SubjectPublicKey);
				if (ptr == IntPtr.Zero)
					return null;

				var publicKeyDict = new NSDictionary (ptr, false);
				var dataPtr = publicKeyDict.LowlevelObjectForKey (SecPropertyKey.Value);
				if (dataPtr == IntPtr.Zero)
					return null;

				return new NSData (dataPtr);
			}
		}
#elif NATIVE_APPLE_CERTIFICATE && (__IOS__ || __WATCHOS__ || __TVOS__)
		//
		// EXPERIMENTAL
		// Needs some more testing before we can make this public.
		// AppleTls does not actually use this API, so it may be removed again.
		//
		internal NSData GetPublicKey ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecCertificate");

			var policy = SecPolicy.CreateBasicX509Policy ();
			var trust = new SecTrust (this, policy);
			trust.Evaluate ();

			SecStatusCode status;

			using (var key = trust.GetPublicKey ())
			using (var query = new SecRecord (SecKind.Key)) {
				query.SetValueRef (key);

				status = SecKeyChain.Add (query);
				if (status != SecStatusCode.Success && status != SecStatusCode.DuplicateItem)
					throw new InvalidOperationException (status.ToString ());

				bool added = status == SecStatusCode.Success;

				try {
					var data = SecKeyChain.QueryAsData (query, false, out status);
					if (status != SecStatusCode.Success)
						throw new InvalidOperationException (status.ToString ());

					return data;
				} finally {
					if (added) {
						status = SecKeyChain.Remove (query);
						if (status != SecStatusCode.Success)
							throw new InvalidOperationException (status.ToString ());
					}
				}
			}
		}
#endif
#endif	
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

#if XAMCORE_2_0
		protected virtual void Dispose (bool disposing)
#else
		public virtual void Dispose (bool disposing)
#endif
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}
	}

	public partial class SecIdentity : INativeObject, IDisposable {
		internal IntPtr handle;
		
		// invoked by marshallers
		public SecIdentity (IntPtr handle)
			: this (handle, false)
		{
		}
		
		[Preserve (Conditional = true)]
		internal SecIdentity (IntPtr handle, bool owns)
		{
			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

#if !COREBUILD
		[DllImport (Constants.SecurityLibrary, EntryPoint="SecIdentityGetTypeID")]
		public extern static nint GetTypeID ();

		[DllImport (Constants.SecurityLibrary)]
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

		public static SecIdentity Import (byte[] data, string password)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (string.IsNullOrEmpty (password)) // SecPKCS12Import() doesn't allow empty passwords.
				throw new ArgumentException ("password");
			using (var pwstring = new NSString (password))
			using (var options = NSDictionary.FromObjectAndKey (pwstring, SecImportExport.Passphrase)) {
				NSDictionary[] array;
				SecStatusCode result = SecImportExport.ImportPkcs12 (data, options, out array);
				if (result != SecStatusCode.Success)
					throw new InvalidOperationException (result.ToString ());

				return new SecIdentity (array [0].LowlevelObjectForKey (SecImportExport.Identity.Handle));
			}
		}

		public static SecIdentity Import (X509Certificate2 certificate)
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
			return Import (pkcs12, password);
		}
#endif

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

#if XAMCORE_2_0
		protected virtual void Dispose (bool disposing)
#else
		public virtual void Dispose (bool disposing)
#endif
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}
	}

#if !COREBUILD
	public partial class SecKey : INativeObject, IDisposable {
		internal IntPtr handle;
		
		// invoked by marshallers
		public SecKey (IntPtr handle)
			: this (handle, false)
		{
		}
		
		[Preserve (Conditional = true)]
		public SecKey (IntPtr handle, bool owns)
		{
			this.handle = handle;
			if (!owns)
				CFObject.CFRetain (handle);
		}

		[DllImport (Constants.SecurityLibrary, EntryPoint="SecKeyGetTypeID")]
		public extern static nint GetTypeID ();
		
		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyGeneratePair (IntPtr dictHandle, out IntPtr pubKey, out IntPtr privKey);

		// TODO: pull all the TypeRefs needed for the NSDictionary
		
		public static SecStatusCode GenerateKeyPair (NSDictionary parameters, out SecKey publicKey, out SecKey privateKey)
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");

			IntPtr pub, priv;
			
			var res = SecKeyGeneratePair (parameters.Handle, out pub, out priv);
			if (res == SecStatusCode.Success){
				publicKey = new SecKey (pub, true);
				privateKey = new SecKey (priv, true);
			} else
				publicKey = privateKey = null;
			return res;
		}
			
		[DllImport (Constants.SecurityLibrary)]
		extern static /* size_t */ nint SecKeyGetBlockSize (IntPtr handle);

		public int BlockSize {
			get {
				if (handle == IntPtr.Zero)
					throw new ObjectDisposedException ("SecKey");
				
				return (int) SecKeyGetBlockSize (handle);
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyRawSign (IntPtr handle, SecPadding padding, IntPtr dataToSign, nint dataToSignLen, IntPtr sig, ref nint sigLen);

		public SecStatusCode RawSign (SecPadding padding, IntPtr dataToSign, int dataToSignLen, out byte [] result)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");
			if (dataToSign == IntPtr.Zero)
				throw new ArgumentException ("dataToSign");

			return _RawSign (padding, dataToSign, dataToSignLen, out result);
		}

		public unsafe SecStatusCode RawSign (SecPadding padding, byte [] dataToSign, out byte [] result)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");
			if (dataToSign == null)
				throw new ArgumentNullException ("dataToSign");

			fixed (byte *bp = dataToSign)
				return _RawSign (padding, (IntPtr) bp, dataToSign.Length, out result);
		}

		unsafe SecStatusCode _RawSign (SecPadding padding, IntPtr dataToSign, int dataToSignLen, out byte [] result)
		{
			SecStatusCode status;
			nint len = 1024;
			result = new byte [len];
			fixed (byte *p = result) {
				status = SecKeyRawSign (handle, padding, dataToSign, dataToSignLen, (IntPtr) p, ref len);
				Array.Resize (ref result, (int) len);
			}
			return status;
		}
		
		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyRawVerify (IntPtr handle, SecPadding padding, IntPtr signedData, nint signedLen, IntPtr sign, nint signLen);

		public unsafe SecStatusCode RawVerify (SecPadding padding, IntPtr signedData, int signedDataLen, IntPtr signature, int signatureLen)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");

			return SecKeyRawVerify (handle, padding, signedData, (nint) signedDataLen, signature, (nint) signatureLen);
		}

		public SecStatusCode RawVerify (SecPadding padding, byte [] signedData, byte [] signature)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");

			if (signature == null)
				throw new ArgumentNullException ("signature");
			if (signedData == null)
				throw new ArgumentNullException ("signedData");
			unsafe {
				fixed (byte *sp = signature)
				fixed (byte *dp = signedData) {
					return SecKeyRawVerify (handle, padding, (IntPtr) dp, (nint) signedData.Length, (IntPtr) sp, (nint) signature.Length);
				}
			}
		}
		
		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyEncrypt (IntPtr handle, SecPadding padding, IntPtr plainText, nint plainTextLen, IntPtr cipherText, ref nint cipherTextLengh);

#if !XAMCORE_2_0
		[Obsolete ("Use the Encrypt overload which returns (out) the cipherTextLen value so you can adjust it if needed")]
		public unsafe SecStatusCode Encrypt (SecPadding padding, IntPtr plainText, int plainTextLen, IntPtr cipherText, int cipherTextLen)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");

			nint len = (nint) cipherTextLen;
			return SecKeyEncrypt (handle, padding, plainText, (nint) plainTextLen, cipherText, ref len);
		}
#endif
		public unsafe SecStatusCode Encrypt (SecPadding padding, IntPtr plainText, nint plainTextLen, IntPtr cipherText, ref nint cipherTextLen)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");

			return SecKeyEncrypt (handle, padding, plainText, plainTextLen, cipherText, ref cipherTextLen);
		}

		public SecStatusCode Encrypt (SecPadding padding, byte [] plainText, byte [] cipherText)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");

			if (cipherText == null)
				throw new ArgumentNullException ("cipherText");
			if (plainText == null)
				throw new ArgumentNullException ("plainText");
			unsafe {
				fixed (byte *cp = cipherText)
				fixed (byte *pp = plainText) {
					nint len = (nint) cipherText.Length;
					return SecKeyEncrypt (handle, padding, (IntPtr) pp, (nint) plainText.Length, (IntPtr) cp, ref len);
				}
			}
		}

		public SecStatusCode Encrypt (SecPadding padding, byte [] plainText, out byte [] cipherText)
		{
			cipherText = new byte [BlockSize];
			return Encrypt (padding, plainText, cipherText);
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyDecrypt (IntPtr handle, SecPadding padding, IntPtr cipherTextLen, nint cipherLen, IntPtr plainText, ref nint plainTextLen);

#if !XAMCORE_2_0
		[Obsolete ("Use the Decrypt overload which returns (ref) the plainTextLen value so you can adjust it if needed")]
		public unsafe SecStatusCode Decrypt (SecPadding padding, IntPtr cipherText, int cipherTextLen, IntPtr plainText, int plainTextLen)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");

			int len = plainTextLen;
			return SecKeyDecrypt (handle, padding, cipherText, cipherTextLen, plainText, ref len);
		}
#endif
		public unsafe SecStatusCode Decrypt (SecPadding padding, IntPtr cipherText, nint cipherTextLen, IntPtr plainText, ref nint plainTextLen)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");

			return SecKeyDecrypt (handle, padding, cipherText, cipherTextLen, plainText, ref plainTextLen);
		}

		SecStatusCode _Decrypt (SecPadding padding, byte [] cipherText, ref byte [] plainText)
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SecKey");

			if (cipherText == null)
				throw new ArgumentNullException ("cipherText");
		
			unsafe {
				fixed (byte *cp = cipherText) {
					if (plainText == null)
						plainText = new byte [cipherText.Length];
					nint len = plainText.Length;
					SecStatusCode status;
					fixed (byte *pp = plainText)
						status = SecKeyDecrypt (handle, padding, (IntPtr)cp, (nint)cipherText.Length, (IntPtr)pp, ref len);
					if (len < plainText.Length)
						Array.Resize<byte> (ref plainText, (int) len);
					return status;
				}
			}
		}

#if !XAMCORE_2_0
		[Obsolete ("Use the Decrypt overload which returns (out) the plainText array so you can adjust it if needed")]
		public SecStatusCode Decrypt (SecPadding padding, byte [] cipherText, byte [] plainText)
		{
			if (plainText == null)
				throw new ArgumentNullException ("plainText");

			return _Decrypt (padding, cipherText, ref plainText);
		}
#endif
		public SecStatusCode Decrypt (SecPadding padding, byte [] cipherText, out byte [] plainText)
		{
			plainText = null;
			return _Decrypt (padding, cipherText, ref plainText);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern IntPtr /* SecKeyRef _Nullable */ SecKeyCreateRandomKey (IntPtr /* CFDictionaryRef* */ parameters, out IntPtr /* CFErrorRef** */ error);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		static public SecKey CreateRandomKey (NSDictionary parameters, out NSError error)
		{
			if (parameters == null)
				throw new ArgumentNullException (nameof (parameters));

			IntPtr err;
			var key = SecKeyCreateRandomKey (parameters.Handle, out err);
			error = err == IntPtr.Zero ? null : new NSError (err);
			return key == IntPtr.Zero ? null : new SecKey (key, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		static public SecKey CreateRandomKey (SecKeyType keyType, int keySizeInBits, NSDictionary parameters, out NSError error)
		{
			using (var ks = new NSNumber (keySizeInBits))
			using (var md = parameters == null ? new NSMutableDictionary () : new NSMutableDictionary (parameters)) {
				md.LowlevelSetObject (keyType.GetConstant (), SecAttributeKey.KeyType);
				md.LowlevelSetObject (ks, SecAttributeKey.KeySizeInBits);
				return CreateRandomKey (md, out error);
			}
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern IntPtr /* SecKeyRef _Nullable */ SecKeyCreateWithData (IntPtr /* CFDataRef* */ keyData, IntPtr /* CFDictionaryRef* */ attributes, out IntPtr /* CFErrorRef** */ error);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		static public SecKey Create (NSData keyData, NSDictionary parameters, out NSError error)
		{
			if (keyData == null)
				throw new ArgumentNullException (nameof (keyData));
			if (parameters == null)
				throw new ArgumentNullException (nameof (parameters));

			IntPtr err;
			var key = SecKeyCreateWithData (keyData.Handle, parameters.Handle, out err);
			error = err == IntPtr.Zero ? null : new NSError (err);
			return key == IntPtr.Zero ? null : new SecKey (key, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		static public SecKey Create (NSData keyData, SecKeyType keyType, SecKeyClass keyClass, int keySizeInBits, NSDictionary parameters, out NSError error)
		{
			using (var ks = new NSNumber (keySizeInBits))
			using (var md = parameters == null ? new NSMutableDictionary () : new NSMutableDictionary (parameters)) {
				md.LowlevelSetObject (keyType.GetConstant (), SecAttributeKey.KeyType);
				md.LowlevelSetObject (keyClass.GetConstant (), SecAttributeKey.KeyClass);
				md.LowlevelSetObject (ks, SecAttributeKey.KeySizeInBits);
				return Create (keyData, md, out error);
			}
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern IntPtr /* CFDataRef _Nullable */ SecKeyCopyExternalRepresentation (IntPtr /* SecKeyRef* */ key, out IntPtr /* CFErrorRef** */ error);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public NSData GetExternalRepresentation (out NSError error)
		{
			IntPtr err;
			var data = SecKeyCopyExternalRepresentation (handle, out err);
			error = err == IntPtr.Zero ? null : new NSError (err);
			return Runtime.GetNSObject<NSData> (data, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public NSData GetExternalRepresentation ()
		{
			IntPtr err;
			var data = SecKeyCopyExternalRepresentation (handle, out err);
			return Runtime.GetNSObject<NSData> (data, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern IntPtr /* CFDictionaryRef _Nullable */ SecKeyCopyAttributes (IntPtr /* SecKeyRef* */ key);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public NSDictionary GetAttributes ()
		{
			var dict = SecKeyCopyAttributes (handle);
			return Runtime.GetNSObject<NSDictionary> (dict, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern IntPtr /* SecKeyRef* */ SecKeyCopyPublicKey (IntPtr /* SecKeyRef* */ key);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public SecKey GetPublicKey ()
		{
			var key = SecKeyCopyPublicKey (handle);
			return key == IntPtr.Zero ? null : new SecKey (key, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern bool /* Boolean */ SecKeyIsAlgorithmSupported (IntPtr /* SecKeyRef* */ key, /* SecKeyOperationType */ nint operation, IntPtr /* SecKeyAlgorithm* */ algorithm);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public bool IsAlgorithmSupported (SecKeyOperationType operation, SecKeyAlgorithm algorithm)
		{
			return SecKeyIsAlgorithmSupported (handle, (int) operation, algorithm.GetConstant ().Handle);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern /* CFDataRef _Nullable */ IntPtr SecKeyCreateSignature (/* SecKeyRef */ IntPtr key, /* SecKeyAlgorithm */ IntPtr algorithm, /* CFDataRef */ IntPtr dataToSign, /* CFErrorRef* */ out IntPtr error);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public NSData CreateSignature (SecKeyAlgorithm algorithm, NSData dataToSign, out NSError error)
		{
			if (dataToSign == null)
				throw new ArgumentNullException (nameof (dataToSign));

			IntPtr err;
			var data = SecKeyCreateSignature (Handle, algorithm.GetConstant ().Handle, dataToSign.Handle, out err);
			error = err == IntPtr.Zero ? null : new NSError (err);
			return Runtime.GetNSObject<NSData> (data, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern /* Boolean */ bool SecKeyVerifySignature (/* SecKeyRef */ IntPtr key, /* SecKeyAlgorithm */ IntPtr algorithm, /* CFDataRef */ IntPtr signedData, /* CFDataRef */ IntPtr signature, /* CFErrorRef* */ out IntPtr error);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public bool VerifySignature (SecKeyAlgorithm algorithm, NSData signedData, NSData signature, out NSError error)
		{
			if (signedData == null)
				throw new ArgumentNullException (nameof (signedData));
			if (signature == null)
				throw new ArgumentNullException (nameof (signature));
			
			IntPtr err;
			var result = SecKeyVerifySignature (Handle, algorithm.GetConstant ().Handle, signedData.Handle, signature.Handle, out err);
			error = err == IntPtr.Zero ? null : new NSError (err);
			return result;
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern /* CFDataRef _Nullable */ IntPtr SecKeyCreateEncryptedData (/* SecKeyRef */ IntPtr key, /* SecKeyAlgorithm */ IntPtr algorithm, /* CFDataRef */ IntPtr plaintext, /* CFErrorRef* */ out IntPtr error);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public NSData CreateEncryptedData (SecKeyAlgorithm algorithm, NSData plaintext, out NSError error)
		{
			if (plaintext == null)
				throw new ArgumentNullException (nameof (plaintext));

			IntPtr err;
			var data = SecKeyCreateEncryptedData (Handle, algorithm.GetConstant ().Handle, plaintext.Handle, out err);
			error = err == IntPtr.Zero ? null : new NSError (err);
			return Runtime.GetNSObject<NSData> (data, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern /* CFDataRef _Nullable */ IntPtr SecKeyCreateDecryptedData (/* SecKeyRef */ IntPtr key, /* SecKeyAlgorithm */ IntPtr algorithm, /* CFDataRef */ IntPtr ciphertext, /* CFErrorRef* */ out IntPtr error);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public NSData CreateDecryptedData (SecKeyAlgorithm algorithm, NSData ciphertext, out NSError error)
		{
			if (ciphertext == null)
				throw new ArgumentNullException (nameof (ciphertext));

			IntPtr err;
			var data = SecKeyCreateDecryptedData (Handle, algorithm.GetConstant ().Handle, ciphertext.Handle, out err);
			error = err == IntPtr.Zero ? null : new NSError (err);
			return Runtime.GetNSObject<NSData> (data, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		[DllImport (Constants.SecurityLibrary)]
		static extern /* CFDataRef _Nullable */ IntPtr SecKeyCopyKeyExchangeResult (/* SecKeyRef */ IntPtr privateKey, /* SecKeyAlgorithm */ IntPtr algorithm, /* SecKeyRef */ IntPtr publicKey, /* CFDictionaryRef */ IntPtr parameters, /* CFErrorRef* */ out IntPtr error);

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public NSData GetKeyExchangeResult (SecKeyAlgorithm algorithm, SecKey publicKey, NSDictionary parameters, out NSError error)
		{
			if (publicKey == null)
				throw new ArgumentNullException (nameof (publicKey));
			if (parameters == null)
				throw new ArgumentNullException (nameof (parameters));

			IntPtr err;
			var data = SecKeyCopyKeyExchangeResult (Handle, algorithm.GetConstant ().Handle, publicKey.Handle, parameters.Handle, out err);
			error = err == IntPtr.Zero ? null : new NSError (err);
			return Runtime.GetNSObject<NSData> (data, true);
		}

		[Watch (3,0)][TV (10,0)][Mac (10,12)][iOS (10,0)]
		public NSData GetKeyExchangeResult (SecKeyAlgorithm algorithm, SecKey publicKey, SecKeyKeyExchangeParameter parameters, out NSError error)
		{
			return GetKeyExchangeResult (algorithm, publicKey, parameters?.Dictionary, out error);
		}

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

#if XAMCORE_2_0
		protected virtual void Dispose (bool disposing)
#else
		public virtual void Dispose (bool disposing)
#endif
		{
			if (handle != IntPtr.Zero){
				CFObject.CFRelease (handle);
				handle = IntPtr.Zero;
			}
		}
	}
#endif
}
