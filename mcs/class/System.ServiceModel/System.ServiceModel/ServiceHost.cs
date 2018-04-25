//
// ServiceHost.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
	public class ServiceHost : ServiceHostBase
	{
		Type service_type;
		object instance;
		Dictionary<string,ContractDescription> contracts;

		protected ServiceHost ()
		{
		}

		public ServiceHost (object singletonInstance,
			params Uri [] baseAddresses)
		{
			if (singletonInstance == null)
				throw new ArgumentNullException ("singletonInstance");
			InitializeDescription (singletonInstance,
				new UriSchemeKeyedCollection (baseAddresses));
		}

		public ServiceHost (Type serviceType,
			params Uri [] baseAddresses)
		{
			InitializeDescription (serviceType,
				new UriSchemeKeyedCollection (baseAddresses));
		}

		public object SingletonInstance {
			get { return instance; }
		}

		static Uri CreateUri (string address)
		{
			if (address.Length > 0 && address[0] == '/')
				return new Uri (address.Substring (1), UriKind.Relative);
			else
				return new Uri (address, UriKind.RelativeOrAbsolute);
		}

		public ServiceEndpoint AddServiceEndpoint (
			Type implementedContract, Binding binding, string address)
		{
			return AddServiceEndpoint (implementedContract, binding, CreateUri (address));
		}

		public ServiceEndpoint AddServiceEndpoint (
			Type implementedContract, Binding binding, string address, Uri listenUri)
		{
			return AddServiceEndpoint (implementedContract, binding,
				CreateUri (address), listenUri);
		}

		public ServiceEndpoint AddServiceEndpoint (
			Type implementedContract, Binding binding, Uri address)
		{
			return AddServiceEndpoint (implementedContract,
				binding, address, null);
		}

		public ServiceEndpoint AddServiceEndpoint (
			Type implementedContract, Binding binding, Uri address, Uri listenUri)
		{
			EndpointAddress ea = new EndpointAddress (BuildAbsoluteUri (address, binding));

			ContractDescription cd = GetExistingContract (implementedContract);
			if (cd == null) {
				cd = ContractDescription.GetContract (implementedContract);
				if (!contracts.ContainsKey (cd.ContractType.FullName)) {
					contracts.Add (cd.ContractType.FullName, cd);
				}
			}

			return AddServiceEndpointCore (cd, binding, ea, listenUri);
		}

		ContractDescription GetExistingContract (Type implementedContract)
		{
			foreach (ContractDescription cd in ImplementedContracts.Values)
				if (cd.ContractType == implementedContract)
					return cd;
			return null;
		}

		protected override ServiceDescription CreateDescription (
			out IDictionary<string,ContractDescription> implementedContracts)
		{
			contracts = new Dictionary<string,ContractDescription> ();
			implementedContracts = contracts;
			ServiceDescription sd;
			IEnumerable<ContractDescription>  contractDescriptions = GetServiceContractDescriptions ();
			foreach (ContractDescription cd in contractDescriptions)
				contracts.Add (cd.ContractType.FullName, cd);

			if (SingletonInstance != null) {
				sd = ServiceDescription.GetService (instance);				
			} else {
				sd = ServiceDescription.GetService (service_type);				
			}

			ServiceBehaviorAttribute sba = PopulateAttribute<ServiceBehaviorAttribute> ();
			if (SingletonInstance != null)
				sba.SetWellKnownSingleton (SingletonInstance);
			sd.Behaviors.Add (sba);

			return sd;
		}

		IEnumerable<ContractDescription> GetServiceContractDescriptions () {
			List<ContractDescription> contracts = new List<ContractDescription> ();
			Dictionary<Type, ServiceContractAttribute> contractAttributes = ContractDescriptionGenerator.GetServiceContractAttributes (service_type);
			foreach (Type contract in contractAttributes.Keys)
				contracts.Add( ContractDescriptionGenerator.GetContract (contract, service_type));
			return contracts;
		}

		TAttr PopulateAttribute<TAttr> ()
		{
			object [] atts = service_type.GetCustomAttributes (typeof (TAttr), true);
			return (TAttr) (atts.Length > 0 ? atts [0] : Activator.CreateInstance (typeof (TAttr)));
		}

		protected void InitializeDescription (Type serviceType, UriSchemeKeyedCollection baseAddresses)
		{
			if (!serviceType.IsClass)
				throw new ArgumentException ("ServiceHost only supports 'class' service types.");

			service_type = serviceType;

			InitializeDescription (baseAddresses);
		}

		protected void InitializeDescription (object singletonInstance, UriSchemeKeyedCollection baseAddresses)
		{
			instance = singletonInstance;
			InitializeDescription (singletonInstance.GetType (), baseAddresses);
		}
	}
}
