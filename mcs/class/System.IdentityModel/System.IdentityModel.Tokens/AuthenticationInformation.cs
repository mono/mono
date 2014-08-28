using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;

namespace System.Security.Claims
{
	public class AuthenticationInformation
	{
		private Collection<AuthenticationContext> authorizationContexts = new Collection<AuthenticationContext> ();

		public String Address { get; set; }
		public Collection<AuthenticationContext> AuthorizationContexts { get { return authorizationContexts; } }
		public String DnsName { get; set; }
		public Nullable<DateTime> NotOnOrAfter { get; set; }
		public String Session { get; set; }
	}
}