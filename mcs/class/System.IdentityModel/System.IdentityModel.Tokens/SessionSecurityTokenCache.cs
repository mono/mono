using System;
using System.Collections.Generic;
using System.IdentityModel.Configuration;
using System.Xml;

namespace System.IdentityModel.Tokens
{
	public abstract class SessionSecurityTokenCache : ICustomIdentityConfiguration
	{
		public abstract void AddOrUpdate (SessionSecurityTokenCacheKey key, SessionSecurityToken value, DateTime expiryTime);
		public abstract SessionSecurityToken Get (SessionSecurityTokenCacheKey key);
		public abstract IEnumerable<SessionSecurityToken> GetAll (string endpointId, UniqueId contextId);
		[MonoTODO]
		public virtual void LoadCustomConfiguration (XmlNodeList nodelist) {
			throw new NotImplementedException ();
		}
		public abstract void Remove (SessionSecurityTokenCacheKey key);
		public abstract void RemoveAll (string endpointId);
		public abstract void RemoveAll (string endpointId, UniqueId contextId);
	}
}