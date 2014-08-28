using System.IdentityModel.Protocols.WSTrust;

namespace System.IdentityModel.Tokens
{
	public abstract class ProofDescriptor
	{
		public abstract SecurityKeyIdentifier KeyIdentifier { get; }

		public abstract void ApplyTo (RequestSecurityTokenResponse response);
	}
}