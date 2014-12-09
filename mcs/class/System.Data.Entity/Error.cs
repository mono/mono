namespace System.Data.Entity
{
	static class Error
	{
		public static Exception ArgumentNull (string paramName)
		{
			return new ArgumentNullException (paramName);
		}

		public static Exception ArgumentOutOfRange (string paramName)
		{
			return new ArgumentOutOfRangeException (paramName);
		}

		public static Exception NotImplemented ()
		{
			return new NotImplementedException ();
		}

		public static Exception NotSupported ()
		{
			return new NotSupportedException ();
		}
	}
}