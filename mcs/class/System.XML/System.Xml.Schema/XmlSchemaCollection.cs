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
		private XmlSchemaSet schemaSet;

		public XmlSchemaCollection ()
			: this (new NameTable ())
		{
		}

		public XmlSchemaCollection (XmlNameTable nameTable)
			: this (new XmlSchemaSet (nameTable))
		{
			this.schemaSet.SchemaCollection = this;
		}

		internal XmlSchemaCollection (XmlSchemaSet schemaSet)
		{
			this.schemaSet = schemaSet;
		}

		//properties
		internal XmlSchemaSet SchemaSet {
			get { return schemaSet; }
		}

		public int Count {
			get { return schemaSet.Count; }
		}

		public XmlNameTable NameTable { 
			get { return schemaSet.NameTable; }
		}

		public XmlSchema this [ string ns ] { 
			get { return schemaSet.Get (ns); }
		}

		// Events
		public event ValidationEventHandler ValidationEventHandler;

		// Methods
		public XmlSchema Add (string ns, XmlReader reader)
		{
			return Add (ns, reader, new XmlUrlResolver ());
		}

#if NET_1_0
		internal XmlSchema Add (string ns, XmlReader reader, XmlResolver resolver)
#else
		public XmlSchema Add (string ns, XmlReader reader, XmlResolver resolver)
#endif
		{
			XmlSchema schema = XmlSchema.Read (reader, ValidationEventHandler);
			schema.Compile (ValidationEventHandler, this, resolver);
			return schemaSet.Add (schema);
		}

		public XmlSchema Add (string ns, string uri)
		{
			return schemaSet.Add (ns, uri);
		}

		public XmlSchema Add (XmlSchema schema)
		{
			return Add (schema, new XmlUrlResolver ());
		}

		public XmlSchema Add (XmlSchema schema, XmlResolver resolver)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");

			// XmlSchemaCollection.Add() compiles, while XmlSchemaSet.Add() does not
			if (!schema.IsCompiled)
				schema.Compile (ValidationEventHandler, this, resolver);

			string ns = GetSafeNs (schema.TargetNamespace);
			if (schemaSet.Contains (ns))
				schemaSet.Remove (schemaSet.Get (ns));
			return schemaSet.Add (schema);
		}

		private string GetSafeNs (string ns)
		{
			return ns != null ? ns : String.Empty;
		}

		public void Add (XmlSchemaCollection schema)
		{
			if (schema == null)
				throw new ArgumentNullException ("schema");

			foreach (XmlSchema s in schema) {
				string ns = GetSafeNs (s.TargetNamespace);
				if (schemaSet.Contains (ns))
					schemaSet.Remove (schemaSet.Get (ns));
				schemaSet.Add (s);
			}
		}

		public bool Contains (string ns)
		{
			return schemaSet.Contains (ns);
		}

		public bool Contains (XmlSchema schema)
		{
			return schemaSet.Contains (schema);
		}

		public void CopyTo (XmlSchema[] array, int index)
		{
			schemaSet.CopyTo (array, index);
		}

		public XmlSchemaCollectionEnumerator GetEnumerator ()
		{
			return new XmlSchemaCollectionEnumerator (this);
		}
		
		// interface Methods
		void ICollection.CopyTo (Array array, int index)
		{
			schemaSet.CopyTo (array, index);
		}

		[MonoTODO]
		bool ICollection.IsSynchronized
		{
			get { throw new NotImplementedException (); }
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return schemaSet.GetEnumerator ();
		}

		[MonoTODO]
		Object ICollection.SyncRoot
		{
			get { throw new NotImplementedException (); }
		}

		// Internal Methods
		internal XmlSchemaAttribute FindAttribute (XmlQualifiedName qname)
		{
			return (XmlSchemaAttribute) schemaSet.GlobalAttributes [qname];
		}

		internal XmlSchemaElement FindElement (XmlQualifiedName qname)
		{
			return (XmlSchemaElement) schemaSet.GlobalElements [qname];
		}

		internal object FindSchemaType (XmlQualifiedName qname)
		{
			return schemaSet.GlobalTypes [qname];
		}

		internal void OnValidationError (object o, ValidationEventArgs e)
		{
			if (ValidationEventHandler != null)
				ValidationEventHandler (o, e);
			else if (e.Severity == XmlSeverityType.Error)
				throw e.Exception;
		}

	}
}
