// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeList.
	/// </summary>
	public class XmlSchemaSimpleTypeList : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaSimpleType itemType;
		private XmlQualifiedName itemTypeName;
		private int errorCount;

		public XmlSchemaSimpleTypeList()
		{
			this.ItemTypeName = XmlQualifiedName.Empty;
		}

		[XmlElement("simpleType",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaSimpleType ItemType 
		{
			get{ return itemType; } 
			set
			{
				itemType = value;
			}
		}

		[System.Xml.Serialization.XmlAttribute("itemType")]
		public XmlQualifiedName ItemTypeName
		{
			get{ return itemTypeName; } 
			set
			{
				itemTypeName = value;
			}
		}
		/// <remarks>
		/// 1. One of itemType or a <simpleType> must be present, but not both.
		/// 2. id must be of type ID
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			errorCount = 0;

			if(this.itemType != null && !this.ItemTypeName.IsEmpty)
				error(h, "both itemType and simpletype can't be present");
			if(this.itemType == null && this.ItemTypeName.IsEmpty)
				error(h, "one of itemType or simpletype must be present");
			if(this.itemType != null)
			{
				errorCount += this.itemType.Compile(h,info);
			}

			if(this.Id != null && !XmlSchemaUtil.CheckID(this.Id))
				error(h,"id must be a valid ID");

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
