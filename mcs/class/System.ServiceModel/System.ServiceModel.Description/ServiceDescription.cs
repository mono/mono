//
// ServiceDescription.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Description
{
	public class ServiceDescription
	{
		ServiceEndpointCollection endpoints = new ServiceEndpointCollection ();
		KeyedByTypeCollection<IServiceBehavior> behaviors = new KeyedByTypeCollection<IServiceBehavior> ();
		Type service_type;
		object well_known;
		string name, ns, config_name;

		public ServiceDescription ()
		{
		}

		public ServiceDescription (IEnumerable<ServiceEndpoint> endpoints)
		{
			foreach (ServiceEndpoint se in endpoints)
				this.endpoints.Add (se);
		}

		public string ConfigurationName {
			get { return config_name; }
			set { config_name = value; }
		}

		public KeyedByTypeCollection<IServiceBehavior> Behaviors {
			get { return behaviors; }
		}

		public ServiceEndpointCollection Endpoints {
			get { return endpoints; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public Type ServiceType {
			get { return service_type; }
			set { service_type = value; }
		}

		public static ServiceDescription GetService (Type serviceType)
		{
			// null Type is not rejected
			ServiceDescription sd = new ServiceDescription ();
			sd.ServiceType = serviceType;
			if (serviceType != null) {
				var att = serviceType.GetCustomAttribute<ServiceBehaviorAttribute> (true);
				if (att != null) {
					sd.Name = att.Name;
					sd.Namespace = att.Namespace;
				}
				if (sd.Name == null)
					sd.Name = serviceType.Name;
				if (sd.Namespace == null)
					sd.Namespace = "http://tempuri.org/";
			}
			return sd;
		}

		public static ServiceDescription GetService (object serviceImplementation)
		{
			// null instance is not rejected
			ServiceDescription sd = new ServiceDescription ();
			sd.ServiceType = serviceImplementation != null ? serviceImplementation.GetType () : null;
			return sd;
		}
	}
}
