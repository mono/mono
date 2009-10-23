//
// WebOperationContext.cs
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

#if NET_2_1
using IncomingWebRequestContext = System.Object;
using OutgoingWebResponseContext = System.Object;
#endif

namespace System.ServiceModel.Web
{
	public class WebOperationContext
#if !NET_2_1
	 : IExtension<OperationContext>
#endif
	{
#if !NET_2_1
		public static WebOperationContext Current {
			get {
				if (OperationContext.Current == null)
					return null;
				var ret = OperationContext.Current.Extensions.Find<WebOperationContext> ();
				if (ret == null) {
					ret = new WebOperationContext (OperationContext.Current);
					OperationContext.Current.Extensions.Add (ret);
				}
				return ret;
			}
		}
#endif

		IncomingWebRequestContext incoming_request;
		IncomingWebResponseContext incoming_response;
		OutgoingWebRequestContext outgoing_request;
		OutgoingWebResponseContext outgoing_response;

		public WebOperationContext (OperationContext operation)
		{
			if (operation == null)
				throw new ArgumentNullException ("operation");

			outgoing_request = new OutgoingWebRequestContext ();
			incoming_response = new IncomingWebResponseContext (operation);
#if !NET_2_1
			incoming_request = new IncomingWebRequestContext (operation);
			outgoing_response = new OutgoingWebResponseContext ();
#endif
		}

#if !NET_2_1
		public IncomingWebRequestContext IncomingRequest {
			get { return incoming_request; }
		}
#endif

		public IncomingWebResponseContext IncomingResponse {
			get { return incoming_response; }
		}

		public OutgoingWebRequestContext OutgoingRequest {
			get { return outgoing_request; }
		}

#if !NET_2_1
		public OutgoingWebResponseContext OutgoingResponse {
			get { return outgoing_response; }
		}
#endif

		public void Attach (OperationContext context)
		{
			// do nothing
		}

		public void Detach (OperationContext context)
		{
			// do nothing
		}
	}
}
