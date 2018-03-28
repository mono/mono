//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Security;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Channels;


    public class PeerResolverSettings
    {
        PeerReferralPolicy referralPolicy;
        PeerResolverMode mode;
        PeerCustomResolverSettings customSettings;

        public PeerResolverSettings() { customSettings = new PeerCustomResolverSettings(); }
        public PeerResolverMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (!PeerResolverModeHelper.IsDefined(value))
                    PeerExceptionHelper.ThrowArgument_InvalidResolverMode(value);
                mode = value;
            }
        }

        public PeerReferralPolicy ReferralPolicy
        {
            get { return referralPolicy; }
            set
            {
                if (!PeerReferralPolicyHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value, typeof(PeerReferralPolicy)));
                }
                referralPolicy = value;
            }
        }

        public PeerCustomResolverSettings Custom
        {
            get
            {
                return customSettings;
            }
        }
    }
}
