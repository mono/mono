//
// System.Runtime.Remoting.Messaging.MethodCallDictionary.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// 2003 (C) Lluis Sanchez Gual
//

using System;

namespace System.Runtime.Remoting.Messaging
{
	internal class MethodCallDictionary : MethodDictionary
	{
		static string[] _keys = new string[] {"__Uri", "__MethodName", "__TypeName", "__MethodSignature", "__Args", "__CallContext"};

		public MethodCallDictionary(IMethodMessage message) : base (message)
		{
			MethodKeys = _keys;
		}
	}
}
