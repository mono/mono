//
// WebServiceHost.cs
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

// This class does:
// - manual addressing support (with ChannelFactory, client will fail with
//   InvalidOperationException that claims missing manual addressing) in every
//   messages.

namespace System.ServiceModel.Web
{
	public class WebServiceHost : ServiceHost
	{
		public WebServiceHost ()
			: base ()
		{
		}

		public WebServiceHost (object singletonInstance, params Uri [] baseAddresses)
			: base (singletonInstance, baseAddresses)
		{
		}

		public WebServiceHost (Type serviceType, params Uri [] baseAddresses)
			: base (serviceType, baseAddresses)
		{
		}

		protected override void OnOpening ()
		{
			base.OnOpening ();

			foreach (Uri baseAddress in BaseAddresses) {
				bool found = false;
				foreach (ServiceEndpoint se in Description.Endpoints)
					if (se.Address.Uri == baseAddress)
						found = true;
				if (!found) {
					if (ImplementedContracts.Count > 1)
						throw new InvalidOperationException ("Service '"+ Description.ServiceType.Name + "' implements multiple ServiceContract types, and no endpoints are defined in the configuration file. WebServiceHost can set up default endpoints, but only if the service implements only a single ServiceContract. Either change the service to only implement a single ServiceContract, or else define endpoints for the service explicitly in the configuration file. When more than one contract is implemented, must add base address endpoint manually");
					var  enumerator = ImplementedContracts.Values.GetEnumerator ();
					enumerator.MoveNext ();
					Type contractType = enumerator.Current.ContractType;
					AddServiceEndpoint (contractType, new WebHttpBinding (), baseAddress);
				}
			}

			foreach (ServiceEndpoint se in Description.Endpoints)
				if (se.Behaviors.Find<WebHttpBehavior> () == null)
					se.Behaviors.Add (new WebHttpBehavior ());

			// disable help page.
			ServiceDebugBehavior serviceDebugBehavior = Description.Behaviors.Find<ServiceDebugBehavior> ();
			if (serviceDebugBehavior != null) {
				serviceDebugBehavior.HttpHelpPageEnabled = false;
				serviceDebugBehavior.HttpsHelpPageEnabled = false;
			}
		}
	}
}
