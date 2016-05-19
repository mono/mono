//------------------------------------------------------------------------------
// <copyright file="ProfileProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
/*
 * ProfileProvider
 *
 * Copyright (c) 2002 Microsoft Corporation
 */
namespace System.Web.Profile
{
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Web.Security;
    using System.Web.Compilation;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Reflection;
    using System.CodeDom;

    public  abstract class ProfileProvider : SettingsProvider
    {
        public abstract int  DeleteProfiles (ProfileInfoCollection  profiles);
        public abstract int  DeleteProfiles (string[]               usernames);

        public abstract int  DeleteInactiveProfiles         (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate);
        public abstract int  GetNumberOfInactiveProfiles    (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate);

        public abstract ProfileInfoCollection GetAllProfiles                (ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords);
        public abstract ProfileInfoCollection GetAllInactiveProfiles        (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords);
        public abstract ProfileInfoCollection FindProfilesByUserName        (ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords);
        public abstract ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords);
    }

    public sealed class ProfileProviderCollection : SettingsProviderCollection
    {
        public override void Add(ProviderBase provider)
        {
            if( provider == null )
            {
                throw new ArgumentNullException( "provider" );
            }

            if( !( provider is ProfileProvider ) )
            {
                throw new ArgumentException(SR.GetString(SR.Provider_must_implement_type, typeof(ProfileProvider).ToString()), "provider");
            }

            base.Add( provider );
        }

        new public ProfileProvider this[string name]
        {
            get
            {
                return (ProfileProvider) base[name];
            }
        }
    }

}
