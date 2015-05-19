//
// System.Web.UI.WebControls.WebParts.ConnectionPoint.cs
//
// Author: Sanjay Gupta (gsanjay@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System.Web;
using System.Reflection;

namespace System.Web.UI.WebControls.WebParts {
        public abstract class ConnectionPoint 
	{
		bool allowMultiConn;
		string name = string.Empty;
		string id = "default";
		Type interfaceType;
		Type controlType;
		MethodInfo callBackMethod;
		
		public const string DefaultID = "default";

		internal ConnectionPoint (MethodInfo callBack, Type interFace, 
					Type control, string name, string id, 
							bool allowsMultiConnections)
		{
			this.allowMultiConn = allowsMultiConnections;
			this.interfaceType = interFace;
			this.controlType = control;
			this.name = name;
			this.id = id;
			this.callBackMethod = callBack;
		}
		
		internal MethodInfo CallbackMethod {
			get{ return callBackMethod;  }
		}
		
		[MonoTODO ("Not implemented")]
		public virtual bool GetEnabled (Control control)
		{
			throw new NotImplementedException ();
		}

		public bool AllowsMultipleConnections {
			get { return allowMultiConn; }
		}

		public Type ControlType {
			get { return controlType; }
		}

		public string ID {
			get { return id; }
		}

		public Type InterfaceType { 
			get { return interfaceType; }
		}

		public string Name { 
			get { return name;}
		}	
        }
}
