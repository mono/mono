//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Configuration;
    using System.Collections.Generic;
    using System.Globalization;

    public sealed partial class MessageLoggingElement : ConfigurationElement
    {
        // These three constructors are used by the configuration system. 
        public MessageLoggingElement() : base()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.LogEntireMessage, DefaultValue = false)]
        public bool LogEntireMessage
        {
            get { return (bool)base[ConfigurationStrings.LogEntireMessage]; }
            set { base[ConfigurationStrings.LogEntireMessage] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.LogKnownPii, DefaultValue = false)]
        public bool LogKnownPii
        {
            get { return (bool)base[ConfigurationStrings.LogKnownPii]; }
            set { base[ConfigurationStrings.LogKnownPii] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.LogMalformedMessages, DefaultValue = false)]
        public bool LogMalformedMessages
        {
            get { return (bool)base[ConfigurationStrings.LogMalformedMessages]; }
            set { base[ConfigurationStrings.LogMalformedMessages] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.LogMessagesAtServiceLevel, DefaultValue = false)]
        public bool LogMessagesAtServiceLevel
        {
            get { return (bool)base[ConfigurationStrings.LogMessagesAtServiceLevel]; }
            set { base[ConfigurationStrings.LogMessagesAtServiceLevel] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.LogMessagesAtTransportLevel, DefaultValue = false)]
        public bool LogMessagesAtTransportLevel
        {
            get { return (bool)base[ConfigurationStrings.LogMessagesAtTransportLevel]; }
            set { base[ConfigurationStrings.LogMessagesAtTransportLevel] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxMessagesToLog, DefaultValue = 10000)]
        [IntegerValidator(MinValue = -1)]
        public int MaxMessagesToLog
        {
            get { return (int)base[ConfigurationStrings.MaxMessagesToLog]; }
            set { base[ConfigurationStrings.MaxMessagesToLog] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxSizeOfMessageToLog, DefaultValue = 262144)]
        [IntegerValidator(MinValue = -1)]
        public int MaxSizeOfMessageToLog
        {
            get { return (int)base[ConfigurationStrings.MaxSizeOfMessageToLog]; }
            set { base[ConfigurationStrings.MaxSizeOfMessageToLog] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Filters, DefaultValue  = null)]
        public XPathMessageFilterElementCollection Filters
        {
            get { return (XPathMessageFilterElementCollection)base[ConfigurationStrings.Filters]; }
        }
    }
}



