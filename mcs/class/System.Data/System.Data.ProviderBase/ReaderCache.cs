using System;
using java.sql;

namespace System.Data.Common
{
	public interface IReaderCacheContainer
	{
		void Fetch(ResultSet rs, int columnIndex);
		bool IsNull();
		object GetValue();
	}

	internal abstract class ReaderCacheContainerBase : IReaderCacheContainer
	{
		#region Fields

		bool _isNull;

		#endregion // Fields

		#region Methods

		protected abstract void FetchInternal(ResultSet rs, int columnIndex);

		public abstract object GetValue();		

		public void Fetch(ResultSet rs, int columnIndex)
		{
			FetchInternal(rs, columnIndex + 1);
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

		java.sql.Array _a;

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			_a = rs.getArray(columnIndex);
		}

		public override object GetValue()
		{
			return _a;
		}

		internal java.sql.Array GetArray()
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
			return _b;
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

		#endregion // Fields

		#region Methods

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			java.sql.Blob blob = rs.getBlob(columnIndex);
			if (blob != null) {
				long length = blob.length();								
				if (length == 0) {
					_b = _emptyByteArr;
				}
				else {	
					java.io.InputStream input = blob.getBinaryStream();	
					byte[] byteValue = new byte[length];
					sbyte[] sbyteValue = vmw.common.TypeUtils.ToSByteArray(byteValue);
					input.read(sbyteValue);
					_b = byteValue;
				}
			}
		}

		public override object GetValue()
		{
			return _b;
		}

		#endregion // Methods
	}
	

	internal abstract class CharsReaderCacheContainer : ReaderCacheContainerBase // 
	{
		#region Fields
		
		#endregion // Fields

		#region Methods

		internal abstract char[] GetChars();

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
			_g = new Guid(rs.getString(columnIndex));
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
		
		char[] _c;

		#endregion // Fields

		#region Methods

		// FIXME : conside adding stream wrapper interface

		protected override void FetchInternal(ResultSet rs, int columnIndex)
		{
			java.sql.Clob clob = rs.getClob(columnIndex);			
			if (clob != null) {
				long length = clob.length();								
				if (length == 0) {
					_s = String.Empty;
					_c = String.Empty.ToCharArray();
				}
				else {	
					java.io.Reader reader = clob.getCharacterStream();	
					char[] charValue = new char[length];
					reader.read(charValue);
					_c = charValue;
					
				}
			}
		}

		public override object GetValue()
		{
			if (_s == null && _c != null) {
				_s = (_c.Length != 0) ? new String(_c) : String.Empty;
			}
			return _s;
		}

		internal override char[] GetChars()
		{
			return _c;
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

		internal override char[] GetChars()
		{
			return _s.ToCharArray();
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
