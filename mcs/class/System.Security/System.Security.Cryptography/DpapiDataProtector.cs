//
// DpapiDataProtector.cs: Protect (encrypt) data without (user involved) key management
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

namespace System.Security.Cryptography {
	/// <summary>Provides Data Protection through the <see cref="ProtectedData"/> class.</summary>
	public sealed class DpapiDataProtector : DataProtector {
		/// <summary>Gets or sets the scope of the data protection. The default is <see cref="DataProtectionScope.CurrentUser" />.</summary>
		public DataProtectionScope Scope {
			get; set;
		}

		/// <summary>Specifies whether the hash is prepended to the byte array before encryption.</summary>
		/// <returns>Always false, as the hash is used as additional entropy in the <see cref="ProtectedData"/> calls.</returns>
		protected override bool PrependHashedPurposeToPlaintext {
			get {
				return false;
			}
		}

		/// <summary>Creates a new instance of the <see cref="DpapiDataProtector" /> class by using the specified
		/// application name, primary purpose, and specific purposes.</summary>
		/// <param name="applicationName">The name of the application.</param>
		/// <param name="primaryPurpose">The primary purpose for the data protector.</param>
		/// <param name="specificPurposes">The specific purpose(s) for the data protector.</param>
		/// <exception cref="ArgumentException">
		///      <paramref name="applicationName" /> is an empty string, a string constisting of only whitespace, or null.
		/// -or- <paramref name="primaryPurpose" /> is an empty string, a string constisting of only whitespace, or null.
		/// -or- <paramref name="specificPurposes" /> contains an empty string, a string constisting of only whitespace, or null.</exception>
		public DpapiDataProtector (string applicationName, string primaryPurpose, params string [] specificPurposes)
			: base (applicationName, primaryPurpose, specificPurposes)
		{
			Scope = DataProtectionScope.CurrentUser;
		}
		/// <summary>Determines if the data must be re-encrypted.</summary>
		/// <param name="encryptedData">The encrypted data to be checked.</param>
		/// <returns>Always true.</returns>
		public override bool IsReprotectRequired (byte [] encryptedData)
		{
			return true;
		}

		/// <summary>Specifies the delegate method in the derived class that the <see cref="DataProtector.Protect(byte[])" />
		/// method in the base class calls back into.
		/// This calls directly into <see cref="ProtectedData.Protect(byte[],byte[],DataProtectionScope)"/>.</summary>
		/// <param name="userData">The user data.</param>
		/// <returns>A byte array with the encrypted data.</returns>
		protected override byte [] ProviderProtect (byte [] userData)
		{
			return ProtectedData.Protect (userData, GetHashedPurpose (), Scope);
		}

		/// <summary>Specifies the delegate method in the derived class that the <see cref="DataProtector.Unprotect(byte[])" />
		///  method in the base class calls back into.
		/// This calls directly into <see cref="ProtectedData.Unprotect(byte[],byte[],DataProtectionScope)"/>.</summary>
		/// <param name="encryptedData">The data to be unencrypted.</param>
		/// <returns>A byte array with the clear text data.</returns>
		protected override byte [] ProviderUnprotect (byte [] encryptedData)
		{
			return ProtectedData.Unprotect (encryptedData, GetHashedPurpose (), Scope);
		}
	}
}
