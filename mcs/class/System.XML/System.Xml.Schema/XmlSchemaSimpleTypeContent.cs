// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeContent.
	/// </summary>
	public abstract class XmlSchemaSimpleTypeContent : XmlSchemaAnnotated
	{
		protected XmlSchemaSimpleTypeContent()
		{
		}

		internal XmlSchemaSimpleType OwnerType;

//		private object actualBaseSchemaType = XmlSchemaSimpleType.AnySimpleType;

		internal object ActualBaseSchemaType {
			get { return OwnerType.BaseSchemaType; }
		}

		internal virtual string Normalize (string s, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return s;
		}

	}
}
