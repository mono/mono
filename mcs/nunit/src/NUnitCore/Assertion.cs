namespace NUnit.Framework {

  using System;

  /// <summary>A set of Assert methods.</summary>
  public class Assertion {

    /// <summary>Protect constructor since it is a static only class</summary>
    protected Assertion() {
    }
    
    /// <summary>Asserts that a condition is true. If it isn't it throws
    /// an <see cref="AssertionFailedError"/>.</summary>
    static public void Assert(string message, bool condition) {
      if (!condition)
        Fail(message);
    }
    
    /// <summary>Asserts that a condition is true. If it isn't it throws
    /// an <see cref="AssertionFailedError"/>.</summary>
    static public void Assert(bool condition) {
      Assert(null, condition);
    }

    /// <summary>Asserts that two booleans are equal.</summary>
    static public void AssertEquals(bool expected, bool actual) {
      AssertEquals(null, expected, actual);
    }

    /// <summary>Asserts that two bytes are equal.</summary>
    static public void AssertEquals(byte expected, byte actual) {
      AssertEquals(null, expected, actual);
    }

    /// <summary>Asserts that two chars are equal.</summary>
    static public void AssertEquals(char expected, char actual) {
      AssertEquals(null, expected, actual);
    }

    /// <summary>Asserts that two doubles are equal concerning a delta. If the expected
    /// value is infinity then the delta value is ignored.</summary>
    static public void AssertEquals(double expected, double actual, double delta) {
      AssertEquals(null, expected, actual, delta);
    }
    
    /// <summary>Asserts that two floats are equal concerning a delta. If the expected
    /// value is infinity then the delta value is ignored.</summary>
    static public void AssertEquals(float expected, float actual, float delta) {
      AssertEquals(null, expected, actual, delta);
    }
    
    /// <summary>Asserts that two ints are equal.</summary>
    static public void AssertEquals(int expected, int actual) {
      AssertEquals(null, expected, actual);
    }
    
    /// <summary>Asserts that two longs are equal.</summary>
    static public void AssertEquals(long expected, long actual) {
      AssertEquals(null, expected, actual);
    }
    
    /// <summary>Asserts that two objects are equal. If they are not
    /// an <see cref="AssertionFailedError"/> is thrown.</summary>
    static public void AssertEquals(Object expected, Object actual) {
      AssertEquals(null, expected, actual);
    }
    
    /// <summary>Asserts that two shorts are equal.</summary>
    static public void AssertEquals(short expected, short actual) {
      AssertEquals(null, expected, actual);
    }
    
    /// <summary>Asserts that two bools are equal.</summary>
    static public void AssertEquals(string message, bool expected, bool actual) {
      AssertEquals(message, (object)expected, (object)actual);
    }
    
    /// <summary>Asserts that two bytes are equal.</summary>
    static public void AssertEquals(string message, byte expected, byte actual) {
      AssertEquals(message, (object)expected, (object)actual);
    }
    
    /// <summary>Asserts that two chars are equal.</summary>
    static public void AssertEquals(string message, char expected, char actual) {
      AssertEquals(message, (object)expected, (object)actual);
    }
    
    /// <summary>Asserts that two doubles are equal concerning a delta. If the expected
    /// value is infinity then the delta value is ignored.</summary>
    static public void AssertEquals(string message, double expected,
                                    double actual, double delta) {
      // handle infinity specially since subtracting two infinite values gives NaN and the
      // following test fails
      if (double.IsInfinity(expected)) {
        if (!(expected == actual))
          FailNotEquals(message, expected, actual);
      } else if (!(Math.Abs(expected-actual) <= delta))
        FailNotEquals(message, expected, actual);
    }
    
    /// <summary>Asserts that two floats are equal concerning a delta. If the expected
    /// value is infinity then the delta value is ignored.</summary>
    static public void AssertEquals(string message, float expected,
                                    float actual, float delta) {
      // handle infinity specially since subtracting two infinite values gives NaN and the
      // following test fails
      if (double.IsInfinity(expected)) {
        if (!(expected == actual))
          FailNotEquals(message, expected, actual);
      } else if (!(Math.Abs(expected-actual) <= delta))
        FailNotEquals(message, expected, actual);
    }
    
    /// <summary>Asserts that two ints are equal.</summary>
    static public void AssertEquals(string message, int expected, int actual) {
      AssertEquals(message, (object)expected, (object)actual);
    }
    
    /// <summary>Asserts that two longs are equal.</summary>
    static public void AssertEquals(string message, long expected, long actual) {
      AssertEquals(message, (object)expected, (object)actual);
    }
    
    /// <summary>Asserts that two objects are equal. If they are not
    /// an <see cref="AssertionFailedError"/> is thrown.</summary>
    static public void AssertEquals(string message, Object expected,
                                    Object actual) {
      if (expected == null && actual == null)
        return;
      if (expected != null && expected.Equals(actual))
        return;
      FailNotEquals(message, expected, actual);
    }

    /// <summary>Asserts that two shorts are equal.</summary>
    static public void AssertEquals(string message, short expected, short actual) {
      AssertEquals(message, (object)expected, (object)actual);
    }
    
    /// <summary>Asserts that an object isn't null.</summary>
    static public void AssertNotNull(Object anObject) {
      AssertNotNull(null, anObject);
    }
    
    /// <summary>Asserts that an object isn't null.</summary>
    static public void AssertNotNull(string message, Object anObject) {
      Assert(message, anObject != null); 
    }
    
    /// <summary>Asserts that an object is null.</summary>
    static public void AssertNull(Object anObject) {
      AssertNull(null, anObject);
    }
    
    /// <summary>Asserts that an object is null.</summary>
    static public void AssertNull(string message, Object anObject) {
      Assert(message, anObject == null); 
    }
    
    /// <summary>Asserts that two objects refer to the same object. If they
    /// are not the same an <see cref="AssertionFailedError"/> is thrown.
    /// </summary>
    static public void AssertSame(Object expected, Object actual) {
      AssertSame(null, expected, actual);
    }
    
    /// <summary>Asserts that two objects refer to the same object. 
    /// If they are not an <see cref="AssertionFailedError"/> is thrown.
    /// </summary>
    static public void AssertSame(string message, Object expected,
                                  Object actual) {
      if (expected == actual)
        return;
      FailNotSame(message, expected, actual);
    }
    
    /// <summary>Fails a test with no message.</summary>
    static public void Fail() {
      Fail(null);
    }
    
    /// <summary>Fails a test with the given message.</summary>
    static public void Fail(string message) {
      if (message == null)
        message = "";
      throw new AssertionFailedError(message);
    }
    
    static private void FailNotEquals(string message, Object expected,
                                      Object actual) {
      string formatted= "";
      if (message != null)
        formatted= message+" ";
      Fail(formatted+"expected:<"+expected+"> but was:<"+actual+">");
    }
    
    static private void FailNotSame(string message, Object expected, Object actual) {
      string formatted= "";
      if (message != null)
        formatted= message+" ";
      Fail(formatted+"expected same");
    }
  }
}
