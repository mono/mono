// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using NUnit.Framework.Constraints;

namespace NUnit.Framework.SyntaxHelpers
{
	/// <summary>
	/// Summary description for HasNoPrefixB.
	/// </summary>
	public class Has
	{
        /// <summary>
        /// Nested class that allows us to restrict the number
        /// of key words that may appear after Has.No.
        /// </summary>
		public class HasNoPrefixBuilder
		{
            /// <summary>
            /// Return a ConstraintBuilder conditioned to apply
            /// the following constraint to a property.
            /// </summary>
            /// <param name="name">The property name</param>
            /// <returns>A ConstraintBuilder</returns>
			public ConstraintBuilder Property(string name)
			{
				return new ConstraintBuilder().Not.Property(name);
			}

            /// <summary>
            /// Return a Constraint that succeeds if the expected object is
            /// not contained in a collection.
            /// </summary>
            /// <param name="expected">The expected object</param>
            /// <returns>A Constraint</returns>
            public Constraint Member(object expected)
			{
				return new NotConstraint( new CollectionContainsConstraint(expected) ) ;
			}
		}

		#region Prefix Operators
		/// <summary>
		/// Has.No returns a ConstraintBuilder that negates
		/// the constraint that follows it.
		/// </summary>
		public static HasNoPrefixBuilder No
		{
			get { return new HasNoPrefixBuilder(); }
		}

		/// <summary>
		/// Has.AllItems returns a ConstraintBuilder, which will apply
		/// the following constraint to all members of a collection,
		/// succeeding if all of them succeed.
		/// </summary>
		public static ConstraintBuilder All
		{
			get { return new ConstraintBuilder().All; }
		}

		/// <summary>
		/// Has.Some returns a ConstraintBuilder, which will apply
		/// the following constraint to all members of a collection,
		/// succeeding if any of them succeed. It is a synonym
		/// for Has.Item.
		/// </summary>
		public static ConstraintBuilder Some
		{
			get { return new ConstraintBuilder().Some; }
		}

		/// <summary>
		/// Has.None returns a ConstraintBuilder, which will apply
		/// the following constraint to all members of a collection,
		/// succeeding only if none of them succeed.
		/// </summary>
		public static ConstraintBuilder None
		{
			get { return new ConstraintBuilder().None; }
		}

		/// <summary>
		/// Returns a new ConstraintBuilder, which will apply the
		/// following constraint to a named property of the object
		/// being tested.
		/// </summary>
		/// <param name="name">The name of the property</param>
		public static ConstraintBuilder Property( string name )
		{
			return new ConstraintBuilder().Property(name);
		}
		#endregion

		#region Property Constraints
		/// <summary>
		/// Returns a new PropertyConstraint checking for the
		/// existence of a particular property value.
		/// </summary>
		/// <param name="name">The name of the property to look for</param>
		/// <param name="expected">The expected value of the property</param>
		public static Constraint Property( string name, object expected )
		{
			return new PropertyConstraint( name, new EqualConstraint( expected ) );
		}

        /// <summary>
        /// Returns a new PropertyConstraint for the Length property
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
		public static Constraint Length( int length )
		{
			return Property( "Length", length );
		}

		/// <summary>
		/// Returns a new PropertyConstraint or the Count property
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public static Constraint Count( int count )
		{
			return Property( "Count", count );
		}
		#endregion

		#region Member Constraint
		/// <summary>
		/// Returns a new CollectionContainsConstraint checking for the
		/// presence of a particular object in the collection.
		/// </summary>
		/// <param name="expected">The expected object</param>
		public static Constraint Member( object expected )
		{
			return new CollectionContainsConstraint( expected );
		}
		#endregion
	}
}
