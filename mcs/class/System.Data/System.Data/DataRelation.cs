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
using System.ComponentModel;
using System.Runtime.Serialization;

namespace System.Data
{
	/// <summary>
	/// DataRelation is used for a parent/child relationship 
	/// between two DataTable objects
	/// </summary>
	[DefaultProperty ("RelationName")]
	[Serializable]
	public class DataRelation {

		#region Constructors

		[MonoTODO]
		public DataRelation (string relationName, DataColumn parentColumn, DataColumn childColumn) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRelation (string relationName, DataColumn[] parentColumns, DataColumn[] childColumns) 
		{

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRelation (string relationName, DataColumn parentColumn, DataColumn childColumn, bool createConstraints) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRelation (string relationName, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[Browsable (false)]
		public DataRelation (string relationName, string parentTableName, string childTableName, string[] parentColumnNames, string[] childColumnNames, bool nested) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the child columns of this relation.")]
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

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual DataSet DataSet {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds custom user information.")]
		public PropertyCollection ExtendedProperties {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether relations are nested.")]
		[DefaultValue (false)]
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

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the parent columns of this relation.")]
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

		[DataCategory ("Data")]
		[DataSysDescription ("The name used to look up this relation in the Relations collection of a DataSet.")]
		[DefaultValue ("")]
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
		protected void CheckStateForProperty () 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		protected internal void OnPropertyChanging (PropertyChangedEventArgs pcevent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void RaisePropertyChanging (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString () 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
