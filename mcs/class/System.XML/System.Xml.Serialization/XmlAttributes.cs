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
		private object xmlDefaultValue;
		private XmlElementAttributes xmlElements = new XmlElementAttributes();
		private XmlEnumAttribute xmlEnum;
		private bool xmlIgnore;
		private bool xmlns;
		private XmlRootAttribute xmlRoot;
		private XmlTextAttribute xmlText;
		private XmlTypeAttribute xmlType;

		private MemberInfo minfo;
		private FieldInfo  finfo;
		private PropertyInfo pinfo;
		internal ArrayList XmlIncludes = new ArrayList();
		//internal string ElementName;

		//The element Order in serialization.
		internal int order;
		internal bool isAttribute;
		internal static XmlAttributes.XmlAttributesComparer attrComparer;

		//Sorting Order of Elements: XmlNs, XmlAttributes, XmlElement
		internal class XmlAttributesComparer : IComparer
		{
			public int Compare(object x,object y)
			{
				if(x is XmlAttributes && y is XmlAttributes)
				{
					XmlAttributes attx = (XmlAttributes)x;
					XmlAttributes atty = (XmlAttributes)y;
					if(attx.xmlns)
						return -1;
					if(atty.xmlns)
						return 1;
					if(attx.isAttribute)
						return -1;
					if(atty.isAttribute)
						return 1;
					int diff = attx.order - atty.order;
					if(diff == 0)
						return 0;
					if(diff > 0)
						return 1;
					if(diff < 0)
						return -1;
				}
				if(x == null)
					return -1;
				if(y == null)
					return 1;
				throw new Exception("Should never occur. XmlAttributesComparer.Compare");
			}
		}

		public XmlAttributes ()
		{
		}

		static XmlAttributes ()
		{
			attrComparer = new XmlAttributes.XmlAttributesComparer();
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
					xmlDefaultValue = obj;
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

		#region internal properties
		internal MemberInfo MemberInfo
		{
			get { return  minfo; }
			set { minfo = value; }
		}

		internal FieldInfo FieldInfo 
		{
			get { return  finfo; }
			set { finfo = value; }
		}

		internal PropertyInfo PropertyInfo
		{
			get { return  pinfo; }
			set { pinfo = value; }
		}
		#endregion

		//Only permissible attributes for a class type are: XmlRoot and XmlInclude
		internal static XmlAttributes FromClass(Type classType)
		{
			XmlAttributes XmlAttr = new XmlAttributes();
			object[] attributes = classType.GetCustomAttributes(false);
			foreach(object obj in attributes)
			{
				if(obj is XmlRootAttribute)
					XmlAttr.xmlRoot = (XmlRootAttribute) obj;
				else if(obj is XmlIncludeAttribute)
					XmlAttr.XmlIncludes.Add(obj);
			}
			return XmlAttr;
		}

		internal static XmlAttributes FromField(MemberInfo member, FieldInfo finfo)
		{
			XmlAttributes XmlAttr = new XmlAttributes();
			object[] attributes = member.GetCustomAttributes(false);
			XmlAttr.AddMemberAttributes(attributes);

			XmlAttr.minfo = member;
			XmlAttr.finfo = finfo;

			return XmlAttr;
		}

		
		internal static XmlAttributes FromProperty(MemberInfo member, PropertyInfo pinfo)
		{

			XmlAttributes XmlAttr = new XmlAttributes();
			object[] attributes = member.GetCustomAttributes(false);
			XmlAttr.AddMemberAttributes(attributes);

			XmlAttr.minfo = member;
			XmlAttr.pinfo = pinfo;
			return XmlAttr;
		}

		internal void AddMemberAttributes(object[] attributes)
		{
			foreach(object obj in attributes)
			{
				if(obj is XmlAnyAttributeAttribute)
				{
					xmlAnyAttribute = (XmlAnyAttributeAttribute) obj;
					isAttribute = true;	
				}
				else if(obj is XmlAttributeAttribute)
				{
					xmlAttribute = (XmlAttributeAttribute) obj;
					isAttribute = true;
				}
				else if(obj is XmlNamespaceDeclarationsAttribute)
				{
					xmlns = true;
					isAttribute = true;
				}
				else if(obj is XmlAnyElementAttribute)
				{
					xmlAnyElements.Add((XmlAnyElementAttribute) obj);
					order = ((XmlAnyElementAttribute) obj).Order;
				}
				else if(obj is XmlArrayAttribute)
				{
					xmlArray = (XmlArrayAttribute) obj;
					order = ((XmlArrayAttribute) obj).Order;
				}
				else if(obj is XmlArrayItemAttribute)
				{
					xmlArrayItems.Add((XmlArrayItemAttribute) obj);
					order = ((XmlArrayItemAttribute) obj).Order;
				}
				else if(obj is XmlChoiceIdentifierAttribute)
				{
					xmlChoiceIdentifier = (XmlChoiceIdentifierAttribute) obj;
					order = ((XmlChoiceIdentifierAttribute) obj).Order;
				}
				else if(obj is XmlTextAttribute)
				{
					xmlText = (XmlTextAttribute) obj;
					order = ((XmlTextAttribute) obj).Order;
				}
				else if(obj is XmlElementAttribute )
				{
					xmlElements.Add((XmlElementAttribute ) obj);
					order = ((XmlElementAttribute ) obj).Order;
				}
				else if(obj is DefaultValueAttribute)
				{
					xmlDefaultValue = ((DefaultValueAttribute ) obj).Value;
				}
				else if(obj is XmlEnumAttribute)
				{
					xmlEnum = (XmlEnumAttribute) obj;
				}
				else if(obj is XmlIgnoreAttribute)
				{
					xmlIgnore = true;
				}
				else if(obj is XmlRootAttribute)
				{
					throw new Exception("should never happen. XmlRoot on a member");
				}
				else if(obj is XmlTypeAttribute)
				{
					xmlType = (XmlTypeAttribute) obj;
				}
			}
		}

		internal string GetAttributeName(Type type, string defaultName)
		{
			if(XmlAttribute != null && XmlAttribute.AttributeName != null && XmlAttribute.AttributeName != "")
				return XmlAttribute.AttributeName;
			else if (XmlType != null && XmlType.TypeName != null && XmlType.TypeName != "")
				return XmlType.TypeName;
			return defaultName;
		}

		internal string GetElementName(Type type, string defaultName)
		{
			string anonymousElemAttrName = null;
			foreach(XmlElementAttribute elem in XmlElements)
			{
				if(elem.Type == type && elem.ElementName != null && elem.ElementName != "")
					return elem.ElementName;
				else if(elem.Type == null && elem.ElementName != null && elem.ElementName != "")
					anonymousElemAttrName = elem.ElementName;
			}
			if (anonymousElemAttrName != null)
				return anonymousElemAttrName;

			if (XmlType != null && XmlType.TypeName != null && XmlType.TypeName != "")
				return XmlType.TypeName;
			return defaultName;
		}

		internal string GetAttributeNamespace(Type type)
		{
			if(XmlAttribute != null)
				return XmlAttribute.Namespace;
			return null;
		}

		internal string GetElementNamespace(Type type)
		{
			string defaultNS = null;
			foreach(XmlElementAttribute elem in XmlElements)
			{
				if(elem.Type == type )
					return elem.Namespace;
				else if(elem.Type == null)
					defaultNS = elem.Namespace;
			}
			return defaultNS;
		}

		internal bool GetElementIsNullable (Type type)
		{
			bool defaultIsNullable = false;
			foreach(XmlElementAttribute elem in XmlElements)
			{
				if(elem.Type == type)
					return elem.IsNullable;
				else if(elem.Type == null)
					defaultIsNullable = elem.IsNullable;
			}
			return defaultIsNullable;
		}
	}
}
