namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.ComponentModel.Design;

    #region Class WindowManager
    //This behavior needs the logical coordinates
    internal sealed class WindowManager : WorkflowDesignerMessageFilter
    {
        #region Members and Constructor
        private ActivityDesigner currentActiveDesigner = null;

        internal WindowManager()
        {
        }
        #endregion

        #region MessageFilter Overrides
        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

            //If the point is not in clickable area then return
            WorkflowView parentView = ParentView;
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
                return true;

            //Check if the mouse is hit on designer
            object selectedObject = null;
            HitTestInfo hitTestInfo = MessageHitTestContext;
            if (hitTestInfo == HitTestInfo.Nowhere)
                selectedObject = parentView.RootDesigner.Activity;
            else
                selectedObject = hitTestInfo.SelectableObject;

            //Selection service handles KeyModifiers, ctrl and shift will be handled as per the standard behavior
            if (selectedObject != null)
            {
                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                    selectionService.SetSelectedComponents(new object[] { selectedObject }, SelectionTypes.Primary);
            }

            //Designer designates an area as action area if there is some special significance associated with the area ie Expand Collapse
            //In such cases we give activity designer an oppertunity to take action when mouse click happens on the area
            //if there are Ctrl or Shift keys pressed, only do the selection change, dont call the child designer
            //Now that the designer is selected
            if (this.currentActiveDesigner != hitTestInfo.AssociatedDesigner)
            {
                if (this.currentActiveDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseLeave();

                this.currentActiveDesigner = hitTestInfo.AssociatedDesigner;

                if (this.currentActiveDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseEnter(eventArgs);
            }

            if (this.currentActiveDesigner != null && ((Control.ModifierKeys & (Keys.Control | Keys.Shift)) == 0))
                ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseDown(eventArgs);

            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

            //If the mouse is not in a valid area then we return
            if (!ParentView.IsClientPointInActiveLayout(clientPoint))
            {
                if (this.currentActiveDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseLeave();
                this.currentActiveDesigner = null;
                return true;
            }

            //Now check which designer is hit
            HitTestInfo hitTestInfo = MessageHitTestContext;
            if (this.currentActiveDesigner != hitTestInfo.AssociatedDesigner)
            {
                if (this.currentActiveDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseLeave();

                this.currentActiveDesigner = hitTestInfo.AssociatedDesigner;

                if (this.currentActiveDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseEnter(eventArgs);
            }
            else
            {
                if (this.currentActiveDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseMove(eventArgs);
            }

            return false;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

            //If the mouse is not in a valid area then we return
            if (!ParentView.IsClientPointInActiveLayout(clientPoint))
            {
                if (this.currentActiveDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseLeave();
                this.currentActiveDesigner = null;
                return true;
            }

            //Now check which designer is hit
            HitTestInfo hitTestInfo = MessageHitTestContext;
            if (this.currentActiveDesigner != hitTestInfo.AssociatedDesigner)
            {
                if (this.currentActiveDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseLeave();

                this.currentActiveDesigner = hitTestInfo.AssociatedDesigner;

                if (this.currentActiveDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseEnter(eventArgs);
            }

            //Dispatch the event
            if (this.currentActiveDesigner != null)
                ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseUp(eventArgs);

            return false;
        }

        protected override bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
            {
                ArrayList selectedComponents = new ArrayList(selectionService.GetSelectedComponents());
                for (int i = 0; i < selectedComponents.Count; i++)
                {
                    Activity selectedComponent = selectedComponents[i] as Activity;
                    if (selectedComponent == null)
                        continue;

                    IDesigner designer = ActivityDesigner.GetDesigner(selectedComponent) as IDesigner;
                    if (designer != null)
                    {
                        designer.DoDefaultAction();
                        ((IWorkflowDesignerMessageSink)designer).OnMouseDoubleClick(eventArgs);
                        break;
                    }
                }
            }

            return false;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            //Fire the event
            if (this.currentActiveDesigner != null)
                ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseEnter(eventArgs);

            return false;
        }

        protected override bool OnMouseHover(MouseEventArgs eventArgs)
        {
            //Fire the event
            if (this.currentActiveDesigner != null)
                ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseHover(eventArgs);

            return false;
        }

        protected override bool OnMouseLeave()
        {
            //Dispatch events
            if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseLeave();
                this.currentActiveDesigner = null;
            }

            return false;
        }

        protected override bool OnMouseWheel(MouseEventArgs eventArgs)
        {
            UpdateViewOnMouseWheel(eventArgs, Control.ModifierKeys);
            return true;
        }

        protected override bool OnMouseCaptureChanged()
        {
            //Dispatch events
            if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseCaptureChanged();
            }

            return false;
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink)this.currentActiveDesigner).OnMouseLeave();
                this.currentActiveDesigner = null;
            }

            return false;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (eventArgs != null && (eventArgs.KeyCode == Keys.PageUp || eventArgs.KeyCode == Keys.PageDown))
                UpdateViewOnPageUpDown(eventArgs.KeyCode == Keys.PageUp);
            ISelectionService selectionService = ((IServiceProvider)this.ParentView).GetService(typeof(ISelectionService)) as ISelectionService;

            //enter key (
            if (eventArgs.KeyCode == Keys.Enter)
            {
                // on enter key we want to do DoDefault of the designer
                IDesigner designer = ActivityDesigner.GetDesigner(selectionService.PrimarySelection as Activity) as IDesigner;
                if (designer != null)
                {
                    designer.DoDefaultAction();
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.KeyCode == Keys.Escape)
            {
                if (!eventArgs.Handled)
                {
                    CompositeActivityDesigner parentDesigner = ActivityDesigner.GetParentDesigner(selectionService.PrimarySelection);
                    if (parentDesigner != null)
                        selectionService.SetSelectedComponents(new object[] { parentDesigner.Activity }, SelectionTypes.Replace);

                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.KeyCode == Keys.Delete)
            {
                // check if we are cutting root component
                IDesignerHost designerHost = ((IServiceProvider)this.ParentView).GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (!(designerHost == null || selectionService.GetComponentSelected(designerHost.RootComponent)))
                {
                    //Check that we are cutting all activities
                    //Check if we are in writable context
                    ICollection components = selectionService.GetSelectedComponents();
                    if (DesignerHelpers.AreComponentsRemovable(components))
                    {
                        // check if we can delete these
                        List<Activity> topLevelActivities = new List<Activity>(Helpers.GetTopLevelActivities(selectionService.GetSelectedComponents()));
                        bool needToDelete = (topLevelActivities.Count > 0);
                        IDictionary commonParentActivities = Helpers.PairUpCommonParentActivities(topLevelActivities);
                        foreach (DictionaryEntry entry in commonParentActivities)
                        {
                            CompositeActivityDesigner compositeActivityDesigner = ActivityDesigner.GetDesigner(entry.Key as Activity) as CompositeActivityDesigner;
                            if (compositeActivityDesigner != null && !compositeActivityDesigner.CanRemoveActivities(new List<Activity>((Activity[])((ArrayList)entry.Value).ToArray(typeof(Activity))).AsReadOnly()))
                                needToDelete = false;
                        }

                        if (needToDelete)
                        {
                            List<ConnectorHitTestInfo> connectors = new List<ConnectorHitTestInfo>();
                            foreach (object component in components)
                            {
                                ConnectorHitTestInfo connector = component as ConnectorHitTestInfo;
                                if (connector != null)
                                    connectors.Add(connector);
                            }

                            //cache selcted connectors before calling this func
                            CompositeActivityDesigner.RemoveActivities((IServiceProvider)this.ParentView, topLevelActivities.AsReadOnly(), SR.GetString(SR.DeletingActivities));

                            //add connectors back to the selection service
                            if (selectionService != null && connectors.Count > 0)
                                selectionService.SetSelectedComponents(connectors, SelectionTypes.Add);

                            eventArgs.Handled = true;
                        }
                    }
                }
            }
            //navigation (left, right, up, down, tab, shift-tab)
            else if (eventArgs.KeyCode == Keys.Left || eventArgs.KeyCode == Keys.Right || eventArgs.KeyCode == Keys.Up || eventArgs.KeyCode == Keys.Down || eventArgs.KeyCode == Keys.Tab)
            {
                //we'll pass it to the parent designer of the primary selected designer
                //sequential designers just navigate between their children
                //free form designers may move their children on arrow keys and navigate on tab
                ActivityDesigner designer = ActivityDesigner.GetDesigner(selectionService.PrimarySelection as Activity) as ActivityDesigner;
                if (designer != null && designer.ParentDesigner != null)
                {
                    //we will let the parent see if it wants to handle the event, 
                    //otherwise the selected designer itself will be called from a designer message filter below
                    ((IWorkflowDesignerMessageSink)designer.ParentDesigner).OnKeyDown(eventArgs);
                    eventArgs.Handled = true;
                }
            }

            if (!eventArgs.Handled)
            {
                ActivityDesigner designerWithFocus = GetDesignerWithFocus();
                if (designerWithFocus != null)
                    ((IWorkflowDesignerMessageSink)designerWithFocus).OnKeyDown(eventArgs);
            }

            return eventArgs.Handled;
        }

        protected override bool OnKeyUp(KeyEventArgs eventArgs)
        {
            ActivityDesigner designerWithFocus = GetDesignerWithFocus();
            if (designerWithFocus != null)
                ((IWorkflowDesignerMessageSink)designerWithFocus).OnKeyUp(eventArgs);

            return false;
        }

        protected override bool OnScroll(ScrollBar sender, int value)
        {
            ActivityDesigner designerWithFocus = GetDesignerWithFocus();
            if (designerWithFocus != null)
                ((IWorkflowDesignerMessageSink)designerWithFocus).OnScroll(sender, value);

            return false;
        }

        protected override bool OnShowContextMenu(Point screenMenuPoint)
        {
            IMenuCommandService menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (menuCommandService != null)
                menuCommandService.ShowContextMenu(WorkflowMenuCommands.SelectionMenu, screenMenuPoint.X, screenMenuPoint.Y);
            return true;
        }

        protected override void OnLayout(LayoutEventArgs eventArgs)
        {
            WorkflowView parentView = ParentView;
            using (Graphics graphics = parentView.CreateGraphics())
            {
                if (parentView.RootDesigner != null)
                {
                    try
                    {
                        ((IWorkflowDesignerMessageSink)parentView.RootDesigner).OnLayoutSize(graphics);
                    }
                    catch (Exception e)
                    {
                        //Eat the exception thrown
                        Debug.WriteLine(e);
                    }

                    try
                    {
                        ((IWorkflowDesignerMessageSink)parentView.RootDesigner).OnLayoutPosition(graphics);
                    }
                    catch (Exception e)
                    {
                        //Eat the exception thrown
                        Debug.WriteLine(e);
                    }
                }
            }
        }

        protected override bool ProcessMessage(Message message)
        {
            ActivityDesigner designerWithFocus = GetDesignerWithFocus();
            if (designerWithFocus != null)
                ((IWorkflowDesignerMessageSink)designerWithFocus).ProcessMessage(message);

            return false;
        }

        protected override void OnThemeChange()
        {
            WorkflowView parentView = ParentView;
            if (parentView.RootDesigner != null)
                ((IWorkflowDesignerMessageSink)parentView.RootDesigner).OnThemeChange();
        }
        #endregion

        #region Helpers
        private ActivityDesigner GetDesignerWithFocus()
        {
            ActivityDesigner designerWithFocus = null;

            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
            {
                object primarySelection = selectionService.PrimarySelection;
                if (primarySelection is Activity)
                    designerWithFocus = ActivityDesigner.GetDesigner(primarySelection as Activity);
                else
                    designerWithFocus = ActivityDesigner.GetParentDesigner(primarySelection);
            }

            return designerWithFocus;
        }

        private void UpdateViewOnPageUpDown(bool pageUp)
        {
            WorkflowView parentView = ParentView;
            Point scrollPosition = parentView.ScrollPosition;
            scrollPosition.Y = scrollPosition.Y + ((pageUp ? -1 : 1) * parentView.VScrollBar.LargeChange);
            parentView.ScrollPosition = scrollPosition;
        }

        private void UpdateViewOnMouseWheel(MouseEventArgs eventArgs, Keys modifierKeys)
        {
            WorkflowView parentView = ParentView;
            if (Control.ModifierKeys == Keys.Control)
            {
                int newZoom = parentView.Zoom + ((eventArgs.Delta / 120) * 10);
                newZoom = Math.Max(newZoom, AmbientTheme.MinZoom);
                newZoom = Math.Min(newZoom, AmbientTheme.MaxZoom);
                parentView.Zoom = newZoom;
            }
            else
            {
                //scroll up and down
                int numberOfLogicalLines = -eventArgs.Delta / 120;
                int logicalLineHeight = parentView.VScrollBar.SmallChange;
                Point scrollPosition = parentView.ScrollPosition;

                scrollPosition.Y = scrollPosition.Y + (numberOfLogicalLines * logicalLineHeight);
                parentView.ScrollPosition = scrollPosition;
            }
        }
        #endregion
    }
    #endregion
}
