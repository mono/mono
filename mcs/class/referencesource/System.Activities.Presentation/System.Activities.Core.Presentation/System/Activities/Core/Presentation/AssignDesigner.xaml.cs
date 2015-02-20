//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Activities.Presentation.Model;
    using System.Runtime;
    using Microsoft.VisualBasic.Activities;
    using System.Reflection;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;

    partial class AssignDesigner
    {
        const string ToPropertyName = "To";
        const string ValuePropertyName = "Value";

        PropertyChangedEventHandler modelItemPropertyChangedHandler;

        public AssignDesigner()
        {
            this.InitializeComponent();
        }

        PropertyChangedEventHandler ModelItemPropertyChangedHandler
        {
            get
            {
                if (this.modelItemPropertyChangedHandler == null)
                {
                    this.modelItemPropertyChangedHandler = new PropertyChangedEventHandler(modelItem_PropertyChanged);
                }

                return this.modelItemPropertyChangedHandler;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.Unloaded += (sender, eventArgs) =>
            {
                AssignDesigner designer = sender as AssignDesigner;
                if (designer != null && designer.ModelItem != null)
                {
                    designer.ModelItem.PropertyChanged -= designer.ModelItemPropertyChangedHandler;
                }
            };            
        }

        internal static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type assignType = typeof(Assign);
            builder.AddCustomAttributes(assignType, new DesignerAttribute(typeof(AssignDesigner)));
            builder.AddCustomAttributes(assignType, new ActivityDesignerOptionsAttribute { AllowDrillIn = false });

            Func<Activity, IEnumerable<ArgumentAccessor>> argumentAccessorGenerator = (activity) =>
            {
                return new ArgumentAccessor[]
                {
                    new ArgumentAccessor
                    {
                        Getter = (ownerActivity) => ((Assign)ownerActivity).To,
                        Setter = (ownerActivity, arg) => ((Assign)ownerActivity).To = arg as OutArgument,
                    },
                    new ArgumentAccessor
                    {
                        Getter = (ownerActivity) => ((Assign)ownerActivity).Value,
                        Setter = (ownerActivity, arg) => ((Assign)ownerActivity).Value = arg as InArgument,
                    },
                };
            };
            ActivityArgumentHelper.RegisterAccessorsGenerator(assignType, argumentAccessorGenerator);
        }

        protected override void OnModelItemChanged(object newItem)
        {
            ModelItem modelItem = newItem as ModelItem;
            if (modelItem != null)
            {
                modelItem.PropertyChanged += ModelItemPropertyChangedHandler;
            }
            base.OnModelItemChanged(newItem);
        }

        void modelItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if the To argument has changed, we may need to update the Value argument's type
            if (e.PropertyName == ToPropertyName)
            {
                Fx.Assert(this.ModelItem != null, "modelItem could not be null if we recent property changed event from it");

                ModelProperty valueProperty = this.ModelItem.Properties[ValuePropertyName];
                ModelProperty toProperty = this.ModelItem.Properties[ToPropertyName];

                Fx.Assert(valueProperty != null, "Value model property could not be null");
                Fx.Assert(toProperty != null, "To model property could not be null");

                Argument value = valueProperty.ComputedValue as Argument;
                Argument to = toProperty.ComputedValue as Argument;

                if (value != null)
                {
                    Type targetType = to == null ? typeof(object) : to.ArgumentType;
                    if (value.ArgumentType != targetType)
                    {
                        valueProperty.SetValue(MorphHelpers.MorphArgument(valueProperty.Value, targetType));
                    }
                }
            }
        }
    }
}
