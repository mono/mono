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
	public class ReturnMessage : IMethodReturnMessage, IMethodMessage, IInternalMessage 
	{
		object[] _outArgs;
		LogicalCallContext _callCtx;
		object _returnValue;
		string _uri;
		Exception _exception;
		MethodBase _methodBase;
		string _methodName;
		object _methodSignature;
		string _typeName;
		MethodReturnDictionary _properties;
		Identity _targetIdentity;

		public ReturnMessage (object returnValue, object [] outArgs,
			       int outArgCount, LogicalCallContext callCtx,
			       IMethodCallMessage request)
		{
			// fixme: why do we need outArgCount?

			_returnValue = returnValue;
			_callCtx = callCtx;
			_uri = request.Uri;
			_outArgs = outArgs;
			_methodBase = request.MethodBase;
			_methodName = request.MethodName;
			_methodSignature = request.MethodSignature;
			_typeName = request.TypeName;
		}

		public ReturnMessage (Exception exc, IMethodCallMessage request)
		{
			_exception = exc;
			_methodBase = request.MethodBase;
			_methodName = request.MethodName;
			_methodSignature = request.MethodSignature;
			_typeName = request.TypeName;
			_outArgs = new object[0];	// .NET does this
		}
		
		public int ArgCount {
			get {
				return (_outArgs != null) ? _outArgs.Length : 0;
			}
		}
		
		public object [] Args {
			get {
				return _outArgs;
			}
		}
		
		public bool HasVarArgs {
			get {
				return false;	//todo: complete
			}
		}

		public LogicalCallContext LogicalCallContext {
			get {
				return _callCtx;
			}
		}

		public MethodBase MethodBase {
			get {
				return _methodBase;
			}
		}

		public string MethodName {
			get {
				return _methodName;
			}
		}

		public object MethodSignature {
			get {
				return _methodSignature;
			}
		}

		public virtual IDictionary Properties {
			get {
				if (_properties == null) _properties = new MethodReturnDictionary (this);
				return _properties;
			}
		}

		public string TypeName {
			get {
				return _typeName;
			}
		}

		public string Uri {
			get {
				return _uri;
			}

			set {
				_uri = value;
			}
		}

		public object GetArg (int arg_num)
		{
			return _outArgs [arg_num];
		}
		
		public string GetArgName (int arg_num)
		{
			return _methodBase.GetParameters()[arg_num].Name;
		}

		public Exception Exception {
			get {
				return _exception;
			}
		}

		public int OutArgCount {
			get {
				return (_outArgs != null) ? _outArgs.Length : 0;
			}
		}

		public object [] OutArgs {
			get {
				return _outArgs;
			}
		}

		public virtual object ReturnValue {
			get {
				return _returnValue;
			}
		}

		public object GetOutArg (int arg_num)
		{
			return _outArgs[arg_num];
		}

		public string GetOutArgName (int arg_num)
		{
			return _methodBase.GetParameters()[arg_num].Name;
		}

		Identity IInternalMessage.TargetIdentity
		{
			get { return _targetIdentity; }
			set { _targetIdentity = value; }
		}
	}
}
