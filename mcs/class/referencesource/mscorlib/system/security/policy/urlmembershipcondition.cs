// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
//  UrlMembershipCondition.cs
//
//  Implementation of membership condition for urls
//

namespace System.Security.Policy {
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Security;
    using System.Security.Util;
    using System.Diagnostics.Contracts;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    sealed public class UrlMembershipCondition : IMembershipCondition, IConstantMembershipCondition, IReportMatchMembershipCondition
    {
        //------------------------------------------------------
        //
        // PRIVATE STATE DATA
        //
        //------------------------------------------------------

        private URLString m_url;
        private SecurityElement m_element;

        //------------------------------------------------------
        //
        // PUBLIC CONSTRUCTORS
        //
        //------------------------------------------------------

        internal UrlMembershipCondition()
        {
            m_url = null;
        }

        public UrlMembershipCondition( String url )
        {
            if (url == null)
                throw new ArgumentNullException( "url" );
            Contract.EndContractBlock();

            // Parse the Url to check that it's valid.
            m_url = new URLString(url, false /* not parsed */, true /* parse eagerly */);

            if (m_url.IsRelativeFileUrl)
                throw new ArgumentException(Environment.GetResourceString("Argument_RelativeUrlMembershipCondition"), "url");
        }

        //------------------------------------------------------
        //
        // PUBLIC ACCESSOR METHODS
        //
        //------------------------------------------------------

        public String Url
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                Contract.EndContractBlock();

                URLString url = new URLString(value);
                if (url.IsRelativeFileUrl)
                    throw new ArgumentException(Environment.GetResourceString("Argument_RelativeUrlMembershipCondition"), "value");

                m_url = url;
            }

            get
            {
                if (m_url == null && m_element != null)
                    ParseURL();

                return m_url.ToString();
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

            Url url = evidence.GetHostEvidence<Url>();
            if (url != null)
            {
                if (m_url == null && m_element != null)
                {
                    ParseURL();
                }

                if (url.GetURLString().IsSubsetOf(m_url))
                {
                    usedEvidence = url;
                    return true;
                }
            }

            return false;
        }

        public IMembershipCondition Copy()
        {
            if (m_url == null && m_element != null)
                ParseURL();

            UrlMembershipCondition mc = new UrlMembershipCondition();
            mc.m_url = new URLString( m_url.ToString() );
            return mc;
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
            if (m_url == null && m_element != null)
                ParseURL();

            SecurityElement root = new SecurityElement( "IMembershipCondition" );
            System.Security.Util.XMLUtil.AddClassAttribute( root, this.GetType(), "System.Security.Policy.UrlMembershipCondition" );
            // If you hit this assert then most likely you are trying to change the name of this class. 
            // This is ok as long as you change the hard coded string above and change the assert below.
            Contract.Assert( this.GetType().FullName.Equals( "System.Security.Policy.UrlMembershipCondition" ), "Class name changed!" );

            root.AddAttribute( "version", "1" );
            if (m_url != null)
                root.AddAttribute( "Url", m_url.ToString() );

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
                m_url = null;
            }
        }

        private void ParseURL()
        {
            lock (this)
            {
                if (m_element == null)
                    return;

                String elurl = m_element.Attribute( "Url" );
                if (elurl == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_UrlCannotBeNull"));
                }
                else
                {
                    URLString url = new URLString(elurl);
                    if (url.IsRelativeFileUrl)
                        throw new ArgumentException(Environment.GetResourceString("Argument_RelativeUrlMembershipCondition"));

                    m_url = url;
                }

                m_element = null;
            }
        }

        public override bool Equals( Object o )
        {
            UrlMembershipCondition that = (o as UrlMembershipCondition);

            if (that != null)
            {
                if (this.m_url == null && this.m_element != null)
                    this.ParseURL();
                if (that.m_url == null && that.m_element != null)
                    that.ParseURL();

                if (Equals( this.m_url, that.m_url ))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (m_url == null && m_element != null)
                ParseURL();

            if (m_url != null)
            {
                return m_url.GetHashCode();
            }
            else
            {
                return typeof( UrlMembershipCondition ).GetHashCode();
            }
        }

        public override String ToString()
        {
            if (m_url == null && m_element != null)
                ParseURL();

            if (m_url != null)
                return String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Url_ToStringArg" ), m_url.ToString() );
            else
                return Environment.GetResourceString( "Url_ToString" );
        }
    }
}
