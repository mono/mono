using System;

namespace System.Data.Common
{
	public class DbTypes
	{
#if TARGET_JVM
		#region java.sql.Types constants

		internal enum JavaSqlTypes {
			ARRAY = 2003 ,
			BIGINT = -5, 
			BINARY = -2 ,
			BIT = -7 ,
			BLOB = 2004, 
			BOOLEAN = 16, 
			CHAR = 1, 
			CLOB = 2005, 
			DATALINK = 70, 
			DATE = 91, 
			DECIMAL = 3, 
			DISTINCT = 2001, 
			DOUBLE = 8, 
			FLOAT = 6, 
			INTEGER = 4, 
			JAVA_OBJECT = 2000, 
			LONGVARBINARY = -4,
			LONGVARCHAR = -1, 
			NULL = 0, 
			NUMERIC = 2 ,
			OTHER = 1111 ,
			REAL = 7 ,
			REF = 2006 ,
			SMALLINT = 5,
			STRUCT = 2002, 
			TIME = 92, 
			TIMESTAMP = 93, 
			TINYINT = -6, 
			VARBINARY = -3, 
			VARCHAR = 12,
//			NOTSET = int.MinValue
		}


		#endregion // java.sql.Types constants
#endif

		#region .Net types constants

		internal static readonly Type TypeOfBoolean = typeof(Boolean);
		internal static readonly Type TypeOfSByte = typeof(SByte);
		internal static readonly Type TypeOfChar = typeof(Char);
		internal static readonly Type TypeOfInt16 = typeof(Int16);
		internal static readonly Type TypeOfInt32 = typeof(Int32);
		internal static readonly Type TypeOfInt64 = typeof(Int64);
		internal static readonly Type TypeOfByte = typeof(Byte);
		internal static readonly Type TypeOfUInt16 = typeof(UInt16);
		internal static readonly Type TypeOfUInt32 = typeof(UInt32);
		internal static readonly Type TypeOfUInt64 = typeof(UInt64);
		internal static readonly Type TypeOfDouble = typeof(Double);
		internal static readonly Type TypeOfSingle = typeof(Single);
		internal static readonly Type TypeOfDecimal = typeof(Decimal);
		internal static readonly Type TypeOfString = typeof(String);
		internal static readonly Type TypeOfDateTime = typeof(DateTime);		
		internal static readonly Type TypeOfObject = typeof(object);
		internal static readonly Type TypeOfGuid = typeof(Guid);
		internal static readonly Type TypeOfType = typeof(Type);

		// additional types
		internal static readonly Type TypeOfByteArray = typeof(Byte[]);
		internal static readonly Type TypeOfFloat = typeof (float);
		internal static readonly Type TypeOfTimespan = typeof (TimeSpan);

		#endregion // .Net types constants
	}
}
