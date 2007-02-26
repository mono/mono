//
// Mainsoft.Web.Security.WPGroupsRoleProvider
//
// Authors:
//	Ilya Kharmatsky (ilyak@mainsoft.com)
//
// (C) 2007 Mainsoft
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Text;
using System.Configuration.Provider;

namespace Mainsoft.Web.Security
{
    public class WPGroupsRoleProvider : RoleProvider 
    {
        public static readonly string GROUP_NAMESPACE_ATTRIBUTE = "WPGroupsRoleNamespace";
        
        private static readonly string PROVIDER_NAME = "WPGroupsRoleProvider";
        private static readonly string DESCRIPTION = "WebSphere Groups Role provider";

        private string applicationName;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            base.Initialize(name, config);

            applicationName = config["applicationName"];
        }

        public override string Name
        {
            get
            {
                return PROVIDER_NAME;
            }
        }

        public override string Description
        {
            get
            {
                return DESCRIPTION;
            }
        }
        public override string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        #region NOT IMPLEMENTED METHODS

        [MonoTODO]
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        #endregion


        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            java.util.List groups = GetGroupsByRoleName(roleName);
            if (groups == null || groups.isEmpty())
                throw new ProviderException("The role name " + roleName + "doesn't exist");

            if (usernameToMatch == null || usernameToMatch == String.Empty)
                usernameToMatch = "*";

            IPumaServicesProvider provider = PumaServicesProviderFactory.CreateProvider();

            com.ibm.portal.um.Group group = (com.ibm.portal.um.Group)groups.get(0);

            java.util.List members = provider.PumaLocator.findMembersByGroup(group, false);
            java.util.List users = provider.PumaLocator.findUsersByAttribute("uid", usernameToMatch);

            java.util.HashSet ids = new java.util.HashSet();

            for (java.util.Iterator iter = members.iterator(); iter.hasNext(); )
                ids.add(((com.ibm.portal.um.Principal)iter.next()).getObjectID());


            java.util.ArrayList res = new java.util.ArrayList(ids.size());
            java.util.ArrayList userAttrib = new java.util.ArrayList();
            userAttrib.add("uid");

            for (java.util.Iterator iter = users.iterator(); iter.hasNext(); )
            {
                com.ibm.portal.um.User user = (com.ibm.portal.um.User)iter.next();
                if (ids.contains(user.getObjectID()))
                {
                    java.util.Map m = provider.PumaProfile.getAttributes(user, userAttrib);
                    res.add(m.get("uid"));
                }
            }

            return (string[])res.toArray(new string[res.size()]);

        }

        public override string[] GetAllRoles()
        {
            IPumaServicesProvider pumaServices = PumaServicesProviderFactory.CreateProvider();
            java.util.List groups = pumaServices.PumaLocator.findGroupsByDefaultAttribute("*");
            return GroupsToStringArray(pumaServices, groups);
        }


        public override string[] GetRolesForUser(string username)
        {
            if (username == null || username == String.Empty)
                throw new ProviderException("The username cannot be null or empty string");

            IPumaServicesProvider pumaServices = PumaServicesProviderFactory.CreateProvider();
            java.util.List users = pumaServices.PumaLocator.findUsersByAttribute("uid", username);

            if (users == null || users.isEmpty())
                return new string[0];

            com.ibm.portal.um.User user = (com.ibm.portal.um.User) users.get(0);

            java.util.List groups = pumaServices.PumaLocator.findGroupsByPrincipal(user, true);
            return GroupsToStringArray(pumaServices, groups);

        }

        public override string[] GetUsersInRole(string roleName)
        {
            if(roleName == null || roleName == String.Empty) 
                throw new ArgumentException("The roleName could not be an empty or null string", "roleName");

            IPumaServicesProvider provider = PumaServicesProviderFactory.CreateProvider();
            roleName = NormalizeRoleName(provider, roleName);

            java.util.List groups = provider.PumaLocator.findGroupsByAttribute("cn", roleName);
            if (groups == null || groups.isEmpty())
                throw new ProviderException("Cannot find role named: " + roleName);

            com.ibm.portal.um.Group group = (com.ibm.portal.um.Group) groups.get(0);
            java.util.List users = provider.PumaLocator.findMembersByGroup(group, true);

            java.util.ArrayList res = new java.util.ArrayList(users.size());
            java.util.ArrayList userAttribs = new java.util.ArrayList(1);
            userAttribs.add("uid");

            for (java.util.Iterator iter = users.iterator(); iter.hasNext(); )
            {
                com.ibm.portal.um.Principal ppp = (com.ibm.portal.um.Principal) iter.next();
                com.ibm.portal.um.User currentUser =  ppp as com.ibm.portal.um.User;
                if (currentUser == null) //sub group has been returned, instead of user
                    continue;

                java.util.Map dic = provider.PumaProfile.getAttributes(currentUser, userAttribs);
                res.add(dic.get("uid"));
            }

            return (string[])res.toArray(new string[res.size()]);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            if(username == null || username == String.Empty) 
                throw new ArgumentException("The username parameter could not be null or an empty string", "username");
            if(roleName == null || roleName == String.Empty)
                throw new ArgumentException("The roleName parameter could not be null or an empty string", "roleName");

            IPumaServicesProvider provider = PumaServicesProviderFactory.CreateProvider();
            java.util.List users = provider.PumaLocator.findUsersByAttribute("uid", username);
            if (users == null || users.isEmpty())
                return false;

            com.ibm.portal.um.User user = (com.ibm.portal.um.User)users.get(0);

            java.util.List groups = provider.PumaLocator.findGroupsByPrincipal(user, false);
            java.util.ArrayList groupAttribs = new java.util.ArrayList();
            groupAttribs.add("cn");

            for (java.util.Iterator iter = groups.iterator(); iter.hasNext(); )
            {
                com.ibm.portal.um.Group group = (com.ibm.portal.um.Group)iter.next();
                java.util.Map m = provider.PumaProfile.getAttributes(group, groupAttribs);
                string groupName = (string)m.get("cn");
                if (roleName.Equals(DeNormalizeRoleName(provider, groupName)))
                    return true;
            }
            return false;
        }

        public override bool RoleExists(string roleName)
        {
            java.util.List groups = GetGroupsByRoleName(roleName);
            if (groups == null || groups.isEmpty())
                return false;
            java.util.ArrayList attribute = new java.util.ArrayList(1);
            attribute.add("cn");

            IPumaServicesProvider provider = PumaServicesProviderFactory.CreateProvider();
            roleName = NormalizeRoleName(provider, roleName);

            for (java.util.Iterator iter = groups.iterator(); iter.hasNext(); )
            {
                com.ibm.portal.um.Group group = (com.ibm.portal.um.Group)iter.next();
                java.util.Map m = provider.PumaProfile.getAttributes(group, attribute);
                if (m.get("cn").Equals(roleName))
                    return true;
            }
            return false;
        }

        #region helper methods
        private java.util.List GetGroupsByRoleName(string roleName)
        {
            if (roleName == null || roleName == String.Empty)
                throw new ArgumentException("The role name cannot be empty or null string", "roleName");

            IPumaServicesProvider provider = PumaServicesProviderFactory.CreateProvider();

            return provider.PumaLocator.findGroupsByAttribute("cn", NormalizeRoleName(provider, roleName));
        }

        private string NormalizeRoleName(IPumaServicesProvider provider, string roleName)
        {
            string groupNamespace = provider.GetConfigAttribute(GROUP_NAMESPACE_ATTRIBUTE);
            if (groupNamespace == null)
                return roleName;

            return groupNamespace + roleName;
        }

        private string DeNormalizeRoleName(IPumaServicesProvider provider, string groupName)
        {
            string groupNamespace = provider.GetConfigAttribute(GROUP_NAMESPACE_ATTRIBUTE);
            if (groupNamespace == null)
                return groupName;
            if (groupName.StartsWith(groupNamespace))
                return groupName.Substring(groupNamespace.Length);
            else
                return groupName;
        }

        private static string[] GroupsToStringArray(IPumaServicesProvider pumaServices, java.util.List groups)
        {
            java.util.ArrayList res = new java.util.ArrayList(groups.size());
            string groupNamespace = pumaServices.GetConfigAttribute(GROUP_NAMESPACE_ATTRIBUTE);

            java.util.ArrayList groupAttribs = new java.util.ArrayList();
            groupAttribs.add("cn");

            for (java.util.Iterator iter = groups.iterator(); iter.hasNext(); )
            {
                java.util.Map dic = pumaServices.PumaProfile.getAttributes((com.ibm.portal.um.Group)iter.next(), groupAttribs);
                string currentGroup = (string)dic.get("cn");

                if (currentGroup != null && currentGroup != String.Empty)
                {
                    if (groupNamespace == null)
                        res.add(currentGroup);
                    else if (currentGroup.StartsWith(groupNamespace))
                        res.add(currentGroup.Substring(groupNamespace.Length));
                }
            }
            return (string[])res.toArray(new string[res.size()]);
        }
        #endregion
    }
}
#endif