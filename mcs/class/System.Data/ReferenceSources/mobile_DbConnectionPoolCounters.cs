namespace System.Data.ProviderBase
{
	class DbConnectionPoolCounters
	{
	}

	class DbConnectionPoolCountersNoCounters : DbConnectionPoolCounters
	{
		public static DbConnectionPoolCounters SingletonInstance { get; set; }
	}
}

