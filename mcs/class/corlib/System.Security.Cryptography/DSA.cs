//
// System.Security.Cryptography DSA.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using System.Text;

namespace System.Security.Cryptography
{

	/// <summary>
	/// Abstract base class for all implementations of the DSA algorithm
	/// </summary>
	public abstract class DSA : AsymmetricAlgorithm
	{
	
		public static new DSA Create()
		{
			return new DSACryptoServiceProvider();
		}

		[MonoTODO]
		public static new DSA Create(string algName)
		{
			// TODO: implement
			return null;
		}
		
		public abstract byte[] CreateSignature(byte[] rgbHash);
		
		public abstract DSAParameters ExportParameters(bool includePrivateParameters);

		[MonoTODO]
		public override void FromXmlString(string xmlString) 
		{
			if (xmlString == null)
				throw new ArgumentNullException();
			
			// TODO: implement
		}
		
		public abstract void ImportParameters(DSAParameters parameters);

		public override string ToXmlString(bool includePrivateParameters)
		{
			DSAParameters dsaParams = ExportParameters(includePrivateParameters);
			
			StringBuilder sb = new StringBuilder();
			
			sb.Append("<DSAKeyValue>");
			
			sb.Append("<P>");
			sb.Append(Convert.ToBase64String(dsaParams.P));
			sb.Append("</P>");
			
			sb.Append("<Q>");
			sb.Append(Convert.ToBase64String(dsaParams.Q));
			sb.Append("</Q>");

			sb.Append("<G>");
			sb.Append(Convert.ToBase64String(dsaParams.G));
			sb.Append("</G>");

			sb.Append("<Y>");
			sb.Append(Convert.ToBase64String(dsaParams.Y));
			sb.Append("</Y>");

			sb.Append("<J>");
			sb.Append(Convert.ToBase64String(dsaParams.J));
			sb.Append("</J>");
			
			sb.Append("<Seed>");
			sb.Append(Convert.ToBase64String(dsaParams.Seed));
			sb.Append("</Seed>");
			
			sb.Append("<PgenCounter>");
			string cnt = Convert.ToString(dsaParams.Counter);
			byte[] inArr = new ASCIIEncoding().GetBytes(cnt);
			sb.Append(Convert.ToBase64String(inArr));
			sb.Append("</PgenCounter>");

			if (dsaParams.X != null)  {
				sb.Append("<X>");
				sb.Append(Convert.ToBase64String(dsaParams.X));
				sb.Append("</X>");
			}

			sb.Append("</DSAKeyValue>");
			
			return sb.ToString();
		}
		
		public abstract bool VerifySignature(byte[] rgbHash, byte[] rgbSignature);
		
	} // DSA
	
} // System.Security.Cryptography
