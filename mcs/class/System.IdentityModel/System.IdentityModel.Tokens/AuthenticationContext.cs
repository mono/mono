using System;
using System.Collections.ObjectModel;

namespace System.IdentityModel.Tokens
{
	public class AuthenticationContext
	{
		public Collection<string> Authorities { get; private set; }
		public string ContextClass { get; set; }
		public string ContextDeclaration { get; set; }

		public AuthenticationContext () {
			Authorities = new Collection<string> ();
		}
	}
}