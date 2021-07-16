namespace System.Configuration
{
  public class ConfigurationBuilderSettings : ConfigurationElement
  {
    private readonly ConfigurationProperty _propBuilders = new ConfigurationProperty((string) null, typeof (ProviderSettingsCollection), (object) null, ConfigurationPropertyOptions.IsDefaultCollection);
    private ConfigurationPropertyCollection _properties;

    public ConfigurationBuilderSettings()
    {
      this._properties = new ConfigurationPropertyCollection();
      this._properties.Add(this._propBuilders);
    }

    protected internal override ConfigurationPropertyCollection Properties
    {
      get
      {
        return this._properties;
      }
    }

    [ConfigurationProperty("", IsDefaultCollection = true, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
    public ProviderSettingsCollection Builders
    {
      get
      {
        return (ProviderSettingsCollection) this[this._propBuilders];
      }
    }
  }
}