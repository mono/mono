//
// System.Security.Cryptography.RSA.cs
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Text;

using Mono.Xml;

namespace System.Security.Cryptography {

	public abstract class RSA : AsymmetricAlgorithm {

		public static new RSA Create () 
		{
			return Create ("System.Security.Cryptography.RSA");
		}

		public static new RSA Create (string algName)
		{
			return (RSA) CryptoConfig.CreateFromName (algName);
		}
	
		public RSA ()
		{
		}

		public abstract byte[] EncryptValue (byte[] rgb);

		public abstract byte[] DecryptValue (byte[] rgb);

		public abstract RSAParameters ExportParameters (bool include);

		public abstract void ImportParameters (RSAParameters parameters);

		internal void ZeroizePrivateKey (RSAParameters parameters)
		{
			if (parameters.P != null)
				Array.Clear (parameters.P, 0, parameters.P.Length);
			if (parameters.Q != null)
				Array.Clear (parameters.Q, 0, parameters.Q.Length);
			if (parameters.DP != null)
				Array.Clear (parameters.DP, 0, parameters.DP.Length);
			if (parameters.DQ != null)
				Array.Clear (parameters.DQ, 0, parameters.DQ.Length);
			if (parameters.InverseQ != null)
				Array.Clear (parameters.InverseQ, 0, parameters.InverseQ.Length);
			if (parameters.D != null)
				Array.Clear (parameters.D, 0, parameters.D.Length);
		}

		private byte[] GetNamedParam (SecurityElement se, string param) 
		{
			SecurityElement sep = se.SearchForChildByTag (param);
			if (sep == null)
				return null;
			return Convert.FromBase64String (sep.Text);
		}

		public override void FromXmlString (string xmlString) 
		{
			if (xmlString == null)
				throw new ArgumentNullException ("xmlString");

			RSAParameters rsaParams = new RSAParameters ();
			try {
				SecurityParser sp = new SecurityParser ();
				sp.LoadXml (xmlString);
				SecurityElement se = sp.ToXml ();

				rsaParams.P = GetNamedParam (se, "P");
				rsaParams.Q = GetNamedParam (se, "Q");
				rsaParams.D = GetNamedParam (se, "D");
				rsaParams.DP = GetNamedParam (se, "DP");
				rsaParams.DQ = GetNamedParam (se, "DQ");
				rsaParams.InverseQ = GetNamedParam (se, "InverseQ");
				rsaParams.Exponent = GetNamedParam (se, "Exponent");
				rsaParams.Modulus = GetNamedParam (se, "Modulus");
				ImportParameters (rsaParams);
			}
			catch (Exception e) {
				ZeroizePrivateKey (rsaParams);
				throw new CryptographicException (
					Locale.GetText ("Couldn't decode XML"), e);
			}
			finally	{
				ZeroizePrivateKey (rsaParams);
			}
		}

		public override string ToXmlString (bool includePrivateParameters) 
		{
			StringBuilder sb = new StringBuilder ();
			RSAParameters rsaParams = ExportParameters (includePrivateParameters);
			try {
				sb.Append ("<RSAKeyValue>");
				
				sb.Append ("<Modulus>");
				sb.Append (Convert.ToBase64String (rsaParams.Modulus));
				sb.Append ("</Modulus>");

				sb.Append ("<Exponent>");
				sb.Append (Convert.ToBase64String (rsaParams.Exponent));
				sb.Append ("</Exponent>");

				if (includePrivateParameters) {
					// we want an ArgumentNullException is only the D is missing, but a
					// CryptographicException if other parameters (CRT) are missings
					if (rsaParams.D == null) {
						string msg = Locale.GetText ("Missing D parameter for the private key.");
						throw new ArgumentNullException (msg);
					} else if ((rsaParams.P == null) || (rsaParams.Q == null) || (rsaParams.DP == null) ||
						(rsaParams.DQ == null) || (rsaParams.InverseQ == null)) {
						// note: we can import a private key, using FromXmlString,
						// without the CRT parameters but we export it using ToXmlString!
						string msg = Locale.GetText ("Missing some CRT parameters for the private key.");
						throw new CryptographicException (msg);
					}

					sb.Append ("<P>");
					sb.Append (Convert.ToBase64String (rsaParams.P));
					sb.Append ("</P>");

					sb.Append ("<Q>");
					sb.Append (Convert.ToBase64String (rsaParams.Q));
					sb.Append ("</Q>");

					sb.Append ("<DP>");
					sb.Append (Convert.ToBase64String (rsaParams.DP));
					sb.Append ("</DP>");

					sb.Append ("<DQ>");
					sb.Append (Convert.ToBase64String (rsaParams.DQ));
					sb.Append ("</DQ>");

					sb.Append ("<InverseQ>");
					sb.Append (Convert.ToBase64String (rsaParams.InverseQ));
					sb.Append ("</InverseQ>");

					sb.Append ("<D>");
					sb.Append (Convert.ToBase64String (rsaParams.D));
					sb.Append ("</D>");
				}
				
				sb.Append ("</RSAKeyValue>");
			}
			catch {
				ZeroizePrivateKey (rsaParams);
				throw;
			}
			
			return sb.ToString ();
		}
	}
}
