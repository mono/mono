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
    using System.Windows;
    using System.Runtime;
    using System.Windows.Controls;

    sealed class CorrelatesOnValueEditor : DialogPropertyValueEditor
    {
        public CorrelatesOnValueEditor()
        {
            this.InlineEditorTemplate = EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("CorrelatesOnDesigner_InlineTemplate");
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            ModelPropertyEntryToOwnerActivityConverter propertyEntryConverter = new ModelPropertyEntryToOwnerActivityConverter();

            ModelItem modelItem = (ModelItem)propertyEntryConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null);
            EditingContext context = modelItem.GetEditingContext();

            this.ShowDialog(modelItem, context);
        }

        public void ShowDialog(ModelItem activity, EditingContext context)
        {
            Fx.Assert(activity != null, "Activity model item shouldn't be null!");
            Fx.Assert(context != null, "EditingContext shouldn't be null!");


            string bookmarkTitle = (string)this.InlineEditorTemplate.Resources["bookmarkTitle"];

            UndoEngine undoEngine = context.Services.GetService<UndoEngine>();
            Fx.Assert(null != undoEngine, "UndoEngine should be available");

            using (EditingScope scope = context.Services.GetRequiredService<ModelTreeManager>().CreateEditingScope(bookmarkTitle, true))
            {
                if ((new EditorWindow(activity, context)).ShowOkCancel())
                {
                    scope.Complete();
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
                this.MinHeight = 250;
                this.MinWidth = 450;
                this.WindowResizeMode = ResizeMode.CanResize;
                this.WindowSizeToContent = SizeToContent.Manual;
                var template = EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("CorrelatesOnDesigner_DialogTemplate");

                var presenter = new ContentPresenter()
                {
                    Content = activity,
                    ContentTemplate = template
                };
                this.Title = (string)template.Resources["controlTitle"];
                this.Content = presenter;
                this.HelpKeyword = HelpKeywords.CorrelatesOnDefinitionDialog;
            }

            protected override void OnWorkflowElementDialogClosed(bool? dialogResult)
            {
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    var correlatesOnProperty = this.ModelItem.Properties["CorrelatesOn"];

                    if (correlatesOnProperty.IsSet && 0 == correlatesOnProperty.Dictionary.Count)
                    {
                        correlatesOnProperty.ClearValue();
                    }
                }
            }
        }
    }
}
