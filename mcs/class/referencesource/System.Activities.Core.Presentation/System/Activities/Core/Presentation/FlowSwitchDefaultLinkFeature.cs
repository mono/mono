//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.ComponentModel;
    using System.Runtime.Versioning;
    using Microsoft.Activities.Presentation;

    internal class FlowSwitchDefaultLinkFeature : Feature
    {
        public override void Initialize(EditingContext context, Type modelType)
        {
            if (context.Services.GetService<DesignerConfigurationService>().TargetFrameworkName.IsLessThan45())
            {
                AttributeTableBuilder builder = new AttributeTableBuilder();
                builder.AddCustomAttributes(typeof(FlowSwitchDefaultLink<>), "DefaultCaseDisplayName", BrowsableAttribute.No);
                MetadataStore.AddAttributeTable(builder.CreateTable());
            }
        }
    }
}
