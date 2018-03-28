//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.ObjectModel;

    public class UserNameSecurityToken : SecurityToken
    {
        string id;
        string password;
        string userName;
        DateTime effectiveTime;

        public UserNameSecurityToken(string userName, string password)
            : this(userName, password, SecurityUniqueId.Create().Value)
        {
        }

        public UserNameSecurityToken(string userName, string password, string id)
        {   
            if (userName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("userName");
            if (userName == string.Empty)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.UserNameCannotBeEmpty));
            
            this.userName = userName;
            this.password = password;
            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
        }

        public override string Id
        {
            get { return this.id; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return EmptyReadOnlyCollection<SecurityKey>.Instance; }
        }

        public override DateTime ValidFrom
        {
            get { return this.effectiveTime; }
        }

        public override DateTime ValidTo
        {
            // Never expire
            get { return SecurityUtils.MaxUtcDateTime; }
        }

        public string UserName
        {
            get { return this.userName; }
        }

        public string Password
        {
            get { return this.password; }
        }
    }
}
