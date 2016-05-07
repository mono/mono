//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public sealed partial class NetPipeSection : ConfigurationSection
    {
        PropertyInformationCollection propertyInfo;
        
        public NetPipeSection()
            : base()
        {
            propertyInfo = this.ElementInformation.Properties;
        }

        [ConfigurationProperty(ConfigurationStrings.AllowAccounts)]
        public SecurityIdentifierElementCollection AllowAccounts
        {
            get { return (SecurityIdentifierElementCollection)base[ConfigurationStrings.AllowAccounts]; }
        }

        static internal NetPipeSection GetSection()
        {
            NetPipeSection retval = (NetPipeSection)ConfigurationManager.GetSection(ConfigurationStrings.NetPipeSectionPath);
            if (retval == null)
            {
                retval = new NetPipeSection();
            }
            return retval;
        }

        protected override void InitializeDefault()
        {
            this.AllowAccounts.SetDefaultIdentifiers();
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
    }
}
