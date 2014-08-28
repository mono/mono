using System.Xml;

namespace System.IdentityModel.Configuration
{
	public interface ICustomIdentityConfiguration
	{
		void LoadCustomConfiguration(XmlNodeList nodeList);
	}
}