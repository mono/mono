//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Security
{
    public abstract class TrustVersion
    {
        readonly XmlDictionaryString trustNamespace;
        readonly XmlDictionaryString prefix;

        internal TrustVersion(XmlDictionaryString ns, XmlDictionaryString prefix)
        {
            this.trustNamespace = ns;
            this.prefix = prefix;
        }

        public XmlDictionaryString Namespace
        {
            get
            {
                return this.trustNamespace;
            }
        }

        public XmlDictionaryString Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public static TrustVersion Default
        {
            get { return WSTrustFeb2005; }
        }

        public static TrustVersion WSTrustFeb2005
        {
            get { return WSTrustVersionFeb2005.Instance; }
        }

        public static TrustVersion WSTrust13
        {
            get { return WSTrustVersion13.Instance; }
        }

        class WSTrustVersionFeb2005 : TrustVersion
        {
            static readonly WSTrustVersionFeb2005 instance = new WSTrustVersionFeb2005();

            protected WSTrustVersionFeb2005()
                : base(XD.TrustFeb2005Dictionary.Namespace, XD.TrustFeb2005Dictionary.Prefix)
            {
            }

            public static TrustVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }

        class WSTrustVersion13 : TrustVersion
        {
            static readonly WSTrustVersion13 instance = new WSTrustVersion13();

            protected WSTrustVersion13()
                : base(DXD.TrustDec2005Dictionary.Namespace, DXD.TrustDec2005Dictionary.Prefix)
            {
            }

            public static TrustVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }

    }
}
