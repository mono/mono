namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.ComponentModel.Design.Serialization;
    using System.IO;
    using System.Windows.Forms;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Xml;
    using System.Workflow.ComponentModel.Compiler;
    using System.Collections.ObjectModel;

    #region Class WorkflowDesignerLoader
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class WorkflowDesignerLoader : BasicDesignerLoader
    {
        #region Members, Construction and Dispose
        internal const string DesignerLayoutFileExtension = ".layout";

        private CustomActivityDesignerAdapter customActivityDesignerAdapter;
        private WorkflowDesignerEventsCoordinator eventsCoordinator;
        private Hashtable createdServices = null;
        private bool loadingDesignerLayout = false;

        static WorkflowDesignerLoader()
        {
            ComponentDispenser.RegisterComponentExtenders(typeof(CustomActivityDesignerAdapter), new IExtenderProvider[] { new CustomActivityPropertyExtender() });
        }

        protected override void Initialize()
        {
            base.Initialize();

            //PLEASE NOTE THE FOLLOWING CODE IS ADDED TO MAKE THE SPECIFIC DESIGNER TYPE INTERNAL
            //This is a work around for invoke workflow so that the ActivityHostDesignerType does not become public
            //Please refer to file WorkflowInlining.cs
            Type invokeWorkflowType = Type.GetType(InvokeWorkflowDesigner.InvokeWorkflowRef);
            if (invokeWorkflowType != null)
                TypeDescriptor.AddAttributes(invokeWorkflowType, new DesignerAttribute(typeof(InvokeWorkflowDesigner), typeof(IDesigner)));

            //Add all the services, it is important to make sure that if user pushes the services then we honor
            //those services
            LoaderHost.AddService(typeof(WorkflowDesignerLoader), this);

            ServiceCreatorCallback callback = new ServiceCreatorCallback(OnCreateService);
            if (LoaderHost.GetService(typeof(IWorkflowCompilerOptionsService)) == null)
                LoaderHost.AddService(typeof(IWorkflowCompilerOptionsService), callback);

            if (LoaderHost.GetService(typeof(IIdentifierCreationService)) == null)
                LoaderHost.AddService(typeof(IIdentifierCreationService), callback);

            if (LoaderHost.GetService(typeof(ComponentSerializationService)) == null)
                LoaderHost.AddService(typeof(ComponentSerializationService), callback);

            LoaderHost.RemoveService(typeof(IReferenceService));
            if (LoaderHost.GetService(typeof(IReferenceService)) == null)
                LoaderHost.AddService(typeof(IReferenceService), callback);

            if (LoaderHost.GetService(typeof(IDesignerVerbProviderService)) == null)
                LoaderHost.AddService(typeof(IDesignerVerbProviderService), callback);

            //Add all the extenders, the extenders are responsible to add the extended properties which are not
            //actual properties on activity
            IExtenderProviderService extenderProviderService = GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
            if (extenderProviderService != null)
            {
                foreach (IExtenderProvider extender in ComponentDispenser.Extenders)
                    extenderProviderService.AddExtenderProvider(extender);
            }

            this.customActivityDesignerAdapter = new CustomActivityDesignerAdapter(LoaderHost);
        }

        public override void Dispose()
        {
            if (this.eventsCoordinator != null)
            {
                ((IDisposable)this.eventsCoordinator).Dispose();
                this.eventsCoordinator = null;
            }

            if (this.customActivityDesignerAdapter != null)
            {
                ((IDisposable)this.customActivityDesignerAdapter).Dispose();
                this.customActivityDesignerAdapter = null;
            }

            IExtenderProviderService extenderProviderService = GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
            if (extenderProviderService != null)
            {
                foreach (IExtenderProvider extender in ComponentDispenser.Extenders)
                    extenderProviderService.RemoveExtenderProvider(extender);
            }

            if (LoaderHost != null)
            {
                if (this.createdServices != null)
                {
                    foreach (Type serviceType in this.createdServices.Keys)
                    {
                        LoaderHost.RemoveService(serviceType);
                        OnDisposeService(serviceType, this.createdServices[serviceType]);
                    }
                    this.createdServices.Clear();
                    this.createdServices = null;
                }
                LoaderHost.RemoveService(typeof(WorkflowDesignerLoader));
            }

            base.Dispose();
        }
        #endregion

        #region Public Methods and Properties
        public abstract TextReader GetFileReader(string filePath);

        public abstract TextWriter GetFileWriter(string filePath);

        public abstract string FileName { get; }

        public virtual void ForceReload()
        {
            Reload(ReloadOptions.Force);
        }

        public override void Flush()
        {
            base.Flush();
        }

        public virtual bool InDebugMode
        {
            get
            {
                return false;
            }
        }

        protected virtual TypeDescriptionProvider TargetFrameworkTypeDescriptionProvider
        {
            get
            {
                return null;
            }
        }

        void AddTargetFrameworkProvider(IComponent component)
        {
            TypeDescriptionProviderService typeDescriptionProviderService = GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
            if (typeDescriptionProviderService != null && component != null)
            {
                TypeDescriptor.AddProvider(typeDescriptionProviderService.GetProvider(component), component);
            }
        }

        public void AddActivityToDesigner(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            if (activity.Parent == null && designerHost.RootComponent == null)
            {
                string fullClassName = activity.GetValue(WorkflowMarkupSerializer.XClassProperty) as String;
                string rootSiteName = (!string.IsNullOrEmpty(fullClassName)) ? Helpers.GetClassName(fullClassName) : Helpers.GetClassName(activity.GetType().FullName);
                designerHost.Container.Add(activity, rootSiteName);
                AddTargetFrameworkProvider(activity);
            }
            else
            {
                designerHost.Container.Add(activity, activity.QualifiedName);
                AddTargetFrameworkProvider(activity);
            }

            if (activity is CompositeActivity)
            {
                foreach (Activity activity2 in Helpers.GetNestedActivities(activity as CompositeActivity))
                {
                    designerHost.Container.Add(activity2, activity2.QualifiedName);
                    AddTargetFrameworkProvider(activity2);
                }
            }
        }

        public void RemoveActivityFromDesigner(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            designerHost.DestroyComponent(activity);
            if (activity is CompositeActivity)
            {
                foreach (Activity activity2 in Helpers.GetNestedActivities(activity as CompositeActivity))
                    designerHost.DestroyComponent(activity2);
            }
        }
        #endregion

        #region Protected Methods and Properties
        protected override void PerformLoad(IDesignerSerializationManager serializationManager)
        {
        }

        protected override void PerformFlush(IDesignerSerializationManager serializationManager)
        {
            SaveDesignerLayout();
        }

        protected override void OnEndLoad(bool successful, ICollection errors)
        {
            base.OnEndLoad(successful, errors);

            if (successful)
            {
                //We initialize the events coordinator only once when loading of the designer is complete
                ActivityDesigner rootDesigner = ActivityDesigner.GetRootDesigner(LoaderHost);
                if (this.eventsCoordinator == null && (rootDesigner == null || rootDesigner.ParentDesigner == null))
                    this.eventsCoordinator = new WorkflowDesignerEventsCoordinator(LoaderHost);

                try
                {
                    this.loadingDesignerLayout = true;

                    string layoutFileName = DesignerLayoutFileName;
                    IList layoutErrors = null;
                    if (File.Exists(layoutFileName))
                        LoadDesignerLayout(out layoutErrors);
                    else if (InDebugMode || (ActivityDesigner.GetRootDesigner(LoaderHost) != null && ActivityDesigner.GetRootDesigner(LoaderHost).ParentDesigner != null))
                        LoadDesignerLayoutFromResource(out layoutErrors);

                    if (layoutErrors != null)
                    {
                        if (errors == null)
                            errors = new ArrayList();
                        IList designerErrors = errors as IList;
                        if (designerErrors != null)
                        {
                            foreach (object layoutError in layoutErrors)
                                designerErrors.Add(layoutError);
                        }
                    }
                }
                finally
                {
                    this.loadingDesignerLayout = false;
                }
            }
        }

        protected void LoadDesignerLayoutFromResource(Type type, string manifestResourceName, out IList errors)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (manifestResourceName == null)
                throw new ArgumentNullException("manifestResourceName");

            if (manifestResourceName.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Error_ParameterCannotBeEmpty), "manifestResourceName");

            errors = new ArrayList();

            Stream stream = type.Module.Assembly.GetManifestResourceStream(type, manifestResourceName);
            if (stream == null)
                stream = type.Module.Assembly.GetManifestResourceStream(manifestResourceName);

            if (stream != null)
            {
                using (XmlReader layoutReader = XmlReader.Create(stream))
                {
                    if (layoutReader != null)
                        LoadDesignerLayout(layoutReader, out errors);
                }
            }
        }

        protected void LoadDesignerLayout(XmlReader layoutReader, out IList layoutLoadErrors)
        {
            if (layoutReader == null)
                throw new ArgumentNullException("layoutReader");

            ArrayList errors = new ArrayList();
            layoutLoadErrors = errors;

            ActivityDesigner rootDesigner = null;
            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost != null && designerHost.RootComponent != null)
                rootDesigner = designerHost.GetDesigner(designerHost.RootComponent) as ActivityDesigner;

            if (rootDesigner != null)
            {
                if (rootDesigner.SupportsLayoutPersistence)
                {
                    DesignerSerializationManager serializationManager = new DesignerSerializationManager(LoaderHost);
                    using (serializationManager.CreateSession())
                    {
                        WorkflowMarkupSerializationManager layoutSerializationManager = new WorkflowMarkupSerializationManager(serializationManager);
                        layoutSerializationManager.AddSerializationProvider(new ActivityDesignerLayoutSerializerProvider());

                        try
                        {
                            new WorkflowMarkupSerializer().Deserialize(layoutSerializationManager, layoutReader);
                        }
                        catch (Exception e)
                        {
                            errors.Add(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_LayoutDeserialization), e));
                        }
                        finally
                        {
                            if (serializationManager.Errors != null)
                                errors.AddRange(serializationManager.Errors);
                        }
                    }
                }
                else
                {
                    errors.Add(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_LayoutSerializationPersistenceSupport)));
                }
            }
            else
            {
                errors.Add(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_LayoutSerializationRootDesignerNotFound)));
            }
        }

        protected void SaveDesignerLayout(XmlWriter layoutWriter, ActivityDesigner rootDesigner, out IList layoutSaveErrors)
        {
            if (layoutWriter == null)
                throw new ArgumentNullException("layoutWriter");

            if (rootDesigner == null)
                throw new ArgumentNullException("rootDesigner");

            ArrayList errors = new ArrayList();
            layoutSaveErrors = errors;

            if (rootDesigner.SupportsLayoutPersistence)
            {
                DesignerSerializationManager serializationManager = new DesignerSerializationManager(LoaderHost);
                using (serializationManager.CreateSession())
                {
                    WorkflowMarkupSerializationManager layoutSerializationManager = new WorkflowMarkupSerializationManager(serializationManager);
                    layoutSerializationManager.AddSerializationProvider(new ActivityDesignerLayoutSerializerProvider());

                    try
                    {
                        new WorkflowMarkupSerializer().Serialize(layoutSerializationManager, layoutWriter, rootDesigner);
                    }
                    catch (Exception e)
                    {
                        errors.Add(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_LayoutSerialization), e));
                    }
                    finally
                    {
                        if (serializationManager.Errors != null)
                            errors.AddRange(serializationManager.Errors);
                    }
                }
            }
            else
            {
                errors.Add(new WorkflowMarkupSerializationException(SR.GetString(SR.Error_LayoutSerializationPersistenceSupport)));
            }
        }
        #endregion

        #region Private Methods and Properties
        internal void SetModified(bool modified)
        {
            if (LoaderHost != null && !LoaderHost.Loading && !this.loadingDesignerLayout)
            {
                OnModifying();
                base.Modified = modified;
            }
        }

        internal static void AddActivityToDesigner(IServiceProvider serviceProvider, Activity activity)
        {
            WorkflowDesignerLoader workflowLoader = serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (workflowLoader == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(WorkflowDesignerLoader).FullName));

            workflowLoader.AddActivityToDesigner(activity);
        }

        internal static void RemoveActivityFromDesigner(IServiceProvider serviceProvider, Activity activity)
        {
            WorkflowDesignerLoader workflowLoader = serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (workflowLoader == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(WorkflowDesignerLoader).FullName));

            workflowLoader.RemoveActivityFromDesigner(activity);
        }

        private object OnCreateService(IServiceContainer container, Type serviceType)
        {
            object createdService = null;

            if (serviceType == typeof(ComponentSerializationService))
                createdService = new XomlComponentSerializationService(LoaderHost);
            else if (serviceType == typeof(IReferenceService))
                createdService = new ReferenceService(LoaderHost);
            else if (serviceType == typeof(IIdentifierCreationService))
                createdService = new IdentifierCreationService(container, this);
            else if (serviceType == typeof(IWorkflowCompilerOptionsService))
                createdService = new WorkflowCompilerOptionsService();
            else if (serviceType == typeof(IDesignerVerbProviderService))
                createdService = new DesignerVerbProviderService();

            if (createdService != null)
            {
                if (this.createdServices == null)
                    this.createdServices = new Hashtable();
                object existingService = this.createdServices[serviceType];
                this.createdServices[serviceType] = createdService;
                if (existingService != null)
                {
                    OnDisposeService(serviceType, existingService);
                }
            }

            return createdService;
        }
        private void OnDisposeService(Type serviceType, object service)
        {
            if (serviceType == typeof(IReferenceService))
            {
                ReferenceService refService = service as ReferenceService;
                if (refService != null)
                {
                    refService.Dispose();
                }
            }
        }

        private void LoadDesignerLayoutFromResource(out IList layoutErrors)
        {
            layoutErrors = null;

            IWorkflowRootDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(LoaderHost);
            if (rootDesigner == null || !rootDesigner.SupportsLayoutPersistence)
                return;

            Type rootActivityType = rootDesigner.Component.GetType();
            string resourceName = rootActivityType.Name + WorkflowDesignerLoader.DesignerLayoutFileExtension;

            LoadDesignerLayoutFromResource(rootActivityType, resourceName, out layoutErrors);
        }

        private void LoadDesignerLayout(out IList layoutErrors)
        {
            layoutErrors = null;

            string layoutFileName = DesignerLayoutFileName;
            IWorkflowRootDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(LoaderHost);
            if (rootDesigner == null || !rootDesigner.SupportsLayoutPersistence || !File.Exists(layoutFileName))
                return;

            using (TextReader layoutReader = GetFileReader(layoutFileName))
            {
                if (layoutReader != null)
                {
                    using (XmlReader xmlReader = XmlReader.Create(layoutReader))
                        LoadDesignerLayout(xmlReader, out layoutErrors);
                }
            }
        }

        private void SaveDesignerLayout()
        {
            string layoutFileName = DesignerLayoutFileName;
            ActivityDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(LoaderHost);
            if (String.IsNullOrEmpty(layoutFileName) || rootDesigner == null || !rootDesigner.SupportsLayoutPersistence)
                return;

            using (TextWriter layoutWriter = GetFileWriter(layoutFileName))
            {
                if (layoutWriter != null)
                {
                    IList layoutErrors = null;
                    using (XmlWriter xmlWriter = Helpers.CreateXmlWriter(layoutWriter))
                        SaveDesignerLayout(xmlWriter, rootDesigner, out layoutErrors);
                }
            }
        }

        private string DesignerLayoutFileName
        {
            get
            {
                string layoutFileName = FileName;
                if (!String.IsNullOrEmpty(layoutFileName))
                {
                    layoutFileName = Path.Combine(Path.GetDirectoryName(layoutFileName), Path.GetFileNameWithoutExtension(layoutFileName));
                    layoutFileName += WorkflowDesignerLoader.DesignerLayoutFileExtension;
                }
                return layoutFileName;
            }
        }
        #endregion
    }
    #endregion

    #region Class DesignerVerbProviderService
    internal sealed class DesignerVerbProviderService : IDesignerVerbProviderService
    {
        private List<IDesignerVerbProvider> designerVerbProviders = new List<IDesignerVerbProvider>();

        public DesignerVerbProviderService()
        {
            ((IDesignerVerbProviderService)this).AddVerbProvider(new FreeFormDesignerVerbProvider());
        }

        #region IDesignerVerbProviderService Implementation
        void IDesignerVerbProviderService.AddVerbProvider(IDesignerVerbProvider verbProvider)
        {
            if (!this.designerVerbProviders.Contains(verbProvider))
                this.designerVerbProviders.Add(verbProvider);
        }

        void IDesignerVerbProviderService.RemoveVerbProvider(IDesignerVerbProvider verbProvider)
        {
            this.designerVerbProviders.Remove(verbProvider);
        }

        ReadOnlyCollection<IDesignerVerbProvider> IDesignerVerbProviderService.VerbProviders
        {
            get
            {
                return this.designerVerbProviders.AsReadOnly();
            }
        }
        #endregion
    }
    #endregion

    #region Class WorkflowDesignerEventsCoordinator
    //THIS CLASS IS CREATED WITH SOLE PURPOSE OF LISTENING TO COMMON EVENTS AT ONE LOCATION, AND AVOID
    //LISTENING TO EVENTS SUCH AS TYPESCHANGED, ACTIVEDESIGNERCHANGED, SELECTIONCHANGED AND COMPONENTCHANGED
    //REPEATEDLY
    internal sealed class WorkflowDesignerEventsCoordinator : IDisposable
    {
        private IDesignerLoaderHost serviceProvider;

        private bool typeSystemTypesChanged = false;

        private EventHandler refreshTypesHandler;
        private EventHandler refreshDesignerActionsHandler;
        private EventHandler refreshTasksHandler;

        public WorkflowDesignerEventsCoordinator(IDesignerLoaderHost serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            this.serviceProvider.LoadComplete += new EventHandler(OnDesignerReloaded);

            //Listen to the events so that we are sure that we appropriately refresh the designer actions and tasks when the types
            //change
            IDesignerEventService designerEventService = this.serviceProvider.GetService(typeof(IDesignerEventService)) as IDesignerEventService;
            if (designerEventService != null)
                designerEventService.ActiveDesignerChanged += new ActiveDesignerEventHandler(OnActiveDesignerChanged);

            ITypeProvider typeProvider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider != null)
                typeProvider.TypesChanged += new EventHandler(OnTypeSystemTypesChanged);

            ISelectionService selectionService = this.serviceProvider.GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
                selectionService.SelectionChanged += new EventHandler(OnSelectionChanged);

            IComponentChangeService componentChangedService = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (componentChangedService != null)
                componentChangedService.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);

            IPropertyValueUIService propertyValueService = this.serviceProvider.GetService(typeof(IPropertyValueUIService)) as IPropertyValueUIService;
            if (propertyValueService != null)
                propertyValueService.AddPropertyValueUIHandler(new PropertyValueUIHandler(OnPropertyGridAdornments));
        }

        void IDisposable.Dispose()
        {
            WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView != null)
            {
                if (this.refreshTypesHandler != null)
                    workflowView.Idle -= this.refreshTypesHandler;

                if (this.refreshDesignerActionsHandler != null)
                    workflowView.Idle -= this.refreshDesignerActionsHandler;

                if (this.refreshTasksHandler != null)
                    workflowView.Idle -= this.refreshTasksHandler;
            }

            this.refreshTypesHandler = null;
            this.refreshDesignerActionsHandler = null;
            this.refreshTasksHandler = null;

            IExtendedUIService extUIService = this.serviceProvider.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (extUIService != null)
                extUIService.RemoveDesignerActions();

            IPropertyValueUIService propertyValueService = this.serviceProvider.GetService(typeof(IPropertyValueUIService)) as IPropertyValueUIService;
            if (propertyValueService != null)
                propertyValueService.RemovePropertyValueUIHandler(new PropertyValueUIHandler(OnPropertyGridAdornments));

            IComponentChangeService componentChangedService = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (componentChangedService != null)
                componentChangedService.ComponentChanged -= new ComponentChangedEventHandler(OnComponentChanged);

            ISelectionService selectionService = this.serviceProvider.GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
                selectionService.SelectionChanged -= new EventHandler(OnSelectionChanged);

            ITypeProvider typeProvider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider != null)
                typeProvider.TypesChanged -= new EventHandler(OnTypeSystemTypesChanged);

            IDesignerEventService designerEventService = this.serviceProvider.GetService(typeof(IDesignerEventService)) as IDesignerEventService;
            if (designerEventService != null)
                designerEventService.ActiveDesignerChanged -= new ActiveDesignerEventHandler(OnActiveDesignerChanged);

            this.serviceProvider.LoadComplete -= new EventHandler(OnDesignerReloaded);
        }

        private void OnDesignerReloaded(object sender, EventArgs e)
        {
            bool refreshTypes = (this.refreshTypesHandler != null);
            bool refreshDesignerActions = (this.refreshDesignerActionsHandler != null);
            bool refreshTasks = (this.refreshTasksHandler != null);

            this.refreshTypesHandler = null;
            this.refreshDesignerActionsHandler = null;
            this.refreshTasksHandler = null;

            if (refreshTypes || refreshTasks || refreshDesignerActions)
            {
                RefreshTypes();
                RefreshDesignerActions();
            }
        }

        private void OnTypeSystemTypesChanged(object sender, EventArgs e)
        {
            this.typeSystemTypesChanged = true;

            //If the current designer is not active designer then we need to wait for it to be active before we update the types
            IDesignerEventService designerEventService = this.serviceProvider.GetService(typeof(IDesignerEventService)) as IDesignerEventService;
            if (designerEventService != null && designerEventService.ActiveDesigner == this.serviceProvider.GetService(typeof(IDesignerHost)))
                RefreshTypes();
        }

        private void OnActiveDesignerChanged(object sender, ActiveDesignerEventArgs e)
        {
            if (e.NewDesigner == this.serviceProvider.GetService(typeof(IDesignerHost)) && this.typeSystemTypesChanged)
                RefreshTypes();
            else
                RefreshTasks();
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs eventArgs)
        {
            RefreshDesignerActions();
        }

        private void RefreshTypes()
        {
            if (this.refreshTypesHandler == null && this.typeSystemTypesChanged)
            {
                WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (workflowView != null)
                {
                    this.refreshTypesHandler = new EventHandler(OnRefreshTypes);
                    workflowView.Idle += this.refreshTypesHandler;
                }
            }

            this.typeSystemTypesChanged = false;
        }

        private void OnRefreshTypes(object sender, EventArgs e)
        {
            if (this.refreshTypesHandler != null)
            {
                WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (workflowView != null)
                    workflowView.Idle -= this.refreshTypesHandler;
                this.refreshTypesHandler = null;
            }

            IDesignerHost designerHost = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            Activity rootActivity = (designerHost != null) ? designerHost.RootComponent as Activity : null;
            if (rootActivity == null)
                return;

            //Now Refresh the types as well as the designer actions
            ITypeProvider typeProvider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider != null)
            {
                Walker walker = new Walker();
                walker.FoundProperty += delegate(Walker w, WalkerEventArgs args)
                {
                    if (args.CurrentValue != null &&
                        args.CurrentProperty != null &&
                        args.CurrentProperty.PropertyType == typeof(System.Type) &&
                        args.CurrentValue is System.Type)
                    {
                        Type updatedType = typeProvider.GetType(((Type)args.CurrentValue).FullName);
                        if (updatedType != null)
                        {
                            args.CurrentProperty.SetValue(args.CurrentPropertyOwner, updatedType, null);

                            if (args.CurrentActivity != null)
                                TypeDescriptor.Refresh(args.CurrentActivity);
                        }
                    }
                    else if (args.CurrentProperty == null && args.CurrentValue is DependencyObject && !(args.CurrentValue is Activity))
                    {
                        walker.WalkProperties(args.CurrentActivity, args.CurrentValue);
                    }
                };
                walker.FoundActivity += delegate(Walker w, WalkerEventArgs args)
                {
                    if (args.CurrentActivity != null)
                    {
                        TypeDescriptor.Refresh(args.CurrentActivity);

                        ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(args.CurrentActivity);
                        if (activityDesigner != null)
                            activityDesigner.RefreshDesignerActions();

                        InvokeWorkflowDesigner invokeWorkflowDesigner = activityDesigner as InvokeWorkflowDesigner;
                        if (invokeWorkflowDesigner != null)
                            invokeWorkflowDesigner.RefreshTargetWorkflowType();
                    }
                };

                walker.Walk(rootActivity);
            }

            IPropertyValueUIService propertyValueService = this.serviceProvider.GetService(typeof(IPropertyValueUIService)) as IPropertyValueUIService;
            if (propertyValueService != null)
                propertyValueService.NotifyPropertyValueUIItemsChanged();

            RefreshTasks();
            RefreshDesignerActions();
        }

        private void RefreshDesignerActions()
        {
            if (this.refreshDesignerActionsHandler == null)
            {
                WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (workflowView != null)
                {
                    this.refreshDesignerActionsHandler = new EventHandler(OnRefreshDesignerActions);
                    workflowView.Idle += this.refreshDesignerActionsHandler;
                }
            }
        }

        private void OnRefreshDesignerActions(object sender, EventArgs e)
        {
            //NOTE: Make sure that we dont invalidate the workflow here. Workflow will be invalidated when'
            //the designer actions are refreshed on idle thread. Putting invalidation logic here causes the validation
            //to go haywire as before the idle message comes in we start getting to DesignerActions thru the paiting
            //logic to show smart tags. This causes problems and ConfigErrors appear everywhere on designer intermittently

            //PROBLEM: ConfigErrors appearing on the entire design surface
            //This is due to a race condition between the validation triggered during painting
            //logic and validation triggered when we try to access the DesignerActions from RefreshTask handler
            //In the validation logic we try to get to the types while doing so we Refresh the code compile unit 
            //in typesystem which in turn triggers CodeDomLoader.Refresh. In this we remove types, call refresh handler
            //and add new types. While doing this after the remove types but before addtypes, call is made to the drawing
            //logic in which we try to grab the designer actions which triggers another set of validations hence now we
            //always call UpdateWindow after invalidatewindow so that the validation will always get triggered on painting thread.
            //NOTE: PLEASE DO NOT CHANGE SEQUENCE IN WHICH THE FOLLOWING HANDLERS ARE ADDED
            //THE PROBLEM CAN BE ALSO FIXED BY TRIGGERING VALIDATION IN RefreshDesignerActions ITSELF
            //FOR NOW THE REFRESH FIX WILL CAUSE MINIMAL IMPACT IN THE LIGHT OF M3

            WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (this.refreshDesignerActionsHandler != null)
            {
                if (workflowView != null)
                    workflowView.Idle -= this.refreshDesignerActionsHandler;
                this.refreshDesignerActionsHandler = null;
            }

            DesignerHelpers.RefreshDesignerActions(this.serviceProvider);

            IPropertyValueUIService propertyValueService = this.serviceProvider.GetService(typeof(IPropertyValueUIService)) as IPropertyValueUIService;
            if (propertyValueService != null)
                propertyValueService.NotifyPropertyValueUIItemsChanged();

            RefreshTasks();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView != null)
                workflowView.Invalidate();

            RefreshTasks();
        }

        private void RefreshTasks()
        {
            if (this.refreshTasksHandler == null)
            {
                //Listen to the next idle event to populate the tasks; this should happen on selection changed as well as active designer changed if the current designer is active
                WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (workflowView != null)
                {
                    this.refreshTasksHandler = new EventHandler(OnRefreshTasks);
                    workflowView.Idle += this.refreshTasksHandler;
                }
            }
        }

        private void OnRefreshTasks(object sender, EventArgs e)
        {
            WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (this.refreshTasksHandler != null)
            {
                if (workflowView != null)
                    workflowView.Idle -= this.refreshTasksHandler;
                this.refreshTasksHandler = null;
            }

            ISelectionService selectionService = this.serviceProvider.GetService(typeof(ISelectionService)) as ISelectionService;
            IExtendedUIService extendedUIService = this.serviceProvider.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (selectionService != null && extendedUIService != null)
            {
                extendedUIService.RemoveDesignerActions();

                //Only if the current designer is active designer we add the designer actions to the task list
                IDesignerEventService designerEventService = (IDesignerEventService)this.serviceProvider.GetService(typeof(IDesignerEventService));
                if (designerEventService != null && designerEventService.ActiveDesigner == this.serviceProvider.GetService(typeof(IDesignerHost)))
                {
                    foreach (object obj in selectionService.GetSelectedComponents())
                    {
                        ActivityDesigner activityDesigner = null;
                        if (obj is HitTestInfo)
                            activityDesigner = ((HitTestInfo)obj).AssociatedDesigner;
                        else if (obj is Activity)
                            activityDesigner = ActivityDesigner.GetDesigner(obj as Activity);

                        if (activityDesigner != null)
                            extendedUIService.AddDesignerActions(new List<DesignerAction>(activityDesigner.DesignerActions).ToArray());
                    }
                }
            }

            if (workflowView != null)
                workflowView.Invalidate();
        }

        private void OnPropertyGridAdornments(ITypeDescriptorContext context, PropertyDescriptor propDesc, ArrayList valueUIItemList)
        {
            IComponent component = null;
            IReferenceService referenceService = this.serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            if (referenceService != null)
                component = referenceService.GetComponent(context.Instance);

            string fullAliasName = string.Empty;
            //this attribue is set to overcome issue with the TypedVariableDeclarationTypeConverter
            //not returning Name property at all. we alias that property to the VariableDeclaration itself 
            DefaultPropertyAttribute aliasPropertyNameAttribute = propDesc.Attributes[typeof(DefaultPropertyAttribute)] as DefaultPropertyAttribute;
            if (aliasPropertyNameAttribute != null && aliasPropertyNameAttribute.Name != null && aliasPropertyNameAttribute.Name.Length > 0)
                fullAliasName = propDesc.Name + "." + aliasPropertyNameAttribute.Name;

            if (component != null)
            {
                ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(component as Activity);
                if (activityDesigner != null)
                {
                    if (!activityDesigner.IsLocked && ActivityBindPropertyDescriptor.IsBindableProperty(propDesc) && !propDesc.IsReadOnly)
                        valueUIItemList.Add(new PropertyValueUIItem(DR.GetImage(DR.Bind), OnBindProperty, DR.GetString(DR.BindProperty)));

                    string fullComponentName = referenceService.GetName(component); //schedule1.send1
                    string fullPropertyName = referenceService.GetName(context.Instance); //schedule1.send1.message
                    fullPropertyName = (fullPropertyName.Length > fullComponentName.Length) ? fullPropertyName.Substring(fullComponentName.Length + 1, fullPropertyName.Length - fullComponentName.Length - 1) + "." + propDesc.Name : string.Empty;

                    foreach (DesignerAction action in activityDesigner.DesignerActions)
                    {
                        string actionPropertyName = action.PropertyName as string;
                        if (actionPropertyName == null || actionPropertyName.Length == 0)
                            continue;

                        if (actionPropertyName == propDesc.Name || (actionPropertyName == fullPropertyName) || (actionPropertyName == fullAliasName))
                        {
                            PropertyValueUIItemHandler propValueUIItemhandler = new PropertyValueUIItemHandler(action);
                            valueUIItemList.Add(new PropertyValueUIItem(action.Image, propValueUIItemhandler.OnFixPropertyError, action.Text));
                            break;
                        }
                    }
                }
            }
        }

        private void OnBindProperty(ITypeDescriptorContext context, PropertyDescriptor descriptor, PropertyValueUIItem invokedItem)
        {
            BindUITypeEditor.EditValue(context);
        }

        #region Class PropertyValueUIItemHandler
        private class PropertyValueUIItemHandler
        {
            DesignerAction action = null;
            internal PropertyValueUIItemHandler(DesignerAction action)
            {
                this.action = action;
            }
            internal void OnFixPropertyError(ITypeDescriptorContext context, PropertyDescriptor descriptor, PropertyValueUIItem invokedItem)
            {
                action.Invoke();
            }
        }
        #endregion
    }
    #endregion
}
