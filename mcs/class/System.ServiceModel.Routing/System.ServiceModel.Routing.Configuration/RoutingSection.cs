using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing.Configuration
{
	static class RoutingConfigurationExtension
	{
		public static ServiceEndpoint CreateEndpoint (this ChannelEndpointElement el)
		{
			// depends on System.ServiceModel internals (by InternalVisibleTo).
			// FIXME: is IRequestReplyRouter okay for every case?
			return new ServiceEndpoint (ContractDescription.GetContract (typeof (IRequestReplyRouter)), ConfigUtil.CreateBinding (el.Binding, el.BindingConfiguration), new EndpointAddress (el.Address));
		}

		public static MessageFilter CreateFilter (this FilterElement el, RoutingSection sec)
		{
			switch (el.FilterType) {
			case FilterType.Action:
				return new ActionMessageFilter (el.FilterData);
			case FilterType.EndpointAddress:
				return new EndpointAddressMessageFilter (new EndpointAddress (el.FilterData), false);
			case FilterType.PrefixEndpointAddress:
				return new PrefixEndpointAddressMessageFilter (new EndpointAddress (el.FilterData), false);
			case FilterType.And:
				var fe1 = (FilterElement) sec.Filters [el.Filter1];
				var fe2 = (FilterElement) sec.Filters [el.Filter2];
				return new StrictAndMessageFilter (fe1.CreateFilter (sec), fe2.CreateFilter (sec));
			case FilterType.Custom:
				return (MessageFilter) Activator.CreateInstance (Type.GetType (el.CustomType));
			case FilterType.EndpointName:
				return new EndpointNameMessageFilter (el.FilterData);
			case FilterType.MatchAll:
				return new MatchAllMessageFilter ();
			case FilterType.XPath:
				return new XPathMessageFilter (el.FilterData);
			default:
				throw new ArgumentOutOfRangeException ("FilterType");
			}
		}
	}

	public class RoutingSection : ConfigurationSection
	{
		static ServiceEndpoint CreateServiceEndpoint (string name)
		{
			// FIXME: might be service endpoints.
			var endpoints = ConfigUtil.ClientSection.Endpoints;
			foreach (ChannelEndpointElement ep in endpoints)
				if (ep.Name == name)
					return ep.CreateEndpoint ();
			throw new KeyNotFoundException (String.Format ("client endpoint '{0}' not found", name));
		}

		[MonoTODO]
		public static MessageFilterTable<IEnumerable<ServiceEndpoint>> CreateFilterTable (string name)
		{
			var sec = (RoutingSection) ConfigurationManager.GetSection ("system.serviceModel/routing");

			var table = new MessageFilterTable<IEnumerable<ServiceEndpoint>> ();
			var ftec = (FilterTableEntryCollection) sec.FilterTables [name];
			foreach (FilterTableEntryElement fte in ftec) {
				var filterElement = (FilterElement) sec.Filters [fte.FilterName];
				MessageFilter filter = filterElement.CreateFilter (sec);
				table.Add (filter, new List<ServiceEndpoint> (), fte.Priority);
				var list = (List<ServiceEndpoint>) table [filter];
				list.Add (CreateServiceEndpoint (fte.EndpointName));
				var bec = (BackupEndpointCollection) sec.BackupLists [fte.BackupList];
				if (bec != null)
					foreach (BackupEndpointElement bee in bec)
						list.Add (CreateServiceEndpoint (bee.EndpointName));
			}
			return table;
		}

		public RoutingSection ()
		{
			//BackupLists = new BackupListCollection ();
			//Filters = new FilterElementCollection ();
			//FilterTables = new FilterTableCollection ();
			//NamespaceTable = new NamespaceElementCollection ();
		}

		[ConfigurationProperty ("backupLists", Options = ConfigurationPropertyOptions.None)]
		public BackupListCollection BackupLists {
			get { return (BackupListCollection) base ["backupLists"]; }
			private set { base ["backupLists"] = value; }
		}

		[ConfigurationProperty ("filters", Options = ConfigurationPropertyOptions.None)]
		public FilterElementCollection Filters {
			get { return (FilterElementCollection) base ["filters"]; }
			private set { base ["filters"] = value; }
		}

		[ConfigurationProperty ("filterTables", Options = ConfigurationPropertyOptions.None)]
		public FilterTableCollection FilterTables {
			get { return (FilterTableCollection) base ["filterTables"]; }
			private set { base ["filterTables"] = value; }
		}

		[ConfigurationProperty ("namespaceTable", Options = ConfigurationPropertyOptions.None)]
		public NamespaceElementCollection NamespaceTable {
			get { return (NamespaceElementCollection) base ["namespaceTable"]; }
			private set { base ["namespaceTable"] = value; }
		}
	}
}
