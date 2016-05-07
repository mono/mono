//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Runtime;
    using System.Windows.Automation;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Threading;
    using System.Activities.Presentation.Hosting;
    using Microsoft.Activities.Presentation;

    // This control presents a System.Type as textblock, which is editable on click, or F2.
    public sealed partial class TypePresenter : ContentControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context",
            typeof(EditingContext),
            typeof(TypePresenter),
            new PropertyMetadata(new PropertyChangedCallback(OnContextChanged)));

        public static readonly DependencyProperty AllowNullProperty =
            DependencyProperty.Register("AllowNull",
            typeof(bool),
            typeof(TypePresenter),
            new PropertyMetadata(false, OnAllowNullChanged));

        public static readonly DependencyProperty BrowseTypeDirectlyProperty =
            DependencyProperty.Register("BrowseTypeDirectly",
            typeof(bool),
            typeof(TypePresenter),
            new PropertyMetadata(false, OnBrowseTypeDirectlyChanged));

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type",
            typeof(Type),
            typeof(TypePresenter),
            new PropertyMetadata(null, new PropertyChangedCallback(OnTypeChanged)));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label",
            typeof(string),
            typeof(TypePresenter),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter",
            typeof(Func<Type, bool>),
            typeof(TypePresenter),
            new PropertyMetadata(new PropertyChangedCallback(OnFilterChanged)));

        static readonly DependencyPropertyKey TextPropertyKey = DependencyProperty.RegisterReadOnly(
            "Text",
            typeof(string),
            typeof(TypePresenter),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty TextProperty = TextPropertyKey.DependencyProperty;

        public static readonly DependencyProperty MostRecentlyUsedTypesProperty =
            DependencyProperty.Register("MostRecentlyUsedTypes",
            typeof(ObservableCollection<Type>),
            typeof(TypePresenter),
            new PropertyMetadata(TypePresenter.DefaultMostRecentlyUsedTypes, new PropertyChangedCallback(OnMostRecentlyUsedTypesPropertyChanged), new CoerceValueCallback(OnCoerceMostRecentlyUsedTypes)));

        public static readonly DependencyProperty CenterActivityTypeResolverDialogProperty =
            DependencyProperty.Register("CenterActivityTypeResolverDialog",
            typeof(bool),
            typeof(TypePresenter),
            new PropertyMetadata(true));

        public static readonly DependencyProperty CenterTypeBrowserDialogProperty =
            DependencyProperty.Register("CenterTypeBrowserDialog",
            typeof(bool),
            typeof(TypePresenter),
            new PropertyMetadata(true));

        public static readonly RoutedEvent TypeBrowserOpenedEvent = EventManager.RegisterRoutedEvent(
            "TypeBrowserOpened",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TypePresenter));

        public static readonly RoutedEvent TypeBrowserClosedEvent = EventManager.RegisterRoutedEvent(
            "TypeBrowserClosed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TypePresenter));

        public static readonly RoutedEvent TypeChangedEvent = EventManager.RegisterRoutedEvent(
            "TypeChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TypePresenter));

        static List<Type> defaultTypes = null;
        static ObservableCollection<Type> defaultMostRecentlyUsedTypes;

        internal static List<Type> DefaultTypes
        {
            get
            {
                if (defaultTypes == null)
                {
                    defaultTypes = new List<Type>
                    {
                        typeof(Boolean),
                        typeof(Int32),
                        typeof(String),
                        typeof(Object),
                    };
                }
                return defaultTypes;
            }
        }

        public static ObservableCollection<Type> DefaultMostRecentlyUsedTypes
        {
            get
            {
                if (defaultMostRecentlyUsedTypes == null)
                {
                    defaultMostRecentlyUsedTypes = new ObservableCollection<Type>(DefaultTypes);
                }
                return defaultMostRecentlyUsedTypes;
            }
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.CollectionPropertiesShouldBeReadOnly,
            Justification = "Setter is provided to bind data on this property.")]
        [Fx.Tag.KnownXamlExternal]
        public ObservableCollection<Type> MostRecentlyUsedTypes
        {
            get { return (ObservableCollection<Type>)GetValue(MostRecentlyUsedTypesProperty); }
            set { SetValue(MostRecentlyUsedTypesProperty, value); }
        }

        bool isMouseLeftButtonDown = true;
        Type lastSelection;
        TypeWrapper nullTypeWrapper = null;

        public TypePresenter()
        {
            InitializeComponent();

            OnBrowseTypeDirectlyChanged(this, new DependencyPropertyChangedEventArgs(
                TypePresenter.BrowseTypeDirectlyProperty, false, this.BrowseTypeDirectly));
            DisableEdit();

            this.typeComboBox.DropDownClosed += OnTypePresenterDropDownClosed;
            this.typeComboBox.PreviewLostKeyboardFocus += OnTypePresenterComboBoxPreviewLostKeyboardFocus;
            this.typeComboBox.LostFocus += OnTypePresenterComboBoxLostFocus;
            this.typeComboBox.KeyDown += OnTypePresenterKeyDown;

            Binding textToType = new Binding();
            textToType.Converter = new TypeWrapperConverter(this);
            textToType.Source = this;
            textToType.Path = new PropertyPath(TypeProperty);
            this.typeComboBox.SetBinding(ComboBox.SelectedItemProperty, textToType);
            this.lastSelection = (Type)TypeProperty.DefaultMetadata.DefaultValue;

            MultiBinding automationNameBinding = new MultiBinding();
            Binding labelBinding = new Binding("Label");
            labelBinding.Source = this;
            automationNameBinding.Bindings.Add(labelBinding);
            Binding typeBinding = new Binding("Text");
            typeBinding.Source = this.typeTextBlock;
            automationNameBinding.Bindings.Add(typeBinding);
            automationNameBinding.Converter = new AutomationNameConverter();
            this.SetBinding(AutomationProperties.NameProperty, automationNameBinding);

            this.Loaded += new RoutedEventHandler(TypePresenter_Loaded);
            this.Unloaded += new RoutedEventHandler(TypePresenter_Unloaded);
        }

        void OnTypePresenterComboBoxPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (this.typeComboBox.Visibility == Visibility.Visible && this.typeComboBox.IsDropDownOpen)
            {
                e.Handled = true;
            }
        }

        void OnTypePresenterComboBoxLostFocus(object sender, RoutedEventArgs e)
        {
            TypeWrapper tw = (TypeWrapper)this.typeComboBox.SelectedItem;
            if (tw != null)
            {
                if (tw.Type == typeof(ArrayOf<>) || tw.Type == typeof(BrowseForType))
                {
                    SetComboBoxToLastSelection();
                }
            }
        }

        void SetComboBoxToLastSelection()
        {
            if (this.lastSelection == null)
            {
                this.typeComboBox.SelectedIndex = this.typeComboBox.Items.IndexOf(this.NullTypeWrapper);
            }
            else
            {
                for (int i = 0; i < this.typeComboBox.Items.Count; i++)
                {
                    TypeWrapper typeWrapper = (TypeWrapper)this.typeComboBox.Items.GetItemAt(i);
                    if (typeWrapper.IsTypeDefinition && Type.Equals(this.lastSelection, typeWrapper.Type))
                    {
                        this.typeComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        public void FocusOnVisibleControl()
        {
            if (BrowseTypeDirectly)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                {
                    Keyboard.Focus(this.typeTextBlock);
                }));
            }
            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                {
                    Keyboard.Focus(this.typeComboBox);
                }));
            }
        }


        void TypePresenter_Loaded(object sender, RoutedEventArgs e)
        {
            //UnRegistering because of 137896: Inside tab control multiple Loaded events happen without an Unloaded event.
            this.MostRecentlyUsedTypes.CollectionChanged -= OnMostRecentlyUsedTypesChanged;
            this.MostRecentlyUsedTypes.CollectionChanged += OnMostRecentlyUsedTypesChanged;
            OnMostRecentlyUsedTypesChanged(this, null);
        }

        void TypePresenter_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MostRecentlyUsedTypes.CollectionChanged -= OnMostRecentlyUsedTypesChanged;
        }

        public event RoutedEventHandler TypeBrowserOpened
        {
            add { this.AddHandler(TypeBrowserOpenedEvent, value); }
            remove { this.RemoveHandler(TypeBrowserOpenedEvent, value); }
        }

        public event RoutedEventHandler TypeBrowserClosed
        {
            add { this.AddHandler(TypeBrowserClosedEvent, value); }
            remove { this.RemoveHandler(TypeBrowserClosedEvent, value); }
        }

        public event RoutedEventHandler TypeChanged
        {
            add { this.AddHandler(TypeChangedEvent, value); }
            remove { this.RemoveHandler(TypeChangedEvent, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Fx.Tag.KnownXamlExternal]
        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public bool AllowNull
        {
            get { return (bool)GetValue(AllowNullProperty); }
            set { SetValue(AllowNullProperty, value); }
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public Func<Type, bool> Filter
        {
            get { return (Func<Type, bool>)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public bool CenterActivityTypeResolverDialog
        {
            get { return (bool)GetValue(CenterActivityTypeResolverDialogProperty); }
            set { SetValue(CenterActivityTypeResolverDialogProperty, value); }
        }

        public bool CenterTypeBrowserDialog
        {
            get { return (bool)GetValue(CenterTypeBrowserDialogProperty); }
            set { SetValue(CenterTypeBrowserDialogProperty, value); }
        }

        internal TypeWrapper NullTypeWrapper
        {
            get
            {
                if (this.nullTypeWrapper == null)
                {
                    this.nullTypeWrapper = new TypeWrapper(NullString, "Null", null);
                }
                return this.nullTypeWrapper;
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            private set { SetValue(TextPropertyKey, value); }
        }


        public IEnumerable<TypeWrapper> Items
        {
            get
            {
                if (AllowNull)
                {
                    yield return this.NullTypeWrapper;
                }
                foreach (Type type in this.MostRecentlyUsedTypes)
                {
                    if (type != null)
                    {
                        if (this.Filter == null
                           || this.Filter(type))
                        {
                            yield return new TypeWrapper(type);
                        }
                    }
                }

                //display Array of [T] option
                if (this.Filter == null
                    || this.Filter(typeof(Array)))
                {
                    yield return new TypeWrapper("Array of [T]", "T[]", typeof(ArrayOf<>));
                }
                //display "Browse for types" option
                //if there are referenced and local assembly info in Editing context (inside VS), type browser will show those assemblies,
                //otherwise (standalone), type browser will just show all loaded assemblies in current appdomain
                yield return new TypeWrapper(BrowseTypeString, "BrowseForTypes", typeof(BrowseForType));
            }
        }

        public bool BrowseTypeDirectly
        {
            get { return (bool)GetValue(BrowseTypeDirectlyProperty); }
            set { SetValue(BrowseTypeDirectlyProperty, value); }
        }


        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "By design.")]
        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public string TypeName
        {
            get
            {
                string typeName = string.Empty;
                this.ToolTip = null;
                if (null != this.Type)
                {
                    typeName = ResolveTypeName(this.Type);
                    this.ToolTip = typeName;
                }
                return typeName;
            }
        }

        internal static string ResolveTypeName(Type type)
        {
            Fx.Assert(type != null, "parameter type is null!");
            string typeName;
            if (TypePresenter.DefaultTypes.Contains(type))
            {
                typeName = type.Name;
            }
            else
            {
                typeName = TypeNameHelper.GetDisplayName(type, true);
            }
            return typeName;
        }

        AssemblyContextControlItem AssemblyContext
        {
            get
            {
                return (null != Context ? Context.Items.GetValue<AssemblyContextControlItem>() : null);
            }
        }

        string BrowseTypeString
        {
            get { return (string)this.FindResource("BrowseTypeString"); }
        }

        string NullString
        {
            get { return "(null)"; }
        }

        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new UIElementAutomationPeer(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.isMouseLeftButtonDown = true;
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (this.isMouseLeftButtonDown)
            {
                if (this.BrowseTypeDirectly)
                {
                    HandleBrowseType();
                }
                else
                {
                    this.EnableEdit();
                }
            }
            this.isMouseLeftButtonDown = false;
            e.Handled = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (IsPreviewKey(e.Key))
            {
                Preview();
            }
        }

        internal static bool IsPreviewKey(Key key)
        {
            return (key == Key.F2 || key == Key.Space || key == Key.Enter);
        }

        internal void Preview()
        {
            if (this.BrowseTypeDirectly)
            {
                HandleBrowseType();
            }
            else
            {
                this.EnableEdit();
            }
        }

        static void OnContextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TypePresenter ctrl = (TypePresenter)sender;
            ctrl.OnItemsChanged();
        }

        static void OnAllowNullChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TypePresenter ctrl = (TypePresenter)sender;
            ctrl.OnItemsChanged();
        }

        static void OnBrowseTypeDirectlyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TypePresenter ctrl = (TypePresenter)sender;
            if (!(bool)args.NewValue)
            {
                ctrl.typeComboBox.Visibility = Visibility.Visible;
                ctrl.typeTextBlock.Visibility = Visibility.Collapsed;
                ctrl.Focusable = false;
            }
            else
            {
                ctrl.typeComboBox.Visibility = Visibility.Collapsed;
                ctrl.typeTextBlock.Visibility = Visibility.Visible;
                ctrl.Focusable = true;
            }
        }

        static void OnTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TypePresenter ctrl = (TypePresenter)sender;
            ctrl.lastSelection = (Type)args.NewValue;

            if (null != ctrl.PropertyChanged)
            {
                ctrl.PropertyChanged(ctrl, new PropertyChangedEventArgs("TypeName"));
            }

            if (null == ctrl.lastSelection)
            {
                ctrl.typeComboBox.SelectedIndex = ctrl.typeComboBox.Items.IndexOf(ctrl.NullTypeWrapper);
            }

            ctrl.Text = ctrl.TypeName;
            ctrl.RaiseEvent(new RoutedEventArgs(TypePresenter.TypeChangedEvent, ctrl));
        }

        static void OnFilterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TypePresenter ctrl = (TypePresenter)sender;
            if (null != ctrl.PropertyChanged)
            {
                ctrl.PropertyChanged(ctrl, new PropertyChangedEventArgs("Items"));
            }
        }

        static void OnMostRecentlyUsedTypesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TypePresenter ctrl = (TypePresenter)sender;
            ((ObservableCollection<Type>)args.NewValue).CollectionChanged += ctrl.OnMostRecentlyUsedTypesChanged;
            ((ObservableCollection<Type>)args.OldValue).CollectionChanged -= ctrl.OnMostRecentlyUsedTypesChanged;

            ctrl.OnItemsChanged();
        }

        static object OnCoerceMostRecentlyUsedTypes(DependencyObject sender, object value)
        {
            if (value != null)
            {
                return value;
            }
            else
            {
                return TypePresenter.DefaultMostRecentlyUsedTypes;
            }
        }

        void DisableEdit()
        {
            if (BrowseTypeDirectly)
            {
                this.typeTextBlock.Visibility = Visibility.Visible;
                this.typeComboBox.Visibility = Visibility.Collapsed;
            }
        }

        void EnableEdit()
        {
            if (BrowseTypeDirectly)
            {
                this.typeTextBlock.Visibility = Visibility.Collapsed;
                this.typeComboBox.Visibility = Visibility.Visible;
            }
            this.typeComboBox.Focus();
        }

        // return true if KeyDownEvent should be set to handled
        bool HandleBrowseType()
        {
            bool retval = false;
            TypeWrapper wrapper = (TypeWrapper)this.typeComboBox.SelectedItem;

            if ((wrapper != null && !wrapper.IsTypeDefinition)
                || this.BrowseTypeDirectly)
            {
                Type result = null;
                bool? dialogResult = true;
                bool typeIsArray = true;
                bool fireEvent = false;
                //handle choosing an array of T
                if (wrapper != null && typeof(ArrayOf<>) == wrapper.Type)
                {
                    fireEvent = true;
                    this.RaiseEvent(new RoutedEventArgs(TypePresenter.TypeBrowserOpenedEvent, this));
                    result = wrapper.Type;
                }
                else if (wrapper != null && wrapper.DisplayName == NullString)
                {
                    this.Type = null;
                    return false;
                }
                else
                {
                    retval = true;
                    fireEvent = true;
                    this.RaiseEvent(new RoutedEventArgs(TypePresenter.TypeBrowserOpenedEvent, this));
                    TypeBrowser browser = new TypeBrowser(AssemblyContext, this.Context, this.Filter);
                    SetWindowOwner(browser);
                    if (this.CenterTypeBrowserDialog)
                    {
                        browser.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }
                    dialogResult = browser.ShowDialog();
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        result = browser.ConcreteType;
                    }

                    typeIsArray = false;
                }

                if (dialogResult.HasValue && dialogResult.Value)
                {
                    //user may have chosen generic type (IList)
                    if (result.IsGenericTypeDefinition)
                    {
                        retval = true;
                        ActivityTypeResolver wnd = new ActivityTypeResolver();
                        SetWindowOwner(wnd);
                        wnd.Context = this.Context;
                        wnd.EditedType = result;
                        if (this.CenterActivityTypeResolverDialog)
                        {
                            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        }

                        result = (true == wnd.ShowDialog() ? wnd.ConcreteType : null);
                    }
                    //if we have a type
                    if (null != result)
                    {
                        //if we have a ArrayOf<some type here>, create actual array type
                        if (typeIsArray)
                        {
                            result = result.GetGenericArguments()[0].MakeArrayType();
                        }
                        //add it to the cache
                        if (!MostRecentlyUsedTypes.Any<Type>(p => Type.Equals(p, result)))
                        {
                            MostRecentlyUsedTypes.Add(result);
                        }

                        //and return updated result
                        this.Type = result;
                    }
                    else
                    {
                        this.Type = this.lastSelection;
                    }

                    BindingExpression binding = this.typeComboBox.GetBindingExpression(ComboBox.SelectedItemProperty);
                    binding.UpdateTarget();
                }
                else
                {
                    SetComboBoxToLastSelection();
                }
                if (fireEvent)
                {
                    this.RaiseEvent(new RoutedEventArgs(TypePresenter.TypeBrowserClosedEvent, this));
                }
            }

            return retval;
        }

        void OnMostRecentlyUsedTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnItemsChanged();
        }

        void OnItemsChanged()
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Items"));
            }
        }

        void OnTypePresenterDropDownClosed(object sender, EventArgs e)
        {
            HandleBrowseType();
            DisableEdit();
            if (!this.BrowseTypeDirectly)
            {
                this.typeComboBox.Focus();
            }
            else
            {
                this.Focus();
            }
        }

        void OnTypePresenterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (HandleBrowseType())
                {
                    e.Handled = true;
                }
                DisableEdit();

                FocusOnVisibleControl();
            }
        }

        void OnTypePresenterLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!(e.NewFocus == this))
            {
                if (!(this.typeComboBox.IsDropDownOpen || this.typeComboBox.IsSelectionBoxHighlighted))
                {
                    DisableEdit();
                }
            }
        }

        void SetWindowOwner(Window wnd)
        {
            WindowHelperService.TrySetWindowOwner(this, this.Context, wnd);
        }

        // internal converter class - assign a meaningful AutomationProperties.Name to the type presenter
        // AutomationProperties.Name = Label + the string displayed on the TypePresenter
        sealed class AutomationNameConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                Fx.Assert(values.Length == 2, "There should be exactly 2 values");
                return (string)values[0] + ": " + (string)values[1];
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                Fx.Assert("Not supported!");
                return null;
            }
        }
    }

    [Fx.Tag.XamlVisible(false)]
    public sealed class TypeWrapper
    {
        string displayName;
        bool isTypeDefinition;
        Type type;

        internal TypeWrapper(Type type)
        {
            this.type = type;
            this.isTypeDefinition = true;
            this.Tag = DisplayName;
        }

        internal TypeWrapper(string text, string tag, Type type)
        {
            this.displayName = text;
            this.isTypeDefinition = false;
            this.Tag = tag;
            this.type = type;
        }

        public string DisplayName
        {
            get
            {
                if (this.isTypeDefinition)
                {
                    if (TypePresenter.DefaultTypes.Contains(this.type))
                    {
                        return this.type.Name;
                    }

                    return TypeNameHelper.GetDisplayName(this.Type, true);
                }
                return this.displayName;
            }
        }

        public bool IsTypeDefinition
        {
            get { return this.isTypeDefinition; }
        }

        public object Tag
        {
            get;
            private set;
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "By design.")]
        public Type Type
        {
            get { return this.type; }
        }

        public override string ToString()
        {
            return Tag as string;
        }

        public override bool Equals(object obj)
        {
            TypeWrapper that = obj as TypeWrapper;
            if (that == null)
            {
                return false;
            }
            if (that.IsTypeDefinition ^ this.IsTypeDefinition)
            {
                return false;
            }
            if (this.displayName != that.displayName)
            {
                return false;
            }
            return object.Equals(this.Type, that.Type);
        }

        public override int GetHashCode()
        {
            if (this.Type != null)
            {
                return this.Type.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }
    }

    sealed class ArrayOf<T>
    {
    }

    sealed class BrowseForType
    {
    }

    // internal converter class - keeps link between display friendly string representation of types
    // and actual underlying system type.
    sealed class TypeWrapperConverter : IValueConverter
    {
        TypePresenter typePresenter;

        //ctor - initialzied with list of loaded types into the presenter
        internal TypeWrapperConverter(TypePresenter typePresenter)
        {
            this.typePresenter = typePresenter;
        }

        //convert from System.Type to TypeWrapper (display friendly)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null != value)
            {
                //lookup in loaded types if type is already there

                //if no - add it to collection - may be reused later
                if (null == this.typePresenter.MostRecentlyUsedTypes.SingleOrDefault<Type>(p => Type.Equals(p, (Type)value)))
                {
                    this.typePresenter.MostRecentlyUsedTypes.Add((Type)value);
                }

                return new TypeWrapper((Type)value);
            }
            else
            {
                return this.typePresenter.NullTypeWrapper;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //convert back - just get the Type property of the wrapper object
            TypeWrapper typeWrapper = value as TypeWrapper;

            if (typeWrapper == this.typePresenter.NullTypeWrapper)
            {
                return null;
            }

            if (null != typeWrapper && null != typeWrapper.Type && typeof(ArrayOf<>) != typeWrapper.Type && typeof(BrowseForType) != typeWrapper.Type)
            {
                return typeWrapper.Type;
            }

            return Binding.DoNothing;
        }
    }
}
