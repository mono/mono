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
			return GetValue (i).GetType ();
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetClassName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetComponentName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			throw new NotImplementedException ();
		}	

		[MonoTODO]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}	

		[MonoTODO]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			throw new NotImplementedException ();
		}	

		[MonoTODO]
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}	

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException ();
		}	

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute[] attributes)
		{
			throw new NotImplementedException ();
		}	

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			throw new NotImplementedException ();
		}	

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute[] attributes)
		{
			throw new NotImplementedException ();
		}	

		[MonoTODO]
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}	

		public bool IsDBNull (int i)
		{
			return GetValue (i) == null;
		}

		#endregion // Methods
	}
}
