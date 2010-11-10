//
// WebScriptServiceHostFactory.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008,2009 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;

namespace System.ServiceModel.Activation
{
	public class WebScriptServiceHostFactory : ServiceHostFactory
	{
		public WebScriptServiceHostFactory ()
		{
			ContractDescriptionGenerator.RegisterGetOperationContractAttributeExtender (WebAttributesOCEExtender);
		}

		static bool WebAttributesOCEExtender (MethodBase method, object[] customAttributes, ref OperationContractAttribute oca)
		{
			int caLength = customAttributes == null ? 0 : customAttributes.Length;
			if (method == null && caLength == 0)
				return false;

			if (caLength == 0) {
				customAttributes = method.GetCustomAttributes (false);

				if (customAttributes.Length == 0)
					return false;
			}

			bool foundWebAttribute = false;
			foreach (object o in customAttributes) {
				if (o is WebInvokeAttribute || o is WebGetAttribute) {
					foundWebAttribute = true;
					break;
				}
			}

			if (!foundWebAttribute)
				return false;

			// LAMESPEC: .NET allows for contract methods decorated only with
			// Web{Get,Invoke}Attribute and _without_ the OperationContractAttribute.
			if (oca == null)
				oca = new OperationContractAttribute ();
			
			return true;
		}
		
		protected override ServiceHost CreateServiceHost (Type serviceType, Uri [] baseAddresses)
		{
			if (serviceType == null)
				throw new ArgumentNullException ("serviceType");
			return new WebScriptServiceHost (serviceType, baseAddresses);
		}

		class WebScriptServiceHost : ServiceHost
		{
			public WebScriptServiceHost (Type serviceType, params Uri [] baseAddresses)
				: base (serviceType, baseAddresses)
			{
				if (serviceType == null)
					throw new ArgumentNullException ("serviceType");
			}

#if false
			protected override void ApplyConfiguration ()
			{
				base.ApplyConfiguration ();

				if (Description.Endpoints.Count > 1)
					throw new InvalidOperationException ("This service host factory does not allow custom endpoint configuration");

				if (ServiceHostingEnvironment.AspNetCompatibilityEnabled) {
					foreach (Type iface in Description.ServiceType.GetInterfaces ())
						if (iface.GetCustomAttributes (typeof (ServiceContractAttribute), true).Length > 0)
							AddServiceEndpoint (iface, new WebHttpBinding (), new Uri (String.Empty, UriKind.Relative));
				}
			}
#endif

			protected override void OnOpening ()
			{
				base.OnOpening ();

				if (Description.Endpoints.Count == 0) {
					if (ImplementedContracts.Count > 1)
						throw new InvalidOperationException ("WebScriptServiceHostFactory does not allow more than one service contract in the service type");
					foreach (var pair in ImplementedContracts) // actually one
						AddServiceEndpoint (pair.Key, new WebHttpBinding (), new Uri (String.Empty, UriKind.Relative));
				}

				foreach (ServiceEndpoint se in Description.Endpoints)
					if (se.Behaviors.Find<WebHttpBehavior> () == null)
						se.Behaviors.Insert (0, new WebScriptEnablingBehavior ());
			}
		}
	}
}
