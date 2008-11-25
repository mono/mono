// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NUnit.Mocks
{
	/// <summary>
	/// Summary description for ExpectedCall.
	/// </summary>
	public class MockCall : ICall
	{
		private MethodSignature signature;
		private object returnVal;
		private Exception exception;
		private object[] expectedArgs;

//		public static object[] Any = new object[0];

		public MockCall( MethodSignature signature, object  returnVal, Exception exception, params object[] args )
		{
			this.signature = signature;
			this.returnVal = returnVal;
			this.exception = exception;
			this.expectedArgs = args;
		}

		public object Call( object[] actualArgs )
		{
			if ( expectedArgs != null )
			{
				Assert.AreEqual( expectedArgs.Length, actualArgs.Length, "Invalid argument count in call to {0}", this.signature.methodName );

				for( int i = 0; i < expectedArgs.Length; i++ )
				{
					if ( expectedArgs[i] is Constraint )
						Assert.That( actualArgs[i], (Constraint)expectedArgs[i] );
					else
						Assert.AreEqual( expectedArgs[i], actualArgs[i] );
				}
			}
			
			if ( exception != null )
				throw exception;

			return returnVal;
		}
	}
}
