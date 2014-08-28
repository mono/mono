using System;
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class RequestedProofToken
	{
		public string ComputedKeyAlgorithm { get; private set; }
		public ProtectedKey ProtectedKey { get; private set; }

		public RequestedProofToken (ProtectedKey protectedKey) {
			ProtectedKey = protectedKey;
		}

		public RequestedProofToken (Byte[] secret) {
			ProtectedKey = new ProtectedKey (secret);
		}

		public RequestedProofToken (string computedKeyAlgorithm) {
			ComputedKeyAlgorithm = computedKeyAlgorithm;
		}

		public RequestedProofToken (Byte[] secret, EncryptingCredentials wrappingCredentials) {
			ProtectedKey = new ProtectedKey (secret, wrappingCredentials);
		}
	}
}