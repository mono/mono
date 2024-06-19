using System.Configuration.Provider;
using System.Xml;

namespace System.Configuration
{
  public abstract class ConfigurationBuilder : ProviderBase
  {
    public virtual XmlNode ProcessRawXml(XmlNode rawXml)
    {
      return rawXml;
    }

    public virtual ConfigurationSection ProcessConfigurationSection(ConfigurationSection configSection)
    {
      return configSection;
    }
  }
}