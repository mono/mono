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
		private int errorCount;

		public XmlSchemaAnyAttribute()
		{
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

		/// <remarks>
		/// 1. id must be of type ID
		/// 2. namespace can have one of the following values:
		///		a) ##any or ##other
		///		b) list of anyURI and ##targetNamespace and ##local
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			errorCount = 0;

			if(Id != null && !XmlSchemaUtil.CheckID(Id))
			{
				error(h, "id attribute must be a valid ID");
			}
			//define ##any=1,##other=2,##targetNamespace=4,##local=8,anyURI=16
			int nscount = 0;
			string[] nslist = XmlSchemaUtil.SplitList(Namespace);
			foreach(string ns in nslist)
			{
				switch(ns)
				{
					case "##any": 
						nscount |= 1;
						break;
					case "##other":
						nscount |= 2;
						break;
					case "##targetNamespace":
						nscount |= 4;
						break;
					case "##local":
						nscount |= 8;
						break;
					default:
						if(!XmlSchemaUtil.CheckAnyUri(ns))
							error(h,"the namespace is not a valid anyURI");
						else
							nscount |= 16;
						break;
				}
			}
			if((nscount&1) == 1 && nscount != 1)
				error(h,"##any if present must be the only namespace attribute");
			if((nscount&2) == 2 && nscount != 2)
				error(h,"##other if present must be the only namespace attribute");
			
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
				
		internal void error(ValidationEventHandler handle,string message)
		{
			this.errorCount++;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
