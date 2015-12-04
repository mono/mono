//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Hosting;
    using System.Runtime;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Reflection;

    internal sealed partial class DynamicArgumentDesigner : UserControl
    {
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context",
            typeof(EditingContext),
            typeof(DynamicArgumentDesigner));

        public static readonly DependencyProperty OwnerActivityProperty =
           DependencyProperty.Register("OwnerActivity",
           typeof(ModelItem),
           typeof(DynamicArgumentDesigner));

        public static readonly DependencyProperty IsDirectionReadOnlyProperty =
            DependencyProperty.Register("IsDirectionReadOnly",
            typeof(bool),
            typeof(DynamicArgumentDesigner),
            new UIPropertyMetadata(true, OnIsDirectionReadOnlyChanged));

        public static readonly DependencyProperty DynamicArgumentsProperty =
            DependencyProperty.Register("DynamicArguments",
            typeof(ObservableCollection<DynamicArgumentWrapperObject>),
            typeof(DynamicArgumentDesigner),
            new PropertyMetadata(new ObservableCollection<DynamicArgumentWrapperObject>()));

        public static readonly DependencyProperty IsDictionaryProperty =
            DependencyProperty.Register("IsDictionary",
            typeof(bool?),
            typeof(DynamicArgumentDesigner),
            new PropertyMetadata(false));

        public static readonly DependencyProperty UnderlyingArgumentTypeProperty =
            DependencyProperty.Register("UnderlyingArgumentType",
            typeof(Type),
            typeof(DynamicArgumentDesigner),
            new PropertyMetadata(typeof(Argument)));

        public static readonly RoutedCommand CreateDynamicArgumentCommand   = new RoutedCommand("CreateDynamicArgumentCommand", typeof(DynamicArgumentDesigner));
        public static readonly RoutedCommand MoveUpArgumentCommand          = new RoutedCommand("MoveUpArgumentCommand", typeof(DynamicArgumentDesigner));
        public static readonly RoutedCommand MoveDownArgumentCommand        = new RoutedCommand("MoveDownArgumentCommand", typeof(DynamicArgumentDesigner));
        public static readonly RoutedCommand DeleteArgumentCommand          = new RoutedCommand("DeleteArgumentCommand", typeof(DynamicArgumentDesigner));
        public const string DefaultArgumentPrefix = "Argument";

        SubscribeContextCallback<ReadOnlyState> onReadOnlyStateChangedCallback;

        static readonly Type InArgumentType = typeof(InArgument);
        static readonly Type OutArgumentType = typeof(OutArgument);
        static readonly Type InOutArgumentType = typeof(InOutArgument);
        static readonly Type ArgumentType = typeof(Argument);

        const int NameColumn = 0;
        const int DirectionColumn = 1;
        const int ArgumentTypeColumn = 2;
        const int ExpressionColumn = 3;

        bool isReadOnly;
        bool hideDirection;
        string argumentPrefix = DefaultArgumentPrefix;
        string hintText;
        DataGridHelper dgHelper;
        ContextItemManager contextItemManager;        

        public DynamicArgumentDesigner()
        {
            InitializeComponent();

            this.dgHelper = new DataGridHelper(this.WPF_DataGrid, this);
            this.dgHelper.AddNewRowCommand = DynamicArgumentDesigner.CreateDynamicArgumentCommand;
            this.HintText = null;

            this.WPF_DataGrid.LoadingRow += this.DataGrid_Standard_LoadingRow;

            this.Loaded += (sender, e) =>
            {
                OnReadOnlyStateChanged(new ReadOnlyState());
                this.ContextItemManager.Subscribe<ReadOnlyState>(this.OnReadOnlyStateChangedCallback);
                this.OnDynamicArgumentsLoaded();
                this.OnUnderlyingArgumentTypeChanged();
            };
            this.Unloaded += (sender, e) =>
            {
                this.ContextItemManager.Unsubscribe<ReadOnlyState>(this.OnReadOnlyStateChangedCallback);
            };

            DynamicArgumentWrapperObject.Editor = this;
        }

        ContextItemManager ContextItemManager
        {
            get
            {
                if (this.contextItemManager == null)
                {
                    this.contextItemManager = this.OwnerActivity.GetEditingContext().Items;
                }
                return this.contextItemManager;
            }
        }

        void OnReadOnlyStateChanged(ReadOnlyState state)
        {
            UpdateChildrenElementStatus();
        }

        void UpdateChildrenElementStatus()
        {
            this.isReadOnly = this.ContextItemManager.GetValue<ReadOnlyState>().IsReadOnly || this.DynamicArguments == null;

            if (this.isReadOnly)
            {
                this.WPF_DataGrid.IsReadOnly = true;
                this.ButtonMovDown.IsEnabled = false;
                this.ButtonMovUp.IsEnabled = false;
                this.ButtonDelete.IsEnabled = false;
            }
        }

        void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.isReadOnly 
                || this.WPF_DataGrid.SelectedItems == null
                || this.WPF_DataGrid.SelectedItems.Count == 0)
            {
                this.ButtonMovUp.IsEnabled   = false;
                this.ButtonMovDown.IsEnabled = false;
                this.ButtonDelete.IsEnabled  = false;
                return;
            }

            // delete button
            this.ButtonDelete.IsEnabled = true;
            
            // up/down button.
            if (this.WPF_DataGrid.SelectedItems.Count == 1)
            {
                bool upHadFocus = ButtonMovUp.IsFocused;
                bool downHadFocus = ButtonMovDown.IsFocused;
                this.ButtonMovUp.IsEnabled = this.WPF_DataGrid.SelectedIndex > 0;
                this.ButtonMovDown.IsEnabled = this.WPF_DataGrid.SelectedIndex < this.DynamicArguments.Count - 1;
                if (!this.ButtonMovDown.IsEnabled && downHadFocus)
                {
                    this.ButtonMovUp.Focus();
                }
                if (!this.ButtonMovUp.IsEnabled && upHadFocus)
                {
                    this.ButtonMovDown.Focus();
                }
            }
            else
            {
                this.ButtonMovUp.IsEnabled = this.ButtonMovDown.IsEnabled = false;
            }
        }

        // The DataGrid does not bubble up KeyDown event and we expect the upper window to be closed when ESC key is down.
        // Thus we added an event handler in DataGrid to handle ESC key and closes the uppper window.
        void OnDataGridRowKeyDown(object sender, KeyEventArgs args)
        {
            DataGridRow row = (DataGridRow)sender;
            if (args.Key == Key.Escape && !row.IsEditing && this.ParentDialog != null)
            {
                this.ParentDialog.CloseDialog(false);
            }
        }        

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Unloaded += this.OnDynamicArgumentDesignerUnloaded;
        }

        SubscribeContextCallback<ReadOnlyState> OnReadOnlyStateChangedCallback
        {
            get
            {
                if (onReadOnlyStateChangedCallback == null)
                {
                    onReadOnlyStateChangedCallback = new SubscribeContextCallback<ReadOnlyState>(OnReadOnlyStateChanged);
                }
                return onReadOnlyStateChangedCallback;
            }
        }

        void OnDynamicArgumentDesignerUnloaded(object sender, RoutedEventArgs e)
        {
            this.WPF_DataGrid.LoadingRow -= this.DataGrid_Standard_LoadingRow;
            this.Unloaded -= this.OnDynamicArgumentDesignerUnloaded;
        }

        public ObservableCollection<DynamicArgumentWrapperObject> DynamicArguments
        {
            get
            {
                return (ObservableCollection<DynamicArgumentWrapperObject>)GetValue(DynamicArgumentsProperty);
            }
            set
            {
                SetValue(DynamicArgumentsProperty, value);
                if (value != null)
                {
                    this.WPF_DataGrid.ItemsSource = value;
                }
            }
        }

        [Fx.Tag.KnownXamlExternal]
        public ModelItem OwnerActivity
        {
            get { return (ModelItem)GetValue(OwnerActivityProperty); }
            set { SetValue(OwnerActivityProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public bool IsDirectionReadOnly
        {
            get { return (bool)GetValue(IsDirectionReadOnlyProperty); }
            set { SetValue(IsDirectionReadOnlyProperty, value); }
        }

        public bool HideDirection
        {
            get
            {
                return this.hideDirection;
            }
            set
            {
                this.hideDirection = value;
                if (this.hideDirection)
                {
                    this.WPF_DataGrid.Columns[DynamicArgumentDesigner.DirectionColumn].Visibility = Visibility.Hidden;
                }
                else
                {
                    this.WPF_DataGrid.Columns[DynamicArgumentDesigner.DirectionColumn].Visibility = Visibility.Visible;
                }
            }
        }

        public string ArgumentPrefix
        {
            get
            {
                return this.argumentPrefix;
            }
            set
            {
                this.argumentPrefix = value;
            }
        }

        public string HintText
        {
            get
            {
                if (this.hintText != null)
                {
                    return this.hintText;
                }
                else
                {
                    return (string)this.FindResource("addDynamicArgumentNewRowLabel");
                }
            }
            set
            {
                this.hintText = value;
                if (hintText == null)
                {
                    dgHelper.AddNewRowContent = (string)this.FindResource("addDynamicArgumentNewRowLabel");
                }
                else
                {
                    dgHelper.AddNewRowContent = hintText;
                }
            }
        }

        public WorkflowElementDialog ParentDialog
        {
            get;
            set;
        }

        static void OnIsDirectionReadOnlyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((DynamicArgumentDesigner)dependencyObject).OnIsDirectionReadOnlyChanged();
        }

        void OnIsDirectionReadOnlyChanged()
        {
            if (!this.IsDirectionReadOnly)
            {
                DataGridTemplateColumn directionCol = this.WPF_DataGrid.Columns[1] as DataGridTemplateColumn;
                directionCol.CellEditingTemplate = (DataTemplate)this.WPF_DataGrid.FindResource("argumentDirectionEditingTemplate");
            }
        }

        internal Type UnderlyingArgumentType
        {
            get
            {
                return (Type)GetValue(UnderlyingArgumentTypeProperty);
            }
            set
            {
                if (!typeof(Argument).IsAssignableFrom(value))
                {
                    ErrorReporting.ShowErrorMessage(SR.NonSupportedDynamicArgumentType);
                }
                else
                {
                    SetValue(UnderlyingArgumentTypeProperty, value);
                    OnUnderlyingArgumentTypeChanged();
                }
            }
        }

        void OnUnderlyingArgumentTypeChanged()
        {
            Type currentArgumentType = this.UnderlyingArgumentType;

            if (currentArgumentType != null && (OutArgumentType.IsAssignableFrom(currentArgumentType) || InOutArgumentType.IsAssignableFrom(currentArgumentType)))
            {
                this.WPF_DataGrid.Columns[DynamicArgumentDesigner.ExpressionColumn].Header = (string)this.FindResource("assignToHeader");
            }
            else
            {
                this.WPF_DataGrid.Columns[DynamicArgumentDesigner.ExpressionColumn].Header = (string)this.FindResource("valueHeader");
            }
        }

        internal bool? IsDictionary
        {
            get
            {
                return (bool?)GetValue(IsDictionaryProperty);
            }
            set
            {
                SetValue(IsDictionaryProperty, value);
            }
        }

        internal static ObservableCollection<DynamicArgumentWrapperObject> ModelItemToWrapperCollection(ModelItem model, out bool isDictionary, out Type underlyingArgumentType)
        {
            string errorMessage = string.Empty;
            underlyingArgumentType = null;
            isDictionary = false;
            if (model is ModelItemCollection)
            {
                underlyingArgumentType = model.GetCurrentValue().GetType().GetGenericArguments()[0];
                if (!typeof(Argument).IsAssignableFrom(underlyingArgumentType))
                {
                    errorMessage = SR.NonSupportedDynamicArgumentType;
                }
            }
            else if (model is ModelItemDictionary)
            {
                Type underlyingKeyType = model.GetCurrentValue().GetType().GetGenericArguments()[0];
                underlyingArgumentType = model.GetCurrentValue().GetType().GetGenericArguments()[1];
                if (!typeof(Argument).IsAssignableFrom(underlyingArgumentType))
                {
                    errorMessage = SR.NonSupportedDynamicArgumentType;
                }
                if (underlyingKeyType != typeof(string))
                {
                    errorMessage += SR.NonSupportedDynamicArgumentKeyType;
                }
                isDictionary = true;
            }
            else
            {
                errorMessage = SR.NonSupportedModelItemCollectionOrDictionary;
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorReporting.ShowErrorMessage(SR.NonSupportedModelItemCollectionOrDictionary);
                return null;
            }
            if (isDictionary)
            {
                ObservableCollection<DynamicArgumentWrapperObject> wrappers = new ObservableCollection<DynamicArgumentWrapperObject>();
                foreach (ModelItem item in GetArgumentCollection(model))
                {
                    wrappers.Add(new DynamicArgumentWrapperObject(item.Properties["Key"].ComputedValue as string, item.Properties["Value"].Value));
                }
                return wrappers;
            }
            else
            {
                ObservableCollection<DynamicArgumentWrapperObject> wrappers = new ObservableCollection<DynamicArgumentWrapperObject>();
                foreach (ModelItem item in GetArgumentCollection(model))
                {
                    wrappers.Add(new DynamicArgumentWrapperObject(null, item));
                }
                return wrappers;
            }
        }

        internal static void WrapperCollectionToModelItem(ObservableCollection<DynamicArgumentWrapperObject> wrappers, ModelItem data, bool isDictionary, Type underlyingArgumentType)
        {
            ModelItemCollection collection = GetArgumentCollection(data);
            using (ModelEditingScope change = collection.BeginEdit(SR.UpdateDynamicArgumentsDescription))
            {
                if (isDictionary)
                {
                    collection.Clear();
                    Type dictionaryEntryType = typeof(ModelItemKeyValuePair<,>).MakeGenericType(new Type[] { typeof(string), underlyingArgumentType });
                    foreach (DynamicArgumentWrapperObject wrapper in wrappers)
                    {
                        Argument argument = Argument.Create(wrapper.Type, wrapper.Direction);
                        object mutableKVPair = Activator.CreateInstance(dictionaryEntryType, new object[] { wrapper.Name, argument });
                        ModelItem argumentKVPair = collection.Add(mutableKVPair);
                        if (wrapper.Expression != null)
                        {
                            argumentKVPair.Properties["Value"].Value.Properties["Expression"].SetValue(wrapper.Expression.GetCurrentValue());
                        }
                    }
                }
                else
                {
                    collection.Clear();
                    foreach (DynamicArgumentWrapperObject wrapper in wrappers)
                    {
                        Argument argument = Argument.Create(wrapper.Type, wrapper.Direction);
                        ModelItem argumentItem = collection.Add(argument);
                        if (wrapper.Expression != null)
                        {
                            argumentItem.Properties["Expression"].SetValue(wrapper.Expression.GetCurrentValue());
                        }
                    }
                }

                change.Complete();
            }
        }

        static ModelItemCollection GetArgumentCollection(ModelItem data)
        {
            if (data is ModelItemCollection)
            {
                return (data as ModelItemCollection);
            }
            else if (data is ModelItemDictionary)
            {
                return (data as ModelItemDictionary).Properties["ItemsCollection"].Collection;
            }
            else
            {
                ErrorReporting.ShowErrorMessage(SR.NonSupportedModelItemCollectionOrDictionary);
                return null;
            }
        }

        void OnDynamicArgumentsLoaded()
        {
            Fx.Assert(this.Context != null, "EditingContext cannot be null");
            Fx.Assert(this.IsDictionary != null, "IsDictionary is not set");
            Fx.Assert(this.UnderlyingArgumentType != null, "UnderlyingArgumentType is not set");            
            if (!(this.IsDictionary.Value))
            {
                this.WPF_DataGrid.Columns[DynamicArgumentDesigner.NameColumn].Visibility = Visibility.Hidden;
            }

            if (null != this.DynamicArguments)
            {
                if (this.UnderlyingArgumentType == ArgumentType)
                {
                    this.IsDirectionReadOnly = false;
                }

                if (this.UnderlyingArgumentType.IsGenericType)
                {
                    Type[] innerArgumentTypes = this.UnderlyingArgumentType.GetGenericArguments();
                    if (innerArgumentTypes.Length > 0)
                    {
                        Type innerArgumentType = innerArgumentTypes[0];
                        this.WPF_DataGrid.Columns[DynamicArgumentDesigner.ArgumentTypeColumn].IsReadOnly = !innerArgumentType.IsGenericParameter;
                    }
                }
            }

            this.WPF_DataGrid.ItemsSource = this.DynamicArguments;

            UpdateChildrenElementStatus();
        }

        internal void ValidateEntry(DynamicArgumentWrapperObject entry, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DynamicArgumentWrapperObject.NameProperty)
            {
                if (this.IsDictionary.Value)
                {
                    DataGridRow row = entry.Row;
                    string newName = e.NewValue as string;

                    bool duplicates =
                        this.DynamicArguments.Any<DynamicArgumentWrapperObject>(
                            p => string.Equals(p.Name, newName) && p != entry);
                    if (duplicates || string.IsNullOrEmpty(newName))
                    {
                        entry.Name = e.OldValue as string;
                        if (duplicates)
                        {
                            ErrorReporting.ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, SR.DuplicateArgumentName, newName));
                        }
                        else
                        {
                            ErrorReporting.ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, SR.EmptyArgumentName));
                        }
                    }
                    entry.IsValidating = false;
                }
            }
            else
            {
                if (e.Property == DynamicArgumentWrapperObject.DirectionProperty)
                {
                    entry.UseLocationExpression = (entry.Direction != ArgumentDirection.In);
                }                
                if ((e.Property != DynamicArgumentWrapperObject.ExpressionProperty) && (entry.Expression != null))
                {
                    ActivityWithResult expression = entry.Expression.GetCurrentValue() as ActivityWithResult;                    
                    if (expression != null)
                    {
                        ActivityWithResult newExpression;
                        if (ExpressionHelper.TryMorphExpression(expression, entry.UseLocationExpression, entry.Type, this.Context, out newExpression))
                        {
                            entry.Expression = (this.OwnerActivity as IModelTreeItem).ModelTreeManager.WrapAsModelItem(newExpression);
                        }
                        else
                        {
                            //[....] 

                            entry.Expression = null;
                        }
                    }
                }
                entry.IsValidating = false;
            }
        }

        internal string GetDefaultName()
        {
            if (!this.IsDictionary.Value)
            {
                return string.Empty;
            }
            else
            {
                var defaultNames = this.DynamicArguments
                        .Select<DynamicArgumentWrapperObject, string>(p => (string)p.Name)
                        .Where<string>(p => 0 == string.Compare(p, 0, this.ArgumentPrefix, 0, this.ArgumentPrefix.Length, StringComparison.Ordinal))
                        .Select(p => p.Substring(this.ArgumentPrefix.Length));

                int maxNum = 1;
                foreach (string numberPart in defaultNames)
                {
                    int current;
                    if (int.TryParse(numberPart, out current))
                    {
                        if (current >= maxNum)
                        {
                            maxNum = current + 1;
                        }
                    }
                }
                return string.Format(CultureInfo.InvariantCulture, "{0}{1}", this.ArgumentPrefix, maxNum);
            }

        }

        internal Type GetDefaultType()
        {
            Type[] genericArguments = this.UnderlyingArgumentType.GetGenericArguments();
            if (genericArguments.Length == 0)
            {
                return typeof(string);
            }
            else
            {
                return genericArguments[0];
            }
        }

        internal ArgumentDirection GetDefaultDirection()
        {
            if (this.UnderlyingArgumentType == ArgumentType)
            {
                return ArgumentDirection.In;
            }

            if (InArgumentType.IsAssignableFrom(this.UnderlyingArgumentType))
            {
                return ArgumentDirection.In;
            }

            if (OutArgumentType.IsAssignableFrom(this.UnderlyingArgumentType))
            {
                return ArgumentDirection.Out;
            }

            Fx.Assert(InOutArgumentType.IsAssignableFrom(this.UnderlyingArgumentType), "UnderlyingArgumentType should be of type OutArgumentType");
            return ArgumentDirection.InOut;
        }

        //Hook LoadingRow event to set different row template (<Click here to add new item>) for new place holder
        void DataGrid_Standard_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item != CollectionView.NewItemPlaceholder)
            {
                DynamicArgumentWrapperObject wrapper = e.Row.Item as DynamicArgumentWrapperObject;
                wrapper.Row = e.Row;
            }
        }

        void OnCreateDynamicArgumentExecute(object sender, ExecutedRoutedEventArgs e)
        {
            DynamicArgumentWrapperObject wrapper = new DynamicArgumentWrapperObject();
            this.DynamicArguments.Add(wrapper);
            this.dgHelper.BeginRowEdit(wrapper);
        }

        void OnMoveUpArgumentExecute(object sender, RoutedEventArgs e)
        {
            if (null != this.WPF_DataGrid.SelectedItem)
            {
                int selectedArgumentIndex = this.WPF_DataGrid.SelectedIndex;
                if (selectedArgumentIndex > 0)
                {
                    this.DynamicArguments.Move(selectedArgumentIndex, selectedArgumentIndex - 1);
                }
                this.OnDataGridSelectionChanged(this, null);
            }
        }

        void OnMoveDownArgumentExecute(object sender, RoutedEventArgs e)
        {
            if (null != this.WPF_DataGrid.SelectedItem)
            {
                int selectedArgumentIndex = this.WPF_DataGrid.SelectedIndex;
                if (selectedArgumentIndex < this.DynamicArguments.Count - 1)
                {
                    this.DynamicArguments.Move(selectedArgumentIndex, selectedArgumentIndex + 1);
                }
                this.OnDataGridSelectionChanged(this, null);
            }
        }

        void OnDeleteArgumentExecute(object sender, RoutedEventArgs e)
        {
            DataGridHelper.OnDeleteSelectedItems(this.WPF_DataGrid);
        }

        void OnExpressionTextBoxLoaded(object sender, RoutedEventArgs args)
        {
            ExpressionTextBox etb = (ExpressionTextBox)sender;
            etb.IsIndependentExpression = true;
            if (!etb.IsReadOnly)
            {
                DataGridHelper.OnEditingControlLoaded(sender, args);
            }
        }

        void OnExpressionTextBoxUnloaded(object sender, RoutedEventArgs args)
        {
            ExpressionTextBox etb = (ExpressionTextBox)sender;
            if (!etb.IsReadOnly)
            {
                DataGridHelper.OnEditingControlUnloaded(sender, args);
            }
        }

        void OnEditingControlLoaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlLoaded(sender, args);
        }

        void OnEditingControlUnloaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlUnloaded(sender, args);
        }
    }

    sealed class DynamicArgumentWrapperObject : DependencyObject
    {
        public static readonly DependencyProperty ModelItemProperty = DependencyProperty.Register(
            "ModelItem",
            typeof(ModelItem),
            typeof(DynamicArgumentWrapperObject),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            "Name",
            typeof(string),
            typeof(DynamicArgumentWrapperObject),
            new UIPropertyMetadata(string.Empty, OnArgumentPropertyChanged));

        public static readonly DependencyProperty ArgumentTypeProperty = DependencyProperty.Register(
            "Type",
            typeof(Type),
            typeof(DynamicArgumentWrapperObject),
            new UIPropertyMetadata(typeof(string), OnArgumentPropertyChanged));

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            "Direction",
            typeof(ArgumentDirection),
            typeof(DynamicArgumentWrapperObject),
            new UIPropertyMetadata(ArgumentDirection.In, OnArgumentPropertyChanged));

        public static readonly DependencyProperty ExpressionProperty =
            DependencyProperty.Register(
            "Expression",
            typeof(ModelItem),
            typeof(DynamicArgumentWrapperObject),
            new UIPropertyMetadata(OnArgumentPropertyChanged));

        public static readonly DependencyProperty UseLocationExpressionProperty =
            DependencyProperty.Register(
            "UseLocationExpression",
            typeof(bool),
            typeof(DynamicArgumentWrapperObject));

        public event PropertyChangedEventHandler PropertyChanged;

        bool isInitializing;

        const string ExpressionPropertyName = "Expression";

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public Type Type
        {
            get { return (Type)GetValue(ArgumentTypeProperty); }
            set { SetValue(ArgumentTypeProperty, value); }
        }

        public ArgumentDirection Direction
        {
            get { return (ArgumentDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public ModelItem Expression
        {
            get { return (ModelItem)GetValue(ExpressionProperty); }
            set { SetValue(ExpressionProperty, value); }
        }

        public bool UseLocationExpression
        {
            get { return (bool)GetValue(UseLocationExpressionProperty); }
            set { SetValue(UseLocationExpressionProperty, value); }
        }

        internal bool IsValidating
        {
            get;
            set;
        }

        internal DataGridRow Row
        {
            get;
            set;
        }

        public static DynamicArgumentDesigner Editor
        {
            get;
            set;
        }

        public DynamicArgumentWrapperObject()
        {
            this.isInitializing = true;
            this.IsValidating = false;
            this.Name = DynamicArgumentWrapperObject.Editor.GetDefaultName();
            this.Type = DynamicArgumentWrapperObject.Editor.GetDefaultType();
            this.Direction = DynamicArgumentWrapperObject.Editor.GetDefaultDirection();
            this.UseLocationExpression = (this.Direction != ArgumentDirection.In);
            this.isInitializing = false;
        }

        public DynamicArgumentWrapperObject(string argumentName, ModelItem argumentItem)
        {
            Fx.Assert(argumentItem != null, "argumentItem canot be null");
            this.isInitializing = true;
            this.IsValidating = false;            
            Argument argument = (Argument)argumentItem.GetCurrentValue();
            this.Name = argumentName;
            this.Direction = argument.Direction;
            this.UseLocationExpression = (this.Direction != ArgumentDirection.In);
            this.Type = argument.ArgumentType;
            this.Expression = argumentItem.Properties[ExpressionPropertyName].Value;
            this.isInitializing = false;
        }

        static void OnArgumentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DynamicArgumentWrapperObject wrapper = (DynamicArgumentWrapperObject)sender;
            if (!wrapper.IsValidating && !wrapper.isInitializing)
            {
                wrapper.OnArgumentPropertyChanged(e);
            }
        }

        void OnArgumentPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            Fx.Assert(DynamicArgumentWrapperObject.Editor != null, "collection editor is null!");
            this.IsValidating = true;
            DynamicArgumentWrapperObject.Editor.ValidateEntry(this, e);

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(e.Property.Name));
            }
        }

        // For screen reader to read the DataGrid row.
        public override string ToString()
        {
            return string.IsNullOrEmpty(this.Name) ? "Parameter" : this.Name;
        }
    }
}
