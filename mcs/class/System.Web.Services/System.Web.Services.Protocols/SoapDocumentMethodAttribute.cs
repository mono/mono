// 
// System.Web.Services.Protocols.SoapDocumentMethodAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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

using System.Web.Services.Description;

namespace System.Web.Services.Protocols {
	[AttributeUsage (AttributeTargets.Method, Inherited = true)]
	public sealed class SoapDocumentMethodAttribute : Attribute {

		#region Fields

		string action;
		string binding;
		bool oneWay;
		SoapParameterStyle parameterStyle;
		string requestElementName;
		string requestNamespace;
		string responseElementName;
		string responseNamespace;
		SoapBindingUse use;

		#endregion

		#region Constructors

		public SoapDocumentMethodAttribute () 
		{
		}

		public SoapDocumentMethodAttribute (string action)
			: this ()
		{
			this.action = action;
		}
		
		#endregion // Constructors

		#region Properties

		public string Action {
			get { return action; }
			set { action = value; }
		}

		public string Binding {
			get {
				if (binding != null)
					return binding;
				return "";
			}
			set { binding = value; }
		}

		public bool OneWay {
			get { return oneWay; }
			set { oneWay = value; }
		}

		public SoapParameterStyle ParameterStyle {
			get { return parameterStyle; }
			set { parameterStyle = value; }
		}

		public string RequestElementName {
			get {
				if (requestElementName == null)
					return "";
				return requestElementName;
			}
			set { requestElementName = value; }
		}

		public string RequestNamespace {
			get {
				if (requestNamespace == null)
					return "";
				
				return requestNamespace;
			}
			set { requestNamespace = value; }
		}

		public string ResponseElementName {
			get {
				if (responseElementName == null)
					return "";
				return responseElementName;
			}
			set { responseElementName = value; }
		}

		public string ResponseNamespace {
			get {
				if (responseNamespace == null)
					return "";
				return responseNamespace;
			}
			set { responseNamespace = value; }
		}

		public SoapBindingUse Use {
			get { return use; }
			set { use = value; }
		}

		#endregion // Properties
	}
}
