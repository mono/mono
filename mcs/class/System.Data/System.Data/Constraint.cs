//
// System.Data.Constraint.cs
//
// Author:
//	Franklin Wise <gracenote@earthlink.net>
//	Daniel Morgan
//      Tim Coleman (tim@timcoleman.com)
//   
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Data {
	[Serializable]
	internal delegate void DelegateConstraintNameChange (object sender, string newName);

	[DefaultProperty ("ConstraintName")]	
	[Serializable]
	public abstract class Constraint 
	{
		internal event DelegateConstraintNameChange BeforeConstraintNameChange;

		//if constraintName is not set then a name is 
		//created when it is added to
		//the ConstraintCollection
		//it can not be set to null, empty or duplicate
		//once it has been added to the collection
		private string _constraintName = null;
		private PropertyCollection _properties = null;

		//Used for membership checking
		private ConstraintCollection _constraintCollection;

		DataSet dataSet;

		protected Constraint () 
		{
			dataSet = null;
			_properties = new PropertyCollection();
		}

		protected internal virtual DataSet _DataSet {
			get { return dataSet; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the name of this constraint.")]
		[DefaultValue ("")]
		public virtual string ConstraintName {
			get{ return "" + _constraintName; } 
			set{
				//This should only throw an exception when it
				//is a member of a ConstraintCollection which
				//means we should let the ConstraintCollection
				//handle exceptions when this value changes
				_onConstraintNameChange(value);
				_constraintName = value;
			}
		}

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds custom user information.")]
		public PropertyCollection ExtendedProperties {
			get { return _properties; }
		}

		[DataSysDescription ("Indicates the table of this constraint.")]
		public abstract DataTable Table {
			get;
		}

		internal ConstraintCollection ConstraintCollection {
			get{ return _constraintCollection; }
			set{ _constraintCollection = value; }
		}
		
		private void _onConstraintNameChange (string newName)
		{
			if (null != BeforeConstraintNameChange)
			{
				BeforeConstraintNameChange (this, newName);
			}
		}

		//call once before adding a constraint to a collection
		//will throw an exception to prevent the add if a rule is broken
		internal virtual void AddToConstraintCollectionSetup (ConstraintCollection collection)
		{
		}
					
		internal virtual void AssertConstraint ()
		{
		}
		
		internal virtual void AssertConstraint (DataRow row)
		{
		}

		//call once before removing a constraint to a collection
		//can throw an exception to prevent the removal
		internal virtual void RemoveFromConstraintCollectionCleanup (ConstraintCollection collection)
		{
		}

		[MonoTODO]
		protected void CheckStateForProperty ()
		{
			throw new NotImplementedException ();
		}

		protected internal void SetDataSet (DataSet dataSet)
		{
			this.dataSet = dataSet;
		}

		/// <summary>
		/// Gets the ConstraintName, if there is one, as a string. 
		/// </summary>
		public override string ToString () 
		{
			return "" + _constraintName;
		}

	}
}
