// cs0120.cs: error CS0120: An object reference is required for the non-static field `Account'
// Line: 28

using System;

namespace Babble.Protocol
{
	public class Query
	{
		public Query(Account a)
		{
		}
	}

	public class Account
	{
	}
	
	public class Auth
	{
		public Account Account
		{
			get { return null; }
		}
		
		private class AuthQuery : Query
		{
			public AuthQuery() : base(Account)
			{
			}
		}
	}
}
