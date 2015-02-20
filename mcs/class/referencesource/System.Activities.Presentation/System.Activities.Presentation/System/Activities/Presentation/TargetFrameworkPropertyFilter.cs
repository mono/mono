//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.ServiceModel.Activities;

    internal static class TargetFrameworkPropertyFilter
    {
        // Ideally we need to filter out ALL new properties introduced in 4.5, for performance
        // reasons, we do not take this approach. Instead, we build a cache for the new properties
        // we want to filter out. Currently the cache is not a full cache, it only contains new properties
        // that affect activities.
        // Ideally, the cache would only contain TypeName and we load type using TypeName.
        // The current implementation of the cache directly references the type defined in S.A.dll and
        // S.SM.A.dll, this is also to save performance cost of resolving the type by type name.
        public static void FilterOut45Properties()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();

            // System.Activities.dll
            builder.AddCustomAttributes(typeof(ActivityBuilder), "ImplementationVersion", BrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(ActivityBuilder<>), "ImplementationVersion", BrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(DynamicActivity), "ImplementationVersion", BrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(DynamicActivity<>), "ImplementationVersion", BrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Flowchart), "ValidateUnconnectedNodes", BrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(FlowDecision), "DisplayName", BrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(FlowSwitch<>), "DisplayName", BrowsableAttribute.No);

            // System.ServiceModel.Activities.dll
            builder.AddCustomAttributes(typeof(WorkflowService), "DefinitionIdentity", BrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(WorkflowService), "ImplementedContracts", BrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Send), "SecurityTokenHandle", BrowsableAttribute.No);

            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
