// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaDocumentation.
	/// </summary>
	public class XmlSchemaDocumentation : XmlSchemaObject
	{
		private string language;
		private XmlNode[] markup;
		private string source;

		public XmlSchemaDocumentation()
		{
			source = string.Empty;
		}
		[XmlAttribute]
		public string Language 
		{
			get{ return  language; }
			set{ language = value; }
		}
		[XmlAnyElement]
		[XmlText]
		public XmlNode[] Markup 
		{
			get{ return  markup; }
			set{ markup = value; }
		}
		[XmlAttribute]
		public string Source 
		{
			get{ return  source; } 
			set{ source = value; }
		}
	}
}
