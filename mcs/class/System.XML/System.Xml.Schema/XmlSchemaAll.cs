// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAll.
	/// </summary>
	public class XmlSchemaAll : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		public XmlSchemaAll()
		{
			items = new XmlSchemaObjectCollection();
		}
		[XmlElement("element",typeof(XmlSchemaElement),Namespace="http://www.w3.org/2001/XMLSchema")]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}
	}
}
