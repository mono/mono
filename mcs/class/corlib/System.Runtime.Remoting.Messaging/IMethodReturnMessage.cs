//
// System.Runtime.Remoting.Messaging.IMethodReturnMessage..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Runtime.Remoting.Messaging {

	public interface IMethodReturnMessage {

		Exception Exception   { get; }
		int       OutArgCount { get; }
		object [] OutArgs     { get; }
		object    ReturnValue { get; }

		object GetOutArg      (int arg_num);
		string GetOutArgName  (int arg_num);
	}
}
