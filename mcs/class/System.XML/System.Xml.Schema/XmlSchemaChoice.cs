// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAll.
	/// </summary>
	public class XmlSchemaChoice : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		public XmlSchemaChoice()
		{
			items = new XmlSchemaObjectCollection();
		}
		[XmlElement]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}
	}
}
