//
// System.Runtime.Serialization.Formatters.SoapMessage.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
// 	   Jean-Marc Andre (jean-marc.andre@polymtl.ca)	
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
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;

namespace System.Runtime.Serialization.Formatters {

	[Serializable]
	public class SoapMessage : ISoapMessage
	{
		private Header[] _headers;
		private string _methodName;
		private string[] _paramNames;
		private Type[] _paramTypes;
		private object[] _paramValues;
		private string _xmlNameSpace;
		
		public SoapMessage ()
		{
		}

		public Header[] Headers {
			get { return _headers; }
			set { _headers = value; }
		}

		public string MethodName {
			get { return _methodName; }
			set { _methodName = value; }
		}

		public string [] ParamNames {
			get { return _paramNames; }
			set { _paramNames = value; }
		}

		public Type [] ParamTypes {
			get { return _paramTypes; }
			set { _paramTypes = value; }
		}

		public object [] ParamValues {
			get { return _paramValues; }
			set { _paramValues = value; }
		}

		public string XmlNameSpace {
			get { return _xmlNameSpace; }
			set { _xmlNameSpace = value; }
		}
	}
}
