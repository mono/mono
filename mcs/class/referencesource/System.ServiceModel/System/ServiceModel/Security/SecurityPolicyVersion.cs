//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Security
{
    public abstract class SecurityPolicyVersion
    {
        readonly String spNamespace;
        readonly String prefix;

        internal SecurityPolicyVersion(String ns, String prefix)
        {
            this.spNamespace = ns;
            this.prefix = prefix;
        }

        public String Namespace
        {
            get
            {
                return this.spNamespace;
            }
        }

        public String Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public static SecurityPolicyVersion WSSecurityPolicy11
        {
            get { return WSSecurityPolicyVersion11.Instance; }
        }

        public static SecurityPolicyVersion WSSecurityPolicy12
        {
            get { return WSSecurityPolicyVersion12.Instance; }
        }

        class WSSecurityPolicyVersion11 : SecurityPolicyVersion
        {
            static readonly WSSecurityPolicyVersion11 instance = new WSSecurityPolicyVersion11();

            protected WSSecurityPolicyVersion11()
                : base(System.ServiceModel.Security.WSSecurityPolicy11.WsspNamespace, WSSecurityPolicy.WsspPrefix)
            {
            }

            public static SecurityPolicyVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }

        class WSSecurityPolicyVersion12 : SecurityPolicyVersion
        {
            static readonly WSSecurityPolicyVersion12 instance = new WSSecurityPolicyVersion12();

            protected WSSecurityPolicyVersion12()
                : base(System.ServiceModel.Security.WSSecurityPolicy12.WsspNamespace, WSSecurityPolicy.WsspPrefix)
            {
            }

            public static SecurityPolicyVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }

    }
}
