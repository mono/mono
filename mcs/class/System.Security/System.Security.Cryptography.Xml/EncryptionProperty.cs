//
// EncryptionProperty.cs - EncryptionProperty implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptionProperty
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public sealed class EncryptionProperty {

		#region Fields

		XmlElement elemProp;
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

		public XmlElement PropertyElement {
			get { return elemProp; }
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
