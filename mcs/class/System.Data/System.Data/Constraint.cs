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

		private string _name = null;
		private PropertyCollection _properties = null;

		//Used for membership checking
		private ConstraintCollection _constraintCollection;

		protected Constraint() 
		{
			_properties = new PropertyCollection();
		}

		public virtual string ConstraintName {
			get{
				return "" + _name;
			} 

			set{
				//This should only throw an exception when it
				//is a member of a ConstraintCollection which
				//means we should let the ConstraintCollection
				//handle exceptions when this value changes
				_onConstraintNameChange(value);
				_name = value;
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
			return "" + _name;
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

	}
}
