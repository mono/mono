
using System;
using System.Collections;

namespace System.Data {
	/// <summary>
	/// Contains a read-only collection of DataViewSetting objects for each DataTable in a DataSet.
	/// </summary>
	public class DataViewSettingCollection : ICollection, IEnumerable {
		private ArrayList settingList;

		public void CopyTo (Array ar, int index) {
			settingList.CopyTo (ar, index);
		}

		public IEnumerator GetEnumerator () {
			return settingList.GetEnumerator ();
		}
		
		public virtual int Count {
			get {
				return settingList.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return settingList.IsReadOnly;
			}
		}

		public bool IsSynchronized {
			get {
				return settingList.IsSynchronized;
			}
		}

		public virtual DataViewSetting this [DataTable dt] {
			get {
				for (int i = 0; i < settingList.Count; i++) {
					DataViewSetting dvs = (DataViewSetting) settingList[i];
					if (dvs.Table == dt)
						return dvs;
				}
				return null;
			}
			set {
				this[dt] = value;
			}
		}

		public virtual DataViewSetting this[string name] {
			get {
				for (int i = 0; i < settingList.Count; i++) {
					DataViewSetting dvs = (DataViewSetting) settingList[i];
					if (dvs.Table.TableName == name)
						return dvs;
				}
				return null;
			}
		}

		public virtual DataViewSetting this[int index] {
			get {
				return (DataViewSetting) settingList[index];
			}
			set {
				settingList[index] = value;
			}
		}

		public object SyncRoot {
			get {
				return settingList.SyncRoot;
			}
		}
	}
}
