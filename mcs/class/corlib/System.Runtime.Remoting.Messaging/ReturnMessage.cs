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
using System.IO;

namespace System.Runtime.Remoting.Messaging {

	[Serializable]
	public class ReturnMessage : IMethodReturnMessage, IMethodMessage, IInternalMessage 
	{
		object[] _outArgs;
		object[] _args;
		int _outArgsCount;
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
		ArgInfo _inArgInfo;

		public ReturnMessage (object returnValue, object [] outArgs,
			       int outArgCount, LogicalCallContext callCtx,
			       IMethodCallMessage request)
		{
			// outArgCount tells how many values of outArgs are valid

			_returnValue = returnValue;
			_args = outArgs;
			_outArgsCount = outArgCount;
			_callCtx = callCtx;
			_uri = request.Uri;
			_methodBase = request.MethodBase;
			_methodName = request.MethodName;
			_methodSignature = request.MethodSignature;
			_typeName = request.TypeName;
			if (_args == null) _args = new object [outArgCount];
		}

		public ReturnMessage (Exception exc, IMethodCallMessage request)
		{
			_exception = exc;
			
			if (request != null)
			{
				_methodBase = request.MethodBase;
				_methodName = request.MethodName;
				_methodSignature = request.MethodSignature;
				_typeName = request.TypeName;
			}
			_args = new object[0];	// .NET does this
		}
		
		public int ArgCount {
			get {
				return _args.Length;
			}
		}
		
		public object [] Args {
			get {
				return _args;
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
			return _args [arg_num];
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
				if (_args.Length == 0) return 0;
				if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
				return _inArgInfo.GetInOutArgCount ();
			}
		}

		public object [] OutArgs {
			get {
				if (_outArgs == null) {
					_outArgs = new object [OutArgCount];
					Array.Copy (_args, _outArgs, OutArgCount);
				}					
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
			return _args[arg_num];
		}

		public string GetOutArgName (int arg_num)
		{
			if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
			return _inArgInfo.GetInOutArgName(arg_num);
		}

		Identity IInternalMessage.TargetIdentity
		{
			get { return _targetIdentity; }
			set { _targetIdentity = value; }
		}

		public override string ToString ()
		{
			string s = _typeName.Split(',')[0] + "." + _methodName + " (";
			if (_exception != null)
			{
				s += "Exception)\n" + _exception;
			}
			else
			{
				for (int n=0; n<OutArgs.Length; n++)
				{
					if (n>0) s+= ", ";
					if (OutArgs[n] != null) s += OutArgs[n].GetType().Name + " ";
					s += GetOutArgName (n);
					if (OutArgs[n] != null) s += " = {" + OutArgs[n] + "}";
					else s+=" = {null}";
				}
				s += ")";
			}
			return s;
		}
	}
}
