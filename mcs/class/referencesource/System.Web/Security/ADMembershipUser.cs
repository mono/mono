//------------------------------------------------------------------------------
// <copyright file="ADMembershipUser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using  System.Web;
    using  System.Web.Configuration;
    using  System.Security.Principal;
    using  System.Security.Permissions;
    using  System.Globalization;
    using  System.Runtime.Serialization;
    using  System.Diagnostics;

    [Serializable]
    public class ActiveDirectoryMembershipUser : MembershipUser
    {        

        internal bool emailModified = true;
        internal bool commentModified = true;
        internal bool isApprovedModified = true;

        //
        // private variables needed for the providerUserKey
        // (We need to store the provider user key here rather than the base class
        // to be able to do custom serialization)
        //
        private byte[] sidBinaryForm = null;
        [NonSerialized] 
        private SecurityIdentifier sid = null;
        
        public override DateTime LastLoginDate 
        { 
            get 
            { 
                throw new NotSupportedException(SR.GetString(SR.ADMembership_UserProperty_not_supported, "LastLoginDate"));
            } 
            set 
            { 
                throw new NotSupportedException(SR.GetString(SR.ADMembership_UserProperty_not_supported, "LastLoginDate"));
            }
        }

        public override DateTime LastActivityDate 
        { 
            get 
            { 
                throw new NotSupportedException(SR.GetString(SR.ADMembership_UserProperty_not_supported, "LastActivityDate"));
            } 
            set 
            { 
                throw new NotSupportedException(SR.GetString(SR.ADMembership_UserProperty_not_supported, "LastActivityDate"));
            }
        }

        public override string Email 
        {
            get 
            {
                return base.Email;
            } 
            set 
            { 
                base.Email = value;
                emailModified = true;
            } 
        }

        public override string Comment
        {
            get 
            { 
                return base.Comment;
            } 
            set 
            { 
                base.Comment = value;
                commentModified = true;
            } 
        }

        public override bool IsApproved 
        {
            get 
            { 
                return base.IsApproved;
            } 
            set 
            { 
                base.IsApproved = value;
                isApprovedModified = true;
            } 
        }

        public override object ProviderUserKey
        {
            get
            {
                if (sid == null && sidBinaryForm != null)
                    sid = new SecurityIdentifier(sidBinaryForm, 0);
                return sid;
            }
        }

        public ActiveDirectoryMembershipUser(string providerName,
                              string              name,
                              object             providerUserKey,
                              string              email,
                              string              passwordQuestion,
                              string              comment,
                              bool                isApproved,
                              bool                isLockedOut,
                              DateTime            creationDate,
                              DateTime            lastLoginDate,
                              DateTime            lastActivityDate,
                              DateTime            lastPasswordChangedDate,
                              DateTime            lastLockoutDate) 
            :base(providerName, 
                        name, 
                        null, 
                        email, 
                        passwordQuestion, 
                        comment, 
                        isApproved, 
                        isLockedOut,
                        creationDate, 
                        lastLoginDate, 
                        lastActivityDate,
                        lastPasswordChangedDate,
                        lastLockoutDate)  
        {
            if ((providerUserKey != null) && !(providerUserKey is SecurityIdentifier))
                throw new ArgumentException( SR.GetString(SR.ADMembership_InvalidProviderUserKey) , "providerUserKey" );

            sid = (SecurityIdentifier) providerUserKey;
            if (sid != null) 
            {
                // 
                // store the sid in binary form for serialization
                //
                sidBinaryForm = new byte[sid.BinaryLength];
                sid.GetBinaryForm(sidBinaryForm, 0);
            }
        }

        internal ActiveDirectoryMembershipUser(string providerName,
                              string              name,
                              byte[]             sidBinaryForm,
                              object             providerUserKey,
                              string              email,
                              string              passwordQuestion,
                              string              comment,
                              bool                isApproved,
                              bool                isLockedOut,
                              DateTime            creationDate,
                              DateTime            lastLoginDate,
                              DateTime            lastActivityDate,
                              DateTime            lastPasswordChangedDate,
                              DateTime            lastLockoutDate,
                              bool valuesAreUpdated) 
            :base(providerName, 
                        name, 
                        null, 
                        email, 
                        passwordQuestion, 
                        comment, 
                        isApproved, 
                        isLockedOut,
                        creationDate, 
                        lastLoginDate, 
                        lastActivityDate,
                        lastPasswordChangedDate,
                        lastLockoutDate) 
        {                

            if (valuesAreUpdated) 
            {
                emailModified = false;
                commentModified = false;
                isApprovedModified = false;
            }  

            Debug.Assert(sidBinaryForm != null);
            this.sidBinaryForm = sidBinaryForm;

            Debug.Assert((providerUserKey != null) && (providerUserKey is SecurityIdentifier));
            sid = (SecurityIdentifier) providerUserKey;
            
        }

        protected ActiveDirectoryMembershipUser() { } // Default CTor: Callable by derived class only.

    }
}



