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

		Hashtable table = new Hashtable ();

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
			get { return (XmlSchema) table[ns!=null?ns:""]; }
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

		public object Find (XmlQualifiedName name, Type type)
		{
			XmlSchema schema = table [name.Namespace] as XmlSchema;
			if (schema == null)
				return null;

			if (!schema.IsCompiled) {
				try {
					schema.Compile (null);
				} catch {
					throw new InvalidOperationException ("Error compiling XmlSchema " + 
									     name.Namespace);
				}
			}

			XmlSchemaObjectTable tbl = null;

			if (type == typeof (XmlSchemaSimpleType) || type == typeof (XmlSchemaComplexType))
				tbl = schema.SchemaTypes;
			else if (type == typeof (XmlSchemaAttribute))
				tbl = schema.Attributes;
			else if (type == typeof (XmlSchemaAttributeGroup))
				tbl = schema.AttributeGroups;
			else if (type == typeof (XmlSchemaElement))
				tbl = schema.Elements;
			else if (type == typeof (XmlSchemaGroup))
				tbl = schema.Groups;
			else if (type == typeof (XmlSchemaNotation))
				tbl = schema.Notations;

			return (tbl != null) ? tbl [name] : null;
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
			string ns = ((XmlSchema) value).TargetNamespace;
			if (ns == null) ns = "";
			table [ns] = value;
		}

		protected override void OnRemove (int index, object value)
		{
			table.Remove (value);
		}

		protected override void OnSet (int index, object oldValue, object newValue)
		{
			string ns = ((XmlSchema) oldValue).TargetNamespace;
			if (ns == null) ns = "";
			table [ns] = newValue;
		}
	
		public void Remove (XmlSchema schema)
		{
			List.Remove (schema);
		}

		#endregion // Methods
	}
}
