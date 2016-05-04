//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Converters;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Runtime;

    class ExpressionValueEditor : DialogPropertyValueEditor
    {
        public ExpressionValueEditor()
        {
            //default template for inline editor
            this.InlineEditorTemplate = EditorResources.GetResources()["inlineExpressionEditorTemplate"] as DataTemplate;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            //get the property entry to model item converter
            IValueConverter converter = (ModelPropertyEntryToOwnerActivityConverter)EditorResources.GetResources()["ModelPropertyEntryToOwnerActivityConverter"];
            ModelItem item = (ModelItem)converter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null);
            //we need editing context 
            EditingContext ctx = ((IModelTreeItem)item).ModelTreeManager.Context;
            //get the default dialog owner
            DependencyObject owner = ctx.Services.GetService<DesignerView>();

            //create and show dialog with owner, edited expression and context
            (new EditorDialog(owner, propertyValue, ctx, this.DialogTemplate, this.DialogTitle)).ShowOkCancel();
        }

        protected virtual DataTemplate DialogTemplate
        {
            get { return (DataTemplate)EditorResources.GetResources()["dialogExpressionEditorTemplate"]; }
        }

        protected virtual string DialogTitle
        {
            get { return (string)EditorResources.GetResources()["dialogExpressionEditorTitle"]; }
        }

        private sealed class EditorDialog : WorkflowElementDialog
        {
            public EditorDialog(DependencyObject owner, PropertyValue propertyValue, EditingContext context, DataTemplate dialogTemplate, string title)
            {
                //setup properties
                this.MinWidth = 350;
                this.MinHeight = 185;
                this.WindowResizeMode = ResizeMode.CanResize;
                this.WindowSizeToContent = SizeToContent.Manual;

                this.Owner = owner;
                this.Context = context;
                this.Title = title;
                ContentPresenter contentPresenter = new ContentPresenter()
                {
                    Content = propertyValue,
                    //get default editor template for content presenter 
                    ContentTemplate = dialogTemplate
                };

                this.Content = contentPresenter;
                this.Loaded += OnWindowLoaded;
            }

            void OnWindowLoaded(object sender, RoutedEventArgs args)
            {
                ContentPresenter presenter = (ContentPresenter)this.Content;
                PropertyValue propertyValue = (PropertyValue)presenter.Content;
                Button okButton = (Button)this.FindName("okButton");
                ExpressionTextBox etb = VisualTreeUtils.GetNamedChild<ExpressionTextBox>(presenter, "PART_expressionTextBox");
                TextBlock hint = VisualTreeUtils.GetNamedChild<TextBlock>(presenter, "PART_hintText");
                Fx.Assert(etb != null, "ExpressionTextBox with name 'PART_expressionTextBox' should be in the template!");
                Fx.Assert(hint != null, "Hint TextBlock with name 'PART_hintText' should be in the template!");
                //bind button with ETB's commit command
                okButton.Command = DesignerView.CommitCommand;
                okButton.CommandTarget = etb;
                etb.Loaded += new RoutedEventHandler(OnExpressionTextBoxLoaded);

                if (null != etb && null != hint)
                {
                    IValueConverter typeToStringConverter = (IValueConverter)EditorResources.GetResources()["TypeParameterConverter"];
                    string hintFormatString = (string)EditorResources.GetResources()["dialogExpressionEditorHintFormatString"];

                    //convert expression's container type to friendly name (i.e. replace generic '1 with <T>)                
                    string friendlyTypeName = (string)
                        typeToStringConverter.Convert(etb.ExpressionType ?? propertyValue.ParentProperty.PropertyType, typeof(string), null, CultureInfo.CurrentCulture);

                    //format editor title to include friendly type name and property name
                    hint.Text = string.Format(CultureInfo.CurrentCulture, hintFormatString, propertyValue.ParentProperty.PropertyName, friendlyTypeName);
                }
            }

            void OnExpressionTextBoxLoaded(object sender, RoutedEventArgs e)
            {
                (sender as ExpressionTextBox).BeginEdit();
            }
        }
    }
}
