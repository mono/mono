using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Claims;

namespace System.IdentityModel.Tokens
{
	public class SecurityTokenDescriptor
	{
		private Dictionary<string, Object> properties = new Dictionary<string, object> ();

		public string AppliesToAddress { get; set; }
		public SecurityKeyIdentifierClause AttachedReference { get; set; }
		public AuthenticationInformation AuthenticationInfo { get; set; }
		public EncryptingCredentials EncryptingCredentials { get; set; }
		public Lifetime Lifetime { get; set; }
		public ProofDescriptor Proof { get; set; }
		public Dictionary<string, Object> Properties { get { return properties; } }
		public string ReplyToAddress { get; set; }
		public SigningCredentials SigningCredentials { get; set; }
		public ClaimsIdentity Subject { get; set; }
		public SecurityToken Token { get; set; }
		public string TokenIssuerName { get; set; }
		public string TokenType { get; set; }
		public SecurityKeyIdentifierClause UnattachedReference { get; set; }

		[MonoTODO]
		public void AddAuthenticationClaims (string authType) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddAuthenticationClaims (string authType, DateTime time) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ApplyTo (RequestSecurityTokenResponse response) {
			throw new NotImplementedException ();
		}
	}
}