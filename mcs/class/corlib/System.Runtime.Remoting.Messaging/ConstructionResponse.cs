//
// System.Runtime.Remoting.Messaging.ConstructionResponse.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Collections;
using System.Runtime.Remoting.Activation;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable] [CLSCompliant (false)]
	public class ConstructionResponse: MethodResponse, IConstructionReturnMessage
	{
		public ConstructionResponse (Header[] headers, IMethodCallMessage mcm)
			: base (headers, mcm)
		{
		}
		
		internal ConstructionResponse(object resultObject, LogicalCallContext callCtx, IMethodCallMessage msg)
			: base (resultObject, null, callCtx, msg)
		{
		}

		internal ConstructionResponse (Exception e, IMethodCallMessage msg): base (e, msg)
		{
		}
		
		internal ConstructionResponse (SerializationInfo info, StreamingContext context): base (info, context)
		{
		}
		
		public override IDictionary Properties 
		{
			get { return base.Properties; }
		}
	}
}
