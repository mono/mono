using System.Data.SqlTypes;
using Mono.Data.Tds.Protocol;

namespace System.Data.SqlClient
{
	static class SqlDecimalExtensions
	{
		public static readonly SqlDecimal Null;

		internal static SqlDecimal FromTdsBigDecimal (TdsBigDecimal x)
		{
			if (x == null)
				return Null;
			else
				return new SqlDecimal (x.Precision, x.Scale, !x.IsNegative, x.Data);
		}	
	}
}