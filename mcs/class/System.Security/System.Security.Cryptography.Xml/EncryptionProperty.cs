//
// EncryptionProperty.cs - EncryptionProperty implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptionProperty
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
	public sealed class EncryptionProperty {

		#region Fields

		string id;
		string target;

		#endregion // Fields
	
		#region Constructors

		public EncryptionProperty ()
		{
		}

		public EncryptionProperty (XmlElement elemProp)
		{
			LoadXml (elemProp);
		}

		#endregion // Constructors

		#region Properties

		public string Id {
			get { return id; }
		}

		[MonoTODO ("Always returns null")]
		public XmlElement PropertyElement {
			get { return null; }
			set { LoadXml (value); }
		}

		public string Target {
			get { return target; }
		}

		#endregion // Properties

		#region Methods

		public XmlElement GetXml ()
		{
			return GetXml (new XmlDocument ());
		}

		internal XmlElement GetXml (XmlDocument document)
		{
			XmlElement xel = document.CreateElement (XmlEncryption.ElementNames.EncryptionProperty, EncryptedXml.XmlEncNamespaceUrl);

			if (Id != null)
				xel.SetAttribute (XmlEncryption.AttributeNames.Id, Id);
			if (Target != null)
				xel.SetAttribute (XmlEncryption.AttributeNames.Target, Target);

			return xel;
		}

		public void LoadXml (XmlElement value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName != XmlEncryption.ElementNames.EncryptionProperty) || (value.NamespaceURI != EncryptedXml.XmlEncNamespaceUrl))
				throw new CryptographicException ("Malformed EncryptionProperty element.");
			else {	
				if (value.HasAttribute (XmlEncryption.AttributeNames.Id))
					this.id = value.Attributes [XmlEncryption.AttributeNames.Id].Value;
				if (value.HasAttribute (XmlEncryption.AttributeNames.Target))
					this.target = value.Attributes [XmlEncryption.AttributeNames.Target].Value;
			}
		}

		#endregion // Methods
	}
}

#endif
