//
// CipherData.cs - CipherData implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-CipherData
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

using System.Security.Cryptography;
using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml {
	public sealed class CipherData {

		#region Fields

		byte[] cipherValue;
		CipherReference cipherReference;
	
		#endregion // Fields
	
		#region Constructors
	
		public CipherData ()
		{
		}
	
		public CipherData (byte[] cipherValue)
		{
			CipherValue = cipherValue;
		}
	
		public CipherData (CipherReference cipherReference)
		{
			CipherReference = cipherReference;
		}
	
		#endregion // Constructors
	
		#region Properties
	
		public CipherReference CipherReference {
			get { return cipherReference; }
			set { 
				if (CipherValue != null)
					throw new CryptographicException ("A Cipher Data element should have either a CipherValue or a CipherReference element.");
				cipherReference = value;
			}
		}
	
		public byte[] CipherValue {
			get { return cipherValue; }
			set {
				if (CipherReference != null)
					throw new CryptographicException ("A Cipher Data element should have either a CipherValue or a CipherReference element.");
				cipherValue = value;
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
			if (CipherReference == null && CipherValue == null)
				throw new CryptographicException ("A Cipher Data element should have either a CipherValue or a CipherReference element.");

			XmlElement xel = document.CreateElement (XmlEncryption.ElementNames.CipherData, EncryptedXml.XmlEncNamespaceUrl);
			if (CipherReference != null) 
				xel.AppendChild (document.ImportNode (cipherReference.GetXml (), true));

			if (CipherValue != null) {
				XmlElement xcv = document.CreateElement (XmlEncryption.ElementNames.CipherValue, EncryptedXml.XmlEncNamespaceUrl);
				StreamReader reader = new StreamReader (new CryptoStream (new MemoryStream (cipherValue), new ToBase64Transform (), CryptoStreamMode.Read));
				xcv.InnerText = reader.ReadToEnd ();
				reader.Close ();
				xel.AppendChild (xcv);
			}
			return xel;
		}

		public void LoadXml (XmlElement value)
		{
			CipherReference = null;
			CipherValue = null;

			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName != XmlEncryption.ElementNames.CipherData) || (value.NamespaceURI != EncryptedXml.XmlEncNamespaceUrl)) 
				throw new CryptographicException ("Malformed Cipher Data element.");
			else {
				foreach (XmlNode n in value.ChildNodes) {
					if (n is XmlWhitespace)
						continue;

					switch (n.LocalName) {
					case XmlEncryption.ElementNames.CipherReference:
						cipherReference = new CipherReference ();
						cipherReference.LoadXml ((XmlElement) n);
						break;
					case XmlEncryption.ElementNames.CipherValue:
						CipherValue = Convert.FromBase64String (n.InnerText);
						break;
					}
				}

				if (CipherReference == null && CipherValue == null)
					throw new CryptographicException ("A Cipher Data element should have either a CipherValue or a CipherReference element.");
			}
		}

		#endregion // Methods
	}
}

#endif
