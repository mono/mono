namespace NUnit.Framework 
{
	using System;

	/// <summary>A set of Assert methods.</summary>
	public class Assertion : MarshalByRefObject
	{

		/// <summary>
		/// Protect constructor since it is a static only class
		/// </summary>
		protected Assertion():base(){}
		/// <summary>
		/// Asserts that a condition is true. If it isn't it throws
		/// an <see cref="AssertionFailedError"/>.
		/// </summary>
		/// <param name="message">The message to display is the condition
		/// is false</param>
		/// <param name="condition">The evaluated condition</param>
		static public void Assert(string message, bool condition) 
		{
			if (!condition)
				Assertion.Fail(message);
		}
    
		/// <summary>
		/// Asserts that a condition is true. If it isn't it throws
		/// an <see cref="AssertionFailedError"/>.
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
		/// an <see cref="AssertionFailedError"/> is thrown.</summary>
		static public void AssertEquals(Object expected, Object actual) 
		{
			Assertion.AssertEquals(string.Empty, expected, actual);
		}
		
		/// <summary>Asserts that two doubles are equal concerning a delta.
		/// If the expected value is infinity then the delta value is ignored.
		/// </summary>
		static public void AssertEquals(string message, double expected, 
			double actual, double delta) 
		{
			// handle infinity specially since subtracting two infinite values gives 
			// NaN and the following test fails
			if (double.IsInfinity(expected)) 
			{
				if (!(expected == actual))
					Assertion.FailNotEquals(message, expected, actual);
			} 
			else if (!(Math.Abs(expected-actual) <= delta))
				Assertion.FailNotEquals(message, expected, actual);
		}
		
		/// <summary>Asserts that two floats are equal concerning a delta.
		/// If the expected value is infinity then the delta value is ignored.
		/// </summary>
		static public void AssertEquals(string message, float expected, 
			float actual, float delta) 
		{
			// handle infinity specially since subtracting two infinite values gives 
			// NaN and the following test fails
			if (float.IsInfinity(expected)) 
			{
				if (!(expected == actual))
					Assertion.FailNotEquals(message, expected, actual);
			} 
			else if (!(Math.Abs(expected-actual) <= delta))
				Assertion.FailNotEquals(message, expected, actual);
		}

		/// <summary>Asserts that two objects are equal. If they are not
		/// an <see cref="AssertionFailedError"/> is thrown.</summary>
		static public void AssertEquals(string message, Object expected, Object actual)
		{
			if (expected == null && actual == null)
				return;
			if (expected != null && expected.Equals(actual))
				return;
			Assertion.FailNotEquals(message, expected, actual);
		}
    
		/// <summary>Asserts that an object isn't null.</summary>
		static public void AssertNotNull(Object anObject) 
		{
			Assertion.AssertNotNull(string.Empty, anObject);
		}
    
		/// <summary>Asserts that an object isn't null.</summary>
		static public void AssertNotNull(string message, Object anObject) 
		{
			Assertion.Assert(string.Empty, anObject != null); 
		}
    
		/// <summary>Asserts that an object is null.</summary>
		static public void AssertNull(Object anObject) 
		{
			Assertion.AssertNull(string.Empty, anObject);
		}
    
		/// <summary>Asserts that an object is null.</summary>
		static public void AssertNull(string message, Object anObject) 
		{
			Assertion.Assert(message, anObject == null); 
		}
    
		/// <summary>Asserts that two objects refer to the same object. If they
		/// are not the same an <see cref="AssertionFailedError"/> is thrown.
		/// </summary>
		static public void AssertSame(Object expected, Object actual) 
		{
			Assertion.AssertSame(string.Empty, expected, actual);
		}
    
		/// <summary>Asserts that two objects refer to the same object. 
		/// If they are not an <see cref="AssertionFailedError"/> is thrown.
		/// </summary>
		static public void AssertSame(string message, Object expected, Object actual)
		{
			if (expected == actual)
				return;
			Assertion.FailNotSame(message, expected, actual);
		}
    
		/// <summary>Fails a test with no message.</summary>
		static public void Fail() 
		{
			Assertion.Fail(string.Empty);
		}
    
		/// <summary>Fails a test with the given message.</summary>
		static public void Fail(string message) 
		{
			if (message == null)
				message = string.Empty;
			throw new AssertionFailedError(message);
		}
    
		static private void FailNotEquals(string message, Object expected, Object actual) 
		{
			string formatted=string.Empty;
			if (message != null)
				formatted= message+" ";
			Assertion.Fail(formatted+"expected:<"+expected+"> but was:<"+actual+">");
		}
    
		static private void FailNotSame(string message, Object expected, Object actual) 
		{
			string formatted=string.Empty;
			if (message != null)
				formatted= message+" ";
			Assertion.Fail(formatted+"expected same");
		}
	}
}