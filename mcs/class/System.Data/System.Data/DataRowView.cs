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
		DataRowVersion rowVersion = DataRowVersion.Default;

		// FIXME: what are the defaults?
		bool isEdit = false;
		bool isNew = false;

		#endregion // Fields

		#region Constructors

		internal DataRowView (DataView dataView, int rowIndex) {
			this.dataView = dataView;
			this.dataRow = dataView.Table.Rows[rowIndex];
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public void BeginEdit ()
		{
			// FIXME:
		}

		[MonoTODO]
		public void CancelEdit ()
		{
			// FIXME:
		}

		[MonoTODO]
		public DataView CreateChildView (DataRelation relation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataView CreateChildView (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Delete ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndEdit ()
		{
			// FIXME:
		}

		#endregion // Methods

		#region Properties
		
		public DataView DataView
		{
			[MonoTODO]
			get {
				return dataView;
			}
		}

		public bool IsEdit {
			[MonoTODO]
			get {
				return isEdit;
			}
		}

		public bool IsNew {
			[MonoTODO]
			get {
				return isNew;
			}
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

		public string Error {
			get {
				return ""; // FIXME:
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
			[MonoTODO]
			get {
				return dataRow;
			}
		}

		public DataRowVersion RowVersion {
			[MonoTODO]
			get {
				return rowVersion;
			}
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
