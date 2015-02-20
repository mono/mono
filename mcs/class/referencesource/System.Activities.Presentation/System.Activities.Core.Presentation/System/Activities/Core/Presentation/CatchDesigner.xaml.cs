//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;
    using Microsoft.Activities.Presentation;
    

    /// <summary>
    /// Interaction logic for CatchDesigner.xaml
    /// </summary>
    partial class CatchDesigner
    {
        string exceptionTypeShortName = null;
        string exceptionTypeFullName = null;

        public CatchDesigner()
        {
            InitializeComponent();
            this.DragHandle = null;
            this.Loaded += (sender, e) =>
            {
                Selection selection = this.Context.Items.GetValue<Selection>();
                if (selection != null)
                {
                    ModelItem primarySelection = selection.PrimarySelection;
                    this.ExpandState = TryCatchDesigner.IsDescendantOfCatch(this.ModelItem, primarySelection);

                    if (this.ExpandState)
                    {
                        // If current focus is at another part, we need to focus this designer
                        // to trigger selection changed, then this part will expand and another
                        // expanded part will collapse. Then we focus on the activity it contains
                        // if there is one.
                        this.ModelItem.Highlight();
                        if (this.ModelItem != primarySelection && primarySelection.View != null)
                        {
                            primarySelection.Highlight();
                        }
                    }
                }
            };
        }

        internal static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(Catch<>);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(CatchDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Action"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, new ActivityDesignerOptionsAttribute { AllowDrillIn = false });
        }

        public string ExceptionTypeShortName
        {
            get
            {
                if (this.exceptionTypeShortName == null)
                {
                    this.exceptionTypeShortName = TypeNameHelper.GetDisplayName((Type)this.ModelItem.Properties["ExceptionType"].Value.GetCurrentValue(), false);
                }
                return this.exceptionTypeShortName;
            }
        }

        public string ExceptionTypeFullName
        {
            get
            {
                if (this.exceptionTypeFullName == null)
                {
                    this.exceptionTypeFullName = TypeNameHelper.GetDisplayName((Type)this.ModelItem.Properties["ExceptionType"].Value.GetCurrentValue(), true);
                }
                return this.exceptionTypeFullName;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                SwitchTryCatchDesignerHelper.MakeParentRootDesigner<TryCatchDesigner>(this);
                e.Handled = true;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                Keyboard.Focus(this);
                e.Handled = true;
                this.Designer.ShouldStillAllowRubberBandEvenIfMouseLeftButtonDownIsHandled = true;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (this.ShowExpanded)
                {
                    Keyboard.Focus(this);
                }
                e.Handled = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            // avoid context menu upon right-click when it's collapsed
            if (!this.ShowExpanded && e.RightButton == MouseButtonState.Released)
            {
                e.Handled = true;
            }
        }

        protected override string GetAutomationIdMemberName()
        {
            return PropertyNames.ExceptionType;
        }

        void OnAddAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (EditingContextUtilities.GetSingleSelectedModelItem(this.Context) == this.ModelItem)
            {
                ContextMenuUtilities.OnAddAnnotationCommandCanExecute(e, this.Context, this.FindTryCatch());
            }
        }

        void OnAddAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ContextMenuUtilities.OnAddAnnotationCommandExecuted(e, this.FindTryCatch());
        }

        void OnEditAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (EditingContextUtilities.GetSingleSelectedModelItem(this.Context) == this.ModelItem)
            {
                // call the same method as delete annotation command
                ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context, this.FindTryCatch());
            }
        }

        void OnEditAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ContextMenuUtilities.OnEditAnnotationCommandExecuted(e, this.FindTryCatch());
        }

        void OnDeleteAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (EditingContextUtilities.GetSingleSelectedModelItem(this.Context) == this.ModelItem)
            {
                ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context, this.FindTryCatch());
            }
        }

        void OnDeleteAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ContextMenuUtilities.OnDeleteAnnotationCommandExecuted(e, this.FindTryCatch());
        }

        private ModelItem FindTryCatch()
        {
            return this.ModelItem.FindParent((ModelItem item) =>
                {
                    return item.ItemType == typeof(TryCatch);
                });
        }
    }
}
