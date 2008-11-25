// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Framework 
{
	using System;

	/// <summary>
	/// The Assertion class is obsolete and has been
	/// replaced by the Assert class.
	/// </summary>
	[Obsolete("Use Assert class instead")]
	public class Assertion
	{
		/// <summary>
		/// Asserts that a condition is true. If it isn't it throws
		/// an <see cref="AssertionException"/>.
		/// </summary>
		/// <param name="message">The message to display is the condition
		/// is false</param>
		/// <param name="condition">The evaluated condition</param>
		static public void Assert(string message, bool condition) 
		{
			NUnit.Framework.Assert.IsTrue(condition, message);
		}
    
		/// <summary>
		/// Asserts that a condition is true. If it isn't it throws
		/// an <see cref="AssertionException"/>.
		/// </summary>
		/// <param name="condition">The evaluated condition</param>
		static public void Assert(bool condition) 
		{
			Assertion.Assert(string.Empty, condition);
		}

		/// <summary>
		/// /// Asserts that two doubles are equal concerning a delta. If the
		/// expected value is infinity then the delta value is ignored.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value</param>
		/// <param name="delta">The maximum acceptable difference between the
		/// the expected and the actual</param>
		static public void AssertEquals(double expected, double actual, double delta) 
		{
			Assertion.AssertEquals(string.Empty, expected, actual, delta);
		}
		/// <summary>
		/// /// Asserts that two singles are equal concerning a delta. If the
		/// expected value is infinity then the delta value is ignored.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value</param>
		/// <param name="delta">The maximum acceptable difference between the
		/// the expected and the actual</param>
		static public void AssertEquals(float expected, float actual, float delta) 
		{
			Assertion.AssertEquals(string.Empty, expected, actual, delta);
		}

		/// <summary>Asserts that two objects are equal. If they are not
		/// an <see cref="AssertionException"/> is thrown.</summary>
		static public void AssertEquals(Object expected, Object actual) 
		{
			Assertion.AssertEquals(string.Empty, expected, actual);
		}

		/// <summary>Asserts that two ints are equal. If they are not
		/// an <see cref="AssertionException"/> is thrown.</summary>
		static public void AssertEquals(int expected, int actual) 
		{
			Assertion.AssertEquals(string.Empty, expected, actual);
		}

		/// <summary>Asserts that two ints are equal. If they are not
		/// an <see cref="AssertionException"/> is thrown.</summary>
		static public void AssertEquals(string message, int expected, int actual) 
		{
			NUnit.Framework.Assert.AreEqual(expected, actual, message);
		}
		
		/// <summary>Asserts that two doubles are equal concerning a delta.
		/// If the expected value is infinity then the delta value is ignored.
		/// </summary>
		static public void AssertEquals(string message, double expected, 
			double actual, double delta) 
		{
			NUnit.Framework.Assert.AreEqual(expected, actual, delta, message);
		}
		
		/// <summary>Asserts that two floats are equal concerning a delta.
		/// If the expected value is infinity then the delta value is ignored.
		/// </summary>
		static public void AssertEquals(string message, float expected, 
			float actual, float delta) 
		{
			NUnit.Framework.Assert.AreEqual(expected, actual, delta, message);
		}

		/// <summary>
		/// Asserts that two objects are equal.  Two objects are considered
		/// equal if both are null, or if both have the same value.  Numeric
		/// types are compared via string comparision on their contents to
		/// avoid problems comparing values between different types.  All
		/// non-numeric types are compared by using the <c>Equals</c> method.
		/// If they are not equal an <see cref="AssertionException"/> is thrown.
		/// </summary>
		static public void AssertEquals(string message, Object expected, Object actual)
		{
			NUnit.Framework.Assert.AreEqual(expected, actual, message);
		}
    
		/// <summary>Asserts that an object isn't null.</summary>
		static public void AssertNotNull(Object anObject) 
		{
			NUnit.Framework.Assert.IsNotNull(anObject, string.Empty);
		}
    
		/// <summary>Asserts that an object isn't null.</summary>
		static public void AssertNotNull(string message, Object anObject) 
		{
			NUnit.Framework.Assert.IsNotNull(anObject, message);
		}
    
		/// <summary>Asserts that an object is null.</summary>
		static public void AssertNull(Object anObject) 
		{
			NUnit.Framework.Assert.IsNull(anObject, string.Empty);
		}
    
		/// <summary>Asserts that an object is null.</summary>
		static public void AssertNull(string message, Object anObject) 
		{
			NUnit.Framework.Assert.IsNull(anObject, message);
		}
    
		/// <summary>Asserts that two objects refer to the same object. If they
		/// are not the same an <see cref="AssertionException"/> is thrown.
		/// </summary>
		static public void AssertSame(Object expected, Object actual) 
		{
			NUnit.Framework.Assert.AreSame(expected, actual, string.Empty);
		}
    
		/// <summary>Asserts that two objects refer to the same object. 
		/// If they are not an <see cref="AssertionException"/> is thrown.
		/// </summary>
		static public void AssertSame(string message, Object expected, Object actual)
		{
			NUnit.Framework.Assert.AreSame(expected, actual, message);
		}
    
		/// <summary>Fails a test with no message.</summary>
		static public void Fail() 
		{
			NUnit.Framework.Assert.Fail();
		}
    
		/// <summary>Fails a test with the given message.</summary>
		static public void Fail(string message) 
		{
			NUnit.Framework.Assert.Fail(message);
		}
	}
}
