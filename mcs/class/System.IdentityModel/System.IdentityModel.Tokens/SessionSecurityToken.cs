using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Xml;

namespace System.IdentityModel.Tokens
{
	[Serializable]
	public class SessionSecurityToken : SecurityToken
	{
		private ReadOnlyCollection<SecurityKey> securityKeys;
		private DateTime validFrom;
		private DateTime validTo;

		public ClaimsPrincipal ClaimsPrincipal { get; private set; }
		public string Context { get; private set; }
		public UniqueId ContextId { get; private set; }
		public string EndpointId { get; private set; }
		[MonoTODO]
		public override string Id { get { throw new NotImplementedException (); } }
		public bool IsPersistent { get; set; }
		public bool IsReferenceMode { get; set; }
		public DateTime KeyEffectiveTime { get; private set; }
		public DateTime KeyExpirationTime { get; private set; }
		public UniqueId KeyGeneration { get; private set; }
		public Uri SecureConversationVersion { get; private set; }
		public override ReadOnlyCollection<SecurityKey> SecurityKeys { get { return securityKeys; } }
		public override DateTime ValidFrom { get { return validFrom; } }
		public override DateTime ValidTo { get { return validTo; } }

		public SessionSecurityToken (ClaimsPrincipal claimsPrincipal)
			: this (claimsPrincipal, null)
		{ }

		protected SessionSecurityToken (SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}

		public SessionSecurityToken (ClaimsPrincipal claimsPrincipal, string context)
			: this (claimsPrincipal, context, DateTime.UtcNow, DateTime.UtcNow + SessionSecurityTokenHandler.DefaultTokenLifetime)
		{ }

		public SessionSecurityToken (ClaimsPrincipal claimsPrincipal, TimeSpan lifetime)
			: this (claimsPrincipal, null, DateTime.UtcNow, DateTime.UtcNow + lifetime)
		{ }

		public SessionSecurityToken (ClaimsPrincipal claimsPrincipal, string context, DateTime? validFrom, DateTime? validTo)
			: this (claimsPrincipal, new UniqueId (), context, String.Empty, validFrom, validTo, null)
		{ }

		public SessionSecurityToken (ClaimsPrincipal claimsPrincipal, string context, string endpointId, DateTime? validFrom, DateTime? validTo)
			: this(claimsPrincipal, new UniqueId (), context, endpointId, validFrom, validTo, null)
		{ }

		public SessionSecurityToken (ClaimsPrincipal claimsPrincipal, UniqueId contextId, string context, string endpointId, TimeSpan lifetime, SymmetricSecurityKey key)
			: this (claimsPrincipal, contextId, context, endpointId, DateTime.UtcNow, lifetime, key)
		{ }

		public SessionSecurityToken (ClaimsPrincipal claimsPrincipal, UniqueId contextId, string context, string endpointId, DateTime validFrom, TimeSpan lifetime, SymmetricSecurityKey key)
			: this (claimsPrincipal, contextId, context, endpointId, validFrom, validFrom + lifetime, key)
		{ }

		public SessionSecurityToken (ClaimsPrincipal claimsPrincipal, UniqueId contextId, string context, string endpointId, DateTime? validFrom, DateTime? validTo, SymmetricSecurityKey key) {
			ClaimsPrincipal = claimsPrincipal;
			ContextId = contextId;
			Context = context;
			EndpointId = endpointId;
			validFrom = (validFrom.HasValue) ? validFrom.Value.ToUniversalTime () : DateTime.UtcNow;
			validTo = (validTo.HasValue) ? validTo.Value.ToUniversalTime () : ValidFrom + SessionSecurityTokenHandler.DefaultTokenLifetime;
			securityKeys = new ReadOnlyCollection<SecurityKey> (new SecurityKey[] { new InMemorySymmetricSecurityKey ((key == null) ? null : key.GetSymmetricKey ()) });
		}

		[MonoTODO]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}
	}
}