namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.CodeDom;
    using System.Drawing;
    using System.Reflection;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.ComponentModel.Design.Serialization;

    #region ActivityHostDesigner Class
    /// <summary>
    /// Base class used to display inlined workflows. This class is for internal use.
    /// 

    internal abstract class ActivityHostDesigner : SequentialActivityDesigner
    {
        #region Fields
        private IWorkflowRootDesigner containedRootDesigner;
        private ContainedDesignerLoader containedLoader;
        private ContainedDesignSurface containedDesignSurface;
        private MemoryStream lastInvokedWorkflowState;
        #endregion

        #region Constructor
        public ActivityHostDesigner()
        {
        }
        #endregion

        #region Properties

        #region Public Properties
        public override ReadOnlyCollection<ActivityDesigner> ContainedDesigners
        {
            get
            {
                List<ActivityDesigner> containedDesigners = new List<ActivityDesigner>();
                if (this.containedRootDesigner != null)
                    containedDesigners.Add((ActivityDesigner)this.containedRootDesigner);

                return containedDesigners.AsReadOnly();
            }
        }
        #endregion

        #region Protected Properties
        protected void RefreshHostedActivity()
        {
            // get currest state
            if (this.containedRootDesigner != null)
            {
                this.lastInvokedWorkflowState = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(this.lastInvokedWorkflowState);
                SaveViewState(writer);
            }

            //Refresh the designer
            this.containedRootDesigner = LoadHostedWorkflow();
            if (this.lastInvokedWorkflowState != null)
            {
                // set state
                this.lastInvokedWorkflowState.Position = 0;
                BinaryReader reader = new BinaryReader(this.lastInvokedWorkflowState);
                try
                {
                    LoadViewState(reader);
                }
                catch
                {
                    // tried to apply serialized state to the wrong hosted activity... ignore 
                }
            }

            PerformLayout();
        }

        protected abstract Activity RootActivity
        {
            get;
        }
        #endregion

        #region Private Properties
        #endregion

        #endregion

        #region Methods

        #region Public Methods
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            return false;
        }

        public override void InsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
        }

        public override bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            return false;
        }

        public override void RemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
        }
        #endregion

        #region Protected Methods
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    //Both of these objects are disposed in design surface dispose.
                    this.containedRootDesigner = null;
                    this.containedLoader = null;
                    if (this.containedDesignSurface != null)
                    {
                        this.containedDesignSurface.Dispose();
                        this.containedDesignSurface = null;
                    }
                    if (this.lastInvokedWorkflowState != null)
                    {
                        this.lastInvokedWorkflowState.Close();
                        this.lastInvokedWorkflowState = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override void SaveViewState(BinaryWriter writer)
        {
            base.SaveViewState(writer);

            if (this.containedDesignSurface != null)
            {
                // mark persistence for invoked workflow
                writer.Write(true);

                IDesignerHost designerHost = this.containedDesignSurface.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (designerHost == null)
                    throw new Exception(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

                DesignerHelpers.SerializeDesignerStates(designerHost, writer);
            }
            else
            {
                // no persistnce data for invoked workflow
                writer.Write(false);
            }
        }

        protected override void LoadViewState(BinaryReader reader)
        {
            base.LoadViewState(reader);

            if (reader.ReadBoolean())
            {
                //verify the hosted surface and designer are loaded
                if (this.containedDesignSurface == null)
                    this.containedRootDesigner = LoadHostedWorkflow();

                if (this.containedDesignSurface != null)
                {
                    IDesignerHost designerHost = this.containedDesignSurface.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (designerHost == null)
                        throw new Exception(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));
                    DesignerHelpers.DeserializeDesignerStates(designerHost, reader);
                }
            }
        }
        #endregion

        #region Private Methods
        private IWorkflowRootDesigner LoadHostedWorkflow()
        {
            if (RootActivity != null)
            {
                this.containedLoader = new ContainedDesignerLoader(RootActivity as Activity);
                this.containedDesignSurface = new ContainedDesignSurface(Activity.Site, this);
                if (this.containedDesignSurface.IsLoaded == false)
                    this.containedDesignSurface.BeginLoad(this.containedLoader);
                return ActivityDesigner.GetSafeRootDesigner(this.containedDesignSurface.GetService(typeof(IDesignerHost)) as IServiceProvider) as IWorkflowRootDesigner;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #endregion

        #region DesignSurface Hosting Related Functionality

        #region Class ContainedDesignSurface
        private class ContainedDesignSurface : DesignSurface
        {
            #region Members and Constructor
            private CompositeActivityDesigner parentDesigner;

            internal ContainedDesignSurface(IServiceProvider parentServiceProvider, CompositeActivityDesigner parentDesigner)
                : base(parentServiceProvider)
            {
                this.parentDesigner = parentDesigner;

                if (ServiceContainer != null)
                    ServiceContainer.RemoveService(typeof(ISelectionService));
            }
            #endregion

            #region Properties and Methods
            //internal CompositeActivityDesigner ContainingDesigner
            //{
            //    get
            //    {
            //        return this.parentDesigner;
            //    }
            //}

            protected override IDesigner CreateDesigner(IComponent component, bool rootDesigner)
            {
                IDesigner designer = base.CreateDesigner(component, rootDesigner);
                if (rootDesigner)
                {
                    IWorkflowRootDesigner workflowRootDesigner = designer as IWorkflowRootDesigner;
                    if (workflowRootDesigner != null)
                        workflowRootDesigner.InvokingDesigner = this.parentDesigner;
                }

                return designer;
            }
            #endregion
        }
        #endregion

        #region Class ContainedDesignerLoader
        private sealed class ContainedDesignerLoader : WorkflowDesignerLoader
        {
            #region Members and Constructor
            private Activity rootActivity;

            internal ContainedDesignerLoader(Activity rootActivity)
            {
                this.rootActivity = rootActivity;
            }
            #endregion

            #region Methods and Properties

            protected override void Initialize()
            {
                base.Initialize();

                ServiceCreatorCallback callback = new ServiceCreatorCallback(OnCreateService);
                LoaderHost.RemoveService(typeof(IReferenceService));
                LoaderHost.AddService(typeof(IReferenceService), callback);
            }

            private object OnCreateService(IServiceContainer container, Type serviceType)
            {
                object createdService = null;

                if (serviceType == typeof(IReferenceService))
                    createdService = new ReferenceService(LoaderHost);

                return createdService;
            }

            public override string FileName
            {
                get
                {
                    return String.Empty;
                }
            }

            public override TextReader GetFileReader(string filePath)
            {
                return null;
            }

            public override TextWriter GetFileWriter(string filePath)
            {
                return null;
            }

            public override void ForceReload()
            {
            }

            protected override void PerformLoad(IDesignerSerializationManager serializationManager)
            {
                IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
                if (this.rootActivity != null && this.rootActivity is Activity)
                {
                    AddActivityToDesigner(this.rootActivity as Activity);
                    SetBaseComponentClassName(this.rootActivity.GetType().FullName);
                }
            }

            public override void Flush()
            {
            }
            #endregion
        }
        #endregion

        #endregion
    }
    #endregion

    #region Class InvokeWorkflowDesigner
    [ActivityDesignerTheme(typeof(InvokeWorkflowDesignerTheme))]
    internal sealed class InvokeWorkflowDesigner : ActivityHostDesigner
    {
        #region members and initializers
        internal const string InvokeWorkflowRef = "System.Workflow.Activities.InvokeWorkflowActivity, " + AssemblyRef.ActivitiesAssemblyRef;
        private static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "TargetWorkflow", "Invoking", "ParameterBindings" });

        private Type targetWorkflowType = null;

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            HelpText = DR.GetString(DR.SpecifyTargetWorkflow);
            RefreshTargetWorkflowType();
        }
        #endregion

        #region Properties and Methods
        protected override Activity RootActivity
        {
            get
            {
                if (this.targetWorkflowType == null || this.targetWorkflowType is DesignTimeType)
                    return null;
                else
                    return Activator.CreateInstance(this.targetWorkflowType) as Activity;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            if (this.targetWorkflowType != null)
            {
                try
                {
                    foreach (PropertyInfo parameterProperty in this.targetWorkflowType.GetProperties())
                    {
                        if (!parameterProperty.CanWrite)
                            continue;

                        if (parameterProperty.DeclaringType == typeof(DependencyObject) ||
                            parameterProperty.DeclaringType == typeof(Activity) ||
                            parameterProperty.DeclaringType == typeof(CompositeActivity) ||
                            ((parameterProperty.DeclaringType == Type.GetType(DesignerHelpers.SequentialWorkflowTypeRef) ||
                             parameterProperty.DeclaringType == Type.GetType(DesignerHelpers.StateMachineWorkflowTypeRef)) &&
                             string.Equals(parameterProperty.Name, "DynamicUpdateCondition", StringComparison.Ordinal)))
                            continue;

                        bool ignoreProperty = false;
                        Type dependencyObjectType = this.targetWorkflowType;
                        while (dependencyObjectType != null && dependencyObjectType is DesignTimeType)
                            dependencyObjectType = dependencyObjectType.BaseType;

                        if (dependencyObjectType != null)
                        {
                            foreach (DependencyProperty dependencyProperty in DependencyProperty.FromType(dependencyObjectType))
                            {
                                if (dependencyProperty.Name == parameterProperty.Name && dependencyProperty.DefaultMetadata.IsMetaProperty)
                                {
                                    ignoreProperty = true;
                                    break;
                                }
                            }
                        }

                        if (!ignoreProperty)
                        {
                            PropertyDescriptor prop = new ParameterInfoBasedPropertyDescriptor(Type.GetType(InvokeWorkflowRef), parameterProperty.Name, parameterProperty.PropertyType, ReservedParameterNames.Contains(parameterProperty.Name), DesignOnlyAttribute.Yes);
                            properties[prop.Name] = prop;
                        }
                    }
                }
                catch (MissingMemberException)
                {
                    // targetServiceType has no default CTor, ignore
                }
            }
        }
        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);

            if (e.Member != null)
            {
                if (string.Equals(e.Member.Name, "TargetWorkflow", StringComparison.Ordinal))
                {
                    //We need to clear the parameter bindings if target workflow type changes
                    if (e.OldValue != e.NewValue && Activity != null)
                    {
                        PropertyInfo parameterProperty = Activity.GetType().GetProperty("ParameterBindings", BindingFlags.Instance | BindingFlags.Public);
                        if (parameterProperty != null)
                        {
                            WorkflowParameterBindingCollection bindings = parameterProperty.GetValue(Activity, null) as WorkflowParameterBindingCollection;
                            if (bindings != null)
                                bindings.Clear();
                        }
                    }

                    RefreshTargetWorkflowType();
                }
            }
        }
        #endregion

        #region Helpers
        internal void RefreshTargetWorkflowType()
        {
            if (Activity == null)
                return;

            ITypeFilterProvider typeFilterProvider = Activity as ITypeFilterProvider;
            Type workflowType = Activity.GetType().InvokeMember("TargetWorkflow", BindingFlags.GetProperty | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.ExactBinding, null, Activity, new object[] { }, CultureInfo.InvariantCulture) as Type;
            if (workflowType != null && typeFilterProvider.CanFilterType(workflowType, false))
            {
                ITypeProvider typeProvider = (ITypeProvider)GetService(typeof(ITypeProvider));
                if (typeProvider != null)
                {
                    Type updatedWorkflowType = null;
                    if (workflowType.Assembly == null && typeProvider.LocalAssembly != null)
                        updatedWorkflowType = typeProvider.LocalAssembly.GetType(workflowType.FullName);
                    else
                        updatedWorkflowType = typeProvider.GetType(workflowType.FullName);

                    if (updatedWorkflowType != null)
                        workflowType = updatedWorkflowType;
                }
            }
            else
            {
                workflowType = null;
            }

            if (this.targetWorkflowType != workflowType)
            {
                this.targetWorkflowType = workflowType;
                RefreshHostedActivity();

                if (this.targetWorkflowType is DesignTimeType)
                    HelpText = DR.GetString(DR.BuildTargetWorkflow);
                else
                    HelpText = DR.GetString(DR.SpecifyTargetWorkflow);
            }

            TypeDescriptor.Refresh(Activity);
        }
        #endregion
    }
    #endregion

    #region InvokeWorkflowDesignerTheme
    internal sealed class InvokeWorkflowDesignerTheme : CompositeDesignerTheme
    {
        public InvokeWorkflowDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
