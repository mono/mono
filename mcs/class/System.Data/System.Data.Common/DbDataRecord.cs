//
// System.Data.Common.DbDataRecord.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
	public class DbDataRecord : IDataRecord, ICustomTypeDescriptor
	{
		#region Fields

		SchemaInfo[] schema;
		object[] values;
		int fieldCount;
		FieldNameLookup lookup;

		#endregion
		
		#region Constructors

		internal DbDataRecord (SchemaInfo[] schema, object[] values, FieldNameLookup lookup)
		{
			this.schema = schema;
			this.lookup = lookup;
			this.values = values;
			this.fieldCount = values.Length;
		}

		#endregion

		#region Properties

		public int FieldCount {
			get { return fieldCount; }
		}

		public object this [string name] {
			get { return this [GetOrdinal (name)]; }
		}

		[System.Runtime.CompilerServices.IndexerName("Item")]
		public object this [int index] {
			get { return GetValue (index); }
		}	

		#endregion

		#region Methods

		public bool GetBoolean (int i)
		{
			return (bool) GetValue (i);
		}

		public byte GetByte (int i)
		{
			return (byte) GetValue (i);
		}

		[MonoTODO]	
		public long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		public char GetChar (int i)
		{
			return (char) GetValue (i);
		}

		[MonoTODO]
		public long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		public string GetDataTypeName (int i)
		{
			return schema[i].DataTypeName;
		}

		public DateTime GetDateTime (int i)
		{
			return (DateTime) GetValue (i); 
		}

		public decimal GetDecimal (int i)
		{
			return (decimal) GetValue (i);
		}

		public double GetDouble (int i)
		{
			return (double) GetValue (i);
		}

		public Type GetFieldType (int i)
		{
			return schema[i].FieldType;
		}

		public float GetFloat (int i)
		{
			return (float) GetValue (i);
		}
		
		public Guid GetGuid (int i)
		{
			return (Guid) GetValue (i);
		}
		
		public short GetInt16 (int i)
		{
			return (short) GetValue (i); 
		}
	
		public int GetInt32 (int i)
		{
			return (int) GetValue (i); 
		}

		public long GetInt64 (int i)
		{
			return (long) GetValue (i); 
		}

		public string GetName (int i)
		{
			return (string) lookup [i];
		}

		public int GetOrdinal (string name)
		{
			return lookup.IndexOf (name);
		}

		public string GetString (int i)
		{
			return (string) GetValue (i);
		}

		public object GetValue (int i)
		{
			return values [i];
		}

		[MonoTODO]
		public int GetValues (object[] values)
		{
			object[] newArray = new object[this.values.Length];
			values.CopyTo (newArray, 0);
			return values.Length;
		}

		[MonoTODO]
		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			return new AttributeCollection(null);
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetClassName ()
		{
			return "";
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetComponentName ()
		{
			return null;
		}

		[MonoTODO]
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			return null;
		}	

		[MonoTODO]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			return null;
		}	

		[MonoTODO]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			return null;
		}	

		[MonoTODO]
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			return null;
		}	

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			return new EventDescriptorCollection(null);
		}	

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute[] attributes)
		{
			return new EventDescriptorCollection(null);
		}	

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			DataColumnPropertyDescriptor[] descriptors = 
				new DataColumnPropertyDescriptor[FieldCount];

			DataColumnPropertyDescriptor descriptor;
			DataColumn dataColumn;
			for(int col = 0; col < FieldCount; col++) {
				descriptor = new DataColumnPropertyDescriptor(
					GetName(col), col, null);
				descriptor.SetComponentType(typeof(DbDataRecord));
				descriptor.SetPropertyType(GetFieldType(col));
				
				descriptors[col] = descriptor;
			}

			return new PropertyDescriptorCollection (descriptors);
		}	

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute[] attributes)
		{
			PropertyDescriptorCollection descriptors;
			descriptors = ((ICustomTypeDescriptor) this).GetProperties ();
			// TODO: filter out descriptors which do not contain
			//       any of those attributes
			//       except, those descriptors 
			//       that contain DefaultMemeberAttribute
			return descriptors;
		}	

		[MonoTODO]
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			return this;
		}	

		public bool IsDBNull (int i)
		{
			return GetValue (i) == null;
		}

		#endregion // Methods
	}
}
