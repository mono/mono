//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System;
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows;
    using System.Collections.ObjectModel;

    partial class InitializeCorrelationDesigner 
    {
        public const string CorrelationPropertyName = "Correlation";
        public const string CorrelationDataPropertyName = "CorrelationData";

        public InitializeCorrelationDesigner()
        {
            InitializeComponent();            
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "PropertyValueEditors association needs to be done in the static constructor.")]
        static InitializeCorrelationDesigner()
        {
            Type type = typeof(InitializeCorrelation);
            AttributeTableBuilder builder = new AttributeTableBuilder();

            builder.AddCustomAttributes(type, type.GetProperty("Correlation"),
                new DescriptionAttribute(StringResourceDictionary.Instance.GetString("messagingCorrelatesWithHint")));

            builder.AddCustomAttributes(type, type.GetProperty("CorrelationData"),
                PropertyValueEditor.CreateEditorAttribute(typeof(CorrelationDataValueEditor)),
                new DescriptionAttribute(StringResourceDictionary.Instance.GetString("messagingCorrelationDataHint")));

            builder.AddCustomAttributes(type, "CorrelationData", BrowsableAttribute.Yes);
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        void OnEditCorrelationData(object sender, RoutedEventArgs e)
        {
            var dlg = new CorrelationDataValueEditor();
            dlg.ShowDialog(this.ModelItem, this.ModelItem.GetEditingContext());
            this.UpdateButton();
        }

        protected override void OnReadOnlyChanged(bool isReadOnly)
        {
            this.btnCorrelationData.IsEnabled = !isReadOnly;
            base.OnReadOnlyChanged(isReadOnly);
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            this.UpdateButton();
        }

        void UpdateButton()
        {
            this.btnCorrelationData.Content =
                this.ModelItem.Properties[CorrelationDataPropertyName].IsSet ? this.FindResource("viewTitle") : this.FindResource("defineTitle");
        }

        internal sealed class CorrelationDataValueEditor : DialogPropertyValueEditor
        {
            public CorrelationDataValueEditor()
            {
                this.InlineEditorTemplate = EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("CorrelationDataValueEditor_InlineTemplate");
            }

            public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
            {
                ModelPropertyEntryToOwnerActivityConverter propertyEntryConverter = new ModelPropertyEntryToOwnerActivityConverter();

                ModelItem modelItem = (ModelItem)propertyEntryConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null);
                EditingContext context = modelItem.GetEditingContext();

                this.ShowDialog(modelItem, context);
            }

            public void ShowDialog(ModelItem modelItem, EditingContext context)
            {
                Fx.Assert(modelItem != null, "Activity model item shouldn't be null!");
                Fx.Assert(context != null, "EditingContext shouldn't be null!");
                
                new EditorWindow(modelItem, modelItem.Properties[CorrelationPropertyName].Value, this.GetCorrelationDataWrapperCollection(modelItem), context).ShowOkCancel();
            }

            ObservableCollection<CorrelationDataWrapper> GetCorrelationDataWrapperCollection(ModelItem modelItem)
            {
                ObservableCollection<CorrelationDataWrapper> wrapperCollection = null;
                if (modelItem.ItemType == typeof(InitializeCorrelation))
                {
                    wrapperCollection = new ObservableCollection<CorrelationDataWrapper>();
                    foreach (ModelItem entry in modelItem.Properties[CorrelationDataPropertyName].Dictionary.Properties["ItemsCollection"].Collection)
                    { 
                        wrapperCollection.Add(new CorrelationDataWrapper((string)entry.Properties["Key"].ComputedValue, entry.Properties["Value"].Value));
                    }
                }
                return wrapperCollection;
            }

            sealed class EditorWindow : WorkflowElementDialog
            {
                public EditorWindow(ModelItem activity, ModelItem correlationHandler, ObservableCollection<CorrelationDataWrapper> correlationData, EditingContext context)
                {
                    this.ModelItem = activity;
                    this.Context = context;
                    this.Owner = activity.View;
                    this.MinHeight = 250;
                    this.MinWidth = 450;
                    this.EnableMaximizeButton = false;
                    this.EnableMinimizeButton = false;
                    this.WindowResizeMode = ResizeMode.CanResize;
                    this.WindowSizeToContent = SizeToContent.Manual;
                    var content = new CorrelationDataDesigner() 
                    { 
                        Activity = activity,
                        CorrelationHandle = correlationHandler,
                        CorrelationInitializeData = correlationData
                    };
                    this.Title = (string)content.Resources["controlTitle"];
                    this.Content = content;
                    this.HelpKeyword = HelpKeywords.InitializeCorrelationDialog;
                }

                protected override void OnWorkflowElementDialogClosed(bool? dialogResult)
                {
                    if ((dialogResult.HasValue) && (dialogResult.Value))
                    {
                        (this.Content as CorrelationDataDesigner).CommitEdit();
                    }
                }
            }
        }
    }
}
