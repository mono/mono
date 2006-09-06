//
// CipherReference.cs - CipherReference implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-CipherReference
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
	public sealed class CipherReference : EncryptedReference {

		#region Constructors
	
		public CipherReference ()
			: base ()
		{
		}
	
		public CipherReference (string uri)
			: base (uri)
		{
		}
	
		public CipherReference (string uri, TransformChain tc)
			: base (uri, tc)
		{
		}
	
		#endregion // Constructors
	
		#region Methods

		public override XmlElement GetXml ()
		{
			return GetXml (new XmlDocument ());
		}

		internal override XmlElement GetXml (XmlDocument document)
		{
			XmlElement xel = document.CreateElement (XmlEncryption.ElementNames.CipherReference, EncryptedXml.XmlEncNamespaceUrl);

			xel.SetAttribute (XmlEncryption.AttributeNames.URI, Uri);

			if (TransformChain != null && TransformChain.Count > 0) {
				XmlElement xtr = document.CreateElement (XmlEncryption.ElementNames.Transforms, EncryptedXml.XmlEncNamespaceUrl);
				foreach (Transform t in TransformChain) 
					xtr.AppendChild (document.ImportNode (t.GetXml (), true));
				xel.AppendChild (xtr);
			}

			return xel;
		}

		public override void LoadXml (XmlElement value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if ((value.LocalName != XmlEncryption.ElementNames.CipherReference) || (value.NamespaceURI != EncryptedXml.XmlEncNamespaceUrl))
				throw new CryptographicException ("Malformed CipherReference element.");
			base.LoadXml (value);
		}

		#endregion // Methods
	}
}

#endif
