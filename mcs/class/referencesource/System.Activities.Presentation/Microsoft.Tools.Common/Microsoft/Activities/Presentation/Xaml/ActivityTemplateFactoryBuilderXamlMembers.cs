// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Activities.Presentation.Toolbox;
    using System.Reflection;
    using System.Xaml;

    internal static class ActivityTemplateFactoryBuilderXamlMembers
    {
        private const string ImplementationPropertyName = "Implementation";
        private const string NamePropertyName = "Name";
        private const string TargetTypePropertyName = "TargetType";

        internal static XamlMember ActivityTemplateFactoryImplementationMemberForReader(Type activityTemplateFactoryType, XamlSchemaContext schemaContext)
        {
            return new XamlMember(ImplementationPropertyName, new XamlType(activityTemplateFactoryType, schemaContext), false);
        }

        internal static XamlMember ActivityTemplateFactoryImplementationMemberForWriter(XamlSchemaContext schemaContext)
        {
            PropertyInfo implementationPropertyInfo = typeof(ActivityTemplateFactory).GetProperty(ImplementationPropertyName, BindingFlags.Instance | BindingFlags.NonPublic);
            SharedFx.Assert(implementationPropertyInfo != null, "ActivityTemplateFactory.Implementation should be defined as a protected property of ActivityTemplateFactory.");
            return new XamlMember(implementationPropertyInfo, schemaContext);
        }

        internal static XamlMember ActivityTemplateFactoryBuilderNameMember(XamlSchemaContext schemaContext)
        {
            PropertyInfo namePropertyInfo = typeof(ActivityTemplateFactoryBuilder).GetProperty(NamePropertyName);
            SharedFx.Assert(namePropertyInfo != null, "ActivityTemplateFactoryBuilder.Name should be defined as a public property of ActivityTemplateFactoryBuilder.");
            return new XamlMember(namePropertyInfo, schemaContext);
        }

        internal static XamlMember ActivityTemplateFactoryBuilderTargetTypeMember(XamlSchemaContext schemaContext)
        {
            PropertyInfo namePropertyInfo = typeof(ActivityTemplateFactoryBuilder).GetProperty(TargetTypePropertyName);
            SharedFx.Assert(namePropertyInfo != null, "ActivityTemplateFactoryBuilder.TargetType should be defined as a public property of ActivityTemplateFactoryBuilder.");
            return new XamlMember(namePropertyInfo, schemaContext);
        }

        internal static XamlMember ActivityTemplateFactoryBuilderImplementationMember(XamlSchemaContext schemaContext)
        {
            PropertyInfo implementationPropertyInfo = typeof(ActivityTemplateFactoryBuilder).GetProperty(ImplementationPropertyName);
            SharedFx.Assert(implementationPropertyInfo != null, "ActivityTemplateFactoryBuilder.Implementation should be defined as a public property of ActivityTemplateFactoryBuilder.");
            return new XamlMember(implementationPropertyInfo, schemaContext);
        }
    }
}
