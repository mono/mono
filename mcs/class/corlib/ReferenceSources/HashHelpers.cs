namespace System.Collections.Concurrent
{
	static class HashHelpers
	{
		// Reference source has broken ConcurrentDictionary code which depends
		// on #if FEATURE_RANDOMIZED_STRING_HASHING this is a workaround not to require it
		public static object GetEqualityComparerForSerialization (object comparer)
		{
			return comparer;
		}
	}
}
