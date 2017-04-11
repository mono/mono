// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// PublisherIdentityPermission.cs
// 
// <OWNER>Microsoft</OWNER>
// 

namespace System.Security.Permissions
{
    using System;
    using SecurityElement = System.Security.SecurityElement;
    using X509Certificate = System.Security.Cryptography.X509Certificates.X509Certificate;
    using System.Security.Util;
    using System.IO;
    using System.Collections;
    using System.Globalization;

[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    sealed public class PublisherIdentityPermission : CodeAccessPermission, IBuiltInPermission
    {
        //------------------------------------------------------
        //
        // PRIVATE STATE DATA
        //
        //------------------------------------------------------

        private bool m_unrestricted;
        private X509Certificate[] m_certs;

        //------------------------------------------------------
        //
        // PUBLIC CONSTRUCTORS
        //
        //------------------------------------------------------


        public PublisherIdentityPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                m_unrestricted = true;
            }
            else if (state == PermissionState.None)
            {
                m_unrestricted = false;
            }
            else
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
            }
        }

        public PublisherIdentityPermission( X509Certificate certificate )
        {
            Certificate = certificate;
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
                CheckCertificate(value);
                m_unrestricted = false;
                m_certs = new X509Certificate[1];
                m_certs[0] = new X509Certificate(value);
            }

            get
            {
                if(m_certs == null || m_certs.Length < 1)
                    return null;
                if(m_certs.Length > 1)
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_AmbiguousIdentity"));
                if(m_certs[0] == null)
                    return null;
                return new X509Certificate(m_certs[0]);
            }
        }

        //------------------------------------------------------
        //
        // PRIVATE AND PROTECTED HELPERS FOR ACCESSORS AND CONSTRUCTORS
        //
        //------------------------------------------------------

        private static void CheckCertificate( X509Certificate certificate )
        {
            if (certificate == null)
            {
                throw new ArgumentNullException( "certificate" );
            }
            if (certificate.GetRawCertData() == null) {
                throw new ArgumentException(Environment.GetResourceString("Argument_UninitializedCertificate"));
            }
        }

        //------------------------------------------------------
        //
        // CODEACCESSPERMISSION IMPLEMENTATION
        //
        //------------------------------------------------------

        //------------------------------------------------------
        //
        // IPERMISSION IMPLEMENTATION
        //
        //------------------------------------------------------


        public override IPermission Copy()
        {
            PublisherIdentityPermission perm = new PublisherIdentityPermission(PermissionState.None);
            perm.m_unrestricted = m_unrestricted;
            if(this.m_certs != null)
            {
                perm.m_certs = new X509Certificate[this.m_certs.Length];
                int n;
                for(n = 0; n < this.m_certs.Length; n++)
                    perm.m_certs[n] = (m_certs[n] == null ? null : new X509Certificate(m_certs[n]));
            }
            return perm;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                if(m_unrestricted)
                    return false;
                if(m_certs == null)
                    return true;
                if(m_certs.Length == 0)
                    return true;
                return false;
            }
            PublisherIdentityPermission that = target as PublisherIdentityPermission;
            if(that == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", this.GetType().FullName));
            if(that.m_unrestricted)
                return true;
            if(m_unrestricted)
                return false;
            if(this.m_certs != null)
            {
                foreach(X509Certificate certThis in this.m_certs)
                {
                    bool bOK = false;
                    if(that.m_certs != null)
                    {
                        foreach(X509Certificate certThat in that.m_certs)
                        {
                            if(certThis.Equals(certThat))
                            {
                                bOK = true;
                                break;
                            }
                        }
                    }
                    if(!bOK)
                        return false;           
                }
            }
            return true;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
                return null;
            PublisherIdentityPermission that = target as PublisherIdentityPermission;
            if(that == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", this.GetType().FullName));
            if(this.m_unrestricted && that.m_unrestricted)
            {
                PublisherIdentityPermission res = new PublisherIdentityPermission(PermissionState.None);
                res.m_unrestricted = true;
                return res;
            }
            if(this.m_unrestricted)
                return that.Copy();
            if(that.m_unrestricted)
                return this.Copy();
            if(this.m_certs == null || that.m_certs == null || this.m_certs.Length == 0 || that.m_certs.Length == 0)
                return null;
            ArrayList alCerts = new ArrayList();
            foreach(X509Certificate certThis in this.m_certs)
            {
                foreach(X509Certificate certThat in that.m_certs)
                {
                    if(certThis.Equals(certThat))
                        alCerts.Add(new X509Certificate(certThis));
                }
            }
            if(alCerts.Count == 0)
                return null;
            PublisherIdentityPermission result = new PublisherIdentityPermission(PermissionState.None);
            result.m_certs = (X509Certificate[])alCerts.ToArray(typeof(X509Certificate));
            return result;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                if((this.m_certs == null || this.m_certs.Length == 0) && !this.m_unrestricted)
                    return null;
                return this.Copy();
            }
            PublisherIdentityPermission that = target as PublisherIdentityPermission;
            if(that == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", this.GetType().FullName));
            if(this.m_unrestricted || that.m_unrestricted)
            {
                PublisherIdentityPermission res = new PublisherIdentityPermission(PermissionState.None);
                res.m_unrestricted = true;
                return res;
            }
            if (this.m_certs == null || this.m_certs.Length == 0)
            {
                if(that.m_certs == null || that.m_certs.Length == 0)
                    return null;
                return that.Copy();
            }
            if(that.m_certs == null || that.m_certs.Length == 0)
                return this.Copy();
            ArrayList alCerts = new ArrayList();
            foreach(X509Certificate certThis in this.m_certs)
                alCerts.Add(certThis);
            foreach(X509Certificate certThat in that.m_certs)
            {
                bool bDupe = false;
                foreach(X509Certificate cert in alCerts)
                {
                    if(certThat.Equals(cert))
                    {
                        bDupe = true;
                        break;
                    }
                }
                if(!bDupe)
                    alCerts.Add(certThat);
            }
            PublisherIdentityPermission result = new PublisherIdentityPermission(PermissionState.None);
            result.m_certs = (X509Certificate[])alCerts.ToArray(typeof(X509Certificate));
            return result;
        }

#if FEATURE_CAS_POLICY
        public override void FromXml(SecurityElement esd)
        {
            m_unrestricted = false;
            m_certs = null;
            CodeAccessPermission.ValidateElement( esd, this );
            String unr = esd.Attribute( "Unrestricted" );
            if(unr != null && String.Compare(unr, "true", StringComparison.OrdinalIgnoreCase) == 0)
            {
                m_unrestricted = true;
                return;
            }
            String elem = esd.Attribute( "X509v3Certificate" );
            ArrayList al = new ArrayList();
            if(elem != null)
                al.Add(new X509Certificate(System.Security.Util.Hex.DecodeHexString(elem)));
            ArrayList alChildren = esd.Children;
            if(alChildren != null)
            {
                foreach(SecurityElement child in alChildren)
                {
                    elem = child.Attribute( "X509v3Certificate" );
                    if(elem != null)
                        al.Add(new X509Certificate(System.Security.Util.Hex.DecodeHexString(elem)));
                }
            }
            if(al.Count != 0)
                m_certs = (X509Certificate[])al.ToArray(typeof(X509Certificate));
        }
        
        public override SecurityElement ToXml()
        {
            SecurityElement esd = CodeAccessPermission.CreatePermissionElement( this, "System.Security.Permissions.PublisherIdentityPermission" );
            if (m_unrestricted)
                esd.AddAttribute( "Unrestricted", "true" );
            else if (m_certs != null)
            {
                if (m_certs.Length == 1)
                    esd.AddAttribute( "X509v3Certificate", m_certs[0].GetRawCertDataString() );
                else
                {
                    int n;
                    for(n = 0; n < m_certs.Length; n++)
                    {
                        SecurityElement child = new SecurityElement("Cert");
                        child.AddAttribute( "X509v3Certificate", m_certs[n].GetRawCertDataString() );
                        esd.AddChild(child);
                    }
                }
            }
            return esd;
        }
#endif // FEATURE_CAS_POLICY

        /// <internalonly/>
        int IBuiltInPermission.GetTokenIndex()
        {
            return PublisherIdentityPermission.GetTokenIndex();
        }

        internal static int GetTokenIndex()
        {
            return BuiltInPermissionIndex.PublisherIdentityPermissionIndex;
        }

    }
}
