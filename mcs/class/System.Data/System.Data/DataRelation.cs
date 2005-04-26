//
// System.Data.DataRelation.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//   Alan Tam Siu Lung <Tam@SiuLung.com>
//   Tim Coleman <tim@timcoleman.com>
//
// (C) 2002 Daniel Morgan
// (C) 2002 Ximian, Inc.
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
	[Editor]
	[DefaultProperty ("RelationName")]
	[Serializable]
	[MonoTODO]
	[TypeConverterAttribute (typeof (RelationshipConverter))]	
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
			if (relationName == null) relationName = string.Empty;
			this.relationName = relationName;
			if (parentColumns == null) throw new ArgumentNullException ();
			this.parentColumns = parentColumns;
			if (childColumns == null) throw new ArgumentNullException ();
			this.childColumns = childColumns;
			this.createConstraints = createConstraints;
			if (parentColumns.Length != childColumns.Length)
				throw new ArgumentException ("ParentColumns and ChildColumns should be the same length");
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

			for (int i=0; i<ChildColumns.Length; i++)
				if (!( parentColumns[i].DataType.Equals( childColumns[i].DataType)))
					throw new InvalidConstraintException();
		}

		[MonoTODO]
		[Browsable (false)]
		public DataRelation (string relationName, string parentTableName, string childTableName, string[] parentColumnNames, string[] childColumnNames, bool nested) 
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public DataRelation (string relationName, string parentTableName, string parentTableNamespace, string childTableName, string childTableNamespace, string[] parentColumnNames, string[] childColumnNames, bool nested)
		{
			throw new NotImplementedException ();
		}
#endif

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

		internal void SetDataSet(DataSet ds) {
			dataSet = ds;
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
				onPropertyChangingDelegate (this, pcevent);
		}

		protected internal void RaisePropertyChanging (string name)
		{
			OnPropertyChanging(new PropertyChangedEventArgs(name));
		}

		public override string ToString () 
		{
			return relationName;
		}
                
        internal void UpdateConstraints ()
        {
            if ( ! createConstraints)
                return;
            
            ForeignKeyConstraint    foreignKeyConstraint    = null;
            UniqueConstraint        uniqueConstraint        = null;
            
            foreignKeyConstraint    = FindForeignKey (ChildTable.Constraints);
            uniqueConstraint        = FindUniqueConstraint (ParentTable.Constraints); 

            // if we did not find the unique constraint in the parent table.
            // we generate new uniqueConstraint and add it to the parent table.
            if (uniqueConstraint == null) {
                uniqueConstraint = new UniqueConstraint (ParentColumns, false);
                ParentTable.Constraints.Add (uniqueConstraint);
            }
            
            // if we did not find the foreign key constraint in the parent table.
            // we generate new foreignKeyConstraint and add it to the parent table.
            if (foreignKeyConstraint == null) {
                foreignKeyConstraint = new ForeignKeyConstraint (RelationName, 
                                                                    ParentColumns, 
                                                                    ChildColumns);
                ChildTable.Constraints.Add (foreignKeyConstraint);
            }

            SetParentKeyConstraint (uniqueConstraint);
            SetChildKeyConstraint (foreignKeyConstraint);
        }

        private static bool CompareDataColumns (DataColumn [] dc1, DataColumn [] dc2)
        {
            if (dc1.Length != dc2.Length)
                return false;

            for (int columnCnt = 0; columnCnt < dc1.Length; ++columnCnt){
                if (dc1 [columnCnt] != dc2 [columnCnt])
                    return false;
            }
            return true;
        }
        
        private ForeignKeyConstraint FindForeignKey (ConstraintCollection cl)
        {
            ForeignKeyConstraint fkc = null; 
            foreach (Constraint o in cl) {
                if (! (o is ForeignKeyConstraint))
                    continue;
                fkc = (ForeignKeyConstraint) o;
                /* Check ChildColumns & ParentColumns */
                if (CompareDataColumns (ChildColumns, fkc.Columns) && 
                    CompareDataColumns (ParentColumns, fkc.RelatedColumns))
                    return fkc;
            }
            return null;
        }

        private UniqueConstraint FindUniqueConstraint (ConstraintCollection cl)
        {
            UniqueConstraint uc = null;
            // find if the unique constraint already exists in the parent table.
            foreach (Constraint o in cl){
                if (! (o is UniqueConstraint))
                    continue;
                uc = (UniqueConstraint) o;
                //Check in ParentColumns
                if (CompareDataColumns (ParentColumns, uc.Columns))
                    return uc;
            }
            return null;
        }

        /// <summary>
        ///     Check whether the given column is part of this relation.
        /// <summary>
        /// <returns>
        ///     true if the column is part of this relation, otherwise false.
        /// </returns>
        internal bool Contains (DataColumn column)
        {
            foreach (DataColumn col in ParentColumns)
                if (col == column)
                    return true;

            foreach (DataColumn col in ChildColumns)
                if (col == column)
                    return true;
            return false;
        }
                
		#endregion // Methods
	}
}
