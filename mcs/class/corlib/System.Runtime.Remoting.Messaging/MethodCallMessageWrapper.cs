//
// System.Runtime.Remoting.Messaging.MethodCallMessageWrapper.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Runtime.Remoting.Messaging {

	[System.Runtime.InteropServices.ComVisible (true)]
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

		public virtual int InArgCount {
			get { return _inArgInfo.GetInOutArgCount(); }
		}

		public virtual object[] InArgs {
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
			set {
				IInternalMessage im = WrappedMessage as IInternalMessage;
				if (im != null) im.Uri = value;
				else Properties["__Uri"] = value; 
			}
		}

		public virtual object GetArg (int argNum)
		{
			return _args[argNum];
		}

		public virtual string GetArgName (int index)
		{
			return ((IMethodCallMessage)WrappedMessage).GetArgName (index);
		}

		public virtual object GetInArg (int argNum)
		{
			return _args[_inArgInfo.GetInOutArgIndex (argNum)];
		}

		public virtual string GetInArgName (int index)
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
