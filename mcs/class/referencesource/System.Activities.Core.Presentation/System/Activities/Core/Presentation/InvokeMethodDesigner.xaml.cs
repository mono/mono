//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Expressions;
    using System.Activities.Statements;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.PropertyEditing;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Activities.Presentation.View;

    /// <summary>
    /// Interaction logic for InvokeMethodDesigner.xaml
    /// </summary>
    partial class InvokeMethodDesigner
    {
        public InvokeMethodDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(InvokeMethod);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(InvokeMethodDesigner)));
            builder.AddCustomAttributes(
                        type,
                        "GenericTypeArguments",
                        PropertyValueEditor.CreateEditorAttribute(typeof(TypeCollectionPropertyEditor)),
                        new EditorOptionAttribute { Name = TypeCollectionPropertyEditor.AllowDuplicate, Value = true });
            builder.AddCustomAttributes(
                        type,
                        "Parameters",
                        PropertyValueEditor.CreateEditorAttribute(typeof(ArgumentCollectionPropertyEditor)));
            builder.AddCustomAttributes(
                        type,
                        "TargetType",
                        new EditorOptionAttribute { Name = TypePropertyEditor.AllowNull, Value = true },
                        new EditorOptionAttribute { Name = TypePropertyEditor.BrowseTypeDirectly, Value = false });
            builder.AddCustomAttributes(type, new ActivityDesignerOptionsAttribute { AllowDrillIn = false });

            Func<Activity, IEnumerable<ArgumentAccessor>> argumentAccessorGenerator = (activity) => new ArgumentAccessor[]
            {
                new ArgumentAccessor
                {
                    Getter = (ownerActivity) => ((InvokeMethod)ownerActivity).TargetObject,
                    Setter = (ownerActivity, arg) =>
                    {
                        ((InvokeMethod)ownerActivity).TargetObject = arg as InArgument;
                    },
                },
                new ArgumentAccessor
                {
                    Getter = (ownerActivity) => ((InvokeMethod)ownerActivity).Result,
                    Setter = (ownerActivity, arg) =>
                    {
                        ((InvokeMethod)ownerActivity).Result = arg as OutArgument;
                    },
                },
            };
            ActivityArgumentHelper.RegisterAccessorsGenerator(type, argumentAccessorGenerator);
        }
    }
}
