//
// System.Runtime.Remoting.Messaging.MethodCall.cs
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
	public class MethodCall : IMethodCallMessage, IMethodMessage, IMessage, ISerializable
	{
		public MethodCall (Header [] headers)
		{
		}

		public MethodCall (IMessage msg)
		{
		}

		protected IDictionary ExternalProperties;
		protected IDictionary InternalProperties;

		[MonoTODO]
		public virtual int ArgCount {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object[] Args {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual bool HasVarArgs {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual int InArgCount {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object[] InArgs {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual LogicalCallContext LogicalCallContext {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual MethodBase MethodBase {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string MethodName {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object MethodSignature {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual IDictionary Properties {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string TypeName {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string Uri {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object GetArg (int argNum)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetArgName (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetInArg (int argNum)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetInArgName (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		} 

		[MonoTODO]
		public virtual object HeaderHandler (Header[] h)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Init ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ResolveMethod ()
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
