//
// XmlAttributes.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Reflection;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAttributes.
	/// </summary>
	public class XmlAttributes
	{
		private XmlAnyAttributeAttribute xmlAnyAttribute;
		private XmlAnyElementAttributes xmlAnyElements;
		private XmlArrayAttribute xmlArray;
		private XmlArrayItemAttributes xmlArrayItems;
		private XmlAttributeAttribute xmlAttribute;
		private XmlChoiceIdentifierAttribute xmlChoiceIdentifier;
		private object xmlDefaultValue;
		private XmlElementAttributes xmlElements;
		private XmlEnumAttribute xmlEnum;
		private bool xmlIgnore;
		private bool xmlns;
		private XmlRootAttribute xmlRoot;
		private XmlTextAttribute xmlText;
		private XmlTypeAttribute xmlType;

		public XmlAttributes ()
		{
			xmlAnyElements = new XmlAnyElementAttributes ();
			xmlArrayItems = new XmlArrayItemAttributes ();
			xmlElements = new XmlElementAttributes ();
		}

		[MonoTODO]
		public XmlAttributes (ICustomAttributeProvider provider)
		{
			throw new NotImplementedException ();
		}

		public XmlAnyAttributeAttribute XmlAnyAttribute {
			get {
				return xmlAnyAttribute;
			}
			set {
				xmlAnyAttribute = value;
			}
		}
		public XmlAnyElementAttributes XmlAnyElements {
			get {
				return xmlAnyElements;
			}
		}
		public XmlArrayAttribute XmlArray {
			get {
				return xmlArray;
			}
			set {
				xmlArray = value;
			}
		}
		public XmlArrayItemAttributes XmlArrayItems {
			get {
				return xmlArrayItems;
			}
		}
		public XmlAttributeAttribute XmlAttribute {
			get {
				return xmlAttribute;
			}
			set {
				xmlAttribute = value;
			}
		}
		public XmlChoiceIdentifierAttribute XmlChoiceIdentifier {
			get {
				return xmlChoiceIdentifier;
			}
			set {
				xmlChoiceIdentifier = value;
			}
		}
		public object XmlDefaultValue {
			get {
				return xmlDefaultValue;
			}
			set {
				xmlDefaultValue = value;
			}
		}
		public XmlElementAttributes XmlElements {
			get {
				return xmlElements;
			}
		}
		public XmlEnumAttribute XmlEnum {
			get {
				return xmlEnum;
			}
			set {
				xmlEnum = value;
			}
		}
		public bool XmlIgnore {
			get {
				return xmlIgnore;
			}
			set {
				xmlIgnore = value;
			}
		}
		public bool Xmlns {
			get {
				return xmlns;
			}
			set {
				xmlns = value;
			}
		}
		public XmlRootAttribute XmlRoot {
			get {
				return xmlRoot;}
			set {
				xmlRoot = value;
			}
		}
		public XmlTextAttribute XmlText {
			get {
				return xmlText;
			}
			set {
				xmlText = value;
			}
		}
		public XmlTypeAttribute XmlType {
			get {
				return xmlType;
			}
			set {
				xmlType = value;
			}
		}

	}
}
