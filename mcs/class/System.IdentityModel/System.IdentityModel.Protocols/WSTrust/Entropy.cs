using System;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class Entropy : ProtectedKey
	{
		public Entropy (ProtectedKey protectedKey) : base (protectedKey.GetKeyBytes (), protectedKey.WrappingCredentials)
		{ }

		public Entropy (byte[] secret) : base (secret)
		{ }

		public Entropy (int entropySizeInBits)
			: this(Entropy.GetRandomByteArray(entropySizeInBits / 8))
		{ }

		public Entropy (byte[] secret, EncryptingCredentials wrappingCredentials) : base (secret, wrappingCredentials)
		{ }

		private static byte[] GetRandomByteArray (int arraySize) {
			byte[] b = new byte[arraySize];

			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetNonZeroBytes(b);

			return b;
		}
	}
}