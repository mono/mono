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

		protected void SetNull(int index,bool isNull)
		{
			_nullValues[index] = isNull;
		}

		internal virtual void CopyValue(AbstractDataContainer fromContainer, int fromIndex, int toIndex)
		{
			_nullValues[toIndex] = fromContainer._nullValues[fromIndex];
		}

		internal virtual void CopyValue(int fromIndex, int toIndex)
		{
			_nullValues[toIndex] = _nullValues[fromIndex];
		}

		internal abstract void SetItemFromDataRecord(int index, IDataRecord record, int field);

		internal abstract int CompareValues(int index1, int index2);

		#endregion //Methods

		sealed class Int16DataContainer : AbstractDataContainer
		{
			#region Fields
		
			short[] _values;

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					return _values[index];
				}
				set {
					if (value == null || value ==  DBNull.Value) {
						SetValue(index,0);
					}
					else if( value is int ) {
						SetValue(index,(short)value);
					}
					else {
						SetValue(index,Convert.ToInt16(value));
					}
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
				SetNull(index,value == 0);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
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
				return (_values[index1] - _values[index2]);
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
					return _values[index];
				}
				set {
					if (value == null || value ==  DBNull.Value) {
						SetValue(index,0);
					}
					else if( value is int ) {
						SetValue(index,(int)value);
					}
					else {
						SetValue(index,Convert.ToInt32(value));
					}
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
				SetNull(index,value == 0);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
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

				if (i1 == i2) {
					return 0;
				}
				return (i1 > i2) ? 1 : -1;
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
					return _values[index];
				}
				set {
					if (value == null || value ==  DBNull.Value) {
						SetValue(index,0);
					}
					else if( value is long ) {
						SetValue(index,(long)value);
					}
					else {
						SetValue(index,Convert.ToInt64(value));
					}
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
				SetNull(index,value == 0);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
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

				if (l1 == l2) {
					return 0;
				}
				return (l1 > l2) ? 1 : -1;
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
					return _values[index];
				}
				set {
					if (value == null || value ==  DBNull.Value) {
						SetValue(index,0);
					}
					else if( value is float ) {
						SetValue(index,(float)value);
					}
					else {
						SetValue(index,Convert.ToSingle(value));
					}
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
				SetNull(index,value == 0);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
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
				return (int)(_values[index1] - _values[index2]);
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
					return _values[index];
				}
				set {
					if (value == null || value ==  DBNull.Value) {
						SetValue(index,0);
					}
					else if( value is double ) {
						SetValue(index,(double)value);
					}
					else {
						SetValue(index,Convert.ToDouble(value));
					}
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
				SetNull(index,value == 0);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
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
				return (int)(_values[index1] - _values[index2]);
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
					return _values[index];
				}
				set {
					if (value == null || value ==  DBNull.Value) {
						SetValue(index,0);
					}
					else if( value is byte ) {
						SetValue(index,(byte)value);
					}
					else {
						SetValue(index,Convert.ToByte(value));
					}
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
				SetNull(index,value == 0);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
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
				return (_values[index1] - _values[index2]);
			}

			#endregion //Methods
		}

		sealed class BitDataContainer : AbstractDataContainer
		{
			#region Fields
		
			// we don't need _values - using _nullValues instead

			#endregion //Fields

			#region Properties

			internal override object this[int index] {
				get {
					return !IsNull(index);
				}
				set {
					if (value == null || value ==  DBNull.Value) {
						SetValue(index,false);
					}
					else if( value is bool ) {
						SetValue(index,(bool)value);
					}
					else {
						SetValue(index,Convert.ToBoolean(value));
					}
				}
			}

			internal override int Capacity {
				set {
					base.Capacity = value;
				}
			}

			#endregion //Properties

			#region Methods
			
			private void SetValue(int index, bool value)
			{
				SetNull(index,!value);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				SetValue(index,record.GetBoolean(field));
			}

			internal override void CopyValue(int fromIndex, int toIndex)
			{
				base.CopyValue(fromIndex, toIndex);
			}

			internal override int CompareValues(int index1, int index2)
			{
				return ((int)this[index1] - (int)this[index2]);
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
					value = DBNull.Value;
				}
				_values[index] = value;
				SetNull(index,value == DBNull.Value);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
				SetValue(index,record.GetValue(field));
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
				if ( value != null && value != DBNull.Value && !(value is string)) {
					SetValue(index, Convert.ToString(value));
					return;
				}

				base.SetValue(index, value);
			}

			internal override void SetItemFromDataRecord(int index, IDataRecord record, int field)
			{
				// if exception thrown, it should be caught 
				// in the  caller method
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
