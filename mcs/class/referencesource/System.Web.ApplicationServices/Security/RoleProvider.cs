//------------------------------------------------------------------------------
// <copyright file="RoleProvider.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security {
   using  System.Web;
   using  System.Security.Principal;
   using  System.Security.Permissions;
   using  System.Globalization;
   using  System.Runtime.CompilerServices;
   using  System.Runtime.Serialization;
   using  System.Collections;
   using  System.Collections.Specialized;
   using  System.Configuration.Provider;
   using System.Diagnostics.CodeAnalysis;

   /// <devdoc>
   ///   <para>[To be supplied.]</para>
   /// </devdoc>
   [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
   public abstract class RoleProvider : ProviderBase
   {

       public abstract string ApplicationName { get; set; }


       [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
       public abstract bool IsUserInRole(string username, string roleName);

       [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
       public abstract string[] GetRolesForUser(string username);

       public abstract void CreateRole(string roleName);

       public abstract bool DeleteRole(string roleName, bool throwOnPopulatedRole);

       public abstract bool RoleExists(string roleName);

       [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "usernames", Justification="This version is required to maintain backwards binary compatibility")]
       public abstract void AddUsersToRoles(string[] usernames, string[] roleNames);

       [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "usernames", Justification="This version is required to maintain backwards binary compatibility")]
       public abstract void RemoveUsersFromRoles(string[] usernames, string[] roleNames);

       public abstract string[] GetUsersInRole(string roleName);

       public abstract string[] GetAllRoles();

       [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "username", Justification="This version is required to maintain backwards binary compatibility")]
       public abstract string[] FindUsersInRole(string roleName, string usernameToMatch);
   }
}
