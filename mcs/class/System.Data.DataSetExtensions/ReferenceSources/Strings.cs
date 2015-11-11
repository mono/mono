namespace System.Data.DataSetExtensions
{
	static class Strings
	{
		public const string DataSetLinq_CannotCompareDeletedRow = "DataSetLinq_CannotCompareDeletedRow";
		public const string DataSetLinq_CannotLoadDeletedRow = "DataSetLinq_CannotLoadDeletedRow";
		public const string DataSetLinq_CannotLoadDetachedRow = "DataSetLinq_CannotLoadDetachedRow";
		public const string DataSetLinq_EmptyDataRowSource = "DataSetLinq_EmptyDataRowSource";
		public const string DataSetLinq_NullDataRow = "DataSetLinq_NullDataRow";
		public const string LDVRowStateError = "LDVRowStateError";
		public const string ToLDVUnsupported = "ToLDVUnsupported";

		public static string DataSetLinq_InvalidEnumerationValue (params object[] args)
		{
			return SR.GetString ("DataSetLinq_InvalidEnumerationValue", args);
		}

		public static string DataSetLinq_NonNullableCast (object arg)
		{
			return SR.GetString ("DataSetLinq_NonNullableCast", arg);
		}

		public static string LDV_InvalidNumOfKeys (params object[] args)
		{
			return SR.GetString ("LDV_InvalidNumOfKeys", args);
		}
	}
}