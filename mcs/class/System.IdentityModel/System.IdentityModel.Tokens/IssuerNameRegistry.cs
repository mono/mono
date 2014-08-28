using System.IdentityModel.Configuration;
using System.Xml;

namespace System.IdentityModel.Tokens
{
	public abstract class IssuerNameRegistry : ICustomIdentityConfiguration
	{
		public abstract string GetIssuerName (SecurityToken securityToken);

		[MonoTODO]
		public virtual string GetIssuerName (SecurityToken securityToken, System.String requestedIssuerName) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetWindowsIssuerName() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void LoadCustomConfiguration(XmlNodeList nodelist) {
			throw new NotImplementedException ();
		}
	}
}