using System;
using System.IdentityModel.Tokens;
using System.Xml;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class RequestedSecurityToken
	{
		public SecurityToken SecurityToken { get; private set; }
		public virtual XmlElement SecurityTokenXml { get; private set; }

		public RequestedSecurityToken (SecurityToken token) {
			SecurityToken = token;
		}

		public RequestedSecurityToken (XmlElement tokenAsXml) {
			SecurityTokenXml = tokenAsXml;
		}
	}
}