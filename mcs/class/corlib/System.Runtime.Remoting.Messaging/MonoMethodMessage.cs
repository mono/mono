//
// System.Runtime.Remoting.Messaging.MonoMethodMessage.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting.Messaging {
	
	public class MonoMethodMessage : IMethodCallMessage, IMethodReturnMessage {

		MonoMethod method;

		object []  args;

		string []  names;

		byte [] arg_types; /* 1 == IN; 2 == OUT ; 3 = INOUT */

		public LogicalCallContext ctx;

		public object rval;

		public Exception exc;

		string uri;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern void InitMessage (MonoMethod method, object [] out_args);

		public MonoMethodMessage (MethodBase method, object [] out_args)
		{
			InitMessage ((MonoMethod)method, out_args);			
		}

		public MonoMethodMessage (Type type, string method_name, object [] in_args)
		{
			// fixme: consider arg types
			MethodInfo minfo = type.GetMethod (method_name);
			
			InitMessage ((MonoMethod)minfo, null);

			int len = in_args.Length;
			for (int i = 0; i < len; i++) {
				args [i] = in_args [i];
			}
		}
		
		public IDictionary Properties {
			get {
				return null;
			}
		}

		public int ArgCount {
			get {
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
		}

		public MethodBase MethodBase {
			get {
				return method;
			}
		}

		public string MethodName {
			get {
				return method.Name;
			}
		}

		public object MethodSignature {
			get {
				return null;
			}
		}

		public string TypeName {
			get {
				return null;
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
			return args [arg_num];
		}
		
		public string GetArgName (int arg_num)
		{
			return names [arg_num];
		}

		public int InArgCount {
			get {
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
				int count = 0;

				foreach (byte t in arg_types) {
					if ((t & 2) != 0) count++;
						
				}
				return count;
			}
		}
		
		public object [] OutArgs {
			get {
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

	}
}
