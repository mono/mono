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
//using System.Xml;

// References:
// a.	FIPS PUB 186-2: Digital Signature Standard (DSS) 
//	http://csrc.nist.gov/publications/fips/fips186-2/fips186-2-change1.pdf

namespace System.Security.Cryptography
{
	/// <summary>
	/// Abstract base class for all implementations of the DSA algorithm
	/// </summary>
	public abstract class DSA : AsymmetricAlgorithm
	{
		// now public like RSA (but unlike MS which is internal)
		// this caused problems to compile the test suite
		public DSA () {}
	
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

		protected void ZeroizePrivateKey (DSAParameters parameters)
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
/*				XmlDocument xml = new XmlDocument ();
				xml.LoadXml (xmlString);
				dsaParams.P = GetElement (xml, "P");
				dsaParams.Q = GetElement (xml, "Q");
				dsaParams.G = GetElement (xml, "G");
				dsaParams.Y = GetElement (xml, "Y");
				dsaParams.J = GetElement (xml, "J");
				dsaParams.Seed = GetElement (xml, "Seed");
				byte[] counter = GetElement (xml, "PgenCounter");
				// else we may have an exception
				byte[] counter4b = new byte[4];
				Array.Copy (counter, 0, counter4b, 0, counter.Length);
				dsaParams.Counter = BitConverter.ToInt32 (counter4b, 0);
				dsaParams.X = GetElement (xml, "X");*/
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
				sb.Append (Convert.ToBase64String( dsaParams.Y));
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
			finally	{
				ZeroizePrivateKey (dsaParams);
			}
                        			
			return sb.ToString ();
		}
		
		public abstract bool VerifySignature (byte[] rgbHash, byte[] rgbSignature);
		
	} // DSA
	
} // System.Security.Cryptography
