namespace NUnit.Framework 
{
	using System;
	using System.Diagnostics;
	using System.Runtime.Serialization;
  
	/// <summary>
	/// Thrown when an assertion failed. Here to preserve the inner
	/// exception and hence its stack trace.
	/// </summary>
	[Serializable]
	public class NUnitException : ApplicationException 
	{
		/// <summary>
		/// Serialization Constructor
		/// </summary>
		protected NUnitException(SerializationInfo info, 
			StreamingContext context) : base(info,context){}
		/// <summary>
		/// Standard constructor
		/// </summary>
		/// <param name="message">The error message that explains 
		/// the reason for the exception</param>
		public NUnitException(string message) : base (message){}
		/// <summary>
		/// Standard constructor
		/// </summary>
		/// <param name="message">The error message that explains 
		/// the reason for the exception</param>
		/// <param name="inner">The exception that caused the 
		/// current exception</param>
		public NUnitException(string message, Exception inner) :
			base(message, inner) {}

		/// <summary>
		/// Indicates that this exception wraps an AssertionFailedError
		/// exception
		/// </summary>
		public virtual bool IsAssertionFailure 
		{
			get
			{
				AssertionFailedError inner = this.InnerException 
					as AssertionFailedError;
				if(inner != null)
					return true;
				return false;
			}
		}
	}
}