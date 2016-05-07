//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Activities.Expressions;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using Microsoft.Activities.Presentation;
    using Microsoft.VisualBasic.Activities;

    partial class ImportDesigner
    {
        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register(
            "Context",
            typeof(EditingContext),
            typeof(ImportDesigner),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContextChanged)));

        const string ErrorMessagePropertyName = "ErrorMessage";
        const string IsInvalidPropertyName = "IsInvalid";

        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        TextBox textBox;
        bool isSetInternally;
        bool causedBySelectionChange;
        bool isSelectedByMouse;
        string lastInput;
        int lastCursePosition;
        bool firstShowUp;
        string importsHintText;
        ModelItem lastSelection;
        SubscribeContextCallback<Selection> onItemSelectedCallback;
        bool isSelectionChangedInternally;
        FrameworkName targetFramework;

        ImportedNamespaceContextItem importedNamespacesItem;
        ModelItemCollection importsModelItem;
        IDictionary<string, List<string>> availableNamespaces;
        IDictionary<string, IDictionary<string, object>> attachedPropertiesForNamespace = new Dictionary<string, IDictionary<string, object>>();

        ImportDesignerProxy proxy;

        public ImportDesigner()
        {
            this.proxy = new ImportDesignerProxy(this);

            InitializeComponent();
            this.firstShowUp = true;
            this.importsHintText = (string)this.FindResource("importsHintText");
        }

        SubscribeContextCallback<Selection> OnItemSelectedCallback
        {
            get
            {
                if (this.onItemSelectedCallback == null)
                {
                    this.onItemSelectedCallback = new SubscribeContextCallback<Selection>(this.OnItemSelected);
                }
                return this.onItemSelectedCallback;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Unloaded += OnImportDesignerUnloaded;
        }

        void OnImportDesignerUnloaded(object sender, RoutedEventArgs e)
        {
            this.importsModelItem.CollectionChanged -= this.OnImportsModelItemCollectionChanged;
            this.Unloaded -= this.OnImportDesignerUnloaded;
        }

        void OnInputComboBoxUnloaded(object sender, RoutedEventArgs e)
        {
            this.inputComboBox.Loaded -= this.OnInputComboBoxLoaded;
            this.inputComboBox.SelectionChanged -= OnComboBoxSelectionChanged;
            this.inputComboBox.Unloaded -= OnInputComboBoxUnloaded;

            Fx.Assert(this.textBox != null, "ComboBox should contains a child of TextBox");
            textBox.TextChanged -= OnTextChanged;
            textBox.PreviewKeyUp -= OnPreviewKeyUp;
            textBox.IsKeyboardFocusedChanged -= OnTextBoxIsKeyboardFocusedChanged;
            AppDomain.CurrentDomain.AssemblyLoad -= this.proxy.OnAssemblyLoad;
        }

        void OnInputComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.textBox = this.inputComboBox.Template.FindName("PART_EditableTextBox", this.inputComboBox) as TextBox;
            this.textBox.TextChanged += new TextChangedEventHandler(OnTextChanged);
            this.textBox.PreviewKeyUp += new KeyEventHandler(OnPreviewKeyUp);
            this.textBox.IsKeyboardFocusedChanged += new DependencyPropertyChangedEventHandler(OnTextBoxIsKeyboardFocusedChanged);
            this.textBox.IsUndoEnabled = false;

            this.isSetInternally = true;
            this.textBox.Foreground = SystemColors.GrayTextBrush;
            this.textBox.FontStyle = FontStyles.Italic;
            this.textBox.Text = this.importsHintText;
            this.isSetInternally = false;

            this.inputComboBox.SelectionChanged += new SelectionChangedEventHandler(OnComboBoxSelectionChanged);
        }

        void OnTextBoxIsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox inputTextBox = sender as TextBox;
            Fx.Assert(inputTextBox != null, "Sender of IsKeyboardFocusedChanged event should be a TextBox");

            if (!this.isSetInternally)
            {
                if ((bool)e.NewValue == true)
                {
                    this.isSetInternally = true;
                    inputTextBox.FontStyle = FontStyles.Normal;
                    inputTextBox.Foreground = SystemColors.ControlTextBrush;
                    if (inputTextBox.Text == this.importsHintText)
                    {
                        inputTextBox.Text = string.Empty;
                        inputTextBox.CaretIndex = 0;
                    }
                    this.isSetInternally = false;
                }
                else if (string.IsNullOrEmpty(inputTextBox.Text))
                {
                    this.isSetInternally = true;
                    inputTextBox.Text = this.importsHintText;
                    inputTextBox.Foreground = SystemColors.GrayTextBrush;
                    inputTextBox.FontStyle = FontStyles.Italic;
                    this.isSetInternally = false;
                }
            }
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            IIntegratedHelpService helpService = this.Context.Services.GetService<IIntegratedHelpService>();
            if (helpService != null)
            {
                if ((Boolean)e.NewValue)
                {
                    this.isSelectionChangedInternally = true;
                    this.Context.Items.SetValue(new Selection());
                    this.isSelectionChangedInternally = false;
                    helpService.AddContextAttribute(string.Empty, WorkflowViewManager.GetF1HelpTypeKeyword(typeof(ImportDesigner)), System.ComponentModel.Design.HelpKeywordType.F1Keyword);
                }
                else
                {
                    helpService.RemoveContextAttribute(string.Empty, WorkflowViewManager.GetF1HelpTypeKeyword(typeof(ImportDesigner)));
                }
            }
            base.OnIsKeyboardFocusWithinChanged(e);
        }

        void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (bool.Equals(true, e.NewValue))
            {
                StoreLastSelection();
                this.Context.Items.Subscribe<Selection>(this.OnItemSelectedCallback);
                if (this.firstShowUp)
                {
                    this.firstShowUp = false;
                    this.inputComboBox.Loaded += this.OnInputComboBoxLoaded;
                    this.inputComboBox.Unloaded += this.OnInputComboBoxUnloaded;

                    this.inputComboBox.ItemsSource = this.availableNamespaces.Keys;
                    this.inputComboBox.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
                    this.inputComboBox.Items.Filter += this.FilterPredicate;
                    this.importedNamespacesDataGrid.Items.SortDescriptions.Add(new SortDescription(NamespaceListPropertyDescriptor.NamespacePropertyName, ListSortDirection.Ascending));
                    this.importedNamespacesDataGrid.ItemsSource = this.importsModelItem;
                    this.importedNamespacesDataGrid.SelectionChanged += this.OnImportedNamespacesDataGridSelectionChanged;
                }
            }
            else
            {
                RestoreLastSelection();
                this.Context.Items.Unsubscribe<Selection>(this.OnItemSelectedCallback);
            }
        }

        void RestoreLastSelection()
        {
            if ((this.Context != null) && (this.lastSelection != null))
            {
                this.Context.Items.SetValue(new Selection(this.lastSelection));
            }
        }

        void StoreLastSelection()
        {
            if (this.Context != null)
            {
                this.lastSelection = this.Context.Items.GetValue<Selection>().PrimarySelection;
            }
        }

        void OnImportedNamespacesDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid importsDataGrid = sender as DataGrid;
            importsDataGrid.CanUserDeleteRows = true;
            foreach (object item in importsDataGrid.SelectedItems)
            {
                ModelItem import = item as ModelItem;
                if ((import != null) && !((bool)import.Properties[IsInvalidPropertyName].ComputedValue))
                {
                    importsDataGrid.CanUserDeleteRows = false;
                    break;
                }
            }
        }

        void CommitImportNamespace(string addedNamespace)
        {
            ModelItem importItem;
            if (!TryGetWrapper(addedNamespace, out importItem))
            {
                NamespaceData newImport = new NamespaceData { Namespace = addedNamespace };
                importItem = this.importsModelItem.Add(newImport);
            }
            this.importedNamespacesDataGrid.SelectedItem = importItem;
            this.importedNamespacesDataGrid.ScrollIntoView(importItem);
            this.textBox.Text = string.Empty;
        }

        void OnPreviewKeyUp(object sender, KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Enter:
                    if (this.inputComboBox.SelectedItem != null)
                    {
                        CommitImportNamespace(this.inputComboBox.SelectedItem.ToString());
                    }
                    break;
                case Key.Down:
                    if (!this.inputComboBox.IsDropDownOpen)
                    {
                        this.inputComboBox.IsDropDownOpen = true;
                    }
                    break;
                default: break;
            }
        }

        bool TryGetWrapper(string imported, out ModelItem importItem)
        {
            importItem = null;
            foreach (ModelItem item in this.importsModelItem)
            {
                NamespaceData importedNamespace = item.GetCurrentValue() as NamespaceData;
                Fx.Assert(importedNamespace != null, "element of import list model has to be NamespaceData");
                if (importedNamespace.Namespace == imported)
                {
                    importItem = item;
                    return true;
                }
            }

            return false;
        }

        void OnImportsModelItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ModelItem namespaceModel in e.NewItems)
                {
                    ValidateNamespaceModelAndUpdateContextItem(namespaceModel);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ModelItem namespaceModel in e.OldItems)
                {
                    NamespaceData namespaceData = namespaceModel.GetCurrentValue() as NamespaceData;
                    Fx.Assert(namespaceData != null, "added item has to be NamespaceData");

                    this.importedNamespacesItem.ImportedNamespaces.Remove(namespaceData.Namespace);
                    this.attachedPropertiesForNamespace.Remove(namespaceData.Namespace);
                }
            }

            UpdateExpressionEditors();
        }

        private void ValidateNamespaceModelAndUpdateContextItem(ModelItem namespaceModel)
        {
            NamespaceData namespaceData = namespaceModel.GetCurrentValue() as NamespaceData;
            Fx.Assert(namespaceData != null, "added item has to be NamespaceData");

            if (!this.availableNamespaces.ContainsKey(namespaceData.Namespace))
            {
                namespaceModel.Properties[ErrorMessagePropertyName].SetValue(string.Format(CultureInfo.CurrentCulture, SR.CannotResolveNamespace, namespaceData.Namespace));
                namespaceModel.Properties[IsInvalidPropertyName].SetValue(true);
            }
            else
            {
                namespaceModel.Properties[ErrorMessagePropertyName].SetValue(string.Empty);
                namespaceModel.Properties[IsInvalidPropertyName].SetValue(false);
                this.importedNamespacesItem.ImportedNamespaces.Add(namespaceData.Namespace);
            }
        }

        private void UpdateExpressionEditors()
        {
            AssemblyContextControlItem assemblies = this.Context.Items.GetValue<AssemblyContextControlItem>();
            IExpressionEditorService expressionEditorService = this.Context.Services.GetService<IExpressionEditorService>();
            if (expressionEditorService != null)
            {
                expressionEditorService.UpdateContext(assemblies, this.importedNamespacesItem);
            }
        }

        void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (!this.isSetInternally)
            {
                if (this.isSelectedByMouse)
                {
                    CommitImportNamespace(this.inputComboBox.SelectedItem.ToString());
                    this.isSelectedByMouse = false;
                }
                else
                {
                    this.causedBySelectionChange = true;
                }
            }
        }

        bool FilterPredicate(object value)
        {
            // We don't like nulls.
            if (value == null)
                return false;


            if (string.IsNullOrEmpty(this.lastInput))
                return true;

            return value.ToString().StartsWith(this.lastInput, StringComparison.OrdinalIgnoreCase);
        }

        void OnTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!this.isSetInternally)
            {
                this.lastInput = this.textBox.Text.Trim();
                this.lastCursePosition = this.textBox.CaretIndex;

                this.isSetInternally = true;

                if (this.causedBySelectionChange)
                {
                    this.causedBySelectionChange = false;
                }
                else
                {
                    Fx.Assert(this.inputComboBox.ItemsSource != null, "combo box's source must already be initialized when textbox is running");
                    CollectionViewSource.GetDefaultView(this.inputComboBox.ItemsSource).Refresh();

                    if (this.lastInput.Length > 0)
                    {
                        this.inputComboBox.IsDropDownOpen = true;
                        foreach (object item in CollectionViewSource.GetDefaultView(this.inputComboBox.ItemsSource))
                        {
                            this.inputComboBox.SelectedItem = null;
                            this.inputComboBox.SelectedItem = item;
                            break;
                        }
                    }
                    else
                    {
                        this.inputComboBox.IsDropDownOpen = false;
                        this.inputComboBox.SelectedItem = null;
                    }
                }

                this.textBox.Text = this.lastInput;
                this.textBox.CaretIndex = this.lastCursePosition;

                this.isSetInternally = false;
            }
        }

        void OnMouseLeftClickComboBoxItem(object sender, MouseButtonEventArgs e)
        {
            ComboBoxItem comboBoxItem = sender as ComboBoxItem;
            if (comboBoxItem != null)
            {
                this.isSetInternally = true;
                string userInput = this.textBox.Text;
                this.inputComboBox.SelectedItem = null;
                this.textBox.Text = userInput;
                this.isSetInternally = false;
                this.isSelectedByMouse = true;
            }
        }

        static void OnContextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ImportDesigner)sender).OnContextChanged();
        }

        void OnContextChanged()
        {
            this.targetFramework = WorkflowDesigner.GetTargetFramework(this.Context);

            AttachedPropertiesService attachedPropertyService = this.Context.Services.GetService<AttachedPropertiesService>();
            Fx.Assert(attachedPropertyService != null, "AttachedPropertiesService shouldn't be null in EditingContext.");
            attachedPropertyService.AddProperty(CreateAttachedPropertyForNamespace<string>(ErrorMessagePropertyName));
            attachedPropertyService.AddProperty(CreateAttachedPropertyForNamespace<bool>(IsInvalidPropertyName));
            //clear any defaults because it's meant to be set by host and not serialized to XAML
            VisualBasicSettings.Default.ImportReferences.Clear();

            ModelService modelService = this.Context.Services.GetService<ModelService>();
            Fx.Assert(modelService != null, "ModelService shouldn't be null in EditingContext.");
            Fx.Assert(modelService.Root != null, "model must have a root");
            this.importsModelItem = modelService.Root.Properties[NamespaceListPropertyDescriptor.ImportCollectionPropertyName].Collection;
            Fx.Assert(this.importsModelItem != null, "root must have imports");
            this.importsModelItem.CollectionChanged += this.OnImportsModelItemCollectionChanged;

            this.importedNamespacesItem = this.Context.Items.GetValue<ImportedNamespaceContextItem>();
            this.importedNamespacesItem.EnsureInitialized(this.Context);

            if (this.availableNamespaces == null)
            {
                //change to available namespace should not be a model change so we access the dictionary directly
                this.availableNamespaces = this.importsModelItem.Properties[NamespaceListPropertyDescriptor.AvailableNamespacesPropertyName].ComputedValue as Dictionary<string, List<string>>;
                Fx.Assert(this.availableNamespaces != null, "Available namespace dictionary is not in right format");
            }
            RefreshNamespaces();
        }

        void OnItemSelected(Selection newSelection)
        {
            if ((!this.Context.Services.GetService<UndoEngine>().IsUndoRedoInProgress) && (!this.isSelectionChangedInternally))
            {
                StoreLastSelection();
            }
        }

        AttachedProperty<T> CreateAttachedPropertyForNamespace<T>(string propertyName)
        {
            return new AttachedProperty<T>
            {
                Getter = (modelItem) =>
                {
                    NamespaceData data = modelItem.GetCurrentValue() as NamespaceData;
                    if (data == null)
                    {
                        return default(T);
                    }

                    IDictionary<string, object> properties;
                    if (!this.attachedPropertiesForNamespace.TryGetValue(data.Namespace, out properties))
                    {
                        return default(T);
                    }

                    object propertyValue;
                    if (!properties.TryGetValue(propertyName, out propertyValue))
                    {
                        return default(T);
                    }

                    return (T)propertyValue;
                },

                Setter = (modelItem, propertyValue) =>
                {
                    NamespaceData data = modelItem.GetCurrentValue() as NamespaceData;
                    if (data == null)
                    {
                        return;
                    }

                    IDictionary<string, object> properties;
                    if (!this.attachedPropertiesForNamespace.TryGetValue(data.Namespace, out properties))
                    {
                        properties = new Dictionary<string, object>();
                        this.attachedPropertiesForNamespace[data.Namespace] = properties;
                    }

                    properties[propertyName] = propertyValue;
                },
                Name = propertyName,
                OwnerType = typeof(NamespaceData),
                IsBrowsable = true,
            };
        }

        private void RefreshNamespaces()
        {
            GetAvailableNamespaces();
            ValidateImportedNamespaces();
        }

        internal void OnReferenceUpdated(AssemblyName updatedReference, bool isAdded)
        {
            if (this.availableNamespaces != null)
            {
                IMultiTargetingSupportService multiTargetingService = this.Context.Services.GetService<IMultiTargetingSupportService>();
                Assembly assembly = AssemblyContextControlItem.GetAssembly(updatedReference, multiTargetingService);
                Fx.Assert(assembly != null, "Assembly shouldn't be null here.");
                // In normal case, assembly shouldn't be null. In case there's any situation we're overlooked, we should ignore instead of throwing exceptions.
                if (assembly != null)
                {
                    OnReferenceUpdated(assembly, isAdded);
                }             
            }
        }

        private void OnReferenceUpdated(Assembly assembly, bool isAdded)
        {
            if (this.availableNamespaces != null)
            {
                if (isAdded)
                {
                    try
                    {
                        UpdateAvailableNamespaces(assembly, null);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }
                else
                {
                    try
                    {
                        string name = assembly.GetName().Name;
                        IEnumerable<string> namespaces = this.GetUniqueNamespacesInAssembly(assembly, null);
                        foreach (string ns in namespaces)
                        {
                            if (!string.IsNullOrEmpty(ns))
                            {
                                List<string> assemblyList;
                                if (this.availableNamespaces.TryGetValue(ns, out assemblyList))
                                {
                                    assemblyList.Remove(name);
                                    if (assemblyList.Count == 0)
                                    {
                                        this.availableNamespaces.Remove(ns);
                                    }
                                }
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        //If removed reference assembly is not there anymore, scan new list of referenced assemblies and refresh the list
                        this.GetAvailableNamespaces();
                    }
                }
                
                if (this.inputComboBox.ItemsSource != null)
                {
                    // This following refresh action may cause ComboBox selection/text changed. Need to restore back to what input by users after refresh is done.
                    this.isSetInternally = true;
                    string originalText = this.textBox.Text;
                    int originalCursePosition = this.textBox.CaretIndex;

                    try
                    {                          
                        CollectionViewSource.GetDefaultView(this.inputComboBox.ItemsSource).Refresh();                        
                    }
                    catch (TargetInvocationException)
                    {
                        // TargetInvocationException may thrown out when assembly is loaded while ComboBox is refreshing and we call Refresh again, see CSDMain:213478
                        // Catch the exception here and ignore it
                    }
                    finally
                    {
                        this.textBox.Text = lastInput;
                        this.textBox.CaretIndex = originalCursePosition;
                        this.isSetInternally = false;
                    }
                }

                ValidateImportedNamespaces();
            }
        }

        //When assemblyContextItem is null, it means the assembly cannot be local assembly
        private void UpdateAvailableNamespaces(Assembly assembly, AssemblyContextControlItem assemblyContextItem)
        {
            string assemblyName = assembly.GetName().Name;
            IEnumerable<string> namespaces = this.GetUniqueNamespacesInAssembly(assembly, assemblyContextItem);
            foreach (string ns in namespaces)
            {
                if (!string.IsNullOrEmpty(ns))
                {
                    List<string> assemblyList;
                    if (!this.availableNamespaces.TryGetValue(ns, out assemblyList))
                    {
                        assemblyList = new List<string> { assemblyName };
                        this.availableNamespaces.Add(ns, assemblyList);
                    }
                    else if (!assemblyList.Contains(assemblyName))
                    {
                        assemblyList.Add(assemblyName);
                    }
                }
            }
        }

        //When assemblyContextItem is null, it means the assembly cannot be local assembly
        IEnumerable<string> GetUniqueNamespacesInAssembly(Assembly assembly, AssemblyContextControlItem assemblyContextItem)
        {
            IEnumerable<Type> types = null;
            //we should only display namespaces for public types except the local assembly
            if ((assemblyContextItem != null) && (assemblyContextItem.LocalAssemblyName != null) && (assembly.FullName == assemblyContextItem.LocalAssemblyName.FullName))
            {
                types = assembly.GetTypes();
            }
            else
            {
                types = assembly.GetExportedTypes();
            }

            HashSet<string> namespaces = new HashSet<string>();
            if (types != null)
            {
                foreach (Type type in types)
                {
                    string ns = type.Namespace;
                    if (!namespaces.Contains(ns))
                    {
                        namespaces.Add(ns);
                    }
                }
            }

            return namespaces;
        }

        void GetAvailableNamespaces()
        {
            Fx.Assert(this.availableNamespaces != null, "available namespace table should have been set before calling this method");
            AssemblyContextControlItem assemblyItem = this.Context.Items.GetValue<AssemblyContextControlItem>();
            if (assemblyItem != null)
            {
                IMultiTargetingSupportService multiTargetingService = this.Context.Services.GetService<IMultiTargetingSupportService>();

                ////When ReferencedAssemblyNames is null, it's in rehost scenario. And we need to preload assemblies in 
                ////TextExpression.ReferencesForImplementation/References if there is any. So that these assemblies will be returned
                ////by AssemblyContextControlItem.GetEnvironmentAssemblies and user can see namespaces defined in these assemlbies 
                ////in the dropdown list of Import Designer.
                if ((assemblyItem.ReferencedAssemblyNames == null) && (this.targetFramework.Is45OrHigher()))
                {
                    ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
                    object root = modelTreeManager.Root.GetCurrentValue();
                    IList<AssemblyReference> references;
                    NamespaceHelper.GetTextExpressionNamespaces(root, out references);
                    foreach (AssemblyReference reference in references)
                    {
                        reference.LoadAssembly();
                    }
                }

                IEnumerable<Assembly> allAssemblies = assemblyItem.GetEnvironmentAssemblies(multiTargetingService);
                if (assemblyItem.LocalAssemblyName != null)
                {
                    allAssemblies = allAssemblies.Union<Assembly>(new Collection<Assembly> { 
                        AssemblyContextControlItem.GetAssembly(assemblyItem.LocalAssemblyName, multiTargetingService)
                    });
                }

                foreach (Assembly assembly in allAssemblies)
                {
                    try
                    {
                        if (assembly != null)
                        {
                            Fx.Assert(!assembly.IsDynamic, "there should not be any dynamic assemblies in reference list");
                            this.UpdateAvailableNamespaces(assembly, assemblyItem);
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }

                if (assemblyItem.LocalAssemblyName == null)
                {
                    AppDomain.CurrentDomain.AssemblyLoad += this.proxy.OnAssemblyLoad;
                }
            }
        }

        void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (!args.LoadedAssembly.IsDynamic)
            {
                this.Dispatcher.BeginInvoke(new Action(() => { this.OnReferenceUpdated(args.LoadedAssembly, true); }), DispatcherPriority.Send);
            }
        }

        void ValidateImportedNamespaces()
        {
            this.importedNamespacesItem.ImportedNamespaces.Clear();
            foreach (ModelItem namespaceModel in this.importsModelItem)
            {
                ValidateNamespaceModelAndUpdateContextItem(namespaceModel);
            }

            UpdateExpressionEditors();
        }

        //Helper function used by other components if need to update import context item
        public static void AddImport(string importedNamespace, EditingContext editingContext)
        {
            //For types defined without any namespace, Type.Namespace is null instead of empty. We don't need to add any namespace to Import list in this case
            if (string.IsNullOrEmpty(importedNamespace))
            {
                return;
            }

            Fx.Assert(editingContext != null, "EditingContext shouldn't be null.");
            ModelService modelService = editingContext.Services.GetService<ModelService>();
            Fx.Assert(modelService != null, "EditingContext should contains ModelService.");
            ModelItemCollection importsCollection = modelService.Root.Properties[NamespaceListPropertyDescriptor.ImportCollectionPropertyName].Collection;
            NamespaceList namespaceList = importsCollection.GetCurrentValue() as NamespaceList;
            if (namespaceList.Lookup(importedNamespace) == -1)
            {
                importsCollection.Add(new NamespaceData { Namespace = importedNamespace });
            }
            else
            {
                namespaceList.UpdateAssemblyInfo(importedNamespace);
            }
        }

        private sealed class ImportDesignerProxy
        {
            private WeakReference reference;

            public ImportDesignerProxy(ImportDesigner importDesigner)
            {
                this.reference = new WeakReference(importDesigner);
            }

            public void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
            {
                ImportDesigner importDesigner = this.reference.Target as ImportDesigner;
                if (importDesigner != null)
                {
                    importDesigner.OnAssemblyLoad(sender, args);
                }
            }
        }
    }
}
