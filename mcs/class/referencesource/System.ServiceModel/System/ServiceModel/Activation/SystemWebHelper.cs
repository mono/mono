//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Web.Security;

    // Note: The helper provides utility in safe access of System.Web methods for both client and extended SKUs.
    static class SystemWebHelper
    {
        static Type typeOfRoles;
        static Type typeOfMembership;
        static Type typeOfWebContext;
        static bool defaultRoleProviderSet;
        static RoleProvider defaultRoleProvider;

        static Type TypeOfRoles
        {
            get
            {
                if (typeOfRoles == null)
                {
                    typeOfRoles = GetSystemWebType("System.Web.Security.Roles");
                }
                return typeOfRoles;
            }
        }

        static Type TypeOfMembership
        {
            get
            {
                if (typeOfMembership == null)
                {
                    typeOfMembership = GetSystemWebType("System.Web.Security.Membership");
                }
                return typeOfMembership;
            }
        }

        static Type TypeOfWebContext
        {
            get
            {
                if (typeOfWebContext == null)
                {
                    typeOfWebContext = GetSystemWebType("System.Web.Configuration.WebContext");
                }
                return typeOfWebContext;
            }
        }

        static Type GetSystemWebType(string typeName)
        {
#pragma warning disable 436
            return Type.GetType(typeName + ", " + AssemblyRef.SystemWeb, false);
#pragma warning restore 436
        }

        // Invoke for System.Web.Security.Roles.Enabled ? System.Web.Security.Roles.Provider : null
        internal static RoleProvider GetDefaultRoleProvider()
        {
            if (defaultRoleProviderSet)
            {
                return defaultRoleProvider;
            }

            Type roleType = TypeOfRoles;
            RoleProvider result = null;
            if (roleType != null)
            {
                // Running on extended sku
                try
                {
                    PropertyInfo rolesEnabledPropertyInfo = roleType.GetProperty("Enabled");
                    Fx.Assert(rolesEnabledPropertyInfo != null, "rolesEnabledPropertyInfo must not be null!");

                    if (((bool)rolesEnabledPropertyInfo.GetValue(null, null)) == true)
                    {
                        PropertyInfo rolesProviderPropertyInfo = roleType.GetProperty("Provider");
                        Fx.Assert(rolesProviderPropertyInfo != null, "rolesProviderPropertyInfo must not be null!");
                        result = rolesProviderPropertyInfo.GetValue(null, null) as RoleProvider;
                    }
                }
                catch (TargetInvocationException exception)
                {
                    // Since reflection invoke always throws TargetInvocationException,
                    // we need to (best effort) maintain the exception contract by rethrow inner exception.
                    if (exception.InnerException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                    }
                    throw;
                }
            }
            else
            {
                // Running in client SKU
                // This is consistent with no provider
            }

            defaultRoleProvider = result;
            defaultRoleProviderSet = true;

            return result;
        }

        // Invoke for System.Web.Security.Roles.Providers[roleProviderName]
        internal static RoleProvider GetRoleProvider(string roleProviderName)
        {
            Type roleType = TypeOfRoles;
            if (roleType != null)
            {
                try
                {
                    // Running on extended sku
                    PropertyInfo roleProvidersPropertyInfo = roleType.GetProperty("Providers");
                    Fx.Assert(roleProvidersPropertyInfo != null, "roleProvidersPropertyInfo must not be null!");

                    // This could throw if RoleManager is not enabled.
                    object roleProviderCollection = roleProvidersPropertyInfo.GetValue(null, null);
                    Fx.Assert(roleProviderCollection != null, "roleProviderCollection must not be null!");

                    PropertyInfo itemPropertyInfo = roleProviderCollection.GetType().GetProperty("Item", new Type[] { typeof(string) });
                    Fx.Assert(itemPropertyInfo != null, "itemPropertyInfo must not be null!");
                    return (RoleProvider)itemPropertyInfo.GetValue(roleProviderCollection, new object[] { roleProviderName });
                }
                catch (TargetInvocationException exception)
                {
                    // Since reflection invoke always throws TargetInvocationException,
                    // we need to (best effort) maintain the exception contract by rethrow inner exception.
                    if (exception.InnerException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                    }
                    throw;
                }
            }
            // Running in client SKU
            // This is consistent with no provider is a given name.
            return null;
        }

        // Invoke for System.Web.Security.Membership.Provider
        internal static MembershipProvider GetMembershipProvider()
        {
            Type membershipType = TypeOfMembership;
            if (membershipType != null)
            {
                try
                {
                    PropertyInfo membershipProviderPropertyInfo = membershipType.GetProperty("Provider");
                    Fx.Assert(membershipProviderPropertyInfo != null, "membershipProviderPropertyInfo must not be null!");
                    return (MembershipProvider)membershipProviderPropertyInfo.GetValue(null, null);
                }
                catch (TargetInvocationException exception)
                {
                    // Since reflection invoke always throws TargetInvocationException,
                    // we need to (best effort) maintain the exception contract by rethrow inner exception.
                    if (exception.InnerException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                    }
                    throw;
                }
            }
            // Running in client SKU
            // This is consistent with no default provider.
            return null;
        }

        // Invoke for System.Web.Security.Membership.Providers[membershipProviderName]
        internal static MembershipProvider GetMembershipProvider(string membershipProviderName)
        {
            Type membershipType = TypeOfMembership;
            if (membershipType != null)
            {
                try
                {
                    PropertyInfo membershipProvidersPropertyInfo = membershipType.GetProperty("Providers");
                    Fx.Assert(membershipProvidersPropertyInfo != null, "membershipProvidersPropertyInfo must not be null!");

                    object membershipProviderCollection = membershipProvidersPropertyInfo.GetValue(null, null);
                    Fx.Assert(membershipProviderCollection != null, "membershipProviderCollection must not be null!");

                    PropertyInfo itemPropertyInfo = membershipProviderCollection.GetType().GetProperty("Item", new Type[] { typeof(string) });
                    Fx.Assert(itemPropertyInfo != null, "itemPropertyInfo must not be null!");
                    return (MembershipProvider)itemPropertyInfo.GetValue(membershipProviderCollection, new object[] { membershipProviderName });
                }
                catch (TargetInvocationException exception)
                {
                    // Since reflection invoke always throws TargetInvocationException,
                    // we need to (best effort) maintain the exception contract by rethrow inner exception.
                    if (exception.InnerException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                    }
                    throw;
                }
            }
            // Running in client SKU
            // This is consistent with no provider is a given name.
            return null;
        }

        // check if ((System.Web.Configuration.WebContext)configHostingContext).ApplicationLevel == WebApplicationLevel.AboveApplication
        internal static bool IsWebConfigAboveApplication(object configHostingContext)
        {
            Type webContextType = TypeOfWebContext;
            if (configHostingContext == null
                || webContextType == null
                || configHostingContext.GetType() != webContextType)
            {
                // if we don't recognize the context we can't enforce the special web.config logic
                return false;
            }

            const int webApplicationLevelAboveApplication = 10; // public value of the enum
            Fx.Assert(GetSystemWebType("System.Web.Configuration.WebApplicationLevel") != null, "Type 'System.Web.Configuration.WebApplicationLevel' MUST exist in System.Web.dll.");
            Fx.Assert(GetSystemWebType("System.Web.Configuration.WebApplicationLevel").GetProperty("AboveApplication") == null ||
                (int)GetSystemWebType("System.Web.Configuration.WebApplicationLevel").GetProperty("AboveApplication").GetValue(null, null) == webApplicationLevelAboveApplication, 
                "unexpected property value");
            try
            {
                PropertyInfo applicationLevelPropertyInfo = webContextType.GetProperty("ApplicationLevel");
                Fx.Assert(applicationLevelPropertyInfo != null, "applicationLevelPropertyInfo must not be null!");
                return (int)applicationLevelPropertyInfo.GetValue(configHostingContext, null) == webApplicationLevelAboveApplication;
            }
            catch (TargetInvocationException exception)
            {
                // Since reflection invoke always throws TargetInvocationException,
                // we need to (best effort) maintain the exception contract by rethrow inner exception.
                if (exception.InnerException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                }
                throw;
            }
        }
    }
}
