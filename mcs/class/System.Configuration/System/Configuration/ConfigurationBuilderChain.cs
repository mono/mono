using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;

namespace System.Configuration
{
  internal class ConfigurationBuilderChain : ConfigurationBuilder
  {
    private List<ConfigurationBuilder> _builders;

    public List<ConfigurationBuilder> Builders
    {
      get
      {
        return this._builders;
      }
    }

    public override void Initialize(string name, NameValueCollection config)
    {
      this._builders = new List<ConfigurationBuilder>();
      base.Initialize(name, config);
    }

    public override XmlNode ProcessRawXml(XmlNode rawXml)
    {
      XmlNode rawXml1 = rawXml;
      foreach (ConfigurationBuilder builder in this._builders)
        rawXml1 = builder.ProcessRawXml(rawXml1);
      return rawXml1;
    }

    public override ConfigurationSection ProcessConfigurationSection(ConfigurationSection configSection)
    {
      ConfigurationSection configSection1 = configSection;
      foreach (ConfigurationBuilder builder in this._builders)
        configSection1 = builder.ProcessConfigurationSection(configSection1);
      return configSection1;
    }
  }
}
