//
// System.Xml.Query.XmlViewSchemaDictionary
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data.Mapping;
using System.Data.SqlXml;

namespace System.Xml.Query {
        public class XmlViewSchemaDictionary : ICollection, IDictionary, IEnumerable
        {
		#region Constructors

		[MonoTODO]
		public XmlViewSchemaDictionary ()
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public int Count {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ICollection.IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		object ICollection.SyncRoot {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IDictionary.IsFixedSize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IDictionary.IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		ICollection IDictionary.Keys {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		ICollection IDictionary.Values {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		object IDictionary.this [object key] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlViewSchema this [string name] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Add (string name, XmlViewSchema mapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (string name, MappingSchema mapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (string name, string mappingUrl)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (string name, XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (XmlViewSchemaDictionary externalCollection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDictionaryEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDictionary.Add (object key, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IDictionary.Contains (object key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDictionary.Remove (object key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (string name)
		{
			throw new NotImplementedException ();
		}


		#endregion // Methods
        }
}

#endif // NET_1_2
