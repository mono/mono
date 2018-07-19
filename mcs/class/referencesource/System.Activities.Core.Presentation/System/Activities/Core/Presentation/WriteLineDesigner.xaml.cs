//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Statements;
    using System.ComponentModel;

    partial class WriteLineDesigner
    {
        public WriteLineDesigner()
        {
            this.InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(WriteLine);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(WriteLineDesigner)));
            builder.AddCustomAttributes(type, new ActivityDesignerOptionsAttribute { AllowDrillIn = false });
        }
    }
}
