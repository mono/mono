//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Services;
    using System.Activities.Statements;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Windows.Controls;
    using System.Activities.Presentation.View.OutlineView;

    /// <summary>
    /// Interaction logic for TryCatchDesigner.xaml
    /// </summary>
    partial class TryCatchDesigner
    {
        const string CatchesPropertyName = "Catches";
        const string ExceptionTypePropertyName = "ExceptionType";
        const string ExpandViewStateKey = "IsExpanded";

        public static readonly DependencyProperty ShowTryExpandedProperty =
            DependencyProperty.Register(
                "ShowTryExpanded",
                typeof(bool),
                typeof(TryCatchDesigner),
                new UIPropertyMetadata(true)
            );

        public static readonly DependencyProperty ShowFinallyExpandedProperty =
            DependencyProperty.Register(
                "ShowFinallyExpanded",
                typeof(bool),
                typeof(TryCatchDesigner),
                new UIPropertyMetadata(false)
            );

        public static readonly DependencyProperty ShowTypePresenterExpandedProperty =
            DependencyProperty.Register(
                "ShowTypePresenterExpanded",
                typeof(bool),
                typeof(TryCatchDesigner),
                new UIPropertyMetadata(false)
            );

        public static readonly DependencyProperty SelectedCatchProperty =
            DependencyProperty.Register(
            "SelectedCatch",
            typeof(ModelItem),
            typeof(TryCatchDesigner),
            new UIPropertyMetadata(null));

        static ObservableCollection<Type> mostRecentlyUsedTypes;
        static ObservableCollection<Type> MostRecentlyUsedTypes
        {
            get
            {
                if (mostRecentlyUsedTypes == null)
                {
                    mostRecentlyUsedTypes = new ObservableCollection<Type>
                    {
                        typeof(ArgumentException),
                        typeof(NullReferenceException),
                        typeof(IOException),
                        typeof(InvalidOperationException),
                        typeof(Exception),
                    };
                }
                return mostRecentlyUsedTypes;
            }
        }

        public bool ShowTryExpanded
        {
            get
            {
                return (bool)this.GetValue(ShowTryExpandedProperty);
            }
            set
            {
                this.SetValue(ShowTryExpandedProperty, value);
            }
        }

        public bool ShowFinallyExpanded
        {
            get
            {
                return (bool)this.GetValue(ShowFinallyExpandedProperty);
            }
            set
            {
                this.SetValue(ShowFinallyExpandedProperty, value);
            }
        }

        public bool ShowTypePresenterExpanded
        {
            get
            {
                return (bool)this.GetValue(ShowTypePresenterExpandedProperty);
            }
            set
            {
                this.SetValue(ShowTypePresenterExpandedProperty, value);
            }
        }

        ModelItem SelectedCatch
        {
            get
            {
                return (ModelItem)this.GetValue(SelectedCatchProperty);
            }
            set
            {
                this.SetValue(SelectedCatchProperty, value);
            }
        }

        TypePresenter typePresenter;
        Label addCatchHintLabel;

        internal static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(TryCatch);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(TryCatchDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Try"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Finally"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Catches"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);

            // Make Catches collection's node visible in the document treeview but hide Catches node itself.
            builder.AddCustomAttributes(type, type.GetProperty("Catches"), new ShowPropertyInOutlineViewAttribute() { CurrentPropertyVisible = false });
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TryCatchDesigner()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Context.Items.Subscribe<Selection>(OnSelectionChanged);
            // at this time, this.ModelItem is already set
            this.ModelItem.PropertyChanged += OnModelItemPropertyChanged;
            this.ModelItem.Properties[CatchesPropertyName].Collection.CollectionChanged += OnModelItemCollectionChanged;

            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();

            foreach (ModelItem modelItem in this.ModelItem.Properties["Catches"].Collection)
            {
                bool? isExpanded = (bool?)viewStateService.RetrieveViewState(modelItem, ExpandViewStateKey);
                if (isExpanded != null && isExpanded.Value)
                {
                    this.SelectedCatch = modelItem;
                    CollapseTryView();
                    CollapseFinallyView();
                    break;
                }
            }
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.ModelItem.PropertyChanged -= OnModelItemPropertyChanged;
            this.ModelItem.Properties[CatchesPropertyName].Collection.CollectionChanged -= OnModelItemCollectionChanged;
            this.Context.Items.Unsubscribe<Selection>(OnSelectionChanged);
        }

        void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Try":
                    ExpandTryView();
                    break;

                case "Finally":
                    ExpandFinallyView();
                    break;

                default:
                    break;
            }
        }

        void OnModelItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // to update the filter
            this.typePresenter.Filter = this.ExceptionTypeFilter;
        }

        void OnSelectionChanged(Selection selection)
        {
            if (IsDescendantOfTry(selection.PrimarySelection))
            {
                this.ExpandTryView();
            }
            else if (IsDescendantOfFinally(selection.PrimarySelection))
            {
                this.ExpandFinallyView();
            }
            else
            {
                foreach (ModelItem catchObject in this.ModelItem.Properties["Catches"].Collection)
                {
                    if (IsDescendantOfCatch(catchObject, selection.PrimarySelection))
                    {
                        UpdateSelection(catchObject);
                        break;
                    }
                }
            }
        }

        bool IsDescendantOfTry(ModelItem descendant)
        {
            return IsDescendantOf(descendant, "Try");
        }

        bool IsDescendantOfFinally(ModelItem descendant)
        {
            return IsDescendantOf(descendant, "Finally");
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

        bool IsDescendantOf(ModelItem descendant, string property)
        {
            if (descendant == null)
            {
                return false;
            }
            else
            {
                ModelItem propertyValue = this.ModelItem.Properties[property].Value;
                return IsAncestorOf(propertyValue, descendant);
            }
        }

        internal static bool IsDescendantOfCatch(ModelItem catchObject, ModelItem descendant)
        {
            Fx.Assert(catchObject != null, "Catch object mustn't be null.");
            if (catchObject == descendant)
            {
                return true;
            }
            else
            {
                ModelItem activityAction = catchObject.Properties["Action"].Value;
                if (activityAction != null)
                {
                    ModelItem activityActionHandler = activityAction.Properties["Handler"].Value;
                    if (activityActionHandler != null)
                    {
                        return IsAncestorOf(activityActionHandler, descendant);
                    }
                }
                return false;
            }
        }

        void UpdateSelection(ModelItem newSelectedCatch)
        {
            ModelItem oldSelectedCatch = this.SelectedCatch;
            this.SelectedCatch = newSelectedCatch;

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                if (oldSelectedCatch != null)
                {
                    CatchDesigner oldSelectedCatchDesigner = (CatchDesigner)oldSelectedCatch.View;
                    if (oldSelectedCatchDesigner != null)
                    {
                        oldSelectedCatchDesigner.ExpandState = false;
                        oldSelectedCatchDesigner.PinState = false;
                    }
                }
                if (newSelectedCatch != null)
                {
                    CollapseTryView();
                    CollapseFinallyView();
                    CatchDesigner newSelectedCatchDesigner = (CatchDesigner)newSelectedCatch.View;
                    if (newSelectedCatchDesigner != null)
                    {
                        newSelectedCatchDesigner.ExpandState = true;
                        newSelectedCatchDesigner.PinState = true;
                    }
                }
            }));
        }

        void CreateCatch(Type exceptionType)
        {
            if (exceptionType != null)
            {
                Type catchType = typeof(Catch<>).MakeGenericType(exceptionType);
                object catchObject = Activator.CreateInstance(catchType);

                Type activityActionType = typeof(ActivityAction<>).MakeGenericType(exceptionType);
                object activityAction = Activator.CreateInstance(activityActionType);

                Type argumentType = typeof(DelegateInArgument<>).MakeGenericType(exceptionType);
                object exceptionArgument = Activator.CreateInstance(argumentType);
                DelegateInArgument delegateArgument = exceptionArgument as DelegateInArgument;
                Fx.Assert(null != delegateArgument, "delegate argument must be of DelegateInArgument type!");
                delegateArgument.Name = "exception";

                catchType.GetProperty(PropertyNames.Action).SetValue(catchObject, activityAction, null);
                activityActionType.GetProperty(PropertyNames.ActionArgument).SetValue(activityAction, exceptionArgument, null);

                this.ModelItem.Properties["Catches"].Collection.Add(catchObject);
            }
        }

        void OnFinallyViewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                SwitchTryCatchDesignerHelper.MakeRootDesigner(this);
                e.Handled = true;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                ExpandFinallyView();
                Keyboard.Focus((IInputElement)sender);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (this.IsExpanded(this.ShowFinallyExpanded))
                {
                    Keyboard.Focus((IInputElement)sender);
                }
                e.Handled = true;
            }
        }

        void OnFinallyViewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // avoid context menu upon right-click when it's collapsed
            if (!IsExpanded(this.ShowFinallyExpanded) && e.RightButton == MouseButtonState.Released)
            {
                e.Handled = true;
            }
        }

        bool IsExpanded(bool isExpanded)
        {
            DesignerView designerView = this.Context.Services.GetService<DesignerView>();
            return isExpanded || designerView.ShouldExpandAll;
        }

        void OnTryViewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                SwitchTryCatchDesignerHelper.MakeRootDesigner(this);
                e.Handled = true;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                ExpandTryView();
                Keyboard.Focus((IInputElement)sender);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (this.IsExpanded(this.ShowTryExpanded))
                {
                    Keyboard.Focus((IInputElement)sender);
                }
                e.Handled = true;
            }
        }

        void OnTryViewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // avoid context menu upon right-click when it's collapsed
            if (!IsExpanded(this.ShowTryExpanded) && e.RightButton == MouseButtonState.Released)
            {
                e.Handled = true;
            }
        }

        void ExpandFinallyView()
        {
            UpdateSelection(null);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                this.ShowTryExpanded = false;
                this.ShowFinallyExpanded = true;
            }));
        }

        void ExpandTryView()
        {
            UpdateSelection(null);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                this.ShowFinallyExpanded = false;
                this.ShowTryExpanded = true;
            }));
        }

        void CollapseFinallyView()
        {
            this.ShowFinallyExpanded = false;
        }

        void CollapseTryView()
        {
            this.ShowTryExpanded = false;
        }

        void OnTryViewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender == e.OriginalSource && (e.Key == Key.Space || e.Key == Key.Enter))
            {
                ExpandTryView();
                e.Handled = true;
            }
        }

        void OnTryAddActivityKeyDown(object sender, KeyEventArgs e)
        {
            if (!LocalAppContextSwitches.UseLegacyAccessibilityFeatures)
            {
                if (sender == e.OriginalSource && (e.Key == Key.Space || e.Key == Key.Enter))
                {
                    ExpandTryView();
                    e.Handled = true;
                }
            }
        }

        void OnFinallyViewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender == e.OriginalSource && (e.Key == Key.Space || e.Key == Key.Enter))
            {
                ExpandFinallyView();
                e.Handled = true;
            }
        }

        void OnFinallyAddActivityKeyDown(object sender, KeyEventArgs e)
        {
            if (!LocalAppContextSwitches.UseLegacyAccessibilityFeatures)
            {
                if (sender == e.OriginalSource && (e.Key == Key.Space || e.Key == Key.Enter))
                {
                    ExpandFinallyView();
                    e.Handled = true;
                }
            }
        }

        #region AddCatch Label & TypePresenter

        void OnAddCatchMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.SwitchToChooseException();
                e.Handled = true;
            }
        }

        void OnAddCatchGotFocus(object sender, RoutedEventArgs e)
        {
            this.SwitchToChooseException();
            e.Handled = true;
        }

        void SwitchToChooseException()
        {
            this.ShowTypePresenterExpanded = true;
            this.typePresenter.FocusOnVisibleControl();
        }

        void SwitchToHintText()
        {
            this.typePresenter.Type = null;
            this.ShowTypePresenterExpanded = false;
            Keyboard.Focus((IInputElement)this);
        }

        void OnAddCatchHintLabelLoaded(object sender, RoutedEventArgs e)
        {
            this.addCatchHintLabel = (Label)sender;
        }

        void OnAddCatchHintLabelUnloaded(object sender, RoutedEventArgs e)
        {
            this.addCatchHintLabel = null;
        }

        void OnTypePresenterLoaded(object sender, RoutedEventArgs e)
        {
            TypePresenter tp = (TypePresenter)sender;
            Fx.Assert(tp != null, "sender must be a TypePresenter.");

            this.typePresenter = tp;
            this.typePresenter.Filter = this.ExceptionTypeFilter;
            this.typePresenter.MostRecentlyUsedTypes = MostRecentlyUsedTypes;
            //UnRegistering because of 137896: Inside tab control multiple Loaded events happen without an Unloaded event.
            this.typePresenter.TypeBrowserClosed -= OnTypePresenterTypeBrowserClosed;
            this.typePresenter.TypeBrowserClosed += OnTypePresenterTypeBrowserClosed;
        }

        void OnTypePresenterUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.typePresenter != null)
            {
                this.typePresenter.TypeBrowserClosed -= OnTypePresenterTypeBrowserClosed;
                this.typePresenter = null;
            }
        }

        void OnTypePresenterTypeBrowserClosed(object sender, RoutedEventArgs e)
        {
            this.typePresenter.FocusOnVisibleControl();
        }

        void OnTypePresenterKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.SwitchToHintText();
                    e.Handled = true;
                    break;

                case Key.Enter:
                    this.AddCatch();
                    e.Handled = true;
                    break;
            }
        }

        void OnTypePresenterLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.ShowTypePresenterExpanded)
            {
                this.AddCatch();
                e.Handled = true;
            }
        }

        void AddCatch()
        {
            if (this.typePresenter != null)
            {
                Type type = this.typePresenter.Type;
                if (type != null && this.ExceptionTypeFilter(type))
                {
                    CreateCatch(type);
                }
                this.SwitchToHintText();
            }
        }

        #endregion

        bool ExceptionTypeFilter(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (type != typeof(Exception) && !type.IsSubclassOf(typeof(Exception)))
            {
                return false;
            }

            ModelProperty catchesProperty = this.ModelItem.Properties[CatchesPropertyName];
            Fx.Assert(catchesProperty != null, "TryCatch.Catches could not be null");
            ModelItemCollection catches = catchesProperty.Collection;
            Fx.Assert(catches != null, "Catches.Collection could not be null");
            foreach (ModelItem catchItem in catches)
            {
                ModelProperty exceptionTypeProperty = catchItem.Properties[ExceptionTypePropertyName];
                Fx.Assert(exceptionTypeProperty != null, "Catch.ExceptionType could not be null");
                Type exceptionType = exceptionTypeProperty.ComputedValue as Type;
                Fx.Assert(exceptionType != null, "Catch.ExceptionType.Value could not be null");

                if (exceptionType == type)
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal class TextBlockWrapper : TextBlock
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if (!LocalAppContextSwitches.UseLegacyAccessibilityFeatures)
            {
                return new TextBlockWrapperAutomationPeer(this);
            }
            return base.OnCreateAutomationPeer();
        }
    }

    internal class TextBlockWrapperAutomationPeer : TextBlockAutomationPeer
    {
        public TextBlockWrapperAutomationPeer(TextBlockWrapper owner)
            : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }
    }
}
