//
// System.Runtime.Remoting.Messaging.MethodCallMessageWrapper.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Runtime.Remoting.Messaging {

	public class MethodCallMessageWrapper : InternalMessageWrapper, IMethodCallMessage, IMethodMessage, IMessage
	{
		object[] _args;
		ArgInfo _inArgInfo;
		DictionaryWrapper _properties;

		public MethodCallMessageWrapper (IMethodCallMessage msg)
			: base (msg)
		{
			_args = ((IMethodCallMessage)WrappedMessage).Args;
			_inArgInfo = new ArgInfo (msg.MethodBase, ArgInfoType.In);
		}
		
		public virtual int ArgCount {
			get { return ((IMethodCallMessage)WrappedMessage).ArgCount; }
		}

		public virtual object [] Args {
			get { return _args; }
			set { _args = value; }
		}
		
		public virtual bool HasVarArgs {
			get { return ((IMethodCallMessage)WrappedMessage).HasVarArgs; }
		}

		public int InArgCount {
			get  { return _inArgInfo.GetInOutArgCount(); }
		}

		public object[] InArgs {
			get { return _inArgInfo.GetInOutArgs (_args); }
		}
		
		public virtual LogicalCallContext LogicalCallContext {
			get { return ((IMethodCallMessage)WrappedMessage).LogicalCallContext; }
		}
		
		public virtual MethodBase MethodBase {
			get { return ((IMethodCallMessage)WrappedMessage).MethodBase; }
		}

		public virtual string MethodName {
			get { return ((IMethodCallMessage)WrappedMessage).MethodName; }
		}

		public virtual object MethodSignature {
			get { return ((IMethodCallMessage)WrappedMessage).MethodSignature; }
		}
		
		public virtual IDictionary Properties 
		{
			get 
			{ 
				if (_properties == null) _properties = new DictionaryWrapper(this, WrappedMessage.Properties);
				return _properties; 
			}
		}

		public virtual string TypeName {
			get { return ((IMethodCallMessage)WrappedMessage).TypeName; }
		}

		public virtual string Uri {
			get { return ((IMethodCallMessage)WrappedMessage).Uri; }
			set { Properties["__Uri"] = value; }
		}

		public virtual object GetArg (int argNum)
		{
			return _args[argNum];
		}

		public virtual string GetArgName (int index)
		{
			return ((IMethodCallMessage)WrappedMessage).GetArgName (index);
		}

		public object GetInArg (int argNum)
		{
			return _args[_inArgInfo.GetInOutArgIndex (argNum)];
		}

		public string GetInArgName (int index)
		{
			return _inArgInfo.GetInOutArgName(index);
		}

		class DictionaryWrapper : MethodCallDictionary
		{
			IDictionary _wrappedDictionary;
			static string[] _keys = new string[] {"__Args"};

			public DictionaryWrapper(IMethodMessage message, IDictionary wrappedDictionary) : base (message)
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
				if (key == "__Args") ((MethodCallMessageWrapper)_message)._args = (object[])value;
				else base.SetMethodProperty (key, value);
			}

			protected override object GetMethodProperty (string key)
			{
				if (key == "__Args") return ((MethodCallMessageWrapper)_message)._args;
				else return base.GetMethodProperty (key);
			}
		}
	}
}
