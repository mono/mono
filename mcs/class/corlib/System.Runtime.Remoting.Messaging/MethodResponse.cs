//
// System.Runtime.Remoting.Messaging.MethodResponse.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[Serializable] [CLSCompliant (false)]
	public class MethodResponse : IMethodReturnMessage, IMethodMessage, IMessage, ISerializable
	{
		public MethodResponse (Header[] headers, IMethodCallMessage mcm)
		{
		}

		protected IDictionary ExternalProperties;
		protected IDictionary InternalProperties;

		[MonoTODO]
		public int ArgCount {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object[] Args {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Exception Exception {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public bool HasVarArgs {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public LogicalCallContext LogicalCallContext {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public MethodBase MethodBase {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string MethodName {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object MethodSignature {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int OutArgCount {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object[] OutArgs {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual IDictionary Properties {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object ReturnValue {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string TypeName {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Uri {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object GetArg (int argNum)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetArgName (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		} 

		[MonoTODO]
		public object GetOutArg (int argNum)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetOutArgName (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object HeaderHandler (Header[] h)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RootSetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		} 
	}
}
