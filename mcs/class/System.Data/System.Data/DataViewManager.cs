//
// System.Data.DataViewManager
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc. 2002
//

using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Contains a default DataViewSettingCollection for each DataTable in a DataSet.
	/// </summary>
	public class DataViewManager : MarshalByValueComponent
		//, IBindingList, ICollection, IList, ITypedList, IEnumerable
	{
		private DataSet dataSet;

		public DataViewManager () {
			dataSet = null;
		}

		public DataViewManager (DataSet ds) {
			dataSet = ds;
		}

		[MonoTODO]
		public DataView CreateDataView (DataTable table) {
			return new DataView (table);
		}

		protected virtual void OnListChanged (ListChangedEventArgs e) {
		}

		protected virtual void RelationCollectionChanged (
			object sender,
			CollectionChangeEventArgs e) {
		}

		protected virtual void TableCollectionChanged (object sender,
							       CollectionChangeEventArgs e) {
		}

		public DataSet DataSet {
			get {
				return dataSet;
			}
			set {
				dataSet = value;
			}
		}

		[MonoTODO]
		public string DataViewSettingCollectionString {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public DataViewSettingCollection DataViewSettings {
			get {
				throw new NotImplementedException ();
			}
		}

		public event ListChangedEventHandler ListChanged;
	}
}
