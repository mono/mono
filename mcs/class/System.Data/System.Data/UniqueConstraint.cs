//
// System.Data.UniqueConstraint.cs
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Daniel Morgan <danmorg@sc.rr.com>
//   
// (C) 2002 Franklin Wise
// (C) 2002 Daniel Morgan

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

		public override bool Equals(object key2) {
			UniqueConstraint cst = key2 as UniqueConstraint;
			if (null == cst) return false;

			//according to spec if the cols are equal
			//then two UniqueConstraints are equal
			bool found = false;
			foreach (DataColumn thisCol in this.Columns) {
				found = false;
				foreach (DataColumn col in cst.Columns) {
					if (thisCol == col)
						found = true;
				}
				if (false == found) return false;
			}

			//if we got here then all columns were found
			return true;
			
		}

		[MonoTODO]
		public override int GetHashCode() {
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
