// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleType.
	/// </summary>
	public class XmlSchemaSimpleType : XmlSchemaType
	{
		private XmlSchemaSimpleTypeContent content;

		public XmlSchemaSimpleType()
		{
		}

		[XmlElement("restriction",typeof(XmlSchemaSimpleTypeRestriction),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("list",typeof(XmlSchemaSimpleTypeList),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("union",typeof(XmlSchemaSimpleTypeUnion),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaSimpleTypeContent Content
		{
			get{ return  content; } 
			set{ content = value; }
		}
	}
}
