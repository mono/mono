// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaNotation.
	/// </summary>
	public class XmlSchemaNotation : XmlSchemaAnnotated
	{
		private string name;
		private string pub;
		private string system;

		public XmlSchemaNotation()
		{
		}
		[XmlAttribute]
		public string Name 
		{
			get{ return  name; } 
			set{ name = value; }
		}
		[XmlAttribute]
		public string Public 
		{
			get{ return  pub; } 
			set{ pub = value; }
		}
		[XmlAttribute]
		public string System 
		{
			get{ return  system; } 
			set{ system = value; }
		}
	}
}
