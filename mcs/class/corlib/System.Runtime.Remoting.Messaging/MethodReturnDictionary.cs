//
// System.Runtime.Remoting.Messaging.MethodReturnDictionary.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// 2003 (C) Lluis Sanchez Gual
//

using System;

namespace System.Runtime.Remoting.Messaging
{
	internal class MethodReturnDictionary : MethodDictionary
	{
		public static string[] InternalReturnKeys = new string[] {"__Uri", "__MethodName", "__TypeName", "__MethodSignature", "__OutArgs", "__Return", "__CallContext"};
		public static string[] InternalExceptionKeys = new string[] {"__CallContext"};

		public MethodReturnDictionary (IMethodReturnMessage message) : base (message)
		{
			if (message.Exception == null)
				MethodKeys = InternalReturnKeys;
			else
				MethodKeys = InternalExceptionKeys;
		}
	}
}
