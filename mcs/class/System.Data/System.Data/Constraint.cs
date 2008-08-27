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
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Data.Common;

namespace System.Data {
	[Serializable]
	internal delegate void DelegateConstraintNameChange (object sender, string newName);

	[DefaultProperty ("ConstraintName")]
#if !NET_2_0
	[Serializable]
#endif
	[TypeConverterAttribute (typeof (ConstraintConverter))]
	public abstract class Constraint {
		static readonly object beforeConstraintNameChange = new object ();

		EventHandlerList events = new EventHandlerList ();

		internal event DelegateConstraintNameChange BeforeConstraintNameChange {
			add { events.AddHandler (beforeConstraintNameChange, value); }
			remove { events.RemoveHandler (beforeConstraintNameChange, value); }
		}

		//if constraintName is not set then a name is
		//created when it is added to
		//the ConstraintCollection
		//it can not be set to null, empty or duplicate
		//once it has been added to the collection
		private string _constraintName;
		private PropertyCollection _properties;

		private Index _index;

		//Used for membership checking
		private ConstraintCollection _constraintCollection;

		DataSet dataSet;

		protected Constraint ()
		{
			dataSet = null;
			_properties = new PropertyCollection ();
		}

		[CLSCompliant (false)]
		protected internal virtual DataSet _DataSet {
			get { return dataSet; }
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates the name of this constraint.")]
#endif
		[DefaultValue ("")]
		public virtual string ConstraintName {
			get { return _constraintName == null ? "" : _constraintName; }
			set {
				//This should only throw an exception when it
				//is a member of a ConstraintCollection which
				//means we should let the ConstraintCollection
				//handle exceptions when this value changes
				_onConstraintNameChange (value);
				_constraintName = value;
			}
		}

		[Browsable (false)]
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("The collection that holds custom user information.")]
#endif
		public PropertyCollection ExtendedProperties {
			get { return _properties; }
		}

#if !NET_2_0
		[DataSysDescription ("Indicates the table of this constraint.")]
#endif
		public abstract DataTable Table {
			get;
		}

		internal ConstraintCollection ConstraintCollection {
			get { return _constraintCollection; }
			set { _constraintCollection = value; }
		}

		private void _onConstraintNameChange (string newName)
		{
			DelegateConstraintNameChange eh = events [beforeConstraintNameChange] as DelegateConstraintNameChange;
			if (eh != null)
				eh (this, newName);
		}

		//call once before adding a constraint to a collection
		//will throw an exception to prevent the add if a rule is broken
		internal abstract void AddToConstraintCollectionSetup (ConstraintCollection collection);

		internal abstract bool IsConstraintViolated ();

		internal static void ThrowConstraintException ()
		{
			throw new ConstraintException("Failed to enable constraints. One or more rows contain values violating non-null, unique, or foreign-key constraints.");
		}

		bool initInProgress = false;
		internal virtual bool InitInProgress {
			get { return initInProgress; }
			set { initInProgress = value; }
		}

		internal virtual void FinishInit (DataTable table)
		{
		}

		internal void AssertConstraint ()
		{
			// The order is important.. IsConstraintViolated fills the RowErrors if it detects
			// a violation
			if (!IsConstraintViolated ())
				return;
			if (Table._duringDataLoad || (Table.DataSet != null && !Table.DataSet.EnforceConstraints))
				return;
			ThrowConstraintException ();
		}

		internal abstract void AssertConstraint (DataRow row);

		internal virtual void RollbackAssert (DataRow row)
		{
		}

		//call once before removing a constraint to a collection
		//can throw an exception to prevent the removal
		internal abstract void RemoveFromConstraintCollectionCleanup (ConstraintCollection collection);

		[MonoTODO]
		protected void CheckStateForProperty ()
		{
			throw new NotImplementedException ();
		}

		protected internal void SetDataSet (DataSet dataSet)
		{
			this.dataSet = dataSet;
		}

		internal void SetExtendedProperties (PropertyCollection properties)
		{
			_properties = properties;
		}

		internal Index Index {
			get { return _index; }
			set {
				if (_index != null) {
					_index.RemoveRef();
					Table.DropIndex(_index);
				}

				_index = value;

				if (_index != null)
					_index.AddRef();
			}
		}

		internal abstract bool IsColumnContained (DataColumn column);
		internal abstract bool CanRemoveFromCollection (ConstraintCollection col, bool shouldThrow);

		/// <summary>
		/// Gets the ConstraintName, if there is one, as a string.
		/// </summary>
		public override string ToString ()
		{
			return _constraintName == null ? "" : _constraintName;
		}
	}
}
