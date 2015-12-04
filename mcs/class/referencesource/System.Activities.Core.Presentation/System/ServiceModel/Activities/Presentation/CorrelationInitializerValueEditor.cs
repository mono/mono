//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Runtime;
    using System.Windows;

    sealed class CorrelationInitializerValueEditor : DialogPropertyValueEditor
    {
        public CorrelationInitializerValueEditor()
        {
            this.InlineEditorTemplate = EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("CorrelationInitializer_InlineTemplate");
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


            string bookmarkTitle = (string)this.InlineEditorTemplate.Resources["bookmarkTitle"];

            UndoEngine undoEngine = context.Services.GetService<UndoEngine>();
            Fx.Assert(null != undoEngine, "UndoEngine should be available");

            using (ModelEditingScope editingScope = modelItem.BeginEdit(bookmarkTitle, shouldApplyChangesImmediately: true))
            {
                if ((new EditorWindow(modelItem, context)).ShowOkCancel())
                {
                    editingScope.Complete();
                }
                else
                {
                    editingScope.Revert();
                }
            }
        }

        sealed class EditorWindow : WorkflowElementDialog
        {
            public EditorWindow(ModelItem activity, EditingContext context)
            {
                this.ModelItem = activity;
                this.Context = context;
                this.Owner = activity.View;
                this.EnableMaximizeButton = false;
                this.EnableMinimizeButton = false;
                this.MinWidth = 450;
                this.MinHeight = 250; 
                this.WindowResizeMode = ResizeMode.CanResize;
                this.WindowSizeToContent = SizeToContent.Manual;
                var content = new CorrelationInitializerDesigner() { Activity = activity };
                this.Title = (string)content.Resources["controlTitle"];
                this.Content = content;
                this.HelpKeyword = HelpKeywords.AddCorrelationInitializersDialog;
            }

            protected override void OnWorkflowElementDialogClosed(bool? dialogResult)
            {
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    ((CorrelationInitializerDesigner)this.Content).CleanupObjectMap();
                }
            }
        }
    }
}
