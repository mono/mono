//
// WebInvokeAttribute.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Web
{
	[AttributeUsage (AttributeTargets.Method)]
	public sealed class WebInvokeAttribute : Attribute, IOperationBehavior
	{
		WebAttributeInfo info = new WebAttributeInfo ();

		internal WebAttributeInfo Info {
			get { return info; }
		}

		public WebMessageBodyStyle BodyStyle {
			get { return info.BodyStyle; }
			set { info.BodyStyle = value; }
		}

		public bool IsBodyStyleSetExplicitly {
			get { return info.IsBodyStyleSetExplicitly; }
		}

		public bool IsRequestFormatSetExplicitly {
			get { return info.IsRequestFormatSetExplicitly; }
		}

		public bool IsResponseFormatSetExplicitly {
			get { return info.IsResponseFormatSetExplicitly; }
		}

		public WebMessageFormat RequestFormat {
			get { return info.RequestFormat; }
			set { info.RequestFormat = value; }
		}

		public WebMessageFormat ResponseFormat {
			get { return info.ResponseFormat ; }
			set { info.ResponseFormat = value; }
		}

		public string Method {
			get { return info.Method; }
			set { info.Method = value; }
		}

		public string UriTemplate {
			get { return info.UriTemplate; }
			set { info.UriTemplate = value; }
		}

		void IOperationBehavior.AddBindingParameters (OperationDescription operation, BindingParameterCollection parameters)
		{
			// "it is a passive operation behavior"
		}

		void IOperationBehavior.ApplyClientBehavior (OperationDescription operation, ClientOperation client)
		{
			// "it is a passive operation behavior"
		}

		void IOperationBehavior.ApplyDispatchBehavior (OperationDescription operation, DispatchOperation service)
		{
			// "it is a passive operation behavior"
		}

		void IOperationBehavior.Validate (OperationDescription operation)
		{
			// "it is a passive operation behavior"
		}
	}
}
