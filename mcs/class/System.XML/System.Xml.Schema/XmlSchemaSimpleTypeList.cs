// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeList.
	/// </summary>
	public class XmlSchemaSimpleTypeList : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaSimpleType itemType;
		private XmlQualifiedName itemTypeName;

		public XmlSchemaSimpleTypeList()
		{}

		[XmlElement("simpleType",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaSimpleType ItemType 
		{
			get{ return itemType; } 
			set
			{
				itemType = value;
				itemTypeName = null;
			}
		}

		[System.Xml.Serialization.XmlAttribute("itemType")]
		public XmlQualifiedName ItemTypeName
		{
			get{ return itemTypeName; } 
			set
			{
				itemTypeName = value;
				itemType = null;
			}
		}
	}
}
