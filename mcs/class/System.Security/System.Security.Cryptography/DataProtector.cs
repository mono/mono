//
// DataProtector.cs: Provides the base class for simple data protectors
//
// Author:
//	Robert J. van der Boon  <rjvdboon@gmail.com>
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

using System.Collections.Generic;
using System.IO;

namespace System.Security.Cryptography {
	/// <summary>Provides the base class for simple data protectors.</summary>
	public abstract class DataProtector {
		byte [] hashed_purpose;

		/// <summary>Gets the name of the application.</summary>
		/// <returns>The name of the application.</returns>
		protected string ApplicationName {
			get; private set;
		}

		/// <summary>Gets the primary purpose for the protected data.</summary>
		/// <returns>The primary purpose for the protected data.</returns>
		protected string PrimaryPurpose {
			get; private set;
		}

		/// <summary>Gets the specific purposes for the protected data.</summary>
		/// <returns>A collection of the specific purposes for the protected data.</returns>
		protected IEnumerable<string> SpecificPurposes {
			get; private set;
		}
		
		/// <summary>Gets whether the hash is prepended to the byte array before encryption.</summary>
		/// <returns>Always true, unless overridden in a derived type.</returns>
		protected virtual bool PrependHashedPurposeToPlaintext {
			get {
				return true;
			}
		}

		/// <summary>Creates a new instance of the <see cref="DataProtector" /> class by using the provided application
		/// name, primary purpose, and (optional) specific purposes.</summary>
		/// <param name="applicationName">The name of the application.</param>
		/// <param name="primaryPurpose">The primary purpose.</param>
		/// <param name="specificPurposes">The specific purposes.</param>
		/// <exception cref="ArgumentException">
		///      <paramref name="applicationName" /> is an empty string, a string containing only whitespace, or null.
		/// -or- <paramref name="primaryPurpose" /> is an empty string, a string containing only whitespace, or null.
		/// -or- <paramref name="specificPurposes" /> contains an empty string, a string containing only whitespace, or null.</exception>
		protected DataProtector (string applicationName, string primaryPurpose, params string [] specificPurposes)
		{
			// The .Net documentation states "empty", but testing on .Net shows that whitespace is not allowed either
			if (string.IsNullOrWhiteSpace (applicationName))
				throw new ArgumentException ("The applicationName is missing", "applicationName");
			// The .Net documentation states "empty", but testing on .Net shows that whitespace is not allowed either
			if (string.IsNullOrWhiteSpace (primaryPurpose))
				throw new ArgumentException ("The primaryPurpose is missing", "primaryPurpose");
			if (specificPurposes != null) {
				foreach (string specificPurpose in specificPurposes)
					// The .Net documentation states "empty", but testing on .Net shows that whitespace is not allowed either
					if (string.IsNullOrWhiteSpace (specificPurpose))
						throw new ArgumentException ("A specificPurpose is null or empty", "specificPurposes");
			}
			ApplicationName = applicationName;
			PrimaryPurpose = primaryPurpose;
			SpecificPurposes = specificPurposes == null ? new List<string>(0) : new List<string>(specificPurposes);
		}

		/// <summary>Creates an instance of a data protector implementation by using the specified class name
		/// of the data protector, the application name, the primary purpose, and the (optional) specific purposes.</summary>
		/// <param name="providerClass">The class name for the data protector.</param>
		/// <param name="applicationName">The name of the application.</param>
		/// <param name="primaryPurpose">The primary purpose.</param>
		/// <param name="specificPurposes">The specific purposes.</param>
		/// <returns>A data protector implementation object.</returns>
		/// <exception cref="ArgumentNullException">
		///   <paramref name="providerClass" /> is null.</exception>
		public static DataProtector Create (string providerClass, string applicationName, string primaryPurpose, params string [] specificPurposes)
		{
			if (providerClass == null)
				throw new ArgumentNullException ("providerClass");
			return (DataProtector)CryptoConfig.CreateFromName (providerClass, new object [] { applicationName, primaryPurpose, specificPurposes });
		}

		/// <summary>Creates a hash of the property values specified by the constructor.</summary>
		/// <returns>the hash of the <see cref="ApplicationName" />, <see cref="PrimaryPurpose" />, and <see cref="SpecificPurposes" /> properties.</returns>
		public byte [] GetHashedPurpose ()
		{
			if (hashed_purpose != null)
				return hashed_purpose;
			using (var memoryStream = new MemoryStream ()) {
				using (var binaryWriter = new BinaryWriter (memoryStream)) {
					binaryWriter.Write (ApplicationName);
					binaryWriter.Write (PrimaryPurpose);
					foreach (string specificPurpose in SpecificPurposes)
						binaryWriter.Write (specificPurpose);
				}
				using (var hashAlgorithm = HashAlgorithm.Create ("SHA256")) {
					hashed_purpose = hashAlgorithm.ComputeHash (memoryStream.ToArray());
				}
			}
			return hashed_purpose;
		}

		/// <summary>Determines if re-encryption is required for the specified encrypted data.</summary>
		/// <param name="encryptedData">The encrypted data to be evaluated.</param>
		/// <returns>true if the data must be re-encrypted; otherwise, false.</returns>
		public abstract bool IsReprotectRequired (byte [] encryptedData);

		/// <summary>Protects the specified data.</summary>
		/// <param name="userData">The user data to be protected.</param>
		/// <returns>A byte array with the encrypted data.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="userData" /> is null.</exception>
		public byte [] Protect (byte [] userData)
		{
			if (userData == null)
				throw new ArgumentNullException ("userData");
			if (!PrependHashedPurposeToPlaintext)
				return ProviderProtect (userData);

			byte [] userDataToProtect;
			var prepend = GetHashedPurpose ();
			userDataToProtect = new byte [prepend.Length + userData.Length];
			Array.Copy (prepend, 0, userDataToProtect, 0, prepend.Length);
			Array.Copy (userData, 0, userDataToProtect, prepend.Length, userData.Length);
			return ProviderProtect (userDataToProtect);
		}

		/// <summary>Specifies the delegate method in the derived class that <see cref="Protect(byte[])" /> calls.</summary>
		/// <param name="userData">The user data.</param>
		/// <returns>A byte array with the encrypted data.</returns>
		protected abstract byte [] ProviderProtect (byte [] userData);

		/// <summary>Specifies the delegate method in the derived class that <see cref="Unprotect(byte[])" /> calls.</summary>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <returns>A byte array with the clear text user data.</returns>
		protected abstract byte [] ProviderUnprotect (byte [] encryptedData);

		/// <summary>Unprotects the specified data.</summary>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <returns>A byte array with the clear text user data.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="encryptedData" /> is null.</exception>
		/// <exception cref="CryptographicException"><paramref name="encryptedData" /> contained an invalid purpose.</exception>
		public byte [] Unprotect (byte [] encryptedData)
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");
			byte [] unprotectedData = ProviderUnprotect (encryptedData);
			if (!PrependHashedPurposeToPlaintext)
				return unprotectedData;

			byte [] hashedToCheck = GetHashedPurpose ();
			if (unprotectedData.Length < hashedToCheck.Length)
				throw new CryptographicException ("The purpose of the protected blob does not match the expected purpose value of this data protector instance.");
			for (int i = 0; i < hashedToCheck.Length; i++)
				if (hashedToCheck [i] != unprotectedData [i])
					throw new CryptographicException ("The purpose of the protected blob does not match the expected purpose value of this data protector instance.");

			byte [] clearTextData = new byte [unprotectedData.Length - hashedToCheck.Length];
			Array.Copy (unprotectedData, hashedToCheck.Length, clearTextData, 0, clearTextData.Length);
			return clearTextData;
		}
	}
}
