//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    public class ClaimTypeRequirement
    {
        internal const bool DefaultIsOptional = false;
        string claimType;
        bool isOptional;

        public ClaimTypeRequirement(string claimType)
            : this(claimType, DefaultIsOptional)
        {
        }

        public ClaimTypeRequirement(string claimType, bool isOptional)
        {
            if (claimType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimType");
            }
            if (claimType.Length <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("claimType", SR.GetString(SR.ClaimTypeCannotBeEmpty));
            }

            this.claimType = claimType;
            this.isOptional = isOptional;
        }

        public string ClaimType 
        {
            get { return this.claimType; }
        }

        public bool IsOptional
        {
            get { return this.isOptional; }
        }
    }
}
