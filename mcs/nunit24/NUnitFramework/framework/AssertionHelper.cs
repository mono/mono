// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections;
using NUnit.Framework.SyntaxHelpers;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
	/// <summary>
	/// AssertionHelper is an optional base class for user tests,
	/// allowing the use of shorter names for constraints and
	/// asserts and avoiding conflict with the definition of 
	/// <see cref="Is"/>, from which it inherits much of its
	/// behavior, in certain mock object frameworks.
	/// </summary>
	public class AssertionHelper : ConstraintBuilder
	{
		#region Expect
		/// <summary>
		/// Apply a constraint to an actual value, succeeding if the constraint
		/// is satisfied and throwing an assertion exception on failure. Works
		/// identically to <see cref="NUnit.Framework.Assert.That(object, Constraint)"/>
		/// </summary>
		/// <param name="constraint">A Constraint to be applied</param>
		/// <param name="actual">The actual value to test</param>
		static public void Expect( object actual, Constraint constraint )
		{
			Assert.That( actual, constraint, null, null );
		}

		/// <summary>
		/// Apply a constraint to an actual value, succeeding if the constraint
		/// is satisfied and throwing an assertion exception on failure. Works
		/// identically to <see cref="NUnit.Framework.Assert.That(object, Constraint, string)"/>
		/// </summary>
		/// <param name="constraint">A Constraint to be applied</param>
		/// <param name="actual">The actual value to test</param>
		/// <param name="message">The message that will be displayed on failure</param>
		static public void Expect( object actual, Constraint constraint, string message )
		{
			Assert.That( actual, constraint, message, null );
		}

		/// <summary>
		/// Apply a constraint to an actual value, succeeding if the constraint
		/// is satisfied and throwing an assertion exception on failure. Works
		/// identically to <see cref="NUnit.Framework.Assert.That(object, Constraint, string, object[])"/>
		/// </summary>
		/// <param name="constraint">A Constraint to be applied</param>
		/// <param name="actual">The actual value to test</param>
		/// <param name="message">The message that will be displayed on failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		static public void Expect( object actual, Constraint constraint, string message, params object[] args )
		{
			Assert.That( actual, constraint, message, args );
		}

		/// <summary>
		/// Asserts that a condition is true. If the condition is false the method throws
		/// an <see cref="AssertionException"/>. Works Identically to 
        /// <see cref="Assert.That(bool, string, object[])"/>.
		/// </summary> 
		/// <param name="condition">The evaluated condition</param>
		/// <param name="message">The message to display if the condition is false</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		static public void Expect(bool condition, string message, params object[] args)
		{
			Assert.That(condition, Is.True, message, args);
		}

		/// <summary>
		/// Asserts that a condition is true. If the condition is false the method throws
		/// an <see cref="AssertionException"/>. Works Identically to 
        /// <see cref="Assert.That(bool, string)"/>.
		/// </summary>
		/// <param name="condition">The evaluated condition</param>
		/// <param name="message">The message to display if the condition is false</param>
		static public void Expect(bool condition, string message)
		{
			Assert.That(condition, Is.True, message, null);
		}

		/// <summary>
		/// Asserts that a condition is true. If the condition is false the method throws
		/// an <see cref="AssertionException"/>. Works Identically to <see cref="Assert.That(bool)"/>.
		/// </summary>
		/// <param name="condition">The evaluated condition</param>
		static public void Expect(bool condition)
		{
			Assert.That(condition, Is.True, null, null);
		}
		#endregion

		#region Map
		/// <summary>
		/// Returns a ListMapper based on a collection.
		/// </summary>
		/// <param name="original">The original collection</param>
		/// <returns></returns>
		public ListMapper Map( ICollection original )
		{
			return new ListMapper( original );
		}
		#endregion
	}
}
