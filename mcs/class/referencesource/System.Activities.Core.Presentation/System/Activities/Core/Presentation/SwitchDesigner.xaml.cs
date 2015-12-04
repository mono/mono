//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Runtime;
    using System.Collections.Generic;
    using System.Activities.Presentation.View;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Activities.Presentation.View.OutlineView;

    partial class SwitchDesigner
    {
        const string ExpandViewStateKey = "IsExpanded";

        public static readonly DependencyProperty CaseTypeProperty =
            DependencyProperty.Register(
            "CaseType",
            typeof(Type),
            typeof(SwitchDesigner),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty SelectedCaseProperty =
            DependencyProperty.Register(
            "SelectedCase",
            typeof(ModelItem),
            typeof(SwitchDesigner),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty ShowDefaultCaseExpandedProperty =
            DependencyProperty.Register(
            "ShowDefaultCaseExpanded",
            typeof(bool),
            typeof(SwitchDesigner),
            new UIPropertyMetadata(false));

        public static readonly DependencyProperty NewKeyProperty =
            DependencyProperty.Register(
            "NewKey",
            typeof(object),
            typeof(SwitchDesigner),
            new UIPropertyMetadata(null));

        static TypeResolvingOptions argumentTypeResolvingOptions;

        TextBlock addNewCaseLabel;
        CaseKeyBox caseKeyBox;

        public bool ShowDefaultCaseExpanded
        {
            get
            {
                return (bool)this.GetValue(ShowDefaultCaseExpandedProperty);
            }
            set
            {
                this.SetValue(ShowDefaultCaseExpandedProperty, value);
            }
        }

        ModelItem SelectedCase
        {
            get
            {
                return (ModelItem)this.GetValue(SelectedCaseProperty);
            }
            set
            {
                this.SetValue(SelectedCaseProperty, value);
            }
        }

        Type CaseType
        {
            get { return (Type)GetValue(CaseTypeProperty); }
            set { SetValue(CaseTypeProperty, value); }
        }

        object NewKey
        {
            get { return GetValue(NewKeyProperty); }
            set { SetValue(NewKeyProperty, value); }
        }

        public CaseKeyValidationCallbackDelegate CheckDuplicateCaseKey
        {
            get
            {
                return (object obj, out string reason) =>
                {
                    reason = string.Empty;
                    if (ContainsCaseKey(obj))
                    {
                        string key = obj != null ? obj.ToString() : "(null)";
                        reason = string.Format(CultureInfo.CurrentCulture, SR.DuplicateCaseKey, key);
                        return false;
                    }
                    return true;
                };
            }
        }

        static List<Type> defaultTypes;
        static List<Type> DefaultTypes
        {
            get
            {
                if (defaultTypes == null)
                {
                    defaultTypes = new List<Type>
                    {
                        typeof(bool),
                        typeof(int),
                        typeof(string),
                    };
                }
                return defaultTypes;
            }
        }

        static TypeResolvingOptions ArgumentTypeResolvingOptions
        {
            get
            {
                if (argumentTypeResolvingOptions == null)
                {
                    argumentTypeResolvingOptions = new TypeResolvingOptions(DefaultTypes)
                    {
                        Filter = null,
                    };
                }
                return argumentTypeResolvingOptions;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SwitchDesigner()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.Unloaded += new RoutedEventHandler(OnUnloaded);
            this.Resources.Add("ModelItemKeyValuePairType", typeof(ModelItemKeyValuePair<,>));
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            Type modelItemType = this.ModelItem.ItemType;
            Type[] types = modelItemType.GetGenericArguments();
            Fx.Assert(types.Length == 1, "Switch should have exactly one generic argument");
            this.CaseType = types[0];
        }

        void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Default" && !this.ShowDefaultCaseExpanded)
            {
                ExpandDefaultView();
                this.UpdateSelection(null);
            }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Context.Items.Subscribe<Selection>(OnSelectionChanged);
            this.ModelItem.PropertyChanged += OnModelItemPropertyChanged;

            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();

            foreach (ModelItem modelItem in this.ModelItem.Properties["Cases"].Dictionary.Properties["ItemsCollection"].Collection)
            {
                bool? isExpanded = (bool?)viewStateService.RetrieveViewState(modelItem, ExpandViewStateKey);
                if (isExpanded != null && isExpanded.Value)
                {
                    this.SelectedCase = modelItem;
                    CollapseDefaultView();
                    break;
                }
            }
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.ModelItem.PropertyChanged -= OnModelItemPropertyChanged;
            this.Context.Items.Unsubscribe<Selection>(OnSelectionChanged);
        }

        void OnSelectionChanged(Selection selection)
        {
            if (this.IsDescendantOfDefault(selection.PrimarySelection))
            {
                this.ExpandDefaultView();
            }
            else
            {
                foreach (ModelItem caseObject in this.ModelItem.Properties["Cases"].Dictionary.Properties["ItemsCollection"].Collection)
                {
                    if (IsDescendantOfCase(caseObject, selection.PrimarySelection))
                    {
                        UpdateSelection(caseObject);
                        break;
                    }
                }
            }
        }

        static bool IsAncestorOf(ModelItem ancester, ModelItem descendant)
        {
            if (ancester == null)
            {
                return false;
            }

            ModelItem itr = descendant;
            while (itr != null)
            {
                if (itr == ancester)
                {
                    return true;
                }
                itr = itr.Parent;
            }
            return false;
        }

        bool IsDescendantOfDefault(ModelItem descendant)
        {
            if (descendant == null)
            {
                return false;
            }
            else
            {
                ModelItem defaultValue = this.ModelItem.Properties["Default"].Value;
                return IsAncestorOf(defaultValue, descendant);
            }
        }

        internal static bool IsDescendantOfCase(ModelItem caseObject, ModelItem descendant)
        {
            Fx.Assert(caseObject != null, "Case object mustn't be null.");
            if (caseObject == descendant)
            {
                return true;
            }
            else
            {
                ModelItem caseValue = caseObject.Properties["Value"].Value;
                return IsAncestorOf(caseValue, descendant);
            }
        }

        void UpdateSelection(ModelItem newSelectedCase)
        {
            ModelItem oldSelectedCase = this.SelectedCase;
            this.SelectedCase = newSelectedCase;

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                if (oldSelectedCase != null)
                {
                    CaseDesigner oldSelectedCaseDesigner = (CaseDesigner)oldSelectedCase.View;
                    if (oldSelectedCaseDesigner != null)
                    {
                        oldSelectedCaseDesigner.ExpandState = false;
                        oldSelectedCaseDesigner.PinState = false;
                    }
                }
                if (newSelectedCase != null)
                {
                    CollapseDefaultView();

                    CaseDesigner newSelectedCaseDesigner = (CaseDesigner)newSelectedCase.View;
                    if (newSelectedCaseDesigner != null)
                    {
                        newSelectedCaseDesigner.ExpandState = true;
                        newSelectedCaseDesigner.PinState = true;
                    }
                }
            }));
        }

        internal static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(Switch<>);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(SwitchDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Default"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, new TypeResolvingOptionsAttribute(ArgumentTypeResolvingOptions));

            // Hide Cases node in the treeview and display its child nodes directly.
            builder.AddCustomAttributes(type, type.GetProperty("Cases"), new ShowPropertyInOutlineViewAttribute() { CurrentPropertyVisible = false, ChildNodePrefix = "Case : " });
        }

        void OnDefaultCaseViewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                SwitchTryCatchDesignerHelper.MakeRootDesigner(this);
                e.Handled = true;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                ExpandDefaultView();
                Keyboard.Focus((IInputElement)sender);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (this.IsDefaultCaseViewExpanded())
                {
                    Keyboard.Focus((IInputElement)sender);
                }
                e.Handled = true;
            }
        }

        void OnDefaultCaseViewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // avoid context menu upon right-click when it's collapsed
            if (!IsDefaultCaseViewExpanded() && e.RightButton == MouseButtonState.Released)
            {
                e.Handled = true;
            }
        }

        bool IsDefaultCaseViewExpanded()
        {
            DesignerView designerView = this.Context.Services.GetService<DesignerView>();
            return this.ShowDefaultCaseExpanded || designerView.ShouldExpandAll;
        }

        void OnDefaultCaseViewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender == e.OriginalSource && (e.Key == Key.Space || e.Key == Key.Enter))
            {
                ExpandDefaultView();
                e.Handled = true;
            }
        }

        void ExpandDefaultView()
        {
            UpdateSelection(null);
            this.ShowDefaultCaseExpanded = true;
        }

        void CollapseDefaultView()
        {
            this.ShowDefaultCaseExpanded = false;
        }

        void OnAddNewCaseLabelLoaded(object sender, RoutedEventArgs e)
        {
            this.addNewCaseLabel = (TextBlock)sender;
            this.addNewCaseLabel.Visibility = Visibility.Collapsed;
        }

        void OnAddNewCaseLabelUnloaded(object sender, RoutedEventArgs e)
        {
            this.addNewCaseLabel = null;
        }

        void OnNewKeyTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            this.addNewCaseLabel.Visibility = Visibility.Visible;
        }

        void OnNewKeyCommitted(object sender, RoutedEventArgs e)
        {
            this.addNewCaseLabel.Visibility = Visibility.Collapsed;
            try
            {
                AddNewCase(this.NewKey);
            }
            catch (ArgumentException ex)
            {
                ErrorReporting.ShowErrorMessage(ex.Message);
            }
        }

        void OnNewKeyEditCancelled(object sender, RoutedEventArgs e)
        {
            this.addNewCaseLabel.Visibility = Visibility.Collapsed;
        }

        void OnCaseKeyBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.caseKeyBox = (CaseKeyBox)sender;
        }

        void AddNewCase(object newKey)
        {
            Type caseType = typeof(ModelItemKeyValuePair<,>).MakeGenericType(new Type[] { this.CaseType, typeof(Activity) });
            object mutableKVPair = Activator.CreateInstance(caseType, new object[] { newKey, null });
            ModelProperty casesProp = this.ModelItem.Properties["Cases"];
            Fx.Assert(casesProp != null, "Property Cases is not available");
            ModelItem cases = casesProp.Value;
            Fx.Assert(cases != null, "Cannot get ModelItem from property Cases");
            ModelProperty itemsCollectionProp = cases.Properties["ItemsCollection"];
            Fx.Assert(itemsCollectionProp != null, "Cannot get property ItemsCollection from Cases");
            ModelItemCollection itemsCollection = itemsCollectionProp.Collection;
            Fx.Assert(itemsCollection != null, "Cannot get ModelItemCollection from property ItemsCollection");
            itemsCollection.Add(mutableKVPair);

            this.caseKeyBox.ResetText();
        }

        bool ContainsCaseKey(object key)
        {
            Type caseType = typeof(ModelItemKeyValuePair<,>).MakeGenericType(new Type[] { this.CaseType, typeof(Activity) });
            ModelProperty casesProp = this.ModelItem.Properties["Cases"];
            ModelItem cases = casesProp.Value;
            ModelProperty itemsCollectionProp = cases.Properties["ItemsCollection"];
            ModelItemCollection itemsCollection = itemsCollectionProp.Collection;

            foreach (ModelItem item in itemsCollection)
            {
                object itemKey = caseType.GetProperty("Key").GetGetMethod().Invoke(item.GetCurrentValue(), null);
                if ((itemKey != null && itemKey.Equals(key)) || (itemKey == key))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
