using System;
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class ProtectedKey
	{
		private byte[] secret;

		public EncryptingCredentials WrappingCredentials { get; private set; }

		public ProtectedKey (byte[] secret) {
			this.secret = secret;
		}

		public ProtectedKey (byte[] secret, EncryptingCredentials wrappingCredentials) {
			this.secret = secret;
			WrappingCredentials = wrappingCredentials;
		}

		public byte[] GetKeyBytes () {
			return secret;
		}
	}
}