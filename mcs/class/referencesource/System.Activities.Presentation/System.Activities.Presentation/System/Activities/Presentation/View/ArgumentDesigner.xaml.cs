//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.Expressions;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Xaml;

    partial class ArgumentDesigner
    {
        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register(
            "Context",
            typeof(EditingContext),
            typeof(ArgumentDesigner),
            new FrameworkPropertyMetadata(null, OnContextChanged));

        public static readonly DependencyProperty ActivitySchemaProperty = DependencyProperty.Register(
            "ActivitySchema",
            typeof(ModelItem),
            typeof(ArgumentDesigner),
            new FrameworkPropertyMetadata(OnActivitySchemaChanged));

        public static readonly RoutedEvent ArgumentCollectionChangedEvent = EventManager.RegisterRoutedEvent(
            "ArgumentCollectionChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ArgumentDesigner));

        static readonly string DefaultArgumentName = "argument";
        static readonly string Members = "Properties";
        static readonly string ArgumentNamePropertyName = "Name";

        ObservableCollection<DesignTimeArgument> argumentWrapperCollection = new ObservableCollection<DesignTimeArgument>();

        bool isCollectionLoaded = false;
        bool isDataGridPopulating = false;
        ModelItem lastSelection;
        bool isSelectionChangeInternal = false;
        ArgumentToExpressionConverter argumentToExpressionConverter;
        DataGridHelper dgHelper;

        public ArgumentDesigner()
        {
            InitializeComponent();

            this.dgHelper = new DataGridHelper(this.argumentsDataGrid, this);
            this.dgHelper.Context = this.Context;
            this.dgHelper.AddNewRowContent = (string)this.FindResource("addNewArgumentTitle");
            this.dgHelper.AddNewRowCommand = DesignerView.CreateArgumentCommand;
            this.dgHelper.ResolveDynamicTemplateCallback = this.OnResolveDynamicContentTemplate;
            this.dgHelper.LoadDynamicContentDataCallback = this.OnShowExtendedValueEditor;
            this.dgHelper.LoadCustomPropertyValueEditorCallback = this.OnLoadExtendedValueEditor;

            this.argumentsDataGrid.SelectionChanged += OnDataGridArgumentSelected;
            this.argumentsDataGrid.GotFocus += OnDataGridArgumentSelected;

            this.argumentWrapperCollection.CollectionChanged += OnArgumentWrapperCollectionChanged;
            this.argumentsDataGrid.ItemsSource = this.argumentWrapperCollection;

            this.argumentsDataGrid.LayoutUpdated += OnArgumentDataGridLayoutUpdated;
        }

        public event RoutedEventHandler ArgumentCollectionChanged
        {
            add
            {
                AddHandler(ArgumentCollectionChangedEvent, value);
            }
            remove
            {
                RemoveHandler(ArgumentCollectionChangedEvent, value);
            }
        }


        public ModelItem ActivitySchema
        {
            get { return (ModelItem)GetValue(ActivitySchemaProperty); }
            set { SetValue(ActivitySchemaProperty, value); }
        }

        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        internal ArgumentToExpressionConverter ArgumentToExpressionConverter
        {
            get
            {
                if (null == this.argumentToExpressionConverter)
                {
                    this.argumentToExpressionConverter = new ArgumentToExpressionConverter();
                }
                return this.argumentToExpressionConverter;
            }
        }

        public bool CreateNewArgumentWrapper()
        {
            bool result = false;

            if (null != this.ActivitySchema)
            {
                DynamicActivityProperty property = new DynamicActivityProperty()
                {
                    Name = this.GetDefaultName(),
                    Type = this.GetDefaultType(),
                };
                DesignTimeArgument wrapper = null;
                using (ModelEditingScope scope = this.ActivitySchema.BeginEdit((string)this.FindResource("addNewArgumentDescription")))
                {
                    ModelItem argument = this.GetArgumentCollection().Add(property);
                    wrapper = new DesignTimeArgument(argument, this);
                    this.argumentWrapperCollection.Add(wrapper);
                    scope.Complete();
                    result = true;
                }
                this.dgHelper.BeginRowEdit(wrapper);
            }
            return result;
        }

        ModelItemCollection GetArgumentCollection()
        {
            if (this.ActivitySchema != null)
            {
                Fx.Assert(this.ActivitySchema.Properties[Members] != null, "Members collection not found!");
                return this.ActivitySchema.Properties[Members].Collection;
            }
            return null;
        }


        string GetDefaultName()
        {
            ModelItemCollection argumentCollection = this.GetArgumentCollection();
            return argumentCollection.GetUniqueName(ArgumentDesigner.DefaultArgumentName, (arg) => ((string)arg.Properties["Name"].ComputedValue));
        }

        Type GetDefaultType()
        {
            return typeof(InArgument<string>);
        }

        void Populate()
        {
            if (!this.isCollectionLoaded)
            {
                this.argumentsDataGrid.ItemsSource = null;
                this.argumentWrapperCollection.All(p => { p.Dispose(); return true; });
                this.argumentWrapperCollection.Clear();
                ModelItemCollection arguments = this.GetArgumentCollection();
                if (null != arguments)
                {
                    foreach (ModelItem argument in arguments)
                    {
                        this.argumentWrapperCollection.Add(new DesignTimeArgument(argument, this));
                    }
                }
                this.argumentsDataGrid.ItemsSource = this.argumentWrapperCollection;
                this.isCollectionLoaded = true;
            }
        }

        void StoreLastSelection()
        {
            if (!this.isSelectionChangeInternal)
            {
                ModelItem current = this.Context.Items.GetValue<Selection>().PrimarySelection;
                if (null == current || !typeof(DesignTimeArgument).IsAssignableFrom(current.ItemType))
                {
                    this.lastSelection = current;
                }
            }
        }

        void OnArgumentTypeTypePresenterLoaded(object sender, RoutedEventArgs args)
        {
            TypePresenter argumentTypeTypePresenter = ((TypePresenter)sender);
            argumentTypeTypePresenter.Filter = ((DesignTimeArgument)argumentTypeTypePresenter.DataContext).Filter;
            DataGridHelper.OnEditingControlLoaded(sender, args);
        }

        void OnArgumentTypeTypePresenterUnloaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlUnloaded(sender, args);
        }

        internal void SelectArgument(ModelItem argument)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (object item in this.argumentsDataGrid.Items)
                {
                    if (item is DesignTimeArgument)
                    {
                        if (object.ReferenceEquals(((DesignTimeArgument)item).ReflectedObject, argument))
                        {
                            this.argumentsDataGrid.SelectedItem = item;
                            this.argumentsDataGrid.ScrollIntoView(item, null);
                        }
                    }
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        void OnDataGridArgumentSelected(object sender, RoutedEventArgs e)
        {
            if (null != this.Context && !this.isSelectionChangeInternal)
            {
                this.isSelectionChangeInternal = true;
                DesignTimeArgument argument = this.dgHelper.SelectedItem<DesignTimeArgument>();
                if (null != argument)
                {
                    this.Context.Items.SetValue(new Selection(argument.Content));
                }
                else
                {
                    // clear arguments in selection
                    Selection oldSelection = this.Context.Items.GetValue<Selection>();
                    List<ModelItem> newSelection = new List<ModelItem>();
                    if (oldSelection != null && oldSelection.SelectionCount > 0)
                    {
                        foreach (ModelItem item in oldSelection.SelectedObjects)
                        {
                            if (item.ItemType != typeof(DesignTimeArgument))
                            {
                                newSelection.Add(item);
                            }
                        }
                    }
                    this.Context.Items.SetValue(new Selection(newSelection));
                }
                this.isSelectionChangeInternal = false;
            }
        }

        void OnArgumentDataGridLayoutUpdated(object sender, EventArgs e)
        {
            if (this.isDataGridPopulating)
            {
                this.isDataGridPopulating = false;
                Mouse.OverrideCursor = null;
            }
        }

        void OnActivitySchemaChanged(ModelItem newSchemaItem)
        {
            this.isCollectionLoaded = false;

            if (null != newSchemaItem && null != newSchemaItem.Properties[Members])
            {
                //lazy initialization, wait till it is visible
                if (this.Visibility == Visibility.Visible)
                {
                    this.Populate();
                }
            }
        }

        void OnContextChanged()
        {
            if (null != this.Context)
            {
                this.Context.Items.Subscribe<Selection>(new SubscribeContextCallback<Selection>(OnItemSelected));
            }
            this.dgHelper.Context = this.Context;
        }

        void OnItemSelected(Selection selection)
        {
            if (!this.isSelectionChangeInternal)
            {
                this.StoreLastSelection();

                bool selectedArgumentIsInSelection = false;

                DesignTimeArgument selectedArgument = this.argumentsDataGrid.SelectedItem as DesignTimeArgument;
                if (selectedArgument != null)
                {
                    foreach (ModelItem item in selection.SelectedObjects)
                    {
                        if (object.ReferenceEquals(selectedArgument, item.GetCurrentValue()))
                        {
                            selectedArgumentIsInSelection = true;
                        }
                    }
                }

                if (!selectedArgumentIsInSelection)
                {
                    this.argumentsDataGrid.SelectedItem = null;
                }
            }
        }

        void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Dispatcher.HasShutdownStarted)
            {
                return;
            }

            if ((Boolean)e.NewValue == true)
            {
                // Changing cursor as Populate() might take long to run. Cursor will be restored when DataGrid.LayoutUpdated fires.
                this.isDataGridPopulating = true;
                Mouse.OverrideCursor = Cursors.Wait;

                this.StoreLastSelection();
                this.Populate();
            }
            else
            {
                if (this.argumentsDataGrid.SelectedItem != null)
                {
                    // Clear argument selection, if possible, restore last selection.
                    Selection restoredSelection = null == this.lastSelection ? new Selection() : new Selection(this.lastSelection);
                    this.isSelectionChangeInternal = true;
                    this.Context.Items.SetValue(restoredSelection);
                    this.argumentsDataGrid.SelectedItem = null;
                    this.isSelectionChangeInternal = false;
                }
            }
        }

        void OnArgumentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //we need to track argument collection changes caused by undo/redo stack - 
            //in such case, we have to add/remove corresponding items from wrapper collection
            bool isUndoRedoInProgress = this.Context.Services.GetService<UndoEngine>().IsUndoRedoInProgress;
            if (isUndoRedoInProgress)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (ModelItem argument in e.NewItems)
                        {
                            var wrapper = this.argumentWrapperCollection
                                .FirstOrDefault(p => (ModelItem.Equals(p.ReflectedObject, argument)));

                            if (wrapper == null)
                            {
                                wrapper = new DesignTimeArgument(argument, this);
                                this.argumentWrapperCollection.Add(wrapper);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (ModelItem argument in e.OldItems)
                        {
                            var wrapper = this.argumentWrapperCollection.FirstOrDefault(p => ModelItem.Equals(p.ReflectedObject, argument));
                            if (null != wrapper)
                            {
                                this.argumentWrapperCollection.Remove(wrapper);
                            }
                        }
                        break;
                }
            }
        }
        void OnArgumentWrapperCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool isUndoRedoInProgress = this.Context.Services.GetService<UndoEngine>().IsUndoRedoInProgress;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (DesignTimeArgument arg in e.OldItems)
                    {
                        this.ClearCaseInsensitiveDuplicates(arg.GetArgumentName(), (string)arg.ReflectedObject.Properties["Name"].ComputedValue);
                        if (!isUndoRedoInProgress)
                        {
                            ModelItemCollection collection = (ModelItemCollection)arg.ReflectedObject.Parent;
                            collection.Remove(arg.ReflectedObject);
                        }
                        arg.Dispose();
                    }
                    break;
                case NotifyCollectionChangedAction.Add:
                    foreach (DesignTimeArgument arg in e.NewItems)
                    {
                        this.CheckCaseInsensitiveDuplicates(arg.GetArgumentName(), (string)arg.ReflectedObject.Properties["Name"].ComputedValue);
                    }
                    break;
            }
        }

        bool OnResolveDynamicContentTemplate(ResolveTemplateParams resolveParams)
        {
            var argument = (DesignTimeArgument)resolveParams.Cell.DataContext;

            //get editor associated with variable's value
            var editorType = argument.GetDynamicPropertyValueEditorType(DesignTimeArgument.ArgumentDefaultValueProperty);

            //if yes there is a custom one - use it
            if (!typeof(DesignTimeArgument.DefaultValueEditor).IsAssignableFrom(editorType))
            {
                //get inline editor template - it will be used for both templates - view and editing; 
                resolveParams.Template = argument.GetDynamicPropertyValueEditor(DesignTimeArgument.ArgumentDefaultValueProperty).InlineEditorTemplate;
                resolveParams.IsDefaultTemplate = false;
            }
            else
            {
                //no custom editor - depending on grid state display either editable or readonly expression template
                string key = string.Empty;
                switch (argument.GetArgumentDirection())
                {
                    case PropertyKind.Property:
                        key = resolveParams.Cell.IsEditing ? "argumentPropertyEditableTemplate" : "argumentPropertyReadOnlyTemplate";
                        break;

                    case PropertyKind.InArgument:
                        key = resolveParams.Cell.IsEditing ? "argumentExpressionEditableTemplate" : "argumentExpressionReadOnlyTemplate";
                        break;

                    case PropertyKind.OutArgument:
                    case PropertyKind.InOutArgument:
                        key = "argumentOutputValueTemplate";
                        break;
                }
                resolveParams.Template = (DataTemplate)this.FindResource(key);
                resolveParams.IsDefaultTemplate = true;
            }
            return true;
        }

        DialogPropertyValueEditor OnLoadExtendedValueEditor(DataGridCell cell, object instance)
        {
            var argument = (DesignObjectWrapper)cell.DataContext;
            return argument.GetDynamicPropertyValueEditor(DesignTimeArgument.ArgumentDefaultValueProperty) as DialogPropertyValueEditor;
        }

        ModelProperty OnShowExtendedValueEditor(DataGridCell cell, object instance)
        {
            var argument = (DesignObjectWrapper)cell.DataContext;
            return argument.Content.Properties[DesignTimeArgument.ArgumentDefaultValueProperty];
        }

        internal void UpdateTypeDesigner(DesignTimeArgument argument)
        {
            this.dgHelper.UpdateDynamicContentColumns(argument);
        }

        //Check case-insensitive duplicates, which are not allowed in VB expressions 
        internal void CheckCaseInsensitiveDuplicates(VBIdentifierName identifierName, string newName)
        {
            Func<DesignTimeArgument, bool> checkForDuplicates = new Func<DesignTimeArgument, bool>(p => string.Equals((string)p.ReflectedObject.Properties["Name"].ComputedValue, newName, StringComparison.OrdinalIgnoreCase) && !object.Equals(p.GetArgumentName(), identifierName));
            DesignTimeArgument duplicate = this.argumentWrapperCollection.FirstOrDefault<DesignTimeArgument>(checkForDuplicates);
            if (duplicate != null)
            {
                identifierName.IsValid = false;
                identifierName.ErrorMessage = string.Format(CultureInfo.CurrentUICulture, SR.DuplicateIdentifier, newName);
                VBIdentifierName duplicateIdentifier = duplicate.GetArgumentName();
                if (duplicateIdentifier.IsValid)
                {
                    duplicateIdentifier.IsValid = false;
                    duplicateIdentifier.ErrorMessage = string.Format(CultureInfo.CurrentUICulture, SR.DuplicateIdentifier, duplicateIdentifier.IdentifierName);
                }
            };
        }

        //Check duplicates with old value. When there's only one variable duplicate with the old value, 
        //the only one variable should be valid now after the change
        void ClearCaseInsensitiveDuplicates(VBIdentifierName identifier, string oldName)
        {
            Func<DesignTimeArgument, bool> checkForOldNameDuplicates = new Func<DesignTimeArgument, bool>(p => string.Equals((string)p.ReflectedObject.Properties["Name"].ComputedValue, oldName, StringComparison.OrdinalIgnoreCase) && !object.Equals(p.GetArgumentName(), identifier));
            IEnumerable<DesignTimeArgument> oldDuplicates = this.argumentWrapperCollection.Where<DesignTimeArgument>(checkForOldNameDuplicates);
            if (oldDuplicates.Count<DesignTimeArgument>() == 1)
            {
                DesignTimeArgument wrapper = oldDuplicates.First<DesignTimeArgument>();
                VBIdentifierName oldDuplicate = wrapper.GetArgumentName();
                oldDuplicate.IsValid = true;
                oldDuplicate.ErrorMessage = string.Empty;
            }
        }

        internal void ValidateArgumentName(VBIdentifierName identifierName, string newName, string oldName)
        {
            //Check whether there're any variables' name conflict with the old name which can be cleaned up now
            this.ClearCaseInsensitiveDuplicates(identifierName, oldName);

            //Check whether there're any duplicates with new name                
            this.CheckCaseInsensitiveDuplicates(identifierName, newName);
        }

        static void OnActivitySchemaChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ModelItem oldItem = e.OldValue as ModelItem;
            ModelItem newItem = e.NewValue as ModelItem;
            ArgumentDesigner designer = (ArgumentDesigner)dependencyObject;

            if (null != oldItem && null != oldItem.Properties[Members])
            {
                oldItem.Properties[Members].Collection.CollectionChanged -= designer.OnArgumentCollectionChanged;
            }
            if (null != newItem && null != newItem.Properties[Members])
            {
                newItem.Properties[Members].Collection.CollectionChanged += designer.OnArgumentCollectionChanged;
            }
            designer.OnActivitySchemaChanged(newItem);
        }

        static void OnContextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((ArgumentDesigner)dependencyObject).OnContextChanged();
        }

        internal void UpdateArgumentName(DesignTimeArgument argumentWrapper, string newName, string oldName)
        {
            ModelItemCollection argumentsCollection = this.GetArgumentCollection();

            //Since underlying object is an KeyedCollection, if we only update the property value, it won't update the key
            //Need to remove the object and add it again to update the key
            ModelItem argument = argumentWrapper.ReflectedObject;
            argumentsCollection.Remove(argument);
            argument.Properties[ArgumentNamePropertyName].SetValue(newName);
            argumentsCollection.Add(argument);
            this.ValidateArgumentName(argumentWrapper.GetArgumentName(), newName, oldName);

            //Update default value editor in Argument designer
            this.UpdateTypeDesigner(argumentWrapper);
        }

        void OnEditingControlLoaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlLoaded(sender, args);
        }

        void OnEditingControlUnloaded(object sender, RoutedEventArgs args)
        {
            DataGridHelper.OnEditingControlUnloaded(sender, args);
        }

        // This is to workaround a 
        internal void NotifyAnnotationTextChanged()
        {
            foreach (object item in this.argumentsDataGrid.Items)
            {
                DesignTimeArgument designTimeArgument = item as DesignTimeArgument;
                if (designTimeArgument != null)
                {
                    designTimeArgument.NotifyPropertyChanged(DesignTimeArgument.AnnotationTextProperty);
                }
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);

            DesignerConfigurationService configurationService = this.Context.Services.GetService<DesignerConfigurationService>();
            Fx.Assert(configurationService != null, "DesignerConfigurationService should not be null");
            if (configurationService.WorkflowDesignerHostId == WorkflowDesignerHostId.Dev10)
            {
                return;
            }

            e.Handled = true;

            bool openedByKeyboard = e.CursorLeft < 0 && e.CursorTop < 0;

            if (openedByKeyboard)
            {
                this.ContextMenu.Placement = PlacementMode.Center;
            }
            else
            {
                this.ContextMenu.Placement = PlacementMode.MousePoint;
            }
            this.ContextMenu.PlacementTarget = this;
            this.ContextMenu.IsOpen = true;
        }

        private void OnDeleteCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ContextMenuUtilities.OnDeleteCommandCanExecute(e, this.argumentsDataGrid);
        }

        private void OnDeleteCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.argumentsDataGrid != null && this.argumentsDataGrid.SelectedItems != null && this.argumentsDataGrid.SelectedItems.Count > 0)
            {
                List<ModelItem> list = new List<ModelItem>();
                foreach (object item in this.argumentsDataGrid.SelectedItems)
                {
                    DesignTimeArgument designTimeArgument = item as DesignTimeArgument;
                    if (designTimeArgument != null)
                    {
                        list.Add(designTimeArgument.ReflectedObject);
                    }
                }

                foreach (ModelItem modelItem in list)
                {
                    foreach (DesignTimeArgument designTimeArgument in this.argumentWrapperCollection)
                    {
                        if (designTimeArgument.ReflectedObject == modelItem)
                        {
                            this.argumentWrapperCollection.Remove(designTimeArgument);
                            break;
                        }
                    }
                }
            }

            e.Handled = true;
        }

        private void OnAnnotationSeparatorLoaded(object sender, RoutedEventArgs e)
        {
            ContextMenuUtilities.OnAnnotationMenuLoaded(this.Context, (Control)sender, e);
        }

        private void OnAddAnnotationMenuLoaded(object sender, RoutedEventArgs e)
        {
            ContextMenuUtilities.OnAnnotationMenuLoaded(this.Context, (Control)sender, e);
        }

        private void OnAddAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ContextMenuUtilities.OnAddAnnotationCommandCanExecute(e, this.Context, this.argumentsDataGrid);
        }

        private void OnAddAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.argumentsDataGrid != null && this.argumentsDataGrid.SelectedItems != null && this.argumentsDataGrid.SelectedItems.Count == 1)
            {
                AnnotationDialog dialog = new AnnotationDialog();
                dialog.Context = Context;
                dialog.Title = SR.AddAnnotationTitle;

                WindowHelperService service = this.Context.Services.GetService<WindowHelperService>();
                if (null != service)
                {
                    service.TrySetWindowOwner(this, dialog);
                }
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                if (dialog.ShowDialog() == true)
                {
                    string annotationText = dialog.AnnotationText;

                    DesignTimeArgument variable = (DesignTimeArgument)this.argumentsDataGrid.SelectedItems[0];
                    variable.Content.Properties[DesignTimeArgument.AnnotationTextProperty].SetValue(annotationText);
                }
            }

            e.Handled = true;
        }

        private void OnEditAnnotationMenuLoaded(object sender, RoutedEventArgs e)
        {
            ContextMenuUtilities.OnAnnotationMenuLoaded(this.Context, (Control)sender, e);
        }

        private void OnEditAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // call the same method as delete annotation command
            ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context, this.argumentsDataGrid);
        }

        private void OnEditAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.argumentsDataGrid != null && this.argumentsDataGrid.SelectedItems != null && this.argumentsDataGrid.SelectedItems.Count == 1)
            {
                DesignTimeArgument variable = (DesignTimeArgument)this.argumentsDataGrid.SelectedItems[0];

                AnnotationDialog dialog = new AnnotationDialog();
                dialog.Context = Context;
                dialog.Title = SR.EditAnnotationTitle;
                dialog.AnnotationText = variable.Content.Properties[DesignTimeArgument.AnnotationTextProperty].ComputedValue as string;

                WindowHelperService service = this.Context.Services.GetService<WindowHelperService>();
                if (null != service)
                {
                    service.TrySetWindowOwner(this, dialog);
                }
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                if (dialog.ShowDialog() == true)
                {
                    string annotationText = dialog.AnnotationText;

                    variable.Content.Properties[DesignTimeArgument.AnnotationTextProperty].SetValue(annotationText);
                }
            }
        }

        private void OnDeleteAnnotationMenuLoaded(object sender, RoutedEventArgs e)
        {
            ContextMenuUtilities.OnAnnotationMenuLoaded(this.Context, (Control)sender, e);
        }

        private void OnDeleteAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context, this.argumentsDataGrid);
        }

        private void OnDeleteAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.argumentsDataGrid != null && this.argumentsDataGrid.SelectedItems != null && this.argumentsDataGrid.SelectedItems.Count == 1)
            {
                DesignTimeArgument variable = (DesignTimeArgument)this.argumentsDataGrid.SelectedItems[0];
                variable.Content.Properties[DesignTimeArgument.AnnotationTextProperty].ClearValue();
            }

            e.Handled = true;
        }
    }

    sealed class DesignTimeArgument : DesignObjectWrapper
    {
        internal static readonly string ArgumentNameProperty = "Name";
        internal static readonly string ArgumentTypeProperty = "ArgumentType";
        internal static readonly string ArgumentDirectionProperty = "Direction";
        internal static readonly string ArgumentDefaultValueProperty = "Value";
        internal static readonly string IsOutputArgument = "IsOutputArgument";
        internal static readonly string OwnerSchemaProperty = "OwnerActivitySchemaType";
        internal static readonly string IsRequiredProperty = "IsRequired";
        internal static readonly string AnnotationTextProperty = "DesignTimeArgumentAnnotationText";

        static readonly string[] Properties =
            new string[] { ArgumentNameProperty, ArgumentTypeProperty, ArgumentDirectionProperty, ArgumentDefaultValueProperty, IsOutputArgument, OwnerSchemaProperty, IsRequiredProperty };

        static readonly Type inArgumentTypeReference = typeof(InArgument);
        static readonly Type outArgumentTypeReference = typeof(OutArgument);
        static readonly Type inOutArgumentTypeReference = typeof(InOutArgument);
        static readonly Type stringTypeReference = typeof(string);
        static readonly XamlSchemaContext xamlContext = new XamlSchemaContext();
        static readonly XamlType xamlType = new XamlType(typeof(string), xamlContext);
        bool argumentExpressionChanged = false;
        VBIdentifierName identifierName;

        public DesignTimeArgument()
        {
            throw FxTrace.Exception.AsError(new NotSupportedException(SR.InvalidConstructorCall));
        }

        internal DesignTimeArgument(ModelItem argument, ArgumentDesigner editor)
            : base(argument)
        {
            this.Editor = editor;
            this.ReflectedObject.Properties["Attributes"].Collection.CollectionChanged += new NotifyCollectionChangedEventHandler(OnAttributesChanged);
            this.identifierName = new VBIdentifierName(true)
            {
                IdentifierName = (string)argument.Properties[ArgumentNameProperty].ComputedValue
            };
        }

        void OnAttributesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChangedEvent(IsRequiredProperty);
        }

        public override void Dispose()
        {
            this.ReflectedObject.Properties["Attributes"].Collection.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnAttributesChanged);
            base.Dispose();
        }

        #region Initialize type properties code
        public static PropertyDescriptorData[] InitializeTypeProperties()
        {
            return new PropertyDescriptorData[]
            {
                new PropertyDescriptorData()
                {
                    PropertyName = ArgumentNameProperty,
                    PropertyType = typeof(VBIdentifierName),
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(VBIdentifierName)).OfType<Attribute>().ToArray(),
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeArgument)instance).SetArgumentName((VBIdentifierName)newValue);
                        },
                    PropertyGetter = (instance) => (((DesignTimeArgument)instance).GetArgumentName()),
                    PropertyValidator  = (instance, value, errors) => (((DesignTimeArgument)instance).ValidateArgumentName(value, errors))
                },
                new PropertyDescriptorData()
                {
                    PropertyName = ArgumentTypeProperty,
                    PropertyType = typeof(Type),
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(Type)).OfType<Attribute>().ToArray(),
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeArgument)instance).SetArgumentType((Type)newValue);
                        },
                    PropertyGetter = (instance) => (((DesignTimeArgument)instance).GetArgumentType()),
                    PropertyValidator = null
                },
                new PropertyDescriptorData()
                {
                    PropertyName = ArgumentDirectionProperty,
                    PropertyType = typeof(PropertyKind),
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(PropertyKind)).OfType<Attribute>().Union( new Attribute[] { new EditorAttribute(typeof(DirectionPropertyEditor), typeof(PropertyValueEditor)) }).ToArray(),
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeArgument)instance).SetArgumentDirection((PropertyKind)newValue);
                        },
                    PropertyGetter = (instance) => (((DesignTimeArgument)instance).GetArgumentDirection()),
                    PropertyValidator  = null
                },
                new PropertyDescriptorData()
                {
                    PropertyName = ArgumentDefaultValueProperty,
                    PropertyType = typeof(object),
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(Activity)).OfType<Attribute>().Union(new Attribute[] { new EditorAttribute(typeof(DesignObjectWrapperDynamicPropertyEditor), typeof(PropertyValueEditor)), new EditorReuseAttribute(false) }).ToArray(),
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeArgument)instance).SetArgumentValue(newValue);
                        },
                    PropertyGetter = (instance) => (((DesignTimeArgument)instance).GetArgumentValue()),
                    PropertyValidator  = (instance, value, errors) => (((DesignTimeArgument)instance).ValidateArgumentValue(value, errors)),
                },
                new PropertyDescriptorData()
                {
                    PropertyName = IsOutputArgument,
                    PropertyType = typeof(bool),
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertyGetter = (instance) => (((DesignTimeArgument)instance).GetIsOutputArgument()), 
                    PropertyValidator = null,
                },
                new PropertyDescriptorData()
                {
                    PropertyName = OwnerSchemaProperty,
                    PropertyType = typeof(ModelItem),
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertyGetter = (instance) => (((DesignTimeArgument)instance).GetOwnerSchemaProperty()),
                },
                new PropertyDescriptorData()
                {
                    PropertyName = IsRequiredProperty,
                    PropertyType = typeof(bool),
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(bool)).OfType<Attribute>().Union(
                        new Attribute[] 
                        { 
                            new EditorAttribute(typeof(IsRequiredPropertyEditor), typeof(PropertyValueEditor)), 
                            new EditorReuseAttribute(false) 
                        }
                    ).ToArray(),
                    PropertySetter = (instance, newValue) =>
                    {
                        ((DesignTimeArgument)instance).SetIsRequired(newValue);
                    },
                    PropertyValidator = null,
                    PropertyGetter = (instance) =>
                    {
                        return ((DesignTimeArgument)instance).GetIsRequired();
                    }
                },
                new PropertyDescriptorData()
                {
                    PropertyName = AnnotationTextProperty,
                    PropertyType = typeof(string),
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeArgument)instance).SetAnnotationText(newValue);
                        },
                    PropertyValidator = null,
                    PropertyGetter = (instance) =>
                        {
                            return ((DesignTimeArgument)instance).GetAnnotationText();
                        }
                },
            };
        }
        #endregion

        internal ArgumentDesigner Editor { get; private set; }

        protected override string AutomationId
        {
            get
            {
                return this.GetArgumentNameString();
            }
        }

        internal VBIdentifierName GetArgumentName()
        {
            return this.identifierName;
        }

        string GetArgumentNameString()
        {
            return (string)this.ReflectedObject.Properties[ArgumentNameProperty].ComputedValue;
        }

        // For screen reader to read the DataGrid row.
        public override string ToString()
        {
            string name = this.GetArgumentNameString();
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            return "Argument";
        }

        object GetAnnotationText()
        {
            ModelProperty property = this.ReflectedObject.Properties.Find(Annotation.AnnotationTextPropertyName);

            if (property != null)
            {
                return property.ComputedValue;
            }
            else
            {
                return null;
            }
        }

        void SetAnnotationText(object annotationText)
        {
            ModelProperty property = this.ReflectedObject.Properties.Find(Annotation.AnnotationTextPropertyName);

            if (property != null)
            {
                property.SetValue(annotationText);
            }
        }

        void SetArgumentName(VBIdentifierName identifierName)
        {
            using (ModelEditingScope scope = this.ReflectedObject.BeginEdit((string)this.Editor.FindResource("changeArgumentNameDescription")))
            {
                this.identifierName = identifierName;
                string name = identifierName.IdentifierName;
                this.Editor.UpdateArgumentName(this, name, (string)this.ReflectedObject.Properties[ArgumentNameProperty].ComputedValue);

                scope.Complete();
            }
        }

        internal Type GetArgumentType()
        {
            Type result = (Type)this.ReflectedObject.Properties["Type"].ComputedValue;
            if (this.GetArgumentDirection() != PropertyKind.Property)
            {
                result = result.GetGenericArguments()[0];
            }
            return result;
        }

        void SetArgumentType(Type type)
        {
            using (ModelEditingScope scope = this.ReflectedObject.BeginEdit((string)this.Editor.FindResource("changeArgumentTypeDescription")))
            {
                PropertyKind currentDirection = this.GetArgumentDirection();
                Type propertyType = GetTypeReference(currentDirection, type);
                this.ReflectedObject.Properties["Type"].SetValue(propertyType);
                this.TryUpdateArgumentType(type, currentDirection);
                ImportDesigner.AddImport(type.Namespace, this.Context);
                scope.Complete();
            }
        }

        internal PropertyKind GetArgumentDirection()
        {
            PropertyKind result = PropertyKind.Property;
            Type argumentType = (Type)this.ReflectedObject.Properties["Type"].ComputedValue;
            if (inArgumentTypeReference.IsAssignableFrom(argumentType) && argumentType.IsGenericType)
            {
                result = PropertyKind.InArgument;
            }
            else if (outArgumentTypeReference.IsAssignableFrom(argumentType) && argumentType.IsGenericType)
            {
                result = PropertyKind.OutArgument;
            }
            else if (inOutArgumentTypeReference.IsAssignableFrom(argumentType) && argumentType.IsGenericType)
            {
                result = PropertyKind.InOutArgument;
            }
            return result;
        }

        void SetArgumentDirection(PropertyKind direction)
        {
            using (ModelEditingScope scope = this.ReflectedObject.BeginEdit((string)this.Editor.FindResource("changeArgumentDirectionDescription")))
            {
                Type currentType = this.GetArgumentType();
                Type propertyType = GetTypeReference(direction, currentType);
                this.ReflectedObject.Properties["Type"].SetValue(propertyType);
                this.TryUpdateArgumentType(currentType, direction);
                if (direction == PropertyKind.Property)
                {
                    this.SetIsRequired(false);
                }
                scope.Complete();
            }
        }

        Type GetTypeReference(PropertyKind direction, Type type)
        {
            Type targetType = null;
            switch (direction)
            {
                case PropertyKind.InArgument:
                    targetType = typeof(InArgument<>).MakeGenericType(type);
                    break;

                case PropertyKind.OutArgument:
                    targetType = typeof(OutArgument<>).MakeGenericType(type);
                    break;

                case PropertyKind.InOutArgument:
                    targetType = typeof(InOutArgument<>).MakeGenericType(type);
                    break;

                case PropertyKind.Property:
                    targetType = type;
                    break;

                default:
                    throw FxTrace.Exception.AsError(new NotSupportedException(direction.ToString()));
            }
            return targetType;
        }

        void SetArgumentValue(object value)
        {
            this.argumentExpressionChanged = true;
            if (PropertyKind.Property == this.GetArgumentDirection())
            {
                //handle empty string - reset the value
                if (this.GetArgumentType() != typeof(string) && value is string && string.IsNullOrEmpty((string)value))
                {
                    value = null;
                }
                //handle conversion if needed
                else if (null != value && !this.GetArgumentType().IsAssignableFrom(value.GetType()))
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(this.GetArgumentType());
                    if (converter.CanConvertFrom(value.GetType()))
                    {
                        value = converter.ConvertFrom(value);
                    }
                    else
                    {
                        value = null;
                    }
                }
                //else: leave value as is
            }
            else
            {
                if (null != value)
                {
                    string direction = null;
                    switch (this.GetArgumentDirection())
                    {
                        case PropertyKind.InArgument:
                            direction = ArgumentDirection.In.ToString();
                            break;

                        case PropertyKind.OutArgument:
                            direction = ArgumentDirection.Out.ToString();
                            break;

                        case PropertyKind.InOutArgument:
                            direction = ArgumentDirection.InOut.ToString();
                            break;

                        default:
                            throw FxTrace.Exception.AsError(new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "{0} argument direction is not supported", this.GetArgumentDirection())));
                    }
                    value = this.Editor.ArgumentToExpressionConverter.ConvertBack(value, typeof(Argument), direction, CultureInfo.CurrentCulture);
                }
            }
            this.ReflectedObject.Properties[DesignTimeArgument.ArgumentDefaultValueProperty].SetValue(value);
        }

        internal object GetArgumentValue()
        {
            ModelItem value = this.ReflectedObject.Properties[DesignTimeArgument.ArgumentDefaultValueProperty].Value;
            object result = null;
            if (PropertyKind.Property == this.GetArgumentDirection())
            {
                if (null != value)
                {
                    result = value.GetCurrentValue();
                }
            }
            else
            {
                result = this.Editor.ArgumentToExpressionConverter.Convert(value, typeof(object), null, CultureInfo.CurrentCulture);
                ModelItem expression = result as ModelItem;
                if (null != expression)
                {
                    result = expression.GetCurrentValue();
                }
            }
            return result;
        }

        bool GetIsOutputArgument()
        {
            PropertyKind direction = this.GetArgumentDirection();
            return (direction == PropertyKind.OutArgument || direction == PropertyKind.InOutArgument);
        }

        bool IsRequired(IList attributes)
        {
            if (attributes == null)
            {
                return false;
            }
            foreach (ModelItem item in attributes)
            {
                if (typeof(RequiredArgumentAttribute).IsAssignableFrom(item.ItemType))
                {
                    return true;
                }
            }
            return false;
        }

        bool GetIsRequired()
        {
            ModelItemCollection attributes = this.ReflectedObject.Properties["Attributes"].Collection;
            return IsRequired(attributes);
        }

        void AddIsRequiredAttribute()
        {
            ModelItemCollection attributes = this.ReflectedObject.Properties["Attributes"].Collection;
            using (ModelEditingScope scope = this.ReflectedObject.BeginEdit((string)this.Editor.FindResource("changeArgumentIsRequiredDescription")))
            {
                attributes.Add(new RequiredArgumentAttribute());
                scope.Complete();
            }
        }

        void RemoveIsRequiredAttribute()
        {
            ModelItemCollection attributes = this.ReflectedObject.Properties["Attributes"].Collection;
            using (ModelEditingScope scope = this.ReflectedObject.BeginEdit((string)this.Editor.FindResource("changeArgumentIsRequiredDescription")))
            {
                foreach (ModelItem toRemove in attributes.Where<ModelItem>(p => typeof(RequiredArgumentAttribute).IsAssignableFrom(p.ItemType)))
                {
                    attributes.Remove(toRemove);
                }
                scope.Complete();
            }
        }

        void SetIsRequired(object isRequired)
        {
            bool required = isRequired is ModelItem ? (bool)(((ModelItem)isRequired).GetCurrentValue()) : (bool)isRequired;

            if (required && !this.GetIsRequired())
            {
                this.AddIsRequiredAttribute();
            }
            else if (!required && this.GetIsRequired())
            {
                this.RemoveIsRequiredAttribute();
            }
        }

        internal bool Filter(Type type)
        {
            // We disallow user to pick any Argument<T> type as a property since this is the same as choosing the right direction in the first place.
            return this.GetArgumentDirection() != PropertyKind.Property || !type.IsGenericType || !typeof(Argument).IsAssignableFrom(type);
        }

        internal ModelItem GetOwnerSchemaProperty()
        {
            return this.ReflectedObject.Parent.Parent;
        }

        protected override Type OnGetDynamicPropertyValueEditorType(string propertyName)
        {
            var type = this.GetArgumentType();
            var direction = this.GetArgumentDirection();

            //if argument name is not valid XAML member name, default value editing is disabled.
            //Since it cannot be saved.
            if (!VBIdentifierName.IsValidXamlName(this.GetArgumentName().IdentifierName))
            {
                return typeof(InvalidXamlMemberValueEditor);
            }

            //in case of arguments which contain handles - display HandleValueEditor
            if (typeof(Handle).IsAssignableFrom(type))
            {
                return typeof(HandleValueEditor);
            }


            //check if there are custom editors on the variable's type
            Type argumentType = null;
            switch (direction)
            {
                case PropertyKind.InArgument:
                    argumentType = typeof(InArgument<>).MakeGenericType(type);
                    break;

                case PropertyKind.InOutArgument:
                    argumentType = typeof(InOutArgument<>).MakeGenericType(type);
                    break;

                case PropertyKind.OutArgument:
                    argumentType = typeof(OutArgument<>).MakeGenericType(type);
                    break;

                default:
                    argumentType = type;
                    break;
            }

            var referenceType = typeof(PropertyValueEditor);
            var expressionEditorType = typeof(ExpressionValueEditor);

            //check if there are custom type editors associated with given type - 
            //first look for type editor defined for In/Out/InOut/Argument<T> (if argument is of proper direction)
            //then, look for type editor defined for type itself (i.e. T) - 
            //in search, skip ExpressionValueEditor instance - it will be returned by default for property grid, but for
            //dataGrid nothing should be used - we use default dg template
            var customEditorType = TypeDescriptor
                .GetAttributes(argumentType)
                .OfType<EditorAttribute>()
                .Where(p =>
                    {
                        Type currentType = Type.GetType(p.EditorTypeName);
                        return (expressionEditorType != currentType && referenceType.IsAssignableFrom(currentType));
                    })
                .Select(p => Type.GetType(p.EditorTypeName))
                .FirstOrDefault();

            //if yes - check if there is at least one editor assigner and it derives from PropertyValueEditor 
            if (null != customEditorType)
            {
                return customEditorType;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (((type != stringTypeReference && (converter == null || !converter.CanConvertFrom(stringTypeReference))) && direction == PropertyKind.Property)
                || direction == PropertyKind.OutArgument || direction == PropertyKind.InOutArgument)
            {
                return typeof(ValueNotSupportedEditor);
            }

            //otherwise - return default expression value editor
            return typeof(DefaultValueEditor);
        }

        internal bool ValidateArgumentName(object value, List<string> errors)
        {
            VBIdentifierName identifier = value as VBIdentifierName;
            string name = identifier.IdentifierName;

            if (string.IsNullOrEmpty(name))
            {
                errors.Add(SR.EmptyArgumentName);
            }
            else
            {
                if (!VBIdentifierName.IsValidXamlName(name))
                {
                    errors.Add(string.Format(CultureInfo.CurrentUICulture, SR.InvalidXamlMemberName, name));
                }
                else
                {
                    ModelItemCollection argumentCollection = (ModelItemCollection)this.ReflectedObject.Parent;

                    bool duplicates =
                        argumentCollection.Any<ModelItem>(p => string.Equals(p.Properties["Name"].ComputedValue, name) && !ModelItem.Equals(p, this.ReflectedObject));

                    if (duplicates)
                    {
                        errors.Add(string.Format(CultureInfo.CurrentUICulture, SR.DuplicateArgumentName, name));
                    }
                }
            }
            return 0 == errors.Count;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Exception content is displayed as error message. Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108",
            Justification = "Exception content is displayed as error message. Propagating exceptions might lead to VS crash.")]
        bool ValidateArgumentValue(object value, List<string> errors)
        {
            if (PropertyKind.Property == this.GetArgumentDirection())
            {
                //the value is a string and is empty - assume user wants to clear the property value
                if (value is string && string.IsNullOrEmpty((string)value))
                {
                    return true;
                }
                //validate the value for PropertyType - check if converter usage is required - if value type is the same as argument type - skip conversion
                if (null != value && !this.GetArgumentType().IsAssignableFrom(value.GetType()))
                {
                    try
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(this.GetArgumentType());
                        converter.ConvertFrom(value);
                    }
                    catch (Exception err)
                    {
                        errors.Add(err.Message);
                    }
                }
            }
            return 0 == errors.Count;
        }

        string GetArgumentValueExpressionText()
        {
            string currentExpressionText = null;
            object currentValue = this.GetArgumentValue();
            if (null != currentValue)
            {
                if (this.GetArgumentDirection() == PropertyKind.Property)
                {
                    TypeConverter oldConverter = TypeDescriptor.GetConverter(this.GetArgumentType());

                    if (oldConverter.CanConvertTo(typeof(string)))
                    {
                        currentExpressionText = (string)oldConverter.ConvertTo(currentValue, typeof(string));
                    }
                }
                else
                {
                    ModelItem expression = null;
                    if (this.ReflectedObject.TryGetPropertyValue(out expression, ArgumentDefaultValueProperty, "Expression") && null != expression)
                    {
                        var activity = expression.GetCurrentValue() as ActivityWithResult;
                        if (null != activity)
                        {
                            currentExpressionText = ExpressionHelper.GetExpressionString(activity);
                        }
                    }
                }
            }
            return currentExpressionText;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Conversion of value when type changes might fail - argument will get null value by default. Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108",
            Justification = "Conversion of value when type changes might fail - argument will get null value by default. Propagating exceptions might lead to VS crash.")]
        void TryUpdateArgumentType(Type newType, PropertyKind newDirection)
        {
            if (newDirection == PropertyKind.Property)
            {
                string currentExpressionText = this.GetArgumentValueExpressionText();
                if (null != currentExpressionText)
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(newType);
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        try
                        {
                            object value = converter.ConvertFrom(currentExpressionText);
                            this.ReflectedObject.Properties[ArgumentDefaultValueProperty].SetValue(value);
                        }
                        catch (Exception err)
                        {
                            System.Diagnostics.Debug.WriteLine(err.ToString());
                            this.ReflectedObject.Properties[ArgumentDefaultValueProperty].ClearValue();
                        }
                    }
                    else
                    {
                        this.ReflectedObject.Properties[ArgumentDefaultValueProperty].ClearValue();
                    }
                }
                else
                {
                    this.ReflectedObject.Properties[ArgumentDefaultValueProperty].ClearValue();
                }
            }
            else if (newDirection == PropertyKind.InArgument)
            {
                Argument currentArgument = this.ReflectedObject.Properties[ArgumentDefaultValueProperty].ComputedValue as Argument;
                ActivityWithResult newExpression = null;
                bool succeeded = false;
                if (currentArgument != null)
                {
                    succeeded = ExpressionHelper.TryMorphExpression(currentArgument.Expression, false, newType, this.Context, out newExpression);
                }                                 
                else
                {
                    ////If the old direction is property, we'll try to convert the default value object to the expression object specified by global editor setting
                    string currentExpressionText = this.GetArgumentValueExpressionText();
                    if (!string.IsNullOrEmpty(currentExpressionText))
                    {                        
                        string rootEditorSetting = ExpressionHelper.GetRootEditorSetting(this.ModelTreeManager, WorkflowDesigner.GetTargetFramework(this.Context));
                        if (!string.IsNullOrEmpty(rootEditorSetting))
                        {
                            succeeded = ExpressionTextBox.TryConvertFromString(rootEditorSetting, currentExpressionText, false, newType, out newExpression);
                        }
                    }
                }

                if (succeeded)
                {
                    Argument newArgument = Argument.Create(newType, ArgumentDirection.In);
                    newArgument.Expression = newExpression;
                    this.ReflectedObject.Properties[ArgumentDefaultValueProperty].SetValue(newArgument);
                }
                else
                {
                    //currently if the value cannot be morphed, it's cleared.
                    this.ReflectedObject.Properties[ArgumentDefaultValueProperty].ClearValue();
                }
            }
            else
            {
                this.ReflectedObject.Properties[ArgumentDefaultValueProperty].ClearValue();
            }
        }

        protected override void OnReflectedObjectPropertyChanged(string propertyName)
        {
            if (string.Equals(propertyName, "Type"))
            {
                //type has changed - most likely custom value editors collection would be obsolete                
                this.RaisePropertyChangedEvent(ArgumentTypeProperty);
                this.RaisePropertyChangedEvent(ArgumentDirectionProperty);
                this.RaisePropertyChangedEvent(IsOutputArgument);
                this.RaisePropertyChangedEvent(ArgumentDefaultValueProperty);
            }
            else if (propertyName == ArgumentNameProperty)
            {
                //Change name may need to update the defaul value editor as well, so clean the cache                
                string oldValue = this.identifierName.IdentifierName;
                string newValue = GetArgumentNameString();

                //This is invoked in undo stack
                if (oldValue != newValue)
                {
                    this.identifierName = new VBIdentifierName(true)
                    {
                        IdentifierName = newValue
                    };
                    Editor.ValidateArgumentName(this.identifierName, newValue, oldValue);
                }
            }
            else if (propertyName == ArgumentDefaultValueProperty)
            {
                this.argumentExpressionChanged = true;
            }
            else if (propertyName == Annotation.AnnotationTextPropertyName)
            {
                RaisePropertyChangedEvent(AnnotationTextProperty);
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            if (string.Equals(propertyName, ArgumentNameProperty))
            {
                this.RaisePropertyChangedEvent(AutomationIdProperty);
            }
            else if (string.Equals(propertyName, TimestampProperty))
            {
                if ((!this.argumentExpressionChanged) && (this.Editor != null))
                {
                    this.CustomValueEditors.Clear();
                    this.Editor.UpdateTypeDesigner(this);
                }
                else
                {
                    this.argumentExpressionChanged = false;
                }
            }
            else if (string.Equals(propertyName, ArgumentDirectionProperty) || (string.Equals(propertyName, ArgumentTypeProperty)))
            {
                this.RaisePropertyChangedEvent(ArgumentDefaultValueProperty);
            }
            base.OnPropertyChanged(propertyName);
        }

        internal sealed class DirectionPropertyEditor : PropertyValueEditor
        {
            public DirectionPropertyEditor()
            {
                this.InlineEditorTemplate = EditorResources.GetResources()["DirectionEditor_InlineEditorTemplate"] as DataTemplate;
            }
        }

        internal sealed class DefaultValueEditor : ExpressionValueEditor
        {
            public DefaultValueEditor()
            {
                this.InlineEditorTemplate = EditorResources.GetResources()["inlineExpressionEditorTemplateForDesignTimeArgument"] as DataTemplate;
            }
        }

        internal sealed class IsRequiredPropertyEditor : PropertyValueEditor
        {
            public IsRequiredPropertyEditor()
            {
                this.InlineEditorTemplate = EditorResources.GetResources()["IsRequiredPropertyEditor_InlineEditorTemplate"] as DataTemplate;
            }
        }

        internal sealed class ValueNotSupportedEditor : PropertyValueEditor
        {
            public ValueNotSupportedEditor()
            {
                this.InlineEditorTemplate = EditorResources.GetResources()["inlineExpressionEditorTemplateForDesignTimeArgument_ValueNotSupported"] as DataTemplate;
            }
        }

        internal sealed class InvalidXamlMemberValueEditor : PropertyValueEditor
        {
            public InvalidXamlMemberValueEditor()
            {
                this.InlineEditorTemplate = EditorResources.GetResources()["inlineExpressionEditorTemplateForDesignTimeArgument_InvalidXamlMember"] as DataTemplate;
            }
        }
    }

    sealed class PropertyValueTextBox : TextBox
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += (s, args) =>
                {
                    //get the binding expression, and hook up exception filter
                    var expr = this.GetBindingExpression(PropertyValueTextBox.TextProperty);
                    if (null != expr && null != expr.ParentBinding)
                    {
                        expr.ParentBinding.UpdateSourceExceptionFilter = this.OnUpdateBindingException;
                    }
                };
        }

        object OnUpdateBindingException(object sender, Exception err)
        {
            //if exception occured, the property value is invalid (conversion to target type failed)
            if (err is TargetInvocationException && err.InnerException is ValidationException || err is ValidationException)
            {
                //show error message
                ErrorReporting.ShowErrorMessage((err.InnerException ?? err).Message);
                //and revert textbox to last valid value
                this.GetBindingExpression(PropertyValueTextBox.TextProperty).UpdateTarget();
            }
            return null;
        }
    }

    public enum PropertyKind
    {
        InArgument,
        InOutArgument,
        OutArgument,
        Property
    }
}
