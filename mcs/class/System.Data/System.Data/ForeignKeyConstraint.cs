//
// System.Data.ForeignKeyConstraint.cs
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) 2002 Franklin Wise
// (C) 2002 Daniel Morgan
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Data
{

	[Serializable]
	public class ForeignKeyConstraint : Constraint {

		#region Constructors

		[MonoTODO]
		public ForeignKeyConstraint(DataColumn parentColumn, 
			DataColumn childColumn) {
		}

		[MonoTODO]
		public ForeignKeyConstraint(DataColumn[] parentColumns,
			DataColumn[] childColumns) {
		}

		[MonoTODO]
		public ForeignKeyConstraint(string constraintName,
			DataColumn parentColumn, DataColumn childColumn) {
		}

		[MonoTODO]
		public ForeignKeyConstraint(string constraintName,
			DataColumn[] parentColumns, 
			DataColumn[] childColumns) {
		}

		[MonoTODO]
		public ForeignKeyConstraint(string constraintName,
			string parentTableName,	string[] parentColumnNames,
			string[] childColumnNames, 
			AcceptRejectRule acceptRejectRule, Rule deleteRule,
			Rule updateRule) {
		}

		#endregion // Constructors

		#region Properites

		public virtual AcceptRejectRule AcceptRejectRule {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public virtual DataColumn[] Columns {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual Rule DeleteRule {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public virtual DataColumn[] RelatedColumns {
			[MonoTODO]
			get {	
				throw new NotImplementedException ();
			}
		}

		public virtual DataTable RelatedTable {
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

		public virtual Rule UpdateRule {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override bool Equals(object key) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode() {
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}

}
