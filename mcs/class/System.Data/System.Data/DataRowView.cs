//
// System.Data.DataRowView.cs
//
// Author:
//    Rodrigo Moya <rodrigo@ximian.com>
//    Miguel de Icaza <miguel@ximian.com>
//
// (C) Ximian, Inc 2002
//

using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Represents a customized view of a DataRow exposed as a fully featured Windows Forms control.
	/// </summary>
	public class DataRowView : ICustomTypeDescriptor, IEditableObject, IDataErrorInfo
	{
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
		
		public DataView DataView
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
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

		public string this[string column] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public DataRow Row {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public DataRowVersion RowVersion {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		//
		// ICustomTypeDescriptor implementations
		//

		[MonoTODO]
		AttributeCollection ICustomTypeDescriptor.GetAttributes  ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetClassName ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		string ICustomTypeDescriptor.GetComponentName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute [] attributes)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}
	}
}
