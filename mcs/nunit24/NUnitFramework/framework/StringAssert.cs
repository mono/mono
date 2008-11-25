// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System.ComponentModel;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
	/// <summary>
	/// Basic Asserts on strings.
	/// </summary>
	public class StringAssert
	{
		#region Equals and ReferenceEquals

		/// <summary>
		/// The Equals method throws an AssertionException. This is done 
		/// to make sure there is no mistake by calling this function.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static new bool Equals(object a, object b)
		{
			throw new AssertionException("Assert.Equals should not be used for Assertions");
		}

		/// <summary>
		/// override the default ReferenceEquals to throw an AssertionException. This 
		/// implementation makes sure there is no mistake in calling this function 
		/// as part of Assert. 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static new void ReferenceEquals(object a, object b)
		{
			throw new AssertionException("Assert.ReferenceEquals should not be used for Assertions");
		}

		#endregion
				
		#region Contains

		/// <summary>
		/// Asserts that a string is found within another string.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The string to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Arguments used in formatting the message</param>
		static public void Contains( string expected, string actual, string message, params object[] args )
		{
            Assert.That(actual, new SubstringConstraint(expected), message, args);
		}

		/// <summary>
		/// Asserts that a string is found within another string.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The string to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		static public void Contains( string expected, string actual, string message )
		{
			Contains( expected, actual, message, null );
		}

		/// <summary>
		/// Asserts that a string is found within another string.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The string to be examined</param>
		static public void Contains( string expected, string actual )
		{
			Contains( expected, actual, string.Empty, null );
		}

		#endregion

		#region StartsWith

		/// <summary>
		/// Asserts that a string starts with another string.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The string to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Arguments used in formatting the message</param>
		static public void StartsWith( string expected, string actual, string message, params object[] args )
		{
            Assert.That(actual, new StartsWithConstraint(expected), message, args);
		}

		/// <summary>
		/// Asserts that a string starts with another string.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The string to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		static public void StartsWith( string expected, string actual, string message )
		{
			StartsWith( expected, actual, message, null );
		}

		/// <summary>
		/// Asserts that a string starts with another string.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The string to be examined</param>
		static public void StartsWith( string expected, string actual )
		{
			StartsWith( expected, actual, string.Empty, null );
		}

		#endregion

		#region EndsWith

		/// <summary>
		/// Asserts that a string ends with another string.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The string to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Arguments used in formatting the message</param>
		static public void EndsWith( string expected, string actual, string message, params object[] args )
		{
            Assert.That(actual, new EndsWithConstraint(expected), message, args);
		}

		/// <summary>
		/// Asserts that a string ends with another string.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The string to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		static public void EndsWith( string expected, string actual, string message )
		{
			EndsWith( expected, actual, message, null );
		}

		/// <summary>
		/// Asserts that a string ends with another string.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The string to be examined</param>
		static public void EndsWith( string expected, string actual )
		{
			EndsWith( expected, actual, string.Empty, null );
		}

		#endregion

		#region AreEqualIgnoringCase
		/// <summary>
		/// Asserts that two strings are equal, without regard to case.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The actual string</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Arguments used in formatting the message</param>
		static public void AreEqualIgnoringCase( string expected, string actual, string message, params object[] args )
		{
            Assert.That(actual, new EqualConstraint(expected).IgnoreCase, message, args);
		}

		/// <summary>
		/// Asserts that two strings are equal, without regard to case.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The actual string</param>
		/// <param name="message">The message to display in case of failure</param>
		static public void AreEqualIgnoringCase( string expected, string actual, string message )
		{
			AreEqualIgnoringCase( expected, actual, message, null );
		}

		/// <summary>
		/// Asserts that two strings are equal, without regard to case.
		/// </summary>
		/// <param name="expected">The expected string</param>
		/// <param name="actual">The actual string</param>
		static public void AreEqualIgnoringCase( string expected, string actual )
		{
			AreEqualIgnoringCase( expected, actual, string.Empty, null );
		}

		#endregion

		#region IsMatch
		/// <summary>
		/// Asserts that a string matches an expected regular expression pattern.
		/// </summary>
		/// <param name="expected">The expected expression</param>
		/// <param name="actual">The actual string</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Arguments used in formatting the message</param>
		static public void IsMatch( string expected, string actual, string message, params object[] args )
		{
            Assert.That(actual, new RegexConstraint(expected), message, args);
		}

		/// <summary>
		/// Asserts that a string matches an expected regular expression pattern.
		/// </summary>
		/// <param name="expected">The expected expression</param>
		/// <param name="actual">The actual string</param>
		/// <param name="message">The message to display in case of failure</param>
		static public void IsMatch( string expected, string actual, string message )
		{
			IsMatch( expected, actual, message, null );
		}

		/// <summary>
		/// Asserts that a string matches an expected regular expression pattern.
		/// </summary>
		/// <param name="expected">The expected expression</param>
		/// <param name="actual">The actual string</param>
		static public void IsMatch( string expected, string actual )
		{
			IsMatch( expected, actual, string.Empty, null );
		}
		#endregion
	}
}
