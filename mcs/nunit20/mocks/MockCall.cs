using System;
using NUnit.Framework;

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
//			if ( expectedArgs.Length != 0 )
			{
				//Assert.IsTrue( signature.IsCompatibleWith( actualArgs ) );
				Assert.AreEqual( expectedArgs.Length, actualArgs.Length );

				for( int i = 0; i < expectedArgs.Length; i++ )
					Assert.AreEqual( expectedArgs[i], actualArgs[i] );
			}
			
			if ( exception != null )
				throw exception;

			return returnVal;
		}
	}
}
