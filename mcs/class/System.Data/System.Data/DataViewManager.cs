//
// System.Data.DataViewManager
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//   Ivan N. Zlatev (contact@i-nz.net)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2005 Novell Inc,
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace System.Data
{
	/// <summary>
	/// Contains a default DataViewSettingCollection for each DataTable in a DataSet.
	/// </summary>
	//[Designer]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.DataViewManagerDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	public class DataViewManager : MarshalByValueComponent, IBindingList, ICollection, IList, ITypedList, IEnumerable
	{
		#region Fields

		DataSet dataSet;
		DataViewManagerListItemTypeDescriptor descriptor;
		DataViewSettingCollection settings;
		string xml;

		#endregion // Fields

		#region Constructors

		public DataViewManager ()
			: this (null)
		{
		}

		public DataViewManager (DataSet dataSet)
		{
			// Null argument is allowed here.
			SetDataSet (dataSet);
		}

		#endregion // Constructors

		#region Properties

#if !NET_2_0
		[DataSysDescription ("Indicates the source of data for this DataViewManager.")]
#endif
		[DefaultValue (null)]
		public DataSet DataSet {
			get { return dataSet; }
			set {
				if (value == null)
					throw new DataException ("Cannot set null DataSet.");
				SetDataSet (value);
			}
		}

		public string DataViewSettingCollectionString {
			get { return xml; }
			set {
				try {
					ParseSettingString (value);
					xml = BuildSettingString ();
				} catch (XmlException ex) {
					throw new DataException ("Cannot set DataViewSettingCollectionString.", ex);
				}
			}
		}

#if !NET_2_0
		[DataSysDescription ("Indicates the sorting/filtering/state settings for any table in the corresponding DataSet.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataViewSettingCollection DataViewSettings {
			get { return settings; }
		}

		int ICollection.Count {
			get { return 1; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
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
			get { return false; }
		}

		bool IBindingList.AllowNew {
			get { return false; }
		}

		bool IBindingList.AllowRemove {
			get { return false; }
		}

		bool IBindingList.IsSorted {
			get { throw new NotSupportedException (); }
		}

		ListSortDirection IBindingList.SortDirection {
			get { throw new NotSupportedException (); }
		}

		PropertyDescriptor IBindingList.SortProperty {
			get { throw new NotSupportedException (); }
		}

		bool IBindingList.SupportsChangeNotification {
			get { return true; }
		}

		bool IBindingList.SupportsSearching {
			get { return false; }
		}

		bool IBindingList.SupportsSorting {
			get { return false; }
		}

		#endregion // Properties

		#region Methods

		private void SetDataSet (DataSet ds)
		{
			if (dataSet != null) {
				dataSet.Tables.CollectionChanged -= new CollectionChangeEventHandler (TableCollectionChanged);
				dataSet.Relations.CollectionChanged -= new CollectionChangeEventHandler (RelationCollectionChanged);
			}

			dataSet = ds;
			settings = new DataViewSettingCollection (this);
			xml = BuildSettingString ();

			if (dataSet != null) {
				dataSet.Tables.CollectionChanged += new CollectionChangeEventHandler (TableCollectionChanged);
				dataSet.Relations.CollectionChanged += new CollectionChangeEventHandler (RelationCollectionChanged);
			}
		}

		private void ParseSettingString (string source)
		{
			XmlTextReader xtr = new XmlTextReader (source,
				XmlNodeType.Element, null);

			xtr.Read ();
			if (xtr.Name != "DataViewSettingCollectionString")
				// easy way to throw the expected exception ;-)
			xtr.ReadStartElement ("DataViewSettingCollectionString");
			if (xtr.IsEmptyElement)
				return; // MS does not change the value.

			xtr.Read ();
			do {
				xtr.MoveToContent ();
				if (xtr.NodeType == XmlNodeType.EndElement)
					break;
				if (xtr.NodeType == XmlNodeType.Element)
					ReadTableSetting (xtr);
				else
					xtr.Skip ();
			} while (!xtr.EOF);
			if (xtr.NodeType == XmlNodeType.EndElement)
				xtr.ReadEndElement ();
		}

		private void ReadTableSetting (XmlReader reader)
		{
			// Namespace is ignored BTW.
			DataTable dt = DataSet.Tables [XmlConvert.DecodeName (
				reader.LocalName)];
			// The code below might result in NullReference error.
			DataViewSetting s = settings [dt];
			string sort = reader.GetAttribute ("Sort");
			if (sort != null)
				s.Sort = sort.Trim ();
			string ads = reader.GetAttribute ("ApplyDefaultSort");
			if (ads != null && ads.Trim () == "true")
				s.ApplyDefaultSort = true;
			string rowFilter = reader.GetAttribute ("RowFilter");
			if (rowFilter != null)
				s.RowFilter = rowFilter.Trim ();
			string rsf = reader.GetAttribute ("RowStateFilter");
			if (rsf != null)
				s.RowStateFilter = (DataViewRowState)
					Enum.Parse (typeof (DataViewRowState), 
					rsf.Trim ());
			reader.Skip ();
		}

		private string BuildSettingString ()
		{
			if (dataSet == null)
				return String.Empty;

			StringWriter sw = new StringWriter ();
			sw.Write ('<');
			sw.Write ("DataViewSettingCollectionString>");
			foreach (DataViewSetting s in DataViewSettings) {
				sw.Write ('<');
				sw.Write (XmlConvert.EncodeName (
						s.Table.TableName));
				sw.Write (" Sort=\"");
				sw.Write (Escape (s.Sort));
				sw.Write ('"');
				// LAMESPEC: MS.NET does not seem to handle this property as expected.
				if (s.ApplyDefaultSort)
					sw.Write (" ApplyDefaultSort=\"true\"");
				sw.Write (" RowFilter=\"");
				sw.Write (Escape (s.RowFilter));
				sw.Write ("\" RowStateFilter=\"");
				sw.Write (s.RowStateFilter.ToString ());
				sw.Write ("\"/>");
			}
			sw.Write ("</DataViewSettingCollectionString>");
			return sw.ToString ();
		}

		private string Escape (string s)
		{
			return s.Replace ("&", "&amp;")
				.Replace ("\"", "&quot;")
				.Replace ("\'", "&apos;")
				.Replace ("<", "&lt;")
				.Replace (">", "&gt;");
		}

		public DataView CreateDataView (DataTable table)
		{
			if (settings [table] != null) {
				DataViewSetting s = settings [table];
				return new DataView (table, this, s.Sort, s.RowFilter, s.RowStateFilter);
			} else {
				return new DataView (table);
			}
		}

		void IBindingList.AddIndex (PropertyDescriptor property)
		{
		}
	
		object IBindingList.AddNew ()
		{
			throw new NotSupportedException ();
		}
	
		void IBindingList.ApplySort (PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotSupportedException ();
		}
	
		int IBindingList.Find (PropertyDescriptor property, object key)
		{
			throw new NotSupportedException ();
		}
	
		void IBindingList.RemoveIndex (PropertyDescriptor property)
		{
		}
	
		void IBindingList.RemoveSort ()
		{
			throw new NotSupportedException ();
		}
	
		void ICollection.CopyTo (Array array, int index)
		{
			array.SetValue (descriptor, index);
		}
	
		IEnumerator IEnumerable.GetEnumerator ()
		{
			DataViewManagerListItemTypeDescriptor[] array = new DataViewManagerListItemTypeDescriptor[((ICollection)this).Count];
			((ICollection)this).CopyTo (array, 0);
			return array.GetEnumerator ();
		}
	
		int IList.Add (object value)
		{
			throw new ArgumentException ("Not modifiable");
		}
	
		void IList.Clear ()
		{
			throw new ArgumentException ("Not modifiable");
		}
	
		bool IList.Contains (object value)
		{
			return value == descriptor;
		}
	
		int IList.IndexOf (object value)
		{
			if (value == descriptor)
				return 0;
			return -1;
		}
	
		void IList.Insert (int index, object value)
		{
			throw new ArgumentException ("Not modifiable");
		}
	
		void IList.Remove (object value)
		{
			throw new ArgumentException ("Not modifiable");
		}
	
		void IList.RemoveAt (int index)
		{
			throw new ArgumentException ("Not modifiable");
		}
	
		[MonoLimitation("Supported only empty list of listAccessors")]
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
	
		string ITypedList.GetListName (PropertyDescriptor[] listAccessors)
		{
			if (dataSet != null) {
				if (listAccessors == null || listAccessors.Length == 0)
					return  dataSet.DataSetName;
			}
			
			return string.Empty;
		}
	
		protected virtual void OnListChanged (ListChangedEventArgs e)
		{
			if (ListChanged != null)
				ListChanged (this, e);
		}

		protected virtual void RelationCollectionChanged (object sender, CollectionChangeEventArgs e)
		{
			this.OnListChanged (CollectionToListChangeEventArgs (e));
		}

		protected virtual void TableCollectionChanged (object sender, CollectionChangeEventArgs e)
		{
			this.OnListChanged (CollectionToListChangeEventArgs (e));
		}

		private ListChangedEventArgs CollectionToListChangeEventArgs (CollectionChangeEventArgs e)
		{
			ListChangedEventArgs args;

			if (e.Action == CollectionChangeAction.Remove)
				args = null;
			else if (e.Action == CollectionChangeAction.Refresh)
				args = new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, null);
			else { 
			       object obj;
			       
			       if (typeof (DataTable).IsAssignableFrom (e.Element.GetType()))
			               obj = new DataTablePropertyDescriptor ((DataTable) e.Element);
			       else // Assume a DataRelation
			               obj = new DataRelationPropertyDescriptor((DataRelation) e.Element);
			       
			       if (e.Action == CollectionChangeAction.Add)
			               args = new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, (PropertyDescriptor) obj);
			       else
			               args = new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, (PropertyDescriptor) obj);
			}
			
			return args;
		}

		#endregion // Methods

		#region Events

		public event ListChangedEventHandler ListChanged;

		#endregion // Events
	}
}
