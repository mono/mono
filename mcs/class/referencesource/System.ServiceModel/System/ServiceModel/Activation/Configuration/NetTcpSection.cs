//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public sealed partial class NetTcpSection : ConfigurationSection
    {
        PropertyInformationCollection propertyInfo;
        
        public NetTcpSection()
            : base()
        {
            propertyInfo = this.ElementInformation.Properties;
        }

        [ConfigurationProperty(ConfigurationStrings.AllowAccounts)]
        public SecurityIdentifierElementCollection AllowAccounts
        {
            get { return (SecurityIdentifierElementCollection)base[ConfigurationStrings.AllowAccounts]; }
        }

        static internal NetTcpSection GetSection()
        {
            NetTcpSection retval = (NetTcpSection)ConfigurationManager.GetSection(ConfigurationStrings.NetTcpSectionPath);
            if (retval == null)
            {
                retval = new NetTcpSection();
            }
            return retval;
        }

        protected override void InitializeDefault()
        {
            this.AllowAccounts.SetDefaultIdentifiers();
        }

        [ConfigurationProperty(ConfigurationStrings.ListenBacklog, DefaultValue = ListenerConstants.DefaultListenBacklog)]
        [IntegerValidator(MinValue = 0)]
        public int ListenBacklog
        {
            get 
            {
                int listenBacklog = (int)base[ConfigurationStrings.ListenBacklog];
                
                if (listenBacklog != ListenerConstants.DefaultListenBacklog)
                {
                    // if the user changed the default, return user's value
                    return listenBacklog;
                }
                else
                {
                    // otherwise return the transport default
                    return TcpTransportDefaults.GetListenBacklog();
                }
            }
            set { base[ConfigurationStrings.ListenBacklog] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxPendingConnections, DefaultValue = ListenerConstants.DefaultMaxPendingConnections)]
        [IntegerValidator(MinValue = 0)]
        public int MaxPendingConnections
        {
            get { return (int)base[ConfigurationStrings.MaxPendingConnections]; }
            set { base[ConfigurationStrings.MaxPendingConnections] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxPendingAccepts, DefaultValue = ListenerConstants.DefaultMaxPendingAccepts)]
        [IntegerValidator(MinValue = 0)]
        public int MaxPendingAccepts
        {
            get
            {
                int maxPendingAccepts = (int)base[ConfigurationStrings.MaxPendingAccepts];
                
                if (maxPendingAccepts != ListenerConstants.DefaultMaxPendingAccepts)
                {
                    // if the user changed the default, return user's value
                    return maxPendingAccepts;
                }
                else
                {
                    // otherwise return 2 * transport default, since SMSvcHost defaults are global
                    return 2 * ConnectionOrientedTransportDefaults.GetMaxPendingAccepts();
                }
            }
            set { base[ConfigurationStrings.MaxPendingAccepts] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReceiveTimeout, DefaultValue = ListenerConstants.DefaultReceiveTimeoutString)]
        [System.ComponentModel.TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [System.ServiceModel.Configuration.ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan ReceiveTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.ReceiveTimeout]; }
            set { base[ConfigurationStrings.ReceiveTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TeredoEnabled, DefaultValue = ListenerConstants.DefaultTeredoEnabled)]
        public bool TeredoEnabled
        {
            get { return (bool)base[ConfigurationStrings.TeredoEnabled]; }
            set { base[ConfigurationStrings.TeredoEnabled] = value; }
        }
    }
}
