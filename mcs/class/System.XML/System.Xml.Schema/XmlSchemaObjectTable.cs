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
		public XmlSchemaObject this [XmlQualifiedName name] 
		{
			get{ return (XmlSchemaObject) table [name]; }
		}
		public ICollection Names 
		{
			get{ return table.Keys; }
		}
		public ICollection Values 
		{
			get{ return table.Values; }
		}

		public bool Contains(XmlQualifiedName name)
		{
			return table.Contains(name);
		}
		public IDictionaryEnumerator GetEnumerator()
		{
			return new XmlSchemaObjectTableEnumerator (this);
		}

		internal void Add(XmlQualifiedName name, XmlSchemaObject value)
		{
			table.Add(name,value);
		}

		internal void Set(XmlQualifiedName name, XmlSchemaObject value)
		{
			table [name] = value;
		}

		internal class XmlSchemaObjectTableEnumerator : IDictionaryEnumerator
		{
			private IDictionaryEnumerator xenum;
			IEnumerable tmp;
			XmlSchemaObjectTable table;
			internal XmlSchemaObjectTableEnumerator (XmlSchemaObjectTable table)
			{
				this.table = table;
				tmp = (IEnumerable) table.table;
				xenum = (IDictionaryEnumerator) tmp.GetEnumerator ();
			}
			// Properties
			public XmlSchemaObject Current { 
				get {
					return (XmlSchemaObject) xenum.Value; 
				}
			}
			public DictionaryEntry Entry {
				get { return xenum.Entry; }
			}
			public XmlQualifiedName Key {
				get { return (XmlQualifiedName) xenum.Key; }
			}
			public XmlSchemaObject Value {
				get { return (XmlSchemaObject) xenum.Value; }
			}
			// Methods
			public bool MoveNext()
			{
				return xenum.MoveNext();
			}

			//Explicit Interface implementation
			bool IEnumerator.MoveNext()
			{
				return xenum.MoveNext();
			}
			void IEnumerator.Reset()
			{
				xenum.Reset();
			}
			object IEnumerator.Current
			{
				get { return xenum.Value; }
			}
			DictionaryEntry IDictionaryEnumerator.Entry {
				get { return xenum.Entry; }
			}
			object IDictionaryEnumerator.Key {
				get { return (XmlQualifiedName) xenum.Key; }
			}
			object IDictionaryEnumerator.Value {
				get { return (XmlSchemaObject) xenum.Value; }
			}
		}
	}
}
