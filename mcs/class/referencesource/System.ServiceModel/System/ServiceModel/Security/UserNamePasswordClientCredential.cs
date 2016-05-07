//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ServiceModel;

    public sealed class UserNamePasswordClientCredential
    {
        string userName;
        string password;
        bool isReadOnly;

        internal UserNamePasswordClientCredential()
        {
            // empty
        }

        internal UserNamePasswordClientCredential(UserNamePasswordClientCredential other)
        {
            this.userName = other.userName;
            this.password = other.password;
            this.isReadOnly = other.isReadOnly;
        }

        public string UserName
        {
            get 
            {
                return this.userName;
            }
            set
            {
                ThrowIfImmutable();
                this.userName = value;
            }
        }

        public string Password
        {
            get 
            {
                return this.password;
            }
            set
            {
                ThrowIfImmutable();
                this.password = value;
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
