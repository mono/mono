//
// TransactionOptions.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//


namespace System.Transactions
{
	public struct TransactionOptions
	{

		IsolationLevel level;
		TimeSpan timeout;

		internal TransactionOptions (IsolationLevel level, TimeSpan timeout)
		{
			this.level = level;
			this.timeout = timeout;
		}

		public IsolationLevel IsolationLevel {
			get { return level; }
			set { level = value; }
		}

		public TimeSpan Timeout {
			get { return timeout; }
			set { timeout = value; }
		}

		public static bool operator == (TransactionOptions  x,
			TransactionOptions y)
		{
			return x.level == y.level &&
				x.timeout == y.timeout;
		}

		public static bool operator != (TransactionOptions x,
			TransactionOptions y)
		{
			return x.level != y.level ||
				x.timeout != y.timeout;
		}

		public override bool Equals (object obj)
		{
			if (! (obj is TransactionOptions))
				return false;
			return this == (TransactionOptions) obj;
		}

		public override int GetHashCode ()
		{
			return (int) level ^ timeout.GetHashCode ();
		}
	}
}

