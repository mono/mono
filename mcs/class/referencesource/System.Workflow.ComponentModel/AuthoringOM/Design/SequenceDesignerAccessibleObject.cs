#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Globalization;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization.Formatters.Binary;

    //

    #region Class SequenceDesignerAccessibleObject
    /// <summary>
    /// Accessibility object class associated with SequentialActivityDesigner
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SequenceDesignerAccessibleObject : CompositeDesignerAccessibleObject
    {
        public SequenceDesignerAccessibleObject(SequentialActivityDesigner activityDesigner)
            : base(activityDesigner)
        {
        }

        public override AccessibleObject GetChild(int index)
        {
            SequentialActivityDesigner sequentialActivityDesigner = base.ActivityDesigner as SequentialActivityDesigner;
            if (sequentialActivityDesigner.ActiveDesigner != sequentialActivityDesigner)
                return base.GetChild(index);

            if (index >= 0 && index < GetChildCount() && ((index % 2) == 0))
                return new SequentialConnectorAccessibleObject(base.ActivityDesigner as SequentialActivityDesigner, index / 2);
            else
                return base.GetChild(index / 2);
        }

        public override int GetChildCount()
        {
            SequentialActivityDesigner sequentialActivityDesigner = base.ActivityDesigner as SequentialActivityDesigner;
            if (sequentialActivityDesigner.ActiveDesigner != sequentialActivityDesigner)
                return base.GetChildCount();

            //We also create create a accessible object for each connector
            if (sequentialActivityDesigner != null)
                return sequentialActivityDesigner.ContainedDesigners.Count + sequentialActivityDesigner.ContainedDesigners.Count + 1;
            else
                return -1;
        }

        public override AccessibleObject Navigate(AccessibleNavigation navdir)
        {
            if (navdir == AccessibleNavigation.Up || navdir == AccessibleNavigation.Previous ||
                navdir == AccessibleNavigation.Down || navdir == AccessibleNavigation.Next)
            {
                DesignerNavigationDirection navigate = default(DesignerNavigationDirection);
                if (navdir == AccessibleNavigation.Up || navdir == AccessibleNavigation.Previous)
                    navigate = DesignerNavigationDirection.Up;
                else
                    navigate = DesignerNavigationDirection.Down;

                CompositeActivityDesigner compositeDesigner = this.ActivityDesigner.ParentDesigner;
                if (compositeDesigner != null)
                {
                    object nextSelectableObj = compositeDesigner.GetNextSelectableObject(this.ActivityDesigner.Activity, navigate);
                    if (nextSelectableObj is ConnectorHitTestInfo)
                        return GetChild(((ConnectorHitTestInfo)nextSelectableObj).MapToIndex());
                }
            }

            return base.Navigate(navdir);
        }

        #region Class SequentialConnectorAccessibleObject
        private sealed class SequentialConnectorAccessibleObject : AccessibleObject
        {
            private ConnectorHitTestInfo connectorHitInfo;

            internal SequentialConnectorAccessibleObject(SequentialActivityDesigner activityDesigner, int connectorIndex)
            {
                if (activityDesigner == null)
                    throw new ArgumentNullException("activityDesigner");

                this.connectorHitInfo = new ConnectorHitTestInfo(activityDesigner, HitTestLocations.Designer, connectorIndex);
            }

            public override Rectangle Bounds
            {
                get
                {
                    return ((SequentialActivityDesigner)this.connectorHitInfo.AssociatedDesigner).InternalRectangleToScreen(this.connectorHitInfo.Bounds);
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return DR.GetString(DR.AccessibleAction);
                }
            }

            public override string Description
            {
                get
                {
                    return DR.GetString(DR.ConnectorAccessibleDescription, this.connectorHitInfo.GetType().Name);
                }
            }

            public override string Help
            {
                get
                {
                    return DR.GetString(DR.ConnectorAccessibleHelp, this.connectorHitInfo.GetType().Name);
                }
            }

            public override string Name
            {
                get
                {
                    return DR.GetString(DR.ConnectorDesc, this.connectorHitInfo.MapToIndex().ToString(CultureInfo.InvariantCulture), Parent.Name);
                }

                set
                {
                }
            }

            public override AccessibleObject Parent
            {
                get
                {
                    return this.connectorHitInfo.AssociatedDesigner.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Diagram;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates state = AccessibleStates.MultiSelectable;

                    if (this.connectorHitInfo.AssociatedDesigner.IsLocked)
                        state |= AccessibleStates.ReadOnly;

                    if (!this.connectorHitInfo.AssociatedDesigner.IsVisible)
                        state |= AccessibleStates.Invisible;

                    ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                    if (selectionService != null)
                    {
                        state |= (selectionService.GetComponentSelected(this.connectorHitInfo.SelectableObject)) ? AccessibleStates.Selected : AccessibleStates.Selectable;
                        state |= (selectionService.PrimarySelection == this.connectorHitInfo.SelectableObject) ? AccessibleStates.Focused : AccessibleStates.Focusable;
                    }

                    return state;
                }
            }

            public override void DoDefaultAction()
            {
                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                    selectionService.SetSelectedComponents(new object[] { this.connectorHitInfo.SelectableObject }, SelectionTypes.Replace);
                else
                    base.DoDefaultAction();
            }

            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                if (navdir == AccessibleNavigation.FirstChild || navdir == AccessibleNavigation.LastChild)
                    return base.Navigate(navdir);

                DesignerNavigationDirection navigate = default(DesignerNavigationDirection);
                if (navdir == AccessibleNavigation.Left)
                    navigate = DesignerNavigationDirection.Left;
                else if (navdir == AccessibleNavigation.Right)
                    navigate = DesignerNavigationDirection.Right;
                else if (navdir == AccessibleNavigation.Up || navdir == AccessibleNavigation.Previous)
                    navigate = DesignerNavigationDirection.Up;
                else if (navdir == AccessibleNavigation.Down || navdir == AccessibleNavigation.Next)
                    navigate = DesignerNavigationDirection.Down;

                object nextSelectableObj = ((CompositeActivityDesigner)this.connectorHitInfo.AssociatedDesigner).GetNextSelectableObject(this.connectorHitInfo, navigate);
                if (nextSelectableObj is ConnectorHitTestInfo)
                {
                    ConnectorHitTestInfo nextConnector = nextSelectableObj as ConnectorHitTestInfo;
                    return new SequentialConnectorAccessibleObject(nextConnector.AssociatedDesigner as SequentialActivityDesigner, nextConnector.MapToIndex());
                }
                else if (nextSelectableObj is Activity)
                {
                    ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(nextSelectableObj as Activity);
                    if (activityDesigner != null)
                        return activityDesigner.AccessibilityObject;
                }

                return base.Navigate(navdir);
            }

            public override void Select(AccessibleSelection flags)
            {
                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                {
                    if (((flags & AccessibleSelection.TakeFocus) > 0) || ((flags & AccessibleSelection.TakeSelection) > 0))
                        selectionService.SetSelectedComponents(new object[] { this.connectorHitInfo.SelectableObject }, SelectionTypes.Replace);
                    else if ((flags & AccessibleSelection.AddSelection) > 0)
                        selectionService.SetSelectedComponents(new object[] { this.connectorHitInfo.SelectableObject }, SelectionTypes.Add);
                    else if ((flags & AccessibleSelection.RemoveSelection) > 0)
                        selectionService.SetSelectedComponents(new object[] { this.connectorHitInfo.SelectableObject }, SelectionTypes.Remove);
                }
            }

            private object GetService(Type serviceType)
            {
                if (this.connectorHitInfo.AssociatedDesigner != null && this.connectorHitInfo.AssociatedDesigner.Activity.Site != null)
                    return this.connectorHitInfo.AssociatedDesigner.Activity.Site.GetService(serviceType);
                else
                    return null;
            }
        }
        #endregion
    }
    #endregion
}
