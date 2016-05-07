//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    //This class is visual representation of ResizeGrip like control, which is used in a Grid to allow resizing.
    class StateContainerResizeGrip : Control
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(DrawingBrush), typeof(StateContainerResizeGrip));

        public static readonly DependencyProperty ParentStateContainerEditorProperty =
            DependencyProperty.Register("ParentStateContainerEditor", typeof(StateContainerEditor), typeof(StateContainerResizeGrip));

        public static readonly DependencyProperty DisabledProperty =
            DependencyProperty.Register("Disabled", typeof(bool), typeof(StateContainerResizeGrip), new UIPropertyMetadata(false));

        Point offset;

        // The scope is used for capturing the current size of all the StateContainer instances that contain the target ResizeGrip.  
        // As the user resizes the target StateContainer, its Visual ancestors would get resized.  
        // The purpose of the scope is to store their sizes before the resizing to facilitate Undo.
        EditingScope scope;

        public DrawingBrush Icon
        {
            get { return (DrawingBrush)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public StateContainerEditor ParentStateContainerEditor
        {
            get { return (StateContainerEditor)GetValue(ParentStateContainerEditorProperty); }
            set { SetValue(ParentStateContainerEditorProperty, value); }
        }

        public bool Disabled
        {
            get { return (bool)GetValue(DisabledProperty); }
            set { SetValue(DisabledProperty, value); }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Cursor = Cursors.SizeNWSE;
        }

        protected override void OnGotMouseCapture(MouseEventArgs e)
        {
            ModelItem stateContainerModelItem = this.ParentStateContainerEditor.ModelItem;
            string undoItemName = string.Empty;
            if (stateContainerModelItem.ItemType == typeof(StateMachine))
            {
                undoItemName = SR.StateMachineResize;
            }
            else if (stateContainerModelItem.ItemType == typeof(State))
            {
                undoItemName = SR.StateResize;
            }
            else
            {
                Fx.Assert(false, "The model item type is invalid");
            }
            this.scope = (EditingScope)this.ParentStateContainerEditor.ModelItem.BeginEdit(undoItemName);
            base.OnGotMouseCapture(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            if (this.scope != null)
            {
                this.scope.Complete();
                this.scope.Dispose();
                this.scope = null;
            }
            base.OnLostMouseCapture(e);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e != null && !this.Disabled)
            {
                this.offset = e.GetPosition(this);
                this.CaptureMouse();
                if (this.scope != null)
                {
                    this.ParentStateContainerEditor.StoreShapeSizeWithUndoRecursively(this.ParentStateContainerEditor.ModelItem);                    
                }
                // Select the designer when it is being resized
                WorkflowViewElement designer = this.ParentStateContainerEditor.ModelItem.View as WorkflowViewElement;
                
                if (!designer.IsKeyboardFocusWithin)
                {
                    // Fix 185562 - if the designer has the keyboard focus (i.e. DisplayName being edited)
                    // then there is no need to refocus on the designer again.  That prevents the
                    // DisplayName editing to be group into the same EditingScope as resizing, and
                    // also the defaultDisplayNameReadOnlyControl from being Visible but not modified.
                    Keyboard.Focus(designer);
                }

                StateMachineDesigner stateMachineDesigner = VisualTreeUtils.FindVisualAncestor<StateMachineDesigner>(this.ParentStateContainerEditor);
                stateMachineDesigner.IsResizing = true;

                e.Handled = true;
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);
            if (args != null && !this.Disabled)
            {
                if (args.LeftButton == MouseButtonState.Pressed && this.IsMouseCaptured && this.scope != null)
                {
                    StateContainerEditor stateContainerEditor = this.ParentStateContainerEditor;
                    FreeFormPanel panel = stateContainerEditor.Panel;
                    Grid stateContainerGrid = stateContainerEditor.stateContainerGrid;
                    Point currentPosition = Mouse.GetPosition(stateContainerGrid);
                    currentPosition.Offset(this.offset.X, this.offset.Y);
                    stateContainerEditor.StateContainerWidth = Math.Min(Math.Max(panel.RequiredWidth, currentPosition.X), stateContainerGrid.MaxWidth);
                    stateContainerEditor.StateContainerHeight = Math.Min(Math.Max(panel.RequiredHeight, currentPosition.Y), stateContainerGrid.MaxHeight);
                    args.Handled = true;
                }
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e != null && !this.Disabled && this.scope != null)
            {
                ModelItem stateContainerModelItem = this.ParentStateContainerEditor.ModelItem;
                ViewStateService viewStateService = this.ParentStateContainerEditor.Context.Services.GetService<ViewStateService>();
                viewStateService.StoreViewStateWithUndo(stateContainerModelItem, StateContainerEditor.StateContainerWidthViewStateKey, this.ParentStateContainerEditor.StateContainerWidth);
                viewStateService.StoreViewStateWithUndo(stateContainerModelItem, StateContainerEditor.StateContainerHeightViewStateKey, this.ParentStateContainerEditor.StateContainerHeight);
                Mouse.OverrideCursor = null;
                Mouse.Capture(null);
                StateMachineDesigner stateMachineDesigner = VisualTreeUtils.FindVisualAncestor<StateMachineDesigner>(this.ParentStateContainerEditor);
                stateMachineDesigner.IsResizing = false;
                e.Handled = true;
            }
            base.OnPreviewMouseLeftButtonUp(e);
        }
    }
}
