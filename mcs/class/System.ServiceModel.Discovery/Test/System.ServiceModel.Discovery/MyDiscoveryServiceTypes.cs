//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace System.ServiceModel.Discovery
{
	public class MyServiceDiscoveryBehavior : ServiceDiscoveryBehavior, IServiceBehavior
	{
		IServiceBehavior GetBase ()
		{
			var sdb = (ServiceDiscoveryBehavior) this;
			return (IServiceBehavior) sdb;
		}

		void IServiceBehavior.AddBindingParameters (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
			GetBase ().AddBindingParameters (serviceDescription, serviceHostBase, endpoints, bindingParameters);
		}

		void IServiceBehavior.ApplyDispatchBehavior (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			GetBase ().ApplyDispatchBehavior (serviceDescription, serviceHostBase);
		}

		void IServiceBehavior.Validate (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			if (serviceHostBase == null)
				throw new ArgumentNullException ("serviceHostBase");
			var dse = serviceHostBase.Extensions.Find<DiscoveryServiceExtension> ();
			if (dse == null) {
				dse = new MyDiscoveryServiceExtension ();
				serviceHostBase.Extensions.Add (dse);
			}

			GetBase ().Validate (serviceDescription, serviceHostBase);
		}
	}

	public class MyDiscoveryServiceExtension : DiscoveryServiceExtension
	{
		protected override DiscoveryService GetDiscoveryService ()
		{
			return new MyDiscoveryService ();
		}
	}

	public class MyDiscoveryService : DiscoveryService
	{
		protected override IAsyncResult OnBeginFind (FindRequestContext findRequestContext, AsyncCallback callback, Object state)
		{
			Console.Error.WriteLine ("OnBeginFind");
			throw new Exception ("1");
		}

		protected override IAsyncResult OnBeginResolve (ResolveCriteria resolveCriteria, AsyncCallback callback, Object state)
		{
			Console.Error.WriteLine ("OnBeginResolve");
			throw new Exception ("2");
		}

		protected override void OnEndFind (IAsyncResult result)
		{
			Console.Error.WriteLine ("OnEndFind");
			throw new Exception ("3");
		}

		protected override EndpointDiscoveryMetadata OnEndResolve (IAsyncResult result)
		{
			Console.Error.WriteLine ("OnEndResolve");
			throw new Exception ("4");
		}
	}
}
