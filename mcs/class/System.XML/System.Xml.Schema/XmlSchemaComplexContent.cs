// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaComplexContent.
	/// </summary>
	public class XmlSchemaComplexContent : XmlSchemaContentModel
	{
		private XmlSchemaContent content;
		private bool isMixed;
		private int errorCount=0;

		public XmlSchemaComplexContent()
		{}

		[XmlElement("restriction",typeof(XmlSchemaComplexContentRestriction),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("extension",typeof(XmlSchemaComplexContentExtension),Namespace="http://www.w3.org/2001/XMLSchema")]
		public override XmlSchemaContent Content 
		{
			get{ return  content; } 
			set{ content = value; }
		}

		[System.Xml.Serialization.XmlAttribute("mixed")]
		public bool IsMixed 
		{
			get{ return  isMixed; } 
			set{ isMixed = value; }
		}

		/// <remarks>
		/// 1. Content must be present
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(Content == null)
			{
				error(h, "Content must be present in a complexContent");
			}
			else
			{
				if(Content is XmlSchemaComplexContentRestriction)
				{
					XmlSchemaComplexContentRestriction xscr = (XmlSchemaComplexContentRestriction) Content;
					errorCount += xscr.Compile(h,info);
				}
				else if(Content is XmlSchemaComplexContentExtension)
				{
					XmlSchemaComplexContentExtension xsce = (XmlSchemaComplexContentExtension) Content;
					errorCount += xsce.Compile(h,info);
				}
				else
					error(h,"complexContent can't have any value other than restriction or extention");
			}

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
