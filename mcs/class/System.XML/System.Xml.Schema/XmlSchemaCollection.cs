// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;
using System.Xml;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaCollection.
	/// </summary>
	public sealed class XmlSchemaCollection : ICollection, IEnumerable
	{
		//private fields
		private Hashtable htable;
		private XmlNameTable ntable;

		[MonoTODO]
		public XmlSchemaCollection()
		{
			htable = new Hashtable();
			ntable = new NameTable();
		}
		public XmlSchemaCollection(XmlNameTable nametable)
		{
			htable = new Hashtable();
			ntable = nametable;
		}

		//properties
		public int Count 
		{ 
			get
			{ 
				return this.htable.Count; 
			}
		}
		public XmlNameTable NameTable 
		{ 
			get
			{
				return this.ntable;
			}
		}
		public XmlSchema this[ string ns ] 
		{ 
			get
			{
				return (XmlSchema) this.htable[ns];
			}
		}

		// Events
		public event ValidationEventHandler ValidationEventHandler;

		// Methods
		[MonoTODO]
		public XmlSchema Add(string ns, XmlReader reader)
		{
			return null;
		}
		[MonoTODO]
		public XmlSchema Add(string ns, string uri)
		{
			return null;
		}
		[MonoTODO]
		public XmlSchema Add(XmlSchema schema)
		{
			return null;
		}

		public void Add(XmlSchemaCollection schema)
		{
			XmlSchemaCollectionEnumerator xenum = schema.GetEnumerator();
			while(xenum.MoveNext())
			{
				this.Add(xenum.Current);
			}
		}

		public bool Contains(string ns)
		{
			return this.htable.Contains(ns);
		}
		public bool Contains(XmlSchema schema)
		{
			return this.htable.Contains(schema.TargetNamespace); 
		}
		public void CopyTo(XmlSchema[] array, int index)
		{

		}
		public XmlSchemaCollectionEnumerator GetEnumerator()
		{
			return new XmlSchemaCollectionEnumerator(this.htable);
		}
		
		//assembly Methods
		[MonoTODO]
		void ICollection.CopyTo(Array array, int index)
		{
		}
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.htable.GetEnumerator();
		}
		Object ICollection.SyncRoot
		{
			get { return this; }
		}
	}
}
