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
using System.ComponentModel;
using System.Collections;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAttributes.
	/// </summary>
	public class XmlAttributes
	{
		private XmlAnyAttributeAttribute xmlAnyAttribute;
		private XmlAnyElementAttributes xmlAnyElements = new XmlAnyElementAttributes();
		private XmlArrayAttribute xmlArray;
		private XmlArrayItemAttributes xmlArrayItems = new XmlArrayItemAttributes();
		private XmlAttributeAttribute xmlAttribute;
		private XmlChoiceIdentifierAttribute xmlChoiceIdentifier;
		private object xmlDefaultValue = System.DBNull.Value;
		private XmlElementAttributes xmlElements = new XmlElementAttributes();
		private XmlEnumAttribute xmlEnum;
		private bool xmlIgnore;
		private bool xmlns;
		private XmlRootAttribute xmlRoot;
		private XmlTextAttribute xmlText;
		private XmlTypeAttribute xmlType;

		public XmlAttributes ()
		{
		}

		public XmlAttributes (ICustomAttributeProvider provider)
		{
			object[] attributes = provider.GetCustomAttributes(false);
			foreach(object obj in attributes)
			{
				if(obj is XmlAnyAttributeAttribute)
					xmlAnyAttribute = (XmlAnyAttributeAttribute) obj;
				else if(obj is XmlAnyElementAttribute)
					xmlAnyElements.Add((XmlAnyElementAttribute) obj);
				else if(obj is XmlArrayAttribute)
					xmlArray = (XmlArrayAttribute) obj;
				else if(obj is XmlArrayItemAttribute)
					xmlArrayItems.Add((XmlArrayItemAttribute) obj);
				else if(obj is XmlAttributeAttribute)
					xmlAttribute = (XmlAttributeAttribute) obj;
				else if(obj is XmlChoiceIdentifierAttribute)
					xmlChoiceIdentifier = (XmlChoiceIdentifierAttribute) obj;
				else if(obj is DefaultValueAttribute)
					xmlDefaultValue = ((DefaultValueAttribute)obj).Value;
				else if(obj is XmlElementAttribute )
					xmlElements.Add((XmlElementAttribute ) obj);
				else if(obj is XmlEnumAttribute)
					xmlEnum = (XmlEnumAttribute) obj;
				else if(obj is XmlIgnoreAttribute)
					xmlIgnore = true;
				else if(obj is XmlNamespaceDeclarationsAttribute)
					xmlns = true;
				else if(obj is XmlRootAttribute)
					xmlRoot = (XmlRootAttribute) obj;
				else if(obj is XmlTextAttribute)
					xmlText = (XmlTextAttribute) obj;
				else if(obj is XmlTypeAttribute)
					xmlType = (XmlTypeAttribute) obj;
			}
		}

		#region public properties
		public XmlAnyAttributeAttribute XmlAnyAttribute 
		{
			get 
			{
				return xmlAnyAttribute;
			}
			set 
			{
				xmlAnyAttribute = value;
			}
		}
		public XmlAnyElementAttributes XmlAnyElements 
		{
			get 
			{
				return xmlAnyElements;
			}
		}
		public XmlArrayAttribute XmlArray
		{
			get 
			{
				return xmlArray;
			}
			set 
			{
				xmlArray = value;
			}
		}
		public XmlArrayItemAttributes XmlArrayItems 
		{
			get 
			{
				return xmlArrayItems;
			}
		}
		public XmlAttributeAttribute XmlAttribute 
		{
			get 
			{
				return xmlAttribute;
			}
			set 
			{
				xmlAttribute = value;
			}
		}
		public XmlChoiceIdentifierAttribute XmlChoiceIdentifier 
		{
			get 
			{
				return xmlChoiceIdentifier;
			}
		}
		public object XmlDefaultValue 
		{
			get 
			{
				return xmlDefaultValue;
			}
			set 
			{
				xmlDefaultValue = value;
			}
		}
		public XmlElementAttributes XmlElements 
		{
			get 
			{
				return xmlElements;
			}
		}
		public XmlEnumAttribute XmlEnum 
		{
			get 
			{
				return xmlEnum;
			}
			set 
			{
				xmlEnum = value;
			}
		}
		public bool XmlIgnore 
		{
			get 
			{
				return xmlIgnore;
			}
			set 
			{
				xmlIgnore = value;
			}
		}
		public bool Xmlns 
		{
			get 
			{
				return xmlns;
			}
			set 
			{
				xmlns = value;
			}
		}
		public XmlRootAttribute XmlRoot 
		{
			get 
			{
				return xmlRoot;}
			set 
			{
				xmlRoot = value;
			}
		}
		public XmlTextAttribute XmlText 
		{
			get 
			{
				return xmlText;
			}
			set 
			{
				xmlText = value;
			}
		}
		public XmlTypeAttribute XmlType 
		{
			get 
			{
				return xmlType;
			}
			set 
			{
				xmlType = value;
			}
		}
		#endregion
		
		internal bool InternalEquals (XmlAttributes other)
		{
			if (other == null) return false;
			
			if (xmlIgnore != other.xmlIgnore) return false;
			if (xmlns != other.xmlns) return false;
			
			if (xmlAnyAttribute == null) {
				if (other.xmlAnyAttribute != null) return false; }
			else
				if (other.xmlAnyAttribute == null) return false;
			
			if (!xmlAnyElements.Equals (other.xmlAnyElements)) return false; 
			if (!xmlArrayItems.Equals (other.xmlArrayItems)) return false;
			if (!xmlElements.Equals (other.xmlElements)) return false;
				
			if (xmlArray == null) {
				if (other.xmlArray != null) return false; }
			else
				if (!xmlArray.InternalEquals (other.xmlArray)) return false;
				
			if (xmlAttribute == null) {
				if (other.xmlAttribute != null) return false; }
			else
				if (!xmlAttribute.InternalEquals (other.xmlAttribute)) return false;
				
			if (xmlDefaultValue == null) {
				if (other.xmlDefaultValue != null) return false; }
			else
				if (!xmlDefaultValue.Equals (other.xmlDefaultValue)) return false;
				
			if (xmlEnum == null) {
				if (other.xmlEnum != null) return false; }
			else
				if (!xmlEnum.InternalEquals (other.xmlEnum)) return false;
				
			if (xmlRoot == null) {
				if (other.xmlRoot != null) return false; }
			else
				if (!xmlRoot.InternalEquals (other.xmlRoot)) return false;
				
			if (xmlText == null) {
				if (other.xmlText != null) return false; }
			else
				if (!xmlText.InternalEquals (other.xmlText)) return false;
				
			if (xmlType == null) {
				if (other.xmlType != null) return false; }
			else
				if (!xmlType.InternalEquals (other.xmlType)) return false;
				
			if (xmlChoiceIdentifier == null) {
				if (other.xmlChoiceIdentifier != null) return false; }
			else
				if (!xmlChoiceIdentifier.InternalEquals (other.xmlChoiceIdentifier)) return false;
				
			return true;
		}
	}
}
