//
// System.Data.DataRowView.cs
//
// Author:
//    Rodrigo Moya <rodrigo@ximian.com>
//    Miguel de Icaza <miguel@ximian.com>
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002
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
using System.Reflection;

namespace System.Data
{
	/// <summary>
	/// Represents a customized view of a DataRow exposed as a fully featured Windows Forms control.
	/// </summary>
	public class DataRowView : ICustomTypeDescriptor, IEditableObject, IDataErrorInfo
	{
		#region Fields

		DataView dataView;
		DataRow dataRow;
		DataRowVersion rowVersion = DataRowVersion.Current;

		bool isNew;

		#endregion // Fields

		#region Constructors

		internal DataRowView (DataView dataView, DataRow row) : this(dataView, row, false){
		}

		internal DataRowView (DataView dataView, DataRow row, bool isNew) {
			this.dataView = dataView;
			this.dataRow = row;
			this.isNew = isNew;
		}

		#endregion // Constructors

		#region Methods

		public override bool Equals(object other)
		{
			return (other != null &&
					other is DataRowView && 
					((DataRowView)other).dataRow != null && 
					((DataRowView)other).dataRow.Equals(this.dataRow));
		}

		public void BeginEdit ()
		{
			dataRow.BeginEdit ();
		}

		public void CancelEdit ()
		{
			dataView.CancelEditRowView (this);
			isNew = false;
		}

		public DataView CreateChildView (DataRelation relation)
		{
			if (relation == null)
				throw new ArgumentException ("The relation is not parented to the table.");
			return new DataView (relation.ChildTable);
		}

		public DataView CreateChildView (string name)
		{
			return CreateChildView (
				dataRow.Table.ChildRelations [name]);
		}

		[MonoTODO]
		public void Delete ()
		{
			dataView.DeleteRowView (this);
			isNew = false;
		}

		public void EndEdit ()
		{
			dataView.EndEditRowView (this);
			isNew = false;
		}

		#endregion // Methods

		#region Properties
		
		public DataView DataView {
			get { return dataView; }
		}

		public bool IsEdit {
			get { return dataRow.IsEditing; }
		}

		// It becomes true when this instance is created by
		// DataView.AddNew(). If it is true, then the DataRow is
		// "Detached", and when this.EndEdit() is invoked, the row
		// will be added to the table.
		public bool IsNew {
			get { return isNew; }
		}
		
		[System.Runtime.CompilerServices.IndexerName("Item")]
		public object this[string column] {
			[MonoTODO]
			get {
				DataColumn dc = dataView.Table.Columns[column];
				return dataRow[dc];
			}
			[MonoTODO]
			set {
				DataColumn dc = dataView.Table.Columns[column];
				dataRow[dc] = value;
			}
		}

		// the compiler creates a DefaultMemeberAttribute from
		// this IndexerNameAttribute
		public object this[int column] {
			[MonoTODO]
			get {
				DataColumn dc = dataView.Table.Columns[column];
				return dataRow[dc];
			}
			[MonoTODO]
			set {
				DataColumn dc = dataView.Table.Columns[column];
				dataRow[dc] = value;

			}
		}

		public DataRow Row {
			get { return dataRow; }
		}

		public DataRowVersion RowVersion {
			[MonoTODO]
			get {
				return rowVersion;
			}
		}

		// It returns the hash code of the DataRow object.
		public override int GetHashCode ()
		{
			return dataRow.GetHashCode ();
		}

		#endregion // Properties
		
		#region ICustomTypeDescriptor implementations
		
		[MonoTODO]
		AttributeCollection ICustomTypeDescriptor.GetAttributes  ()
		{
			System.ComponentModel.AttributeCollection attributes;
			attributes = AttributeCollection.Empty;
			return attributes;
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetClassName ()
		{
			return "";
		}
		
		[MonoTODO]
		string ICustomTypeDescriptor.GetComponentName ()
		{
			return null;
		}

		[MonoTODO]
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			return null;
		}

		[MonoTODO]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			return null;
		}
		
		[MonoTODO]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			return null;
		}
		
		[MonoTODO]
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			return null;
		}
		
		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			return new EventDescriptorCollection(null);
		}

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute [] attributes)
		{
			return new EventDescriptorCollection(null);
		}

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			ITypedList typedList = (ITypedList) dataView;
			return typedList.GetItemProperties(new PropertyDescriptor[0]);
		}

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute [] attributes)
		{
			PropertyDescriptorCollection descriptors;
			descriptors = ((ICustomTypeDescriptor) this).GetProperties ();
			// TODO: filter out descriptors which do not contain
			//       any of those attributes
			//       except, those descriptors 
			//       that contain DefaultMemeberAttribute
			return descriptors;
		}
		
		[MonoTODO]
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			return this;
		}

		#endregion // ICustomTypeDescriptor implementations

		#region IDataErrorInfo implementation

		string IDataErrorInfo.Error {
			[MonoTODO]
			get {
				return ""; // FIXME
			}
		}

		string IDataErrorInfo.this[string columnName] {
			[MonoTODO]
			get {
				return ""; // FIXME
			}
		}

		#endregion // IDataErrorInfo implementation
	}
}
