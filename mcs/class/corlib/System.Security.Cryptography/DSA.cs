//
// System.Security.Cryptography.DSA.cs class implementation
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using Mono.Security;

// References:
// a.	FIPS PUB 186-2: Digital Signature Standard (DSS) 
//	http://csrc.nist.gov/publications/fips/fips186-2/fips186-2-change1.pdf

namespace System.Security.Cryptography {

	public abstract class DSA : AsymmetricAlgorithm	{

#if NET_2_0
		// Constructor visibility fixed in Fx 2.0
		protected DSA ()
#else
		// LAMESPEC: It says to derive new DSA implemenation from DSA class.
		// Well it's aint gonna be easy this way.
		// RSA constructor is public
		internal DSA ()
#endif
		{
		}

		public static new DSA Create ()
		{
			return Create ("System.Security.Cryptography.DSA");
		}

		public static new DSA Create (string algName) 
		{
			return (DSA) CryptoConfig.CreateFromName (algName);
		}
		
		public abstract byte[] CreateSignature (byte[] rgbHash);
		
		public abstract DSAParameters ExportParameters (bool includePrivateParameters);

		internal void ZeroizePrivateKey (DSAParameters parameters)
		{
			if (parameters.X != null)
				Array.Clear (parameters.X, 0, parameters.X.Length);
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
			
			DSAParameters dsaParams = new DSAParameters ();
			try {
				SecurityParser sp = new SecurityParser ();
				sp.LoadXml (xmlString);
				SecurityElement se = sp.ToXml ();
				if (se.Tag != "DSAKeyValue")
					throw new Exception ();
				dsaParams.P = GetNamedParam (se, "P");
				dsaParams.Q = GetNamedParam (se, "Q");
				dsaParams.G = GetNamedParam (se, "G");
				dsaParams.J = GetNamedParam (se, "J");
				dsaParams.Y = GetNamedParam (se, "Y");
				dsaParams.X = GetNamedParam (se, "X");
				dsaParams.Seed = GetNamedParam (se, "Seed");
				byte[] counter = GetNamedParam (se, "PgenCounter");
				if (counter != null) {
					byte[] counter4b = new byte [4]; // always 4 bytes
					Buffer.BlockCopy (counter, 0, counter4b, 0, counter.Length);
					dsaParams.Counter = BitConverterLE.ToInt32 (counter4b, 0);
				}
				ImportParameters (dsaParams);
			}
			catch {
				ZeroizePrivateKey (dsaParams);
				throw;
			}
			finally	{
				ZeroizePrivateKey (dsaParams);
			}
		}
		
		public abstract void ImportParameters (DSAParameters parameters);

		// note: using SecurityElement.ToXml wouldn't generate the same string as the MS implementation
		public override string ToXmlString (bool includePrivateParameters)
		{
			StringBuilder sb = new StringBuilder ();
			DSAParameters dsaParams = ExportParameters (includePrivateParameters);
			try {
				sb.Append ("<DSAKeyValue>");
				
				sb.Append ("<P>");
				sb.Append (Convert.ToBase64String (dsaParams.P));
				sb.Append ("</P>");
				
				sb.Append ("<Q>");
				sb.Append (Convert.ToBase64String (dsaParams.Q));
				sb.Append ("</Q>");

				sb.Append ("<G>");
				sb.Append (Convert.ToBase64String (dsaParams.G));
				sb.Append ("</G>");

				sb.Append ("<Y>");
				sb.Append (Convert.ToBase64String (dsaParams.Y));
				sb.Append( "</Y>");

				sb.Append ("<J>");
				sb.Append (Convert.ToBase64String (dsaParams.J));
				sb.Append ("</J>");

				if (dsaParams.Seed != null) {
					sb.Append ("<Seed>");
					sb.Append (Convert.ToBase64String (dsaParams.Seed));
					sb.Append ("</Seed>");

					sb.Append ("<PgenCounter>");
					// the number of bytes is important (no matter == 0x00)
					byte[] inArr = BitConverterLE.GetBytes (dsaParams.Counter);
					int l = inArr.Length;
					while (inArr[l-1] == 0x00)
						l--;
					if (l > 0) {
						byte[] c = new byte [l];
						Buffer.BlockCopy (inArr, 0, c, 0, l);
						sb.Append (Convert.ToBase64String (c));
					} else {
						sb.Append ("AA==");	// base64 encoded 0
					}
					sb.Append ("</PgenCounter>");
				}

				if (dsaParams.X != null) {
					sb.Append ("<X>");
					sb.Append (Convert.ToBase64String (dsaParams.X));
					sb.Append ("</X>");
				}
				else if (includePrivateParameters) {
					throw new CryptographicException (
						Locale.GetText ("Missing private key informations"));
				}

				sb.Append ("</DSAKeyValue>");
			}
			catch {
				ZeroizePrivateKey (dsaParams);
				throw;
			}
                        			
			return sb.ToString ();
		}
		
		public abstract bool VerifySignature (byte[] rgbHash, byte[] rgbSignature);
	}
}
