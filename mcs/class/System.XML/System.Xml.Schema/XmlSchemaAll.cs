// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAll.
	/// </summary>
	public class XmlSchemaAll : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		private int errorCount=0;
		public XmlSchemaAll()
		{
			items = new XmlSchemaObjectCollection();
		}
		[XmlElement("element",typeof(XmlSchemaElement),Namespace="http://www.w3.org/2001/XMLSchema")]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}
		/// <remarks>
		/// 1. MaxOccurs must be one. (default is also one)
		/// 2. MinOccurs must be zero or one.
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(MaxOccurs != Decimal.One)
				error(h,"maxOccurs must be 1");
			if(MinOccurs != Decimal.One && MinOccurs != Decimal.Zero)
				error(h,"minOccurs must be 0 or 1");

			foreach(XmlSchemaObject obj in Items)
			{
				if(obj is XmlSchemaElement)
				{
					errorCount += ((XmlSchemaElement)obj).Compile(h,info);
				}
				else
				{
					error(h,"XmlSchemaAll can only contain Items of type Element");
				}
			}
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
