// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaExternal.
	/// </summary>
	public abstract class XmlSchemaExternal : XmlSchemaObject
	{
		private string id;
		private XmlSchema schema;
		private string location;
		private XmlAttribute[] unhandledAttributes;

		protected XmlSchemaExternal()
		{
		}
		[XmlAttribute]
		public string Id 
		{
			get{ return  id; }
			set{ id = value; }
		}
		[XmlIgnore]
		public XmlSchema Schema 
		{
			get{ return  schema; }
			set{ schema = value; }
		}
		[XmlAttribute]
		public string SchemaLocation 
		{
			get{ return  location; } 
			set{ location = value; }
		}
		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{
			get{ return  unhandledAttributes; }
			set{ unhandledAttributes = value; }
		}
	}
}
