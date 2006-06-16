// cs0120-5.cs: `Babble.Protocol.Auth.Account': An object reference is required for the nonstatic field, method or property
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
