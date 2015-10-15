//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Threading;

    partial class VariableDesigner
    {
        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register(
            "Context",
            typeof(EditingContext),
            typeof(VariableDesigner),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(VariableDesigner.OnContextChanged)));

        static readonly DependencyPropertyKey CurrentVariableScopePropertyKey = DependencyProperty.RegisterReadOnly(
            "CurrentVariableScope",
            typeof(ModelItemCollection),
            typeof(VariableDesigner),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnVariableScopeChanged)));

        public static readonly DependencyProperty CurrentVariableScopeProperty = CurrentVariableScopePropertyKey.DependencyProperty;

        public static readonly RoutedEvent VariableCollectionChangedEvent =
                EventManager.RegisterRoutedEvent("VariableCollectionChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(VariableDesigner));

        const string DefaultVariableName = "variable";

        List<ModelItem> scopesList = new List<ModelItem>();
        ObservableCollection<DesignTimeVariable> variableWrapperCollection = new ObservableCollection<DesignTimeVariable>();
        bool isSelectionSourceInternal = false;
        ModelItem variableToSelect = null;
        bool continueScopeEdit = false;
        ModelItem lastSelection;

        DataGridHelper dgHelper;

        public VariableDesigner()
        {
            InitializeComponent();

            this.dgHelper = new DataGridHelper(this.variableDataGrid, this);
            this.dgHelper.Context = this.Context;
            this.dgHelper.AddNewRowCommand = DesignerView.CreateVariableCommand;
            this.dgHelper.AddNewRowContent = (string)this.FindResource("addVariableNewRowLabel");
            this.dgHelper.ResolveDynamicTemplateCallback = this.OnResolveDynamicContentTemplate;
            this.dgHelper.LoadDynamicContentDataCallback = this.OnShowExtendedValueEditor;
            this.dgHelper.LoadCustomPropertyValueEditorCallback = this.OnLoadExtendedValueEditor;
            this.dgHelper.ShowValidationErrorAsToolTip = false;

            this.variableDataGrid.SelectionChanged += OnDataGridRowSelected;
            this.variableWrapperCollection.CollectionChanged += OnVariableWrapperCollectionChanged;
            this.variableDataGrid.ItemsSource = this.variableWrapperCollection;

            var converter = (BreadCrumbTextConverter)this.FindResource("scopeToNameConverter");
            converter.PixelsPerChar = (this.FontSize - 5);
        }

        public event RoutedEventHandler VariableCollectionChanged
        {
            add
            {
                AddHandler(VariableCollectionChangedEvent, value);
            }
            remove
            {
                RemoveHandler(VariableCollectionChangedEvent, value);
            }
        }

        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public ModelItemCollection CurrentVariableScope
        {
            get { return (ModelItemCollection)GetValue(CurrentVariableScopeProperty); }
            private set { SetValue(CurrentVariableScopePropertyKey, value); }
        }

        public List<ModelItem> ScopesList
        {
            get { return this.scopesList; }
        }

        public bool CreateNewVariableWrapper()
        {
            bool result = false;
            ModelItemCollection scope = this.GetDefaultVariableScope();
            if (null != scope)
            {
                DesignTimeVariable wrapperObject = null;
                Variable variable = Variable.Create(this.GetDefaultName(), this.GetDefaultType(), VariableModifiers.None);
                using (ModelEditingScope change = scope.BeginEdit((string)this.FindResource("addNewVariableDescription")))
                {
                    ModelItem wrappedVariable = scope.Add(variable);
                    wrapperObject = new DesignTimeVariable(wrappedVariable, this);
                    this.variableWrapperCollection.Add(wrapperObject);
                    change.Complete();
                    result = true;
                }
                this.dgHelper.BeginRowEdit(wrapperObject);
            }
            return result;
        }

        internal void ChangeVariableType(DesignTimeVariable oldVariableWrapper, Variable newVariable)
        {
            //check who is the sender of the event - data grid or property inspector
            int index = this.variableDataGrid.Items.IndexOf(this.variableDataGrid.SelectedItem);
            DataGridCell cell = DataGridHelper.GetCell(this.variableDataGrid, index, 1);
            bool shouldReselct = cell.IsEditing;

            //get location of the variable
            ModelItemCollection container = VariableHelper.GetVariableCollection(oldVariableWrapper.ReflectedObject.Parent.Parent);
            index = container.IndexOf(oldVariableWrapper.ReflectedObject);
            //remove all value 
            container.RemoveAt(index);
            oldVariableWrapper.Dispose();
            oldVariableWrapper.Initialize(container.Insert(index, newVariable));
        }

        internal void NotifyVariableScopeChanged(DesignTimeVariable variable)
        {
            this.variableToSelect = null != variable ? variable.ReflectedObject : null;
            int index = this.variableDataGrid.Items.IndexOf(this.variableDataGrid.SelectedItem);
            DataGridCell cell = DataGridHelper.GetCell(this.variableDataGrid, index, 2);
            this.continueScopeEdit = cell.IsEditing;
        }

        //Check case-insensitive duplicates, which are not allowed in VB expressions 
        internal void CheckCaseInsensitiveDuplicates(VBIdentifierName identifierName, string newName)
        {
            Func<DesignTimeVariable, bool> checkForDuplicates = new Func<DesignTimeVariable, bool>(p => string.Equals((string)p.ReflectedObject.Properties["Name"].ComputedValue, newName, StringComparison.OrdinalIgnoreCase) && !object.Equals(p.GetVariableName(), identifierName));
            DesignTimeVariable duplicate = this.variableWrapperCollection.FirstOrDefault<DesignTimeVariable>(checkForDuplicates);
            if (duplicate != null)
            {
                identifierName.IsValid = false;
                identifierName.ErrorMessage = string.Format(CultureInfo.CurrentUICulture, SR.DuplicateIdentifier, newName);
                VBIdentifierName duplicateIdentifier = duplicate.GetVariableName();
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
            Func<DesignTimeVariable, bool> checkForOldNameDuplicates = new Func<DesignTimeVariable, bool>(p => string.Equals((string)p.ReflectedObject.Properties["Name"].ComputedValue, oldName, StringComparison.OrdinalIgnoreCase) && !object.Equals(p.GetVariableName(), identifier));
            IEnumerable<DesignTimeVariable> oldDuplicates = this.variableWrapperCollection.Where<DesignTimeVariable>(checkForOldNameDuplicates);
            if (oldDuplicates.Count<DesignTimeVariable>() == 1)
            {
                DesignTimeVariable wrapper = oldDuplicates.First<DesignTimeVariable>();
                VBIdentifierName oldDuplicate = wrapper.GetVariableName();
                oldDuplicate.IsValid = true;
                oldDuplicate.ErrorMessage = string.Empty;
            }
        }

        internal void NotifyVariableNameChanged(VBIdentifierName identifierName, string newName, string oldName)
        {
            //Check whether there're any variables' name conflict with the old name which can be cleaned up now
            this.ClearCaseInsensitiveDuplicates(identifierName, oldName);

            //Check whether there're any duplicates with new name    
            this.CheckCaseInsensitiveDuplicates(identifierName, newName);
        }

        internal void UpdateTypeDesigner(DesignTimeVariable variable)
        {
            this.dgHelper.UpdateDynamicContentColumns(variable);
        }

        string GetDefaultName()
        {
            return this.variableWrapperCollection.GetUniqueName<DesignTimeVariable>(VariableDesigner.DefaultVariableName, wrapper => wrapper.GetVariableName().IdentifierName);
        }

        Type GetDefaultType()
        {
            return typeof(string);
        }

        ModelItemCollection GetDefaultVariableScope()
        {
            //do a lazdy scope refresh
            //if we have a valid variable scope
            if (null != this.CurrentVariableScope)
            {
                //get the tree manager
                var treeManager = this.Context.Services.GetService<ModelTreeManager>();
                if (null != treeManager)
                {
                    //get the model tree root 
                    var root = treeManager.Root;
                    //get the first scope, which is attached to the model tree (in case of undo/redo operations, even though VariableScope might be
                    //valid, it actually isn't attached to model tree, so using it as a variable scope doesn't make any sense)
                    var validScope = this.scopesList.FirstOrDefault(p => ModelItem.Equals(p, root) || root.IsParentOf(p));
                    //check if valid scope is different that current variable scope. most likely - due to undo/redo operation which removed an activity
                    if (!ModelItem.Equals(validScope, this.CurrentVariableScope.Parent))
                    {
                        //it is different - update the current variable scope (this setter will unhook old event handlers, clean the scopesList collection and hook 
                        //for new event notifications
                        this.CurrentVariableScope = validScope.GetVariableCollection();
                    }
                }
            }
            //return validated variable scope
            return this.CurrentVariableScope;
        }

        void Populate(ModelItem workflowElement)
        {
            this.scopesList.ForEach(p =>
            {
                p.Properties["Variables"].Collection.CollectionChanged -= OnVariableCollectionChanged;
            });

            this.scopesList.Clear();
            this.variableDataGrid.ItemsSource = null;
            this.variableWrapperCollection.All(p => { p.Dispose(); return true; });
            this.variableWrapperCollection.Clear();
            var allVariables = VariableHelper.FindDeclaredVariables(workflowElement, this.scopesList);
            //fill variable wrapper collection only if designer is visible
            if (workflowElement != null && this.IsVisible)
            {
                allVariables.ForEach(variable =>
                    {
                        this.variableWrapperCollection.Add(new DesignTimeVariable(variable, this));
                    });
            }
            this.variableDataGrid.ItemsSource = this.variableWrapperCollection;

            this.scopesList.ForEach(p =>
            {
                p.Properties["Variables"].Collection.CollectionChanged += OnVariableCollectionChanged;
            });
        }

        void StoreLastSelection()
        {
            if (!this.isSelectionSourceInternal)
            {
                ModelItem current = this.Context.Items.GetValue<Selection>().PrimarySelection;
                if (null == current || !typeof(DesignTimeVariable).IsAssignableFrom(current.ItemType))
                {
                    this.lastSelection = current;
                }
            }
        }

        internal void SelectVariable(ModelItem variable)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (object item in this.variableDataGrid.Items)
                {
                    if (item is DesignTimeVariable)
                    {
                        if (object.ReferenceEquals(((DesignTimeVariable)item).ReflectedObject, variable))
                        {
                            this.variableDataGrid.SelectedItem = item;
                            this.variableDataGrid.ScrollIntoView(item, null);
                        }
                    }
                }
            }), DispatcherPriority.ApplicationIdle);

        }

        void OnDataGridRowSelected(object sender, RoutedEventArgs e)
        {
            if (null != this.Context && !this.isSelectionSourceInternal)
            {
                this.isSelectionSourceInternal = true;
                if (null != this.variableDataGrid.SelectedItem && this.variableDataGrid.SelectedItem is DesignTimeVariable)
                {
                    DesignTimeVariable variable = (DesignTimeVariable)this.variableDataGrid.SelectedItem;
                    this.Context.Items.SetValue(new Selection(variable.Content));
                }
                else
                {
                    // clear variables in selection
                    Selection oldSelection = this.Context.Items.GetValue<Selection>();
                    List<ModelItem> newSelection = new List<ModelItem>();
                    if (oldSelection != null && oldSelection.SelectionCount > 0)
                    {
                        foreach (ModelItem item in oldSelection.SelectedObjects)
                        {
                            if (item.ItemType != typeof(DesignTimeVariable))
                            {
                                newSelection.Add(item);
                            }
                        }
                    }
                    this.Context.Items.SetValue(new Selection(newSelection));
                }
                this.isSelectionSourceInternal = false;
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

        void OnItemSelected(Selection newSelection)
        {
            //check if selection update source is internal - it is internal if someone clicks on row in variable designer. in such case, do not update selection
            if (!this.isSelectionSourceInternal)
            {
                //whenever selection changes:
                //1) check if selection is a result of undo/redo operation - if it is, we might be in the middle of collection changed 
                //   notification, so i have to postpone variable scope update untill collection update is completed.
                if (this.Context.Services.GetService<UndoEngine>().IsUndoRedoInProgress)
                {
                    //delegate call to update selection after update is completed
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //get current selection - i can't use newSelection passed into the method, because undo/redo may alter that value
                        var current = this.Context.Items.GetValue<Selection>().PrimarySelection;
                        //do the actual selection update
                        this.OnItemSelectedCore(current);
                    }), DispatcherPriority.ApplicationIdle);
                }
                else
                {
                    //store element selected before variable designer started updating selection - when designer is closed, we try to restore that selection
                    this.StoreLastSelection();
                    this.OnItemSelectedCore(newSelection.PrimarySelection);
                }
            }
        }

        void OnItemSelectedCore(ModelItem primarySelection)
        {
            //update variable scope - but ignore selection changes made by selecting variables.
            if (null == primarySelection || !primarySelection.IsAssignableFrom<DesignTimeVariable>())
            {
                ModelItem element = VariableHelper.GetVariableScopeElement(primarySelection);
                ModelItemCollection newVariableScope = VariableHelper.GetVariableCollection(element);
                if (this.CurrentVariableScope != newVariableScope)
                {
                    this.CurrentVariableScope = newVariableScope;
                }
                else
                {
                    bool selectedVariableIsInSelection = false;
                    Selection selection = this.Context.Items.GetValue<Selection>();

                    DesignTimeVariable selectedVariable = this.variableDataGrid.SelectedItem as DesignTimeVariable;
                    if (selectedVariable != null)
                    {
                        foreach (ModelItem item in selection.SelectedObjects)
                        {
                            if (object.ReferenceEquals(selectedVariable, item.GetCurrentValue()))
                            {
                                selectedVariableIsInSelection = true;
                            }
                        }
                    }

                    if (!selectedVariableIsInSelection)
                    {
                        this.variableDataGrid.SelectedItem = null;
                    }
                }
            }
        }

        void OnVariableScopeChanged()
        {
            this.Populate(null != this.CurrentVariableScope ? this.CurrentVariableScope.Parent : null);
        }

        void OnVariableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var isUndoRedoInProgress = this.Context.Services.GetService<UndoEngine>().IsUndoRedoInProgress;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ModelItem variable in e.NewItems)
                    {
                        DesignTimeVariable wrapper = this.variableWrapperCollection
                            .FirstOrDefault<DesignTimeVariable>(p => (ModelItem.Equals(p.ReflectedObject, variable)));

                        if (wrapper == null)
                        {
                            wrapper = new DesignTimeVariable(variable, this);
                            this.variableWrapperCollection.Add(wrapper);
                        }
                        if (null != this.variableToSelect && this.variableToSelect == variable)
                        {
                            this.variableDataGrid.SelectedItem = wrapper;
                            this.variableToSelect = null;

                            int index = this.variableDataGrid.Items.IndexOf(this.variableDataGrid.SelectedItem);
                            DataGridRow row = DataGridHelper.GetRow(this.variableDataGrid, index);
                            if (!row.IsSelected)
                            {
                                row.IsSelected = true;
                            }

                            if (this.continueScopeEdit)
                            {
                                DataGridCell cell = DataGridHelper.GetCell(this.variableDataGrid, index, 2);
                                cell.IsEditing = true;
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ModelItem variable in e.OldItems)
                    {
                        DesignTimeVariable wrapper = this.variableWrapperCollection.FirstOrDefault(p => ModelItem.Equals(p.ReflectedObject, variable));
                        if (null != wrapper)
                        {
                            //in case of undo/redo operation - just remove old reference to the wrapper, new one will be added be undo stack anyway
                            if (!this.ScopesList.Contains((sender as ModelItem).Parent) || isUndoRedoInProgress)
                            {
                                this.variableWrapperCollection.Remove(wrapper);
                            }
                        }
                    }
                    break;
            }
            this.RaiseEvent(new RoutedEventArgs(VariableDesigner.VariableCollectionChangedEvent, this));
        }

        void OnVariableWrapperCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    bool isUndoRedoInProgress = this.Context.Services.GetService<UndoEngine>().IsUndoRedoInProgress;
                    foreach (DesignTimeVariable arg in e.OldItems)
                    {
                        if (!isUndoRedoInProgress)
                        {
                            this.ClearCaseInsensitiveDuplicates(arg.GetVariableName(), (string)arg.ReflectedObject.Properties["Name"].ComputedValue);
                            ModelItemCollection collection = (ModelItemCollection)arg.ReflectedObject.Parent;
                            collection.Remove(arg.ReflectedObject);
                        }
                        arg.Dispose();
                    }
                    break;
                case NotifyCollectionChangedAction.Add:
                    foreach (DesignTimeVariable var in e.NewItems)
                    {
                        this.CheckCaseInsensitiveDuplicates(var.GetVariableName(), (string)var.ReflectedObject.Properties["Name"].ComputedValue);
                    }
                    break;
            }
        }

        void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (bool.Equals(true, e.NewValue))
            {
                this.StoreLastSelection();
                this.OnVariableScopeChanged();
            }
            else if (this.variableDataGrid.SelectedItem is DesignTimeVariable)
            {
                Selection newSelection = null == this.lastSelection ? new Selection() : new Selection(this.lastSelection);
                this.isSelectionSourceInternal = true;
                this.Context.Items.SetValue(newSelection);
                this.variableDataGrid.SelectedItem = null;
                this.isSelectionSourceInternal = false;
            }
        }

        bool OnResolveDynamicContentTemplate(ResolveTemplateParams resolveParams)
        {
            var variable = (DesignObjectWrapper)resolveParams.Cell.DataContext;

            //get editor associated with variable's value
            var editorType = variable.GetDynamicPropertyValueEditorType(DesignTimeVariable.VariableDefaultProperty);

            //if yes there is a custom one - use it
            if (!typeof(ExpressionValueEditor).IsAssignableFrom(editorType))
            {
                //get inline editor template - it will be used for both templates - view and editing; 
                resolveParams.Template = variable.GetDynamicPropertyValueEditor(DesignTimeVariable.VariableDefaultProperty).InlineEditorTemplate;
                resolveParams.IsDefaultTemplate = false;
            }
            else
            {
                //no custom editor - depending on grid state display either editable or readonly expression template
                string key = resolveParams.Cell.IsEditing ? "variableExpressionEditableTemplate" : "variableExpressionReadonlyTemplate";
                resolveParams.Template = (DataTemplate)this.FindResource(key);
                resolveParams.IsDefaultTemplate = true;
            }
            return true;
        }

        DialogPropertyValueEditor OnLoadExtendedValueEditor(DataGridCell cell, object instance)
        {
            var variable = (DesignObjectWrapper)cell.DataContext;
            return variable.GetDynamicPropertyValueEditor(DesignTimeVariable.VariableDefaultProperty) as DialogPropertyValueEditor;
        }

        ModelProperty OnShowExtendedValueEditor(DataGridCell cell, object instance)
        {
            var variable = (DesignObjectWrapper)cell.DataContext;
            return variable.Content.Properties[DesignTimeVariable.VariableDefaultProperty];
        }

        static void OnContextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((VariableDesigner)sender).OnContextChanged();
        }

        static void OnVariableScopeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            VariableDesigner control = (VariableDesigner)sender;
            if (!object.Equals(e.OldValue, e.NewValue))
            {
                control.OnVariableScopeChanged();
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

        // This is to workaround a 
        internal void NotifyAnnotationTextChanged()
        {
            foreach (object item in this.variableDataGrid.Items)
            {
                DesignTimeVariable designTimeVariable = item as DesignTimeVariable;
                if (designTimeVariable != null)
                {
                    designTimeVariable.NotifyPropertyChanged(DesignTimeVariable.AnnotationTextProperty);
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
            ContextMenuUtilities.OnDeleteCommandCanExecute(e, this.variableDataGrid);
        }

        private void OnDeleteCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.variableDataGrid != null && this.variableDataGrid.SelectedItems != null && this.variableDataGrid.SelectedItems.Count > 0)
            {
                List<ModelItem> list = new List<ModelItem>();
                foreach (object item in this.variableDataGrid.SelectedItems)
                {
                    DesignTimeVariable designTimeVariable = item as DesignTimeVariable;
                    if (designTimeVariable != null)
                    {
                        list.Add(designTimeVariable.ReflectedObject);
                    }
                }

                foreach (ModelItem modelItem in list)
                {
                    foreach (DesignTimeVariable designTimeVariable in this.variableWrapperCollection)
                    {
                        if (designTimeVariable.ReflectedObject == modelItem)
                        {
                            this.variableWrapperCollection.Remove(designTimeVariable);
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
            ContextMenuUtilities.OnAddAnnotationCommandCanExecute(e, this.Context, this.variableDataGrid);
        }

        private void OnAddAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.variableDataGrid != null && this.variableDataGrid.SelectedItems != null && this.variableDataGrid.SelectedItems.Count == 1)
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

                    DesignTimeVariable variable = (DesignTimeVariable)this.variableDataGrid.SelectedItems[0];
                    variable.Content.Properties[DesignTimeVariable.AnnotationTextProperty].SetValue(annotationText);
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
            ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context, this.variableDataGrid);
        }

        private void OnEditAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.variableDataGrid != null && this.variableDataGrid.SelectedItems != null && this.variableDataGrid.SelectedItems.Count == 1)
            {
                DesignTimeVariable variable = (DesignTimeVariable)this.variableDataGrid.SelectedItems[0];

                AnnotationDialog dialog = new AnnotationDialog();
                dialog.Context = Context;
                dialog.Title = SR.EditAnnotationTitle;
                dialog.AnnotationText = variable.Content.Properties[DesignTimeVariable.AnnotationTextProperty].ComputedValue as string;

                WindowHelperService service = this.Context.Services.GetService<WindowHelperService>();
                if (null != service)
                {
                    service.TrySetWindowOwner(this, dialog);
                }
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                if (dialog.ShowDialog() == true)
                {
                    string annotationText = dialog.AnnotationText;

                    variable.Content.Properties[DesignTimeVariable.AnnotationTextProperty].SetValue(annotationText);
                }
            }
        }

        private void OnDeleteAnnotationMenuLoaded(object sender, RoutedEventArgs e)
        {
            ContextMenuUtilities.OnAnnotationMenuLoaded(this.Context, (Control)sender, e);
        }

        private void OnDeleteAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context, this.variableDataGrid);
        }

        private void OnDeleteAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.variableDataGrid != null && this.variableDataGrid.SelectedItems != null && this.variableDataGrid.SelectedItems.Count == 1)
            {
                DesignTimeVariable variable = (DesignTimeVariable)this.variableDataGrid.SelectedItems[0];
                variable.Content.Properties[DesignTimeVariable.AnnotationTextProperty].ClearValue();
            }

            e.Handled = true;
        }
    }

    internal static class VariableHelper
    {
        static Type VariablesCollectionType = typeof(Collection<Variable>);
        static Type CodeActivityType = typeof(CodeActivity);
        static Type GenericCodeActivityType = typeof(CodeActivity<>);
        static Type AsyncCodeActivityType = typeof(AsyncCodeActivity);
        static Type GenericAsyncCodeActivityType = typeof(AsyncCodeActivity<>);

        internal static ModelItemCollection GetVariableCollection(this ModelItem element)
        {
            if (null != element)
            {
                Type elementType = element.ItemType;
                if (!((CodeActivityType.IsAssignableFrom(elementType)) || (GenericAsyncCodeActivityType.IsAssignableFrom(elementType)) ||
                    (AsyncCodeActivityType.IsAssignableFrom(elementType)) || (GenericAsyncCodeActivityType.IsAssignableFrom(elementType))))
                {
                    ModelProperty variablesProperty = element.Properties["Variables"];
                    if ((variablesProperty != null) && (variablesProperty.PropertyType == VariablesCollectionType))
                    {
                        return variablesProperty.Collection;
                    }
                }
            }
            return null;
        }

        internal static ModelItem GetVariableScopeElement(this ModelItem element)
        {
            while (null != element && null == VariableHelper.GetVariableCollection(element))
            {
                element = element.Parent;
            }
            return element;
        }

        internal static List<ModelItem> FindDeclaredVariables(this ModelItem element, IList<ModelItem> scopeList)
        {
            var variables = VariableHelper.FindVariablesInScope(element, scopeList);
            var contained = VariableHelper.GetVariableCollection(element);
            if (null != contained)
            {
                if (null != scopeList)
                {
                    scopeList.Insert(0, element);
                }
                variables.InsertRange(0, contained.AsEnumerable());
            }
            return variables;
        }

        internal static List<ModelItem> FindDeclaredVariables(this ModelItem element)
        {
            return VariableHelper.FindDeclaredVariables(element, null);
        }

        internal static List<ModelItem> FindActivityDelegateArgumentsInScope(this ModelItem workflowElement)
        {
            List<ModelItem> variables = new List<ModelItem>();
            if (workflowElement != null)
            {
                workflowElement = workflowElement.Parent;
                while (null != workflowElement)
                {
                    variables.AddRange(workflowElement.FindActivityDelegateArguments());
                    workflowElement = workflowElement.Parent;
                }
            }
            return variables;
        }

        internal static List<ModelItem> FindActivityDelegateArguments(this ModelItem element)
        {
            List<ModelItem> delegateArguments = new List<ModelItem>();
            if (element.GetCurrentValue() is ActivityDelegate)
            {
                //browse all properties in given ActivityDelegate
                delegateArguments.AddRange(element.Properties
                    //choose only those of base type equal to DelegateArgument
                    .Where<ModelProperty>(p => (typeof(DelegateArgument).IsAssignableFrom(p.PropertyType) && null != p.Value))
                    //from those, take actual ModelItem value
                    .Select<ModelProperty, ModelItem>(p => p.Value));
            }

            return delegateArguments;
        }

        internal static List<ModelItem> FindVariablesInScope(this ModelItem element, IList<ModelItem> scopeList)
        {
            List<ModelItem> variables = new List<ModelItem>();
            if (null != scopeList)
            {
                scopeList.Clear();
            }
            if (null != element)
            {
                element = element.Parent;
                while (element != null)
                {
                    ModelItemCollection variablesInElement = VariableHelper.GetVariableCollection(element);
                    if (null != variablesInElement)
                    {
                        if (null != scopeList)
                        {
                            scopeList.Add(element);
                        }
                        variables.AddRange(variablesInElement.AsEnumerable());
                    }
                    element = element.Parent;
                }
            }
            return variables;
        }

        internal static List<ModelItem> FindUniqueVariablesInScope(ModelItem element)
        {
            Dictionary<string, ModelItem> variables = new Dictionary<string, ModelItem>();
            while (element != null)
            {
                ModelItemCollection variablesInElement = VariableHelper.GetVariableCollection(element);
                if (null != variablesInElement)
                {
                    foreach (ModelItem modelVariable in variablesInElement)
                    {
                        LocationReference locationReference = modelVariable.GetCurrentValue() as LocationReference;
                        if (locationReference != null && !string.IsNullOrWhiteSpace(locationReference.Name) && !variables.ContainsKey(locationReference.Name))
                        {
                            variables.Add(locationReference.Name, modelVariable);
                        }
                    }
                }
                element = element.Parent;
            }
            return new List<ModelItem>(variables.Values);
        }


        internal static List<ModelItem> FindVariablesInScope(ModelItem element)
        {
            return VariableHelper.FindVariablesInScope(element, null);
        }

        internal static bool ContainsVariable(this ModelItemCollection variableContainer, string variableName)
        {
            if (null == variableContainer)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("variableContainer"));
            }
            if (!variableContainer.ItemType.IsGenericType ||
                variableContainer.ItemType.GetGenericArguments().Length != 1 ||
                !typeof(Variable).IsAssignableFrom(variableContainer.ItemType.GetGenericArguments()[0]))
            {
                throw FxTrace.Exception.AsError(new ArgumentException("non variable collection"));
            }

            return variableContainer.Any(p => string.Equals(p.Properties[DesignTimeVariable.VariableNameProperty].ComputedValue, variableName));
        }

        internal static string CreateUniqueVariableName(this ModelItemCollection variableContainer, string namePrefix, int countStartValue)
        {
            if (null == variableContainer)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("variableContainer"));
            }
            if (!variableContainer.ItemType.IsGenericType ||
                variableContainer.ItemType.GetGenericArguments().Length != 1 ||
                !typeof(Variable).IsAssignableFrom(variableContainer.ItemType.GetGenericArguments()[0]))
            {
                throw FxTrace.Exception.AsError(new ArgumentException("non variable collection"));
            }

            string name = string.Empty;

            //in order to generate unique variable name, browse all scopes from current to the root - variable name should be unique in whole tree up to 
            //the root. we don't check unique check below current scope - it would be too expensive to ---- whole tree
            var variables = VariableHelper.FindVariablesInScope(variableContainer);
            while (true)
            {
                name = string.Format(CultureInfo.CurrentUICulture, "{0}{1}", namePrefix, countStartValue++);
                if (!variables.Any(p => string.Equals(p.Properties[DesignTimeVariable.VariableNameProperty].ComputedValue, name)))
                {
                    break;
                }
            }

            return name;
        }

        internal static ModelItem FindRootVariableScope(ModelItem element)
        {
            if (null == element)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            ModelItem result = element.GetParentEnumerator().Where(p => null != VariableHelper.GetVariableCollection(p)).LastOrDefault();
            return result;
        }

        internal static ModelItem FindCommonVariableScope(ModelItem scope1, ModelItem scope2)
        {
            if (null == scope1 || null == scope2)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException(null == scope1 ? "scope1" : "scope2"));
            }

            var scope1List = scope1.GetParentEnumerator().Where(p => null != VariableHelper.GetVariableCollection(p)).ToList();
            var scope2List = scope2.GetParentEnumerator().Where(p => null != VariableHelper.GetVariableCollection(p)).ToList();

            if (null != VariableHelper.GetVariableCollection(scope1))
            {
                scope1List.Insert(0, scope1);
            }
            if (null != VariableHelper.GetVariableCollection(scope2))
            {
                scope2List.Insert(0, scope2);
            }

            if (scope1 == scope2)
            {
                return scope1List.FirstOrDefault();
            }

            return scope1List.Intersect(scope2List).FirstOrDefault();
        }
    }

    sealed class DesignTimeVariable : DesignObjectWrapper
    {
        internal static readonly string VariableNameProperty = "Name";
        internal static readonly string VariableTypeProperty = "Type";
        internal static readonly string VariableScopeProperty = "Scope";
        internal static readonly string VariableDefaultProperty = "Default";
        internal static readonly string ToolTipProperty = "ToolTip";
        internal static readonly string VariableScopeLevelProperty = "ScopeLevel";
        internal static readonly string VariableModifiersProperty = "Modifiers";
        internal static readonly string AnnotationTextProperty = "DesignTimeVariableAnnotationText";
        static readonly string[] Properties =
            new string[] { VariableNameProperty, VariableTypeProperty, VariableScopeProperty, VariableDefaultProperty, 
                           ToolTipProperty, VariableScopeLevelProperty, VariableModifiersProperty };

        bool variableTypeChanged = false;

        internal VariableDesigner Editor
        {
            get;
            private set;
        }

        VBIdentifierName identifierName;

        #region Initialize type properties code
        public static PropertyDescriptorData[] InitializeTypeProperties()
        {
            return new PropertyDescriptorData[]
            {
                new PropertyDescriptorData()
                {
                    PropertyName = VariableNameProperty,
                    PropertyType = typeof(VBIdentifierName),
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(VBIdentifierName)).OfType<Attribute>().ToArray(),
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeVariable)instance).SetVariableName((VBIdentifierName)newValue);
                        },
                    PropertyGetter = (instance) => (((DesignTimeVariable)instance).GetVariableName()),
                    PropertyValidator  = (instance, value, errors) => (((DesignTimeVariable)instance).ValidateVariableName(value, errors))
                },
                new PropertyDescriptorData()
                {
                    PropertyName = VariableTypeProperty,
                    PropertyType = typeof(Type),
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(Type)).OfType<Attribute>().ToArray(),
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeVariable)instance).SetVariableType((Type)newValue);
                        },
                    PropertyGetter = (instance) => (((DesignTimeVariable)instance).GetVariableType()),
                    PropertyValidator = null
                },
                new PropertyDescriptorData()
                {
                    PropertyName = VariableScopeProperty,
                    PropertyType = typeof(ModelItem),
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(ModelItem)).OfType<Attribute>().Union(new Attribute[] { new EditorAttribute(typeof(ScopeValueEditor), typeof(PropertyValueEditor)) }).ToArray(),
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeVariable)instance).SetVariableScope(newValue);
                        },
                    PropertyGetter = (instance) => (((DesignTimeVariable)instance).GetVariableScope()),
                    PropertyValidator  = (instance, value, errors) => (((DesignTimeVariable)instance).ValidateVariableScope(value, errors))
                },
                new PropertyDescriptorData()
                {
                    PropertyName = VariableDefaultProperty,
                    PropertyType = typeof(Activity),
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(Activity)).OfType<Attribute>().Union(new Attribute[] { new EditorAttribute(typeof(DesignObjectWrapperDynamicPropertyEditor), typeof(DialogPropertyValueEditor)), new EditorReuseAttribute(false) }).ToArray(),
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeVariable)instance).SetVariableValue(newValue);
                        },
                    PropertyGetter = (instance) => (((DesignTimeVariable)instance).GetVariableValue()),
                    PropertyValidator  = null,
                },
                new PropertyDescriptorData()
                {
                    PropertyName = ToolTipProperty,
                    PropertyType = typeof(string),
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertySetter = null,
                    PropertyGetter = (instance) => (((DesignTimeVariable)instance).GetToolTip()),
                    PropertyValidator = null
                },
                new PropertyDescriptorData()
                {
                    PropertyName = VariableScopeLevelProperty,
                    PropertyType = typeof(int),
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertySetter = null,
                    PropertyValidator = null,
                    PropertyGetter = (instance) =>
                        (
                            ((DesignTimeVariable)instance).GetScopeLevel()
                        ),
                },
                new PropertyDescriptorData()
                {
                    PropertyName = VariableModifiersProperty,
                    PropertyType = typeof(VariableModifiers), 
                    PropertyAttributes = TypeDescriptor.GetAttributes(typeof(VariableModifiers)).OfType<Attribute>().ToArray(), 
                    PropertySetter = (instance, newValue) => 
                        {
                            ((DesignTimeVariable)instance).SetVariableModifiers(newValue);
                        },
                    PropertyValidator = null,
                    PropertyGetter = (instance) => 
                        {
                            return ((DesignTimeVariable)instance).GetVariableModifiers();
                        }
                },
                new PropertyDescriptorData()
                {
                    PropertyName = AnnotationTextProperty,
                    PropertyType = typeof(string),
                    PropertyAttributes = new Attribute[] { BrowsableAttribute.No },
                    PropertySetter = (instance, newValue) =>
                        {
                            ((DesignTimeVariable)instance).SetAnnotationText(newValue);
                        },
                    PropertyValidator = null,
                    PropertyGetter = (instance) =>
                        {
                            return ((DesignTimeVariable)instance).GetAnnotationText();
                        }
                }
            };
        }
        #endregion

        public DesignTimeVariable()
        {
            throw FxTrace.Exception.AsError(new NotSupportedException(SR.InvalidConstructorCall));
        }

        internal DesignTimeVariable(ModelItem modelItem, VariableDesigner editor)
            : base(modelItem)
        {
            this.Editor = editor;
            this.identifierName = new VBIdentifierName
            {
                IdentifierName = (string)modelItem.Properties[VariableNameProperty].ComputedValue
            };
        }

        protected override string AutomationId
        {
            get { return this.GetVariableNameString(); }
        }

        void SetVariableName(VBIdentifierName identifierName)
        {
            using (ModelEditingScope change = this.ReflectedObject.BeginEdit((string)this.Editor.FindResource("changeVariableNameDescription")))
            {
                this.identifierName = identifierName;
                string name = this.identifierName.IdentifierName;
                this.Editor.NotifyVariableNameChanged(this.identifierName, name, (string)this.ReflectedObject.Properties[VariableNameProperty].ComputedValue);
                this.ReflectedObject.Properties[VariableNameProperty].SetValue(name);

                change.Complete();
            }
        }

        internal VBIdentifierName GetVariableName()
        {
            return this.identifierName;
        }

        string GetVariableNameString()
        {
            return (string)this.ReflectedObject.Properties[VariableNameProperty].ComputedValue;
        }

        protected override void OnReflectedObjectPropertyChanged(string propertyName)
        {
            if (propertyName == Annotation.AnnotationTextPropertyName)
            {
                RaisePropertyChangedEvent(AnnotationTextProperty);
            }

            if (propertyName == VariableNameProperty)
            {
                string oldValue = this.identifierName.IdentifierName;
                string newValue = GetVariableNameString();

                //This is invoked in undo stack
                if (oldValue != newValue)
                {
                    this.identifierName = new VBIdentifierName
                    {
                        IdentifierName = newValue
                    };
                    Editor.NotifyVariableNameChanged(this.identifierName, newValue, oldValue);
                }
            }
        }

        void SetVariableModifiers(object modifiers)
        {
            this.ReflectedObject.Properties[VariableModifiersProperty].SetValue(
                modifiers is ModelItem ? (modifiers as ModelItem).GetCurrentValue() : modifiers);
        }

        object GetVariableModifiers()
        {
            return this.ReflectedObject.Properties[VariableModifiersProperty].ComputedValue;
        }

        int GetScopeLevel()
        {
            int level = 0;
            ModelItem parent = this.ReflectedObject.Parent;
            while (null != parent)
            {
                ++level;
                parent = parent.Parent;
            }
            return level;
        }

        // Used by screen reader to read the DataGrid row.
        public override string ToString()
        {
            string name = this.GetVariableNameString();
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            return "Variable";
        }

        void SetVariableType(Type type)
        {
            if (!Type.Equals(type, this.GetVariableType()))
            {
                using (ModelEditingScope change = this.ReflectedObject.BeginEdit((string)this.Editor.FindResource("changeVariableTypeDescription")))
                {
                    this.variableTypeChanged = true;
                    ModelItemCollection variableContainer = (ModelItemCollection)this.ReflectedObject.Parent;
                    //proceed only if variable is associated with container
                    if (null != variableContainer)
                    {
                        Variable variable = Variable.Create(this.GetVariableNameString(), type, (VariableModifiers)this.GetVariableModifiers());
                        string annotationText = this.GetAnnotationText() as string;
                        if (annotationText != null)
                        {
                            Annotation.SetAnnotationText(variable, annotationText);
                        }

                        //try to preserve expression
                        ModelItem expressionModelItem = this.ReflectedObject.Properties[VariableDefaultProperty].Value;
                        if (expressionModelItem != null)
                        {
                            ActivityWithResult expression = expressionModelItem.GetCurrentValue() as ActivityWithResult;
                            //check if there existed expression
                            if (expression != null)
                            {
                                ActivityWithResult morphedExpression = null;
                                if (ExpressionHelper.TryMorphExpression(expression, false, type, this.Context, out morphedExpression))
                                {
                                    variable.Default = morphedExpression;
                                }
                                //Microsoft 

                            }
                        }
                        Editor.ChangeVariableType(this, variable);
                        ImportDesigner.AddImport(type.Namespace, this.Context);
                        change.Complete();
                    }
                }
            }
        }

        Type GetVariableType()
        {
            return (Type)this.ReflectedObject.Properties[VariableTypeProperty].ComputedValue;
        }

        void SetVariableScope(object newScope)
        {
            using (ModelEditingScope change = this.ReflectedObject.BeginEdit((string)this.Editor.FindResource("changeVariableScopeDescription")))
            {
                if (!ModelItem.Equals(newScope, this.GetVariableScope()))
                {
                    ModelItemCollection currentScopeContainer = this.ReflectedObject.Parent.Parent.Properties["Variables"].Collection;
                    currentScopeContainer.Remove(this.ReflectedObject.GetCurrentValue());
                    ModelItem scope = (newScope as ModelItem) ?? Editor.ScopesList.FirstOrDefault(p => object.Equals(p.GetCurrentValue(), newScope));
                    ModelItemCollection newScopeContainer = scope.Properties["Variables"].Collection;
                    newScopeContainer.Add(this.ReflectedObject.GetCurrentValue());
                    Editor.NotifyVariableScopeChanged(this);
                }
                change.Complete();
            }
        }

        object GetVariableScope()
        {
            return this.ReflectedObject.Parent.Parent;
        }

        void SetVariableValue(object value)
        {
            object expression = value is ModelItem ? ((ModelItem)value).GetCurrentValue() : value;
            this.ReflectedObject.Properties[VariableDefaultProperty].SetValue(expression);
        }

        object GetVariableValue()
        {
            return this.ReflectedObject.Properties[VariableDefaultProperty].ComputedValue;
        }

        string GetToolTip()
        {
            ModelItem s = this.ReflectedObject.Parent.Parent;
            IMultiValueConverter converter = (IMultiValueConverter)(this.Editor.FindResource("scopeToNameConverter"));
            return ScopeToTooltipConverter.BuildToolTip(s, converter, CultureInfo.CurrentCulture);
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

        protected override Type OnGetDynamicPropertyValueEditorType(string propertyName)
        {
            var type = this.GetVariableType();

            //in case of variables which contain handles - display HandleValueEditor
            if (typeof(Handle).IsAssignableFrom(type))
            {
                return typeof(HandleValueEditor);
            }

            var referenceType = typeof(PropertyValueEditor);
            var expressionEditorType = typeof(ExpressionValueEditor);

            //check if there are custom editors on the variable's type
            var variableOfType = typeof(Variable<>).MakeGenericType(type);

            //check if there are custom type editors associated with given type - 
            //look for type editor defined for Variable<T>
            //in search, skip ExpressionValueEditor instance - it will be returned by default for property grid, but for
            //dataGrid nothing should be used - we use default dg template
            var customEditorType = TypeDescriptor
                .GetAttributes(variableOfType)
                .OfType<EditorAttribute>()
                .Where(p =>
                    {
                        Type currentType = Type.GetType(p.EditorTypeName);
                        return (expressionEditorType != currentType && referenceType.IsAssignableFrom(currentType));
                    })
                .Select(p => Type.GetType(p.EditorTypeName))
                .FirstOrDefault();


            //return custom editor type (if any)
            if (null != customEditorType)
            {
                return customEditorType;
            }

            //otherwise - return default expression value editor
            return typeof(ExpressionValueEditor);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            //this method is called by the thread's dispatcher AFTER all prorties have been updated, so all the property values
            //are updated, regardless of the editing scope deep
            if (string.Equals(propertyName, DesignTimeVariable.VariableScopeProperty))
            {
                this.RaisePropertyChangedEvent(ToolTipProperty);
                this.RaisePropertyChangedEvent(VariableScopeLevelProperty);
            }
            else if (string.Equals(propertyName, DesignTimeVariable.VariableNameProperty))
            {
                this.RaisePropertyChangedEvent(AutomationIdProperty);
            }
            else if (string.Equals(propertyName, DesignTimeVariable.VariableTypeProperty))
            {
                this.RaisePropertyChangedEvent(VariableDefaultProperty);
            }
            else if (string.Equals(propertyName, DesignTimeVariable.TimestampProperty))
            {
                if (this.variableTypeChanged)
                {
                    this.RaisePropertyChangedEvent(DesignTimeVariable.VariableTypeProperty);
                    this.variableTypeChanged = false;
                    this.CustomValueEditors.Clear();
                    this.Editor.UpdateTypeDesigner(this);
                }
            }
            base.OnPropertyChanged(propertyName);
        }

        bool ValidateVariableName(object newValue, List<string> errors)
        {
            if (!base.IsUndoRedoInProgress && null != this.ReflectedObject.Parent)
            {
                VBIdentifierName identifier = newValue as VBIdentifierName;

                string newName = null;
                if (identifier != null)
                {
                    newName = identifier.IdentifierName;
                }


                if (!string.IsNullOrEmpty(newName))
                {
                    Func<ModelItem, bool> checkForDuplicates =
                        new Func<ModelItem, bool>(p => string.Equals(p.Properties[VariableNameProperty].ComputedValue, newName) && !object.Equals(p, this.ReflectedObject));

                    bool duplicates = this.ReflectedObject.Parent.Parent.Properties["Variables"].Collection.Any(checkForDuplicates);

                    if (duplicates)
                    {
                        errors.Add(string.Format(CultureInfo.CurrentUICulture, SR.DuplicateVariableName, newName));
                    }
                }
                else
                {
                    errors.Add(SR.EmptyVariableName);
                }
            }
            return 0 == errors.Count;
        }

        bool ValidateVariableScope(object newValue, List<string> errors)
        {
            if (!base.IsUndoRedoInProgress)
            {
                ModelItem scope = (newValue as ModelItem) ?? Editor.ScopesList.FirstOrDefault(p => object.Equals(p.GetCurrentValue(), newValue));
                string currentName = this.GetVariableNameString();

                Func<ModelItem, bool> checkForDuplicates =
                    new Func<ModelItem, bool>(p => string.Equals(p.Properties[VariableNameProperty].ComputedValue, currentName) && !object.Equals(p, this.ReflectedObject));

                bool duplicates = scope.Properties["Variables"].Collection.Any(checkForDuplicates);
                if (duplicates)
                {
                    errors.Add(string.Format(CultureInfo.CurrentUICulture, SR.DuplicateVariableName, currentName));
                }
            }
            return 0 == errors.Count;
        }

        #region Internal classes
        internal sealed class ScopeValueEditor : PropertyValueEditor
        {
            public ScopeValueEditor()
            {
                this.InlineEditorTemplate = EditorResources.GetResources()["ScopeEditor_InlineEditorTemplate"] as DataTemplate;
            }
        }

        #endregion
    }

    sealed class DesignTimeVariableToScopeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModelItem designTimeVariable = value as ModelItem;
            object result = null;
            if (null != designTimeVariable && typeof(DesignTimeVariable).IsAssignableFrom(designTimeVariable.ItemType))
            {
                DesignTimeVariable variable = (DesignTimeVariable)designTimeVariable.GetCurrentValue();
                result = variable.Editor.ScopesList;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }

    sealed class ScopeToTooltipConverter : IValueConverter
    {
        IMultiValueConverter baseConverter = new BreadCrumbTextConverter();

        internal static string BuildToolTip(ModelItem entry, IMultiValueConverter displayNameConverter, CultureInfo culture)
        {
            string result = null;
            if (null != entry && null != displayNameConverter)
            {
                StringBuilder sb = new StringBuilder();
                int indent = 0;
                ModelItem currentEntry = entry;
                while (currentEntry != null)
                {
                    if (null != currentEntry.Properties["Variables"])
                    {
                        ++indent;
                    }
                    currentEntry = currentEntry.Parent;
                }

                while (entry != null)
                {
                    if (null != entry.Properties["Variables"])
                    {
                        if (sb.Length != 0)
                        {
                            sb.Insert(0, "/");
                            sb.Insert(0, " ", --indent);
                            sb.Insert(0, Environment.NewLine);
                        }
                        var input = new object[] { entry, null != entry.Properties["DisplayName"] ? entry.Properties["DisplayName"].Value : null, (double)short.MaxValue };
                        sb.Insert(0, displayNameConverter.Convert(input, typeof(string), null, culture));
                    }
                    entry = entry.Parent;
                }
                result = sb.ToString();
            }
            return result;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BuildToolTip(value as ModelItem, this.baseConverter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }

    sealed class ScopeComboBox : ComboBox
    {
        bool isScopeValid = true;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += (s, args) =>
                {
                    //get the binding expression, and hook up exception filter
                    var expr = this.GetBindingExpression(ScopeComboBox.SelectedItemProperty);
                    if (null != expr && null != expr.ParentBinding)
                    {
                        expr.ParentBinding.UpdateSourceExceptionFilter = this.OnUpdateBindingException;
                    }
                };
        }

        object OnUpdateBindingException(object sender, Exception err)
        {
            //if exception occured, the scope as invalid
            if (err is TargetInvocationException && err.InnerException is ValidationException || err is ValidationException)
            {
                this.isScopeValid = false;
            }
            return null;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            //if validation succeeded - update the control state with new selection
            if (this.isScopeValid)
            {
                base.OnSelectionChanged(e);
            }
            //otherwise, get the binding expression and update control with current state from the source
            else
            {
                var expr = this.GetBindingExpression(ScopeComboBox.SelectedItemProperty);
                if (null != expr)
                {
                    expr.UpdateTarget();
                }
                //the next failed validation pass may set this flag to false, but if validation succeeds, it has to be set to true
                this.isScopeValid = true;
            }
        }
    }
}
