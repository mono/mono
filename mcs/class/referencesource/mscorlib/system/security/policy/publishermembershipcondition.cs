// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  PublisherMembershipCondition.cs
// 
// <OWNER>Microsoft</OWNER>
//
//  Implementation of membership condition for X509 certificate based publishers
//

namespace System.Security.Policy {
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Policy;
    using System.Diagnostics.Contracts;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    sealed public class PublisherMembershipCondition : IMembershipCondition, IConstantMembershipCondition, IReportMatchMembershipCondition
    {
        //------------------------------------------------------
        //
        // PRIVATE STATE DATA
        //
        //------------------------------------------------------

        private X509Certificate m_certificate;
        private SecurityElement m_element;

        //------------------------------------------------------
        //
        // PUBLIC CONSTRUCTORS
        //
        //------------------------------------------------------

        internal PublisherMembershipCondition()
        {
            m_element = null;
            m_certificate = null;
        }

        public PublisherMembershipCondition( X509Certificate certificate )
        {
            CheckCertificate( certificate );
            m_certificate = new X509Certificate( certificate );
        }

        private static void CheckCertificate( X509Certificate certificate )
        {
            if (certificate == null)
            {
                throw new ArgumentNullException( "certificate" );
            }
            Contract.EndContractBlock();
        }

        //------------------------------------------------------
        //
        // PUBLIC ACCESSOR METHODS
        //
        //------------------------------------------------------

        public X509Certificate Certificate
        {
            set
            {
                CheckCertificate( value );
                m_certificate = new X509Certificate( value );
            }

            get
            {
                if (m_certificate == null && m_element != null)
                    ParseCertificate();

                if (m_certificate != null)
                    return new X509Certificate( m_certificate );
                else
                    return null;
            }
        }

        public override String ToString()
        {
            if (m_certificate == null && m_element != null)
                ParseCertificate();

            if (m_certificate == null)
                return Environment.GetResourceString( "Publisher_ToString" );
            else
            {
                String name = m_certificate.Subject;
                if (name != null)
                    return String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Publisher_ToStringArg" ), System.Security.Util.Hex.EncodeHexString( m_certificate.GetPublicKey() ) );
                else
                    return Environment.GetResourceString( "Publisher_ToString" );
            }
        }

        //------------------------------------------------------
        //
        // IMEMBERSHIPCONDITION IMPLEMENTATION
        //
        //------------------------------------------------------

        public bool Check( Evidence evidence )
        {
            object usedEvidence = null;
            return (this as IReportMatchMembershipCondition).Check(evidence, out usedEvidence);
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;

            if (evidence == null)
                return false;

            Publisher publisher = evidence.GetHostEvidence<Publisher>();
            if (publisher != null)
            {
                if (m_certificate == null && m_element != null)
                    ParseCertificate();

                // We can't just compare certs directly here because Publisher equality
                // depends only on the keys inside the certs.
                if (publisher.Equals(new Publisher(m_certificate)))
                {
                    usedEvidence = publisher;
                    return true;
                }
            }

            return false;
        }

        public IMembershipCondition Copy()
        {
            if (m_certificate == null && m_element != null)
                ParseCertificate();

            return new PublisherMembershipCondition( m_certificate );
        }

        public SecurityElement ToXml()
        {
            return ToXml( null );
        }

        public void FromXml( SecurityElement e )
        {
            FromXml( e, null );
        }

        public SecurityElement ToXml( PolicyLevel level )
        {
            if (m_certificate == null && m_element != null)
                ParseCertificate();

            SecurityElement root = new SecurityElement( "IMembershipCondition" );
            System.Security.Util.XMLUtil.AddClassAttribute( root, this.GetType(), "System.Security.Policy.PublisherMembershipCondition" );
            // If you hit this assert then most likely you are trying to change the name of this class. 
            // This is ok as long as you change the hard coded string above and change the assert below.
            Contract.Assert( this.GetType().FullName.Equals( "System.Security.Policy.PublisherMembershipCondition" ), "Class name changed!" );

            root.AddAttribute( "version", "1" );
            if (m_certificate != null)
                root.AddAttribute( "X509Certificate", m_certificate.GetRawCertDataString() );

            return root;
        }

        public void FromXml( SecurityElement e, PolicyLevel level )
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (!e.Tag.Equals( "IMembershipCondition" ))
            {
                throw new ArgumentException( Environment.GetResourceString( "Argument_MembershipConditionElement" ) );
            }
            Contract.EndContractBlock();

            lock (this)
            {
                m_element = e;
                m_certificate = null;
            }
        }

        private void ParseCertificate()
        {
            lock (this)
            {
                if (m_element == null)
                    return;

                String elCert = m_element.Attribute( "X509Certificate" );
                m_certificate = elCert == null ? null : new X509Certificate( System.Security.Util.Hex.DecodeHexString( elCert ) );
                CheckCertificate( m_certificate );
                m_element = null;
            }
        }

        public override bool Equals( Object o )
        {
            PublisherMembershipCondition that = (o as PublisherMembershipCondition);
            if (that != null)
            {
                if (this.m_certificate == null && this.m_element != null)
                    this.ParseCertificate();
                if (that.m_certificate == null && that.m_element != null)
                    that.ParseCertificate();

                if ( Publisher.PublicKeyEquals( this.m_certificate, that.m_certificate ))
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (m_certificate == null && m_element != null)
                ParseCertificate();

            if (m_certificate != null)
                return m_certificate.GetHashCode();
            else
                return typeof( PublisherMembershipCondition ).GetHashCode();
        }
    }
}
