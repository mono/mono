//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Windows;

    internal sealed partial class InvokeDelegateDesigner
    {
        private const string DelegatePropertyName = "Delegate";
        private const string DelegateArgumentsPropertyName = "DelegateArguments";
        private const string DefaultPropertyName = "Default";

        private bool isSetInternally;

        public InvokeDelegateDesigner()
        {
            this.InitializeComponent();
            this.chooser.Filter = IsActivityDelegate;
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(InvokeDelegate), new DesignerAttribute(typeof(InvokeDelegateDesigner)));
            builder.AddCustomAttributes(typeof(InvokeDelegate), new ActivityDesignerOptionsAttribute { AllowDrillIn = false });
            builder.AddCustomAttributes(typeof(InvokeDelegate), new FeatureAttribute(typeof(InvokeDelegateValidationFeature)));
            builder.AddCustomAttributes(typeof(InvokeDelegate), DelegatePropertyName, BrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(InvokeDelegate), DelegateArgumentsPropertyName, PropertyValueEditor.CreateEditorAttribute(typeof(DelegateArgumentsValueEditor)), BrowsableAttribute.Yes);
            builder.AddCustomAttributes(typeof(InvokeDelegate), DefaultPropertyName, BrowsableAttribute.No);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.Loaded += new RoutedEventHandler(this.OnLoaded);
            this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
        }

        protected override void OnReadOnlyChanged(bool isReadOnly)
        {
            this.chooser.IsEnabled = !isReadOnly;
        }

        private static bool IsActivityDelegate(DynamicActivityProperty instance)
        {
            return instance.Type == typeof(ActivityDelegate) || instance.Type.IsSubclassOf(typeof(ActivityDelegate));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ModelItem.Root.ItemType != typeof(ActivityBuilder))
            {
                return;
            }

            this.ModelItem.PropertyReferenceChanged += this.OnPropertyReferenceChanged;

            this.isSetInternally = true;
            this.chooser.Properties = ModelItem.Root.Properties["Properties"].Collection;
            this.chooser.SelectedPropertyName = ModelItem.Properties[DelegatePropertyName].Reference;
            this.isSetInternally = false;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.ModelItem.PropertyReferenceChanged -= this.OnPropertyReferenceChanged;
        }

        private void OnPropertyReferenceChanged(object sender, PropertyReferenceChangedEventArgs e)
        {
            if (ModelItem.Root.ItemType != typeof(ActivityBuilder))
            {
                return;
            }

            string propertyName = this.ModelItem.Properties[DelegatePropertyName].Reference;

            this.isSetInternally = true;
            if (!string.IsNullOrEmpty(propertyName))
            {
                this.chooser.SelectedPropertyName = propertyName;
            }
            else
            {
                this.chooser.SelectedPropertyName = null;
            }

            this.isSetInternally = false;
        }

        private void OnSelectedPropertyNameChanged(object sender, SelectedPropertyNameChangedEventArgs e)
        {
            if (this.isSetInternally)
            {
                return;
            }

            if (ModelItem.Root.ItemType != typeof(ActivityBuilder))
            {
                return;
            }

            string propertyName = this.chooser.SelectedPropertyName;

            if (propertyName != null && !this.chooser.IsUpdatingDropDownItems)
            {
                this.ModelItem.Properties[DelegatePropertyName].SetReference(propertyName);

                this.FillArguments();
            }
        }

        private void FillArguments()
        {
            string propertyName = PropertyReferenceUtilities.GetPropertyReference(this.ModelItem.GetCurrentValue(), DelegatePropertyName);

            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            ModelTreeManager manager = this.Context.Services.GetService<ModelTreeManager>();
            DynamicActivityProperty property = DynamicActivityPropertyUtilities.Find(manager.Root.Properties["Properties"].Collection, propertyName);

            if (property == null || !property.Type.IsSubclassOf(typeof(ActivityDelegate)))
            {
                return;
            }

            ActivityDelegateMetadata metadata = ActivityDelegateUtilities.GetMetadata(property.Type);

            ModelItemCollection collection = this.ModelItem.Properties[DelegateArgumentsPropertyName].Value.Properties["ItemsCollection"].Collection;

            Type underlyingArgumentType = this.ModelItem.Properties[DelegateArgumentsPropertyName].Value.GetCurrentValue().GetType().GetGenericArguments()[1];
            if (!typeof(Argument).IsAssignableFrom(underlyingArgumentType))
            {
                return;
            }

            if (collection.Count == 0)
            {
                using (ModelEditingScope change = collection.BeginEdit(SR.FillDelegateArguments))
                {
                    Type dictionaryEntryType = typeof(ModelItemKeyValuePair<,>).MakeGenericType(new Type[] { typeof(string), underlyingArgumentType });
                    foreach (ActivityDelegateArgumentMetadata arg in metadata)
                    {
                        Argument argument = Argument.Create(arg.Type, arg.Direction == ActivityDelegateArgumentDirection.In ? ArgumentDirection.In : ArgumentDirection.Out);
                        object mutableKVPair = Activator.CreateInstance(dictionaryEntryType, new object[] { arg.Name, argument });
                        ModelItem argumentKVPair = collection.Add(mutableKVPair);
                    }

                    change.Complete();
                }
            }
        }
    }
}
