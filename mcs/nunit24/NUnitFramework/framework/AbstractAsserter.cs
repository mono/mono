// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Framework
{
	/// <summary>
	/// NOTE: The use of asserters for extending NUnit has
	/// now been replaced by the use of constraints. This
	/// class is marked obsolete.
	/// 
	/// AbstractAsserter is the base class for all asserters.
	/// Asserters encapsulate a condition test and generation 
	/// of an AssertionException with a tailored message. They
	/// are used by the Assert class as helper objects.
	/// 
	/// User-defined asserters may be passed to the 
	/// Assert.DoAssert method in order to implement 
	/// extended asserts.
	/// </summary>
	[Obsolete("Use Constraints rather than Asserters for new work")]
	public abstract class AbstractAsserter : IAsserter
	{
		/// <summary>
		/// The user-defined message for this asserter.
		/// </summary>
		protected readonly string userMessage;
		
		/// <summary>
		/// Arguments to use in formatting the user-defined message.
		/// </summary>
		protected readonly object[] args;

		/// <summary>
		/// Our failure message object, initialized as needed
		/// </summary>
		private AssertionFailureMessage failureMessage;

		/// <summary>
		/// Constructs an AbstractAsserter
		/// </summary>
		/// <param name="message">The message issued upon failure</param>
		/// <param name="args">Arguments to be used in formatting the message</param>
		public AbstractAsserter( string message, params object[] args )
		{
			this.userMessage = message;
			this.args = args;
		}

		/// <summary>
		/// AssertionFailureMessage object used internally
		/// </summary>
		protected AssertionFailureMessage FailureMessage
		{
			get
			{
				if ( failureMessage == null )
					failureMessage = new AssertionFailureMessage( userMessage, args );
				return failureMessage;
			}
		}

		#region IAsserter Interface
		/// <summary>
		/// Test method to be implemented by derived types.
		/// Default always succeeds.
		/// </summary>
		/// <returns>True if the test succeeds</returns>
		public abstract bool Test();

		/// <summary>
		/// Message related to a failure. If no failure has
		/// occured, the result is unspecified.
		/// </summary>
		public virtual string Message
		{
			get
			{
				return FailureMessage.ToString();
			}
		}
		#endregion
	}
}
