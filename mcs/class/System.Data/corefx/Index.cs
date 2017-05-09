namespace System.Data
{
	partial class Index
	{
		internal delegate int ComparisonBySelector<TKey,TRow>(TKey key, TRow row) where TRow:DataRow;

		/// <summary>This method exists for LinqDataView to keep a level of abstraction away from the RBTree</summary>
		internal Range FindRecords<TKey,TRow>(ComparisonBySelector<TKey,TRow> comparison, TKey key) where TRow:DataRow
		{
			int x = _records.root;
			while (IndexTree.NIL != x)
			{
				int c = comparison(key, (TRow)_table._recordManager[_records.Key(x)]);
				if (c == 0) { break; }
				if (c < 0) { x = _records.Left(x); }
				else { x = _records.Right(x); }
			}
			return GetRangeFromNode(x);
		}
	}
}