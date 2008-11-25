// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.IO;
using System.Collections;

namespace NUnit.Framework.Constraints
{
	/// <summary>
	/// The Constraint class is the base of all built-in or
	/// user-defined constraints in NUnit. It provides the operator
	/// overloads used to combine constraints.
	/// </summary>
    public abstract class Constraint
    {
        #region UnsetObject Class
        /// <summary>
        /// Class used to detect any derived constraints
        /// that fail to set the actual value in their
        /// Matches override.
        /// </summary>
        private class UnsetObject
        {
            public override string ToString()
            {
                return "UNSET";
            }
        }
        #endregion

		#region Static and Instance Fields
        /// <summary>
        /// Static UnsetObject used to detect derived constraints
        /// failing to set the actual value.
        /// </summary>
        protected static object UNSET = new UnsetObject();

		/// <summary>
		/// If true, all string comparisons will ignore case
		/// </summary>
		protected bool caseInsensitive;

        /// <summary>
        /// If true, strings in error messages will be clipped
        /// </summary>
        protected bool clipStrings = true;

		/// <summary>
		/// If true, arrays will be treated as collections, allowing
		/// those of different dimensions to be compared
		/// </summary>
		protected bool compareAsCollection;

		/// <summary>
		/// If non-zero, equality comparisons within the specified 
		/// tolerance will succeed.
		/// </summary>
		protected object tolerance;

        /// <summary>
        /// IComparer object used in comparisons for some constraints.
        /// </summary>
        protected IComparer compareWith;

		/// <summary>
        /// The actual value being tested against a constraint
        /// </summary>
        protected object actual = UNSET;
        #endregion

        #region Properties
        /// <summary>
		/// Flag the constraint to ignore case and return self.
		/// </summary>
		public virtual Constraint IgnoreCase
		{
			get
			{
				caseInsensitive = true;
				return this;
			}
		}

        /// <summary>
        /// Flag the constraint to suppress string clipping 
        /// and return self.
        /// </summary>
        public Constraint NoClip
        {
            get
            {
                clipStrings = false;
                return this;
            }
        }

		/// <summary>
		/// Flag the constraint to compare arrays as collections
		/// and return self.
		/// </summary>
		public Constraint AsCollection
		{
			get
			{
				compareAsCollection = true;
				return this;
			}
		}

        /// <summary>
        /// Flag the constraint to use a tolerance when determining equality.
        /// Currently only used for doubles and floats.
        /// </summary>
        /// <param name="tolerance">Tolerance to be used</param>
        /// <returns>Self.</returns>
        public Constraint Within(object tolerance)
		{
			this.tolerance = tolerance;
			return this;
		}

        /// <summary>
        /// Flag the constraint to use the supplied IComparer object.
        /// </summary>
        /// <param name="comparer">The IComparer object to use.</param>
        /// <returns>Self.</returns>
        public Constraint Comparer(IComparer comparer)
        {
            this.compareWith = comparer;
            return this;
        }
		#endregion

		#region Public Methods
        /// <summary>
        /// Write the failure message to the MessageWriter provided
        /// as an argument. The default implementation simply passes
        /// the constraint and the actual value to the writer, which
        /// then displays the constraint description and the value.
        /// 
        /// Constraints that need to provide additional details,
        /// such as where the error occured can override this.
        /// </summary>
        /// <param name="writer">The MessageWriter on which to display the message</param>
        public virtual void WriteMessageTo(MessageWriter writer)
        {
            writer.DisplayDifferences(this);
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public abstract bool Matches(object actual);

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public abstract void WriteDescriptionTo(MessageWriter writer);

		/// <summary>
		/// Write the actual value for a failing constraint test to a
		/// MessageWriter. The default implementation simply writes
		/// the raw value of actual, leaving it to the writer to
		/// perform any formatting.
		/// </summary>
		/// <param name="writer">The writer on which the actual value is displayed</param>
		public virtual void WriteActualValueTo(MessageWriter writer)
		{
			writer.WriteActualValue( actual );
		}
		#endregion

        #region Operator Overloads
        /// <summary>
        /// This operator creates a constraint that is satisfied only if both 
        /// argument constraints are satisfied.
        /// </summary>
        public static Constraint operator &(Constraint left, Constraint right)
        {
            return new AndConstraint(left, right);
        }

        /// <summary>
        /// This operator creates a constraint that is satisfied if either 
        /// of the argument constraints is satisfied.
        /// </summary>
        public static Constraint operator |(Constraint left, Constraint right)
        {
            return new OrConstraint(left, right);
        }

        /// <summary>
        /// This operator creates a constraint that is satisfied if the 
        /// argument constraint is not satisfied.
        /// </summary>
        public static Constraint operator !(Constraint m)
        {
            return new NotConstraint(m == null ? new EqualConstraint(null) : m);
        }
        #endregion
	}
}
