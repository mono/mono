//
// System.Runtime.Serialization.Formatters.SoapMessage.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
// 	   Jean-Marc Andre (jean-marc.andre@polymtl.ca)	
//
// 2002 (C) Copyright, Ximian, Inc.
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
