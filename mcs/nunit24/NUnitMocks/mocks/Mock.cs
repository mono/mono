// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using NUnit.Framework;

namespace NUnit.Mocks
{
	/// <summary>
	/// Summary description for MockObject.
	/// </summary>
	public class Mock : IMock
	{
		#region Private Fields

		private string name;

		private bool strict;

		private IDictionary methods = new Hashtable();

		private Exception lastException;

		#endregion

		#region Properties

		public Exception LastException
		{
			get { return lastException; }
		}

		#endregion

		#region Constructors

		public Mock() : this( "Mock" ) { }

		public Mock( string name )
		{
			this.name = name;
		}

		#endregion

		#region IMock Members

		public string Name
		{
			get { return name; }
		}

		public bool Strict
		{
			get { return strict; }
			set { strict = value; }
		}

		public void Expect( string methodName, params object[] args )
		{
			ExpectAndReturn( methodName, null, args );
		}

		public void Expect( string methodName )
		{
			ExpectAndReturn( methodName, null, null );
		}

		public void ExpectNoCall( string methodName )
		{
			methods[methodName] = new MockMethod( methodName, null, 
				new AssertionException("Unexpected call to method " + methodName) );
		}

		public void ExpectAndReturn( string methodName, object returnVal, params object[] args )
		{
			AddExpectedCall( methodName, returnVal, null, args );
		}

		public void ExpectAndThrow( string methodName, Exception exception, params object[] args )
		{
			AddExpectedCall( methodName, null, exception, args );
		}

		public void SetReturnValue( string methodName, object returnVal )
		{
			methods[methodName] = new MockMethod( methodName, returnVal );
		}

		#endregion

		#region IVerify Members

		public virtual void Verify()
		{
			foreach( IMethod method in methods.Values )
				method.Verify();
		}

		#endregion

		#region ICallHandler Members

		public virtual object Call( string methodName, params object[] args )
		{
			if ( methods.Contains( methodName ) )
			{
				try
				{
					IMethod method = (IMethod)methods[methodName];
					return method.Call( args );
				}
				catch( Exception exception )
				{
					// Save exception in case MO is running on a separate thread
					lastException = exception;
					throw;
				}
			}
			else // methodName is not listed in methods
			if ( Strict )
				Assert.Fail( "Unexpected call to " + methodName );
			
			// not listed but Strict is not specified
			return null;
		}

		#endregion
	
		#region Helper Methods

		private void AddExpectedCall( string methodName, object returnVal, Exception exception, object[] args )
		{
			IMethod method = (IMethod)methods[methodName];
			if ( method == null )
			{
				method = new MockMethod( methodName );
				methods[methodName] = method;
			}

			Type[] argTypes = MethodSignature.GetArgTypes( args );
			MethodSignature signature = new MethodSignature( this.Name, methodName, argTypes );

			method.Expect( new MockCall( signature, returnVal, exception, args ) );
		}

		#endregion
	}
}
