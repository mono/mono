//
// System.Runtime.Remoting.Messaging.MonoMethodMessage.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//   Patrik Torstensson
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Messaging {
	
	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	internal class MonoMethodMessage : IMethodCallMessage, IMethodReturnMessage, IInternalMessage {

#pragma warning disable 649
		#region keep in sync with MonoMessage in object-internals.h
		MonoMethod method;
		object []  args;
		string []  names;
		byte [] arg_types; /* 1 == IN; 2 == OUT; 3 == INOUT; 4 == COPY OUT */
		public LogicalCallContext ctx;
		public object rval;
		public Exception exc;
		AsyncResult asyncResult;
		CallType call_type;
		#endregion
#pragma warning restore 649

		string uri;

		MCMDictionary properties;

		Type[] methodSignature;

		Identity identity;

		internal void InitMessage (MonoMethod method, object [] out_args)
		{
			this.method = method;
			ParameterInfo[] paramInfo = method.GetParametersInternal ();
			int param_count = paramInfo.Length;
			args = new object[param_count];
			arg_types = new byte[param_count];
			asyncResult = null;
			call_type = CallType.Sync;
			names = new string[param_count];
			for (int i = 0; i < param_count; i++) {
				names[i] = paramInfo[i].Name;
			}
			bool hasOutArgs = out_args != null;
			int j = 0;
			for (int i = 0; i < param_count; i++) {
				byte arg_type;
				bool isOut = paramInfo[i].IsOut;
				if (paramInfo[i].ParameterType.IsByRef) {
					if (hasOutArgs)
						args[i] = out_args[j++];
					arg_type = 2; // OUT
					if (!isOut)
						arg_type |= 1; // INOUT
				} else {
					arg_type = 1; // IN
					if (isOut)
						arg_type |= 4; // IN, COPY OUT
				}
				arg_types[i] = arg_type;
			}
		}

		public MonoMethodMessage (MethodBase method, object [] out_args)
		{
			if (method != null)
				InitMessage ((MonoMethod)method, out_args);
			else
				args = null;
		}

		internal MonoMethodMessage (MethodInfo minfo, object [] in_args, object [] out_args)
		{
			InitMessage ((MonoMethod)minfo, out_args);

			int len = in_args.Length;
			for (int i = 0; i < len; i++) {
				args [i] = in_args [i];
			}
		}

		private static MethodInfo GetMethodInfo (Type type, string methodName)
		{
			// fixme: consider arg types
			MethodInfo minfo = type.GetMethod(methodName);
			if (minfo == null)
				throw new ArgumentException (String.Format("Could not find '{0}' in {1}", methodName, type), "methodName");
			return minfo;
		}
		
		public MonoMethodMessage (Type type, string methodName, object [] in_args)
			: this (GetMethodInfo (type, methodName), in_args, null)
		{
		}

		public IDictionary Properties {
			get {
				if (properties == null) properties = new MCMDictionary (this);
				return properties;
			}
		}

		public int ArgCount {
			get {
				if (CallType == CallType.EndInvoke)
					return -1;
					
				if (null == args)
					return 0;

				return args.Length;
			}
		}
		
		public object [] Args {
			get {
				return args;
			}
		}
		
		public bool HasVarArgs {
			get {
				return false;
			}
		}

		public LogicalCallContext LogicalCallContext {
			get {
				return ctx;
			}

			set {
				ctx = value;
			}
		}

		public MethodBase MethodBase {
			get {
				return method;
			}
		}

		public string MethodName {
			get {
				if (null == method)
					return String.Empty;

				return method.Name;
			}
		}

		public object MethodSignature {
			get {
				if (methodSignature == null) {
					ParameterInfo[] parameters = method.GetParameters();
					methodSignature = new Type[parameters.Length];
					for (int n=0; n<parameters.Length; n++)
						methodSignature[n] = parameters[n].ParameterType;
				}
				return methodSignature;
			}
		}

		public string TypeName {
			get {
				if (null == method)
					return String.Empty;

				return method.DeclaringType.AssemblyQualifiedName;
			}
		}

		public string Uri {
			get {
				return uri;
			}

			set {
				uri = value;
			}
		}

		public object GetArg (int arg_num)
		{
			if (null == args)
				return null;

			return args [arg_num];
		}
		
		public string GetArgName (int arg_num)
		{
			if (null == args)
				return String.Empty;

			return names [arg_num];
		}

		public int InArgCount {
			get {
				if (CallType == CallType.EndInvoke)
					return -1;

				if (null == args)
					return 0;

				int count = 0;

				foreach (byte t in arg_types) {
					if ((t & 1) != 0) count++;
						
				}
				return count;
			}
		}
		
		public object [] InArgs {
			get {                
				int i, j, count = InArgCount;
				object [] inargs = new object [count];

				i = j = 0;
				foreach (byte t in arg_types) {
					if ((t & 1) != 0)
						inargs [j++] = args [i];
					i++;
				}
				
				return inargs;
			}
		}
		
		public object GetInArg (int arg_num)
		{
			int i = 0, j = 0;
			foreach (byte t in arg_types) {
				if ((t & 1) != 0) {
					if (j++ == arg_num)
						return args [i]; 
				}
				i++;
			}
			return null;
		}
		
		public string GetInArgName (int arg_num)
		{
			int i = 0, j = 0;
			foreach (byte t in arg_types) {
				if ((t & 1) != 0) {
					if (j++ == arg_num)
						return names [i]; 
				}
				i++;
			}
			return null;
		}

		public Exception Exception {
			get {
				return exc;
			}
		}
		
		public int OutArgCount {
			get {
				if (null == args)
					return 0;
		                
				int count = 0;

				foreach (byte t in arg_types) {
					if ((t & 2) != 0) count++;
						
				}
				return count;
			}
		}
		
		public object [] OutArgs {
			get {
				if (null == args)
					return null;

				int i, j, count = OutArgCount;
				object [] outargs = new object [count];

				i = j = 0;
				foreach (byte t in arg_types) {
					if ((t & 2) != 0)
						outargs [j++] = args [i];
					i++;
				}
				
				return outargs;
			}
		}
		
		public object ReturnValue {
			get {
				return rval;
			}
		}

		public object GetOutArg (int arg_num)
		{
			int i = 0, j = 0;
			foreach (byte t in arg_types) {
				if ((t & 2) != 0) {
					if (j++ == arg_num)
						return args [i]; 
				}
				i++;
			}
			return null;
		}
		
		public string GetOutArgName (int arg_num)
		{
			int i = 0, j = 0;
			foreach (byte t in arg_types) {
				if ((t & 2) != 0) {
					if (j++ == arg_num)
						return names [i]; 
				}
				i++;
			}
			return null;
		}

		Identity IInternalMessage.TargetIdentity
		{
			get { return identity; }
			set { identity = value; }
		}

		bool IInternalMessage.HasProperties()
		{
			return properties != null;
		}

		public bool IsAsync
		{
			get { return asyncResult != null; }
		}

		public AsyncResult AsyncResult
		{
			get { return asyncResult; }
		}

		internal CallType CallType
		{
			get
			{
				// FIXME: ideally, the OneWay type would be set by the runtime
				
				if (call_type == CallType.Sync && RemotingServices.IsOneWay (method))
					call_type = CallType.OneWay;
				return call_type;
			}
		}
		
		public bool NeedsOutProcessing (out int outCount) {
			bool res = false;
			outCount = 0;
			foreach (byte t in arg_types) {
				if ((t & 2) != 0)
					outCount++;
				else if ((t & 4) != 0)
					res = true;
			}
			return outCount > 0 || res;
		}
		
	}

	internal enum CallType: int
	{
		Sync = 0,
		BeginInvoke = 1,
		EndInvoke = 2,
		OneWay = 3
	}
}

