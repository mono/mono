// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaContent.
	/// </summary>
	public abstract class XmlSchemaContent : XmlSchemaAnnotated
	{
		internal object actualBaseSchemaType;

		protected XmlSchemaContent()
		{}

		internal object ActualBaseSchemaType
		{
			get { return actualBaseSchemaType; }
		}
	}
}