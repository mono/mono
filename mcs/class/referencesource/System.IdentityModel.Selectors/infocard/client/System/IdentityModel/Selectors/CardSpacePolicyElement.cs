//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;
    using System.Xml;

    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;


    //
    // Summary:
    //  This is the managed representation of the native POLICY_ELEMENT struct.
    //
    public class CardSpacePolicyElement
    {
        XmlElement m_target;
        XmlElement m_issuer;
        Collection<XmlElement> m_parameters;
        Uri m_policyNoticeLink;
        int m_policyNoticeVersion;
        bool m_isManagedIssuer;

        public bool IsManagedIssuer
        {
            get { return m_isManagedIssuer; }
            set { m_isManagedIssuer = value; }
        }

        public XmlElement Target
        {
            get { return m_target; }
            set { m_target = value; }
        }

        public XmlElement Issuer
        {
            get { return m_issuer; }
            set { m_issuer = value; }
        }

        public Collection<XmlElement> Parameters
        {
            get { return m_parameters; }
            //            set { m_parameters = value; }
        }

        public Uri PolicyNoticeLink
        {
            get { return m_policyNoticeLink; }
            set { m_policyNoticeLink = value; }
        }

        public int PolicyNoticeVersion
        {
            get { return m_policyNoticeVersion; }
            set { m_policyNoticeVersion = value; }
        }

        //
        // Parameters:
        //  target     - The target of the token being described.
        //  parameters - describes the type of token required by the target.
        //
        public CardSpacePolicyElement(XmlElement target, XmlElement issuer, Collection<XmlElement> parameters, Uri privacyNoticeLink, int privacyNoticeVersion, bool isManagedIssuer)
        {
            //
            // Ensure that if a version is specified( value != 0 ), that a valid url is specified.
            //
            IDT.ThrowInvalidArgumentConditional(0 == privacyNoticeVersion && null != privacyNoticeLink, "privacyNoticeVersion");
            IDT.ThrowInvalidArgumentConditional(0 != privacyNoticeVersion && null == privacyNoticeLink, "privacyNoticeLink");

            m_target = target;
            m_issuer = issuer;
            m_parameters = parameters;
            m_policyNoticeLink = privacyNoticeLink;
            m_policyNoticeVersion = privacyNoticeVersion;
            m_isManagedIssuer = isManagedIssuer;
        }
    }
}
