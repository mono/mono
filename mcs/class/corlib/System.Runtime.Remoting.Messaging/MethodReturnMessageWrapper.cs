//
// System.Runtime.Remoting.Messaging.MethodReturnMessageWrapper.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	public class MethodReturnMessageWrapper : InternalMessageWrapper, IMethodReturnMessage, IMethodMessage, IMessage
	{
		object[] _args;
		ArgInfo _outArgInfo;
		DictionaryWrapper _properties;
		Exception _exception;
		object _return;

		public MethodReturnMessageWrapper (IMethodReturnMessage msg)
			: base (msg)
		{
			_args = ((IMethodCallMessage)WrappedMessage).Args;
			_exception = msg.Exception;
			_outArgInfo = new ArgInfo (msg.MethodBase, ArgInfoType.Out);
		}

		public virtual int ArgCount {
			get { return ((IMethodReturnMessage)WrappedMessage).ArgCount; }
		}

		public virtual object [] Args 
		{
			get { return _args; }
			set { _args = value; }
		}

		public virtual Exception Exception {
			get { return _exception; }
			set { _exception = value; }
		}
		
		public virtual bool HasVarArgs {
			get { return ((IMethodReturnMessage)WrappedMessage).HasVarArgs; }
		}
		
		public virtual LogicalCallContext LogicalCallContext {
			get { return ((IMethodReturnMessage)WrappedMessage).LogicalCallContext; }
		}
		
		public virtual MethodBase MethodBase {
			get { return ((IMethodReturnMessage)WrappedMessage).MethodBase; }
		}

		public virtual string MethodName {
			get { return ((IMethodReturnMessage)WrappedMessage).MethodName; }
		}

		public virtual object MethodSignature {
			get { return ((IMethodReturnMessage)WrappedMessage).MethodSignature; }
		}

		public virtual int OutArgCount {
			get { return _outArgInfo.GetInOutArgCount(); }
		}

		public virtual object[] OutArgs {
			get { return _outArgInfo.GetInOutArgs (_args); }
		}

		public virtual IDictionary Properties 
		{
			get { 
				if (_properties == null) _properties = new DictionaryWrapper(this, WrappedMessage.Properties);
				return _properties; 
			}
		}

		public virtual object ReturnValue {
			get { return _return; }
			set { _return = value; }
		}

		public virtual string TypeName {
			get { return ((IMethodReturnMessage)WrappedMessage).TypeName; }
		}

		public virtual string Uri 
		{
			get { return ((IMethodReturnMessage)WrappedMessage).Uri; }
			set { Properties["__Uri"] = value; }
		}

		public virtual object GetArg (int argNum)
		{
			return _args[argNum];
		}

		public virtual string GetArgName (int index)
		{
			return ((IMethodReturnMessage)WrappedMessage).GetArgName(index);
		}

		[MonoTODO]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		} 

		public virtual object GetOutArg (int argNum)
		{
			return _args [_outArgInfo.GetInOutArgIndex (argNum)];
		}

		public virtual string GetOutArgName (int index)
		{
			return _outArgInfo.GetInOutArgName(index);
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

		class DictionaryWrapper : MethodReturnDictionary
		{
			IDictionary _wrappedDictionary;
			static string[] _keys = new string[] {"__Args", "__Return"};

			public DictionaryWrapper(IMethodReturnMessage message, IDictionary wrappedDictionary) : base (message)
			{
				_wrappedDictionary = wrappedDictionary;
				MethodKeys = _keys;
			}

			protected override IDictionary AllocInternalProperties()
			{
				return _wrappedDictionary;
			}

			protected override void SetMethodProperty (string key, object value)
			{
				if (key == "__Args") ((MethodReturnMessageWrapper)_message)._args = (object[])value;
				else if (key == "__Return") ((MethodReturnMessageWrapper)_message)._return = value;
				else base.SetMethodProperty (key, value);
			}

			protected override object GetMethodProperty (string key)
			{
				if (key == "__Args") return ((MethodReturnMessageWrapper)_message)._args;
				else if (key == "__Return") return ((MethodReturnMessageWrapper)_message)._return;
				else return base.GetMethodProperty (key);
			}
		}
	}
}
