//
// System.Runtime.Remoting.Messaging.IMethodMessage..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;

namespace System.Runtime.Remoting.Messaging {

	public interface IMethodMessage : IMessage {
		int       ArgCount        {
			get;
		}
		
		object [] Args            {
			get;
		}
		
		bool      HasVarArgs      {
			get;
		}

		LogicalCallContext LogicalCallContext {
			get;
		}

		MethodBase MethodBase {
			get;
		}

		string MethodName {
			get;
		}

		object MethodSignature {
			get;
		}

		string TypeName {
			get;
		}

		string Uri {
			get;
		}

		object GetArg     (int arg_num);
		string GetArgName (int arg_num);
	}
}

