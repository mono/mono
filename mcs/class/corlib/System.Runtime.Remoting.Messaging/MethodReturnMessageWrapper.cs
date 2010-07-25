//
// System.Runtime.Remoting.Messaging.MethodReturnMessageWrapper.cs
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
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[System.Runtime.InteropServices.ComVisible (true)]
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
			if (msg.Exception != null) {
				_exception = msg.Exception;
				_args = new object[0];
			} else {
				_args = msg.Args;
				_return = msg.ReturnValue;
				if (msg.MethodBase != null)
					_outArgInfo = new ArgInfo (msg.MethodBase, ArgInfoType.Out);
			}
		}

		public virtual int ArgCount {
			get { return _args.Length; }
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
			get { return _outArgInfo != null ? _outArgInfo.GetInOutArgCount() : 0; }
		}

		public virtual object[] OutArgs {
			get { return _outArgInfo != null ? _outArgInfo.GetInOutArgs (_args) : _args; }
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

		public string Uri 
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

		public virtual object GetOutArg (int argNum)
		{
			return _args [_outArgInfo.GetInOutArgIndex (argNum)];
		}

		public virtual string GetOutArgName (int index)
		{
			return _outArgInfo.GetInOutArgName(index);
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
