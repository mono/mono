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

		static Hashtable table = new Hashtable ();

		#endregion

		#region Constructors

		public XmlSchemas ()
		{
		}

		#endregion // Constructors

		#region Properties

		public XmlSchema this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (XmlSchema) List [index]; 
			}
			set { List [index] = value; }
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

		protected override void OnClear ()
		{
			table.Clear ();
		}

		protected override void OnInsert (int index, object value)
		{	
			table [((XmlSchema) value).TargetNamespace] = value;
		}

		protected override void OnRemove (int index, object value)
		{
			table.Remove (value);
		}

		protected override void OnSet (int index, object oldValue, object newValue)
		{
			table [((XmlSchema) oldValue).TargetNamespace] = newValue;
		}
	
		public void Remove (XmlSchema schema)
		{
			List.Remove (schema);
		}

		#endregion // Methods
	}
}
