using System;
using System.IdentityModel.Configuration;
using System.Xml;

namespace System.IdentityModel.Tokens
{
	public abstract class TokenReplayCache : ICustomIdentityConfiguration
	{
		public abstract void AddOrUpdate (string key, SecurityToken securityToken, DateTime expirationTime);

		public abstract bool Contains (string key);

		public abstract SecurityToken Get (string key);

		[MonoTODO]
		public virtual void LoadCustomConfiguration (XmlNodeList nodelist) {
			throw new NotImplementedException ();
		}

		public abstract void Remove (string key);
	}
}