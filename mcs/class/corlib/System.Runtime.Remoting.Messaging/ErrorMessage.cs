//
// System.Runtime.Remoting.Messaging.ErrorMessage.cs
//
// Author:
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

namespace System.Runtime.Remoting.Messaging {

    // simple message to avoid serialization of crap data
    [Serializable]
    internal class ErrorMessage : IMethodCallMessage
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
