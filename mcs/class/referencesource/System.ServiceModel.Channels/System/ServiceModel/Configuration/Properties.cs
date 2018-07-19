//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

// This code was produced by a tool, ConfigPropertyGenerator.exe, by reflecting over
// System.ServiceModel.Channels, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35.
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


// configType.Name: UdpBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class UdpBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("duplicateMessageHistoryLength", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxRetransmitCount", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingMessagesTotalSize", typeof(System.Int64), (long)0, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("multicastInterfaceId", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("textEncoding", typeof(System.Text.Encoding), "utf-8", new System.ServiceModel.Configuration.EncodingConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("timeToLive", typeof(System.Int32), 1, null, new System.Configuration.IntegerValidator(0, 255, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ByteStreamMessageEncodingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ByteStreamMessageEncodingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: SoapUdpTransportElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class UdpTransportElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("duplicateMessageHistoryLength", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingMessagesTotalSize", typeof(System.Int64), (long)0, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("multicastInterfaceId", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("socketReceiveBufferSize", typeof(System.Int32), 65536, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("timeToLive", typeof(System.Int32), 1, null, new System.Configuration.IntegerValidator(0, 255, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("retransmissionSettings", typeof(System.ServiceModel.Configuration.UdpRetransmissionSettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: UdpRetransmissionSettingsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class UdpRetransmissionSettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("delayLowerBound", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:00.050", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("delayUpperBound", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:00.250", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxDelayPerRetransmission", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:00.500", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxMulticastRetransmitCount", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxUnicastRetransmitCount", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

