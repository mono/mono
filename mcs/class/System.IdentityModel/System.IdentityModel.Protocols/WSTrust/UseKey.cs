using System;
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class UseKey
	{
		public SecurityKeyIdentifier SecurityKeyIdentifier { get; private set; }
		public SecurityToken Token { get; private set; }

		public UseKey () : this (null, null)
		{ }

		public UseKey (SecurityKeyIdentifier ski)
			: this (ski, null)
		{ }

		public UseKey (SecurityToken token)
			: this (null, token)
		{ }

		public UseKey (SecurityKeyIdentifier ski, SecurityToken token) {
			SecurityKeyIdentifier = ski;
			Token = token;
		}
	}
}