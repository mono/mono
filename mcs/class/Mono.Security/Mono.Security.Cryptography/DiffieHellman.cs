//
// DiffieHellman.cs: Defines a base class from which all Diffie-Hellman implementations inherit
//
// Author:
//	Pieter Philippaerts (Pieter@mentalis.org)
//
// (C) 2003 The Mentalis.org Team (http://www.mentalis.org/)
//

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
using System.Text;
using System.Security;
using System.Security.Cryptography;
using Mono.Xml;
using Mono.Math;

namespace Mono.Security.Cryptography {
	/// <summary>
	/// Defines a base class from which all Diffie-Hellman implementations inherit.
	/// </summary>
	public abstract class DiffieHellman : AsymmetricAlgorithm {
		/// <summary>
		/// Creates an instance of the default implementation of the <see cref="DiffieHellman"/> algorithm.
		/// </summary>
		/// <returns>A new instance of the default implementation of DiffieHellman.</returns>
		public static new DiffieHellman Create () {
			return Create ("Mono.Security.Cryptography.DiffieHellman");
		}
		/// <summary>
		/// Creates an instance of the specified implementation of <see cref="DiffieHellman"/>.
		/// </summary>
		/// <param name="algName">The name of the implementation of DiffieHellman to use.</param>
		/// <returns>A new instance of the specified implementation of DiffieHellman.</returns>
		public static new DiffieHellman Create (string algName) {
			return (DiffieHellman) CryptoConfig.CreateFromName (algName);
		}

		/// <summary>
		/// When overridden in a derived class, creates the key exchange data. 
		/// </summary>
		/// <returns>The key exchange data to be sent to the intended recipient.</returns>
		public abstract byte[] CreateKeyExchange();
		/// <summary>
		/// When overridden in a derived class, extracts secret information from the key exchange data.
		/// </summary>
		/// <param name="keyex">The key exchange data within which the secret information is hidden.</param>
		/// <returns>The secret information derived from the key exchange data.</returns>
		public abstract byte[] DecryptKeyExchange (byte[] keyex);

		/// <summary>
		/// When overridden in a derived class, exports the <see cref="DHParameters"/>.
		/// </summary>
		/// <param name="includePrivate"><b>true</b> to include private parameters; otherwise, <b>false</b>.</param>
		/// <returns>The parameters for Diffie-Hellman.</returns>
		public abstract DHParameters ExportParameters (bool includePrivate);
		/// <summary>
		/// When overridden in a derived class, imports the specified <see cref="DHParameters"/>.
		/// </summary>
		/// <param name="parameters">The parameters for Diffie-Hellman.</param>
		public abstract void ImportParameters (DHParameters parameters);

		private byte[] GetNamedParam(SecurityElement se, string param) {
			SecurityElement sep = se.SearchForChildByTag(param);
			if (sep == null)
				return null;
			return Convert.FromBase64String(sep.Text);
		}
		/// <summary>
		/// Reconstructs a <see cref="DiffieHellman"/> object from an XML string.
		/// </summary>
		/// <param name="xmlString">The XML string to use to reconstruct the DiffieHellman object.</param>
		/// <exception cref="CryptographicException">One of the values in the XML string is invalid.</exception>
		public override void FromXmlString (string xmlString) {
			if (xmlString == null)
				throw new ArgumentNullException ("xmlString");

			DHParameters dhParams = new DHParameters();
			try {
				SecurityParser sp = new SecurityParser();
				sp.LoadXml(xmlString);
				SecurityElement se = sp.ToXml();
				if (se.Tag != "DHKeyValue")
					throw new CryptographicException();
				dhParams.P = GetNamedParam(se, "P");
				dhParams.G = GetNamedParam(se, "G");
				dhParams.X = GetNamedParam(se, "X");
				ImportParameters(dhParams);
			} finally {
				if (dhParams.P != null)
					Array.Clear(dhParams.P, 0, dhParams.P.Length);
				if (dhParams.G != null)
					Array.Clear(dhParams.G, 0, dhParams.G.Length);
				if (dhParams.X != null)
					Array.Clear(dhParams.X, 0, dhParams.X.Length);
			}
		}
		/// <summary>
		/// Creates and returns an XML string representation of the current <see cref="DiffieHellman"/> object.
		/// </summary>
		/// <param name="includePrivateParameters"><b>true</b> to include private parameters; otherwise, <b>false</b>.</param>
		/// <returns>An XML string encoding of the current DiffieHellman object.</returns>
		public override string ToXmlString (bool includePrivateParameters) {
			StringBuilder sb = new StringBuilder ();
			DHParameters dhParams = ExportParameters(includePrivateParameters);
			try {
				sb.Append ("<DHKeyValue>");
				
				sb.Append ("<P>");
				sb.Append (Convert.ToBase64String (dhParams.P));
				sb.Append ("</P>");

				sb.Append ("<G>");
				sb.Append (Convert.ToBase64String (dhParams.G));
				sb.Append ("</G>");

				if (includePrivateParameters) {
					sb.Append ("<X>");
					sb.Append (Convert.ToBase64String (dhParams.X));
					sb.Append ("</X>");
				}
				
				sb.Append ("</DHKeyValue>");
			} finally {
				Array.Clear(dhParams.P, 0, dhParams.P.Length);
				Array.Clear(dhParams.G, 0, dhParams.G.Length);
				if (dhParams.X != null)
					Array.Clear(dhParams.X, 0, dhParams.X.Length);
			}
			return sb.ToString ();
		}
	}
}