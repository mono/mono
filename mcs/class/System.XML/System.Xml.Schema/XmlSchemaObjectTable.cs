// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaObjectTable.
	/// </summary>
	public class XmlSchemaObjectTable
	{
		private Hashtable table;

		internal XmlSchemaObjectTable()
		{
			table = new Hashtable(); 
		}
		public int Count 
		{
			get{ return table.Count; }
		}
		public XmlSchemaObject this[XmlQualifiedName name] 
		{
			get{ return (XmlSchemaObject) table[name]; }
		}
		public ICollection Names 
		{
			get{ return table.Keys; }
		}
		public ICollection Values 
		{
			get{ return table.Values;}
		}

		public bool Contains(XmlQualifiedName name)
		{
			return table.Contains(name);
		}
		public IDictionaryEnumerator GetEnumerator()
		{
			return table.GetEnumerator();
		}

		internal void Add(XmlQualifiedName name, XmlSchemaObject value)
		{
			table.Add(name,value);
		}
	}
}
