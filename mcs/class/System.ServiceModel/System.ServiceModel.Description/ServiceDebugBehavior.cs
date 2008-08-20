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
		bool inc_details;
		bool http_help_enabled = true;
		bool https_help_enabled = true;
		Uri http_help_url, https_help_url;

		public ServiceDebugBehavior ()
		{
		}

		public bool IncludeExceptionDetailInFaults {
			get { return inc_details; }
			set { inc_details = value; }
		}

		public bool HttpHelpPageEnabled {
			get { return http_help_enabled; }
			set { http_help_enabled = value; }
		}

		public Uri HttpHelpPageUrl {
			get { return http_help_url; }
			set { http_help_url = value; }
		}

		public bool HttpsHelpPageEnabled {
			get { return https_help_enabled; }
			set { https_help_enabled = value; }
		}

		public Uri HttpsHelpPageUrl {
			get { return https_help_url; }
			set { https_help_url = value; }
		}

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
			ServiceMetadataExtension sme = ServiceMetadataExtension.EnsureServiceMetadataExtension (description, serviceHostBase);

			foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
				if (IncludeExceptionDetailInFaults) // may be set also in ServiceBehaviorAttribute
					dispatcher.IncludeExceptionDetailInFaults = true;

			if (HttpHelpPageEnabled) {
				Uri uri = serviceHostBase.CreateUri ("http", HttpHelpPageUrl);
				if (uri != null)
					ServiceMetadataExtension.EnsureServiceMetadataHttpChanelDispatcher (description, serviceHostBase, sme, uri);
			}

			if (HttpsHelpPageEnabled) {
				Uri uri = serviceHostBase.CreateUri ("https", HttpsHelpPageUrl);
				if (uri != null)
					ServiceMetadataExtension.EnsureServiceMetadataHttpsChanelDispatcher (description, serviceHostBase, sme, uri);
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
