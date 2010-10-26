//
// System.Runtime.Remoting.Messaging.MethodResponse.cs
//
// Author:	Duncan Mak (duncan@ximian.com)
//		Patrik Torstensson
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
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[Serializable] [CLSCompliant (false)]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public class MethodResponse : IMethodReturnMessage, ISerializable, IInternalMessage, ISerializationRootObject
	{
		string _methodName;
		string _uri;
		string _typeName;
		MethodBase _methodBase;

		object _returnValue;
		Exception _exception;
		Type [] _methodSignature;
		ArgInfo _inArgInfo;

		object []  _args;
		object []  _outArgs;
		IMethodCallMessage _callMsg;
		LogicalCallContext _callContext;
		Identity _targetIdentity;

		protected IDictionary ExternalProperties;
		protected IDictionary InternalProperties;

		public MethodResponse (Header[] h1, IMethodCallMessage mcm)
		{
			if (mcm != null)
			{
				_methodName = mcm.MethodName;
				_uri = mcm.Uri;
				_typeName = mcm.TypeName;
				_methodBase = mcm.MethodBase;
				_methodSignature = (Type[]) mcm.MethodSignature;
				_args = mcm.Args;
			}

			if (h1 != null)
			{
				foreach (Header header in h1)
					InitMethodProperty (header.Name, header.Value);
			}
		}

		internal MethodResponse (Exception e, IMethodCallMessage msg) {
			_callMsg = msg;

			if (null != msg)
				_uri = msg.Uri;
			else
				_uri = String.Empty;
			
			_exception = e;
			_returnValue = null;
			_outArgs = new object[0];	// .NET does this
		}

		internal MethodResponse (object returnValue, object [] outArgs, LogicalCallContext callCtx, IMethodCallMessage msg) {
			_callMsg = msg;

			_uri = msg.Uri;
			
			_exception = null;
			_returnValue = returnValue;
			_args = outArgs;
		}

		internal MethodResponse (IMethodCallMessage msg, CADMethodReturnMessage retmsg) {
			_callMsg = msg;

			_methodBase = msg.MethodBase;
			//_typeName = msg.TypeName;
			_uri = msg.Uri;
			_methodName = msg.MethodName;
			
			// Get unmarshalled arguments
			ArrayList args = retmsg.GetArguments ();

			_exception = retmsg.GetException (args);
			_returnValue = retmsg.GetReturnValue (args);
			_args = retmsg.GetArgs (args);

			_callContext = retmsg.GetLogicalCallContext (args);
			if (_callContext == null) _callContext = new LogicalCallContext ();
			
			if (retmsg.PropertiesCount > 0)
				CADMessageBase.UnmarshalProperties (Properties, retmsg.PropertiesCount, args);
		}

		internal MethodResponse (SerializationInfo info, StreamingContext context) 
		{
			foreach (SerializationEntry entry in info)
				InitMethodProperty (entry.Name, entry.Value);
		}

		internal void InitMethodProperty (string key, object value) 
		{
			switch (key) 
			{
				case "__TypeName": _typeName = (string) value; break;
				case "__MethodName": _methodName = (string) value; break;
				case "__MethodSignature": _methodSignature = (Type[]) value; break;
				case "__Uri": _uri = (string) value; break;
				case "__Return": _returnValue = value; break;
				case "__OutArgs": _args = (object[]) value; break;
				case "__fault": _exception = (Exception) value; break;
				case "__CallContext": _callContext = (LogicalCallContext) value; break;
				default: Properties [key] = value; break;
			}
		}

		public int ArgCount {
			get { 
				if (null == _args)
					return 0;

				return _args.Length;
			}
		}

		public object[] Args {
			get { 
				return _args; 
			}
		}

		public Exception Exception {
			get { 
				return _exception; 
			}
		}
		
		public bool HasVarArgs {
			get { 
				return (MethodBase.CallingConvention | CallingConventions.VarArgs) != 0;
			}
		}
		
		public LogicalCallContext LogicalCallContext {
			get {
				if (_callContext == null)
					_callContext = new LogicalCallContext ();
				return _callContext;
			}
		}
		
		public MethodBase MethodBase {
			get { 
				if (null == _methodBase) {
					if (_callMsg != null)
						_methodBase = _callMsg.MethodBase;
					#if !DISABLE_REMOTING
					else if (MethodName != null && TypeName != null)
						_methodBase = RemotingServices.GetMethodBaseFromMethodMessage (this);
					#endif
				}
				return _methodBase;
			}
		}

		public string MethodName {
			get { 
				if (null == _methodName && null != _callMsg)
					_methodName = _callMsg.MethodName;

				return _methodName;
			}
		}

		public object MethodSignature {
			get { 
				if (null == _methodSignature && null != _callMsg)
					_methodSignature = (Type []) _callMsg.MethodSignature;

				return _methodSignature;
			}
		}

		public int OutArgCount {
			get { 
				if (_args == null || _args.Length == 0) return 0;
				if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
				return _inArgInfo.GetInOutArgCount ();
			}
		}

		public object[] OutArgs {
			get { 
				if (_outArgs == null && _args != null) {
					if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
					_outArgs = _inArgInfo.GetInOutArgs (_args);
				}					
				return _outArgs;
			}
		}

		public virtual IDictionary Properties {
			get { 
				if (null == ExternalProperties) {
					MethodReturnDictionary properties = new MethodReturnDictionary (this);
					ExternalProperties = properties;
					InternalProperties = properties.GetInternalProperties();
				}
				
				return ExternalProperties;
			}
		}

		public object ReturnValue {
			get { 
				return _returnValue;
			}
		}

		public string TypeName {
			get { 
				if (null == _typeName && null != _callMsg)
					_typeName = _callMsg.TypeName;

				return _typeName;
			}
		}

		public string Uri {
			get { 
				if (null == _uri && null != _callMsg)
					_uri = _callMsg.Uri;
				
				return _uri;
			}

			set { 
				_uri = value;
			}
		}

		string IInternalMessage.Uri {
			get { return Uri; }
			set { Uri = value; }
		}

		public object GetArg (int argNum)
		{
			if (null == _args)
				return null;

			return _args [argNum];
		}

		public string GetArgName (int index)
		{
			return MethodBase.GetParameters()[index].Name;
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (_exception == null)
			{
				info.AddValue ("__TypeName", _typeName);
				info.AddValue ("__MethodName", _methodName);
				info.AddValue ("__MethodSignature", _methodSignature);
				info.AddValue ("__Uri", _uri);
				info.AddValue ("__Return", _returnValue);
				info.AddValue ("__OutArgs", _args);
			}
			else
				info.AddValue ("__fault", _exception);

			info.AddValue ("__CallContext", _callContext);

			if (InternalProperties != null) {
				foreach (DictionaryEntry entry in InternalProperties)
					info.AddValue ((string) entry.Key, entry.Value);
			}
		} 

		public object GetOutArg (int argNum)
		{
			if (_args == null) return null;
			if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
			return _args[_inArgInfo.GetInOutArgIndex (argNum)];
		}

		public string GetOutArgName (int index)
		{
			if (null == _methodBase)
				return "__method_" + index;

			if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
			return _inArgInfo.GetInOutArgName(index);
		}

		[MonoTODO]
		public virtual object HeaderHandler (Header[] h)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RootSetObjectData (SerializationInfo info, StreamingContext ctx)
		{
			throw new NotImplementedException ();
		} 

		Identity IInternalMessage.TargetIdentity
		{
			get { return _targetIdentity; }
			set { _targetIdentity = value; }
		}

#if !NET_2_0
		public override string ToString ()
		{
			string s = _typeName.Split(',')[0] + "." + _methodName + " (";
			if (_exception != null)
			{
				s += "Exception\n)" + _exception;
			}
			else
			{
				if (_args != null)
				{
					for (int n=0; n<_args.Length; n++)
					{
						if (n>0) s+= ", ";
						if (_args[n] != null) s += _args[n].GetType().Name + " ";
						s += GetOutArgName (n);
						if (_args[n] != null) s += " = {" + _args[n] + "}";
						else s+=" = {null}";
					}
				}
				s += ")";
			}
			return s;
		}
#endif
	}
}
