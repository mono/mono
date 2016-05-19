//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System.IdentityModel.Claims;
    using System.ServiceModel;
    using System.IdentityModel.Policy;
    using System.Security.Principal;

    class WindowsSidIdentity : IIdentity
    {
        SecurityIdentifier sid;
        string name;
        string authenticationType;

        public WindowsSidIdentity(SecurityIdentifier sid)
        {
            if (sid == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sid");

            this.sid = sid;
            this.authenticationType = String.Empty;
        }

        public WindowsSidIdentity(SecurityIdentifier sid, string name, string authenticationType)
        {
            if (sid == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sid");
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            if (authenticationType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authenticationType");

            this.sid = sid;
            this.name = name;
            this.authenticationType = authenticationType;
        }

        public SecurityIdentifier SecurityIdentifier
        {
            get { return this.sid; }
        }

        public string AuthenticationType 
        {
            get { return this.authenticationType; }
        }
        
        public bool IsAuthenticated
        { 
            get { return true; } 
        }

        public string Name 
        {
            get
            {
                if (this.name == null)
                    this.name = ((NTAccount)this.sid.Translate(typeof(NTAccount))).Value;
                return this.name;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            WindowsSidIdentity sidIdentity = obj as WindowsSidIdentity;
            if (sidIdentity == null)
                return false;

            return this.sid == sidIdentity.SecurityIdentifier;
        }

        public override int GetHashCode()
        {
            return this.sid.GetHashCode();
        }
    }
}
