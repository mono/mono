// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaDatatype.
	/// </summary>
	public abstract class XmlSchemaDatatype
	{
		protected XmlSchemaDatatype()
		{
		}
		
		public abstract XmlTokenizedType TokenizedType {  get; }
		public abstract Type ValueType {  get; }

		// Methods
		public abstract object ParseValue(string s, 
			XmlNameTable nameTable, XmlNamespaceManager nsmgr);

		//TODO: This should return appropriate inbuilt type
		internal static XmlSchemaDatatype GetType(XmlQualifiedName qname)
		{
			return null;
		}
	}
}
