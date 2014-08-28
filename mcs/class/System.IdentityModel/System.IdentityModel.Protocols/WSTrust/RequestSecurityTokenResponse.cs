using System.IdentityModel.Tokens;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class RequestSecurityTokenResponse : WSTrustMessage
	{
		public bool IsFinal { get; set; }
		public SecurityKeyIdentifierClause RequestedAttachedReference { get; set; }
		public RequestedProofToken RequestedProofToken { get; set; }
		public RequestedSecurityToken RequestedSecurityToken { get; set; }
		public bool RequestedTokenCancelled { get; set; }
		public SecurityKeyIdentifierClause RequestedUnattachedReference { get; set; }
		public Status Status { get; set; }

		public RequestSecurityTokenResponse ()
		{ }

		public RequestSecurityTokenResponse (WSTrustMessage message) {
			Context = message.Context;
			KeyType = message.KeyType;
			KeySizeInBits = message.KeySizeInBits;
			RequestType = message.RequestType;
		}
	}
}