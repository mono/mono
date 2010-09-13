//
// ServiceModelSectionGroup.cs
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
using System.Configuration;

using ConfigurationType = System.Configuration.Configuration;

namespace System.ServiceModel.Configuration
{
	public sealed class ServiceModelSectionGroup : ConfigurationSectionGroup
	{
		public static ServiceModelSectionGroup GetSectionGroup (
			ConfigurationType config)
		{
			ServiceModelSectionGroup ret = (ServiceModelSectionGroup) config.GetSectionGroup ("system.serviceModel");
			if (ret == null)
				throw new SystemException ("Internal configuration error: section 'system.serviceModel' was not found.");
			return ret;
		}

		public ServiceModelSectionGroup ()
		{
		}

		public BehaviorsSection Behaviors {
			get { return (BehaviorsSection) Sections ["behaviors"]; }
		}

		public BindingsSection Bindings {
			get { return (BindingsSection) Sections ["bindings"]; }
		}

		public ClientSection Client {
			get { return (ClientSection) Sections ["client"]; }
		}

		public CommonBehaviorsSection CommonBehaviors {
			get { return (CommonBehaviorsSection) Sections ["commonBehaviors"]; }
		}

		public DiagnosticSection Diagnostic {
			get { return (DiagnosticSection) Sections ["diagnostics"]; }
		}

		public ExtensionsSection Extensions {
			get { return (ExtensionsSection) Sections ["extensions"]; }
		}

		public ServiceHostingEnvironmentSection ServiceHostingEnvironment {
			get { return (ServiceHostingEnvironmentSection) Sections ["serviceHostingEnvironment"]; }
		}

		public ServicesSection Services {
			get { return (ServicesSection) Sections ["services"]; }
		}

#if NET_4_0
		public StandardEndpointsSection StandardEndpoints {
			get { return (StandardEndpointsSection) Sections ["standardEndpoints"]; }
		}
#endif
	}
}
