//
// System.Runtime.Remoting.Messaging.ReturnMessage.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Runtime.Remoting.Messaging {

	[Serializable]
	public class ReturnMessage : IMethodReturnMessage, IMethodMessage 
	{
		MonoMethodMessage msg;
		IMethodCallMessage request;
		
		public ReturnMessage (object returnValue, object [] outArgs,
			       int outArgCount, LogicalCallContext callCtx,
			       IMethodCallMessage request)
		{
			// fixme: request can be null
			// fixme: why do we need outArgCount?
			msg = new MonoMethodMessage (request.MethodBase as MonoMethod, outArgs);
			this.request = request;
			msg.rval = returnValue;
			msg.ctx = callCtx;
			msg.Uri = request.Uri;
		}

		public ReturnMessage (Exception exc, IMethodCallMessage request)
		{
			if (null != request) {
				msg = new MonoMethodMessage (request.MethodBase as MonoMethod, null);
				msg.ctx = request.LogicalCallContext;
			}
			else
				msg = new MonoMethodMessage (null, null);

			this.request = request;
			msg.exc = exc;
		}
		
		public int ArgCount {
			get {
				return msg.ArgCount;
			}
		}
		
		public object [] Args {
			get {
				return msg.Args;
			}
		}
		
		public bool HasVarArgs {
			get {
				return msg.HasVarArgs;
			}
		}

		public LogicalCallContext LogicalCallContext {
			get {
				if (null == msg)
					return null;
				return msg.ctx;
			}
		}

		public MethodBase MethodBase {
			get {
				return msg.MethodBase;
			}
		}

		public string MethodName {
			get {
				return msg.MethodName;
			}
		}

		public object MethodSignature {
			get {
				return msg.MethodSignature;
			}
		}

		public virtual IDictionary Properties {
			get {
				return msg.Properties;
			}
		}

		public string TypeName {
			get {
				return msg.TypeName;
			}
		}

		public string Uri {
			get {
				return msg.Uri;
			}

			set {
				msg.Uri = value;
			}
		}

		public object GetArg (int arg_num)
		{
			return msg.GetArg (arg_num);
		}
		
		public string GetArgName (int arg_num)
		{
			return msg.GetArgName (arg_num);
		}

		public Exception Exception {
			get {
				return msg.exc;
			}
		}

		public int OutArgCount {
			get {
				return msg.OutArgCount;
			}
		}

		public object [] OutArgs {
			get {
				return msg.OutArgs;
			}
		}

		public virtual object ReturnValue {
			get {
				return msg.rval;
			}
		}

		public object GetOutArg (int arg_num)
		{
			return msg.GetOutArg (arg_num);
		}

		public string GetOutArgName (int arg_num)
		{
			return msg.GetOutArgName (arg_num);
		}


		class InternalDictionary : MethodReturnDictionary
		{
			public InternalDictionary(ReturnMessage message) : base (message) { }

			protected override void SetMethodProperty (string key, object value)
			{
				if (key == "__Uri") ((ReturnMessage)_message).Uri = (string)value;
				else base.SetMethodProperty (key, value);
			}
		}
	}
}
