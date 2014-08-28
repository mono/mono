using System;
using System.Xml;

namespace System.IdentityModel.Tokens
{
	public class SessionSecurityTokenCacheKey
	{
		[MonoTODO]
		public static bool operator !=(SessionSecurityTokenCacheKey first, SessionSecurityTokenCacheKey second) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool operator ==(SessionSecurityTokenCacheKey first, SessionSecurityTokenCacheKey second) {
			throw new NotImplementedException ();
		}

		public UniqueId ContextId { get; private set; }
		public string EndpointId { get; private set; }
		public bool IgnoreKeyGeneration { get; set; }
		public UniqueId KeyGeneration { get; private set; }

		public SessionSecurityTokenCacheKey (string endpointId, UniqueId contextId, UniqueId keyGeneration) {
			EndpointId = endpointId;
			ContextId = contextId;
			KeyGeneration = keyGeneration;
		}

		[MonoTODO]
		public override bool Equals (System.Object obj) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString () {
			throw new NotImplementedException ();
		}
	}
}