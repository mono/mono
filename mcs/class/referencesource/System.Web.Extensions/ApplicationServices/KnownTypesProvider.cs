//------------------------------------------------------------------------------
// <copyright file="KnownTypesProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Configuration;
using System.Reflection;
using System.Web.Profile;

namespace System.Web.ApplicationServices
{

    public static class KnownTypesProvider
    {
        public static Type[] GetKnownTypes(ICustomAttributeProvider knownTypeAttributeTarget)
        {
            if (ProfileBase.Properties == null)
                return new Type[0];
            Type[] retArray = new Type[ProfileBase.Properties.Count];
            int i = 0;
            foreach (SettingsProperty property in ProfileBase.Properties)
            {
                retArray[i++] = property.PropertyType;
            }
            return retArray;
        }

    }
}
