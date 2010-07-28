//
// ServiceDebugBehavior.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Description
{
	public class ServiceDebugBehavior : IServiceBehavior
	{
		public ServiceDebugBehavior ()
		{
			HttpHelpPageEnabled = true;
			HttpsHelpPageEnabled = true;
		}

		public bool IncludeExceptionDetailInFaults { get; set; }

		public bool HttpHelpPageEnabled { get; set; }

		public Uri HttpHelpPageUrl { get; set; }

		public bool HttpsHelpPageEnabled { get; set; }

		public Uri HttpsHelpPageUrl { get; set; }

		public Binding HttpHelpPageBinding { get; set; }

		public Binding HttpsHelpPageBinding { get; set; }

		void IServiceBehavior.AddBindingParameters (
			ServiceDescription description,
			ServiceHostBase serviceHostBase,
			Collection<ServiceEndpoint> endpoints,
			BindingParameterCollection parameters)
		{
			// do nothing
		}

		void IServiceBehavior.ApplyDispatchBehavior (
			ServiceDescription description,
			ServiceHostBase serviceHostBase)
		{
			ServiceMetadataExtension sme = ServiceMetadataExtension.EnsureServiceMetadataExtension (serviceHostBase);

			foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
				if (IncludeExceptionDetailInFaults) // may be set also in ServiceBehaviorAttribute
					dispatcher.IncludeExceptionDetailInFaults = true;

			if (HttpHelpPageEnabled) {
				Uri uri = serviceHostBase.CreateUri ("http", HttpHelpPageUrl);
				if (uri != null)
					sme.EnsureChannelDispatcher (false, "http", uri, HttpHelpPageBinding);
			}

			if (HttpsHelpPageEnabled) {
				Uri uri = serviceHostBase.CreateUri ("https", HttpsHelpPageUrl);
				if (uri != null)
					sme.EnsureChannelDispatcher (false, "https", uri, HttpsHelpPageBinding);
			}
		}

		[MonoTODO]
		void IServiceBehavior.Validate (
			ServiceDescription description,
			ServiceHostBase serviceHostBase)
		{
		}
	}
}
