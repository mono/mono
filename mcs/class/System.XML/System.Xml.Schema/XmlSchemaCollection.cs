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
		private Hashtable uriTable;
		private XmlNameTable ntable;

		public XmlSchemaCollection()
			: this (new NameTable ())
		{
		}

		public XmlSchemaCollection(XmlNameTable nametable)
		{
			htable = new Hashtable();
			uriTable = new Hashtable ();
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
			if (reader == null)
				throw new ArgumentNullException ("reader");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlSchema Add(string ns, string uri)
		{
			if (uri == null || uri == String.Empty)
				throw new ArgumentNullException ("uri");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlSchema Add(XmlSchema schema)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");

			throw new NotImplementedException ();
		}

		public void Add(XmlSchemaCollection schema)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");

			foreach (XmlSchema s in schema)
				Add (s);
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
