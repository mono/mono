//
// System.Data.DataRelation.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) 2002 Daniel Morgan
// (C) 2002 Ximian, Inc.
//

using System;
using System.Runtime.Serialization;

namespace System.Data
{
	/// <summary>
	/// DataRelation is used for a parent/child relationship 
	/// between two DataTable objects
	/// </summary>
	[Serializable]
	public class DataRelation {

		#region Constructors

		[MonoTODO]
		public DataRelation(string relationName,
			DataColumn parentColumn, DataColumn childColumn) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRelation(string relationName,
			DataColumn[] parentColumns, 
			DataColumn[] childColumns) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRelation(string relationName,
			DataColumn parentColumn, DataColumn childColumn,
			bool createConstraints) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRelation(string relationName,
			DataColumn[] parentColumns, DataColumn[] childColumns,
			bool createConstraints) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRelation(string relationName,
			string parentTableName,	string childTableName,
			string[] parentColumnNames, string[] childColumnNames,
			bool nested) {

			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		public virtual DataColumn[] ChildColumns {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual ForeignKeyConstraint ChildKeyConstraint {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual DataTable ChildTable {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual DataSet DataSet {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public PropertyCollection ExtendedProperties {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual bool Nested {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public virtual DataColumn[] ParentColumns {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual UniqueConstraint ParentKeyConstraint {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual DataTable ParentTable {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual string RelationName {
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
		public override string ToString() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void CheckStateForProperty() {
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
