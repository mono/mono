// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

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
		private XmlQualifiedName qualifiedName;
		private int errorCount;

		public XmlSchemaNotation()
		{
		}
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return  name; } 
			set{ name = value; }
		}
		[System.Xml.Serialization.XmlAttribute("public")]
		public string Public 
		{
			get{ return  pub; } 
			set{ pub = value; }
		}
		[System.Xml.Serialization.XmlAttribute("system")]
		public string System 
		{
			get{ return  system; } 
			set{ system = value; }
		}

		[XmlIgnore]
		internal XmlQualifiedName QualifiedName 
		{
			get{ return qualifiedName;}
		}

		// 1. name and public must be present
		// public and system must be anyURI
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(Name == null)
				error(h,"Required attribute name must be present");
			else if(!XmlSchemaUtil.CheckNCName(this.name)) 
				error(h,"attribute name must be NCName");
			else
				qualifiedName = new XmlQualifiedName(Name,info.targetNS);

			if(Public==null||Public == "")
				error(h,"public must be present");
			else if(!XmlSchemaUtil.CheckAnyUri(Public))
				error(h,"public must be anyURI");

			if(system != null && XmlSchemaUtil.CheckAnyUri(system))
				error(h,"system must be anyURI");

			if(this.Id != null && !XmlSchemaUtil.CheckID(Id))
				error(h, "id must be a valid ID");

			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
				
		internal void error(ValidationEventHandler handle,string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
