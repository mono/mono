//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Input;

    partial class CaseDesigner
    {
        public CaseDesigner()
        {
            this.InitializeComponent();
            this.DragHandle = null;
            this.Loaded += (sender, e) =>
            {
                Selection selection = this.Context.Items.GetValue<Selection>();
                if (selection != null)
                {
                    ModelItem primarySelection = selection.PrimarySelection;
                    this.ExpandState = SwitchDesigner.IsDescendantOfCase(this.ModelItem, primarySelection);

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

        // When the CaseDesigner is collapsed, its CaseKeyBox will be disabled. Thus CaseKeyBox.RegainFocus() doesn't
        // work in such situation, we must re-focus the CaseDesigner to expand it first to re-enable the CaseKeyBox. 
        // This situation happens when inputting and invalid case key value and clicking on another Case or Default in 
        // the same parent SwitchDesigner.
        public Action<CaseKeyBox> FocusSelf
        {
            get
            {
                return (ckb) =>
                    {
                        Keyboard.Focus((IInputElement)this);
                    };
            }
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(ModelItemKeyValuePair<,>);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(CaseDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Value"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, new ActivityDesignerOptionsAttribute { AllowDrillIn = false });
        }

        void AttachDisplayName()
        {
            AttachedPropertiesService attachedPropertiesService = this.Context.Services.GetService<AttachedPropertiesService>();
            Fx.Assert(attachedPropertiesService != null, "AttachedPropertiesService is not available.");
            Type modelItemType = this.ModelItem.ItemType;
            foreach (AttachedProperty property in attachedPropertiesService.GetAttachedProperties(modelItemType))
            {
                if (property.Name == "DisplayName" && property.OwnerType == modelItemType)
                {
                    return;
                }
            }
            AttachedProperty<string> displayNameProperty = new AttachedProperty<string>
            {
                Name = "DisplayName",
                OwnerType = modelItemType,
                Getter = (modelItem) => { return "Case"; }
            };
            attachedPropertiesService.AddProperty(displayNameProperty);
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            this.AttachDisplayName();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                SwitchTryCatchDesignerHelper.MakeParentRootDesigner<SwitchDesigner>(this);
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

        void OnAddAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (EditingContextUtilities.GetSingleSelectedModelItem(this.Context) == this.ModelItem)
            {
                ContextMenuUtilities.OnAddAnnotationCommandCanExecute(e, this.Context, this.FindSwitch());
            }
        }

        void OnAddAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ContextMenuUtilities.OnAddAnnotationCommandExecuted(e, this.FindSwitch());
        }

        void OnEditAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (EditingContextUtilities.GetSingleSelectedModelItem(this.Context) == this.ModelItem)
            {
                // call the same method as delete annotation command
                ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context, this.FindSwitch());
            }
        }

        void OnEditAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ContextMenuUtilities.OnEditAnnotationCommandExecuted(e, this.FindSwitch());
        }

        void OnDeleteAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (EditingContextUtilities.GetSingleSelectedModelItem(this.Context) == this.ModelItem)
            {
                ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context, this.FindSwitch());
            }
        }

        void OnDeleteAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ContextMenuUtilities.OnDeleteAnnotationCommandExecuted(e, this.FindSwitch());
        }

        private ModelItem FindSwitch()
        {
            return this.ModelItem.FindParent((ModelItem item) =>
                {
                    return item.ItemType.IsGenericType && item.ItemType.GetGenericTypeDefinition() == typeof(Switch<>);
                });
        }
    }
}
