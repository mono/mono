//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.PropertyEditing;

    sealed class EndpointDesigner
    {
        internal static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type endpointType = typeof(Endpoint);

            var browsableAttribute = new BrowsableAttribute(false);
            builder.AddCustomAttributes(endpointType, endpointType.GetProperty("BehaviorConfigurationName"), browsableAttribute);
            builder.AddCustomAttributes(endpointType, endpointType.GetProperty("Headers"), browsableAttribute);
            builder.AddCustomAttributes(endpointType, endpointType.GetProperty("Identity"), browsableAttribute);
            builder.AddCustomAttributes(endpointType, endpointType.GetProperty("Name"), browsableAttribute);
            builder.AddCustomAttributes(endpointType, endpointType.GetProperty("ListenUri"), browsableAttribute);
            builder.AddCustomAttributes(endpointType, endpointType.GetProperty("ServiceContractName"), browsableAttribute);

            builder.AddCustomAttributes(endpointType, endpointType.GetProperty("Binding"),
                PropertyValueEditor.CreateEditorAttribute(typeof(BindingPropertyValueEditor)));
        }

    }
}
