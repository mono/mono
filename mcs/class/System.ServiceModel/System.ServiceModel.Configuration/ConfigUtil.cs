//
// ConfigUtil.cs
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
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Configuration;

namespace System.ServiceModel.Configuration
{
	internal static class ConfigUtil
	{
		static object GetSection (string name)
		{
			if (ServiceHostingEnvironment.InAspNet)
				return WebConfigurationManager.GetSection (name);
			else
				return ConfigurationManager.GetSection (name);
		}

		public static BindingsSection BindingsSection {
			get {
				return (BindingsSection) GetSection ("system.serviceModel/bindings");
			}
		}

		public static ClientSection ClientSection {
			get { return (ClientSection) GetSection ("system.serviceModel/client"); }
		}

		public static ServicesSection ServicesSection {
			get { return (ServicesSection) GetSection ("system.serviceModel/services"); }
		}

		public static BehaviorsSection BehaviorsSection {
			get { return (BehaviorsSection) GetSection ("system.serviceModel/behaviors"); }
		}

		public static ExtensionsSection ExtensionsSection {
			get { return (ExtensionsSection) GetSection ("system.serviceModel/extensions"); }
		}

#if NET_4_0
		public static ProtocolMappingSection ProtocolMappingSection {
			get {
				return (ProtocolMappingSection) GetSection ("system.serviceModel/protocolMapping");
			}
		}

		public static StandardEndpointsSection StandardEndpointsSection {
			get {
				return (StandardEndpointsSection) GetSection ("system.serviceModel/standardEndpoints");
			}
		}
#endif

		public static Binding CreateBinding (string binding, string bindingConfiguration)
		{
			BindingCollectionElement section = ConfigUtil.BindingsSection [binding];
			if (section == null)
				throw new ArgumentException (String.Format ("binding section for {0} was not found.", binding));

			Binding b = section.GetDefault ();

			foreach (IBindingConfigurationElement el in section.ConfiguredBindings)
				if (el.Name == bindingConfiguration)
					el.ApplyConfiguration (b);

			return b;
		}

#if NET_4_0
		public static ServiceEndpoint ConfigureStandardEndpoint (ContractDescription cd, ServiceEndpointElement element)
		{
			string kind = element.Kind;
			string endpointConfiguration = element.EndpointConfiguration;

			EndpointCollectionElement section = ConfigUtil.StandardEndpointsSection [kind];
			if (section == null)
				throw new ArgumentException (String.Format ("standard endpoint section for '{0}' was not found.", kind));

			StandardEndpointElement e = section.GetDefaultStandardEndpointElement ();

			ServiceEndpoint inst = e.CreateServiceEndpoint (cd);

			foreach (StandardEndpointElement el in section.ConfiguredEndpoints) {
				if (el.Name == endpointConfiguration) {
					el.InitializeAndValidate (element);
					el.ApplyConfiguration (inst, element);
					break;
				}
			}
			
			return inst;
		}
#endif

		public static KeyedByTypeCollection<IEndpointBehavior>  CreateEndpointBehaviors (string bindingConfiguration)
		{
			var ec = BehaviorsSection.EndpointBehaviors [bindingConfiguration];
			if (ec == null)
				return null;
			var c = new KeyedByTypeCollection<IEndpointBehavior> ();
			foreach (var bxe in ec)
				c.Add ((IEndpointBehavior) bxe.CreateBehavior ());
			return c;
		}

		public static EndpointAddress CreateInstance (this EndpointAddressElementBase el)
		{
			return new EndpointAddress (el.Address, el.Identity.CreateInstance (), el.Headers.Headers);
		}

		public static void CopyFrom (this ChannelEndpointElement to, ChannelEndpointElement from)
		{
			to.Address = from.Address;
			to.BehaviorConfiguration = from.BehaviorConfiguration;
			to.Binding = from.Binding;
			to.BindingConfiguration = from.BindingConfiguration;
			to.Contract = from.Contract;
			if (from.Headers != null)
				to.Headers.Headers = from.Headers.Headers;
			if (from.Identity != null)
				to.Identity.InitializeFrom (from.Identity.CreateInstance ());
			to.Name = from.Name;
		}

		public static EndpointAddress CreateEndpointAddress (this ChannelEndpointElement el)
		{
			return new EndpointAddress (el.Address, el.Identity != null ? el.Identity.CreateInstance () : null, el.Headers.Headers);
		}

		public static EndpointAddress CreateEndpointAddress (this ServiceEndpointElement el)
		{
			return new EndpointAddress (el.Address, el.Identity != null ? el.Identity.CreateInstance () : null, el.Headers.Headers);
		}

		public static EndpointIdentity CreateInstance (this IdentityElement el)
		{
			if (el.Certificate != null)
				return new X509CertificateEndpointIdentity (el.Certificate.CreateInstance ());
			else if (el.CertificateReference != null)
				return new X509CertificateEndpointIdentity (el.CertificateReference.CreateInstance ());
			else if (el.Dns != null)
				return new DnsEndpointIdentity (el.Dns.Value);
			else if (el.Rsa != null)
				return new RsaEndpointIdentity (el.Rsa.Value);
			else if (el.ServicePrincipalName != null)
				return new SpnEndpointIdentity (el.ServicePrincipalName.Value);
			else if (el.UserPrincipalName != null)
				return new UpnEndpointIdentity (el.UserPrincipalName.Value);
			else
				return null;
		}

		public static X509Certificate2 CreateCertificateFrom (StoreLocation storeLocation, StoreName storeName, X509FindType findType, Object findValue)
		{
			var store = new X509Store (storeName, storeLocation);
			store.Open (OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
			try {
				foreach (var c in store.Certificates.Find (findType, findValue, false))
					return c;
				throw new InvalidOperationException (String.Format ("Specified X509 certificate with find type {0} and find value {1} was not found in X509 store {2} location {3}", findType, findValue, storeName, storeLocation));
			} finally {
				store.Close ();
			}
		}

		public static X509Certificate2 CreateInstance (this CertificateElement el)
		{
			return new X509Certificate2 (Convert.FromBase64String (el.EncodedValue));
		}
		
		public static X509Certificate2 CreateInstance (this CertificateReferenceElement el)
		{
			return CreateCertificateFrom (el.StoreLocation, el.StoreName, el.X509FindType, el.FindValue);
		}

		public static X509Certificate2 CreateInstance (this X509ClientCertificateCredentialsElement el)
		{
			return CreateCertificateFrom (el.StoreLocation, el.StoreName, el.X509FindType, el.FindValue);
		}
		
		public static X509Certificate2 CreateInstance (this X509ScopedServiceCertificateElement el)
		{
			return CreateCertificateFrom (el.StoreLocation, el.StoreName, el.X509FindType, el.FindValue);
		}

		public static X509Certificate2 CreateInstance (this X509DefaultServiceCertificateElement el)
		{
			return CreateCertificateFrom (el.StoreLocation, el.StoreName, el.X509FindType, el.FindValue);
		}
	}
}
