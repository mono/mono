//
// System.Security.Cryptography.RSA.cs
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Text;

using Mono.Xml;

namespace System.Security.Cryptography {

	internal class RSAHandler : MiniParser.IHandler {

		private RSAParameters rsa;
		private byte[] temp;

		public RSAHandler () 
		{
			rsa = new RSAParameters ();
		}

		public RSAParameters GetParams () 
		{
			return rsa;
		}

		public void OnStartParsing (MiniParser parser) {}

		public void OnStartElement (string name, MiniParser.IAttrList attrs) {}

		public void OnEndElement (string name) {
			switch (name) {
				case "P":
					rsa.P = temp;
					break;
				case "Q":
					rsa.Q = temp;
					break;
				case "D":
					rsa.D = temp;
					break;
				case "DP":
					rsa.DP = temp;
					break;
				case "DQ":
					rsa.DQ = temp;
					break;
				case "Exponent":
					rsa.Exponent = temp;
					break;
				case "InverseQ":
					rsa.InverseQ = temp;
					break;
				case "Modulus":
					rsa.Modulus = temp;
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

	public abstract class RSA : AsymmetricAlgorithm 
	{
		public static new RSA Create () 
		{
			return Create ("System.Security.Cryptography.RSA");
		}

		public static new RSA Create (string algName)
		{
			return (RSA) CryptoConfig.CreateFromName (algName);
		}
	
		public RSA () { }

		public abstract byte[] EncryptValue (byte[] rgb);
		public abstract byte[] DecryptValue (byte[] rgb);

		public abstract RSAParameters ExportParameters (bool include);
		public abstract void ImportParameters (RSAParameters parameters);

		internal void ZeroizePrivateKey (RSAParameters parameters)
		{
			if (parameters.P != null)
				Array.Clear(parameters.P, 0, parameters.P.Length);
			if (parameters.Q != null)
				Array.Clear(parameters.Q, 0, parameters.Q.Length);
			if (parameters.DP != null)
				Array.Clear(parameters.DP, 0, parameters.DP.Length);
			if (parameters.DQ != null)
				Array.Clear(parameters.DQ, 0, parameters.DQ.Length);
			if (parameters.InverseQ != null)
				Array.Clear(parameters.InverseQ, 0, parameters.InverseQ.Length);
			if (parameters.D != null)
				Array.Clear(parameters.D, 0, parameters.D.Length);
		}

		public override void FromXmlString (string xmlString) 
		{
			if (xmlString == null)
				throw new ArgumentNullException ();

			RSAParameters rsaParams = new RSAParameters ();
			try {
				MiniParser parser = new MiniParser ();
				AsymmetricParameters reader = new AsymmetricParameters (xmlString);
				RSAHandler handler = new RSAHandler ();
				parser.Parse(reader, handler);
				ImportParameters (handler.GetParams ());
			}
			catch {
				ZeroizePrivateKey (rsaParams);
				throw new CryptographicException ();
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

				if (includePrivateParameters)
				{
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
