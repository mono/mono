using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Xml;

[ComVisible(false)]
public interface IInternalConfigurationBuilderHost {
    
    ConfigurationSection ProcessConfigurationSection(
        ConfigurationSection configSection, ConfigurationBuilder builder);

    XmlNode ProcessRawXml(XmlNode rawXml, ConfigurationBuilder builder);
}