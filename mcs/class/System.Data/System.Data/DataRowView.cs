//
// System.Data.DataRowView.cs
//
// Author:
//    Rodrigo Moya <rodrigo@ximian.com>
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
		public void BeginEdit () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CancelEdit () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataView CreateChildView (DataRelation relation) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataView CreateChildView (string name) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Delete () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndEdit () {
			throw new NotImplementedException ();
		}
		
		public DataView DataView {
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

		public object this[string column] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
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
	}
}
