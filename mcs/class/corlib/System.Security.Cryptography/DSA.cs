//
// System.Security.Cryptography.DSA.cs class implementation
//
// Authors:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//   Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Text;

// References:
// a.	FIPS PUB 186-2: Digital Signature Standard (DSS) 
//	http://csrc.nist.gov/publications/fips/fips186-2/fips186-2-change1.pdf

namespace System.Security.Cryptography
{
	internal class DSAHandler : MiniParser.IHandler {

		private DSAParameters dsa;
		private bool unknown;
		private byte[] temp;

		public DSAHandler () 
		{
			dsa = new DSAParameters();
		}

		public DSAParameters GetParams () 
		{
			return dsa;
		}

		public void OnStartParsing (MiniParser parser) {}

		public void OnStartElement (string name, MiniParser.IAttrList attrs) {}

		public void OnEndElement (string name) 
		{
			switch (name) {
				case "P":
					dsa.P = temp;
					break;
				case "Q":
					dsa.Q = temp;
					break;
				case "G":
					dsa.G = temp;
					break;
				case "J":
					dsa.J = temp;
					break;
				case "Y":
					dsa.Y = temp;
					break;
				case "X":
					dsa.X = temp;
					break;
				case "Seed":
					dsa.Seed = temp;
					break;
				case "PgenCounter":
					byte[] counter4b = new byte[4];
					Array.Copy (temp, 0, counter4b, 0, temp.Length);
					dsa.Counter = BitConverter.ToInt32 (counter4b, 0);
					break;
				default:
					// unknown tag in parameters
					break;
			}
		}

		public void OnChars (string ch) 
		{
			temp = Convert.FromBase64String (ch);
		}

		public void OnEndParsing (MiniParser parser) {}
	}

	/// <summary>
	/// Abstract base class for all implementations of the DSA algorithm
	/// </summary>
	public abstract class DSA : AsymmetricAlgorithm
	{
		// LAMESPEC: It says to derive new DSA implemenation from DSA class.
		// Well it's aint gonna be easy this way.
		// RSA constructor is public
		internal DSA () {}
	
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

		public override void FromXmlString (string xmlString) 
		{
			if (xmlString == null)
				throw new ArgumentNullException ();
			
			DSAParameters dsaParams = new DSAParameters ();
			try {
				MiniParser parser = new MiniParser ();
				AsymmetricParameters reader = new AsymmetricParameters (xmlString);
				DSAHandler handler = new DSAHandler ();
				parser.Parse(reader, handler);
				ImportParameters (handler.GetParams ());
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
				
				sb.Append ("<Seed>");
				sb.Append (Convert.ToBase64String (dsaParams.Seed));
				sb.Append ("</Seed>");
				
				sb.Append ("<PgenCounter>");
				// the number of bytes is important (no matter == 0x00)
				byte[] inArr = BitConverter.GetBytes (dsaParams.Counter);
				int l = inArr.Length;
				while (inArr[l-1] == 0x00)
					l--;
				byte[] c = new byte[l];
				Array.Copy (inArr, 0, c, 0, l);
				sb.Append (Convert.ToBase64String (c));
				sb.Append ("</PgenCounter>");

				if (dsaParams.X != null) {
					sb.Append ("<X>");
					sb.Append (Convert.ToBase64String (dsaParams.X));
					sb.Append ("</X>");
				}
				else if (includePrivateParameters)
					throw new CryptographicException();

				sb.Append ("</DSAKeyValue>");
			}
			catch {
				ZeroizePrivateKey (dsaParams);
				throw;
			}
                        			
			return sb.ToString ();
		}
		
		public abstract bool VerifySignature (byte[] rgbHash, byte[] rgbSignature);
		
	} // DSA
	
} // System.Security.Cryptography
