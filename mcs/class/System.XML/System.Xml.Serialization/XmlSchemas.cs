// 
// System.Xml.Serialization.XmlSchemas 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Xml.Schema;

namespace System.Xml.Serialization {
	public class XmlSchemas : CollectionBase {

		#region Fields

		Hashtable table;

		#endregion

		#region Constructors

		public XmlSchemas ()
		{
			table = new Hashtable ();
		}

		#endregion // Constructors

		#region Properties

		public XmlSchema this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (XmlSchema) List [index]; 
			}
			set { throw new NotImplementedException (); }
		}

		public XmlSchema this [string ns] {
			get { return (XmlSchema) table[ns]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (XmlSchema schema)
		{
			Insert (Count, schema);
			return (Count - 1);
		}

		public void Add (XmlSchemas schemas) 
		{
			foreach (XmlSchema schema in schemas) 
				Add (schema);
		}

		public bool Contains (XmlSchema schema)
		{
			return List.Contains (schema);
		}

		public void CopyTo (XmlSchema[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (XmlSchema schema)
		{
			return List.IndexOf (schema);
		}

		public void Insert (int index, XmlSchema schema)
		{
			List.Insert (index, schema);
		}

		[MonoTODO]
		public static bool IsDataSet (XmlSchema schema)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnClear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnInsert (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnRemove (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnSet (int index, object oldValue, object newValue)
		{
			throw new NotImplementedException ();
		}
	
		public void Remove (XmlSchema schema)
		{
			List.Remove (schema);
		}

		#endregion // Methods
	}
}
