// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleContent.
	/// </summary>
	public class XmlSchemaSimpleContent : XmlSchemaContentModel
	{
		private XmlSchemaContent content;
		private int errorCount;

		public XmlSchemaSimpleContent()
		{
		}

		[XmlElement("restriction",typeof(XmlSchemaSimpleContentRestriction),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("extension",typeof(XmlSchemaSimpleContentExtension),Namespace="http://www.w3.org/2001/XMLSchema")]
		public override XmlSchemaContent Content 
		{
			get{ return  content; } 
			set{ content = value; }
		}

		///<remarks>
		/// 1. Content must be present and one of restriction or extention
		///</remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(Content == null)
			{
				error(h, "Content must be present in a simpleContent");
			}
			else
			{
				if(Content is XmlSchemaSimpleContentRestriction)
				{
					XmlSchemaSimpleContentRestriction xscr = (XmlSchemaSimpleContentRestriction) Content;
					errorCount += xscr.Compile(h,info);
				}
				else if(Content is XmlSchemaSimpleContentExtension)
				{
					XmlSchemaSimpleContentExtension xsce = (XmlSchemaSimpleContentExtension) Content;
					errorCount += xsce.Compile(h,info);
				}
				else
					error(h,"simpleContent can't have any value other than restriction or extention");
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
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
