// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// Summary description for IMock.
	/// </summary>
	public interface IMock : IVerify, ICallHandler
	{
		/// <summary>
		/// The name of this mock - used in messages
		/// </summary>
		string Name { get; }
	
		/// <summary>
		/// True if unexpected calls should cause an error, false to ignore them
		/// </summary>
		bool Strict { get; set; }

		/// <summary>
		/// Set up to expect a call to a method with a set of arguments
		/// </summary>
		/// <param name="methodName">The name of the method</param>
		/// <param name="args">Arguments for this call</param>
		void Expect( string methodName, params object[] args );

		void Expect( string MethodName );

		/// <summary>
		/// Set up expectation that the named method will not be called
		/// </summary>
		/// <param name="methodName">The name of the method</param>
		void ExpectNoCall( string methodName );

		/// <summary>
		/// Set up to expect a call to a method with a set of arguments.
		/// The specified value will be returned.
		/// </summary>
		/// <param name="methodName">The name of the method</param>
		/// <param name="returnVal">The value to be returned</param>
		/// <param name="args">Arguments for this call</param>
		void ExpectAndReturn( string methodName, object returnVal, params object[] args );

		/// <summary>
		/// Set up to expect a call to a method with a set of arguments.
		/// The specified exception will be thrown.
		/// </summary>
		/// <param name="methodname">The name of the method</param>
		/// <param name="exception">The exception to throw</param>
		/// <param name="args">Arguments for this call</param>
		void ExpectAndThrow( string methodname, Exception exception, params object[] args );

		/// <summary>
		/// Set value to return for a method or property called with any arguments
		/// </summary>
		/// <param name="methodName">The name of the method</param>
		/// <param name="returnVal">The value to be returned</param>
		void SetReturnValue( string methodName, object returnVal );
	}
}
