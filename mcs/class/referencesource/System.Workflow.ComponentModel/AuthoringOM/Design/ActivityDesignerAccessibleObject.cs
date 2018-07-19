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

    #region Class ActivityDesignerAccessibleObject
    /// <summary>
    /// Accessibility object class for the ActivityDesigner
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDesignerAccessibleObject : AccessibleObject
    {
        private ActivityDesigner activityDesigner;

        /// <summary>
        /// Constructs the accessibility class for ActivityDesigner
        /// </summary>
        /// <param name="activityDesigner">ActivityDesigner associated with accessiblity object</param>
        public ActivityDesignerAccessibleObject(ActivityDesigner activityDesigner)
        {
            if (activityDesigner == null)
                throw new ArgumentNullException("activityDesigner");
            if (activityDesigner.Activity == null)
                throw new ArgumentException(DR.GetString(DR.DesignerNotInitialized), "activityDesigner");

            this.activityDesigner = activityDesigner;
        }

        public override Rectangle Bounds
        {
            get
            {
                return this.activityDesigner.InternalRectangleToScreen(this.activityDesigner.Bounds);
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
                return DR.GetString(DR.ActivityDesignerAccessibleDescription, this.activityDesigner.Activity.GetType().Name);
            }
        }

        public override string Help
        {
            get
            {
                return DR.GetString(DR.ActivityDesignerAccessibleHelp, this.activityDesigner.Activity.GetType().Name);
            }
        }

        public override string Name
        {
            get
            {
                Activity activity = this.activityDesigner.Activity as Activity;
                if (activity != null)
                {
                    if (TypeDescriptor.GetProperties(activity)["TypeName"] != null)
                        return TypeDescriptor.GetProperties(activity)["TypeName"].GetValue(activity) as string;
                    else if (!string.IsNullOrEmpty(activity.QualifiedName))
                        return activity.QualifiedName;
                    else
                        return activity.GetType().FullName;
                }
                else
                {
                    return base.Name;
                }
            }

            set
            {
                //We do not allow setting ID programatically
            }
        }

        public override AccessibleObject Parent
        {
            get
            {
                CompositeActivityDesigner compositeDesigner = this.activityDesigner.ParentDesigner;
                return (compositeDesigner != null) ? compositeDesigner.AccessibilityObject : null;
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
                AccessibleStates state = (this.activityDesigner.IsSelected) ? AccessibleStates.Selected : AccessibleStates.Selectable;
                state |= AccessibleStates.MultiSelectable;
                state |= (this.activityDesigner.IsPrimarySelection) ? AccessibleStates.Focused : AccessibleStates.Focusable;

                if (this.activityDesigner.IsLocked)
                    state |= AccessibleStates.ReadOnly;
                else
                    state |= AccessibleStates.Moveable;

                if (!this.activityDesigner.IsVisible)
                    state |= AccessibleStates.Invisible;

                return state;
            }
        }

        public override void DoDefaultAction()
        {
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
                selectionService.SetSelectedComponents(new object[] { this.activityDesigner.Activity }, SelectionTypes.Replace);
            else
                base.DoDefaultAction();
        }

        public override AccessibleObject Navigate(AccessibleNavigation navdir)
        {
            if (navdir == AccessibleNavigation.FirstChild)
            {
                return GetChild(0);
            }
            else if (navdir == AccessibleNavigation.LastChild)
            {
                return GetChild(GetChildCount() - 1);
            }
            else
            {
                CompositeActivityDesigner compositeDesigner = this.activityDesigner.ParentDesigner;
                if (compositeDesigner != null)
                {
                    DesignerNavigationDirection navigate = default(DesignerNavigationDirection);
                    if (navdir == AccessibleNavigation.Left)
                        navigate = DesignerNavigationDirection.Left;
                    else if (navdir == AccessibleNavigation.Right)
                        navigate = DesignerNavigationDirection.Right;
                    else if (navdir == AccessibleNavigation.Up || navdir == AccessibleNavigation.Previous)
                        navigate = DesignerNavigationDirection.Up;
                    else if (navdir == AccessibleNavigation.Down || navdir == AccessibleNavigation.Next)
                        navigate = DesignerNavigationDirection.Down;

                    ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(compositeDesigner.GetNextSelectableObject(this.activityDesigner.Activity, navigate) as Activity);
                    if (activityDesigner != null)
                        return activityDesigner.AccessibilityObject;
                }
            }

            return base.Navigate(navdir);
        }

        public override void Select(AccessibleSelection flags)
        {
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
            {
                if (((flags & AccessibleSelection.TakeFocus) > 0) || ((flags & AccessibleSelection.TakeSelection) > 0))
                    selectionService.SetSelectedComponents(new object[] { this.activityDesigner.Activity }, SelectionTypes.Replace);
                else if ((flags & AccessibleSelection.AddSelection) > 0)
                    selectionService.SetSelectedComponents(new object[] { this.activityDesigner.Activity }, SelectionTypes.Add);
                else if ((flags & AccessibleSelection.RemoveSelection) > 0)
                    selectionService.SetSelectedComponents(new object[] { this.activityDesigner.Activity }, SelectionTypes.Remove);
            }
        }

        protected object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            if (this.ActivityDesigner.Activity != null && this.ActivityDesigner.Activity.Site != null)
                return this.ActivityDesigner.Activity.Site.GetService(serviceType);
            else
                return null;
        }

        protected ActivityDesigner ActivityDesigner
        {
            get
            {
                return this.activityDesigner;
            }
        }
    }
    #endregion

}
