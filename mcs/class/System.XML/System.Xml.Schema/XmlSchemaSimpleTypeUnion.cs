// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeUnion.
	/// </summary>
	public class XmlSchemaSimpleTypeUnion : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaObjectCollection baseTypes;
		private XmlQualifiedName[] memberTypes;

		public XmlSchemaSimpleTypeUnion()
		{
			baseTypes = new XmlSchemaObjectCollection();
		}
		[XmlElement]
		public XmlSchemaObjectCollection BaseTypes 
		{
			get{ return baseTypes; }
		}
		[XmlAttribute]
		public XmlQualifiedName[] MemberTypes 
		{
			get{ return  memberTypes; } 
			set{ memberTypes = value; }
		}
	}
}
