//
// System.Data.Common.DbDataRecord.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002-2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;

namespace System.Data.Common
{
	public abstract class DbDataRecord : IDataRecord, ICustomTypeDescriptor
	{
		protected DbDataRecord ()
		{
		}

		public abstract int FieldCount { get; }
		public abstract object this [string name] { get; }
		public abstract object this [int i] { get; }

		public abstract bool GetBoolean (int i);
		public abstract byte GetByte (int i);
		public abstract long GetBytes (int i, long dataIndex, byte [] buffer, int bufferIndex,int length);
		public abstract char GetChar (int i);
		public abstract long GetChars (int i, long dataIndex, char [] buffer, int bufferIndex, int length);
		public abstract string GetDataTypeName (int i);
		public abstract DateTime GetDateTime (int i);
		public abstract decimal GetDecimal (int i);
		public abstract double GetDouble (int i);
		public abstract Type GetFieldType (int i);
		public abstract float GetFloat (int i);
		public abstract Guid GetGuid (int i);
		public abstract short GetInt16 (int i);
		public abstract int GetInt32 (int i);
		public abstract long GetInt64 (int i);
		public abstract string GetName (int i);
		public abstract int GetOrdinal (string name);
		public abstract string GetString (int i);
		public abstract object GetValue (int i);
		public abstract int GetValues (object [] values);
		public abstract bool IsDBNull (int i);

		public IDataReader GetData (int i)
		{
			return (IDataReader) GetValue (i);
		}
		
		protected virtual DbDataReader GetDbDataReader (int i)
		{
			throw new NotSupportedException ();
		}

		[MonoTODO]
		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			return new AttributeCollection (null);
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetClassName ()
		{
			return string.Empty;
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
			return new EventDescriptorCollection (null);
		}

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute [] attributes)
		{
			return new EventDescriptorCollection (null);
		}

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			DataColumnPropertyDescriptor[] descriptors = 
				new DataColumnPropertyDescriptor [FieldCount];

			DataColumnPropertyDescriptor descriptor;
			for (int col = 0; col < FieldCount; col++) {
				descriptor = new DataColumnPropertyDescriptor(
					GetName (col), col, null);
				descriptor.SetComponentType (typeof (DbDataRecord));
				descriptor.SetPropertyType (GetFieldType (col));
				descriptors [col] = descriptor;
			}

			return new PropertyDescriptorCollection (descriptors);
		}

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute [] attributes)
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
	}

	class DbDataRecordImpl : DbDataRecord
	{
		#region Fields

		readonly SchemaInfo [] schema;
		readonly object [] values;
		readonly int fieldCount;

		#endregion
		
		#region Constructors

		// FIXME: this class should actually be reimplemented to be one
		// of the derived classes of DbDataRecord, which should become
		// almost abstract.
		internal DbDataRecordImpl (SchemaInfo[] schema, object[] values)
		{
			this.schema = schema;
			this.values = values;
			this.fieldCount = values.Length;
		}

		#endregion

		#region Properties

		public override int FieldCount {
			get { return fieldCount; }
		}

		public override object this [string name] {
			get { return this [GetOrdinal (name)]; }
		}

		public override object this [int i] {
			get { return GetValue (i); }
		}

		#endregion

		#region Methods

		public override bool GetBoolean (int i)
		{
			return (bool) GetValue (i);
		}

		public override byte GetByte (int i)
		{
			return (byte) GetValue (i);
		}

		public override long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			if (!(value is byte []))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());

			if ( buffer == null ) {
				// Return length of data
				return ((byte []) value).Length;
			} else {
				// Copy data into buffer
				Array.Copy ((byte []) value, (int) dataIndex, buffer, bufferIndex, length);
				return ((byte []) value).Length - dataIndex;
			}
		}

		public override char GetChar (int i)
		{
			return (char) GetValue (i);
		}

		public override long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			char [] valueBuffer;

			if (value is char[])
				valueBuffer = (char []) value;
			else if (value is string)
				valueBuffer = ((string) value).ToCharArray ();
			else
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());

			if (buffer == null) {
				// Return length of data
				return valueBuffer.Length;
			} else {
				// Copy data into buffer
				Array.Copy (valueBuffer, (int) dataIndex, buffer, bufferIndex, length);
				return valueBuffer.Length - dataIndex;
			}
		}

		public override string GetDataTypeName (int i)
		{
			return schema[i].DataTypeName;
		}

		public override DateTime GetDateTime (int i)
		{
			return (DateTime) GetValue (i);
		}

#if NET_2_0
		[MonoTODO]
		protected override DbDataReader GetDbDataReader (int ordinal)
		{
			throw new NotImplementedException ();
		}
#endif

		public override decimal GetDecimal (int i)
		{
			return (decimal) GetValue (i);
		}

		public override double GetDouble (int i)
		{
			return (double) GetValue (i);
		}

		public override Type GetFieldType (int i)
		{
			return schema[i].FieldType;
		}

		public override float GetFloat (int i)
		{
			return (float) GetValue (i);
		}
		
		public override Guid GetGuid (int i)
		{
			return (Guid) GetValue (i);
		}
		
		public override short GetInt16 (int i)
		{
			return (short) GetValue (i);
		}
	
		public override int GetInt32 (int i)
		{
			return (int) GetValue (i);
		}

		public override long GetInt64 (int i)
		{
			return (long) GetValue (i);
		}

		public override string GetName (int i)
		{
			return schema [i].ColumnName;
		}

		public override int GetOrdinal (string name)
		{
			for (int i = 0; i < FieldCount; i++)
				if (schema [i].ColumnName == name)
					return i;
			return -1;
		}

		public override string GetString (int i)
		{
			return (string) GetValue (i);
		}

		public override object GetValue (int i)
		{
			if (i < 0 || i > fieldCount)
				throw new IndexOutOfRangeException ();
			return values [i];
		}

		public override int GetValues (object[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			
			int count = values.Length > this.values.Length ? this.values.Length : values.Length;
			for(int i = 0; i < count; i++)
				values [i] = this.values [i];
			return count;
		}

		public override bool IsDBNull (int i)
		{
			return GetValue (i) == DBNull.Value;
		}

		#endregion // Methods
	}
}
