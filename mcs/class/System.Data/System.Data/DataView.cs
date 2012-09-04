//
// System.Data.DataView.cs
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//    Tim Coleman (tim@timcoleman.com)
//    Punit Todi (punits_mailbox@yahoo.com)
//    Atsushi Enomoto <atsushi@ximian.com>
//    Konstantin Triger (kostat@mainsoft.com)
//
// Copyright (C) Daniel Morgan, 2002, 2003
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002-2003

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Data.Common;
using System.Globalization;
using Mono.Data.SqlExpressions;
using System.Text;

namespace System.Data
{
	/// <summary>
	/// A DataView is used in the binding of data between
	/// a DataTable and Windows Forms or Web Forms allowing
	/// a view of a DataTable for editing, filtering,
	/// navigation, searching, and sorting.
	/// </summary>
	//[Designer]
	[Editor ("Microsoft.VSDesigner.Data.Design.DataSourceEditor, " + Consts.AssemblyMicrosoft_VSDesigner,
		 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	[DefaultEvent ("PositionChanged")]
	[DefaultProperty ("Table")]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.DataViewDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	public partial class DataView : MarshalByValueComponent, IEnumerable, ISupportInitialize {
		internal DataTable dataTable;
		string rowFilter = String.Empty;
		IExpression rowFilterExpr;
		string sort = String.Empty;
		ListSortDirection [] sortOrder;
		PropertyDescriptor sortProperty;
		DataColumn [] sortColumns;
		internal DataViewRowState rowState;
		internal DataRowView[] rowCache = new DataRowView [0];

		// BeginInit() support
		bool isInitPhase;
		bool inEndInit;
		DataTable initTable;
		bool initApplyDefaultSort;
		string initSort;
		string initRowFilter;
		DataViewRowState initRowState;

		// FIXME: what are the default values?
		bool allowNew = true;
		bool allowEdit = true;
		bool allowDelete = true;
		bool applyDefaultSort;
		//bool isSorted = false;

		bool isOpen;

		bool useDefaultSort = true;

		Index _index;
		internal DataRow _lastAdded;

		private DataViewManager dataViewManager;
		internal static ListChangedEventArgs ListResetEventArgs = new ListChangedEventArgs (ListChangedType.Reset,-1,-1);

		public DataView ()
		{
			rowState = DataViewRowState.CurrentRows;
			Open ();
		}

		public DataView (DataTable table)
			: this (table, (DataViewManager) null)
		{
		}

		internal DataView (DataTable table, DataViewManager manager)
		{
			dataTable = table;
			rowState = DataViewRowState.CurrentRows;
			dataViewManager = manager;
			Open ();
		}

		public DataView (DataTable table, string RowFilter,
			string Sort, DataViewRowState RowState)
			: this (table, null, RowFilter, Sort, RowState)
		{
		}

		internal DataView (DataTable table, DataViewManager manager,
			string RowFilter, string Sort, DataViewRowState RowState)
		{
			dataTable = table;
			dataViewManager = manager;
			rowState = DataViewRowState.CurrentRows;
			this.RowFilter = RowFilter;
			this.Sort = Sort;
			rowState = RowState;
			Open ();
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows deletes.")]
#endif
		[DefaultValue (true)]
		public bool AllowDelete {
			get { return allowDelete; }
			set { allowDelete = value; }
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows edits.")]
#endif
		[DefaultValue (true)]
		public bool AllowEdit {
			get { return allowEdit; }
			set { allowEdit = value; }
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows new rows to be added.")]
#endif
		[DefaultValue (true)]
		public bool AllowNew {
			get { return allowNew; }
			set { allowNew = value; }
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates whether to use the default sort if the Sort property is not set.")]
#endif
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.All)]
		public bool ApplyDefaultSort {
			get { return applyDefaultSort; }
			set {
				if (isInitPhase) {
					initApplyDefaultSort = value;
					return;
				}
				if (applyDefaultSort == value)
					return;

				applyDefaultSort = value;
				if (applyDefaultSort == true && (sort == null || sort == string.Empty))
					PopulateDefaultSort ();
				if (!inEndInit) {
					UpdateIndex (true);
					OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1, -1));
				}
			}
		}
		// get the count of rows in the DataView after RowFilter
		// and RowStateFilter have been applied
		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("Returns the number of items currently in this view.")]
#endif
		public int Count {
			get { return rowCache.Length; }
		}

		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("This returns a pointer to back to the DataViewManager that owns this DataSet (if any).")]
#endif
		public DataViewManager DataViewManager {
			get { return dataViewManager; }
		}

		// Item indexer
		// the compiler creates a DefaultMemeberAttribute from
		// this IndexerNameAttribute
		[System.Runtime.CompilerServices.IndexerName("Item")]
		public DataRowView this [int recordIndex] {
			get {
				if (recordIndex > rowCache.Length)
					throw new IndexOutOfRangeException ("There is no row at position: " + recordIndex + ".");
				return rowCache [recordIndex];
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates an expression used to filter the data returned by this DataView.")]
#endif
		[DefaultValue ("")]
		public virtual string RowFilter {
			get { return rowFilter; }
			set {
				if (value == null)
					value = String.Empty;
				if (isInitPhase) {
					initRowFilter = value;
					return;
				}

				CultureInfo info = (Table != null) ? Table.Locale : CultureInfo.CurrentCulture;
				if (String.Compare (rowFilter, value, false, info) == 0)
					return;

				if (value.Length == 0) {
					rowFilterExpr = null;
				} else {
					Parser parser = new Parser ();
					rowFilterExpr = parser.Compile (value);
				}
				rowFilter = value;
				if (!inEndInit) {
					UpdateIndex (true);
					OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1, -1));
				}
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the versions of data returned by this DataView.")]
#endif
		[DefaultValue (DataViewRowState.CurrentRows)]
		public DataViewRowState RowStateFilter {
			get { return rowState; }
			set {
				if (isInitPhase) {
					initRowState = value;
					return;
				}

				if (value == rowState)
					return;

				rowState = value;
				if (!inEndInit) {
					UpdateIndex (true);
					OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1, -1));
				}
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the order in which data is returned by this DataView.")]
#endif
		[DefaultValue ("")]
		public string Sort {
			get {
				if (useDefaultSort)
					return String.Empty;
				else
					return sort;
			}
			set {
				if (isInitPhase) {
					initSort = value;
					return;
				}
				if (value == Sort)
					return;

				if (value == null || value.Length == 0) {
				/* if given value is null useDefaultSort */
					useDefaultSort = true;
					/* if ApplyDefault sort is true try appling it */
					if (ApplyDefaultSort)
						PopulateDefaultSort ();
				} else {
					/* else donot useDefaultSort. set it as false */
					/* sort is set to value specified */
					useDefaultSort = false;
					sort = value;
					//sortedColumns = SortableColumn.ParseSortString (dataTable, value, true);
				}

				if (!inEndInit) {
					UpdateIndex (true);
					OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1, -1));
				}
			}
		}

		[TypeConverter (typeof (DataTableTypeConverter))]
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the table this DataView uses to get data.")]
#endif
		[DefaultValue (null)]
		[RefreshProperties (RefreshProperties.All)]
		public DataTable Table {
			get { return dataTable; }
			set {
				if (value == dataTable)
					return;

				if (isInitPhase) {
					initTable = value;
					return;
				}

				if (value != null && value.TableName.Equals(string.Empty)) {
					throw new DataException("Cannot bind to DataTable with no name.");
				}

				if (dataTable != null)
					UnregisterEventHandlers ();

				dataTable = value;

				if (dataTable != null) {
					RegisterEventHandlers ();
					OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorChanged, 0, 0));
					sort = string.Empty;
					rowFilter = string.Empty;
					if (!inEndInit) {
						UpdateIndex (true);
						OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1, -1));
					}
				}
			}
		}

		public virtual DataRowView AddNew ()
		{
			if (!IsOpen)
				throw new DataException ("DataView is not open.");
			if (!AllowNew)
				throw new DataException ("Cannot call AddNew on a DataView where AllowNew is false.");

			if (_lastAdded != null)
				// FIXME : finish last added
				CompleteLastAdded (true);

			_lastAdded = dataTable.NewRow ();
			UpdateIndex (true);
			OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, Count - 1, -1));

			return this [Count - 1];
		}

		internal void CompleteLastAdded (bool add)
		{
			DataRow dr = _lastAdded;

			if (add) {
				try {
					dataTable.Rows.Add (_lastAdded);
					//OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, Count - 1, -1));
					_lastAdded = null;
					UpdateIndex ();
				} catch (Exception) {
					_lastAdded = dr;
					throw;
				}
			} else {
				_lastAdded.CancelEdit ();
				_lastAdded = null;
				UpdateIndex ();
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, Count, -1));
			}
		}

		public void BeginInit ()
		{
			initTable = Table;
			initApplyDefaultSort = ApplyDefaultSort;
			initSort = Sort;
			initRowFilter = RowFilter;
			initRowState = RowStateFilter;

			isInitPhase = true;
			DataViewInitialized (false);
		}

		partial void DataViewInitialized (bool value);

		public void CopyTo (Array array, int index)
		{
			if (index + rowCache.Length > array.Length)
				throw new IndexOutOfRangeException ();

			int row = 0;
			for (; row < rowCache.Length && row < array.Length; row++)
				array.SetValue (rowCache [row], index + row);
		}

		public void Delete (int index)
		{
			if (!IsOpen)
				throw new DataException ("DataView is not open.");

			if (_lastAdded != null && index == Count) {
				CompleteLastAdded (false);
				return;
			}

			if (!AllowDelete)
				throw new DataException ("Cannot delete on a DataSource where AllowDelete is false.");

			if (index > rowCache.Length)
				throw new IndexOutOfRangeException ("There is no row at position: " + index + ".");
			DataRowView row = rowCache [index];
			row.Row.Delete ();
		}

		public void EndInit ()
		{
			isInitPhase = false;

			inEndInit = true;

			Table = initTable;
			ApplyDefaultSort = initApplyDefaultSort;
			Sort = initSort;
			RowFilter = initRowFilter;
			RowStateFilter = initRowState;

			inEndInit = false;

			UpdateIndex (true);

			DataViewInitialized (true);
		}

		public int Find (object key)
		{
			object [] keys = new object[] { key };
			return Find (keys);
		}

		public int Find (object [] key)
		{
			if (sort == null || sort.Length == 0)
				throw new ArgumentException ("Find finds a row based on a Sort order, and no Sort order is specified");

			if (Index == null)
				UpdateIndex (true);

			int index = -1;
			try {
				index = Index.FindIndex (key);
			} catch (FormatException) {
				// suppress exception
			} catch (InvalidCastException) {
				// suppress exception
			}
			return index;
		}

		public DataRowView [] FindRows (object key)
		{
			return FindRows (new object[] {key});
		}

		public DataRowView [] FindRows (object [] key)
		{
			if (sort == null || sort.Length == 0)
				throw new ArgumentException ("Find finds a row based on a Sort order, and no Sort order is specified");

			if (Index == null)
				UpdateIndex (true);

			int [] indexes = Index.FindAllIndexes (key);

			DataRowView[] rowViewArr = new DataRowView [indexes.Length];
			for (int r = 0; r < indexes.Length; r++)
				rowViewArr [r] = rowCache [indexes[r]];
			return rowViewArr;
		}

		public IEnumerator GetEnumerator ()
		{
			DataRowView[] dataRowViews = new DataRowView [Count];
			CopyTo (dataRowViews, 0);
			return dataRowViews.GetEnumerator ();
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates that the data returned by this DataView has somehow changed.")]
#endif
		public event ListChangedEventHandler ListChanged;

		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("Indicates whether the view is open.  ")]
#endif
		protected bool IsOpen {
			get { return isOpen; }
		}

		internal Index Index {
			get { return _index; }
			set {
				if (_index != null) {
					_index.RemoveRef ();
					Table.DropIndex (_index);
				}

				_index = value;

				if (_index != null)
					_index.AddRef ();
			}
		}

		protected void Close ()
		{
			if (dataTable != null)
				UnregisterEventHandlers ();
			Index = null;
			rowCache = new DataRowView [0];
			isOpen = false;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				Close ();

			base.Dispose (disposing);
		}

		protected virtual void IndexListChanged (object sender, ListChangedEventArgs e)
		{
		}

		protected virtual void OnListChanged (ListChangedEventArgs e)
		{
			// Yes, under MS.NET, when it is overriden, the
			// events are not fired (even if it is essential
			// to internal processing).
			try {
				if (ListChanged != null)
					ListChanged (this, e);
			} catch {
			}
		}

		internal void ChangedList (ListChangedType listChangedType, int newIndex,int oldIndex)
		{
			ListChangedEventArgs e = new ListChangedEventArgs (listChangedType,newIndex,oldIndex);
			OnListChanged (e);
		}

		protected void Open ()
		{
			// I wonder if this comment is still valid, but keep
			// in the meantime.

			// FIXME: create the initial index cache to the DataTable, and
			//        only refresh the index when the DataTable
			//        has changes via column, row, or constraint
			//        changed events. the index cache is generally
			//        a DataViewRow array that points to the actual
			//        DataRows in the this DataTable's DataRowCollection;
			//        this index is really a cache that gets
			//        created during Open(), gets Updated
			//        when various properties of this view
			//        changes, gets Updated when this DataTable's
			//        row, column, or constraint collections have changed.
			//        I'm not sure what else.
			//        The data view will know one of the DataTable's
			//        collections have changed via one of
			//        its changed events.
			//        Otherwise, if getting a/the DataRowView(s),
			//        Count, or other properties, then just use the
			//        index cache.
			//		dataTable.ColumnChanged  += new DataColumnChangeEventHandler(OnColumnChanged);

			UpdateIndex (true);
			if (dataTable != null)
				RegisterEventHandlers ();
			isOpen = true;
		}

		private void RegisterEventHandlers ()
		{
			//dataTable.ColumnChanging += new DataColumnChangeEventHandler(OnColumnChanging);
			dataTable.ColumnChanged  += new DataColumnChangeEventHandler(OnColumnChanged);
			dataTable.RowChanged     += new DataRowChangeEventHandler(OnRowChanged);
			//dataTable.RowDeleting    += new DataRowChangeEventHandler(OnRowDeleting);
			dataTable.RowDeleted     += new DataRowChangeEventHandler(OnRowDeleted);
			dataTable.Columns.CollectionChanged += new CollectionChangeEventHandler(ColumnCollectionChanged);
			dataTable.Columns.CollectionMetaDataChanged += new CollectionChangeEventHandler(ColumnCollectionChanged);
			dataTable.Constraints.CollectionChanged += new CollectionChangeEventHandler(OnConstraintCollectionChanged);
			dataTable.ChildRelations.CollectionChanged += new CollectionChangeEventHandler(OnRelationCollectionChanged);
			dataTable.ParentRelations.CollectionChanged += new CollectionChangeEventHandler(OnRelationCollectionChanged);

			dataTable.Rows.ListChanged += new ListChangedEventHandler (OnRowCollectionChanged);
		}

		private void OnRowCollectionChanged (object sender, ListChangedEventArgs args)
		{
			if (args.ListChangedType == ListChangedType.Reset) {
				rowCache = new DataRowView [0];
				UpdateIndex (true);
				OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1, -1 ));
			}
		}

		private void UnregisterEventHandlers ()
		{
//			dataTable.ColumnChanging -= new DataColumnChangeEventHandler(OnColumnChanging);
			dataTable.ColumnChanged  -= new DataColumnChangeEventHandler(OnColumnChanged);
			dataTable.RowChanged     -= new DataRowChangeEventHandler(OnRowChanged);
//			dataTable.RowDeleting    -= new DataRowChangeEventHandler(OnRowDeleting);
			dataTable.RowDeleted     -= new DataRowChangeEventHandler(OnRowDeleted);
			dataTable.Columns.CollectionChanged -= new CollectionChangeEventHandler(ColumnCollectionChanged);
			dataTable.Columns.CollectionMetaDataChanged -= new CollectionChangeEventHandler(ColumnCollectionChanged);
			dataTable.Constraints.CollectionChanged -= new CollectionChangeEventHandler(OnConstraintCollectionChanged);
			dataTable.ChildRelations.CollectionChanged -= new CollectionChangeEventHandler(OnRelationCollectionChanged);
			dataTable.ParentRelations.CollectionChanged -= new CollectionChangeEventHandler(OnRelationCollectionChanged);

			dataTable.Rows.ListChanged -= new ListChangedEventHandler (OnRowCollectionChanged);
		}

		// These index storing and rowView preservation must be done
		// before the actual row value is changed; thus we can't use
		// RowChanging which accepts "already modified" DataRow.

		private void OnColumnChanged (object sender, DataColumnChangeEventArgs args)
		{	/* not used */
			//UpdateIndex(true);
		}

		private void OnRowChanged (object sender, DataRowChangeEventArgs args)
		{
			int oldIndex,newIndex;
			oldIndex = newIndex = -1;
			oldIndex = IndexOf (args.Row);
			UpdateIndex (true);
			newIndex = IndexOf (args.Row);

			/* ItemAdded */
			if (args.Action == DataRowAction.Add && oldIndex != newIndex)
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, newIndex, -1));

			/* ItemChanged or ItemDeleted */
			if (args.Action == DataRowAction.Change) {
				if (oldIndex != -1 && oldIndex == newIndex)
					OnListChanged (new ListChangedEventArgs (ListChangedType.ItemChanged, newIndex, -1));
				else if (oldIndex != newIndex) {
					if (newIndex < 0)
						OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, newIndex, oldIndex));
					else
						OnListChanged (new ListChangedEventArgs (ListChangedType.ItemMoved, newIndex, oldIndex));
				}
			}
			
			/* Rollback - ItemAdded or ItemDeleted */
			if (args.Action == DataRowAction.Rollback) {
				if (oldIndex < 0 && newIndex > -1)
					OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, newIndex, -1));
				else if (oldIndex > -1 && newIndex < 0)
					OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, newIndex, oldIndex));
				else if (oldIndex != -1 && oldIndex == newIndex)
					OnListChanged (new ListChangedEventArgs (ListChangedType.ItemChanged, newIndex, -1));
			}
		}

		private void OnRowDeleted (object sender, DataRowChangeEventArgs args)
		{
			/* ItemDeleted */
			int newIndex, oldCount;
			oldCount = Count;
			newIndex = IndexOf (args.Row);
			UpdateIndex (true);
			/* Fire ListChanged only when the RowFilter is affected */
			if (oldCount != Count)
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, newIndex, -1));
		}

		protected virtual void ColumnCollectionChanged (object sender, CollectionChangeEventArgs e)
		{
			// UpdateIndex() is not invoked here (even if the sort
			// column is being removed).

			// PropertyDescriptor Add
			if (e.Action == CollectionChangeAction.Add)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorAdded, 0, 0));

			// PropertyDescriptor Removed
			if (e.Action == CollectionChangeAction.Remove)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorDeleted, 0, 0));

			// PropertyDescriptor Changed
			if (e.Action == CollectionChangeAction.Refresh)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorChanged, 0, 0));
		}

		private void OnConstraintCollectionChanged (object sender, CollectionChangeEventArgs args)
		{
			//	The Sort variable is set to the UniqueConstraint column.
			//  if ApplyDefault Sort is true and Sort is null or is not set Explicitly

			// FIXME: The interal cache may change as result of change in Constraint collection
			// one such scenerio is taken care.
			// There may be more. I dont know what else can be done.
			/* useDefaultSort is set to false when Sort is set explicitly */
			if (args.Action == CollectionChangeAction.Add && args.Element is UniqueConstraint) {
				if (ApplyDefaultSort && useDefaultSort)
					PopulateDefaultSort ((UniqueConstraint) args.Element);
			}
			// UpdateIndex() is not invoked here.
		}

		private void OnRelationCollectionChanged (object sender, CollectionChangeEventArgs args)
		{
			/* PropertyDescriptor Add */
			if (args.Action == CollectionChangeAction.Add)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorAdded,0,0));

			/* PropertyDescriptor Removed */
			if (args.Action == CollectionChangeAction.Remove)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorDeleted,0,0));

			/* FIXME: PropertyDescriptor Changed ???*/
			if (args.Action == CollectionChangeAction.Refresh)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorChanged,0,0));
		}

		// internal use by Mono
		protected void Reset ()
		{
			// TODO: what really happens?
			Close ();
			rowCache = new DataRowView [0];
			Open ();
			OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1, -1 ));
		}

		protected void UpdateIndex ()
		{
			UpdateIndex (false);
		}

		// This is method is internal to
		// the Mono implementation of DataView; it
		// is not to be used from your code.
		//
		// Update the DataRowView array which is an index cache
		// into the DataTable's DataRowCollection.
		//
		// I assume this is what UpdateIndex is used for
		protected virtual void UpdateIndex (bool force)
		{
			if (Table == null)
				// FIXME
				return;

			if (Index == null || force) {
				sortColumns = DataTable.ParseSortString(Table, Sort, out sortOrder, false);
				Index = dataTable.GetIndex(sortColumns,sortOrder,RowStateFilter,FilterExpression,true);
			} else {
				Index.Key.RowStateFilter = RowStateFilter;
				Index.Reset();
			}

			int[] records = Index.GetAll ();

			if (records != null)
				InitDataRowViewArray (records,Index.Size);
			else
				rowCache = new DataRowView [0];
		}

		internal virtual IExpression FilterExpression {
			get { return rowFilterExpr; }
		}

		private void InitDataRowViewArray (int [] records, int size)
		{
			if (_lastAdded != null)
				rowCache = new DataRowView [size + 1];
			else
				rowCache = new DataRowView [size];

			for (int r = 0; r < size; r++)
				rowCache [r] = new DataRowView (this, Table.RecordCache [records [r]],r);

			if (_lastAdded != null)
				rowCache [size] = new DataRowView (this, _lastAdded, size);
		}

		PropertyDescriptorCollection ITypedList.GetItemProperties (PropertyDescriptor [] listAccessors)
		{
			if (dataTable == null)
				return new PropertyDescriptorCollection (new PropertyDescriptor [0]);

			// FIXME: use listAccessors somehow
			PropertyDescriptor [] descriptors =
				new PropertyDescriptor [dataTable.Columns.Count + dataTable.ChildRelations.Count];

			int d = 0;
			for (int col = 0; col < dataTable.Columns.Count; col ++) {
				DataColumn dataColumn = dataTable.Columns[col];
				DataColumnPropertyDescriptor descriptor;

				descriptor = new DataColumnPropertyDescriptor (dataColumn.ColumnName, col, null);
				descriptor.SetComponentType (typeof (System.Data.DataRowView));
				descriptor.SetPropertyType (dataColumn.DataType);
				descriptor.SetReadOnly (dataColumn.ReadOnly);
				descriptor.SetBrowsable (dataColumn.ColumnMapping != MappingType.Hidden);
				descriptors [d++] = descriptor;
			}
			for (int rel = 0; rel < dataTable.ChildRelations.Count; rel ++) {
				DataRelation dataRelation = dataTable.ChildRelations [rel];
				DataRelationPropertyDescriptor descriptor;

				descriptor = new DataRelationPropertyDescriptor (dataRelation);
				descriptors [d++] = descriptor;
			}

			return new PropertyDescriptorCollection (descriptors);
		}


		private int IndexOf (DataRow dr)
		{
			for (int i=0; i < rowCache.Length; i++)
				if (dr.Equals (rowCache [i].Row))
				return i;
			return -1;
		}

		private void PopulateDefaultSort ()
		{
			sort = string.Empty;
			foreach (Constraint c in dataTable.Constraints) {
				if (c is UniqueConstraint) {
					PopulateDefaultSort ((UniqueConstraint) c);
					break;
				}
			}
		}

		private void PopulateDefaultSort (UniqueConstraint uc)
		{
			if (isInitPhase)
				return;

			DataColumn [] columns = uc.Columns;
			if (columns.Length == 0) {
				sort = String.Empty;
				return;
			}

			StringBuilder builder = new StringBuilder ();
			builder.Append (columns[0].ColumnName);
			for (int i = 1; i < columns.Length; i++) {
				builder.Append (", ");
				builder.Append (columns [i].ColumnName);
			}
			sort = builder.ToString ();
		}

		internal DataView CreateChildView (DataRelation relation, int index)
		{
			if (relation == null || relation.ParentTable != Table)
				throw new ArgumentException("The relation is not parented to the table to which this DataView points.");

			int record = GetRecord (index);
			object[] keyValues = new object [relation.ParentColumns.Length];
			for (int i = 0; i < relation.ParentColumns.Length; i++)
				keyValues [i] = relation.ParentColumns [i] [record];

			return new RelatedDataView (relation.ChildColumns, keyValues);
		}

		private int GetRecord (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException(String.Format("There is no row at position {0}.", index));

			return (index == Index.Size) ?
				_lastAdded.IndexFromVersion (DataRowVersion.Default) :
				Index.IndexToRecord (index);
		}

		internal DataRowVersion GetRowVersion (int index)
		{
			int record = GetRecord (index);
			return Table.RecordCache [record].VersionFromIndex (record);
		}
	}

	partial class DataView : ITypedList {
		string ITypedList.GetListName (PropertyDescriptor [] listAccessors)
		{
			if (dataTable != null)
				return dataTable.TableName;
			return string.Empty;
		}
	}

	partial class DataView : ICollection {
		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}
	}

	partial class DataView : IList {
		bool IList.IsFixedSize {
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}

		object IList.this [int recordIndex] {
			get { return this [recordIndex]; }
			[MonoTODO]
			set { throw new InvalidOperationException (); }
		}

		int IList.Add (object value)
		{
			throw new ArgumentException ("Cannot add external objects to this list.");
		}

		void IList.Clear ()
		{
			throw new ArgumentException ("Cannot clear this list.");
		}

		bool IList.Contains (object value)
		{
			DataRowView drv = value as DataRowView;
			if (drv == null)
				return false;

			return drv.DataView == this;
		}

		int IList.IndexOf (object value)
		{
			DataRowView drv = value as DataRowView;
			if (drv != null && drv.DataView == this)
				return drv.Index;
			return -1;
		}

		void IList.Insert (int index,object value)
		{
			throw new ArgumentException ("Cannot insert external objects to this list.");
		}

		void IList.Remove (object value)
		{
			DataRowView drv = value as DataRowView;
			if (drv != null && drv.DataView == this)
				((IList) this).RemoveAt (drv.Index);

			throw new ArgumentException ("Cannot remove external objects to this list.");
		}

		void IList.RemoveAt (int index)
		{
			Delete (index);
		}
	}

	partial class DataView : IBindingList {
		[MonoTODO]
		void IBindingList.AddIndex (PropertyDescriptor property)
		{
			throw new NotImplementedException ();
		}

		object IBindingList.AddNew ()
		{
			return this.AddNew ();
		}

		void IBindingList.ApplySort (PropertyDescriptor property, ListSortDirection direction)
		{
			if (!(property is DataColumnPropertyDescriptor))
				throw new ArgumentException ("Dataview accepts only DataColumnPropertyDescriptors", "property");
			sortProperty = property;
			string sort = String.Format ("[{0}]" , property.Name);
			if (direction == ListSortDirection.Descending)
				sort += " DESC";
			this.Sort = sort;
		}

		int IBindingList.Find (PropertyDescriptor property, object key)
		{
			DataColumn dc = Table.Columns [property.Name];
			Index index = Table.FindIndex (new DataColumn [] {dc}, sortOrder, RowStateFilter, FilterExpression);
			if (index == null)
				index = new Index (new Key (Table, new DataColumn [] {dc}, sortOrder, RowStateFilter, FilterExpression));

			return index.FindIndex (new object [] {key});
		}

		[MonoTODO]
		void IBindingList.RemoveIndex (PropertyDescriptor property)
		{
			throw new NotImplementedException ();
		}

		void IBindingList.RemoveSort ()
		{
			sortProperty = null;
			this.Sort = String.Empty;
		}

		bool IBindingList.AllowEdit {
			get { return AllowEdit; }
		}

		bool IBindingList.AllowNew {
			get { return AllowNew; }
		}

		bool IBindingList.AllowRemove {
			[MonoTODO]
			get { return AllowDelete; }
		}

		bool IBindingList.IsSorted {
			get { return (Sort != null && Sort.Length != 0); }
		}

		ListSortDirection IBindingList.SortDirection {
			get {
				if (sortOrder != null && sortOrder.Length > 0)
					return sortOrder [0];
				return ListSortDirection.Ascending;
			}
		}

		PropertyDescriptor IBindingList.SortProperty {
			get {
				if (sortProperty == null && sortColumns != null && sortColumns.Length > 0) {
					// return property from Sort String
					PropertyDescriptorCollection properties = ((ITypedList)this).GetItemProperties (null);
					return properties.Find (sortColumns [0].ColumnName, false);
				}
				return sortProperty;
			}
		}

		bool IBindingList.SupportsChangeNotification {
			get { return true; }
		}

		bool IBindingList.SupportsSearching {
			get { return true; }
		}

		bool IBindingList.SupportsSorting {
			get { return true; }
		}
	}

#if NET_2_0
	partial class DataView : IBindingListView {
		string IBindingListView.Filter {
			get { return ((DataView) this).RowFilter; }
			set { ((DataView) this).RowFilter = value; }
		}

		ListSortDescriptionCollection IBindingListView.SortDescriptions {
			get {
				ListSortDescriptionCollection col = new ListSortDescriptionCollection ();
				for (int i = 0; i < sortColumns.Length; ++i) {
					ListSortDescription ldesc = new ListSortDescription (
										new DataColumnPropertyDescriptor (sortColumns [i]),
										sortOrder [i]);
					((IList) col).Add (ldesc);
				}
				return col;
			}
		}

		bool IBindingListView.SupportsAdvancedSorting {
			get { return true; }
		}

		bool IBindingListView.SupportsFiltering {
			get { return true; }
		}

		[MonoTODO]
		void IBindingListView.ApplySort (ListSortDescriptionCollection sorts)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (ListSortDescription ldesc in sorts)
				sb.AppendFormat ("[{0}]{1},", ldesc.PropertyDescriptor.Name,
					(ldesc.SortDirection == ListSortDirection.Descending ? " DESC" : string.Empty));
			this.Sort = sb.ToString (0, sb.Length-1);
		}

		void IBindingListView.RemoveFilter ()
		{
			((IBindingListView) this).Filter = string.Empty;
		}
	}

	partial class DataView : ISupportInitializeNotification {
		private bool dataViewInitialized = true;

		[Browsable (false)]
		public bool IsInitialized {
			get { return dataViewInitialized; }
		}

		public event EventHandler Initialized;

		partial void DataViewInitialized (bool value)
		{
			dataViewInitialized = value;
			if (value)
				OnDataViewInitialized (new EventArgs ());
		}

		private void OnDataViewInitialized (EventArgs e)
		{
			if (null != Initialized)
				Initialized (this, e);
		}
	}

	partial class DataView {
		public virtual bool Equals (DataView view)
		{
			if (this == view)
				return true;
			if (!(this.Table == view.Table && this.Sort == view.Sort &&
				this.RowFilter == view.RowFilter &&
				this.RowStateFilter == view.RowStateFilter &&
				this.AllowEdit == view.AllowEdit &&
				this.AllowNew == view.AllowNew &&
				this.AllowDelete == view.AllowDelete &&
				this.Count == view.Count))
				return false;

			for (int i = 0; i < Count; ++i)
				if (!this [i].Equals (view [i]))
					return false;
			return true;
		}
		public DataTable ToTable ()
		{
			return this.ToTable (Table.TableName, false, new string[] {});
		}

		public DataTable ToTable (string tableName)
		{
			return this.ToTable (tableName, false, new string[] {});
		}

		public DataTable ToTable (bool distinct, params string[] columnNames)
		{
			return this.ToTable (Table.TableName, distinct, columnNames);
		}

		public DataTable ToTable (string tableName, bool distinct, params string[] columnNames)
		{
			if (columnNames == null)
				throw new ArgumentNullException ("columnNames", "'columnNames' argument cannot be null.");

			DataTable newTable = new DataTable (tableName);

			DataColumn[] columns;
			ListSortDirection[] sortDirection = null;
			if (columnNames.Length > 0) {
				columns = new DataColumn [columnNames.Length];
				for (int i=0; i < columnNames.Length; ++i)
					columns [i] = Table.Columns [columnNames [i]];

				if (sortColumns != null) {
					sortDirection = new ListSortDirection [columnNames.Length];
					for (int i=0; i < columnNames.Length; ++i) {
						sortDirection [i] = ListSortDirection.Ascending;
						for (int j = 0; j < sortColumns.Length; ++j) {
							if (sortColumns [j] != columns [i])
								continue;
							sortDirection [i] = sortOrder [j];
						}
					}
				}
			} else {
				columns = (DataColumn[]) Table.Columns.ToArray (typeof (DataColumn));
				sortDirection = sortOrder;
			}

			ArrayList expressionCols = new ArrayList ();
			for (int i = 0; i < columns.Length; ++i) {
				DataColumn col = columns [i].Clone ();
				if (col.Expression != String.Empty) {
					col.Expression = string.Empty;
					expressionCols.Add (col);
				}
				if (col.ReadOnly)
					col.ReadOnly = false;
				newTable.Columns.Add (col);
			}

			DataRow [] rows;

			// Get the index from index collection of the data table.
			Index index = null;
			if (sort != string.Empty)
				index = Table.GetIndex(sortColumns,sortOrder,RowStateFilter,FilterExpression,true);
			else
				index = new Index (new Key(Table, columns, sortDirection, RowStateFilter, rowFilterExpr));
			
			if (distinct)
				rows = index.GetDistinctRows ();
			else
				rows = index.GetAllRows ();

			foreach (DataRow row in rows) {
				DataRow newRow = newTable.NewNotInitializedRow ();
				newTable.Rows.AddInternal (newRow);
				newRow.Original = -1;
				if (row.HasVersion (DataRowVersion.Current))
					newRow.Current = newTable.RecordCache.CopyRecord (Table, row.Current, -1);
				else if (row.HasVersion (DataRowVersion.Original))
					newRow.Current = newTable.RecordCache.CopyRecord (Table, row.Original, -1);

				foreach (DataColumn col in expressionCols)
					newRow [col] = row [col.ColumnName];
				newRow.Original = -1;
			}
			return newTable;
		}
	}
#endif
}
