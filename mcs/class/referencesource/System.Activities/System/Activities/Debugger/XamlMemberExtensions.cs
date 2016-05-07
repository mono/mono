// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Reflection;
    using System.Xaml;
    using System.Xaml.Schema;

    internal static class XamlMemberExtensions
    {
        internal static XamlMember ReplaceXamlMemberInvoker(this XamlMember originalXamlMember, XamlSchemaContext schemaContext, XamlMemberInvoker newInvoker)
        {
            if (originalXamlMember.IsEvent)
            {
                if (originalXamlMember.IsAttachable)
                {
                    UnitTestUtility.Assert(originalXamlMember.UnderlyingMember is MethodInfo, "Guaranteed by XamlMember.");
                    return new XamlMember(originalXamlMember.Name, originalXamlMember.UnderlyingMember as MethodInfo, schemaContext, newInvoker);
                }
                else
                {
                    UnitTestUtility.Assert(originalXamlMember.UnderlyingMember is EventInfo, "Guaranteed by XamlMember.");
                    return new XamlMember(originalXamlMember.UnderlyingMember as EventInfo, schemaContext, newInvoker);
                }
            }
            else if (originalXamlMember.IsDirective)
            {
                return originalXamlMember;
            }
            else if (originalXamlMember.IsUnknown)
            {
                return originalXamlMember;
            }
            else
            {
                if (originalXamlMember.IsAttachable)
                {
                    MethodInfo attachablePropertyMethod = originalXamlMember.UnderlyingMember as MethodInfo;
                    if (attachablePropertyMethod.ReturnType == typeof(void))
                    {
                        return new XamlMember(originalXamlMember.Name, null, originalXamlMember.UnderlyingMember as MethodInfo, schemaContext, newInvoker);
                    }
                    else
                    {
                        return new XamlMember(originalXamlMember.Name, originalXamlMember.UnderlyingMember as MethodInfo, null, schemaContext, newInvoker);
                    }
                }
                else
                {
                    PropertyInfo propertyInfo = originalXamlMember.UnderlyingMember as PropertyInfo;
                    if (propertyInfo != null)
                    {
                        return new XamlMember(propertyInfo, schemaContext, newInvoker);
                    }
                    else
                    {
                        return originalXamlMember;
                    }
                }
            }
        }
    }
}
