// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Text.RegularExpressions;

namespace NUnit.Framework.Constraints
{
	/// <summary>
	/// EmptyStringConstraint tests whether a string is empty.
	/// </summary>
	public class EmptyStringConstraint : EmptyConstraint
	{
        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches(object actual)
		{
			this.actual = actual;

			if ( !(actual is string) )
				return false;

			return (string)actual == string.Empty;
		}

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
		{
			writer.Write( "<empty>" );
		}
	}

	/// <summary>
	/// SubstringConstraint can test whether a string contains
	/// the expected substring.
	/// </summary>
    public class SubstringConstraint : Constraint
    {
        string expected;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SubstringConstraint"/> class.
        /// </summary>
        /// <param name="expected">The expected.</param>
        public SubstringConstraint(string expected)
        {
            this.expected = expected;
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;
            
            if ( !(actual is string) )
                return false;

            if (this.caseInsensitive)
                return ((string)actual).ToLower().IndexOf(expected.ToLower()) >= 0;
            else
                return ((string)actual).IndexOf(expected) >= 0;
        }

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WritePredicate("String containing");
            writer.WriteExpectedValue(expected);
			if ( this.caseInsensitive )
				writer.WriteModifier( "ignoring case" );
		}
    }

	/// <summary>
	/// StartsWithConstraint can test whether a string starts
	/// with an expected substring.
	/// </summary>
    public class StartsWithConstraint : Constraint
    {
        private string expected;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:StartsWithConstraint"/> class.
        /// </summary>
        /// <param name="expected">The expected string</param>
        public StartsWithConstraint(string expected)
        {
            this.expected = expected;
        }

        /// <summary>
        /// Test whether the constraint is matched by the actual value.
        /// This is a template method, which calls the IsMatch method
        /// of the derived class.
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;

            if (!(actual is string))
                return false;

            if ( this.caseInsensitive )
                return ((string)actual).ToLower().StartsWith(expected.ToLower());
            else
                return ((string)actual).StartsWith(expected);
        }

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WritePredicate("String starting with");
            writer.WriteExpectedValue( MsgUtils.ClipString(expected, writer.MaxLineLength - 40, 0) );
			if ( this.caseInsensitive )
				writer.WriteModifier( "ignoring case" );
		}
    }

    /// <summary>
    /// EndsWithConstraint can test whether a string ends
    /// with an expected substring.
    /// </summary>
    public class EndsWithConstraint : Constraint
    {
        private string expected;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EndsWithConstraint"/> class.
        /// </summary>
        /// <param name="expected">The expected string</param>
        public EndsWithConstraint(string expected)
        {
            this.expected = expected;
        }

        /// <summary>
        /// Test whether the constraint is matched by the actual value.
        /// This is a template method, which calls the IsMatch method
        /// of the derived class.
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;

            if (!(actual is string))
                return false;

            if ( this.caseInsensitive )
                return ((string)actual).ToLower().EndsWith(expected.ToLower());
            else
                return ((string)actual).EndsWith(expected);
        }

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WritePredicate("String ending with");
            writer.WriteExpectedValue(expected);
			if ( this.caseInsensitive )
				writer.WriteModifier( "ignoring case" );
		}
    }

    /// <summary>
    /// RegexConstraint can test whether a string matches
    /// the pattern provided.
    /// </summary>
    public class RegexConstraint : Constraint
    {
        string pattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RegexConstraint"/> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        public RegexConstraint(string pattern)
        {
            this.pattern = pattern;
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;

            return actual is string && 
                Regex.IsMatch( 
                    (string)actual, 
                    this.pattern,
                    this.caseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None );
        }

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WritePredicate("String matching");
            writer.WriteExpectedValue(this.pattern);
			if ( this.caseInsensitive )
				writer.WriteModifier( "ignoring case" );
		}
    }
}
