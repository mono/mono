//
// System.Runtime.Remoting.Messaging.MethodResponse.cs
//
// Author:	Duncan Mak (duncan@ximian.com)
//		Patrik Torstensson
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[Serializable] [CLSCompliant (false)]
	public class MethodResponse : IMethodReturnMessage, ISerializable, IInternalMessage
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

		public MethodResponse (Header[] headers, IMethodCallMessage mcm)
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

			if (headers != null)
			{
				foreach (Header header in headers)
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
			_outArgs = outArgs;
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
			_outArgs = retmsg.GetArgs (args);

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
				case "__OutArgs": _outArgs = (object[]) value; break;
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
				return false;	// TODO: implement var args
			}
		}
		
		public LogicalCallContext LogicalCallContext {
			get { 
				return _callContext;
			}
		}
		
		public MethodBase MethodBase {
			get { 
				if (null == _methodBase && null != _callMsg)
					_methodBase = _callMsg.MethodBase;

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
				if (null == _outArgs)
					return 0;

				return _outArgs.Length;
			}
		}

		public object[] OutArgs {
			get { 
				if (null == _outArgs)
					return new object[0];

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

		public object GetArg (int argNum)
		{
			if (null == _outArgs)
				return null;

			return _outArgs [argNum];
		}

		public string GetArgName (int index)
		{
			throw new NotSupportedException ();
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
				info.AddValue ("__OutArgs", _outArgs);
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
			if (null == _methodBase)
				return null;

			return _outArgs [argNum];
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
		public void RootSetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		} 

		Identity IInternalMessage.TargetIdentity
		{
			get { return _targetIdentity; }
			set { _targetIdentity = value; }
		}
	}
}
