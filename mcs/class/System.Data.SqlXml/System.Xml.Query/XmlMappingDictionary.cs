//
// System.Xml.Query.XmlMappingDictionary
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

#if NET_2_0

namespace System.Xml.Query 
{
	public sealed class XmlMappingDictionary
		: IDictionary, ICollection, IEnumerable, IXmlCompilerInclude
	{
		private Hashtable mappings;

		public XmlMappingDictionary ()
		{
			mappings = new Hashtable ();
		}

		public event QueryEventHandler OnProcessingEvent;

		[MonoTODO ("Check name conflicts")]
		public XmlMapping Add (string name, string mappingUrl)
		{
			using (XmlReader xr = new XmlTextReader (mappingUrl)) {
				return Add (name, xr);
			}
		}

		[MonoTODO ("Check name conflicts")]
		public XmlMapping Add (string name, XmlReader reader)
		{
			XmlMapping map = mapppings [name] as XmlMapping;
			if (map != null) {
				map = new XmlMapping (reader);
				mappings [name] = map;
			}
			return map;
		}

		[MonoTODO ("Check name conflicts")]
		public void Add (string name, XmlMapping mapping)
		{
			mappings.Add (name, mapping);
		}

		// Why virtual method inside sealed class :-?
		public virtual void Clear ()
		{
			mappings.Clear ();
		}

		public bool Contains (string name)
		{
			return mappings.Contains (name);
		}

		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			return mappings.GetEnumerator ();
		}

		public void Remove (string name)
		{
			mappings.Remove (name);
		}

		public virtual int Count {
			get { return mappings.Count; }
		}

		public XmlMapping this [string name] {
			get { return mappings [name] as XmlMapping; }
			set { mappings [name] = value; }
		}

		void ICollections.CopyTo (Array array, int index)
		{
			mappings.CopyTo (array, index);
		}

		[MonoTODO]
		bool ICollection.IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		object ICollection.SyncRoot {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO ("What if invalid type value?")]
		void IDictionary.Add (object key, object value)
		{
			if (value is string)
				Add ((string) key, (string) value);
			else if (value is XmlReader)
				Add ((string) key, (XmlReader) value);
			else
				Add ((string) key, (XmlMapping) value);
		}

		bool IDictionary.Contains (object key)
		{
			return Containts ((string) key);
		}

		bool IDictionary IsFixedSize {
			get { return false; }
		}

		bool IDictionary.IsReadOnly {
			get { return false; }
		}

		object IDictionary.this [object key] {
			get { return this [(string) key]; }
			set { this [(string) key] = (XmlMapping) value; }
		}

		ICollection IDictionary.Keys {
			get { return mappings.Keys; }
		}

		ICollection IDictionary.Values {
			get { return mappings.Values; }
		}

		void IDictionary.Remove (object key)
		{
			mappings.Remove ((string) key);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		[MonoTODO]
		XmlExpression IXmlCompilerInclude.ResolveContextDocument ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		XmlExpression IXmlCompilerInclude.ResolveContextFunction (
			XmlQualifiedName funcName, object [] funcParams)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		XmlExpression IXmlCompilerInclude.ResolveContextDocument (
			XmlQualifiedName varName)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif // NET_2_0
