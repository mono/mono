namespace NUnit.Framework {
    using System;

    /// <summary>
    /// The given exception should be thrown by the annotated method.
    /// </summary>
    /// <remarks>
    /// To use this attribute, attach it to a method in a
    /// <see cref="TestCase"/> subclass.
    /// <example>Here is an example:
    /// <code>
    /// public class FooTest : TestCase {
    ///   public ExpectExceptionTest(string name) : base(name) {}
    ///   [ExpectException(typeof(ArgumentException))]
    ///   [ExpectException(typeof(IndexOutOfRangeException))]
    ///   public void TestBar() {
    ///     throw new ArgumentException("bad argument");
    ///   }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public class ExpectExceptionAttribute : Attribute {
        private Type expected;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exceptionExpected"></param>
        public ExpectExceptionAttribute(Type exceptionExpected) {
            this.expected = exceptionExpected;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public override string ToString() {
            return expected.ToString();
        }
		/// <summary>
		/// 
		/// </summary>
        public Type ExceptionExpected {
            get { return expected; }
        }
    }
}
