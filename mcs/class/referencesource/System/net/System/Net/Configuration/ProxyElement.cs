//------------------------------------------------------------------------------
// <copyright file="ProxyElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Reflection;
    using System.ComponentModel;
    using System.Security.Permissions;

    public sealed class ProxyElement : ConfigurationElement
    {
        public enum BypassOnLocalValues
        {
            Unspecified = -1,
            False       =  0,
            True        =  1,
        }

        public enum UseSystemDefaultValues
        {
            Unspecified = -1,
            False       =  0,
            True        =  1,
        }

        public enum AutoDetectValues
        {
            Unspecified = -1,
            False       =  0,
            True        =  1,
        }

        public ProxyElement()
        {
            this.properties.Add(this.autoDetect);
            this.properties.Add(this.scriptLocation);
            this.properties.Add(this.bypassonlocal);
            this.properties.Add(this.proxyaddress);
            this.properties.Add(this.usesystemdefault);
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get 
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.AutoDetect, DefaultValue=AutoDetectValues.Unspecified)]
        public AutoDetectValues AutoDetect
        {
            get { return (AutoDetectValues)this[this.autoDetect]; }
            set { this[this.autoDetect] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ScriptLocation)]
        public Uri ScriptLocation
        {
            get { return (Uri)this[this.scriptLocation]; }
            set { this[this.scriptLocation] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.BypassOnLocal, DefaultValue=(BypassOnLocalValues) BypassOnLocalValues.Unspecified)]
        public BypassOnLocalValues BypassOnLocal
        {
            get { return (BypassOnLocalValues) this[this.bypassonlocal]; }
            set { this[this.bypassonlocal] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ProxyAddress)]
        public Uri ProxyAddress
        {
            get { return (Uri) this[this.proxyaddress]; }
            set { this[this.proxyaddress] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseSystemDefault, DefaultValue=(UseSystemDefaultValues) UseSystemDefaultValues.Unspecified)]
        public UseSystemDefaultValues UseSystemDefault
        {
            get { return (UseSystemDefaultValues)this[this.usesystemdefault]; }
            set { this[this.usesystemdefault] = value; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty autoDetect =
            new ConfigurationProperty(ConfigurationStrings.AutoDetect, 
                                      typeof(AutoDetectValues), 
                                      AutoDetectValues.Unspecified,
                                      new EnumConverter(typeof(AutoDetectValues)), 
                                      null, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty scriptLocation =
            new ConfigurationProperty(ConfigurationStrings.ScriptLocation, 
                                      typeof(Uri), 
                                      null, 
                                      new UriTypeConverter(UriKind.Absolute), 
                                      null, 
                                      ConfigurationPropertyOptions.None);

        // Supply a type converter, even though it's a plain type converter, to get around ConfigurationProperty's internal
        // Enum conversion routine.  The internal one is case-sensitive, we want this to be case-insensitive.
        readonly ConfigurationProperty bypassonlocal =
            new ConfigurationProperty(ConfigurationStrings.BypassOnLocal, 
                                      typeof(BypassOnLocalValues), 
                                      BypassOnLocalValues.Unspecified,
                                      new EnumConverter(typeof(BypassOnLocalValues)), 
                                      null, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty proxyaddress =
            new ConfigurationProperty(ConfigurationStrings.ProxyAddress, 
                                      typeof(Uri), 
                                      null, 
                                      new UriTypeConverter(UriKind.Absolute), 
                                      null, 
                                      ConfigurationPropertyOptions.None);

        readonly ConfigurationProperty usesystemdefault =
            new ConfigurationProperty(ConfigurationStrings.UseSystemDefault, 
                                      typeof(UseSystemDefaultValues), 
                                      UseSystemDefaultValues.Unspecified,
                                      new EnumConverter(typeof(UseSystemDefaultValues)), 
                                      null, 
                                      ConfigurationPropertyOptions.None);
    }
}

