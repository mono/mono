//
// System.Runtime.Remoting.Messaging.IMethodCallMessage.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;

namespace System.Runtime.Remoting.Messaging {

	public interface IMethodCallMessage : IMethodMessage {

		int InArgCount {
			get;
		}
		
		object [] InArgs {
			get;
		}
		
		object GetInArg (int arg_num);
		string GetInArgName (int arg_num);
	}
}

