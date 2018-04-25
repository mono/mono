//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    public sealed class WindowsServiceCredential
    {
        bool allowAnonymousLogons = SspiSecurityTokenProvider.DefaultAllowUnauthenticatedCallers;
        bool includeWindowsGroups = SspiSecurityTokenProvider.DefaultExtractWindowsGroupClaims;
        bool isReadOnly;

        internal WindowsServiceCredential()
        {
            // empty
        }

        internal WindowsServiceCredential(WindowsServiceCredential other)
        {
            this.allowAnonymousLogons = other.allowAnonymousLogons;
            this.includeWindowsGroups = other.includeWindowsGroups;
            this.isReadOnly = other.isReadOnly;
        }

        public bool AllowAnonymousLogons 
        {
            get
            {
                return this.allowAnonymousLogons;
            }
            set
            {
                ThrowIfImmutable();
                this.allowAnonymousLogons = value;
            }
        }

        public bool IncludeWindowsGroups 
        {
            get
            {
                return this.includeWindowsGroups;
            }
            set
            {
                ThrowIfImmutable();
                this.includeWindowsGroups = value;
            }
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }
    }
}
