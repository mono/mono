using System.Collections.Specialized;
using System.IO;
using System.Security.Permissions;

namespace System.Configuration
{
  public sealed class ConfigurationBuildersSection : ConfigurationSection
  {
    private static readonly ConfigurationProperty _propBuilders = new ConfigurationProperty("builders", typeof (ConfigurationBuilderSettings), (object) new ConfigurationBuilderSettings(), ConfigurationPropertyOptions.None);
    private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
    private const string _ignoreLoadFailuresSwitch = "ConfigurationBuilders.IgnoreLoadFailure";

    public ConfigurationBuilder GetBuilderFromName(string builderName)
    {
      string[] strArray = builderName.Split(',');
      bool flag = AppDomain.CurrentDomain.GetData("ConfigurationBuilders.IgnoreLoadFailure") == null;
      if (strArray.Length == 1)
      {
        ProviderSettings builder = this.Builders[builderName];
        if (builder == null)
          throw new Exception(SR.GetString("Config_builder_not_found", new object[1]
          {
            (object) builderName
          }));
        try
        {
          return this.InstantiateBuilder(builder);
        }
        catch (FileNotFoundException)
        {
          if (flag)
            throw;
        }
        catch (TypeLoadException)
        {
          if (!flag)
            return (ConfigurationBuilder) null;
          throw;
        }
      }
      ConfigurationBuilderChain configurationBuilderChain = new ConfigurationBuilderChain();
      configurationBuilderChain.Initialize(builderName, (NameValueCollection) null);
      foreach (string str in strArray)
      {
        ProviderSettings builder = this.Builders[str.Trim()];
        if (builder == null)
          throw new Exception(SR.GetString("Config_builder_not_found", new object[1]
          {
            (object) str
          }));
        try
        {
          configurationBuilderChain.Builders.Add(this.InstantiateBuilder(builder));
        }
        catch (FileNotFoundException)
        {
          if (flag)
            throw;
        }
        catch (TypeLoadException)
        {
          if (flag)
            throw;
        }
      }
      if (configurationBuilderChain.Builders.Count == 0)
        return (ConfigurationBuilder) null;
      return (ConfigurationBuilder) configurationBuilderChain;
    }

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    private ConfigurationBuilder CreateAndInitializeBuilderWithAssert(Type t, ProviderSettings ps)
    {
      ConfigurationBuilder reflectionPermission = (ConfigurationBuilder) TypeUtil.CreateInstanceWithReflectionPermission(t);
      NameValueCollection parameters = ps.Parameters;
      NameValueCollection config = new NameValueCollection(parameters.Count);
      foreach (string index in (NameObjectCollectionBase) parameters)
        config[index] = parameters[index];
      reflectionPermission.Initialize(ps.Name, config);
      return reflectionPermission;
    }

    private ConfigurationBuilder InstantiateBuilder(ProviderSettings ps)
    {
      Type reflectionPermission = TypeUtil.GetTypeWithReflectionPermission(ps.Type, true);
      if (!typeof (ConfigurationBuilder).IsAssignableFrom(reflectionPermission))
        throw new Exception(SR.GetString("WrongType_of_config_builder"));
      if (!TypeUtil.IsTypeAllowedInConfig(reflectionPermission))
        throw new Exception(SR.GetString("Type_from_untrusted_assembly", new object[1]
        {
          (object) reflectionPermission.FullName
        }));
      return this.CreateAndInitializeBuilderWithAssert(reflectionPermission, ps);
    }

    static ConfigurationBuildersSection()
    {
      ConfigurationBuildersSection._properties.Add(ConfigurationBuildersSection._propBuilders);
    }

    protected internal override ConfigurationPropertyCollection Properties
    {
      get
      {
        return ConfigurationBuildersSection._properties;
      }
    }

    private ConfigurationBuilderSettings _Builders
    {
      get
      {
        return (ConfigurationBuilderSettings) this[ConfigurationBuildersSection._propBuilders];
      }
    }

    [ConfigurationProperty("builders")]
    public ProviderSettingsCollection Builders
    {
      get
      {
        return this._Builders.Builders;
      }
    }
  }
}
