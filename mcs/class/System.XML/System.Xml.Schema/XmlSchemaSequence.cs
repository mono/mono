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

		[XmlElement("element",typeof(XmlSchemaElement),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("group",typeof(XmlSchemaGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("any",typeof(XmlSchemaAny),Namespace="http://www.w3.org/2001/XMLSchema")]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}
	}
}
