//
// System.Data.DataViewSettingCollection.cs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Miguel de Icaza (miguel@gnome.org)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data {
	/// <summary>
	/// Contains a read-only collection of DataViewSetting objects for each DataTable in a DataSet.
	/// </summary>
	[Editor]
	[Serializable]
	public class DataViewSettingCollection : ICollection, IEnumerable 
	{
		#region Fields

		ArrayList settingList;

		#endregion // Fields

		#region Constructors

		internal DataViewSettingCollection (DataViewManager manager)
		{
		}

		#endregion // Constructors

		#region Properties
	
		[Browsable (false)]	
		public virtual int Count {
			get { return settingList.Count; }
		}

		[Browsable (false)]	
		public bool IsReadOnly {
			get { return settingList.IsReadOnly; }
		}

		[Browsable (false)]	
		public bool IsSynchronized {
			get { return settingList.IsSynchronized; }
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
			get { return (DataViewSetting) settingList[index]; }
			set { settingList[index] = value; }
		}

		[Browsable (false)]	
		public object SyncRoot {
			get { return settingList.SyncRoot; }
		}

		#endregion // Properties

		#region Methods

		public void CopyTo (Array ar, int index) 
		{
			settingList.CopyTo (ar, index);
		}

		public IEnumerator GetEnumerator () 
		{
			return settingList.GetEnumerator ();
		}

		#endregion // Methods
	}
}
