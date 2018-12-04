// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Reflection;
using System.Windows.Markup;

namespace System.Xaml
{
    static class LooseTypeExtensions
    {
        const string WindowsBase = "WindowsBase";
        static readonly byte[] WindowsBaseToken = { 49, 191, 56, 86, 173, 54, 78, 53 };

        // Note: this is a version-tolerant comparison, i.e. the types are considered equal if their
        // names, namespaces, assembly short names, culture infos, and public keys match.
        internal static bool AssemblyQualifiedNameEquals(Type t1, Type t2)
        {
            if (Object.ReferenceEquals(t1, null))
            {
                return Object.ReferenceEquals(t2, null);
            }

            if (Object.ReferenceEquals(t2, null))
            {
                return false;
            }

            if (t1.FullName != t2.FullName)
            {
                return false;
            }
            if (t1.Assembly.FullName == t2.Assembly.FullName)
            {
                return true;
            }
            AssemblyName t1name = new AssemblyName(t1.Assembly.FullName);
            AssemblyName t2name = new AssemblyName(t2.Assembly.FullName);
            if (t1name.Name == t2name.Name)
            {
                return t1name.CultureInfo.Equals(t2name.CultureInfo) &&
                    SafeSecurityHelper.IsSameKeyToken(t1name.GetPublicKeyToken(), t2name.GetPublicKeyToken());
            }
            return IsWindowsBaseToSystemXamlComparison(t1.Assembly, t2.Assembly, t1name, t2name);
        }

        // When doing a version-tolerant comparison against System.Xaml types, we also need to
        // support references to types that were type-forwarded from WindowsBase.
        static bool IsWindowsBaseToSystemXamlComparison(Assembly a1, Assembly a2,
            AssemblyName name1, AssemblyName name2)
        {
            AssemblyName windowsBaseName = null;
            if (name1.Name == WindowsBase && a2 == typeof(MarkupExtension).Assembly)
            {
                windowsBaseName = name1;
            }
            else if (name2.Name == WindowsBase && a1 == typeof(MarkupExtension).Assembly)
            {
                windowsBaseName = name2;
            }
            return (windowsBaseName != null && SafeSecurityHelper.IsSameKeyToken(windowsBaseName.GetPublicKeyToken(), WindowsBaseToken));
        }

        internal static bool IsAssemblyQualifiedNameAssignableFrom(Type t1, Type t2)
        {
            if (t1 == null || t2 == null)
            {
                return false;
            }

            if (AssemblyQualifiedNameEquals(t1, t2))
            {
                return true;
            }

            if (IsLooseSubClassOf(t2, t1))
            {
                return true;
            }

            if (t1.IsInterface)
            {
                return LooselyImplementInterface(t2, t1);
            }

            if (!t1.IsGenericParameter)
            {
                return false;
            }

            Type[] genericParameterConstraints = t1.GetGenericParameterConstraints();
            for (int i = 0; i < genericParameterConstraints.Length; i++)
            {
                if (!IsAssemblyQualifiedNameAssignableFrom(genericParameterConstraints[i], t2))
                {
                    return false;
                }
            }

            return true;            
        }

        static bool LooselyImplementInterface(Type t, Type interfaceType)
        {
            for (Type type = t; type != null; type = type.BaseType)
            {
                Type[] interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (AssemblyQualifiedNameEquals(interfaces[i], interfaceType)
                        || LooselyImplementInterface(interfaces[i], interfaceType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool IsLooseSubClassOf(Type t1, Type t2)
        {
            if (t1 == null || t2 == null)
            {
                return false;
            }

            if (AssemblyQualifiedNameEquals(t1, t2))
            {
                return false; //strictly testing for sub-class
            }

            for(Type baseType = t1.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                if (AssemblyQualifiedNameEquals(baseType, t2))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
