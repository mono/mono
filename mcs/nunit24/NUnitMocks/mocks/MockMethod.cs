// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections;
using NUnit.Framework;

namespace NUnit.Mocks
{
	/// <summary>
	/// The MockMethod object represents one named method on a mock object.
	/// All overloads are represented by one MockMethod. A method may return
	/// a fixed value, throw a fixed exception or have an expected sequence
	/// of calls. If it has a call sequence, then the signature must match and
	/// each call provides it's own return value or exception.
	/// </summary>
	public class MockMethod : IMethod
	{
		#region Private Fields

		/// <summary>
		/// Name of this method
		/// </summary>
		private string methodName;
		
		/// <summary>
		/// Fixed return value
		/// </summary>
		private object returnVal;
		
		/// <summary>
		/// Exception to be thrown
		/// </summary>
		private Exception exception;

		/// <summary>
		/// Expected call sequence. If null, this method has no expectations
		/// and simply provides a fixed return value or exception.
		/// </summary>
		private ArrayList expectedCalls = null;
		
		/// <summary>
		/// Actual sequence of calls... currently not used
		/// </summary>
		//private ArrayList actualCalls = null;

		#endregion

		#region Constructors

		public MockMethod( string methodName ) 
			: this( methodName, null, null ) { }

		public MockMethod( string methodName, object returnVal ) 
			: this( methodName, returnVal, null ) { }

		public MockMethod( string methodName, object returnVal, Exception exception )
		{
			this.methodName = methodName;
			this.returnVal = returnVal;
			this.exception = exception;
		}

		#endregion

		#region IMethod Members

		public string Name
		{
			get { return methodName; }
		}

		public void Expect( ICall call )
		{
			if ( expectedCalls == null )
				expectedCalls = new ArrayList();

			expectedCalls.Add( call );
		}

		#endregion

		#region ICall Members

		public object Call( object[] args )
		{
			if ( expectedCalls == null )
			{
				if ( exception != null )
					throw exception;

				return returnVal;
			}
			else
			{
				//actualCalls.Add( new MethodCall( methodName, null, null, args ) );
				Assert.IsTrue( expectedCalls.Count > 0, "Too many calls to " + Name );
				MockCall mockCall = (MockCall)expectedCalls[0];
				expectedCalls.RemoveAt( 0 );
				return mockCall.Call( args );
			}
		}

		#endregion

		#region IVerify Members

		public void Verify()
		{
			if ( expectedCalls != null )
				Assert.IsTrue( expectedCalls.Count == 0, "Not all methods were called" );
		}

		#endregion
	}
}
