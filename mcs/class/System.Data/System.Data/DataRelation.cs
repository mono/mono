//
// System.Data.DataRelation.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//   Alan Tam Siu Lung <Tam@SiuLung.com>
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
		private DataSet dataSet;
		private string relationName;
		private UniqueConstraint parentKeyConstraint;
		private ForeignKeyConstraint childKeyConstraint;
		private DataColumn[] parentColumns;
		private DataColumn[] childColumns;
		private bool nested;
		internal bool createConstraints;
		private PropertyCollection extendedProperties;
		private PropertyChangedEventHandler onPropertyChangingDelegate;

		#region Constructors

		public DataRelation (string relationName, DataColumn parentColumn, DataColumn childColumn) 
		: this(relationName, parentColumn, childColumn, true)
		{
		}

		public DataRelation (string relationName, DataColumn[] parentColumns, DataColumn[] childColumns) 
		: this(relationName, parentColumns, childColumns, true)
		{
		}

		public DataRelation (string relationName, DataColumn parentColumn, DataColumn childColumn, bool createConstraints)
		: this(relationName, new DataColumn[] { parentColumn }, new DataColumn[] { childColumn }, createConstraints)
		{
		}

		public DataRelation (string relationName, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints) 
		{
			this.extendedProperties = new PropertyCollection();
			if (relationName == null) relationName = "Relation";
			this.relationName = relationName;
			if (parentColumns == null) throw new ArgumentNullException ();
			this.parentColumns = parentColumns;
			if (childColumns == null) throw new ArgumentNullException ();
			this.childColumns = childColumns;
			this.createConstraints = createConstraints;
			if (parentColumns.Length != childColumns.Length)
				throw new InvalidConstraintException ();
			DataTable parentTable = parentColumns[0].Table;
			DataTable childTable = childColumns[0].Table;
			if (parentTable.DataSet != childTable.DataSet)
				throw new InvalidConstraintException ();
			foreach (DataColumn column in parentColumns)
				if (column.Table != parentTable)
					throw new InvalidConstraintException ();
			foreach (DataColumn column in childColumns)
				if (column.Table != childTable)
					throw new InvalidConstraintException ();
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
			get {
				return childColumns;
			}
		}

		public virtual ForeignKeyConstraint ChildKeyConstraint {
			get {
				return childKeyConstraint;
			}
		}

		internal void SetChildKeyConstraint(ForeignKeyConstraint foreignKeyConstraint) {
			childKeyConstraint = foreignKeyConstraint;
		}

		public virtual DataTable ChildTable {
			get {
				return childColumns[0].Table;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual DataSet DataSet {
			get {
				return childColumns[0].Table.DataSet;
			}
		}

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds custom user information.")]
		public PropertyCollection ExtendedProperties {
			get {
				if (extendedProperties == null)
					extendedProperties = new PropertyCollection();
				return extendedProperties;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether relations are nested.")]
		[DefaultValue (false)]
		public virtual bool Nested {
			get {
				return nested;
			} 
			
			set {
				nested = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the parent columns of this relation.")]
		public virtual DataColumn[] ParentColumns {
			get {
				return parentColumns;
			}
		}

		public virtual UniqueConstraint ParentKeyConstraint {
			get {
				return parentKeyConstraint;
			}
		}

		internal void SetParentKeyConstraint(UniqueConstraint uniqueConstraint) {
			parentKeyConstraint = uniqueConstraint;
		}

		public virtual DataTable ParentTable {
			get {
				return parentColumns[0].Table;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("The name used to look up this relation in the Relations collection of a DataSet.")]
		[DefaultValue ("")]
		public virtual string RelationName {
			get {
				return relationName;
			}
			
			set {
				relationName = value;
			}
		}

		#endregion // Properties

		#region Methods

		protected void CheckStateForProperty () 
		{
			// TODO: check consistency of constraints
			DataTable parentTable = parentColumns[0].Table;
			DataTable childTable = parentColumns[0].Table;
			if (parentTable.DataSet != childTable.DataSet)
				throw new DataException ();
			bool allColumnsEqual = false;
			for (int colCnt = 0; colCnt < parentColumns.Length; ++colCnt) {
				if (!parentColumns [colCnt].DataType.Equals (childColumns [colCnt].DataType))
					throw new DataException ();
				if (parentColumns [colCnt] != childColumns [colCnt]) allColumnsEqual = false;
			}
			if (allColumnsEqual) throw new DataException ();
		}

		protected internal void OnPropertyChanging (PropertyChangedEventArgs pcevent)
		{
			if (onPropertyChangingDelegate != null)
				onPropertyChangingDelegate.Invoke(this, pcevent);
		}

		protected internal void RaisePropertyChanging (string name)
		{
			OnPropertyChanging(new PropertyChangedEventArgs(name));
		}

		public override string ToString () 
		{
			return relationName;
		}

		#endregion // Methods
	}
}
