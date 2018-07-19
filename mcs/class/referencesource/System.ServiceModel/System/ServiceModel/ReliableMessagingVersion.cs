//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ComponentModel;
    using System.ServiceModel.Configuration;
    using System.Xml;

    [TypeConverter(typeof(ReliableMessagingVersionConverter))]
    public abstract class ReliableMessagingVersion
    {
        XmlDictionaryString dictionaryNs;
        string ns;

        // Do not initialize directly, this constructor is for derived classes.
        internal ReliableMessagingVersion(string ns, XmlDictionaryString dictionaryNs)
        {
            this.ns = ns;
            this.dictionaryNs = dictionaryNs;
        }

        public static ReliableMessagingVersion Default
        {
            get { return System.ServiceModel.Channels.ReliableSessionDefaults.ReliableMessagingVersion; }
        }

        public static ReliableMessagingVersion WSReliableMessaging11
        {
            get { return WSReliableMessaging11Version.Instance; }
        }

        public static ReliableMessagingVersion WSReliableMessagingFebruary2005
        {
            get { return WSReliableMessagingFebruary2005Version.Instance; }
        }

        internal XmlDictionaryString DictionaryNamespace
        {
            get { return this.dictionaryNs; }
        }

        internal string Namespace
        {
            get { return this.ns; }
        }

        internal static bool IsDefined(ReliableMessagingVersion reliableMessagingVersion)
        {
            return (reliableMessagingVersion == WSReliableMessaging11)
                || (reliableMessagingVersion == WSReliableMessagingFebruary2005);
        }
    }

    class WSReliableMessaging11Version : ReliableMessagingVersion
    {
        static ReliableMessagingVersion instance = new WSReliableMessaging11Version();

        WSReliableMessaging11Version()
            : base(Wsrm11Strings.Namespace, DXD.Wsrm11Dictionary.Namespace)
        {
        }

        internal static ReliableMessagingVersion Instance
        {
            get { return instance; }
        }

        public override string ToString()
        {
            return "WSReliableMessaging11";
        }
    }

    class WSReliableMessagingFebruary2005Version : ReliableMessagingVersion
    {
        WSReliableMessagingFebruary2005Version()
            : base(WsrmFeb2005Strings.Namespace, XD.WsrmFeb2005Dictionary.Namespace)
        {
        }

        static ReliableMessagingVersion instance = new WSReliableMessagingFebruary2005Version();

        internal static ReliableMessagingVersion Instance
        {
            get { return instance; }
        }

        public override string ToString()
        {
            return "WSReliableMessagingFebruary2005";
        }
    }
}
