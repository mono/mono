//
// System.Runtime.Remoting.Messaging.ReturnMessage.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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
using System.IO;

namespace System.Runtime.Remoting.Messaging 
{
	public class ReturnMessage : IMethodReturnMessage, IMethodMessage, IMessage, IInternalMessage 
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
		Type [] _methodSignature;
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
			if (_args == null) _args = new object [outArgCount];
		}

		public ReturnMessage (Exception exc, IMethodCallMessage request)
		{
			_exception = exc;
			
			if (request != null)
				_methodBase = request.MethodBase;
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
				return (MethodBase.CallingConvention | CallingConventions.VarArgs) != 0;
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
				if (_methodBase != null && _methodName == null)
					_methodName = _methodBase.Name;
				return _methodName;
			}
		}

		public object MethodSignature {
			get {
				if (_methodBase != null && _methodSignature == null) {
					ParameterInfo[] parameters = _methodBase.GetParameters();
					_methodSignature = new Type [parameters.Length];
					for (int n=0; n<parameters.Length; n++)
						_methodSignature[n] = parameters[n].ParameterType;
				}
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

				// lazily fill in _typeName from _methodBase
				if (_methodBase != null && _typeName == null)
					_typeName = _methodBase.DeclaringType.AssemblyQualifiedName;
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
				if (_args == null || _args.Length == 0) return 0;
				if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
				return _inArgInfo.GetInOutArgCount ();
			}
		}

		public object [] OutArgs {
			get {
				if (_outArgs == null && _args != null) {
					if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
					_outArgs = _inArgInfo.GetInOutArgs (_args);
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
			if (_inArgInfo == null) _inArgInfo = new ArgInfo (MethodBase, ArgInfoType.Out);
			return _args[_inArgInfo.GetInOutArgIndex (arg_num)];
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
