//
// XmlDsigBase64Transform.cs - Base64 Transform implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	// http://www.w3.org/2000/09/xmldsig#base64
	public class XmlDsigBase64Transform : Transform {

		private CryptoStream cs;

		public XmlDsigBase64Transform () 
		{
			Algorithm = "http://www.w3.org/2000/09/xmldsig#base64";
		}

		public override Type[] InputTypes {
			get {
				Type[] input = new Type [3];
				input[0] = typeof (System.IO.Stream);
				input[1] = typeof (System.Xml.XmlDocument);
				input[2] = typeof (System.Xml.XmlNodeList);
				return input;
			}
		}

		public override Type[] OutputTypes {
			get {
				Type[] output = new Type [1];
				output[0] = typeof (System.IO.Stream);
				return output;
			}
		}

		protected override XmlNodeList GetInnerXml () 
		{
			return null; // THIS IS DOCUMENTED AS SUCH
		}

		public override object GetOutput () 
		{
			return (object) cs;
		}

		public override object GetOutput (Type type) 
		{
			if (type != typeof (System.IO.Stream))
				throw new ArgumentException ("type");
			return GetOutput ();
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			// documented as not changing the state of the transform
		}

		[MonoTODO("There's still one unit test failing")]
		public override void LoadInput (object obj) 
		{
			XmlNodeList xnl = null;
			Stream stream = null;

			if (obj is Stream) 
				stream = (obj as Stream);
			else if (obj is XmlDocument)
				xnl = (obj as XmlDocument).ChildNodes;
			else if (obj is XmlNodeList)
				xnl = (XmlNodeList) obj;

			if (xnl != null) {
				StringBuilder sb = new StringBuilder ();
				foreach (XmlNode xn in xnl) {
					if (xn is XmlElement)
						sb.Append (xn.InnerText);
				}

				byte[] data = Encoding.UTF8.GetBytes (sb.ToString ());
				stream = new MemoryStream (data);
			}

			if (stream != null)
				cs = new CryptoStream (stream, new FromBase64Transform (), CryptoStreamMode.Read);
			// note: there is no default are other types won't throw an exception
		}
	}
}
