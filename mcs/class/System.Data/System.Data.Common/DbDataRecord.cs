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

#if NET_2_0
		[MonoTODO]
		public DbDataRecord (object[] values, PropertyDescriptorCollection descriptors, FieldNameLookup fieldNameLookup)
		{
		}

		[MonoTODO]
		public DbDataRecord (SchemaInfo[] schemaInfo, object[] values, PropertyDescriptorCollection descriptors, FieldNameLookup fieldNameLookup)
		{
		}
#endif

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

		public long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			 object value = GetValue (i);
                         if (!(value is byte []))
                                throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
                                                                                                   
                        if ( buffer == null ) {
                                // Return length of data
                                return ((byte []) value).Length;
                        }
                        else {
                                // Copy data into buffer
                                Array.Copy ((byte []) value, (int) dataIndex, buffer, bufferIndex, length);
                                return ((byte []) value).Length - dataIndex;
                        }

		}

		public char GetChar (int i)
		{
			return (char) GetValue (i);
		}

		public long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
	                object value = GetValue (i);
                        char [] valueBuffer;
                                                                                                    
                        if (value is char[])
                                valueBuffer = (char[])value;
                        else if (value is string)
                                valueBuffer = ((string)value).ToCharArray();
                        else
                                throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
                                                                                                                             if ( buffer == null ) {
                                // Return length of data
                                return valueBuffer.Length;
                        }
                        else {
                                // Copy data into buffer
                                Array.Copy (valueBuffer, (int) dataIndex, buffer, bufferIndex, length);
                                return valueBuffer.Length - dataIndex;
                        }

		}

		public IDataReader GetData (int i)
		{
			return (IDataReader) GetValue (i);
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

#if NET_2_0
		[MonoTODO]
		public virtual object GetObjectRef (int i)
		{
			throw new NotImplementedException ();
		}
#endif

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
                       if ((i < 0) || (i > fieldCount))
                                throw new IndexOutOfRangeException();

			object value = values [i];
			if (value == null)
				value = DBNull.Value;
			return value;
		}

		public int GetValues (object[] values)
		{
			if(values == null)
				throw new ArgumentNullException("values");
			
			int count = values.Length > this.values.Length ? this.values.Length : values.Length;
			for(int i = 0; i < count; i++)
				values[i] = this.values[i];

			return count;
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
			return GetValue (i) == DBNull.Value;
		}
#if NET_2_0
		public virtual bool IsSetAsDefault (int i)
		{
			throw new NotImplementedException ();
		}

		public void SetSchemaInfo (SchemaInfo[] schemaInfo)
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion // Methods
	}
}
