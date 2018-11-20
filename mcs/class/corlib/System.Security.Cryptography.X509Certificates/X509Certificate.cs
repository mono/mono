//
// X509Certificate.cs: Handles X.509 certificates.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Diagnostics;
using System.Text;
using Internal.Cryptography;
using Microsoft.Win32.SafeHandles;

using Mono.Security;

namespace System.Security.Cryptography.X509Certificates
{
	// References:
	// a.	Internet X.509 Public Key Infrastructure Certificate and CRL Profile
	//	http://www.ietf.org/rfc/rfc3280.txt

	// LAMESPEC: the MSDN docs always talks about X509v3 certificates
	// and/or Authenticode certs. However this class works with older
	// X509v1 certificates and non-authenticode (code signing) certs.
	[Serializable]
	public partial class X509Certificate : IDisposable, IDeserializationCallback, ISerializable
	{
#region CoreFX Implementation

		X509CertificateImpl impl;
		volatile byte[] lazyCertHash;
		volatile byte[] lazySerialNumber;
		volatile string lazyIssuer;
		volatile string lazySubject;
		volatile string lazyKeyAlgorithm;
		volatile byte[] lazyKeyAlgorithmParameters;
		volatile byte[] lazyPublicKey;
		DateTime lazyNotBefore = DateTime.MinValue;
		DateTime lazyNotAfter = DateTime.MinValue;

		public virtual void Reset ()
		{
			if (impl != null) {
				impl.Dispose ();
				impl = null;
			}

			lazyCertHash = null;
			lazyIssuer = null;
			lazySubject = null;
			lazySerialNumber = null;
			lazyKeyAlgorithm = null;
			lazyKeyAlgorithmParameters = null;
			lazyPublicKey = null;
			lazyNotBefore = DateTime.MinValue;
			lazyNotAfter = DateTime.MinValue;
		}

#endregion

#region CoreFX Implementation - with X509Helper

		public X509Certificate ()
		{
		}

		public X509Certificate (byte[] data)
		{
			if (data != null && data.Length != 0)
				impl = X509Helper.Import (data);
		}

		public X509Certificate (byte[] rawData, string password)
			: this (rawData, password, X509KeyStorageFlags.DefaultKeySet)
		{
		}

		[CLSCompliantAttribute (false)]
		public X509Certificate (byte[] rawData, SecureString password)
			: this (rawData, password, X509KeyStorageFlags.DefaultKeySet)
		{
		}

		public X509Certificate (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			if (rawData == null || rawData.Length == 0)
				throw new ArgumentException (SR.Arg_EmptyOrNullArray, nameof (rawData));

			ValidateKeyStorageFlags (keyStorageFlags);

			using (var safePasswordHandle = new SafePasswordHandle (password))
				impl = X509Helper.Import (rawData, safePasswordHandle, keyStorageFlags);
		}

		[CLSCompliantAttribute (false)]
		public X509Certificate (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			if (rawData == null || rawData.Length == 0)
				throw new ArgumentException (SR.Arg_EmptyOrNullArray, nameof (rawData));

			ValidateKeyStorageFlags (keyStorageFlags);

			using (var safePasswordHandle = new SafePasswordHandle (password))
				impl = X509Helper.Import (rawData, safePasswordHandle, keyStorageFlags);
		}

		public X509Certificate (IntPtr handle)
		{
			throw new PlatformNotSupportedException ("Initializing `X509Certificate` from native handle is not supported.");
		}

		internal X509Certificate (X509CertificateImpl impl)
		{
			Debug.Assert (impl != null);
			this.impl = X509Helper.InitFromCertificate (impl);
		}

		public X509Certificate (string fileName)
			: this (fileName, (string)null, X509KeyStorageFlags.DefaultKeySet)
		{
		}

		public X509Certificate (string fileName, string password)
			: this (fileName, password, X509KeyStorageFlags.DefaultKeySet)
		{
		}

		[CLSCompliantAttribute(false)]
		public X509Certificate (string fileName, SecureString password)
			: this (fileName, password, X509KeyStorageFlags.DefaultKeySet)
		{
		}

		public X509Certificate (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			ValidateKeyStorageFlags (keyStorageFlags);

			var rawData = File.ReadAllBytes (fileName);
			using (var safePasswordHandle = new SafePasswordHandle (password))
				impl = X509Helper.Import (rawData, safePasswordHandle, keyStorageFlags);
		}

		[CLSCompliantAttribute (false)]
		public X509Certificate (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags) : this ()
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			ValidateKeyStorageFlags (keyStorageFlags);

			var rawData = File.ReadAllBytes (fileName);
			using (var safePasswordHandle = new SafePasswordHandle (password))
				impl = X509Helper.Import (rawData, safePasswordHandle, keyStorageFlags);
		}

		public X509Certificate (X509Certificate cert)
		{
			if (cert == null)
				throw new ArgumentNullException (nameof (cert));

			impl = X509Helper.InitFromCertificate (cert);
		}

#endregion

#region CoreFX Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA2229", Justification = "Public API has already shipped.")]
		public X509Certificate (SerializationInfo info, StreamingContext context) : this ()
		{
			throw new PlatformNotSupportedException ();
		}

		public static X509Certificate CreateFromCertFile (string filename)
		{
			return new X509Certificate (filename);
		}

		public static X509Certificate CreateFromSignedFile (string filename)
		{
			return new X509Certificate (filename);
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new PlatformNotSupportedException ();
		}

		void IDeserializationCallback.OnDeserialization (object sender)
		{
			throw new PlatformNotSupportedException ();
		}

		public IntPtr Handle {
			get {
				if (X509Helper.IsValid (impl))
					return impl.Handle;
				return IntPtr.Zero;
			}
		}

		public string Issuer {
			get {
				ThrowIfInvalid ();

				string issuer = lazyIssuer;
				if (issuer == null)
					issuer = lazyIssuer = Impl.Issuer;
				return issuer;
			}
		}

		public string Subject {
			get {
				ThrowIfInvalid ();

				string subject = lazySubject;
				if (subject == null)
					subject = lazySubject = Impl.Subject;
				return subject;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				Reset ();
		}

		public override bool Equals (object obj)
		{
			X509Certificate other = obj as X509Certificate;
			if (other == null)
				return false;
			return Equals (other);
		}

		public virtual bool Equals (X509Certificate other)
		{
			if (other == null)
				return false;

			if (Impl == null)
				return other.Impl == null;

			if (!Issuer.Equals (other.Issuer))
				return false;

			byte[] thisSerialNumber = GetRawSerialNumber ();
			byte[] otherSerialNumber = other.GetRawSerialNumber ();

			if (thisSerialNumber.Length != otherSerialNumber.Length)
				return false;
			for (int i = 0; i < thisSerialNumber.Length; i++) {
				if (thisSerialNumber[i] != otherSerialNumber[i])
					return false;
			}

			return true;
		}

#endregion

#region CoreFX Implementation - With X509Helper

		public virtual byte[] Export (X509ContentType contentType)
		{
			return Export (contentType, (string)null);
		}

		public virtual byte[] Export (X509ContentType contentType, string password)
		{
			VerifyContentType (contentType);

			if (Impl == null)
				throw new CryptographicException (ErrorCode.E_POINTER);  // Not the greatest error, but needed for backward compat.

			using (var safePasswordHandle = new SafePasswordHandle (password))
				return Impl.Export (contentType, safePasswordHandle);
		}

		[System.CLSCompliantAttribute (false)]
		public virtual byte[] Export (X509ContentType contentType, SecureString password)
		{
			VerifyContentType (contentType);

			if (Impl == null)
				throw new CryptographicException (ErrorCode.E_POINTER);  // Not the greatest error, but needed for backward compat.

			using (var safePasswordHandle = new SafePasswordHandle (password))
				return Impl.Export (contentType, safePasswordHandle);
		}

#endregion

#region CoreFX Implementation

		public virtual string GetRawCertDataString ()
		{
			ThrowIfInvalid ();
			return GetRawCertData ().ToHexStringUpper ();
		}

		public virtual byte[] GetCertHash ()
		{
			ThrowIfInvalid ();
			return GetRawCertHash ().CloneByteArray ();
		}

		public virtual byte[] GetCertHash (HashAlgorithmName hashAlgorithm)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual bool TryGetCertHash (HashAlgorithmName hashAlgorithm, Span<byte> destination, out int bytesWritten)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual string GetCertHashString ()
		{
			ThrowIfInvalid ();
			return GetRawCertHash ().ToHexStringUpper ();
		}

		public virtual string GetCertHashString (HashAlgorithmName hashAlgorithm)
		{
			ThrowIfInvalid ();

			return GetCertHash (hashAlgorithm).ToHexStringUpper ();
		}

		// Only use for internal purposes when the returned byte[] will not be mutated
		byte[] GetRawCertHash ()
		{
			return lazyCertHash ?? (lazyCertHash = Impl.Thumbprint);
		}

		public virtual string GetEffectiveDateString ()
		{
			return GetNotBefore ().ToString ();
		}

		public virtual string GetExpirationDateString ()
		{
			return GetNotAfter ().ToString ();
		}

		public virtual string GetFormat ()
		{
			return "X509";
		}

		public virtual string GetPublicKeyString ()
		{
			return GetPublicKey ().ToHexStringUpper ();
		}

		public virtual byte[] GetRawCertData ()
		{
			ThrowIfInvalid ();

			return Impl.RawData.CloneByteArray ();
		}

		public override int GetHashCode ()
		{
			if (Impl == null)
				return 0;

			byte[] thumbPrint = GetRawCertHash ();
			int value = 0;
			for (int i = 0; i < thumbPrint.Length && i < 4; ++i) {
				value = value << 8 | thumbPrint[i];
			}
			return value;
		}

		public virtual string GetKeyAlgorithm ()
		{
			ThrowIfInvalid ();

			string keyAlgorithm = lazyKeyAlgorithm;
			if (keyAlgorithm == null)
				keyAlgorithm = lazyKeyAlgorithm = Impl.KeyAlgorithm;
			return keyAlgorithm;
		}

		public virtual byte[] GetKeyAlgorithmParameters ()
		{
			ThrowIfInvalid ();

			byte[] keyAlgorithmParameters = lazyKeyAlgorithmParameters;
			if (keyAlgorithmParameters == null)
				keyAlgorithmParameters = lazyKeyAlgorithmParameters = Impl.KeyAlgorithmParameters;
			return keyAlgorithmParameters.CloneByteArray ();
		}

		public virtual string GetKeyAlgorithmParametersString ()
		{
			ThrowIfInvalid ();

			byte[] keyAlgorithmParameters = GetKeyAlgorithmParameters ();
			return keyAlgorithmParameters.ToHexStringUpper ();
		}

		public virtual byte[] GetPublicKey ()
		{
			ThrowIfInvalid ();

			byte[] publicKey = lazyPublicKey;
			if (publicKey == null)
				publicKey = lazyPublicKey = Impl.PublicKeyValue;
			return publicKey.CloneByteArray ();
		}

		public virtual byte[] GetSerialNumber ()
		{
			ThrowIfInvalid ();
			byte[] serialNumber = GetRawSerialNumber ().CloneByteArray ();
			// PAL always returns big-endian, GetSerialNumber returns little-endian
			Array.Reverse (serialNumber);
			return serialNumber;
		}

		public virtual string GetSerialNumberString ()
		{
			ThrowIfInvalid ();
			// PAL always returns big-endian, GetSerialNumberString returns big-endian too
			return GetRawSerialNumber ().ToHexStringUpper ();
		}

		// Only use for internal purposes when the returned byte[] will not be mutated
		byte[] GetRawSerialNumber ()
		{
			return lazySerialNumber ?? (lazySerialNumber = Impl.SerialNumber);
		}

		// See https://github.com/dotnet/corefx/issues/30544
		[Obsolete ("This method has been deprecated.  Please use the Subject property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public virtual string GetName ()
		{
			ThrowIfInvalid ();
			return Impl.LegacySubject;
		}

		// See https://github.com/dotnet/corefx/issues/30544
		[Obsolete ("This method has been deprecated.  Please use the Issuer property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public virtual string GetIssuerName ()
		{
			ThrowIfInvalid ();
			return Impl.LegacyIssuer;
		}

		public override string ToString ()
		{
			return ToString (fVerbose: false);
		}

		public virtual string ToString (bool fVerbose)
		{
			if (!fVerbose || !X509Helper.IsValid (impl))
				return base.ToString ();

			StringBuilder sb = new StringBuilder ();

			// Subject
			sb.AppendLine ("[Subject]");
			sb.Append ("  ");
			sb.AppendLine (Subject);

			// Issuer
			sb.AppendLine ();
			sb.AppendLine ("[Issuer]");
			sb.Append ("  ");
			sb.AppendLine (Issuer);

			// Serial Number
			sb.AppendLine ();
			sb.AppendLine ("[Serial Number]");
			sb.Append ("  ");
			byte[] serialNumber = GetSerialNumber ();
			Array.Reverse (serialNumber);
			sb.Append (serialNumber.ToHexArrayUpper ());
			sb.AppendLine ();

			// NotBefore
			sb.AppendLine ();
			sb.AppendLine ("[Not Before]");
			sb.Append ("  ");
			sb.AppendLine (FormatDate (GetNotBefore ()));

			// NotAfter
			sb.AppendLine ();
			sb.AppendLine ("[Not After]");
			sb.Append ("  ");
			sb.AppendLine (FormatDate (GetNotAfter ()));

			// Thumbprint
			sb.AppendLine ();
			sb.AppendLine ("[Thumbprint]");
			sb.Append ("  ");
			sb.Append (GetRawCertHash ().ToHexArrayUpper ());
			sb.AppendLine ();

			return sb.ToString ();
		}

		[ComVisible (false)]
		public virtual void Import (byte[] rawData)
		{
			throw new PlatformNotSupportedException (SR.NotSupported_ImmutableX509Certificate);
		}

		[ComVisible (false)]
		public virtual void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			throw new PlatformNotSupportedException (SR.NotSupported_ImmutableX509Certificate);
		}

		public virtual void Import (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			throw new PlatformNotSupportedException (SR.NotSupported_ImmutableX509Certificate);
		}

		[ComVisible (false)]
		public virtual void Import (string fileName)
		{
			throw new PlatformNotSupportedException (SR.NotSupported_ImmutableX509Certificate);
		}

		[ComVisible (false)]
		public virtual void Import (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			throw new PlatformNotSupportedException (SR.NotSupported_ImmutableX509Certificate);
		}

		public virtual void Import (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			throw new PlatformNotSupportedException (SR.NotSupported_ImmutableX509Certificate);
		}

		internal DateTime GetNotAfter ()
		{
			ThrowIfInvalid ();

			DateTime notAfter = lazyNotAfter;
			if (notAfter == DateTime.MinValue)
				notAfter = lazyNotAfter = impl.NotAfter;
			return notAfter;
		}

		internal DateTime GetNotBefore ()
		{
			ThrowIfInvalid ();

			DateTime notBefore = lazyNotBefore;
			if (notBefore == DateTime.MinValue)
				notBefore = lazyNotBefore = impl.NotBefore;
			return notBefore;
		}

		/// <summary>
		///     Convert a date to a string.
		/// 
		///     Some cultures, specifically using the Um-AlQura calendar cannot convert dates far into
		///     the future into strings.  If the expiration date of an X.509 certificate is beyond the range
		///     of one of these cases, we need to fall back to a calendar which can express the dates
		/// </summary>
		protected static string FormatDate (DateTime date)
		{
			CultureInfo culture = CultureInfo.CurrentCulture;

			if (!culture.DateTimeFormat.Calendar.IsValidDay (date.Year, date.Month, date.Day, 0)) {
				// The most common case of culture failing to work is in the Um-AlQuara calendar. In this case,
				// we can fall back to the Hijri calendar, otherwise fall back to the invariant culture.
#if !MOBILE
				if (culture.DateTimeFormat.Calendar is UmAlQuraCalendar) {
					culture = culture.Clone () as CultureInfo;
					culture.DateTimeFormat.Calendar = new HijriCalendar ();
				} else
#endif
				{
					culture = CultureInfo.InvariantCulture;
				}
			}

			return date.ToString (culture);
		}

		internal static void ValidateKeyStorageFlags (X509KeyStorageFlags keyStorageFlags)
		{
			if ((keyStorageFlags & ~KeyStorageFlagsAll) != 0)
				throw new ArgumentException (SR.Argument_InvalidFlag, nameof (keyStorageFlags));

			const X509KeyStorageFlags EphemeralPersist =
			    X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.PersistKeySet;

			X509KeyStorageFlags persistenceFlags = keyStorageFlags & EphemeralPersist;

			if (persistenceFlags == EphemeralPersist) {
				throw new ArgumentException (
				    SR.Format (SR.Cryptography_X509_InvalidFlagCombination, persistenceFlags),
				    nameof (keyStorageFlags));
			}
		}

		void VerifyContentType (X509ContentType contentType)
		{
			if (!(contentType == X509ContentType.Cert || contentType == X509ContentType.SerializedCert || contentType == X509ContentType.Pkcs12))
				throw new CryptographicException (SR.Cryptography_X509_InvalidContentType);
		}

		internal const X509KeyStorageFlags KeyStorageFlagsAll =
		    X509KeyStorageFlags.UserKeySet |
		    X509KeyStorageFlags.MachineKeySet |
		    X509KeyStorageFlags.Exportable |
		    X509KeyStorageFlags.UserProtected |
		    X509KeyStorageFlags.PersistKeySet |
		    X509KeyStorageFlags.EphemeralKeySet;

#endregion // CoreFX Implementation

		internal void ImportHandle (X509CertificateImpl impl)
		{
			Reset ();
			this.impl = impl;
		}

		internal X509CertificateImpl Impl {
			get {
				return impl;
			}
		}

		internal bool IsValid {
			get { return X509Helper.IsValid (impl); }
		}

		internal void ThrowIfInvalid ()
		{
			X509Helper.ThrowIfContextInvalid (impl);
		}
	}
}
