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



    #region Class SecondaryViewProvider
    internal static class SecondaryViewProvider
    {
        private const string EventHandlersRef = "System.Workflow.Activities.EventHandlersActivity, " + AssemblyRef.ActivitiesAssemblyRef;
        private const string EventHandlingScopeRef = "System.Workflow.Activities.EventHandlingScopeActivity, " + AssemblyRef.ActivitiesAssemblyRef;

        internal static ReadOnlyCollection<DesignerView> GetViews(StructuredCompositeActivityDesigner designer)
        {
            Debug.Assert(designer.Activity != null);
            if (designer.Activity == null)
                throw new ArgumentException("Component can not be null!");

            bool locked = !designer.IsEditable;

            //Get all the possible view types
            List<object[]> viewTypes = new List<object[]>();

            string displayName = ActivityToolboxItem.GetToolboxDisplayName(designer.Activity.GetType());
            viewTypes.Add(new object[] { designer.Activity.GetType(), DR.GetString(DR.ViewActivity, displayName) });

            //Only show the views in workflow designer or for nested activities
            if (designer.Activity.Site != null)
            {
                WorkflowDesignerLoader loader = designer.Activity.Site.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                Type activityType = designer.Activity.GetType();

                if (loader == null ||
                    (typeof(CompositeActivity).IsAssignableFrom(activityType) &&
                    (!locked || FindActivity(designer, typeof(CancellationHandlerActivity)) != null)))
                    viewTypes.Add(new object[] { typeof(CancellationHandlerActivity), DR.GetString(DR.ViewCancelHandler) });

                if (loader == null ||
                    (typeof(CompositeActivity).IsAssignableFrom(activityType) &&
                    (!locked || FindActivity(designer, typeof(FaultHandlersActivity)) != null)))
                    viewTypes.Add(new object[] { typeof(FaultHandlersActivity), DR.GetString(DR.ViewExceptions) });

                if (loader == null ||
                    (designer.Activity is ICompensatableActivity && typeof(CompositeActivity).IsAssignableFrom(activityType) &&
                    (!locked || FindActivity(designer, typeof(CompensationHandlerActivity)) != null)))
                    viewTypes.Add(new object[] { typeof(CompensationHandlerActivity), DR.GetString(DR.ViewCompensation) });

                if (loader == null ||
                    (Type.GetType(EventHandlingScopeRef).IsAssignableFrom(activityType) &&
                    (!locked || FindActivity(designer, Type.GetType(EventHandlersRef)) != null)))
                    viewTypes.Add(new object[] { Type.GetType(EventHandlersRef), DR.GetString(DR.ViewEvents) });
            }

            //Now go through the view types and create views
            List<DesignerView> views = new List<DesignerView>();
            for (int i = 0; i < viewTypes.Count; i++)
            {
                Type viewType = viewTypes[i][0] as Type;
                DesignerView view = new SecondaryView(designer, i + 1, viewTypes[i][1] as string, viewType);
                views.Add(view);
            }

            return views.AsReadOnly();
        }

        internal static IList<Type> GetActivityTypes(StructuredCompositeActivityDesigner designer)
        {
            List<Type> activityTypes = new List<Type>();
            ReadOnlyCollection<DesignerView> views = designer.Views;
            for (int i = 1; i < views.Count; i++)
            {
                Type activityType = views[i].UserData[SecondaryView.UserDataKey_ActivityType] as Type;
                activityTypes.Add(activityType);
            }
            return activityTypes.AsReadOnly();
        }

        internal static void OnViewRemoved(StructuredCompositeActivityDesigner designer, Type viewTypeRemoved)
        {
            ReadOnlyCollection<DesignerView> views = designer.Views;
            for (int i = 1; i < views.Count; i++)
            {
                Type activityType = views[i].UserData[SecondaryView.UserDataKey_ActivityType] as Type;
                if (viewTypeRemoved == activityType)
                    views[i].UserData[SecondaryView.UserDataKey_Designer] = null;
            }
        }

        internal static Activity FindActivity(StructuredCompositeActivityDesigner designer, Type activityType)
        {
            Debug.Assert(activityType != null);
            CompositeActivity compositeActivity = designer.Activity as CompositeActivity;
            if (activityType == null || compositeActivity == null)
                return null;

            foreach (Activity activity in compositeActivity.Activities)
            {
                if (activityType.IsAssignableFrom(activity.GetType()))
                    return activity;
            }

            return null;
        }
    }
    #endregion

    #region Class SecondaryView
    internal sealed class SecondaryView : DesignerView
    {
        internal static readonly Guid UserDataKey_ActivityType = new Guid("03C4103A-D6E9-46e9-B98E-149E145EC2C9");
        internal static readonly Guid UserDataKey_Designer = new Guid("2B72C7F7-DE4A-4e32-8EB4-9E1ED1C5E84E");

        private StructuredCompositeActivityDesigner parentDesigner;

        internal SecondaryView(StructuredCompositeActivityDesigner parentDesigner, int id, string text, Type activityType)
            : base(id, text, ActivityToolboxItem.GetToolboxImage(activityType))
        {
            this.parentDesigner = parentDesigner;
            UserData[UserDataKey_ActivityType] = activityType;
            if (this.parentDesigner.Activity.GetType() == activityType)
                UserData[UserDataKey_Designer] = this.parentDesigner;
        }

        public override ActivityDesigner AssociatedDesigner
        {
            get
            {
                ActivityDesigner mappedDesigner = UserData[UserDataKey_Designer] as ActivityDesigner;
                if (mappedDesigner == null)
                {
                    Type activityType = UserData[UserDataKey_ActivityType] as Type;
                    if (activityType != null)
                    {
                        if (activityType != this.parentDesigner.Activity.GetType())
                        {
                            Activity activity = SecondaryViewProvider.FindActivity(this.parentDesigner, activityType);
                            if (activity != null)
                                mappedDesigner = ActivityDesigner.GetDesigner(activity);
                        }
                        else
                        {
                            mappedDesigner = this.parentDesigner;
                        }

                        UserData[UserDataKey_Designer] = mappedDesigner;
                    }
                }

                return mappedDesigner;
            }
        }

        public override void OnActivate()
        {
            if (AssociatedDesigner != null)
                return;

            Type activityType = UserData[UserDataKey_ActivityType] as Type;
            Debug.Assert(activityType != null);

            CompositeActivity parentActivity = this.parentDesigner.Activity as CompositeActivity;
            if (activityType == null || parentActivity == null || !this.parentDesigner.IsEditable)
                return;

            Activity activity = Activator.CreateInstance(activityType) as Activity;
            try
            {
                CompositeActivityDesigner.InsertActivities(this.parentDesigner, new HitTestInfo(this.parentDesigner, HitTestLocations.Designer), new List<Activity>(new Activity[] { activity }).AsReadOnly(), SR.GetString(SR.AddingImplicitActivity));
            }
            catch (Exception e)
            {
                if (e != CheckoutException.Canceled)
                {
                    IUIService uiService = this.parentDesigner.Activity.Site.GetService(typeof(IUIService)) as IUIService;
                    if (uiService != null)
                        uiService.ShowError(e.Message);
                }
            }

            ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
            UserData[UserDataKey_Designer] = designer;
        }
    }
    #endregion
}
