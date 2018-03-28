//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;

using System.ServiceModel;
using System.Xml;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Globalization;
using System.Net;


namespace System.ServiceModel.Security.Tokens
{
    public sealed class InitiatorServiceModelSecurityTokenRequirement : ServiceModelSecurityTokenRequirement 
    {
        WebHeaderCollection webHeaderCollection;

        public InitiatorServiceModelSecurityTokenRequirement()
            : base()
        {
            Properties.Add(IsInitiatorProperty, (object)true);
        }

        public EndpointAddress TargetAddress
        {
            get
            {
                return GetPropertyOrDefault<EndpointAddress>(TargetAddressProperty, null);
            }
            set
            {
                this.Properties[TargetAddressProperty] = value;
            }
        }

        public Uri Via
        {
            get
            {
                return GetPropertyOrDefault<Uri>(ViaProperty, null);
            }
            set
            {
                this.Properties[ViaProperty] = value;
            }
        }

        internal bool IsOutOfBandToken
        {
            get
            {
                return GetPropertyOrDefault<bool>(IsOutOfBandTokenProperty, false);
            }
            set
            {
                this.Properties[IsOutOfBandTokenProperty] = value;
            }
        }

        internal bool PreferSslCertificateAuthenticator
        {
            get
            {
                return GetPropertyOrDefault<bool>(PreferSslCertificateAuthenticatorProperty, false);
            }
            set
            {
                this.Properties[PreferSslCertificateAuthenticatorProperty] = value;
            }
        }

        internal WebHeaderCollection WebHeaders
        {
            get
            {
                return this.webHeaderCollection;
            }
            set
            {
                this.webHeaderCollection = value;
            }
        }

        public override string ToString()
        {
            return InternalToString();
        }
    }
}
