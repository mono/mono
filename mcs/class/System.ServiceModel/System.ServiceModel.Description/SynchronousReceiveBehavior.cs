//
// SynchronousReceiveBehavior.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public class SynchronousReceiveBehavior : IEndpointBehavior
	{
		#region IEndpointBehavior Members

		void IEndpointBehavior.AddBindingParameters (ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection parameters) {
			throw new NotImplementedException ();
		}

		void IEndpointBehavior.ApplyDispatchBehavior (ServiceEndpoint serviceEndpoint, System.ServiceModel.Dispatcher.EndpointDispatcher dispatcher) {
			throw new NotImplementedException ();
		}

		void IEndpointBehavior.ApplyClientBehavior (ServiceEndpoint serviceEndpoint, System.ServiceModel.Dispatcher.ClientRuntime behavior) {
			throw new NotImplementedException ();
		}

		void IEndpointBehavior.Validate (ServiceEndpoint serviceEndpoint) {
			throw new NotImplementedException ();
		}

		#endregion
	}
}
