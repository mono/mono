//
// System.Data.ProviderBase.ReaderCache.cs
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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
using java.sql;

namespace System.Data.ProviderBase
{
	public interface IReaderCacheContainer
	{
		void Fetch(ResultSet rs, int columnIndex, bool isSequential);
		bool IsNull();
		bool IsNumeric();
		object GetValue();
	}

	public abstract class ReaderCacheContainerBase : IReaderCacheContainer
	{
		#region Fields

		bool _isNull;

		#endregion // Fields

		#region Methods

		protected abstract void FetchInternal(ResultSet rs, int columnIndex);
		protected virtual void FetchInternal(ResultSet rs, int columnIndex, bool isSequential) {
			FetchInternal(rs, columnIndex);
		}

		public virtual bool IsNumeric() {
			return false;
		}

		public abstract object GetValue();		

		public void Fetch(ResultSet rs, int columnIndex, bool isSequential)
		{
			FetchInternal(rs, columnIndex + 1, isSequential);
			_isNull = rs.wasNull();
		}

		public bool IsNull()
		{
			return _isNull;
		}

		#endregion // Methods
	}


	internal sealed class ArrayReaderCacheContainer : ReaderCacheContainerBase // Types.ARRAY
	{
		#region Fields

		object _a;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			_a = rs.getArray(columnIndex).getArray();
		}

		public override object GetValue()
		{
			return _a;
		}

		#endregion // Methods
	}


	internal sealed class Int64ReaderCacheContainer : ReaderCacheContainerBase // Types.BIGINT
	{
		#region Fields
		
		long _l;

		#endregion // Fields

		#region Methods

		protected override  void FetchInternal(ResultSet rs, int columnIndex)
		{
			_l = rs.getLong(columnIndex);
		}

		public override bool IsNumeric() {
			return true;
		}


		public override object GetValue()
		{
			return _l;
		}

		internal long GetInt64()
		{
			return _l;
		}

		#endregion // Methods
	}


	internal class BytesReaderCacheContainer : ReaderCacheContainerBase // Types.BINARY, Types.VARBINARY, Types.LONGVARBINARY
	{
		#region Fields

		protected byte[] _b;
		
		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			sbyte[] sbyteArray = rs.getBytes(columnIndex);
			if (sbyteArray != null) {
				_b = (byte[])vmw.common.TypeUtils.ToByteArray(sbyteArray);
			}
		}

		public override object GetValue()
		{
			return _b;
		}

		internal byte[] GetBytes()
		{
			return (byte[])GetValue();
		}

		internal virtual long GetBytes(
			long dataIndex,
			byte[] buffer,
			int bufferIndex,
			int length) {
			if (_b == null)
				return 0;
			if (buffer == null)
				return _b.LongLength;
			long actualLength = ((dataIndex + length) >= _b.LongLength) ? (_b.LongLength - dataIndex) : length;
			Array.Copy(_b,dataIndex,buffer,bufferIndex,actualLength);
			return actualLength;
		}

		#endregion // Methods
	}


	internal sealed class BooleanReaderCacheContainer : ReaderCacheContainerBase // Types.BIT
	{
		#region Fields
		
		bool _b;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			_b = rs.getBoolean(columnIndex);
		}

		public override bool IsNumeric() {
			return true;
		}

		public override object GetValue()
		{
			return _b;
		}

		internal bool GetBoolean()
		{
			return _b;
		}

		#endregion // Methods
	}


	internal sealed class BlobReaderCacheContainer : BytesReaderCacheContainer // Types.BLOB
	{
		#region Fields

		static readonly byte[] _emptyByteArr = new byte[0];
		java.sql.Blob _blob;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex) {
			throw new NotImplementedException("Should not be called");
		}


		protected override void FetchInternal(ResultSet rs, int columnIndex, bool isSequential)
		{
			_blob = rs.getBlob(columnIndex);
			if (!isSequential)
				ReadAll();
			
		}

		void ReadAll() {
			if (_blob != null) {
				long length = _blob.length();								
				if (length == 0) {
					_b = _emptyByteArr;
				}
				else {	
					java.io.InputStream input = _blob.getBinaryStream();	
					byte[] byteValue = new byte[length];
					sbyte[] sbyteValue = vmw.common.TypeUtils.ToSByteArray(byteValue);
					input.read(sbyteValue);
					_b = byteValue;
				}
			}
		}

		public override object GetValue()
		{
			if (_b == null)
				ReadAll();
			return base.GetValue();
		}

		internal override long GetBytes(long dataIndex, byte[] buffer, int bufferIndex, int length) {
			if (_b != null)
				return base.GetBytes (dataIndex, buffer, bufferIndex, length);

			if (_blob == null)
				return 0;

			if (buffer == null)
				return _blob.length();

			java.io.InputStream input = _blob.getBinaryStream();
			input.skip(dataIndex);
			return input.read(vmw.common.TypeUtils.ToSByteArray(buffer), bufferIndex, length);
		}


		#endregion // Methods
	}
	

	internal abstract class CharsReaderCacheContainer : ReaderCacheContainerBase // 
	{
		#region Fields
		
		#endregion // Fields

		#region Methods

		internal abstract long GetChars(
			long dataIndex,
			char[] buffer,
			int bufferIndex,
			int length);

		#endregion // Methods
	}


	internal sealed class GuidReaderCacheContainer : ReaderCacheContainerBase // Types.CHAR
	{
		#region Fields
		
		Guid _g;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			string s = rs.getString(columnIndex);
			if (s != null)
				_g = new Guid(s);
		}

		public override object GetValue()
		{
			return _g;
		}

		internal Guid GetGuid()
		{
			return _g;
		}

		#endregion // Methods
	}


	internal sealed class ClobReaderCacheContainer : StringReaderCacheContainer // Types.CLOB
	{
		#region Fields
		
		java.sql.Clob _clob;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex, bool isSequential)
		{
			_clob = rs.getClob(columnIndex);
			if (!isSequential)
				ReadAll();
			
		}

		void ReadAll() {
			if (_clob != null) {
				long length = _clob.length();								
				if (length == 0) {
					_s = String.Empty;
				}
				else {	
					java.io.Reader reader = _clob.getCharacterStream();	
					char[] charValue = new char[length];
					reader.read(charValue);
					if (charValue != null)
						_s = new String(charValue);
				}
			}
		}

		public override object GetValue()
		{
			if (_s == null)
				ReadAll();
			return base.GetValue();
		}

		internal override long GetChars(long dataIndex, char[] buffer, int bufferIndex, int length) {
			if (_s != null)
				return base.GetChars (dataIndex, buffer, bufferIndex, length);

			if (_clob == null)
				return 0;

			if (buffer == null)
				return _clob.length();

			java.io.Reader reader = _clob.getCharacterStream();
			reader.skip(dataIndex);
			return reader.read(buffer, bufferIndex, length);
		}


		#endregion // Methods
	}
	

	internal sealed class TimeSpanReaderCacheContainer : ReaderCacheContainerBase // Types.TIME
	{
		#region Fields
		
		TimeSpan _t;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			Time t = rs.getTime(columnIndex);
			if (t != null) {				
				_t = new TimeSpan(DbConvert.JavaTimeToClrTicks(t));
			}
		}

		public override object GetValue()
		{
			return _t;
		}

		internal TimeSpan GetTimeSpan()
		{
			return _t;
		}

		#endregion // Methods
	}


	internal class DateTimeReaderCacheContainer : ReaderCacheContainerBase // Types.TIMESTAMP
	{
		#region Fields
		
		protected DateTime _d;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			Date d = rs.getDate(columnIndex);
			if (d != null) {
				_d = new DateTime(DbConvert.JavaDateToClrTicks(d));
			}
		}

		public override object GetValue()
		{
			return _d;
		}

		internal DateTime GetDateTime()
		{
			return _d;
		}

		#endregion // Methods
	}

	internal sealed class TimestampReaderCacheContainer : DateTimeReaderCacheContainer // Types.DATE
	{
		protected override void FetchInternal(ResultSet rs, int columnIndex) {
			Timestamp ts = rs.getTimestamp(columnIndex);
			if (ts != null) {
				_d = new DateTime(DbConvert.JavaTimestampToClrTicks(ts));
			}
		}
	}


	internal sealed class DecimalReaderCacheContainer : ReaderCacheContainerBase // Types.DECIMAL, Types.NUMERIC
	{
		#region Fields
		
		decimal _d;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			java.math.BigDecimal bigDecimal = rs.getBigDecimal(columnIndex);
			if (bigDecimal != null) {
				_d = (decimal)vmw.common.PrimitiveTypeUtils.BigDecimalToDecimal(bigDecimal);
			}
		}

		public override bool IsNumeric() {
			return true;
		}

		public override object GetValue()
		{
			return _d;
		}

		internal decimal GetDecimal()
		{
			return _d;
		}

		#endregion // Methods
	}


	internal sealed class DoubleReaderCacheContainer : ReaderCacheContainerBase // Types.DOUBLE, Types.Float, Types.NUMERIC for Oracle with scale = -127
	{
		#region Fields
		
		double _d;

		#endregion // Fields

		#region Methods

		protected override  void FetchInternal(ResultSet rs, int columnIndex)
		{
			_d = rs.getDouble(columnIndex);
		}

		public override bool IsNumeric() {
			return true;
		}

		public override object GetValue()
		{
			return _d;
		}

		internal double GetDouble()
		{
			return _d;
		}

		#endregion // Methods
	}


	internal sealed class Int32ReaderCacheContainer : ReaderCacheContainerBase // Types.INTEGER
	{
		#region Fields
		
		int _i;

		#endregion // Fields

		#region Methods

		protected override  void FetchInternal(ResultSet rs, int columnIndex)
		{
			_i = rs.getInt(columnIndex);
		}

		public override bool IsNumeric() {
			return true;
		}

		public override object GetValue()
		{
			return _i;
		}

		internal int GetInt32()
		{
			return _i;
		}

		#endregion // Methods
	}


	internal class StringReaderCacheContainer : CharsReaderCacheContainer // Types.LONGVARCHAR, Types.VARCHAR, Types.CHAR
	{
		#region Fields
		
		protected string _s;

		#endregion // Fields

		#region Methods

		protected override  void FetchInternal(ResultSet rs, int columnIndex)
		{
			_s = rs.getString(columnIndex);
			// Oracle Jdbc driver returns extra trailing 0 chars for NCHAR columns
//			if ((_s != null) && (_jdbcType == 1)) {	
//				Console.WriteLine(_jdbcType);
//				int zeroIndex = ((string)_s).IndexOf((char)0);
//				if (zeroIndex > 0) {
//					Console.WriteLine("zero-padded");
//					_s = ((string)_s).Substring(0,zeroIndex);
//				}
//				else {
//					// Oracle sometimes pads with blanks (32)
//					int blankIndex = ((string)_s).IndexOf((char)32);
//					if (blankIndex > 0) {
//						Console.WriteLine("blank-padded");
//						_s = ((string)_s).Substring(0,blankIndex);
//					}
//				}
//			}
		}

		public override object GetValue()
		{
			return _s;
		}

		internal string GetString()
		{
			return _s;
		}
		
		internal override long GetChars(long dataIndex, char[] buffer, int bufferIndex, int length) {
			if (_s == null)
				return 0;
			if (buffer == null)
				return _s.Length;
			int actualLength = ((dataIndex + length) >= _s.Length) ? (_s.Length - (int)dataIndex) : length;
			_s.CopyTo((int)dataIndex, buffer, bufferIndex, actualLength);
			return actualLength;
		}


		#endregion // Methods
	}


	internal sealed class NullReaderCacheContainer : ReaderCacheContainerBase // Types.NULL
	{
		#region Fields

		#endregion // Fields

		#region Methods

		protected override  void FetchInternal(ResultSet rs, int columnIndex)
		{
		}

		public override object GetValue()
		{
			return DBNull.Value;
		}

		#endregion // Methods
	}


	internal sealed class FloatReaderCacheContainer : ReaderCacheContainerBase // Types.REAL
	{
		#region Fields
		
		float _f;

		#endregion // Fields

		#region Methods

		protected override  void FetchInternal(ResultSet rs, int columnIndex)
		{
			_f = rs.getFloat(columnIndex);
		}

		public override bool IsNumeric() {
			return true;
		}

		public override object GetValue()
		{
			return _f;
		}

		internal float GetFloat()
		{
			return _f;
		}

		#endregion // Methods
	}


	internal sealed class RefReaderCacheContainer : ReaderCacheContainerBase // Types.REF
	{
		#region Fields
		
		java.sql.Ref _r;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			_r = rs.getRef(columnIndex);
		}

		public override object GetValue()
		{
			return _r;
		}

		#endregion // Methods
	}


	internal sealed class Int16ReaderCacheContainer : ReaderCacheContainerBase // Types.SMALLINT
	{
		#region Fields
		
		short _s;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			_s = rs.getShort(columnIndex);
		}

		public override bool IsNumeric() {
			return true;
		}

		public override object GetValue()
		{
			return _s;
		}

		internal short GetInt16()
		{
			return _s;
		}

		#endregion // Methods
	}


	internal sealed class ByteReaderCacheContainer : ReaderCacheContainerBase // Types.TINYINT
	{
		#region Fields
		
		byte _b;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			_b = (byte)rs.getByte(columnIndex);
		}

		public override bool IsNumeric() {
			return true;
		}

		public override object GetValue()
		{
			return _b;
		}

		internal byte GetByte()
		{
			return _b;
		}

		#endregion // Methods
	}


	internal sealed class ObjectReaderCacheContainer : ReaderCacheContainerBase // Types.Distinct, Types.JAVA_OBJECT, Types.OTHER, Types.STRUCT
	{
		#region Fields
		
		object o;

		#endregion // Fields

		#region Methods

		protected override  void FetchInternal(ResultSet rs, int columnIndex)
		{
			o = rs.getObject(columnIndex);
		}

		public override object GetValue()
		{
			return o;
		}

		#endregion // Methods
	}

}
