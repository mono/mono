//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.ComponentModel.Design;
    using System.Activities.Presentation.Hosting;
    using System.Windows.Threading;
    using Microsoft.Activities.Presentation;

    internal sealed partial class TypeBrowser : DialogWindow
    {
        static readonly DependencyProperty SelectedTypeProperty =
            DependencyProperty.Register("SelectedType",
            typeof(Type),
            typeof(TypeBrowser),
            new UIPropertyMetadata());

        static readonly DependencyProperty HasGenericTypesProperty =
            DependencyProperty.Register("HasGenericTypes",
            typeof(bool),
            typeof(TypeBrowser));

        static readonly DependencyProperty GenericTypeNameProperty =
            DependencyProperty.Register("GenericTypeName",
            typeof(String),
            typeof(TypeBrowser));

        static readonly DependencyProperty GenericTypeMappingProperty =
            DependencyProperty.Register("GenericTypeMapping",
            typeof(ObservableCollection<TypeKeyValue>),
            typeof(TypeBrowser));

        static readonly DependencyProperty ConcreteTypeProperty =
            DependencyProperty.Register("ConcreteType",
            typeof(Type),
            typeof(TypeBrowser));

        static Size size = Size.Empty;

        SearchAction currentSearch = null;

        ObservableCollection<AssemblyNode> localAssemblies;
        ObservableCollection<AssemblyNode> referenceAssemblies;
        AssemblyContextControlItem assemblyContext;
        Func<Type, bool> filter;
        DesignerPerfEventProvider perfEventProvider;

        public TypeBrowser(AssemblyContextControlItem assemblyContext, EditingContext context, Func<Type, bool> filter)
        {
            this.assemblyContext = assemblyContext;
            this.Context = context;
            this.filter = filter;
            SetValue(GenericTypeMappingProperty, new ObservableCollection<TypeKeyValue>());
            GenericTypeMapping.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(GenericTypeMappingCollectionChanged);
            InitializeComponent();
            this.typeEntryTextBox.Focus();

            if (!size.IsEmpty)
            {
                this.Height = size.Height;
                this.Width = size.Width;
            }

            this.SizeChanged += new SizeChangedEventHandler(TypeBrowser_SizeChanged);

            this.HelpKeyword = HelpKeywords.TypeBrowser;
            this.perfEventProvider = new DesignerPerfEventProvider();
        }

        static void TypeBrowser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TypeBrowser.size = e.NewSize;
        }

        Type SelectedType
        {
            get { return (Type)GetValue(SelectedTypeProperty); }
            set { SetValue(SelectedTypeProperty, value); }
        }

        bool HasGenericTypes
        {
            get { return (bool)GetValue(HasGenericTypesProperty); }
            set { SetValue(HasGenericTypesProperty, value); }
        }

        string GenericTypeName
        {
            get { return (string)GetValue(GenericTypeNameProperty); }
            set { SetValue(GenericTypeNameProperty, value); }
        }

        ObservableCollection<TypeKeyValue> GenericTypeMapping
        {
            get { return (ObservableCollection<TypeKeyValue>)GetValue(GenericTypeMappingProperty); }
            set { SetValue(GenericTypeMappingProperty, value); }
        }

        public Type ConcreteType
        {
            get { return (Type)GetValue(ConcreteTypeProperty); }
            private set { SetValue(ConcreteTypeProperty, value); }
        }

        public ObservableCollection<AssemblyNode> LocalAssemblies
        {
            get
            {
                if (null == this.localAssemblies)
                {
                    this.localAssemblies = new ObservableCollection<AssemblyNode>();
                    if (null != this.assemblyContext)
                    {
                        if (null != this.assemblyContext.LocalAssemblyName)
                        {
                            IMultiTargetingSupportService multiTargetingSupportService = this.Context.Services.GetService<IMultiTargetingSupportService>();
                            Assembly local = AssemblyContextControlItem.GetAssembly(this.assemblyContext.LocalAssemblyName, multiTargetingSupportService);
                            if (local != null)
                            {
                                this.localAssemblies.Add(new AssemblyNode(local, true, this.filter, this.Context));
                            }
                        }
                    }

                    if (this.localAssemblies.Count == 0)
                    {
                        this.LocalAssembly.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        this.LocalAssembly.Visibility = Visibility.Visible;
                    }
                }
                return this.localAssemblies;
            }
        }

        public ObservableCollection<AssemblyNode> ReferenceAssemblies
        {
            get
            {
                if (null == this.referenceAssemblies)
                {
                    this.referenceAssemblies = new ObservableCollection<AssemblyNode>();
                    if (null != this.assemblyContext)
                    {
                        IMultiTargetingSupportService multiTargetingSupportService = this.Context.Services.GetService<IMultiTargetingSupportService>();
                        IEnumerable<Assembly> assemblies = this.assemblyContext.GetEnvironmentAssemblies(multiTargetingSupportService);
                        foreach (Assembly assembly in assemblies.OrderBy<Assembly, string>(p => p.FullName))
                        {
                            this.referenceAssemblies.Add(new AssemblyNode(assembly, false, this.filter, this.Context));
                        }
                    }
                }
                return this.referenceAssemblies;
            }
        }

        public bool? ShowDialog(DependencyObject owner)
        {
            WindowHelperService.TrySetWindowOwner(owner, this.Context, this);
            this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { this.perfEventProvider.TypeBrowserApplicationIdleAfterShowDialog(); }));
            return base.ShowDialog();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && null != SelectedType)
            {
                OnDialogClose();
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new UIElementAutomationPeer(this);
        }

        private void GenericTypeMappingCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.HasGenericTypes = GenericTypeMapping.Count > 0 ? true : false;
            if (this.HasGenericTypes)
            {
                string strName = this.SelectedType.FullName;
                this.GenericTypeName = strName.Substring(0, strName.Length - 2) + " <";
            }
            else
            {
                this.GenericTypeName = null;
            }
        }

        private Type ResolveType(out string errorTitle, out string errorMessage)
        {
            errorTitle = null;
            errorMessage = null;
            Type result = this.SelectedType;

            try
            {
                IMultiTargetingSupportService multiTargetingSupport = this.Context.Services.GetService<IMultiTargetingSupportService>();
                if (multiTargetingSupport != null)
                {
                    result = multiTargetingSupport.GetRuntimeType(result);
                }

                if (result == null)
                {
                    errorTitle = SR.TypeBrowserErrorMessageTitle;
                    errorMessage = SR.TypeBrowserError;
                    return null;
                }

                if (result.IsGenericTypeDefinition)
                {
                    bool isValid = true;
                    //get number of generic parameters in edited type
                    Type[] arguments = new Type[this.GenericTypeMapping.Count];

                    //for each argument, get resolved type
                    for (int i = 0; i < this.GenericTypeMapping.Count && isValid; ++i)
                    {
                        arguments[i] = this.GenericTypeMapping[i].GetConcreteType();
                        if (multiTargetingSupport != null && arguments[i] != null)
                        {
                            arguments[i] = multiTargetingSupport.GetRuntimeType(arguments[i]);
                        }
                        isValid = isValid && (null != arguments[i]);
                    }

                    //if all parameters are resolved, create concrete type
                    if (isValid)
                    {
                        result = result.MakeGenericType(arguments);
                    }
                    else
                    {
                        errorTitle = SR.TypeBrowserErrorMessageTitle;
                        errorMessage = SR.TypeResolverError;
                        result = null;
                    }
                }
            }
            catch (ArgumentException err)
            {
                errorTitle = err.GetType().Name;
                errorMessage = err.Message;
                return null;
            }

            return result;
        }

        private void OnOkClick(object sender, RoutedEventArgs args)
        {
            OnDialogClose();
        }

        private void OnCancelClick(object sender, RoutedEventArgs args)
        {
            this.DialogResult = false;
        }

        private void OnTypeDoubleClick(object sender, RoutedEventArgs args)
        {
            if (((System.Windows.Input.MouseButtonEventArgs)(args)).ChangedButton == MouseButton.Left)
            {
                TypeNode entry = ((TreeViewItem)sender).Header as TypeNode;
                if (null != entry && entry.Data is Type)
                {
                    OnDialogClose();
                    args.Handled = true;
                }
            }
        }

        private void OnDialogClose()
        {
            string errorTitle = null;
            string errorMessage = null;

            Type type = ResolveType(out errorTitle, out errorMessage);
            if (null != type)
            {
                this.ConcreteType = type;
                this.DialogResult = true;
                this.perfEventProvider.TypeBrowserOkPressed();
            }
            else
            {
                MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnTypeBrowserClickStart(object sender, RoutedEventArgs args)
        {
            TreeViewItem item = sender as TreeViewItem;
            NamespaceNode ns = null;
            if (null != item)
            {
                ns = item.Header as NamespaceNode;
            }

            if (null != ns && null == ns.Tag)
            {
                ns.Tag = string.Empty;
                Mouse.OverrideCursor = Cursors.Wait;
            }
        }

        private void OnTypeBrowserClickEnd(object sender, RoutedEventArgs args)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (null != item && item.Header is AssemblyNode && null != Mouse.OverrideCursor)
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void OnTypeSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = ((TextBox)sender).Text;

            SearchAction newSearch = new SearchAction(searchText, this.localAssemblies, this.referenceAssemblies);
            newSearch.Completed += delegate(object s, EventArgs args)
            {
                SearchAction senderAction = s as SearchAction;
                SearchAction currentAction = this.currentSearch;
                this.currentSearch = null;

                if (senderAction == currentAction)
                {
                    TypeNode match = ((SearchActionEventArgs)args).Result as TypeNode;

                    UpdateSelectedItem(match);
                    if (match != null)
                    {
                        match.IsSelected = true;
                    }
                }
            };

            if (this.currentSearch != null)
            {
                this.currentSearch.Abort();
            }

            ClearSelection();

            this.currentSearch = newSearch;
            this.currentSearch.Run();
        }

        private void ClearSelection()
        {
            TypeNode currentSelection = this.typesTreeView.SelectedItem as TypeNode;
            if (currentSelection != null)
            {
                currentSelection.IsSelected = false;
            }
        }

        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TypeNode entry = ((System.Windows.Controls.TreeView)sender).SelectedItem as TypeNode;
            if (entry != null && this.SelectedType != entry.Data as Type)
            {
                UpdateSelectedItem(entry);
                if (null != this.SelectedType)
                {
                    typeEntryTextBox.TextChanged -= new TextChangedEventHandler(OnTypeSearchTextChanged);
                    typeEntryTextBox.Text = TypeNameHelper.GetDisplayName(this.SelectedType, true);
                    typeEntryTextBox.TextChanged += new TextChangedEventHandler(OnTypeSearchTextChanged);
                }
            }
        }

        private void UpdateSelectedItem(TypeNode entry)
        {
            GenericTypeMapping.Clear();

            SelectedType = (null != entry ? entry.Data as Type : null);

            if (null != this.SelectedType)
            {
                if (this.SelectedType.IsGenericTypeDefinition)
                {
                    this.ConcreteType = null;
                    Type[] generics = this.SelectedType.GetGenericArguments();
                    foreach (Type t in generics)
                    {
                        this.GenericTypeMapping.Add(new TypeKeyValue(t, null));
                    }
                }
                else
                {
                    this.ConcreteType = this.SelectedType;
                }
            }
            else
            {
                this.ConcreteType = null;
            }
        }

        internal class Node : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private bool isExpanded;
            private bool isSelected;
            private Visibility visibility;

            protected Node()
            {
                this.isExpanded = false;
                this.isSelected = false;
                this.visibility = Visibility.Visible;
            }

            protected void Notify(string property)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                }
            }

            public bool IsExpanded
            {
                get
                {
                    return this.isExpanded;
                }
                set
                {
                    if (this.isExpanded != value)
                    {
                        this.isExpanded = value;
                        Notify("IsExpanded");
                    }
                }
            }

            public bool IsSelected
            {
                get
                {
                    return this.isSelected;
                }
                set
                {
                    if (this.isSelected != value)
                    {
                        this.isSelected = value;
                        Notify("IsSelected");
                    }
                }
            }

            public Visibility Visibility
            {
                get
                {
                    return this.visibility;
                }
                set
                {
                    if (this.visibility != value)
                    {
                        this.visibility = value;
                        Notify("Visibility");
                    }
                }
            }

            public object Tag
            {
                get;
                set;
            }
        }

        internal class AssemblyNode : Node
        {
            string displayName;
            Assembly assembly;
            bool isLocal;
            Func<Type, bool> filter;
            EditingContext context;

            public AssemblyNode(Assembly assembly, bool isLocal, Func<Type, bool> filter, EditingContext context)
            {
                if (null == assembly)
                {
                    throw FxTrace.Exception.AsError(new ArgumentNullException("assembly"));
                }
                this.assembly = assembly;
                this.isLocal = isLocal;
                this.displayName = GetDisplayName(this.assembly.GetName());
                this.filter = filter;
                this.context = context;
            }

            private static string GetDisplayName(AssemblyName name)
            {
                StringBuilder sb = new StringBuilder();
                if (name != null && name.Name != null)
                {
                    sb.Append(name.Name);
                    if (name.Version != null)
                    {
                        sb.Append(" [");
                        sb.Append(name.Version.Major);
                        sb.Append(".");
                        sb.Append(name.Version.Minor);
                        sb.Append(".");
                        sb.Append(name.Version.Build);
                        sb.Append(".");
                        sb.Append(name.Version.Revision);
                        sb.Append("]");
                    }
                }
                return sb.ToString();
            }

            public string DisplayName
            {
                get { return this.displayName; }
            }

            private ObservableCollection<NamespaceNode> namespaces;
            public ObservableCollection<NamespaceNode> Namespaces
            {
                get
                {
                    if (namespaces == null)
                    {
                        namespaces = new ObservableCollection<NamespaceNode>();

                        try
                        {
                            Func<Type, bool> typeFilter = this.filter;
                            IMultiTargetingSupportService multiTargetingSupport = this.context.Services.GetService<IMultiTargetingSupportService>();
                            if (multiTargetingSupport != null && typeFilter != null)
                            {
                                typeFilter = (type) => this.filter(multiTargetingSupport.GetRuntimeType(type));
                            }

                            var exportedTypes =
                                from type in (this.isLocal ? this.assembly.GetTypes() : this.assembly.GetExportedTypes())
                                where (type.IsPublic && type.IsVisible && (typeFilter == null || typeFilter(type)))
                                orderby type.Namespace, type.Name
                                select type;

                            NamespaceNode lastNamespace = null;
                            foreach (Type type in exportedTypes)
                            {
                                if (lastNamespace == null || !StringComparer.OrdinalIgnoreCase.Equals(lastNamespace.DisplayName, type.Namespace))
                                {
                                    lastNamespace = new NamespaceNode(type.Namespace);
                                    namespaces.Add(lastNamespace);
                                }

                                lastNamespace.Types.Add(new TypeNode(type));
                            }
                        }
                        catch (NotSupportedException)
                        {
                            //Dynamic (in memory) assemblies will throw exception when this method is called
                            //that's the reason i'm swollowing that exception
                        }

                    }
                    return namespaces;
                }
            }

            public string Data
            {
                get { return this.displayName; }
            }

            public override string ToString()
            {
                return this.displayName;
            }
        }

        internal class NamespaceNode : Node
        {

            private string displayName;
            public string DisplayName
            {
                get
                {
                    return this.displayName;
                }
            }

            private ObservableCollection<TypeNode> types;
            public ObservableCollection<TypeNode> Types
            {
                get
                {
                    return this.types;
                }
            }

            public string Data
            {
                get { return this.displayName; }
            }

            public NamespaceNode(string name)
            {
                this.displayName = name;
                this.types = new ObservableCollection<TypeNode>();
            }
        }

        internal class TypeNode : Node
        {
            private Type type;

            private string displayName;
            public string DisplayName
            {
                get
                {
                    return this.displayName;
                }
            }

            private string fullName;
            public string FullName
            {
                get
                {
                    return this.fullName;
                }
            }

            public Type Data
            {
                get { return this.type; }
            }

            public TypeNode(Type type)
            {
                this.type = type;
                this.displayName = TypeNameHelper.GetDisplayName(type, false);
                this.fullName = TypeNameHelper.GetDisplayName(type, true);
            }
        }

        private class SearchActionEventArgs : EventArgs
        {
            public object Result
            {
                get;
                set;
            }
        }

        private class SearchAction
        {
            private string searchText;
            Collection<AssemblyNode>[] range;
            DispatcherOperation dispatcherOperation;

            public event EventHandler Completed;

            public SearchAction(string searchText, Collection<AssemblyNode> localAssemblies, Collection<AssemblyNode> referenceAssemblies)
            {
                this.searchText = searchText;
                this.range = new Collection<AssemblyNode>[] { localAssemblies, referenceAssemblies };
            }


            public void Run()
            {
                this.dispatcherOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new Func<TypeNode>(this.OnRun));
                this.dispatcherOperation.Completed += this.OnCompleted;
            }

            public bool Abort()
            {
                if (this.dispatcherOperation != null)
                {
                    return this.dispatcherOperation.Abort();
                }
                return true;
            }

            private TypeNode OnRun()
            {
                bool noSearch = string.IsNullOrEmpty(searchText);
                Func<TypeNode, string, bool> matchAlgorithm = SearchAction.MatchShortName;

                TypeNode match = null;
                TypeNode firstCandidate = null;
                bool tooManyCandiates = false;

                if (!noSearch && searchText.Contains('.'))
                {
                    matchAlgorithm = SearchAction.MatchFullName;
                }

                foreach (Collection<AssemblyNode> assemblies in this.range)
                {
                    foreach (AssemblyNode assembly in assemblies)
                    {
                        Visibility assemblyVisibility = Visibility.Collapsed;
                        bool assemblyIsExpanded = false;

                        if (noSearch)
                        {
                            assemblyVisibility = Visibility.Visible;
                        }

                        foreach (NamespaceNode ns in assembly.Namespaces)
                        {
                            Visibility namespaceVisibility = Visibility.Collapsed;
                            bool namespaceIsExpanded = false;

                            if (noSearch)
                            {
                                namespaceVisibility = Visibility.Visible;
                            }

                            foreach (TypeNode entry in ns.Types)
                            {
                                if (noSearch)
                                {
                                    entry.Visibility = Visibility.Visible;
                                }
                                else if (matchAlgorithm(entry, searchText))
                                {
                                    entry.Visibility = Visibility.Visible;
                                    assemblyVisibility = Visibility.Visible;
                                    assemblyIsExpanded = true;
                                    namespaceVisibility = Visibility.Visible;
                                    namespaceIsExpanded = true;

                                    if (string.Equals(searchText, entry.FullName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        match = entry;
                                    }

                                    if (firstCandidate == null)
                                    {
                                        firstCandidate = entry;
                                    }
                                    else if (!tooManyCandiates)
                                    {
                                        tooManyCandiates = true;
                                    }
                                }
                                else
                                {
                                    entry.Visibility = Visibility.Collapsed;
                                }
                            }

                            if (searchText.Contains('.') && ns.DisplayName != null && ns.DisplayName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                            {
                                namespaceIsExpanded = false;
                            }

                            if (namespaceIsExpanded)
                            {
                                ns.Tag = string.Empty;
                            }

                            ns.Visibility = namespaceVisibility;
                            ns.IsExpanded = namespaceIsExpanded;
                        }

                        assembly.Visibility = assemblyVisibility;
                        assembly.IsExpanded = assemblyIsExpanded;
                    }
                }

                if (match == null && !tooManyCandiates)
                {
                    match = firstCandidate;
                }

                return match;
            }

            private void OnCompleted(object sender, EventArgs args)
            {
                this.dispatcherOperation.Completed -= this.OnCompleted;
                if (this.Completed != null)
                {
                    SearchActionEventArgs arg = new SearchActionEventArgs();
                    arg.Result = this.dispatcherOperation.Result;
                    this.Completed(this, arg);
                }
            }

            // "abc.def" matches regex ^.*\.abc\.def.* or ^abc\.def.*, but does not match ^.*[^.]abc\.def.*
            private static bool MatchFullName(TypeNode type, string searchText)
            {
                if (searchText.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    return -1 != type.FullName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                }

                if (type.FullName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (-1 != type.FullName.IndexOf("." + searchText, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }

            private static bool MatchShortName(TypeNode type, string searchText)
            {
                return type.DisplayName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
