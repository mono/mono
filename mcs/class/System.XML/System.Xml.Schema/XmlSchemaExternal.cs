// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
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
		{}
		
		[System.Xml.Serialization.XmlAttribute("schemaLocation")]
		public string SchemaLocation 
		{
			get{ return  location; } 
			set{ location = value; }
		}

		[XmlIgnore]
		public XmlSchema Schema 
		{
			get{ return  schema; }
			set{ schema = value; }
		}

		[System.Xml.Serialization.XmlAttribute("id")]
		public string Id 
		{
			get{ return  id; }
			set{ id = value; }
		}

		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{
			get
			{
				if(unhandledAttributeList != null)
				{
					unhandledAttributes = (XmlAttribute[]) unhandledAttributeList.ToArray(typeof(XmlAttribute));
					unhandledAttributeList = null;
				}
				return unhandledAttributes;
			}
			set
			{ 
				unhandledAttributes = value; 
				unhandledAttributeList = null;
			}
		}
	}
}
