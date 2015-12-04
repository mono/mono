//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Statements;

    using System.ComponentModel;

    /// <summary>
    /// Interaction logic for TryCatchDesigner.xaml
    /// </summary>
    partial class CompensableActivityDesigner
    {
        public CompensableActivityDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(CompensableActivity);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(CompensableActivityDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Body"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("CompensationHandler"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("ConfirmationHandler"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("CancellationHandler"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);
        }
    }
}
