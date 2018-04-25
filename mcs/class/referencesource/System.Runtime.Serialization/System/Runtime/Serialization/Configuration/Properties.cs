//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

// This code was produced by a tool, ConfigPropertyGenerator.exe, by reflecting over
// System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089.
// Please add this file to the project that built the assembly.
// Doing so will provide better performance for retrieving the ConfigurationElement Properties.
// If compilation errors occur, make sure that the Properties property has not
// already been provided. If it has, decide if you want the version produced by 
// this tool or by the developer.
// If build errors result, make sure the config class is marked with the partial keyword.

// To regenerate a new Properties.cs after changes to the configuration OM for
// this assembly, simply run Indigo\Suites\Configuration\Infrastructure\ConfigPropertyGenerator.
// If any changes affect this file, the suite will fail.  Instructions on how to
// update Properties.cs will be included in the tests output file (ConfigPropertyGenerator.out).

using System.Configuration;
using System.Globalization;


// configType.Name: DeclaredTypeElement

namespace System.Runtime.Serialization.Configuration
{
    public sealed partial class DeclaredTypeElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.Runtime.Serialization.Configuration.TypeElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    properties.Add(new ConfigurationProperty("type", typeof(System.String), string.Empty, null, new System.Runtime.Serialization.Configuration.DeclaredTypeValidator(), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetDataContractSerializerSection

namespace System.Runtime.Serialization.Configuration
{
    public sealed partial class NetDataContractSerializerSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("enableUnsafeTypeForwarding", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ParameterElement

namespace System.Runtime.Serialization.Configuration
{
    public sealed partial class ParameterElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("index", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("", typeof(System.Runtime.Serialization.Configuration.ParameterElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    properties.Add(new ConfigurationProperty("type", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: DataContractSerializerSection

namespace System.Runtime.Serialization.Configuration
{
    public sealed partial class DataContractSerializerSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("declaredTypes", typeof(System.Runtime.Serialization.Configuration.DeclaredTypeElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TypeElement

namespace System.Runtime.Serialization.Configuration
{
    public sealed partial class TypeElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.Runtime.Serialization.Configuration.ParameterElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    properties.Add(new ConfigurationProperty("type", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("index", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

