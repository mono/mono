//
// XmlDsigBase64Transform.cs - Base64 Transform implementation for XML Signature
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
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

using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	// http://www.w3.org/2000/09/xmldsig#base64
	public class XmlDsigBase64Transform : Transform {

		private CryptoStream cs;
		private Type[] input;
		private Type[] output;

		public XmlDsigBase64Transform () 
		{
			Algorithm = XmlSignature.AlgorithmNamespaces.XmlDsigBase64Transform;
		}

		public override Type[] InputTypes {
			get {
				if (input == null) {
					input = new Type [3];
					input[0] = typeof (System.IO.Stream);
					input[1] = typeof (System.Xml.XmlDocument);
					input[2] = typeof (System.Xml.XmlNodeList);
				}
				return input;
			}
		}

		public override Type[] OutputTypes {
			get {
				if (output == null) {
					output = new Type [1];
					output[0] = typeof (System.IO.Stream);
				}
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

		public override void LoadInput (object obj) 
		{
			XmlNodeList xnl = null;
			Stream stream = null;

			if (obj is Stream) 
				stream = (obj as Stream);
			else if (obj is XmlDocument)
				xnl = (obj as XmlDocument).SelectNodes ("//.");
			else if (obj is XmlNodeList)
				xnl = (XmlNodeList) obj;

			if (xnl != null) {
				stream = new MemoryStream ();
				StreamWriter sw = new StreamWriter (stream);
				foreach (XmlNode xn in xnl) {
					switch (xn.NodeType) {
					case XmlNodeType.Attribute:
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Whitespace:
						sw.Write (xn.Value);
						break;
					}
				}
				sw.Flush ();
				// ready to be re-used
				stream.Position = 0;
			}

			if (stream != null)
				cs = new CryptoStream (stream, new FromBase64Transform (), CryptoStreamMode.Read);
			// note: there is no default are other types won't throw an exception
		}
	}
}
