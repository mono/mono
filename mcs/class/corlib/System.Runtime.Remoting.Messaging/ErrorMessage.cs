//
// System.Runtime.Remoting.Messaging.ErrorMessage.cs
//
// Author:
//   Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Runtime.Remoting.Messaging {

    // simple message to avoid serialization of crap data
    [Serializable]
    public class ErrorMessage : IMethodCallMessage
    {
		string _uri = "Exception";
		
		public ErrorMessage()
		{
		}
		
		public int ArgCount {
			get {
				return 0;
			}
		}
		
		public object [] Args {
			get {
				return null;
			}
		}
		
		public bool HasVarArgs {
			get {
				return false;
			}
		}

		public MethodBase MethodBase {
			get {
				return null;
			}
		}

		public string MethodName {
			get {
				return "unknown";
			}
		}

		public object MethodSignature {
			get {
				return null;
			}
		}

		public virtual IDictionary Properties {
			get {
				return null;
			}
		}

		public string TypeName {
			get {
				return "unknown";
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
			return null;
		}
		
		public string GetArgName (int arg_num)
		{
			return "unknown";
		}

		public int InArgCount                  
		{ 
			get { 
				return 0;
			} 
		}

		public String GetInArgName(int index)   
		{ 
			return null; 
		}

		public Object GetInArg(int argNum)      
		{ 
			return null;
		}

		public Object[] InArgs             
		{ 
			get { return null; }
		}

		public LogicalCallContext LogicalCallContext 
		{ 
			get { return null; } 
		}
    }
}
