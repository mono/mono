//
// System.Data.DataViewManager
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Contains a default DataViewSettingCollection for each DataTable in a DataSet.
	/// </summary>
	public class DataViewManager : MarshalByValueComponent, IBindingList, ICollection, IList, ITypedList, IEnumerable
	{
		#region Fields

		DataSet dataSet;
		DataViewManagerListItemTypeDescriptor descriptor;

		#endregion // Fields

		#region Constructors

		public DataViewManager () 
		{
			dataSet = null;
		}

		public DataViewManager (DataSet ds) 
		{
			dataSet = ds;
		}

		#endregion // Constructors

		#region Properties

		[DataSysDescription ("Indicates the source of data for this DataViewManager.")]
		[DefaultValue (null)]
		public DataSet DataSet {
			get { return dataSet; }
			set { dataSet = value; }
		}

		[MonoTODO]
		[DataSysDescription ("Indicates the sorting/filtering/state settings for any table in the corresponding DataSet.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public string DataViewSettingCollectionString {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataViewSettingCollection DataViewSettings {
			get { throw new NotImplementedException (); }
		}

		int ICollection.Count {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool ICollection.IsSynchronized {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		object ICollection.SyncRoot {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IList.IsFixedSize {
			get { return true; }
		}

		bool IList.IsReadOnly {
			get { return true; }
		}

		object IList.this [int index] {
			get { 
				if (descriptor == null)
					descriptor = new DataViewManagerListItemTypeDescriptor (this);

				return descriptor;
			}

			set { throw new ArgumentException ("Not modifiable"); }
		}

		bool IBindingList.AllowEdit {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.AllowNew {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.AllowRemove {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.IsSorted {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		ListSortDirection IBindingList.SortDirection {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		PropertyDescriptor IBindingList.SortProperty {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.SupportsChangeNotification {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.SupportsSearching {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.SupportsSorting {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public DataView CreateDataView (DataTable table) 
		{
			return new DataView (table);
		}

		[MonoTODO]
		void IBindingList.AddIndex (PropertyDescriptor property)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		object IBindingList.AddNew ()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IBindingList.ApplySort (PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		int IBindingList.Find (PropertyDescriptor property, object key)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IBindingList.RemoveIndex (PropertyDescriptor property)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IBindingList.RemoveSort ()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		int IList.Add (object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IList.Clear ()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		bool IList.Contains (object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		int IList.IndexOf (object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IList.Insert (int index, object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IList.Remove (object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IList.RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}
	
		PropertyDescriptorCollection ITypedList.GetItemProperties (PropertyDescriptor[] listAccessors)
		{
			if (dataSet == null)
				throw new DataException ("dataset is null");

			if (listAccessors == null || listAccessors.Length == 0) {
				ICustomTypeDescriptor desc = new DataViewManagerListItemTypeDescriptor (this);
				return desc.GetProperties ();
			}
				
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		string ITypedList.GetListName (PropertyDescriptor[] listAccessors)
		{
			throw new NotImplementedException ();
		}
	
		protected virtual void OnListChanged (ListChangedEventArgs e) 
		{
			if (ListChanged != null)
				ListChanged (this, e);
		}

		protected virtual void RelationCollectionChanged (object sender, CollectionChangeEventArgs e) 
		{
		}

		protected virtual void TableCollectionChanged (object sender, CollectionChangeEventArgs e) 
		{
		}

		#endregion // Methods

		#region Events

		public event ListChangedEventHandler ListChanged;

		#endregion // Events
	}
}
