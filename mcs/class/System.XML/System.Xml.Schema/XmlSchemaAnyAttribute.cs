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
		public XmlSchemaContentProcessing processing;

		public XmlSchemaAnyAttribute()
		{
			nameSpace = string.Empty;
			this.processing = XmlSchemaContentProcessing.Strict;
		}

		[XmlAttribute]
		public string Namespace 
		{ 
			get{ return nameSpace; } 
			set{ nameSpace = value; } 
		}
		[DefaultValue(XmlSchemaContentProcessing.Strict)]
		[XmlAttribute]
		public XmlSchemaContentProcessing ProcessContents 
		{ 
			get{ return processing; } 
			set{ processing = value; }
		}
	}
}
