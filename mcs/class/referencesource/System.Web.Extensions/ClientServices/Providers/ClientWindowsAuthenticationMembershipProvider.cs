//------------------------------------------------------------------------------
// <copyright file="ClientWindowsAuthenticationMembershipProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices.Providers
{
    using System;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;
    using System.Windows.Forms;
    using System.Web;
    using System.Web.Resources;
    using System.Web.Security;
    using System.Threading;
    using System.Security;
    using System.Security.Principal;
    using System.Collections.Specialized;
    using System.Web.ClientServices;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;


    public class ClientWindowsAuthenticationMembershipProvider : MembershipProvider
    {
        public override bool ValidateUser(string username, string password)
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            if (!string.IsNullOrEmpty(password))
                throw new ArgumentException(AtlasWeb.ArgumentMustBeNull, "password");
            if (!string.IsNullOrEmpty(username) && string.Compare(username, id.Name, StringComparison.OrdinalIgnoreCase) != 0)
                throw new ArgumentException(AtlasWeb.ArgumentMustBeNull, "username");

            Thread.CurrentPrincipal = new ClientRolePrincipal(id);
            return true;
        }
        public void Logout()
        {
            Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        }

        public override bool EnablePasswordRetrieval { get { return false; } }
        public override bool EnablePasswordReset { get { return false; } }
        public override bool RequiresQuestionAndAnswer { get { return false; } }
        public override string ApplicationName { get { return ""; } set { } }
        public override int MaxInvalidPasswordAttempts { get { return int.MaxValue; } }
        public override int PasswordAttemptWindow { get { return int.MaxValue; } }
        public override bool RequiresUniqueEmail { get { return false; } }
        public override MembershipPasswordFormat PasswordFormat { get { return MembershipPasswordFormat.Hashed; } }
        public override int MinRequiredPasswordLength { get { return 1; } }
        public override int MinRequiredNonAlphanumericCharacters { get { return 0; } }
        public override string PasswordStrengthRegularExpression { get { return "*"; } }


        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
                                                   bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotSupportedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotSupportedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotSupportedException();
        }


        public override string ResetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotSupportedException();
        }


        public override bool UnlockUser(string username)
        {
            throw new NotSupportedException();
        }


        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotSupportedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotSupportedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotSupportedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotSupportedException();
        }


        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }


        public override int GetNumberOfUsersOnline()
        {
            throw new NotSupportedException();
        }


        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

    }
}
