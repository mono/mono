// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAnyAttribute.
	/// </summary>
	public class XmlSchemaAnyAttribute : XmlSchemaAnnotated
	{
		private string nameSpace;
		private XmlSchemaContentProcessing processing;

		public XmlSchemaAnyAttribute()
		{
			nameSpace = string.Empty;
		}

		[System.Xml.Serialization.XmlAttribute("namespace")]
		public string Namespace 
		{ 
			get{ return nameSpace; } 
			set{ nameSpace = value; } 
		}
		
		[DefaultValue(XmlSchemaContentProcessing.None)]
		[System.Xml.Serialization.XmlAttribute("processContents")]
		public XmlSchemaContentProcessing ProcessContents 
		{ 
			get{ return processing; } 
			set{ processing = value; }
		}
	}
}
