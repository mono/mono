//
// EncryptionMethod.cs - EncryptionMethod implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptionMethod
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

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

#if NET_2_0

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public class EncryptionMethod {

		#region Fields

		string algorithm;
		int keySize;

		#endregion // Fields
	
		#region Constructors

		public EncryptionMethod ()
		{
			KeyAlgorithm = null;
		}

		public EncryptionMethod (string strAlgorithm)
		{
			KeyAlgorithm = strAlgorithm;
		}

		#endregion // Constructors

		#region Properties

		public string KeyAlgorithm {
			get { return algorithm; }
			set { algorithm = value; }
		}

		public int KeySize {
			get { return keySize; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("The key size should be a non negative integer.");
				keySize = value; 
			}
		}

		#endregion // Properties

		#region Methods

		public XmlElement GetXml ()
		{
			return GetXml (new XmlDocument ());
		}

		internal XmlElement GetXml (XmlDocument document)
		{
			XmlElement xel = document.CreateElement (XmlEncryption.ElementNames.EncryptionMethod, EncryptedXml.XmlEncNamespaceUrl);

			if (KeySize != 0) {
				XmlElement xks = document.CreateElement (XmlEncryption.ElementNames.KeySize, EncryptedXml.XmlEncNamespaceUrl);
				xks.InnerText = String.Format ("{0}", keySize);
				xel.AppendChild (xks);
			}

			if (KeyAlgorithm != null)
				xel.SetAttribute (XmlEncryption.AttributeNames.Algorithm, KeyAlgorithm);
			return xel;
		}

		public void LoadXml (XmlElement value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if ((value.LocalName != XmlEncryption.ElementNames.EncryptionMethod) || (value.NamespaceURI != EncryptedXml.XmlEncNamespaceUrl))
				throw new CryptographicException ("Malformed EncryptionMethod element.");
			else {
				KeyAlgorithm = null;
				foreach (XmlNode n in value.ChildNodes) {
					if (n is XmlWhitespace)
						continue;
					switch (n.LocalName) {
					case XmlEncryption.ElementNames.KeySize:
						KeySize = Int32.Parse (n.InnerText);
						break;
					}
				}
				if (value.HasAttribute (XmlEncryption.AttributeNames.Algorithm))
					KeyAlgorithm = value.Attributes [XmlEncryption.AttributeNames.Algorithm].Value;
			}
		}

		#endregion // Methods
	}
}

#endif
