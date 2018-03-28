//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
//


namespace System.ServiceModel.Security
{
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Security.Tokens;


    internal class InfoCardChannelParameter
    {
        SecurityToken m_token;
        Uri m_relyingPartyIssuer;
        bool m_requiresInfocard;

        public SecurityToken Token
        {
            get
            {
                return m_token;
            }
        }

        public Uri RelyingPartyIssuer
        {
            get
            {
                return m_relyingPartyIssuer;
            }
        }

        public bool RequiresInfoCard
        {
            get
            {
                return m_requiresInfocard;
            }
        }

        public InfoCardChannelParameter(SecurityToken token, Uri relyingIssuer, bool requiresInfoCard)
        {
            m_token = token;
            m_relyingPartyIssuer = relyingIssuer;
            m_requiresInfocard = requiresInfoCard;
        }

    }
}
