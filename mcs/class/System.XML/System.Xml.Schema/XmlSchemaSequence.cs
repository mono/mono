// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSequence.
	/// </summary>
	public class XmlSchemaSequence : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		public XmlSchemaSequence()
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
