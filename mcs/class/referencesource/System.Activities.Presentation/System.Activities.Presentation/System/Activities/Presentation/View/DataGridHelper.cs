//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Windows.Media.Effects;

    [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.TypesShouldHavePublicParameterlessConstructors,
        Justification = "This class is never supposed to be created in xaml directly.")]
    sealed partial class DataGridHelper
    {
        public static readonly string PART_ButtonAdd = "PART_ButtonAdd";
        static readonly string dynamicContentControlName = "PART_Dynamic";

        //content of the Add new row button
        public static readonly DependencyProperty AddNewRowContentProperty =
            DependencyProperty.Register("AddNewRowContent", typeof(object), typeof(DataGridHelper), new UIPropertyMetadata("<Add new row>"));

        //binding to the command, which gets executed when new row button is clicked
        public static readonly DependencyProperty AddNewRowCommandProperty =
            DependencyProperty.Register("AddNewRowCommand", typeof(ICommand), typeof(DataGridHelper), new UIPropertyMetadata(null));

        //attached property - used to store reference to data grid helper within data grid instance
        static readonly DependencyProperty DGHelperProperty =
            DependencyProperty.RegisterAttached("DGHelper", typeof(DataGridHelper), typeof(DataGrid), new UIPropertyMetadata(null));

        static readonly DependencyProperty ControlBehaviorProperty =
            DependencyProperty.RegisterAttached("ControlBehavior", typeof(EditingControlBehavior), typeof(DataGridHelper), new UIPropertyMetadata(null));

        static readonly DependencyProperty NewRowLoadedProperty =
            DependencyProperty.RegisterAttached("NewRowLoaded", typeof(bool), typeof(DataGridHelper), new UIPropertyMetadata(false));

        static readonly DependencyProperty IsCommitInProgressProperty =
            DependencyProperty.RegisterAttached("IsCommitInProgress", typeof(bool), typeof(DataGridHelper), new UIPropertyMetadata(false));

        public static readonly DependencyProperty ShowValidationErrorAsToolTipProperty =
            DependencyProperty.Register("ShowValidationErrorAsToolTip", typeof(bool), typeof(DataGridHelper), new UIPropertyMetadata(false));

        public static readonly DependencyProperty IsCustomEditorProperty =
            DependencyProperty.RegisterAttached("IsCustomEditor", typeof(bool), typeof(DataGridHelper), new UIPropertyMetadata(false));

        public event EventHandler<DataGridCellEditEndingEventArgs> DataGridCellEditEnding;

        static DataTemplate dynamicCellContentTemplate;
        static Dictionary<Type, Type> EditorBehaviorTypeMapping = new Dictionary<Type, Type>
        {
            { typeof(ExpressionTextBox), typeof(ExpressionTextBoxBehavior) },
            { typeof(TextBox), typeof(TextBoxBehavior) },
            { typeof(TypePresenter), typeof(TypePresenterBehavior) },
            { typeof(VBIdentifierDesigner), typeof(VBIdentifierDesignerBehavior) },
        };

        DataGrid dataGrid;
        bool isNewRowAdded;

        Func<ResolveTemplateParams, bool> resolveDynamicTemplateCallback;
        Dictionary<string, DataGridColumn> MemberPathToColumnDict = new Dictionary<string, DataGridColumn>();

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.ReviewUnusedParameters, Justification = "Existing code")]
        public DataGridHelper(DataGrid instance, Control owner)
        {
            this.InitializeComponent();

            this.dataGrid = instance;
            //apply default cell style
            this.ApplyCellStyle();
            //apply default row style
            this.ApplyRowStyle();
            //apply default datagrid style
            this.dataGrid.Style = (Style)this.FindResource("defaultDataGridStyle");
            //handle data grid's loading event
            this.dataGrid.LoadingRow += OnDataGridRowLoading;
            //store reference to data grid helper within datagrid
            DataGridHelper.SetDGHelper(this.dataGrid, this);
            this.dataGrid.MouseDown += OnDataGridMouseDown;
            this.dataGrid.Sorting += OnDataGridSorting;
            this.dataGrid.CellEditEnding += OnDataGridCellEditEnding;
            this.InitMemberPathToColumnDict();
        }

        public static bool GetIsCustomEditor(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCustomEditorProperty);
        }

        public static void SetIsCustomEditor(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCustomEditorProperty, value);
        }

        void OnDataGridCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (this.DataGridCellEditEnding != null)
            {
                this.DataGridCellEditEnding(sender, e);
            }

            if ((!e.Cancel) && (!GetIsCommitInProgress(this.dataGrid)))
            {
                SetIsCommitInProgress(this.dataGrid, true);
                //try to commit edit
                bool commitSucceeded = false;
                try
                {
                    commitSucceeded = this.dataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                }
                catch (InvalidOperationException)
                {
                    // Ignore and cancel edit
                }
                finally
                {
                    //if commit fails - undo change
                    if (!commitSucceeded)
                    {
                        this.dataGrid.CancelEdit();
                    }
                }
                SetIsCommitInProgress(this.dataGrid, false);
            }
        }

        void InitMemberPathToColumnDict()
        {
            MemberPathToColumnDict.Clear();
            foreach (DataGridColumn column in this.dataGrid.Columns)
            {
                if (column.CanUserSort &&
                    !string.IsNullOrEmpty(column.SortMemberPath) &&
                    !MemberPathToColumnDict.ContainsKey(column.SortMemberPath))
                {
                    MemberPathToColumnDict.Add(column.SortMemberPath, column);
                }
            }
        }

        void OnDataGridSorting(object sender, DataGridSortingEventArgs e)
        {
            bool primaryColumnSorted = false;
            ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
            e.Column.SortDirection = null;

            foreach (SortDescription description in this.dataGrid.Items.SortDescriptions.Reverse())
            {
                if (MemberPathToColumnDict[description.PropertyName].SortDirection == null)
                {
                    this.dataGrid.Items.SortDescriptions.Remove(description);
                }
                else if (description.PropertyName == this.dataGrid.Columns[0].SortMemberPath)
                {
                    primaryColumnSorted = true;
                }
            }

            this.dataGrid.Items.SortDescriptions.Add(new SortDescription(e.Column.SortMemberPath, direction));
            e.Column.SortDirection = direction;
            if (e.Column != this.dataGrid.Columns[0] && !primaryColumnSorted)
            {
                this.dataGrid.Items.SortDescriptions.Add(new SortDescription(this.dataGrid.Columns[0].SortMemberPath, ListSortDirection.Ascending));
            }
            this.dataGrid.Items.Refresh();

            e.Handled = true;
        }

        //Hook KeyDown event on DataGrid row to workaround DataGrid 
        void OnDataGridRowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                // If currentCell is the cell containing AddNewRowTemplate, its Column will be null since this cell
                // spread across all columns in the grid. In this case, consume the event with no action.
                if (dataGrid.CurrentCell.Column != null)
                {
                    DataGridCellInfo currentCell = dataGrid.CurrentCell;
                    ObservableCollection<DataGridColumn> columns = dataGrid.Columns;
                    ItemCollection items = dataGrid.Items;
                    int currentColumnIndex = columns.IndexOf(dataGrid.ColumnFromDisplayIndex(currentCell.Column.DisplayIndex));
                    DataGridCell currentCellContainer = GetCell(dataGrid, items.IndexOf(currentCell.Item), currentColumnIndex);
                    if ((currentCellContainer != null) && (columns.Count > 0))
                    {
                        int numItems = items.Count;
                        bool shiftModifier = ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
                        int index = Math.Max(0, Math.Min(numItems - 1, items.IndexOf(currentCell.Item) + (shiftModifier ? -1 : 1)));
                        if (index < numItems)
                        {
                            if (items[index] == CollectionView.NewItemPlaceholder)
                            {
                                CommitAnyEdit(currentCellContainer);
                                e.Handled = true;
                            }
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        void CommitAnyEdit(DataGridCell currentCellContainer)
        {
            IEditableCollectionView editableItems = (IEditableCollectionView)(this.dataGrid.Items);
            DataGridCell cell = currentCellContainer;
            bool isCurrentCellEditing = false;
            this.ExplicitCommit = true;
            if (cell != null)
            {
                isCurrentCellEditing = cell.IsEditing;
            }
            if (editableItems.IsAddingNew || editableItems.IsEditingItem)
            {
                this.dataGrid.CommitEdit(DataGridEditingUnit.Row, true);
            }
            else if (isCurrentCellEditing)
            {
                this.dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            }
            this.ExplicitCommit = false;
        }

        //callback executed whenever user clicks AddNewRow
        public Func<DataGrid, object, object> NotifyNewRowAddedCallback
        {
            get;
            set;
        }

        //callback executed whenever users starts editing cell
        public Action<Control, DataGridCell, bool> NotifyBeginCellEditCallback
        {
            get;
            set;
        }

        //callback executed whenver cell edit is complete
        public Action<Control, DataGridCell> NotifyEndCellEditCallback
        {
            get;
            set;
        }

        public bool ExplicitCommit
        {
            get;
            private set;
        }

        internal DataGrid DataGrid
        {
            get { return this.dataGrid; }
        }

        internal bool IsEditInProgress
        {
            get { return GetIsCommitInProgress(this.dataGrid); }
        }

        internal EditingContext Context
        {
            get;
            set;
        }

        //callback executed whenever dynamic content cell is loaded
        //parameters: 
        //  - clicked data grid cell,
        //  - reference to data item in given dg row
        //  - boolean value indicating whether cell is beeing edited or viewewd
        //returns:
        // - data template to be applied
        public Func<ResolveTemplateParams, bool> ResolveDynamicTemplateCallback
        {
            get { return this.resolveDynamicTemplateCallback; }
            set
            {
                this.resolveDynamicTemplateCallback = value;
                //if user adds dynamic template, we need to hook for EditMode button event - ShowDialogEditor; 
                //otherwise, clicking on that button wouldn't have any effect, since it is normally handled by property grid
                bool containsBinding = this.dataGrid.CommandBindings
                    .Cast<CommandBinding>()
                    .Any(cb => ICommand.Equals(cb.Command, PropertyValueEditorCommands.ShowDialogEditor));

                if (!containsBinding)
                {
                    var cb = new CommandBinding(PropertyValueEditorCommands.ShowDialogEditor, this.OnShowPropertyValueEditor, this.OnCanShowPropertyValueEditor);
                    this.dataGrid.CommandBindings.Add(cb);
                }
            }
        }

        //callback executed whenever user clicks extended dialog property editor in the data grid - 
        //client has to specify reference to edited model property, which will be placed in extended editor dialog
        //parameters:
        // - clicked data grid cell
        // - reference to data item in given dg row
        //returns:
        // - reference to model property which should be displayed
        public Func<DataGridCell, object, ModelProperty> LoadDynamicContentDataCallback
        {
            get;
            set;
        }

        //callback executed whenever user clicks extended dialog property editor in the data grid
        //parameters:
        // - clicked data grid cell
        // - reference to data item in given dg row
        //returns:
        // - instance of dialog property value editor
        public Func<DataGridCell, object, DialogPropertyValueEditor> LoadCustomPropertyValueEditorCallback
        {
            get;
            set;
        }

        //default row template 
        ControlTemplate DefaultRowControlTemplate
        {
            get;
            set;
        }

        ContentPresenter AddNewRowContentPresenter
        {
            get;
            set;
        }

        //property containing content displayed on the Add new row button
        public object AddNewRowContent
        {
            get { return (object)GetValue(AddNewRowContentProperty); }
            set { SetValue(AddNewRowContentProperty, value); }
        }

        //command bound to add new row button
        public ICommand AddNewRowCommand
        {
            get { return (ICommand)GetValue(AddNewRowCommandProperty); }
            set { SetValue(AddNewRowCommandProperty, value); }
        }

        public bool ShowValidationErrorAsToolTip
        {
            get { return (bool)GetValue(ShowValidationErrorAsToolTipProperty); }
            set { SetValue(ShowValidationErrorAsToolTipProperty, value); }
        }

        //helper method - returns selected data grid item casted to the target type
        public T SelectedItem<T>() where T : class
        {
            return this.dataGrid.SelectedItem as T;
        }

        public T Source<T>() where T : class
        {
            return (T)this.dataGrid.ItemsSource;
        }

        public void BeginRowEdit(object value, DataGridColumn column)
        {
            if (null == value)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("value"));
            }
            if (null == column)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("column"));
            }
            int columnIndex = this.dataGrid.Columns.IndexOf(column);
            if (columnIndex < 0)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("column"));
            }
            ICollectionView items = CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
            if (null != items)
            {
                this.CommitDataGrid();
                this.dataGrid.SelectedItem = null;
                //lookup element in the collection
                if (items.MoveCurrentTo(value))
                {
                    //set the SelectedItem to passed value
                    this.dataGrid.SelectedItem = value;
                    //get the cell which contains given elemnt
                    DataGridCell cell = DataGridHelper.GetCell(this.dataGrid, items.CurrentPosition, columnIndex);
                    //and begin edit
                    if (null != cell)
                    {
                        cell.Focus();
                        dataGrid.BeginEdit();
                    }
                }
                else
                {
                    throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("value"));
                }
            }
        }

        public void BeginRowEdit(object value)
        {
            var column = this.dataGrid.Columns[0];
            int index = 1;
            while (null != column && column.Visibility == Visibility.Hidden && this.dataGrid.Columns.Count > index)
            {
                column = this.dataGrid.Columns[index];
                ++index;
            }
            this.BeginRowEdit(value, column);
        }

        void OnDataGridRowLoading(object sender, DataGridRowEventArgs e)
        {
            if (this.DefaultRowControlTemplate == null)
            {
                this.DefaultRowControlTemplate = e.Row.Template;
            }

            if (e.Row.Item == CollectionView.NewItemPlaceholder)
            {
                e.Row.Style = (Style)this.FindResource("defaultNewRowStyle");
                e.Row.UpdateLayout();
            }
        }

        void OnAddNewRowContentPresenterLoaded(object sender, RoutedEventArgs args)
        {
            var presenter = (ContentPresenter)sender;
            this.AddNewRowContentPresenter = presenter;
            if (null != this.AddNewRowContent)
            {
                if (this.AddNewRowContent is DataTemplate)
                {
                    presenter.ContentTemplate = (DataTemplate)this.AddNewRowContent;
                    presenter.ApplyTemplate();
                }
                else
                {
                    presenter.ContentTemplate = (DataTemplate)this.FindResource("defaultAddNewRowTemplate");
                    presenter.ApplyTemplate();
                    presenter.Content = this.AddNewRowContent.ToString();
                }
            }
        }

        void OnAddNewRowClick(object sender, RoutedEventArgs args)
        {
            //user clicked on AddNew row - commit all pending changes
            this.CommitDataGrid();
            Button btn = (Button)sender;
            //if there is callback registered 
            if (null != this.NotifyNewRowAddedCallback)
            {
                //execute it
                object added = this.NotifyNewRowAddedCallback(this.dataGrid, btn.CommandParameter);
                //if add was successfull, begin editing new row
                this.isNewRowAdded = (null != added);
                if (this.isNewRowAdded)
                {
                    this.BeginRowEdit(added);
                }
            }
            //if there is command registered
            else if (null != this.AddNewRowCommand)
            {
                //try to invoke command as routed command, the as the interface command
                RoutedCommand cmd = this.AddNewRowCommand as RoutedCommand;
                if (null == cmd)
                {
                    if (this.AddNewRowCommand.CanExecute(btn.CommandParameter))
                    {
                        this.AddNewRowCommand.Execute(btn.CommandParameter);
                        this.isNewRowAdded = true;
                    }
                }
                else
                {
                    if (cmd.CanExecute(btn.CommandParameter, this.dataGrid))
                    {
                        cmd.Execute(btn.CommandParameter, this.dataGrid);
                        this.isNewRowAdded = true;
                    }
                }
            }
        }

        void OnAddNewRowGotFocus(object sender, RoutedEventArgs e)
        {
            //When tab over the last row, the last column won't get commit by default, which is a 

            this.CommitDataGrid();
            this.dataGrid.SelectedItem = null;
        }

        void CommitDataGrid()
        {
            if (!GetIsCommitInProgress(this.dataGrid))
            {
                SetIsCommitInProgress(this.dataGrid, true);
                this.ExplicitCommit = true;                
                this.dataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                this.ExplicitCommit = false;
                SetIsCommitInProgress(this.dataGrid, false);
            }
        }

        void NotifyEditingControlLoaded(Control control, DataGridCell cell, bool isNewRowLoaded)
        {
            Type controlType = control.GetType();
            Type editorBehaviorType;
            if (EditorBehaviorTypeMapping.ContainsKey(controlType))
            {
                editorBehaviorType = EditorBehaviorTypeMapping[controlType];
            }
            else
            {
                editorBehaviorType = typeof(DefaultControlBehavior);
            }

            EditingControlBehavior behavior = Activator.CreateInstance(editorBehaviorType, this.dataGrid) as EditingControlBehavior;
            bool isHandled = behavior.HandleControlLoaded(control, cell, isNewRowLoaded);
            if (isHandled)
            {
                SetControlBehavior(control, behavior);
            }

            if (null != this.NotifyBeginCellEditCallback)
            {
                this.NotifyBeginCellEditCallback(control, cell, isNewRowLoaded);
            }
        }

        void NotifyEditingControlUnloaded(Control control, DataGridCell cell)
        {
            bool isHandled = false;

            EditingControlBehavior behavior = GetControlBehavior(control);

            if (null != behavior)
            {
                isHandled = behavior.ControlUnloaded(control, cell);
            }

            if (null != this.NotifyEndCellEditCallback)
            {
                this.NotifyEndCellEditCallback(control, cell);
            }
        }

        void OnCanShowPropertyValueEditor(object sender, CanExecuteRoutedEventArgs args)
        {
            Fx.Assert(this.LoadCustomPropertyValueEditorCallback != null, "LoadCustomPropertyValueEditorCallback is not set!");
            Fx.Assert(this.LoadDynamicContentDataCallback != null, "LoadDynamicContentDataCallback is not set!");

            if (null != this.LoadDynamicContentDataCallback && null != this.LoadCustomPropertyValueEditorCallback)
            {
                var cell = VisualTreeUtils.FindVisualAncestor<DataGridCell>((DependencyObject)args.OriginalSource);
                var row = VisualTreeUtils.FindVisualAncestor<DataGridRow>(cell);
                args.CanExecute = null != this.LoadCustomPropertyValueEditorCallback(cell, row.Item);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "Propagating exceptions might lead to VS crash.")]
        void OnShowPropertyValueEditor(object sender, ExecutedRoutedEventArgs args)
        {
            //user clicked on dialog's property editor's button - now we need to show custom designer in dialog mode
            var cell = VisualTreeUtils.FindVisualAncestor<DataGridCell>((DependencyObject)args.OriginalSource);
            var row = VisualTreeUtils.FindVisualAncestor<DataGridRow>(cell);
            //ask client for custom editor, given for currently selected row
            var editor = this.LoadCustomPropertyValueEditorCallback(cell, row.Item);

            Fx.Assert(editor != null, "Custom property value editor is not set or doesn't derive from DialogPropertyValueEditor!");
            if (null != editor)
            {
                //out of currently selected row, get actual property which is beeing edited
                var value = this.LoadDynamicContentDataCallback(cell, row.Item);

                Fx.Assert(value != null, "ModelProperty shouldn't be null");

                //create model property entry - it is required by dialog property editor
                var propertyEntry = new ModelPropertyEntry(value, null);
                try
                {
                    editor.ShowDialog(propertyEntry.PropertyValue, (IInputElement)args.OriginalSource);
                }
                catch (Exception err)
                {
                    ErrorReporting.ShowErrorMessage(err);
                }
            }
        }

        internal static void OnEditingControlLoaded(object sender, RoutedEventArgs args)
        {
            //editing control has been loaded - user starts editing the cell
            Control ctrl = (Control)sender;

            //get the data grid reference from control
            DataGrid dg = VisualTreeUtils.FindVisualAncestor<DataGrid>(ctrl);
            Fx.Assert(null != dg, string.Format(CultureInfo.CurrentCulture, "DataGrid is not in the visual tree of this control: {0}", ctrl));
            if (null != dg)
            {
                //get the target instance of data grid helper
                DataGridHelper helper = DataGridHelper.GetDGHelper(dg);
                //store data grid helper in the control
                DataGridHelper.SetDGHelper(ctrl, helper);
                if (null != helper)
                {
                    //notify user that given control is becoming acive one
                    DataGridCell cell = VisualTreeUtils.FindVisualAncestor<DataGridCell>(ctrl);
                    helper.NotifyEditingControlLoaded(ctrl, cell, helper.isNewRowAdded);
                    helper.isNewRowAdded = false;
                }
            }
        }

        internal static void OnEditingControlUnloaded(object sender, RoutedEventArgs args)
        {
            //editing control has been unloaded - user ends editing the cell
            Control ctrl = (Control)sender;

            //get data grid helper out of it
            DataGridHelper helper = DataGridHelper.GetDGHelper(ctrl);

            //notify user that edit is complete
            if (null != helper)
            {
                DataGridCell cell = VisualTreeUtils.FindVisualAncestor<DataGridCell>(ctrl);
                helper.NotifyEditingControlUnloaded(ctrl, cell);
            }
        }

        void OnPreviewCellMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //When Ctrl of Shift is pressed, let DataGrid to handle multi-selection
            //and DataGrid shouldn't enter editing mode in this case
            if ((Keyboard.IsKeyDown(Key.RightShift)) || (Keyboard.IsKeyDown(Key.LeftShift)) ||
                (Keyboard.IsKeyDown(Key.LeftCtrl)) || (Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                return;
            }

            //support for single click edit
            DataGridCell cell = sender as DataGridCell;
            //enter this code only if cell is not beeing edited already and is not readonly
            if (null != cell && !cell.IsEditing && !cell.IsReadOnly && null != this.dataGrid.SelectedItem)
            {
                bool shouldFocus = true;
                //depending on the selection type - either select cell or row
                if (this.dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                {
                    if (!cell.IsSelected)
                    {
                        cell.IsSelected = true;
                    }
                }
                else
                {
                    DataGridRow row = VisualTreeUtils.FindVisualAncestor<DataGridRow>(cell);
                    //if row was not selected - first click will select it, second will start editing cell's value
                    if (null != row && !row.IsSelected)
                    {
                        this.dataGrid.SelectedItem = row;
                        shouldFocus = false;
                    }
                }

                //if allowed - begin edit
                if ((shouldFocus && !cell.IsFocused) && !GetIsCustomEditor(cell))
                {
                    //attempt to set focus to the cell, and let DG start editing                    
                    if (cell.Focus() && !cell.IsEditing)
                    {
                        if (dataGrid.SelectionUnit == DataGridSelectionUnit.FullRow)
                        {
                            dataGrid.SelectedItems.Clear();
                            DataGridRow row = VisualTreeUtils.FindVisualAncestor<DataGridRow>(cell);
                            if (row != null)
                            {
                                dataGrid.SelectedItems.Add(dataGrid.ItemContainerGenerator.ItemFromContainer(row));
                            }
                        }
                        else
                        {
                            dataGrid.SelectedCells.Clear();
                            dataGrid.SelectedCells.Add(new DataGridCellInfo(cell));
                        }
                        this.dataGrid.BeginEdit();
                    }
                }
            }
        }

        void OnDataGridMouseDown(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
            if (null != this.dataGrid.SelectedItem && this.dataGrid.CurrentCell.IsValid && view.MoveCurrentTo(this.dataGrid.SelectedItem))
            {
                int rowIndex = view.CurrentPosition;
                int columnIndex = this.dataGrid.Columns.IndexOf(this.dataGrid.CurrentCell.Column);
                var cell = DataGridHelper.GetCell(this.dataGrid, rowIndex, columnIndex);
                if (null != cell && cell.IsEditing)
                {
                    this.CommitDataGrid();
                    cell.Focus();
                }
            }
            else
            {
                this.dataGrid.Focus();
            }
        }


        void OnDynamicContentColumnLoaded(DataGridCell cell, ContentControl contentContainer)
        {
            //user marked at least one column in data grid with DynamicContent - now, we have to query client for
            //cell template for given row's property
            if (null != this.ResolveDynamicTemplateCallback)
            {
                var resolveParams = new ResolveTemplateParams(cell, contentContainer.Content);
                if (this.ResolveDynamicTemplateCallback(resolveParams) && null != resolveParams.Template)
                {
                    if (!resolveParams.IsDefaultTemplate)
                    {
                        var content = this.LoadDynamicContentDataCallback(cell, contentContainer.Content);
                        var propertyEntry = new ModelPropertyEntry(content, null);
                        contentContainer.Content = propertyEntry.PropertyValue;
                        SetIsCustomEditor(cell, true);
                    }
                    else
                    {
                        contentContainer.Content = cell.DataContext;
                        SetIsCustomEditor(cell, false);
                    }
                    contentContainer.ContentTemplate = resolveParams.Template;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ResolveDynamicTemplateCallback not registered for column " + cell.Column.Header);
            }
        }

        public void UpdateDynamicContentColumns(object entry)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
            int rowIndex = -1;
            //get index of given entry
            if (view.MoveCurrentTo(entry))
            {
                rowIndex = view.CurrentPosition;
            }
            if (-1 != rowIndex)
            {
                //pickup all dynamic columns in this data grid
                var dynamicColumnsIndexes = this.dataGrid.Columns
                    .OfType<DataGridTemplateColumn>()
                    .Where(p => DataTemplate.Equals(p.CellEditingTemplate, DataGridHelper.DynamicCellContentTemplate) &&
                                DataTemplate.Equals(p.CellTemplate, DataGridHelper.DynamicCellContentTemplate))
                    .Select<DataGridColumn, int>(p => this.dataGrid.Columns.IndexOf(p));

                //foreach dynamic column
                foreach (var columnIndex in dynamicColumnsIndexes)
                {
                    //get the cell
                    var cell = DataGridHelper.GetCell(this.dataGrid, rowIndex, columnIndex);

                    //get the content presenter within it
                    var dynamicContent = VisualTreeUtils.GetNamedChild<ContentControl>(cell, DataGridHelper.dynamicContentControlName, 5);

                    //reload the template
                    if (null != dynamicContent)
                    {
                        dynamicContent.ContentTemplate = null;
                        this.OnDynamicContentColumnLoaded(cell, dynamicContent);
                    }
                }
            }

        }

        void ApplyCellStyle()
        {
            //create default cell style
            Style baseStyle = this.dataGrid.CellStyle;
            //respect any user's base styles
            Style style = null == baseStyle ? new Style(typeof(DataGridCell)) : new Style(typeof(DataGridCell), baseStyle);

            //event handler for preview mouse down - single click edit
            style.Setters.Add(new EventSetter(DataGridCell.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(this.OnPreviewCellMouseLeftButtonDown)));
            //width binding - prevent columns from expanding while typing long texts
            style.Setters.Add(new Setter(DataGridCell.WidthProperty, new Binding("Column.ActualWidth")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                Mode = BindingMode.OneWay
            }));
            //automation id - for cell it is always column name
            style.Setters.Add(new Setter(AutomationProperties.AutomationIdProperty, new Binding("Column.Header")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                Mode = BindingMode.OneWay
            }));

            //apply style
            this.dataGrid.CellStyle = style;
        }

        void ApplyRowStyle()
        {
            //create default row style
            Style baseStyle = this.dataGrid.RowStyle;
            //respect any user's base styles
            Style style = null == baseStyle ? new Style(typeof(DataGridRow)) : new Style(typeof(DataGridRow), baseStyle);

            EventSetter keyDownSetter = new EventSetter
            {
                Event = DataGridRow.KeyDownEvent,
                Handler = new KeyEventHandler(this.OnDataGridRowKeyDown)
            };
            style.Setters.Add(keyDownSetter);

            //define a multibinding which displays a tooltip when cell validation fails (failure mean user's data was invalid and was not set in the target property)
            //first - create a binding and add ErrorToTooltipConverter, pass reference to owning data grid helper
            var multiBinding = new MultiBinding() { Converter = new ErrorToTooltipConverter(this) };
            //now define bindings
            //first - bind to actual object behind the row - only DesignObjectWrapper is supported
            var objectWrapperBinding = new Binding() { Mode = BindingMode.OneTime };
            //second - bind to a HasError property change notifications - this will trigger tooltip to appear
            var hasErrorsBinding = new Binding() { Mode = BindingMode.OneWay, Path = new PropertyPath("HasErrors") };
            //finally - bind to a row which contains the data - this will be used as tooltip placement target
            var rowBinding = new Binding() { Mode = BindingMode.OneTime, RelativeSource = new RelativeSource(RelativeSourceMode.Self) };
            multiBinding.Bindings.Add(objectWrapperBinding);
            multiBinding.Bindings.Add(hasErrorsBinding);
            multiBinding.Bindings.Add(rowBinding);

            var errorTooltipTrigger = new DataTrigger()
            {
                Binding = multiBinding,
                Value = true
            };
            //define a dummy setter - it will never be executed anyway, but it is required for the binding to work
            errorTooltipTrigger.Setters.Add(new Setter(DataGridRow.TagProperty, null));
            //add trigger to the collection
            style.Triggers.Add(errorTooltipTrigger);
            //apply style
            this.dataGrid.RowStyle = style;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "Propagating exceptions might lead to VS crash.")]
        static void OnDynamicCellContentLoaded(object sender, RoutedEventArgs e)
        {
            var container = (ContentControl)sender;
            var dataGridCell = VisualTreeUtils.FindVisualAncestor<DataGridCell>(container);
            var dataGrid = VisualTreeUtils.FindVisualAncestor<DataGrid>(dataGridCell);
            if (dataGrid != null)
            {
                var dataGridHelper = DataGridHelper.GetDGHelper(dataGrid);

                if (GetIsCustomEditor(dataGridCell) && (dataGridCell.IsEditing))
                {
                    dataGridHelper.CommitDataGrid();
                }
                else
                {
                    try
                    {
                        dataGridHelper.OnDynamicContentColumnLoaded(dataGridCell, container);
                    }
                    catch (Exception err)
                    {
                        container.Content = err.ToString();
                        container.ContentTemplate = (DataTemplate)dataGrid.Resources["dynamicContentErrorTemplate"];
                        System.Diagnostics.Debug.WriteLine(err.ToString());
                    }
                }
            }
        }

        public static DataTemplate DynamicCellContentTemplate
        {
            get
            {
                if (null == dynamicCellContentTemplate)
                {
                    DataTemplate template = new DataTemplate();

                    FrameworkElementFactory contentControlFactory = new FrameworkElementFactory(typeof(ContentControl));
                    contentControlFactory.SetValue(ContentControl.NameProperty, DataGridHelper.dynamicContentControlName);
                    contentControlFactory.SetBinding(ContentControl.ContentProperty, new Binding());
                    contentControlFactory.AddHandler(ContentControl.LoadedEvent, new RoutedEventHandler(DataGridHelper.OnDynamicCellContentLoaded));

                    template.VisualTree = new FrameworkElementFactory(typeof(NoContextMenuGrid));
                    template.VisualTree.AppendChild(contentControlFactory);
                    template.Seal();
                    dynamicCellContentTemplate = template;
                }
                return dynamicCellContentTemplate;
            }
        }

        static DataGridHelper GetDGHelper(DependencyObject obj)
        {
            return (DataGridHelper)obj.GetValue(DGHelperProperty);
        }

        static void SetDGHelper(DependencyObject obj, DataGridHelper value)
        {
            obj.SetValue(DGHelperProperty, value);
        }

        static EditingControlBehavior GetControlBehavior(DependencyObject obj)
        {
            return (EditingControlBehavior)obj.GetValue(ControlBehaviorProperty);
        }

        static void SetControlBehavior(DependencyObject obj, EditingControlBehavior value)
        {
            obj.SetValue(ControlBehaviorProperty, value);
        }

        static bool GetNewRowLoaded(DependencyObject obj)
        {
            return (bool)obj.GetValue(NewRowLoadedProperty);
        }

        static void SetNewRowLoaded(DependencyObject obj, bool value)
        {
            obj.SetValue(NewRowLoadedProperty, value);
        }

        static bool GetIsCommitInProgress(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCommitInProgressProperty);
        }

        static void SetIsCommitInProgress(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCommitInProgressProperty, value);
        }

        public static DataGridCell GetCell(DataGrid dataGrid, int row, int column)
        {
            DataGridRow rowContainer = GetRow(dataGrid, row);
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter != null)
                {
                    // try to get the cell but it may possibly be virtualized
                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    if (cell == null)
                    {
                        // now try to bring into view and retreive the cell
                        dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);

                        cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    }

                    return cell;
                }
            }

            return null;
        }

        internal static ModelItem GetSingleSelectedObject(DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.SelectedItems == null || dataGrid.SelectedItems.Count != 1)
            {
                return null;
            }

            if (dataGrid.SelectedItems[0] == CollectionView.NewItemPlaceholder)
            {
                return null;
            }

            DesignObjectWrapper designObjectWrapper = dataGrid.SelectedItems[0] as DesignObjectWrapper;
            if (designObjectWrapper != null)
            {
                return designObjectWrapper.ReflectedObject;
            }

            return null;
        }

        /// <summary>
        /// Gets the DataGridRow based on the given index
        /// </summary>
        /// <param name="index">the index of the container to get</param>
        public static DataGridRow GetRow(DataGrid dataGrid, int index)
        {
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // may be virtualized, bring into view and try again
                dataGrid.ScrollIntoView(dataGrid.Items[index]);
                dataGrid.UpdateLayout();

                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }

            return row;
        }

        public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }

            return child;
        }

        public static void CommitPendingEdits(DataGrid dataGrid)
        {
            if (null == dataGrid)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("dataGrid"));
            }

            if (!GetIsCommitInProgress(dataGrid))
            {
                SetIsCommitInProgress(dataGrid, true);

                //try to commit edit
                bool commitSucceeded = false;
                DataGridHelper helper = DataGridHelper.GetDGHelper(dataGrid) as DataGridHelper;
                bool orginalExplicitCommit = helper.ExplicitCommit;
                helper.ExplicitCommit = true;
                try
                {
                    commitSucceeded = dataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                }
                catch (InvalidOperationException)
                {
                    // Ignore and cancel edit
                }
                finally
                {
                    //if commit fails - undo change
                    if (!commitSucceeded)
                    {
                        dataGrid.CancelEdit();
                    }
                    helper.ExplicitCommit = orginalExplicitCommit;
                }

                SetIsCommitInProgress(dataGrid, false);
            }
        }

        public static void OnDeleteSelectedItems(DataGrid dataGrid)
        {
            if (dataGrid == null
                || dataGrid.SelectedItems == null
                || dataGrid.SelectedItems.Count == 0
                || dataGrid.ItemsSource == null)
            {
                return;
            }

            if (!(dataGrid.ItemsSource is IList))
            {
                // if the item source is not a list
                // we can't delete items from it.
                return;
            }

            Int32 nextSelectedIndex = -1;
            ICollection<object> toBeDeleted = new HashSet<object>();
            foreach (object obj in dataGrid.SelectedItems)
            {
                toBeDeleted.Add(obj);
            }

            if (toBeDeleted.Count == 0)
            {
                return;
            }

            if (toBeDeleted.Count == 1)
            {
                // if it is single selection,
                // set selected index to be the current selected index.
                // Set nextSelectedIndex only in single selection to keep the behavior
                // consistent with the "delete" button on key board, where if you 
                // select more than one items and push the "Delete" button on keyboard,
                // nothing will be selected. 
                nextSelectedIndex = dataGrid.Items.IndexOf(toBeDeleted.ElementAt(0));
            }

            IList itemSource = (IList)dataGrid.ItemsSource;
            foreach (object obj in toBeDeleted)
            {
                itemSource.Remove(obj);
            }

            if (nextSelectedIndex >= 0
                && nextSelectedIndex < dataGrid.Items.Count - 1)
            {
                // The last row, whose index is (dataGrid.Items.Count - 1), is
                // the "add new item" row.
                dataGrid.SelectedIndex = nextSelectedIndex;
            }
        }

        internal abstract class EditingControlBehavior
        {
            protected DesignerView DesignerView
            {
                get;
                private set;
            }

            protected DataGrid OwnerDataGrid
            {
                get;
                set;
            }

            public EditingControlBehavior(DataGrid dataGrid)
            {
                this.OwnerDataGrid = dataGrid;
                var helper = DataGridHelper.GetDGHelper(dataGrid);
                if (null != helper && null != helper.Context)
                {
                    this.DesignerView = helper.Context.Services.GetService<DesignerView>();
                }
            }

            public abstract bool HandleControlLoaded(Control control, DataGridCell cell, bool newRowLoaded);
            public abstract bool ControlUnloaded(Control control, DataGridCell cell);

            protected void ToggleDesignerViewAutoCommit(bool shouldIgnore)
            {
                if (null != this.DesignerView)
                {
                    //enable/disable handling of lost keyboard focus events in designer view - 
                    //if shouldIgnore is true, designer view should ignore keyboard focus events thus, not forcing DataGrid to 
                    //commit any changes
                    this.DesignerView.ShouldIgnoreDataGridAutoCommit = shouldIgnore;
                }
            }
        }

        internal sealed class DefaultControlBehavior : EditingControlBehavior
        {
            public DefaultControlBehavior(DataGrid dataGrid)
                : base(dataGrid)
            { }

            public override bool HandleControlLoaded(Control control, DataGridCell cell, bool newRowLoaded)
            {
                System.Diagnostics.Debug.WriteLine("DefaultControlBehavior.HandleControlLoaded");
                control.Focus();
                return true;
            }

            public override bool ControlUnloaded(Control control, DataGridCell cell)
            {
                System.Diagnostics.Debug.WriteLine("DefaultControlBehavior.ControlUnloaded");
                return true;
            }
        }

        internal sealed class TextBoxBehavior : EditingControlBehavior
        {
            public TextBoxBehavior(DataGrid dataGrid)
                : base(dataGrid)
            { }

            public override bool HandleControlLoaded(Control control, DataGridCell cell, bool newRowLoaded)
            {
                System.Diagnostics.Debug.WriteLine("TextBoxBehavior.HandleControlLoaded");
                bool handled = false;
                TextBox tb = control as TextBox;
                if (null != tb)
                {
                    if (newRowLoaded)
                    {
                        tb.SelectAll();
                    }
                    else
                    {
                        tb.CaretIndex = tb.Text.Length;
                    }
                    tb.Focus();
                    handled = true;
                }
                return handled;
            }

            public override bool ControlUnloaded(Control control, DataGridCell cell)
            {
                System.Diagnostics.Debug.WriteLine("TextBoxBehavior.ControlUnloaded");
                return true;
            }
        }

        internal sealed class VBIdentifierDesignerBehavior : EditingControlBehavior
        {
            public VBIdentifierDesignerBehavior(DataGrid dataGrid)
                : base(dataGrid)
            { }

            public override bool HandleControlLoaded(Control control, DataGridCell cell, bool newRowLoaded)
            {
                System.Diagnostics.Debug.WriteLine("VBIdentifierDesignerBehavior.HandleControlLoaded");
                bool handled = false;
                VBIdentifierDesigner identifierDesigner = control as VBIdentifierDesigner;
                if ((null != identifierDesigner) && (!identifierDesigner.IsReadOnly))
                {
                    if (newRowLoaded)
                    {
                        DataGridHelper.SetNewRowLoaded(identifierDesigner, true);
                    }
                    else
                    {
                        DataGridHelper.SetNewRowLoaded(identifierDesigner, false);
                    }

                    identifierDesigner.TextBoxPropertyChanged += this.OnIdentifierDesignerTextBoxChanged;
                    identifierDesigner.Focus();
                    handled = true;
                }
                return handled;
            }

            void OnIdentifierDesignerTextBoxChanged(object sender, PropertyChangedEventArgs e)
            {
                Fx.Assert(e.PropertyName == "IdentifierTextBox", "VBIdentifierDesignerBehavior.TextBoxPropertyChanged event should only be raised when IdentifierTextBox property is changed.");
                VBIdentifierDesigner identifierDesigner = sender as VBIdentifierDesigner;
                TextBox textBox = identifierDesigner.IdentifierTextBox;
                if (textBox != null)
                {
                    if (DataGridHelper.GetNewRowLoaded(identifierDesigner))
                    {
                        textBox.SelectAll();
                        DataGridHelper.SetNewRowLoaded(identifierDesigner, false);
                    }
                    else
                    {
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                    textBox.Focus();
                }
            }

            public override bool ControlUnloaded(Control control, DataGridCell cell)
            {
                System.Diagnostics.Debug.WriteLine("VBIdentifierDesignerBehavior.ControlUnloaded");
                VBIdentifierDesigner identifierDesigner = control as VBIdentifierDesigner;
                if (identifierDesigner != null)
                {
                    identifierDesigner.TextBoxPropertyChanged -= this.OnIdentifierDesignerTextBoxChanged;
                }
                return true;
            }
        }

        internal sealed class TypePresenterBehavior : EditingControlBehavior
        {
            DataGridCell cell;
            TypePresenter typePresenter;
            bool isTypeBrowserOpen = false;
            bool isRegisteredForEvents = false;

            public TypePresenterBehavior(DataGrid dataGrid)
                : base(dataGrid)
            {
                DataGridHelper helper = DataGridHelper.GetDGHelper(dataGrid) as DataGridHelper;
                helper.DataGridCellEditEnding += OnCellEditEnding;
            }

            public override bool HandleControlLoaded(Control control, DataGridCell cell, bool newRowLoaded)
            {
                System.Diagnostics.Debug.WriteLine("TypePresenterBehavior.HandleControlLoaded");
                bool handled = false;
                this.cell = cell;
                this.typePresenter = control as TypePresenter;
                if (null != this.typePresenter)
                {
                    this.isRegisteredForEvents = true;
                    this.typePresenter.TypeBrowserOpened += OnTypeBrowserOpened;
                    this.typePresenter.TypeBrowserClosed += OnTypeBrowserClosed;

                    handled = this.typePresenter.typeComboBox.Focus();
                }
                return handled;
            }

            public override bool ControlUnloaded(Control control, DataGridCell cell)
            {
                System.Diagnostics.Debug.WriteLine("TypePresenterBehavior.ControlUnloaded");
                this.cell = null;
                if (this.isRegisteredForEvents && null != this.typePresenter)
                {
                    this.typePresenter.TypeBrowserOpened -= OnTypeBrowserOpened;
                    this.typePresenter.TypeBrowserClosed -= OnTypeBrowserClosed;
                    this.typePresenter = null;
                    this.isRegisteredForEvents = false;
                    DataGridHelper helper = DataGridHelper.GetDGHelper(this.OwnerDataGrid) as DataGridHelper;
                    helper.DataGridCellEditEnding -= this.OnCellEditEnding;
                }
                return true;
            }

            void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
            {
                if (null != this.cell)
                {
                    e.Cancel = this.isTypeBrowserOpen;
                }
            }

            void OnTypeBrowserOpened(object sender, RoutedEventArgs e)
            {
                base.ToggleDesignerViewAutoCommit(true);
                this.isTypeBrowserOpen = true;
            }

            void OnTypeBrowserClosed(object sender, RoutedEventArgs e)
            {
                base.ToggleDesignerViewAutoCommit(false);
                this.isTypeBrowserOpen = false;
            }
        }

        internal sealed class ExpressionTextBoxBehavior : EditingControlBehavior
        {
            bool isExpressionEditInProgress;
            DataGridCell cell;

            public ExpressionTextBoxBehavior(DataGrid dataGrid)
                : base(dataGrid)
            {
                DataGridHelper helper = DataGridHelper.GetDGHelper(dataGrid) as DataGridHelper;
                helper.DataGridCellEditEnding += OnCellEditEnding;
            }

            public override bool HandleControlLoaded(Control control, DataGridCell cell, bool newRowLoaded)
            {
                System.Diagnostics.Debug.WriteLine("ExpressionTextBoxBehavior.HandleControlLoaded");
                bool handled = false;
                this.cell = cell;
                ExpressionTextBox etb = control as ExpressionTextBox;
                if (null != etb)
                {
                    etb.Tag = cell;
                    //register for logical lost focus events 
                    etb.EditorLostLogicalFocus += OnExpressionEditComplete;

                    if (!etb.IsReadOnly)
                    {
                        //start editing expression
                        etb.BeginEdit();
                        //mark expression edit is in progress, so all CellEditEnding calls will be ignored by datagrid
                        this.isExpressionEditInProgress = true;
                        //disable forced keyboard focus lost events - intelisense window will trigger lost keyboard event, 
                        //which eventualy will lead to commit edit
                        base.ToggleDesignerViewAutoCommit(true);
                    }
                    handled = true;
                }
                return handled;
            }

            public override bool ControlUnloaded(Control control, DataGridCell cell)
            {
                System.Diagnostics.Debug.WriteLine("ExpressionTextBoxBehavior.ControlUnloaded");
                ExpressionTextBox etb = control as ExpressionTextBox;
                if (null != etb)
                {
                    //control is unloaded - unregister from the event
                    etb.EditorLostLogicalFocus -= OnExpressionEditComplete;
                    //if it happens that complete row is beeing unloaded, it is possible that expression edit was still in progress
                    if (this.isExpressionEditInProgress)
                    {
                        //force expression update before unload is complete
                        this.OnExpressionEditComplete(etb, null);
                    }
                }
                DataGridHelper helper = DataGridHelper.GetDGHelper(this.OwnerDataGrid) as DataGridHelper;
                helper.DataGridCellEditEnding -= OnCellEditEnding;
                this.cell = null;
                return true;
            }

            void OnExpressionEditComplete(object sender, RoutedEventArgs e)
            {
                if (this.isExpressionEditInProgress)
                {
                    ExpressionTextBox etb = (ExpressionTextBox)sender;
                    //commit the expression value
                    ((RoutedCommand)DesignerView.CommitCommand).Execute(null, etb);
                    //allow data grid to consume cell editing events
                    this.isExpressionEditInProgress = false;
                    this.OwnerDataGrid.CommitEdit();
                    //restore keyboard focus handling for designer view
                    base.ToggleDesignerViewAutoCommit(false);
                }
            }

            void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
            {
                DataGridHelper helper = DataGridHelper.GetDGHelper(this.OwnerDataGrid) as DataGridHelper;
                if (this.isExpressionEditInProgress && (helper != null) && !helper.ExplicitCommit)
                {
                    e.Cancel = true;
                }
                else if (this.isExpressionEditInProgress)
                {
                    ExpressionTextBox etb = VisualTreeUtils.GetTemplateChild<ExpressionTextBox>(e.EditingElement);
                    this.OnExpressionEditComplete(etb, null);
                }
            }
        }
    }

    sealed class ResolveTemplateParams
    {
        internal ResolveTemplateParams(DataGridCell cell, object instance)
        {
            this.Cell = cell;
            this.Instance = instance;
            this.IsDefaultTemplate = true;
        }

        public DataGridCell Cell { get; private set; }
        public object Instance { get; private set; }
        public bool IsDefaultTemplate { get; set; }
        public DataTemplate Template { get; set; }
    }

    sealed class ErrorToTooltipConverter : IMultiValueConverter
    {
        DataGridHelper owner;
        DataTemplate toolTipTemplate;

        public ErrorToTooltipConverter(DataGridHelper owner)
        {
            this.owner = owner;
            this.toolTipTemplate = (DataTemplate)this.owner.FindResource("errorToolTipTemplate");
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var entry = values.OfType<DesignObjectWrapper>().FirstOrDefault();
            var row = values.OfType<DataGridRow>().FirstOrDefault();
            if (this.owner.IsEditInProgress && null != entry && entry.HasErrors && null != row)
            {
                var invalidProperties = new List<string>();
                if (this.owner.ShowValidationErrorAsToolTip)
                {
                    var errorTip = new ToolTip()
                    {
                        PlacementTarget = row,
                        Placement = PlacementMode.Bottom,
                        ContentTemplate = this.toolTipTemplate,
                        Content = entry.GetValidationErrors(invalidProperties),
                        Effect = new DropShadowEffect() { ShadowDepth = 1 },
                    };
                    AutomationProperties.SetAutomationId(errorTip, "errorToolTip");

                    row.Dispatcher.BeginInvoke(new Action<ToolTip, DataGridRow>((tip, r) =>
                    {
                        tip.IsOpen = true;
                        var dt = new DispatcherTimer(TimeSpan.FromSeconds(6), DispatcherPriority.ApplicationIdle, (sender, e) => { tip.IsOpen = false; }, r.Dispatcher);
                    }), DispatcherPriority.ApplicationIdle, errorTip, row);
                }
                else
                {
                    row.Dispatcher.BeginInvoke(new Action<string>((error) =>
                        {
                            //get currently focused element
                            var currentFocus = (UIElement)Keyboard.FocusedElement;
                            if (null != currentFocus)
                            {
                                //if focus was within datagrid's cell, after loosing focus most likely the editing control would be gone, so try to preserve 
                                //reference to the cell itself
                                currentFocus = VisualTreeUtils.FindVisualAncestor<DataGridCell>(currentFocus) ?? currentFocus;
                            }
                            //show error message (this will result in KeyboardFocus changed
                            ErrorReporting.ShowErrorMessage(error);
                            //restore keyboard focus to stored element, but only if it is somewhere within DesignerView (i don't want to mess with focus in other windows)
                            if (null != currentFocus && null != VisualTreeUtils.FindVisualAncestor<DesignerView>(currentFocus))
                            {
                                Keyboard.Focus(currentFocus);
                            }
                        }), DispatcherPriority.ApplicationIdle, entry.GetValidationErrors(invalidProperties));
                }
                //clear the validation error messages - once the error is raised and displayed, i don't need it anymore in the collection
                entry.ClearValidationErrors(invalidProperties);
            }
            //in case of property grid edit, the errors would be displayed by model item infrastructure, 
            //so just delegate the call to clear errors collection
            if (!this.owner.IsEditInProgress && null != entry && entry.HasErrors)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action<DesignObjectWrapper>((instance) =>
                    {
                        instance.ClearValidationErrors();
                    }), DispatcherPriority.ApplicationIdle, entry);
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
