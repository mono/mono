
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
using System;
using System.Collections;

namespace System.Data.Common
{
	internal abstract class AbstractDataContainer
	{
		#region Fields

		BitArray _nullValues;
		System.Type _type;
		DataColumn _column;

		#endregion //Fields

		#region Properties

		internal abstract object this[int index] {
			get;
			set;
		}

		internal virtual int Capacity {
			get { 
				return (_nullValues != null) ? _nullValues.Count : 0; 
			}
			set { 
				if (_nullValues == null) {
					_nullValues = new BitArray(value);
				}
				else {
					_nullValues.Length = value;
				}
			}
		}

		internal Type Type {
			get {
				return _type;
			}
		}

		protected DataColumn Column {
			get {
				return _column;
			}
		}

		#endregion //Properties

		#region Methods

		internal static AbstractDataContainer CreateInstance(Type type, DataColumn column)
		{
			AbstractDataContainer container;
			switch (Type.GetTypeCode(type)) {
				case TypeCode.Int16 :
					container = new Int16DataContainer();
					break;
				case TypeCode.Int32 : 
					container = new Int32DataContainer();
					break;
				case TypeCode.Int64 :
					container = new Int64DataContainer();
					break;
				case TypeCode.String :
					container = new StringDataContainer();
					break;
				case TypeCode.Boolean:
					container = new BitDataContainer();
					break;
				case TypeCode.Byte :
					container = new ByteDataContainer();
					break;
				//case TypeCode.Char :
				case TypeCode.DateTime :
					container = new DateTimeDataContainer();
					break;
				//case TypeCode.Decimal :
				case TypeCode.Double :
					container = new DoubleDataContainer();
					break;
				//case TypeCode.SByte :
				case TypeCode.Single :
					container = new SingleDataContainer();
					break;
				//case TypeCode.UInt16 :
				//case TypeCode.UInt32 :
				//case TypeCode.UInt64 :
				default :
					container = new ObjectDataContainer();
					break;
			}
			container._type = type;
			container._column = column;
			return container;
		}

		internal bool IsNull(int index)
		{
			return (_nullValues != null) ? _nullValues[index] : true;
		}

		internal void SetNullBit(int index,bool isNull)
		{
			_nullValues[index] = isNull;
		}

		protected void SetNull(int index,bool isNull,bool isDbNull)
		{
			SetNullBit(index,isDbNull);
			// this method must be called after setting the value into value array
			// otherwise the dafault value will be overriden
			if ( isNull ) {
				// set the value to default
				CopyValue(Column.Table.DefaultValuesRowIndex,index);
			}
		}

		internal void FillValues(int fromIndex)
		{
			for(int i=0; i < Capacity; i++) {
				CopyValue(fromIndex,i);
				_nullValues[i] = _nullValues[fromIndex];
			}
		}

		internal virtual void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
		{
			_nullValues[toIndex] = fromContainer._nullValues[fromIndex];
		}

		internal virtual void CopyValue(int fromIndex, int toIndex)
		{
			_nullValues[toIndex] = _nullValues[fromIndex];
		}

		internal virtual void SetItemFromDataRecord(int index, IDataRecord record, int field)
		{
			bool isDbNull = record.IsDBNull(field);
			SetNull(index,false,isDbNull);
		}

		protected  bool CheckAndSetNull( int index, IDataRecord record, int field)
		{
			 bool isDbNull = record.IsDBNull(field);
	    	         SetNull(index,false,isDbNull);
			 return isDbNull;		
		}

		protected int CompareNulls(int index1, int index2)
		{
			bool null1 = IsNull(index1);
			bool null2 = IsNull(index2);

			if ( null1 ^ null2 ) {
				return null1 ? -1 : 1;
			}
			else {
				return 0;
			}
		}

		internal abstract int CompareValues(int index1, int index2);

		internal abstract long GetInt64(int index);

		#endregion //Methods

		sealed class Int16DataContainer : AbstractDataContainer
		{
			#region Fields
		
			short[] _values;

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					if (IsNull(index)) {
						return DBNull.Value;
					}
					else {
						return _values[index];
					}
				}
				set {
					bool isDbNull = (value ==  DBNull.Value);
					if (value == null || isDbNull) {
						SetValue(index,0);
					}
					else if( value is short ) {
						SetValue(index,(short)value);
					}
					else {
						SetValue(index,Convert.ToInt16(value));
					}
					SetNull(index,value == null,isDbNull);
				}
			}

			internal override int Capacity {
				set {
					base.Capacity = value;
					if (_values == null) {
						_values = new short[value];
					}
					else {
						short[] tmp = new short[value];
						Array.Copy(_values,0,tmp,0,_values.Length);
						_values = tmp;
					}
				}
			}

			#endregion //Properties

			#region Methods
			
			private void SetValue(int index, short value)
			{
				_values[index] = value;
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				if (!CheckAndSetNull ( index, record,field))
					SetValue(index,record.GetInt16(field));
			}

			internal override void CopyValue(int fromIndex, int toIndex)
			{
				base.CopyValue(fromIndex, toIndex);
				_values[toIndex] = _values[fromIndex];
			}

			internal override void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
			{
				base.CopyValue(fromContainer, fromIndex, toIndex);
				_values[toIndex] = ((Int16DataContainer)fromContainer)._values[fromIndex];
			}

			internal override int CompareValues(int index1, int index2)
			{
				short s1 = _values[index1];
				short s2 = _values[index2];

				if ( s1 == 0 && s2 == 0 ) {
					int cn = CompareNulls(index1, index2);
					return cn;
				}

				bool b1 = IsNull(index1);
				bool b2 = IsNull(index2);
				
				if ( s1 == 0 && b1 ) {
					return -1;
				}

				if ( s2 == 0 && b2 ) {
					return 1;
				}

				if ( s1 <= s2 ) {
					return ( s1 != s2 ) ? -1 : 0;
				}
				return 1;
			}

			internal override long GetInt64(int index)
			{
				return (long) _values[index];
			}

			#endregion //Methods
		}

		sealed class Int32DataContainer : AbstractDataContainer
		{
			#region Fields
		
			int[] _values;

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					if (IsNull(index)) {
						return DBNull.Value;
					}
					else {
						return _values[index];
					}
				}
				set {
					bool isDbNull = (value ==  DBNull.Value);
					if (value == null || isDbNull) {
						SetValue(index,0);
					}
					else if( value is int ) {
						SetValue(index,(int)value);
					}
					else {
						SetValue(index,Convert.ToInt32(value));
					}
					SetNull(index,value == null,isDbNull);
				}
			}

			internal override int Capacity {
				set {
					base.Capacity = value;
					if (_values == null) {
						_values = new int[value];
					}
					else {
						int[] tmp = new int[value];
						Array.Copy(_values,0,tmp,0,_values.Length);
						_values = tmp;
					}
				}
			}

			#endregion //Properties

			#region Methods
			
			private void SetValue(int index, int value)
			{
				_values[index] = value;
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				if (!CheckAndSetNull ( index, record,field))
                                        SetValue(index,record.GetInt32(field));
			}

			internal override void CopyValue(int fromIndex, int toIndex)
			{
				base.CopyValue(fromIndex, toIndex);
				_values[toIndex] = _values[fromIndex];
			}

			internal override void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
			{
				base.CopyValue(fromContainer, fromIndex, toIndex);
				_values[toIndex] = ((Int32DataContainer)fromContainer)._values[fromIndex];
			}

			internal override int CompareValues(int index1, int index2)
			{
				int i1 = _values[index1];
				int i2 = _values[index2];

				if ( i1 == 0 && i2 == 0 ) {
					int cn = CompareNulls(index1, index2);
					return cn;
				}

				bool b1 = IsNull(index1);
				bool b2 = IsNull(index2);
				
				if ( i1 == 0 && b1 ) {
					return -1;
				}

				if ( i2 == 0 && b2 ) {
					return 1;
				}

				if ( i1 <= i2 ) {
					return ( i1 != i2 ) ? -1 : 0;
				}
				return 1;
			}

			internal override long GetInt64(int index)
			{
				return (long) _values[index];
			}

			#endregion //Methods
		}

		sealed class Int64DataContainer : AbstractDataContainer
		{
			#region Fields
		
			long[] _values;

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					if (IsNull(index)) {
						return DBNull.Value;
					}
					else {
						return _values[index];
					}
				}
				set {
					bool isDbNull = (value ==  DBNull.Value);
					if (value == null || isDbNull) {
						SetValue(index,0);
					}
					else if( value is long ) {
						SetValue(index,(long)value);
					}
					else {
						SetValue(index,Convert.ToInt64(value));
					}
					SetNull(index,value == null,isDbNull);
				}
			}

			internal override int Capacity {
				set {
					base.Capacity = value;
					if (_values == null) {
						_values = new long[value];
					}
					else {
						long[] tmp = new long[value];
						Array.Copy(_values,0,tmp,0,_values.Length);
						_values = tmp;
					}
				}
			}

			#endregion //Properties

			#region Methods
			
			private void SetValue(int index, long value)
			{
				_values[index] = value;
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				if (!CheckAndSetNull ( index, record,field))
                                        SetValue(index,record.GetInt64(field));
			}

			internal override void CopyValue(int fromIndex, int toIndex)
			{
				base.CopyValue(fromIndex, toIndex);
				_values[toIndex] = _values[fromIndex];
			}

			internal override void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
			{
				base.CopyValue(fromContainer, fromIndex, toIndex);
				_values[toIndex] = ((Int64DataContainer)fromContainer)._values[fromIndex];
			}

			internal override int CompareValues(int index1, int index2)
			{
				long l1 = _values[index1];
				long l2 = _values[index2];

				if ( l1 == 0 || l2 == 0 ) {
					int cn = CompareNulls(index1, index2);
					if (cn != 0) {
						return cn;
					}
				}

				if ( l1 <= l2 ) {
					return ( l1 != l2 ) ? -1 : 0;
				}
				return 1;
			}

			internal override long GetInt64(int index)
			{
				return _values[index];
			}

			#endregion //Methods
		}

		sealed class SingleDataContainer : AbstractDataContainer
		{
			#region Fields
		
			float[] _values;

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					if (IsNull(index)) {
						return DBNull.Value;
					}
					else {
						return _values[index];
					}
				}
				set {
					bool isDbNull = (value ==  DBNull.Value);
					if (value == null || isDbNull) {
						SetValue(index,0);
					}
					else if( value is float ) {
						SetValue(index,(float)value);
					}
					else {
						SetValue(index,Convert.ToSingle(value));
					}
					SetNull(index,value == null,isDbNull);
				}
			}

			internal override int Capacity {
				set {
					base.Capacity = value;
					if (_values == null) {
						_values = new float[value];
					}
					else {
						float[] tmp = new float[value];
						Array.Copy(_values,0,tmp,0,_values.Length);
						_values = tmp;
					}
				}
			}

			#endregion //Properties

			#region Methods
			
			private void SetValue(int index, float value)
			{
				_values[index] = value;
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				if (!CheckAndSetNull ( index, record,field))
                                        SetValue(index,record.GetFloat(field));
			}

			internal override void CopyValue(int fromIndex, int toIndex)
			{
				base.CopyValue(fromIndex, toIndex);
				_values[toIndex] = _values[fromIndex];
			}

			internal override void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
			{
				base.CopyValue(fromContainer, fromIndex, toIndex);
				_values[toIndex] = ((SingleDataContainer)fromContainer)._values[fromIndex];
			}

			internal override int CompareValues(int index1, int index2)
			{
				float f1 = _values[index1];
				float f2 = _values[index2];

				if ( f1 == 0 || f2 == 0 ) {
					int cn = CompareNulls(index1, index2);
					if (cn != 0) {
						return cn;
					}
				}

				if ( f1 <= f2 ) {
					return ( f1 != f2 ) ? -1 : 0;
				}
				return 1;
			}

			internal override long GetInt64(int index)
			{
				return Convert.ToInt64(_values[index]);
			}

			#endregion //Methods
		}

		sealed class DoubleDataContainer : AbstractDataContainer
		{
			#region Fields
		
			double[] _values;

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					if (IsNull(index)) {
						return DBNull.Value;
					}
					else {
						return _values[index];
					}
				}
				set {
					bool isDbNull = (value ==  DBNull.Value);
					if (value == null || isDbNull) {
						SetValue(index,0);
					}
					else if( value is double ) {
						SetValue(index,(double)value);
					}
					else {
						SetValue(index,Convert.ToDouble(value));
					}
					SetNull(index,value == null,isDbNull);
				}
			}

			internal override int Capacity {
				set {
					base.Capacity = value;
					if (_values == null) {
						_values = new double[value];
					}
					else {
						double[] tmp = new double[value];
						Array.Copy(_values,0,tmp,0,_values.Length);
						_values = tmp;
					}
				}
			}

			#endregion //Properties

			#region Methods
			
			private void SetValue(int index, double value)
			{
				_values[index] = value;
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				if (!CheckAndSetNull ( index, record,field))
                                        SetValue(index,record.GetDouble(field));
			}

			internal override void CopyValue(int fromIndex, int toIndex)
			{
				base.CopyValue(fromIndex, toIndex);
				_values[toIndex] = _values[fromIndex];
			}

			internal override void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
			{
				base.CopyValue(fromContainer, fromIndex, toIndex);
				_values[toIndex] = ((DoubleDataContainer)fromContainer)._values[fromIndex];
			}

			internal override int CompareValues(int index1, int index2)
			{
				double d1 = _values[index1];
				double d2 = _values[index2];

				if ( d1 == 0 || d2 == 0 ) {
					int cn = CompareNulls(index1, index2);
					if (cn != 0) {
						return cn;
					}
				}

				if ( d1 <= d2 ) {
					return ( d1 != d2 ) ? -1 : 0;
				}
				return 1;
			}

			internal override long GetInt64(int index)
			{
				return Convert.ToInt64(_values[index]);
			}

			#endregion //Methods
		}

		sealed class ByteDataContainer : AbstractDataContainer
		{
			#region Fields
		
			byte[] _values;

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					if (IsNull(index)) {
						return DBNull.Value;
					}
					else {
						return _values[index];
					}
				}
				set {
					bool isDbNull = (value ==  DBNull.Value);
					if (value == null || isDbNull) {
						SetValue(index,0);
					}
					else if( value is byte ) {
						SetValue(index,(byte)value);
					}
					else {
						SetValue(index,Convert.ToByte(value));
					}
					SetNull(index,value == null,isDbNull);
				}
			}

			internal override int Capacity {
				set {
					base.Capacity = value;
					if (_values == null) {
						_values = new byte[value];
					}
					else {
						byte[] tmp = new byte[value];
						Array.Copy(_values,0,tmp,0,_values.Length);
						_values = tmp;
					}
				}
			}

			#endregion //Properties

			#region Methods
			
			private void SetValue(int index, byte value)
			{
				_values[index] = value;
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				if (!CheckAndSetNull ( index, record,field))
                                        SetValue(index,record.GetByte(field));
			}

			internal override void CopyValue(int fromIndex, int toIndex)
			{
				base.CopyValue(fromIndex, toIndex);
				_values[toIndex] = _values[fromIndex];
			}

			internal override void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
			{
				base.CopyValue(fromContainer, fromIndex, toIndex);
				_values[toIndex] = ((ByteDataContainer)fromContainer)._values[fromIndex];
			}

			internal override int CompareValues(int index1, int index2)
			{
				byte b1 = _values[index1];
				byte b2 = _values[index2];

				if ( b1 == 0 || b2 == 0 ) {
					int cn = CompareNulls(index1, index2);
					if (cn != 0) {
						return cn;
					}
				}

				if ( b1 <= b2 ) {
					return ( b1 != b2 ) ? -1 : 0;
				}
				return 1;
			}

			internal override long GetInt64(int index)
			{
				return (long) _values[index];
			}

			#endregion //Methods
		}

		sealed class BitDataContainer : AbstractDataContainer
		{
			#region Fields
		
			bool[] _values;

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					bool isNull = IsNull(index);
					if (isNull) {
						return DBNull.Value;
					}
					else {
						return _values[index];
					}
				}
				set {
					bool isDbNull = (value ==  DBNull.Value);
					if (value == null || isDbNull) {
						SetValue(index,false);
					}
					else if( value is bool ) {
						SetValue(index,(bool)value);
					}
					else {
						SetValue(index,Convert.ToBoolean(value));
					}
					SetNull(index,value == null,isDbNull);
				}
			}

			internal override int Capacity {
				set {
					base.Capacity = value;
					if (_values == null) {
						_values = new bool[value];
					}
					else {
						bool[] tmp = new bool[value];
						Array.Copy(_values,0,tmp,0,_values.Length);
						_values = tmp;
					}
				}
			}

			#endregion //Properties

			#region Methods
			
			private void SetValue(int index, bool value)
			{
				_values[index] = value;
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				if (!CheckAndSetNull ( index, record,field))
                                        SetValue(index,record.GetBoolean(field));
			}

			internal override void CopyValue(int fromIndex, int toIndex)
			{
				base.CopyValue(fromIndex, toIndex);
				_values[toIndex] = _values[fromIndex];
			}

			internal override void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
			{
				base.CopyValue(fromContainer, fromIndex, toIndex);
				_values[toIndex] = ((BitDataContainer)fromContainer)._values[fromIndex];
			}

			internal override int CompareValues(int index1, int index2)
			{
				bool b1 = _values[index1];
				bool b2 = _values[index2];

				if ( b1 ^ b2 ) {
					return b1 ? 1 : -1;
				}
				
				if ( b1 ) {
					return 0;
				}

				return CompareNulls(index1, index2);	
			}

			internal override long GetInt64(int index)
			{
				return Convert.ToInt64(_values[index]);
			}

			#endregion //Methods
		}

		class ObjectDataContainer : AbstractDataContainer
		{
			#region Fields
		
			object[] _values;

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					return _values[index];
				}
				set {
					SetValue(index,value);
					SetNull(index,value == null,value == DBNull.Value);
				}
			}

			internal override int Capacity {
				set {
					base.Capacity = value;
					if (_values == null) {
						_values = new object[value];
					}
					else {
						object[] tmp = new object[value];
						Array.Copy(_values,0,tmp,0,_values.Length);
						_values = tmp;
					}
				}
			}

			#endregion //Properties

			#region Methods
			
			protected virtual void SetValue(int index, object value)
			{
				if(value == null) {
					value = Column.DefaultValue;
				}
				_values[index] = value;
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller metho
				SetValue(index,record.GetValue(field));
				base.SetItemFromDataRecord(index,record,field);
			}

			internal override void CopyValue(int fromIndex, int toIndex)
			{
				base.CopyValue(fromIndex, toIndex);
				_values[toIndex] = _values[fromIndex];
			}

			internal override void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
			{
				base.CopyValue(fromContainer, fromIndex, toIndex);
				_values[toIndex] = ((ObjectDataContainer)fromContainer)._values[fromIndex];
			}

			internal override int CompareValues(int index1, int index2)
			{
				object obj1 = _values[index1];
				object obj2 = _values[index2];
				if(obj1 == obj2) {
					return 0;
				}
				else if (obj1 is IComparable) {
					try {
						return ((IComparable)obj1).CompareTo(obj2);
					}
					catch {
						//just suppress
					}

					if (obj2 is IComparable) {
						obj2 = Convert.ChangeType(obj2, Type.GetTypeCode(obj1.GetType()));
						return ((IComparable)obj1).CompareTo(obj2);
					}
				}

				return String.Compare(obj1.ToString(), obj2.ToString());
			}

			internal override long GetInt64(int index)
			{
				return Convert.ToInt64(_values[index]);
			}

			#endregion //Methods
	 
		}

		sealed class StringDataContainer : ObjectDataContainer
		{
			#region Methods

			private void SetValue(int index, string value)
			{
				if (value != null && Column.MaxLength >= 0 && Column.MaxLength < value.Length ) {
					throw new ArgumentException("Cannot set column '" + Column.ColumnName + "' to '" + value + "'. The value violates the MaxLength limit of this column.");
				}
				base.SetValue(index,value);
			}
			
			protected override void SetValue(int index, object value)
			{
				if ( value != null && value != DBNull.Value ) {
					if ( value is string ) {
						SetValue(index, (string) value);
					}
					else {
						SetValue(index, Convert.ToString(value));
					}
					return;
				}

				base.SetValue(index, value);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				if (!CheckAndSetNull ( index, record,field))
                                        SetValue(index,record.GetString(field));
			}

			internal override int CompareValues(int index1, int index2)
			{
				bool isNull1 = IsNull(index1);
				bool isNull2 = IsNull(index2);

				if (isNull1) {
					return isNull2 ? 0 : -1;
				}
				else {
					if (isNull2) {
						return 1;
					}
				}
				return String.Compare((string)this[index1], (string)this[index2], !Column.Table.CaseSensitive);
			}

			#endregion //Methods 
		}

		sealed class DateTimeDataContainer : ObjectDataContainer
		{
			#region Methods
			
			protected override void SetValue(int index, object value)
			{
				if ( value != null && value != DBNull.Value && !(value is DateTime)) {
					value = Convert.ToDateTime(value);
				}

				base.SetValue(index,value);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				 if (!CheckAndSetNull(index,record,field))
                                        base.SetValue(index,record.GetDateTime(field));
			}

			internal override int CompareValues(int index1, int index2)
			{
				bool isNull1 = IsNull(index1);
				bool isNull2 = IsNull(index2);

				if (isNull1) {
					return isNull2 ? 0 : -1;
				}
				else {
					if (isNull2) {
						return 1;
					}
				}
				return DateTime.Compare((DateTime)this[index1], (DateTime)this[index2]);
			}

			#endregion //Methods 
		}
	}
}
