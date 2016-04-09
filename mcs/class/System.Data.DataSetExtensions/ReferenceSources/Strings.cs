namespace System.Data.DataSetExtensions
{
	static partial class Strings
	{
		const string DataSetLinq_InvalidEnumerationValue_ = "The {0} enumeration value, {1}, is not valid.";
		public const string DataSetLinq_EmptyDataRowSource = "The source contains no DataRows.";
		public const string DataSetLinq_NullDataRow = "The source contains a DataRow reference that is null.";
		public const string DataSetLinq_CannotLoadDeletedRow = "The source contains a deleted DataRow that cannot be copied to the DataTable.";
		public const string DataSetLinq_CannotLoadDetachedRow = "The source contains a detached DataRow that cannot be copied to the DataTable.";
		public const string DataSetLinq_CannotCompareDeletedRow = "The DataRowComparer does not work with DataRows that have been deleted since it only compares current values.";
		const string DataSetLinq_NonNullableCast_ = "Cannot cast DBNull.Value to type '{0}'. Please use a nullable type.";
		public const string ToLDVUnsupported = "Can not create DataView after using projection";
		const string LDV_InvalidNumOfKeys_ = "Must provide '{0}' keys to find";

		public static string DataSetLinq_InvalidEnumerationValue (params object[] args)
		{
			return SR.GetString (DataSetLinq_InvalidEnumerationValue_, args);
		}

		public static string DataSetLinq_NonNullableCast (object arg)
		{
			return SR.GetString (DataSetLinq_NonNullableCast_, arg);
		}

		public static string LDV_InvalidNumOfKeys (params object[] args)
		{
			return SR.GetString (LDV_InvalidNumOfKeys_, args);
		}

		// Missing
		public const string LDVRowStateError = "LDVRowStateError";
	}
}