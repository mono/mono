namespace System.Data.ProviderBase
{
	partial class FieldNameLookup
	{
		public int IndexOfName(string fieldName)
		{
			if (null == _fieldNameLookup)
			{
				GenerateLookup();
			}
			// via case sensitive search, first match with lowest ordinal matches
			object value = _fieldNameLookup[fieldName];
			return ((null != value) ? (int)value : -1);
		}
	}
}