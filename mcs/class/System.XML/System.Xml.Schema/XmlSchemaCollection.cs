//
// System.Xml.Schema.XmlSchemaCollection.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto      ginga@kit.hi-ho.ne.jp
//
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
		internal Guid CompilationId;

		public XmlSchemaCollection()
			: this (new NameTable ())
		{
		}

		public XmlSchemaCollection(XmlNameTable nametable)
		{
			htable = new Hashtable();
			uriTable = new Hashtable ();
			ntable = nametable;
			CompilationId = Guid.NewGuid ();
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
				return (XmlSchema) this.htable[GetSafeNs(ns)];
			}
		}

		// Events
		public event ValidationEventHandler ValidationEventHandler;

		// Methods
		public XmlSchema Add (string ns, XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			XmlSchema schema = XmlSchema.Read (reader, ValidationEventHandler);
			return Add (schema);
		}

		public XmlSchema Add(string ns, string uri)
		{
			return Add (ns, new XmlTextReader (uri));
		}

		public XmlSchema Add(XmlSchema schema)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");

			if (!schema.IsCompiled)
				schema.Compile (null, this);
			/*
			// This is requried to complete maybe missing sub components.
			foreach (XmlSchema existing in htable.Values)
				if (existing.CompilationId != this.CompilationId)
					existing.Compile (null, this);
			*/

			htable [GetSafeNs(schema.TargetNamespace)] = schema;
			return schema;
		}

		public void Add(XmlSchemaCollection schema)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");

			foreach (XmlSchema s in schema)
				Add (s);
		}

		string GetSafeNs (string ns)
		{
			return ns == null ? "" : ns;
		}

		public bool Contains(string ns)
		{
			return this.htable.Contains(GetSafeNs(ns));
		}
		public bool Contains(XmlSchema schema)
		{
			return this.htable.Contains(GetSafeNs(schema.TargetNamespace)); 
		}
		public void CopyTo(XmlSchema[] array, int index)
		{
			((ICollection) this).CopyTo (array, index);
		}

		public XmlSchemaCollectionEnumerator GetEnumerator()
		{
			return new XmlSchemaCollectionEnumerator(this);
		}
		
		// interface Methods
		void ICollection.CopyTo(Array array, int index)
		{
			htable.Values.CopyTo (array, index);
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

		// Internal Methods
		internal XmlSchemaAttribute FindAttribute (XmlQualifiedName qname)
		{
			XmlSchemaAttribute found = null;
			XmlSchema target = this [qname.Namespace];
			if (target != null)
				found = target.Attributes [qname] as XmlSchemaAttribute;
			if (found != null)
				return found;
			foreach (XmlSchema schema in htable.Values) {
				found = schema.Attributes [qname] as XmlSchemaAttribute;
				if (found != null)
					return found;
			}
			return null;
		}

		internal XmlSchemaElement FindElement (XmlQualifiedName qname)
		{
			XmlSchemaElement found = null;
			XmlSchema target = this [qname.Namespace];
			if (target != null)
				found = target.Elements [qname] as XmlSchemaElement;
			if (found != null)
				return found;
			foreach (XmlSchema schema in htable.Values) {
				found = schema.Elements [qname] as XmlSchemaElement;
				if (found != null)
					return found;
			}
			return null;
		}

		internal object FindSchemaType (XmlQualifiedName qname)
		{
			if (qname == XmlSchemaComplexType.AnyTypeName)
				return XmlSchemaComplexType.AnyType;
			else if (qname.Namespace == XmlSchema.Namespace)
				return XmlSchemaDatatype.FromName (qname);

			XmlSchemaType found = null;
			XmlSchema target = this [qname.Namespace];
			if (target != null)
				found = target.SchemaTypes [qname] as XmlSchemaType;
			if (found != null)
				return found;
			foreach (XmlSchema schema in htable.Values) {
				found = schema.SchemaTypes [qname] as XmlSchemaType;
				if (found != null)
					return found;
			}
			return null;
		}
	}
}
