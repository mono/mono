//
// System.Runtime.Remoting.RemotingServices.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting
{
	public sealed class RemotingServices {

		public static IMethodReturnMessage ExecuteMessage (
		        MarshalByRefObject target, IMethodCallMessage reqMsg)
		{
			throw new NotImplementedException ();
		}
								   
		
	}
}
