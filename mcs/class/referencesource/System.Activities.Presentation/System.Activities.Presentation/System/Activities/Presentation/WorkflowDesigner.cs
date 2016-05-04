//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Debugger;
    using System.Activities.Debugger.Symbol;
    using System.Activities.Presentation.Debug;
    using System.Activities.Presentation.Documents;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.Metadata;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Sqm;
    using System.Activities.Presentation.Validation;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.TreeView;
    using System.Activities.Presentation.Xaml;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Policy;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Threading;
    using System.Xml;
    using Microsoft.Activities.Presentation;
    using Microsoft.Activities.Presentation.Xaml;

    // This is the workflow designer context class.
    // it provides two views the primary workflow view in View property and the property browser view in the
    // propertyInspectorView property.
    // Load takes a objects instance or Xaml ( in the future) to load the designer from
    public partial class WorkflowDesigner
    {
        EditingContext context;
        ModelTreeManager modelTreeManager;
        Grid view;
        Grid outlineView;
        PropertyInspector propertyInspector;
        string text;
        ViewStateIdManager idManager;
        string loadedFile;
        DebuggerService debuggerService;
        UndoEngine undoEngine;
        ViewManager viewManager;
        ValidationService validationService;
        ObjectReferenceService objectReferenceService;
        DesignerPerfEventProvider perfEventProvider;
        bool isLoaded = false;
        bool isModelChanged = false;

        IXamlLoadErrorService xamlLoadErrorService;
        WorkflowDesignerXamlSchemaContext workflowDesignerXamlSchemaContext;

        public event TextChangedEventHandler TextChanged;
        public event EventHandler ModelChanged;
        WorkflowSymbol lastWorkflowSymbol;
        ObjectToSourceLocationMapping objectToSourceLocationMapping;

        internal class PreviewLoadEventArgs : EventArgs
        {
            object instance;
            EditingContext context;

            public PreviewLoadEventArgs(object instance, EditingContext context)
            {
                this.instance = instance;
                this.context = context;
            }

            public object Instance
            {
                get { return this.instance; }
            }

            public EditingContext Context
            {
                get { return this.context; }
            }
        }
        internal event EventHandler<PreviewLoadEventArgs> PreviewLoad;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "The static constructor is required to initialize the PropertyInspector metadata.")]
        static WorkflowDesigner()
        {
            InitializePropertyInspectorMetadata();
            DesignerMetadata metaData = new DesignerMetadata();
            metaData.Register();
        }

        public WorkflowDesigner()
        {
            // create our perf trace provider first
            this.perfEventProvider = new DesignerPerfEventProvider();            
            this.idManager = new ViewStateIdManager();
            this.context = new EditingContext();
            this.ModelSearchService = new ModelSearchServiceImpl(this);
            this.context.Items.SetValue(new ReadOnlyState { IsReadOnly = false });
            this.view = new Grid();
            this.view.Focusable = false;
            
            //add the resource dictionary to application resource so every component could reference it
            if (Application.Current == null)
            {
                //create an application if it doesn't exist, make sure it will not shutdown after windows being shut down
                Application app = new Application();
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            Fx.Assert(Application.Current != null, "Application and resources must be there");
            Application.Current.Resources.MergedDictionaries.Add(WorkflowDesignerColors.FontAndColorResources);
            Application.Current.Resources.MergedDictionaries.Add(WorkflowDesignerIcons.IconResourceDictionary);
            AttachedPropertiesService propertiesService = new AttachedPropertiesService();
            this.context.Services.Publish(typeof(AttachedPropertiesService), propertiesService);

            undoEngine = new UndoEngine(context);
            this.context.Services.Publish(typeof(UndoEngine), undoEngine);
            undoEngine.UndoCompleted += new EventHandler<UndoUnitEventArgs>(OnUndoCompleted);

            this.context.Services.Publish<ValidationService>(this.ValidationService);
            this.context.Services.Publish<ObjectReferenceService>(this.ObjectReferenceService);
            this.context.Services.Publish<DesignerPerfEventProvider>(this.perfEventProvider);
            this.context.Services.Publish<FeatureManager>(new FeatureManager(this.context));
            this.context.Services.Publish<DesignerConfigurationService>(new DesignerConfigurationService());

            this.context.Services.Subscribe<ICommandService>((s) =>
            {
                const string addinTypeName = "Microsoft.VisualStudio.Activities.AddIn.WorkflowDesignerAddIn";
                if (s != null && s.GetType().FullName.Equals(addinTypeName))
                {
                    DesignerConfigurationService service = this.context.Services.GetService<DesignerConfigurationService>();
                    if (service != null)
                    {
                        service.WorkflowDesignerHostId = WorkflowDesignerHostId.Dev10;
                    }
                }
            });

            this.context.Services.Subscribe<IVSSqmService>((service) =>
            {
                const string serviceTypeName = "Microsoft.VisualStudio.Activities.AddIn.VSSqmService";
                if (service != null && service.GetType().FullName.Equals(serviceTypeName))
                {
                    DesignerConfigurationService configurationService = this.context.Services.GetService<DesignerConfigurationService>();
                    if (configurationService != null)
                    {
                        configurationService.WorkflowDesignerHostId = WorkflowDesignerHostId.Dev11;
                    }
                }
            });

            this.Context.Items.Subscribe<ErrorItem>(delegate(ErrorItem errorItem)
            {
                ErrorView errorView = new ErrorView();
                errorView.Message = errorItem.Message;
                errorView.Details = errorItem.Details;
                errorView.Context = this.Context;

                // Clear views
                this.view.Children.Clear();
                this.view.Children.Add(errorView);
                if (this.outlineView != null)
                {
                    this.outlineView.Children.Clear();
                }
            }
                );

            this.context.Items.Subscribe<ReadOnlyState>(new SubscribeContextCallback<ReadOnlyState>(OnReadonlyStateChanged));

            this.context.Services.Subscribe<IXamlLoadErrorService>(s => this.xamlLoadErrorService = s);

            this.PreviewLoad += NamespaceSettingsHandler.PreviewLoadRoot;
            this.view.Loaded += (s, e) =>
            {
                //when view is loaded, check if user did provide his own WindowHelperService - if not, provide a default one
                if (!this.context.Services.Contains<WindowHelperService>())
                {
                    IntPtr hWND = IntPtr.Zero;
                    Window ownerWindow = Window.GetWindow(this.view);
                    if (null != ownerWindow)
                    {
                        WindowInteropHelper helper = new WindowInteropHelper(ownerWindow);
                        hWND = helper.Handle;
                    }
                    this.Context.Services.Publish<WindowHelperService>(new WindowHelperService(hWND));
                }
                WindowHelperService whs = this.context.Services.GetService<WindowHelperService>();
                whs.View = this.view;

                //check if workflow command extension item is available - if not, provide default one
                if (!this.context.Items.Contains<WorkflowCommandExtensionItem>())
                {
                    WorkflowCommandExtensionItem item = new WorkflowCommandExtensionItem(new DefaultCommandExtensionCallback());
                    this.context.Items.SetValue(item);
                }

                ComponentDispatcher.EnterThreadModal += new EventHandler(ComponentDispatcher_EnterThreadModal);
                ComponentDispatcher.LeaveThreadModal += new EventHandler(ComponentDispatcher_LeaveThreadModal);
            };

            this.view.Unloaded += (s, e) =>
            {
                ComponentDispatcher.EnterThreadModal -= new EventHandler(ComponentDispatcher_EnterThreadModal);
                ComponentDispatcher.LeaveThreadModal -= new EventHandler(ComponentDispatcher_LeaveThreadModal);
            };

            this.view.IsKeyboardFocusWithinChanged += (s, e) =>
            {
                // The ModelTreeManager is null when there is an active ErrorItem.
                // We have nothing to write to text in this case.
                if (this.modelTreeManager != null && (bool)e.NewValue == false)
                {
                    if ((FocusManager.GetFocusedElement(this.view) as TextBox) != null)
                    {
                        FocusManager.SetFocusedElement(this.view, null);
                        this.NotifyModelChanged();
                    }
                }
            };
        }

        internal ValidationService ValidationService
        {
            get
            {
                if (this.validationService == null)
                {
                    this.validationService = new ValidationService(this.context);
                    this.validationService.ErrorsMarked += ActivityArgumentHelper.UpdateInvalidArgumentsIfNecessary;
                }

                return this.validationService;
            }
        }

        internal ObjectReferenceService ObjectReferenceService
        {
            get
            {
                if (this.objectReferenceService == null)
                {
                    this.objectReferenceService = new ObjectReferenceService(this.context);
                }

                return this.objectReferenceService;
            }
        }

        public UIElement View
        {
            get
            {
                return this.view;
            }
        }

        public UIElement PropertyInspectorView
        {
            get
            {
                if (this.propertyInspector == null)
                {
                    // We change WorkflowDesigner.PropertyInspectorView to be lazy load because the propertyinspector hosted in
                    // Winform elementhost will not get the resource change notification from Application level resource dictionary.
                    // So we have to have all colors be ready before propertyinspector gets initialized.
                    this.propertyInspector = new PropertyInspector();
                    this.propertyInspector.DesignerContextItemManager = this.context.Items;
                    this.propertyInspector.EditingContext = this.context;
                    this.InitializePropertyInspectorResources();
                    this.InitializePropertyInspectorCommandHandling();
                }

                return this.propertyInspector;
            }
        }

        public UIElement OutlineView
        {
            get
            {
                if (this.outlineView == null)
                {
                    this.outlineView = new Grid();
                    this.outlineView.Focusable = false;
                    AddOutlineView();
                }

                return this.outlineView;
            }
        }

        void AddOutlineView()
        {
            DesignerTreeView treeView = new DesignerTreeView();
            treeView.Initialize(context);
            this.context.Services.Subscribe<ModelService>(delegate(ModelService modelService)
            {
                if (modelService.Root != null)
                {
                    treeView.SetRootDesigner(modelService.Root);

                }
                treeView.RestoreDesignerStates();
            });
            this.outlineView.Children.Add(treeView);
        }


        public EditingContext Context
        {
            get
            {
                return this.context;
            }
        }

        public ContextMenu ContextMenu
        {
            get
            {
                if (null != this.context)
                {
                    DesignerView designerView = this.context.Services.GetService<DesignerView>();
                    if (null != designerView)
                    {
                        return designerView.ContextMenu;
                    }
                }
                return null;
            }
        }

        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }

        [SuppressMessage(FxCop.Category.Design, "CA1044:PropertiesShouldNotBeWriteOnly",
            Justification = "The host just sets this property for the designer to know which colors to display.")]
        public string PropertyInspectorFontAndColorData
        {
            set
            {
                StringReader stringReader = new StringReader(value);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                Hashtable fontAndColorDictionary = (Hashtable)System.Windows.Markup.XamlReader.Load(xmlReader);
                foreach (string key in fontAndColorDictionary.Keys)
                {
                    WorkflowDesignerColors.FontAndColorResources[key] = fontAndColorDictionary[key];
                }
            }
        }


        public bool IsInErrorState()
        {
            ErrorItem errorItem = this.context.Items.GetValue<ErrorItem>();
            return errorItem.Message != null && errorItem.Details != null ? true : false;
        }

        // Load using Xaml.
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Deserializer might throw if it fails to deserialize. Catching all exceptions to avoid VS Crash.")]
        [SuppressMessage("Reliability", "Reliability108",
            Justification = "Deserializer might throw if it fails to deserialize. Catching all exceptions to avoid VS crash.")]
        public void Load()
        {
            this.perfEventProvider.WorkflowDesignerLoadStart();
            if (!string.IsNullOrEmpty(this.text))
            {
                try
                {
                    this.perfEventProvider.WorkflowDesignerDeserializeStart();

                    IList<XamlLoadErrorInfo> loadErrors;
                    Dictionary<object, SourceLocation> sourceLocations;
                    object deserializedObject = DeserializeString(this.text, out loadErrors, out sourceLocations);

                    this.perfEventProvider.WorkflowDesignerDeserializeEnd();

                    if (deserializedObject != null)
                    {
                        this.Load(deserializedObject);
                        this.ValidationService.ValidateWorkflow(ValidationReason.Load);
                    }
                    else
                    {
                        StringBuilder details = new StringBuilder();
                        foreach (XamlLoadErrorInfo error in loadErrors)
                        {
                            details.AppendLine(error.Message);
                        }
                        this.Context.Items.SetValue(new ErrorItem() { Message = SR.SeeErrorWindow, Details = details.ToString() });
                    }
                    if (loadErrors != null)
                    {
                        RaiseLoadErrors(loadErrors);
                    }
                    this.isModelChanged = false;
                }
                catch (Exception e)
                {
                    this.Context.Items.SetValue(new ErrorItem() { Message = e.Message, Details = e.ToString() });
                    RaiseLoadError(e);
                }
            }
            else
            {
                this.Context.Items.SetValue(new ErrorItem() { Message = string.Empty, Details = string.Empty });
            }
            if (this.IsInErrorState())
            {
                // Clear workflow symbol in case ErrorState changes during validation
                this.lastWorkflowSymbol = null;
            }
            this.perfEventProvider.WorkflowDesignerLoadComplete();
        }

        public void Load(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("fileName"));
            }
            
            DesignerConfigurationService service = this.Context.Services.GetService<DesignerConfigurationService>();
            service.SetDefaultOfLoadingFromUntrustedSourceEnabled();
            if (!service.LoadingFromUntrustedSourceEnabled && !IsFromUnrestrictedPath(fileName))
            {
                throw FxTrace.Exception.AsError(new SecurityException(string.Format(CultureInfo.CurrentUICulture, SR.UntrustedSourceDetected, fileName)));
            }

            try
            {
                IDocumentPersistenceService documentPersistenceService = this.Context.Services.GetService<IDocumentPersistenceService>();
                if (documentPersistenceService != null)
                {
                    this.Load(documentPersistenceService.Load(fileName));
                }
                else
                {
                    using (StreamReader fileStream = new StreamReader(fileName))
                    {
                        this.loadedFile = fileName;
                        WorkflowFileItem fileItem = new WorkflowFileItem();
                        fileItem.LoadedFile = fileName;
                        this.context.Items.SetValue(fileItem);
                        this.Text = fileStream.ReadToEnd();
                        this.Load();
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                else
                {
                    this.Context.Items.SetValue(new ErrorItem() { Message = e.Message, Details = e.ToString() });
                }
            }
            if (!this.IsInErrorState())
            {
                if (this.debuggerService != null)
                {
                    this.debuggerService.InvalidateSourceLocationMapping(fileName);
                }
            }
        }

        // This supports loading objects instead of xaml into the designer 
        public void Load(object instance)
        {
            if (isLoaded)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WorkflowDesignerLoadShouldBeCalledOnlyOnce));
            }

            isLoaded = true;

            if (instance == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("instance"));
            }

            DesignerConfigurationService configurationService = this.context.Services.GetService<DesignerConfigurationService>();
            configurationService.ApplyDefaultPreference();

            // Because we want AutoConnect/AutoSplit to be on even in Dev10 if PU1 is installed.
            // But we cannot know whether PU1 is installed or not, we decide to enable these 2 features for all Dev10.
            if (configurationService.WorkflowDesignerHostId == WorkflowDesignerHostId.Dev10)
            {
                configurationService.AutoConnectEnabled = true;
                configurationService.AutoSplitEnabled = true;
            }

            configurationService.IsWorkflowLoaded = true;
            configurationService.Validate();

            if (this.PreviewLoad != null)
            {
                this.PreviewLoad(this, new PreviewLoadEventArgs(instance, this.context));
            }

            if (configurationService.TargetFrameworkName.IsLessThan45())
            {
                TargetFrameworkPropertyFilter.FilterOut45Properties();
            }

            modelTreeManager = new ModelTreeManager(this.context);
            modelTreeManager.Load(instance);
            this.context.Services.Publish(typeof(ModelTreeManager), modelTreeManager);
            viewManager = GetViewManager(this.modelTreeManager.Root);
            this.context.Services.Publish<ModelSearchService>(this.ModelSearchService);
            view.Children.Add((UIElement)viewManager.View);

            modelTreeManager.EditingScopeCompleted += new EventHandler<EditingScopeEventArgs>(OnEditingScopeCompleted);

            this.view.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                             new Action(() => { this.perfEventProvider.WorkflowDesignerApplicationIdleAfterLoad(); }));

            //Subscribe to the ViewStateChanged event of ViewStateService to show document dirty. It would be published in the call to GetViewManager().
            WorkflowViewStateService wfViewStateService = this.Context.Services.GetService(typeof(ViewStateService)) as WorkflowViewStateService;
            if (wfViewStateService != null)
            {
                wfViewStateService.UndoableViewStateChanged += new ViewStateChangedEventHandler(OnViewStateChanged);
            }
            this.isModelChanged = false;
            if (!this.IsInErrorState())
            {
                this.lastWorkflowSymbol = GetAttachedWorkflowSymbol();
            }
        }

        public void Save(string fileName)
        {
            this.isModelChanged = true; // ensure flushing any viewstate changes that does not imply model changed.
            try
            {
                // Cancel pervious validation and suppress validation work.
                this.ValidationService.DeactivateValidation();
                Flush(fileName);
            }
            finally
            {
                this.ValidationService.ActivateValidation();
            }

            using (StreamWriter fileStreamWriter = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                fileStreamWriter.Write(this.Text);
                fileStreamWriter.Flush();
            }

            if (this.Context.Services.GetService<ModelService>() != null)
            {
                this.ValidationService.ValidateWorkflow(ValidationReason.Save);
            }

            if (this.debuggerService != null)
            {
                this.debuggerService.InvalidateSourceLocationMapping(fileName);
            }
        }

        public void Flush()
        {
            Flush(null);
        }

        private bool IsFromUnrestrictedPath(string fileName)
        {
            Evidence folderEvidence = new Evidence();
            folderEvidence.AddHostEvidence(Zone.CreateFromUrl(fileName));

            PermissionSet standardFolderSandbox = SecurityManager.GetStandardSandbox(folderEvidence);
            return standardFolderSandbox.IsUnrestricted();
        }

        void Flush(string fileName)
        {
            if (this.modelTreeManager == null)
            {
                // It's possible for modelTreeManager to be null if Load is called but the xaml file being loaded is invalid.
                // We only want to throw exception if Load hasn't been called yet.
                if (IsInErrorState() == false)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WorkflowDesignerLoadShouldBeCalledFirst));
                }
            }
            else
            {
                this.FlushEdits();
                IDocumentPersistenceService documentPersistenceService = this.Context.Services.GetService<IDocumentPersistenceService>();
                if (documentPersistenceService != null)
                {
                    documentPersistenceService.Flush(this.modelTreeManager.Root.GetCurrentValue());
                }
                else
                {
                    this.WriteModelToText(fileName);
                }
            }
        }

        void FlushEdits()
        {
            UIElement oldFocus = null;
            //check if property grid has keyboard focus within, if yes - get focused control
            if (null != this.propertyInspector && this.propertyInspector.IsKeyboardFocusWithin)
            {
                oldFocus = FocusManager.GetFocusedElement(this.propertyInspector) as UIElement;
            }
            //check if view has keyboard focus within, if yes - get focused control
            if (null != this.view && this.view.IsKeyboardFocusWithin)
            {
                oldFocus = FocusManager.GetFocusedElement(this.view) as UIElement;
            }
            if (null != oldFocus)
            {
                RoutedCommand cmd = DesignerView.CommitCommand as RoutedCommand;
                if (cmd != null)
                {
                    cmd.Execute(null, oldFocus);
                }
            }

            //commit changes within arguments and variables editor
            var designerView = this.Context.Services.GetService<DesignerView>();
            if (null != designerView)
            {
                if (null != designerView.arguments1)
                {
                    DataGridHelper.CommitPendingEdits(designerView.arguments1.argumentsDataGrid);
                }
                if (null != designerView.variables1)
                {
                    DataGridHelper.CommitPendingEdits(designerView.variables1.variableDataGrid);
                }
            }
        }

        static void InitializePropertyInspectorMetadata()
        {
            PropertyInspectorMetadata.Initialize();
        }

        static Activity GetRootWorkflowElement(object rootModelObject)
        {
            return WorkflowDesignerXamlHelper.GetRootWorkflowElement(rootModelObject);
        }
    }
}
