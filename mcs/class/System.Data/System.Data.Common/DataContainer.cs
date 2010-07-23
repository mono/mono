//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Reflection.Emit;

namespace System.Data.Common
{
	internal abstract class DataContainer {
		BitArray null_values;
		System.Type _type;
		DataColumn _column;

		// implementing class protocol
		protected abstract object GetValue (int index);
		internal abstract long GetInt64 (int index);

		// used to set the array value to something neutral when the corresponding item is null (in the database sense)
		// note: we don't actually ever look at the value written there, but the GC may like us to avoid keeping stale
		// values in the array.
		protected abstract void ZeroOut (int index);
		protected abstract void SetValue (int index, object value);
		protected abstract void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field);

		protected abstract void DoCopyValue (DataContainer from, int from_index, int to_index);
		protected abstract int DoCompareValues (int index1, int index2);

		protected abstract void Resize (int length);

		internal object this [int index] {
			get { return IsNull (index) ? DBNull.Value : GetValue (index); }
			set {
				if (value == null) {
					CopyValue (Column.Table.DefaultValuesRowIndex, index);
					return;
				}

				bool is_dbnull = value == DBNull.Value;
				if (is_dbnull)
					ZeroOut (index);
				else
					SetValue (index, value);
				null_values [index] = is_dbnull;
			}
		}

		internal int Capacity {
			get { return null_values != null ? null_values.Count : 0; }
			set {
				int old_capacity = Capacity;
				if (value == old_capacity)
					return;
				if (null_values == null)
					null_values = new BitArray (value);
				else
					null_values.Length = value;
				Resize (value);
			}
		}

		internal Type Type {
			get { return _type; }
		}

		protected DataColumn Column {
			get { return _column; }
		}

		internal static DataContainer Create (Type type, DataColumn column)
		{
			DataContainer container;
			switch (Type.GetTypeCode(type)) {
			case TypeCode.Int16:
				container = new Int16DataContainer ();
				break;
			case TypeCode.Int32:
				container = new Int32DataContainer ();
				break;
			case TypeCode.Int64:
				container = new Int64DataContainer ();
				break;
			case TypeCode.String:
				container = new StringDataContainer ();
				break;
			case TypeCode.Boolean:
				container = new BitDataContainer ();
				break;
			case TypeCode.Byte:
				container = new ByteDataContainer ();
				break;
			case TypeCode.Char:
				container = new CharDataContainer ();
				break;
			case TypeCode.Double:
				container = new DoubleDataContainer ();
				break;
			case TypeCode.SByte:
				container = new SByteDataContainer ();
				break;
			case TypeCode.Single:
				container = new SingleDataContainer ();
				break;
			case TypeCode.UInt16:
				container = new UInt16DataContainer ();
				break;
			case TypeCode.UInt32:
				container = new UInt32DataContainer ();
				break;
			case TypeCode.UInt64:
				container = new UInt64DataContainer ();
				break;
			case TypeCode.DateTime:
				container = new DateTimeDataContainer ();
				break;
			case TypeCode.Decimal:
				container = new DecimalDataContainer ();
				break;
			default:
				container = new ObjectDataContainer ();
				break;
			}
			container._type = type;
			container._column = column;
			return container;
		}

		internal static object GetExplicitValue (object value) 
		{
			Type valueType = value.GetType ();
			MethodInfo method = valueType.GetMethod ("op_Explicit", new Type[]{valueType});
			if (method != null) 
				return (method.Invoke (value, new object[]{value}));
			return null;
		}
		
		internal object GetContainerData (object value) 
		{
			object obj; 
			TypeCode tc;

			if (_type.IsInstanceOfType (value)) {
				return value;
			} else if ((tc = Type.GetTypeCode (_type)) == TypeCode.String) {
				return (Convert.ToString (value));
			} else if (value is IConvertible) {
				switch (tc) {
					case TypeCode.Int16:
						return (Convert.ToInt16 (value));
					case TypeCode.Int32:
						return (Convert.ToInt32 (value));
					case TypeCode.Int64:
						return (Convert.ToInt64 (value));
					case TypeCode.Boolean:
						return (Convert.ToBoolean (value));
					case TypeCode.Byte:
						return (Convert.ToByte (value));
					case TypeCode.Char:
						return (Convert.ToChar (value));
					case TypeCode.Double:
						return (Convert.ToDouble (value));
					case TypeCode.SByte:
						return (Convert.ToSByte (value));
					case TypeCode.Single:
						return (Convert.ToSingle (value));
					case TypeCode.UInt16:
						return (Convert.ToUInt16 (value));
					case TypeCode.UInt32:
						return (Convert.ToUInt32 (value));
					case TypeCode.UInt64:
						return (Convert.ToUInt64 (value));
					case TypeCode.DateTime:
						return (Convert.ToDateTime (value));
					case TypeCode.Decimal:
						return (Convert.ToDecimal (value));
					default:
						throw new InvalidCastException ();
				}
			} else if ((obj = GetExplicitValue (value)) != null) {
				return (obj);
			} else {
				throw new InvalidCastException ();
			}
		}
		
		internal bool IsNull (int index)
		{
			return null_values == null || null_values [index];
		}

		internal void FillValues (int fromIndex)
		{
			for (int i = 0; i < Capacity; i++)
				CopyValue (fromIndex, i);
		}

		internal void CopyValue (int from_index, int to_index)
		{
			CopyValue (this, from_index, to_index);
		}

		internal void CopyValue (DataContainer from, int from_index, int to_index)
		{
			DoCopyValue (from, from_index, to_index);
			null_values [to_index] = from.null_values [from_index];
		}

		internal void SetItemFromDataRecord (int index, IDataRecord record, int field)
		{
			if (record.IsDBNull (field))
				this [index] = DBNull.Value;
			else if (record is ISafeDataRecord)
				SetValueFromSafeDataRecord (index, (ISafeDataRecord) record, field);
			else
				this [index] = record.GetValue (field);
		}

		internal int CompareValues (int index1, int index2)
		{
			bool null1 = IsNull (index1);
			bool null2 = IsNull (index2);

			if (null1 == null2)
				return null1 ? 0 : DoCompareValues (index1, index2);
			return null1 ? -1 : 1;
		}
	}

	sealed class BitDataContainer : DataContainer {
		BitArray _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = false;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (bool) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = record.GetBooleanSafe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((BitDataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			bool val1 = _values [index1];
			bool val2 = _values [index2];
			return val1 == val2 ? 0 : val1 ? 1 : -1;
		}

		protected override void Resize (int size)
		{
			if (_values == null)
				_values = new BitArray (size);
			else
				_values.Length = size;
		}

		internal override long GetInt64 (int index)
		{
			return Convert.ToInt64 (_values [index]);
		}
	}

	sealed class CharDataContainer : DataContainer {
		char [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = '\0';
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (char) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = record.GetCharSafe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((CharDataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			char val1 = _values [index1];
			char val2 = _values [index2];
			return val1 == val2 ? 0 : val1 < val2 ? -1 : 1;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new char [size];
				return;
			}

			char[] tmp = new char [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return Convert.ToInt64 (_values [index]);
		}
	}

	sealed class ByteDataContainer : DataContainer {
		byte [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (byte) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = record.GetByteSafe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((ByteDataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			int val1 = _values [index1];
			int val2 = _values [index2];
			return val1 - val2;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new byte [size];
				return;
			}

			byte[] tmp = new byte [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return _values [index];
		}
	}

	sealed class SByteDataContainer : DataContainer {
		sbyte [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (sbyte) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = (sbyte) record.GetByteSafe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((SByteDataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			int val1 = _values [index1];
			int val2 = _values [index2];
			return val1 - val2;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new sbyte [size];
				return;
			}

			sbyte[] tmp = new sbyte [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return _values [index];
		}
	}

	sealed class Int16DataContainer : DataContainer {
		short [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (short) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = record.GetInt16Safe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((Int16DataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			int val1 = _values [index1];
			int val2 = _values [index2];
			return val1 - val2;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new short [size];
				return;
			}

			short[] tmp = new short [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return _values [index];
		}
	}

	sealed class UInt16DataContainer : DataContainer {
		ushort [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (ushort) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = (ushort) record.GetInt16Safe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((UInt16DataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			int val1 = _values [index1];
			int val2 = _values [index2];
			return val1 - val2;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new ushort [size];
				return;
			}

			ushort[] tmp = new ushort [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return _values [index];
		}
	}

	sealed class Int32DataContainer : DataContainer {
		int [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (int) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = record.GetInt32Safe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((Int32DataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			int val1 = _values [index1];
			int val2 = _values [index2];
			return val1 == val2 ? 0 : val1 < val2 ? -1 : 1;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new int [size];
				return;
			}

			int[] tmp = new int [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return _values [index];
		}
	}

	sealed class UInt32DataContainer : DataContainer {
		uint [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (uint) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = (uint) record.GetInt32Safe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((UInt32DataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			uint val1 = _values [index1];
			uint val2 = _values [index2];
			return val1 == val2 ? 0 : val1 < val2 ? -1 : 1;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new uint [size];
				return;
			}

			uint[] tmp = new uint [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return _values [index];
		}
	}

	sealed class Int64DataContainer : DataContainer {
		long [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (long) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = record.GetInt64Safe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((Int64DataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			long val1 = _values [index1];
			long val2 = _values [index2];
			return val1 == val2 ? 0 : val1 < val2 ? -1 : 1;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new long [size];
				return;
			}

			long[] tmp = new long [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return _values [index];
		}
	}

	sealed class UInt64DataContainer : DataContainer {
		ulong [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (ulong) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = (ulong) record.GetInt64Safe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((UInt64DataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			ulong val1 = _values [index1];
			ulong val2 = _values [index2];
			return val1 == val2 ? 0 : val1 < val2 ? -1 : 1;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new ulong [size];
				return;
			}

			ulong[] tmp = new ulong [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return Convert.ToInt64 (_values [index]);
		}
	}

	sealed class SingleDataContainer : DataContainer {
		float [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (float) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = record.GetFloatSafe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((SingleDataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			float val1 = _values [index1];
			float val2 = _values [index2];
			return val1 == val2 ? 0 : val1 < val2 ? -1 : 1;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new float [size];
				return;
			}

			float[] tmp = new float [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return Convert.ToInt64 (_values [index]);
		}
	}

	sealed class DoubleDataContainer : DataContainer {
		double [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = 0;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = (double) GetContainerData (value);
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = record.GetDoubleSafe (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((DoubleDataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			double val1 = _values [index1];
			double val2 = _values [index2];
			return val1 == val2 ? 0 : val1 < val2 ? -1 : 1;
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new double [size];
				return;
			}

			double[] tmp = new double [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return Convert.ToInt64 (_values[index]);
		}
	}

	class ObjectDataContainer : DataContainer {
		object [] _values;

		protected override object GetValue (int index)
		{
			return _values [index];
		}

		protected override void ZeroOut (int index)
		{
			_values [index] = null;
		}

		protected override void SetValue (int index, object value)
		{
			_values [index] = value;
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			_values [index] = record.GetValue (field);
		}

		protected override void DoCopyValue (DataContainer from, int from_index, int to_index)
		{
			_values [to_index] = ((ObjectDataContainer) from)._values [from_index];
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			object obj1 = _values [index1];
			object obj2 = _values [index2];

			if (obj1 == obj2)
				return 0;

			if (obj1 is IComparable) {
				try {
					return ((IComparable)obj1).CompareTo (obj2);
				} catch {
					if (obj2 is IComparable) {
						obj2 = Convert.ChangeType (obj2, Type.GetTypeCode (obj1.GetType ()));
						return ((IComparable)obj1).CompareTo (obj2);
					}
				}
			}

			return String.Compare (obj1.ToString (), obj2.ToString ());
		}

		protected override void Resize (int size)
		{
			if (_values == null) {
				_values = new object [size];
				return;
			}

			object[] tmp = new object [size];
			Array.Copy (_values, 0, tmp, 0, _values.Length);
			_values = tmp;
		}

		internal override long GetInt64 (int index)
		{
			return Convert.ToInt64 (_values [index]);
		}
	}

	sealed class DateTimeDataContainer : ObjectDataContainer {
		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			base.SetValue (index, record.GetDateTimeSafe (field));
		}

		protected override void SetValue (int index, object value)
		{
			base.SetValue (index, GetContainerData (value));
		}
	}

	sealed class DecimalDataContainer : ObjectDataContainer {
		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			base.SetValue (index, record.GetDecimalSafe (field));
		}

		protected override void SetValue (int index, object value)
		{
			base.SetValue (index, GetContainerData (value));
		}
	}

	sealed class StringDataContainer : ObjectDataContainer {
		private void SetValue (int index, string value)
		{
			if (value != null && Column.MaxLength >= 0 && Column.MaxLength < value.Length)
				throw new ArgumentException ("Cannot set column '" + Column.ColumnName + "' to '" + value + "'. The value violates the MaxLength limit of this column.");
			base.SetValue (index, value);
		}

		protected override void SetValue (int index, object value)
		{
			SetValue (index, (string) GetContainerData (value));
		}

		protected override void SetValueFromSafeDataRecord (int index, ISafeDataRecord record, int field)
		{
			SetValue (index, record.GetStringSafe (field));
		}

		protected override int DoCompareValues (int index1, int index2)
		{
			DataTable table = Column.Table;
			return String.Compare ((string) this [index1], (string) this [index2], !table.CaseSensitive, table.Locale);
		}
	}
}
