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
	//[DefaultMember("Item")]
	[DefaultProperty("Item")]
	public class DataRowView : ICustomTypeDescriptor, IEditableObject, IDataErrorInfo
	{
		#region Fields

		private DataView dataView;
		private DataRow dataRow;

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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CancelEdit ()
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}

		public bool IsNew {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
		
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
				throw new NotImplementedException ();
			}
		}

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
				throw new NotImplementedException ();
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
			//object[] attributes = this.GetType ().GetCustomAttributes (true);
			//AttributeCollection attribCollection;
			//attribCollection = new AttributeCollection (attributes);
			//return attribCollection;
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetClassName ()
		{
			return "";
		}
		
		[MonoTODO]
		string ICustomTypeDescriptor.GetComponentName ()
		{
			return "";
		}

		[MonoTODO]
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			return null;
		}

		[MonoTODO]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			return null;
		}
		
		[MonoTODO]
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute [] attributes)
		{
			throw new NotImplementedException ();
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
			// TODO: filter out any Attributes not in the attributes array
			return descriptors;
		}
		
		[MonoTODO]
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}

		#endregion // ICustomTypeDescriptor implementations

		#region IDataErrorInfo implementation

		string IDataErrorInfo.Error {
			[MonoTODO]
			get {
				throw new NotImplementedException();
			}
		}

		string IDataErrorInfo.this[string columnName] {
			[MonoTODO]
			get {
				throw new NotImplementedException();
			}
		}

		#endregion // IDataErrorInfo implementation
	}
}
