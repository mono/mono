namespace System.Data
{
	partial class DataView
	{
        /// <summary>
        /// Allow construction of DataView with <see cref="System.Predicate&lt;DataRow&gt;"/> and <see cref="System.Comparison&lt;DataRow&gt;"/>
        /// </summary>
        /// <remarks>This is a copy of the other DataView ctor and needs to be kept in sync</remarks>
        internal DataView(DataTable table, System.Predicate<DataRow> predicate, System.Comparison<DataRow> comparison, DataViewRowState RowState) {
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataView.DataView|API> %d#, table=%d, RowState=%d{ds.DataViewRowState}\n",
                           ObjectID, (table != null) ? table.ObjectID : 0, (int)RowState);
            if (table == null)
                throw ExceptionBuilder.CanNotUse();

            this._dvListener = new DataViewListener(this);
            this._locked = false;
            this._table = table;
            _dvListener.RegisterMetaDataEvents(this._table);

            if ((((int)RowState) &
                 ((int)~(DataViewRowState.CurrentRows | DataViewRowState.OriginalRows))) != 0) {
                throw ExceptionBuilder.RecordStateRange();
            }
            else if (( ((int)RowState) & ((int)DataViewRowState.ModifiedOriginal) ) != 0 &&
                     ( ((int)RowState) &  ((int)DataViewRowState.ModifiedCurrent) ) != 0
                    ) {
                throw ExceptionBuilder.SetRowStateFilter();
            }
            _comparison = comparison;
            SetIndex2("", RowState, ((null != predicate) ? new RowPredicateFilter(predicate) : null), true);
        }

		/// <summary>This method exists for LinqDataView to keep a level of abstraction away from the RBTree</summary>
		internal Range FindRecords<TKey, TRow>(Index.ComparisonBySelector<TKey, TRow> comparison, TKey key) where TRow : DataRow
		{
			return _index.FindRecords(comparison, key);
		}
	}
}