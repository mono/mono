//------------------------------------------------------------------------------
// <copyright file="MembershipCreateStatus.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class MembershipCreateUserException : Exception {

        public MembershipCreateUserException(MembershipCreateStatus statusCode)
            : base(GetMessageFromStatusCode(statusCode)) {
            _StatusCode = statusCode;
        }

        public MembershipCreateUserException(String message)
            : base(message) { }


        protected MembershipCreateUserException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            _StatusCode = (MembershipCreateStatus)info.GetInt32("_StatusCode");
        }

        public MembershipCreateUserException() { }

        public MembershipCreateUserException(String message, Exception innerException)
            : base(message, innerException) { }

        private MembershipCreateStatus _StatusCode = MembershipCreateStatus.ProviderError;

        public MembershipCreateStatus StatusCode { get { return _StatusCode; } }

        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        // This is a Level 1 transparency assembly, so we can't use [SecurityCritical] directly as this public member
        // will still be safe-critical. However, the [PermissionSet] above provides equivalent link-time protection
        // as [SecurityCritical] does in a Level 2 transparency assembly.
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("_StatusCode", _StatusCode);
        }

        internal static string GetMessageFromStatusCode(MembershipCreateStatus statusCode) {
            switch (statusCode) {
                case MembershipCreateStatus.Success:
                    return ApplicationServicesStrings.Membership_no_error;

                case MembershipCreateStatus.InvalidUserName:
                    return ApplicationServicesStrings.Membership_InvalidUserName;
                    
                case MembershipCreateStatus.InvalidPassword:
                    return ApplicationServicesStrings.Membership_InvalidPassword;
                    
                case MembershipCreateStatus.InvalidQuestion:
                    return ApplicationServicesStrings.Membership_InvalidQuestion;
                    
                case MembershipCreateStatus.InvalidAnswer:
                    return ApplicationServicesStrings.Membership_InvalidAnswer;
                    
                case MembershipCreateStatus.InvalidEmail:
                    return ApplicationServicesStrings.Membership_InvalidEmail;
                    
                case MembershipCreateStatus.InvalidProviderUserKey:
                    return ApplicationServicesStrings.Membership_InvalidProviderUserKey;
                    
                case MembershipCreateStatus.DuplicateUserName:
                    return ApplicationServicesStrings.Membership_DuplicateUserName;
                    
                case MembershipCreateStatus.DuplicateEmail:
                    return ApplicationServicesStrings.Membership_DuplicateEmail;
                    
                case MembershipCreateStatus.DuplicateProviderUserKey:
                    return ApplicationServicesStrings.Membership_DuplicateProviderUserKey;
                    
                case MembershipCreateStatus.UserRejected:
                    return ApplicationServicesStrings.Membership_UserRejected;                    
            }

            return ApplicationServicesStrings.Provider_Error;
        }
    }
}
