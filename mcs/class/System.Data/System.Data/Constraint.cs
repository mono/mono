//
// System.Data.Constraint.cs
//
// Author:
//	Franklin Wise <gracenote@earthlink.net>
//	Daniel Morgan
//   
//
// (C) Ximian, Inc. 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Data
{
	[Serializable]
	internal delegate void DelegateConstraintNameChange(object sender,
			string newName);
	
	[Serializable]
	public abstract class Constraint 
	{
		internal event DelegateConstraintNameChange 
					BeforeConstraintNameChange;

		//if constraintName is not set then a name is 
		//created when it is added to
		//the ConstraintCollection
		//it can not be set to null, empty or duplicate
		//once it has been added to the collection
		private string _constraintName = null;
		private PropertyCollection _properties = null;

		//Used for membership checking
		private ConstraintCollection _constraintCollection;

		protected Constraint() 
		{
			_properties = new PropertyCollection();
		}

		public virtual string ConstraintName {
			get{
				return "" + _constraintName;
			} 

			set{
				//This should only throw an exception when it
				//is a member of a ConstraintCollection which
				//means we should let the ConstraintCollection
				//handle exceptions when this value changes
				_onConstraintNameChange(value);
				_constraintName = value;
			}
		}

		public PropertyCollection ExtendedProperties {
			get {
				return _properties;
			}
		}

		public abstract DataTable Table {
			get;
		}

		/// <summary>
		/// Gets the ConstraintName, if there is one, as a string. 
		/// </summary>
		public override string ToString() 
		{
			return "" + _constraintName;
		}

		internal ConstraintCollection ConstraintCollection {
			get{
				return _constraintCollection;
			}
			set{
				_constraintCollection = value;
			}
		}
		

		private void _onConstraintNameChange(string newName)
		{
			if (null != BeforeConstraintNameChange)
			{
				BeforeConstraintNameChange(this, newName);
			}
		}

		//call once before adding a constraint to a collection
		//will throw an exception to prevent the add if a rule is broken
		internal protected abstract void AddToConstraintCollectionSetup(
				ConstraintCollection collection);
					
		//call once before removing a constraint to a collection
		//can throw an exception to prevent the removal
		internal protected abstract void RemoveFromConstraintCollectionCleanup( 
				ConstraintCollection collection);


		//Call to Validate the constraint
		//Will throw if constraint is violated
		internal abstract void AssertConstraint();
		
	}
}
