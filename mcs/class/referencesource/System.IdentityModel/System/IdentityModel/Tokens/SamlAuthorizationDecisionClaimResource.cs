//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    public class SamlAuthorizationDecisionClaimResource
    {
        [DataMember]
        string resource;

        [DataMember]
        SamlAccessDecision accessDecision;

        [DataMember]
        string actionNamespace;

        [DataMember]
        string actionName;

        [OnDeserialized]
        void OnDeserialized(StreamingContext ctx)
        {
            if (string.IsNullOrEmpty(resource))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("resource");
            if (string.IsNullOrEmpty(actionName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("actionName");
        }

        public SamlAuthorizationDecisionClaimResource(string resource, SamlAccessDecision accessDecision, string actionNamespace, string actionName)
        {
            if (string.IsNullOrEmpty(resource))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("resource");
            if (string.IsNullOrEmpty(actionName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("actionName");

            this.resource = resource;
            this.accessDecision = accessDecision;
            this.actionNamespace = actionNamespace;
            this.actionName = actionName;
        }

        public string Resource
        {
            get
            {
                return this.resource;
            }
        }

        public SamlAccessDecision AccessDecision
        {
            get
            {
                return this.accessDecision;
            }
        }

        public string ActionNamespace
        {
            get
            {
                return this.actionNamespace;
            }
        }

        public string ActionName
        {
            get
            {
                return this.actionName;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            SamlAuthorizationDecisionClaimResource rhs = obj as SamlAuthorizationDecisionClaimResource;
            if (rhs == null)
                return false;

            return ((this.ActionName == rhs.ActionName) && (this.ActionNamespace == rhs.ActionNamespace) &&
                (this.Resource == rhs.Resource) && (this.AccessDecision == rhs.AccessDecision));
        }

        public override int GetHashCode()
        {
            return (this.resource.GetHashCode() ^ this.accessDecision.GetHashCode());
        }
    }
}
