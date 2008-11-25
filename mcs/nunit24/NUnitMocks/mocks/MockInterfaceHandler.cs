// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace NUnit.Mocks
{
	/// <summary>
	/// Summary description for MockInterfaceHandler.
	/// </summary>
	public class MockInterfaceHandler : RealProxy
	{
		private ICallHandler callHandler;

		public MockInterfaceHandler( Type type, ICallHandler callHandler ) : base( type ) 
		{ 
			this.callHandler = callHandler;
		}

		public override IMessage Invoke( IMessage msg )
		{
			IMethodCallMessage call = (IMethodCallMessage)msg;
			IMethodReturnMessage result = null; 

			if ( call != null )
			{
				try
				{
					object ret = callHandler.Call( call.MethodName, call.Args );

					if ( ret == null )
					{
						MethodInfo info = call.MethodBase as MethodInfo;
						Type returnType = info.ReturnType;

						if( returnType == typeof( System.Boolean ) ) ret = false; 

						if( returnType == typeof( System.Byte    ) ) ret = (System.Byte)0;
						if( returnType == typeof( System.SByte   ) ) ret = (System.SByte)0;
						if( returnType == typeof( System.Decimal ) ) ret = (System.Decimal)0;
						if( returnType == typeof( System.Double  ) ) ret = (System.Double)0;
						if( returnType == typeof( System.Single  ) ) ret = (System.Single)0;
						if( returnType == typeof( System.Int32   ) ) ret = (System.Int32)0;
						if( returnType == typeof( System.UInt32  ) ) ret = (System.UInt32)0;
						if( returnType == typeof( System.Int64   ) ) ret = (System.Int64)0;
						if( returnType == typeof( System.UInt64  ) ) ret = (System.UInt64)0;
						if( returnType == typeof( System.Int16   ) ) ret = (System.Int16)0;
						if( returnType == typeof( System.UInt16  ) ) ret = (System.UInt16)0;

						if( returnType == typeof( System.Char	 ) ) ret = '?';
					}

					result = new ReturnMessage( ret, null, 0, null, call );
				} 
				catch( Exception e )
				{
					result = new ReturnMessage( e, call );
				}
			}

			return result;
		}
	}
}

