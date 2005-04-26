//
// System.Data.Common.DbConvert
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//

using System;

using java.io;
using java.sql;

namespace System.Data.Common
{
	internal class DbConvert
	{
		#region Fields

		const long JAVA_MIN_MILLIS_UTC = -62135769600000L; // java.sql.Timestamp.valueOf("0001-01-01 00:00:00.000000000").getTime() at Greenwich time zone.
		static readonly long TIMEZONE_RAW_OFFSET;
		// .NET milliseconds value of DateTime(1582,1,1,0,0,0,0).Ticks/TimeSpan.TicksPerMillisecond			
		const long CLR_MILLIS_1582 = 49891507200000L;
		const long MILLIS_PER_TWO_DAYS = 2 * TimeSpan.TicksPerDay / TimeSpan.TicksPerMillisecond; // 172800000L;
		internal static readonly java.util.TimeZone DEFAULT_TIME_ZONE;

		#endregion // Fields

		#region Methods

		static DbConvert()
		{
			DEFAULT_TIME_ZONE = java.util.SimpleTimeZone.getDefault();			
			TIMEZONE_RAW_OFFSET = (long)DEFAULT_TIME_ZONE.getRawOffset();						
		}

		// The diff between .Net and Java goes as the following:
		//  * at 1582: java has 10 days less than .net
		//  * below 1500 (exept 1200,800,400) : each 100'th year java adds 1 day over .net. 
		// Current implementation compatible with .net in 1-99 and since 1582. In 100-1582 we're not compatible with .Ner nor with Java

		internal static long JavaMillisToClrMillis(long javaMillis)
		{
			return JavaMillisToClrMillisUTC(javaMillis) + TIMEZONE_RAW_OFFSET;
		}

		internal static long JavaMillisToClrMillisUTC(long javaMillis) {
			long clrMillis = javaMillis - JAVA_MIN_MILLIS_UTC;
			if (clrMillis > CLR_MILLIS_1582) {
				clrMillis -= MILLIS_PER_TWO_DAYS;
			}
			return clrMillis;
		}

		internal static long ClrMillisToJavaMillis(long clrMillis)
		{
			return ClrMillisToJavaMillisUTC(clrMillis) - TIMEZONE_RAW_OFFSET;
		}

		internal static long ClrMillisToJavaMillisUTC(long clrMillis) {
			long javaMillis = clrMillis + JAVA_MIN_MILLIS_UTC;
			if (clrMillis > CLR_MILLIS_1582) {
				javaMillis += MILLIS_PER_TWO_DAYS;
			}
			return javaMillis;
		}

		internal static java.sql.Time ClrTicksToJavaTime(long ticks) {
			return new Time((ticks / TimeSpan.TicksPerMillisecond)
				- DEFAULT_TIME_ZONE.getRawOffset());
		}

		internal static java.sql.Date ClrTicksToJavaDate(long ticks) {
			java.sql.Date d = new java.sql.Date(0);
			ClrTicksToJavaDate(d, ticks);
			return d;
		}

		internal static java.sql.Timestamp ClrTicksToJavaTimestamp(long ticks)
		{
			java.sql.Timestamp ts = new java.sql.Timestamp(0);
			ClrTicksToJavaDate(ts, ticks);

//			int nanos = (int)(ticks % TimeSpan.TicksPerMillisecond) * 100;
//			ts.setNanos(javaTimestamp.getNanos() + nanos);

			return ts;
		}

		internal static void ClrTicksToJavaDate(java.util.Date d, long ticks) {
			long millis = ClrMillisToJavaMillis(ticks / TimeSpan.TicksPerMillisecond);

			d.setTime(millis);
			if (DEFAULT_TIME_ZONE.inDaylightTime(d)) {
				millis -= DEFAULT_TIME_ZONE.getDSTSavings();
				d.setTime(millis);
			}
		}
		
		internal static long JavaTimestampToClrTicks(java.sql.Timestamp ts)
		{
			long ticks = JavaDateToClrTicks(ts);
			// Extra ticks, for dbs that can save them. 
			// We do not use it, since .net does not saves ticks for fractial milliseconds
			// long ticksLessThanMilliseconds = (ts.getNanos()*100) % TimeSpan.TicksPerMillisecond;
			// ticks += ticksLessThanMilliseconds;
			
			return ticks;
		}

		internal static long JavaDateToClrTicks(java.util.Date d) {
			long millis = JavaMillisToClrMillis(d.getTime());
			if (DEFAULT_TIME_ZONE.inDaylightTime(d)) {
				millis += DEFAULT_TIME_ZONE.getDSTSavings();
			}
			return millis * TimeSpan.TicksPerMillisecond;
		}

		internal static long JavaTimeToClrTicks(java.sql.Time t) {
			return (t.getTime() + DEFAULT_TIME_ZONE.getRawOffset())
				* TimeSpan.TicksPerMillisecond;
		}

		internal protected static Type JavaSqlTypeToClrType(int sqlTypeValue)
		{
			DbTypes.JavaSqlTypes sqlType = (DbTypes.JavaSqlTypes)sqlTypeValue;

			switch (sqlType) {
				case DbTypes.JavaSqlTypes.ARRAY : return typeof (java.sql.Array);
				case DbTypes.JavaSqlTypes.BIGINT : return DbTypes.TypeOfInt64;
				case DbTypes.JavaSqlTypes.BINARY : return DbTypes.TypeOfByteArray;
				case DbTypes.JavaSqlTypes.BIT : return DbTypes.TypeOfBoolean;
				case DbTypes.JavaSqlTypes.BLOB : return DbTypes.TypeOfByteArray;
				case DbTypes.JavaSqlTypes.BOOLEAN : return DbTypes.TypeOfBoolean;
				case DbTypes.JavaSqlTypes.CHAR : return DbTypes.TypeOfString;
				case DbTypes.JavaSqlTypes.CLOB : return DbTypes.TypeOfString;
//				case DbTypes.JavaSqlTypes.DATALINK :
				case DbTypes.JavaSqlTypes.DATE : return DbTypes.TypeOfDateTime;
				case DbTypes.JavaSqlTypes.DECIMAL : return DbTypes.TypeOfDecimal;
//				case DbTypes.JavaSqlTypes.DISTINCT :
				case DbTypes.JavaSqlTypes.DOUBLE : return DbTypes.TypeOfDouble;
				case DbTypes.JavaSqlTypes.FLOAT : return DbTypes.TypeOfDouble;
				case DbTypes.JavaSqlTypes.INTEGER : return DbTypes.TypeOfInt32;
//				case DbTypes.JavaSqlTypes.JAVA_OBJECT :
				case DbTypes.JavaSqlTypes.LONGVARBINARY : return DbTypes.TypeOfByteArray;
				case DbTypes.JavaSqlTypes.LONGVARCHAR : return DbTypes.TypeOfString;
				case DbTypes.JavaSqlTypes.NULL : return null;
				case DbTypes.JavaSqlTypes.NUMERIC : return DbTypes.TypeOfDecimal;
//				case DbTypes.JavaSqlTypes.OTHER :
				case DbTypes.JavaSqlTypes.REAL : return DbTypes.TypeOfSingle;
				case DbTypes.JavaSqlTypes.REF : return typeof (java.sql.Ref);
				case DbTypes.JavaSqlTypes.SMALLINT : return DbTypes.TypeOfInt16;
				case DbTypes.JavaSqlTypes.STRUCT : return typeof (java.sql.Struct);
				case DbTypes.JavaSqlTypes.TIME : return DbTypes.TypeOfTimespan;
				case DbTypes.JavaSqlTypes.TIMESTAMP : return DbTypes.TypeOfDateTime;
				case DbTypes.JavaSqlTypes.TINYINT : return DbTypes.TypeOfByte;
				case DbTypes.JavaSqlTypes.VARBINARY : return DbTypes.TypeOfByteArray;
				case DbTypes.JavaSqlTypes.VARCHAR : return DbTypes.TypeOfString;
				default : return DbTypes.TypeOfObject;
			}

		}


		internal protected static object JavaResultSetToClrWrapper(CallableStatement results,int columnIndex,DbTypes.JavaSqlTypes javaSqlType,int maxLength ,ResultSetMetaData resultsMetaData)
		{
			object returnValue = null;	
			sbyte[] sbyteArray;
			long milliseconds;
			long ticks;
			string s;
			columnIndex++; //jdbc style
			switch (javaSqlType) {
				case DbTypes.JavaSqlTypes.ARRAY :
					returnValue = results.getArray(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.BIGINT :
					returnValue = results.getLong(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.BINARY :
				case DbTypes.JavaSqlTypes.VARBINARY :
				case DbTypes.JavaSqlTypes.LONGVARBINARY :
					// FIXME : comsider using maxLength
					sbyteArray = results.getBytes(columnIndex);
					if (sbyteArray != null) {
						returnValue = vmw.common.TypeUtils.ToByteArray(sbyteArray);
					}
					break;
				case DbTypes.JavaSqlTypes.BIT :
					returnValue = results.getBoolean(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.BLOB :
					// FIXME : comsider using maxLength
					java.sql.Blob blob = results.getBlob(columnIndex);
					if (blob != null) {
						InputStream input = blob.getBinaryStream();					
						if (input == null) {
							returnValue = new byte[0];
						}
						else {
							long length = blob.length();
							byte[] byteValue = new byte[length];
							sbyte[] sbyteValue = vmw.common.TypeUtils.ToSByteArray(byteValue);
							input.read(sbyteValue);
							returnValue = byteValue;
						}
					}
					break;	
				case DbTypes.JavaSqlTypes.CHAR :						
					if (resultsMetaData != null && "uniqueidentifier".Equals(resultsMetaData.getColumnTypeName(columnIndex))) {
						returnValue = new Guid(results.getString(columnIndex));
					}
					else {
						// Oracle Jdbc driver returns extra trailing 0 chars for NCHAR columns, so we threat this at parameter.Size level
						s = results.getString(columnIndex);
						if ((s != null) && (maxLength < s.Length)) {
							s = s.Substring(0,maxLength);
						}
						returnValue = s;
					}
					break;
				case DbTypes.JavaSqlTypes.CLOB :
					// FIXME : comsider using maxLength
					java.sql.Clob clob = results.getClob(columnIndex);
					if (clob != null) {
						java.io.Reader reader = clob.getCharacterStream();					
						if (reader == null) {
							returnValue = String.Empty;
						}
						else {
							long length = clob.length();
							char[] charValue = new char[length];
							reader.read(charValue);
							returnValue = new string(charValue);
						}
					}
					break;		
				case DbTypes.JavaSqlTypes.TIME :
					Time t = results.getTime(columnIndex);
					if (t != null) {
						returnValue = new TimeSpan(JavaTimeToClrTicks(t));
					}
					break;	
				case DbTypes.JavaSqlTypes.DATE :
					Date d = results.getDate(columnIndex);
					if (d != null) {
						returnValue = new DateTime(JavaDateToClrTicks(d));
					}
					break;
				case DbTypes.JavaSqlTypes.TIMESTAMP :				
					Timestamp ts = results.getTimestamp(columnIndex);
					if (ts != null) {
						returnValue = new DateTime(JavaTimestampToClrTicks(ts));
					}
					break;		
				case DbTypes.JavaSqlTypes.DECIMAL :
				case DbTypes.JavaSqlTypes.NUMERIC :
					// java.sql.Types.NUMERIC (2), columnTypeName NUMBER, columnClassName java.math.BigDecimal 
					// therefore we rely on scale
					if (resultsMetaData != null &&  resultsMetaData.getScale(columnIndex) == -127) {
						// Oracle db type FLOAT
						returnValue = results.getDouble(columnIndex);
					}
					else {
						java.math.BigDecimal bigDecimal = results.getBigDecimal(columnIndex);
						if (bigDecimal != null) {
							returnValue = vmw.common.PrimitiveTypeUtils.BigDecimalToDecimal(bigDecimal);
						}
					}
					break;		
				case DbTypes.JavaSqlTypes.DISTINCT :
					returnValue = results.getObject(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.DOUBLE :
					returnValue = results.getDouble(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.FLOAT :
					//float f = results.getFloat(columnIndex);
					returnValue = results.getDouble(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.INTEGER :
					returnValue = results.getInt(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.JAVA_OBJECT :
					returnValue = results.getObject(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.LONGVARCHAR :
					returnValue = results.getString(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.NULL :
					returnValue = DBNull.Value;
					break;
				case DbTypes.JavaSqlTypes.OTHER :
					returnValue = results.getObject(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.REAL :
					returnValue = results.getFloat(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.REF :
					returnValue = results.getRef(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.SMALLINT :
					returnValue = results.getShort(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.STRUCT :
					returnValue = results.getObject(columnIndex);
					break;
				case DbTypes.JavaSqlTypes.TINYINT :
					returnValue = Convert.ToByte(results.getByte(columnIndex));
					break;
				case DbTypes.JavaSqlTypes.VARCHAR :
					s = results.getString(columnIndex);
					if ((s != null) && (maxLength < s.Length)) {
						s = s.Substring(0,maxLength);
					}
					returnValue = s;
					break;
				default :
					returnValue = results.getObject(columnIndex);
					break;
			}
				
			if (results.wasNull() || results == null) {
				return DBNull.Value;
			}                
			return  returnValue;
		}

		#endregion // Methods
	}
}
