using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Security
{
    public abstract class SecureConversationVersion
    {
        readonly XmlDictionaryString scNamespace;
        readonly XmlDictionaryString prefix;
        
        internal SecureConversationVersion(XmlDictionaryString ns, XmlDictionaryString prefix)
        {
            this.scNamespace = ns;
            this.prefix = prefix;
        }

        public XmlDictionaryString Namespace
        {
            get
            {
                return this.scNamespace;
            }
        }

        public XmlDictionaryString Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public static SecureConversationVersion Default
        {
            get { return WSSecureConversationFeb2005; }
        }

        public static SecureConversationVersion WSSecureConversationFeb2005
        {
            get { return WSSecureConversationVersionFeb2005.Instance; }
        }

        public static SecureConversationVersion WSSecureConversation13
        {
            get { return WSSecureConversationVersion13.Instance; }
        }

        class WSSecureConversationVersionFeb2005 : SecureConversationVersion
        {
            static readonly WSSecureConversationVersionFeb2005 instance = new WSSecureConversationVersionFeb2005();

            protected WSSecureConversationVersionFeb2005()
                : base(XD.SecureConversationFeb2005Dictionary.Namespace, XD.SecureConversationFeb2005Dictionary.Prefix)
            {
            }

            public static SecureConversationVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }

        class WSSecureConversationVersion13 : SecureConversationVersion
        {
            static readonly WSSecureConversationVersion13 instance = new WSSecureConversationVersion13();

            protected WSSecureConversationVersion13()
                : base(DXD.SecureConversationDec2005Dictionary.Namespace, DXD.SecureConversationDec2005Dictionary.Prefix)
            {
            }

            public static SecureConversationVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }

    }
}
