//
// System.Data.UniqueConstraint.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) 2002 Daniel Morgan
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Data
{
	public class UniqueConstraint : Constraint {

		#region Constructors

		[MonoTODO]
		public UniqueConstraint(DataColumn column) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public UniqueConstraint(DataColumn[] columns) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public UniqueConstraint(DataColumn column,
			bool isPrimaryKey) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public UniqueConstraint(DataColumn[] columns, bool isPrimaryKey) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public UniqueConstraint(string name, DataColumn column) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public UniqueConstraint(string name, DataColumn[] columns) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public UniqueConstraint(string name, DataColumn column,
			bool isPrimaryKey) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public UniqueConstraint(string name,
			DataColumn[] columns, bool isPrimaryKey) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public UniqueConstraint(string name,
			string[] columnNames, bool isPrimaryKey) {

			throw new NotImplementedException ();
		}

		#endregion // Constructors
		
		#region Properties

		public virtual DataColumn[] Columns {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsPrimaryKey {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public override DataTable Table {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override bool Equals(object key2) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode() {
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
