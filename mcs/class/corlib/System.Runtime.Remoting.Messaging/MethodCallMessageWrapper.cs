//
// System.Runtime.Remoting.Messaging.MethodCallMessageWrapper.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Runtime.Remoting.Messaging {

	public class MethodCallMessageWrapper : InternalMessageWrapper, IMethodCallMessage, IMethodMessage, IMessage
	{
		public MethodCallMessageWrapper (IMethodCallMessage msg)
			: base (msg)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual int ArgCount {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object [] Args {
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
		public virtual object [] InArgs {
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
	}
}
