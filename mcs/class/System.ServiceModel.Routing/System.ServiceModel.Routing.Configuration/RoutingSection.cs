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
	public class RoutingSection : ConfigurationSection
	{
		[MonoTODO]
		public static MessageFilterTable<IEnumerable<ServiceEndpoint>> CreateFilterTable (string name)
		{
			throw new NotImplementedException ();
			/*
			// FIXME: I feel messed.
			var sec = (RoutingSection) ConfigurationManager.GetSection ("system.serviceModel/routing");
			var endpoints = ((ServiceModelSectionGroup) ConfigurationManager.GetSection ("system.serviceModel")).Client.Endpoints;
			var table = new MessageFilterTable<IEnumerable<ServiceEndpoint>> ();
			var ftec = (FilterTableEntryCollection) sec.FilterTables [name];
			foreach (FilterTableEntryElement fte in ftec) {
				var filter = table.Keys.FirstOrDefault (f => ((EndpointNameMessageFilter) f).Name == fte.FilterName);
				if (filter == null) {
					filter = new EndpointNameMessageFilter (fte.EndpointName);
					table.Add (filter, new List<ServiceEndpoint> ());
				}
				var list = table [filter];
				var bec = (BackupEndpointCollection) sec.BackupLists [fte.BackupList];
				foreach (var bee in bec)
					list.Add (endpoints [bee.EndpointName]);
			}
			return table;
			*/
		}

		public RoutingSection ()
		{
			BackupLists = new BackupListCollection ();
			Filters = new FilterElementCollection ();
			FilterTables = new FilterTableCollection ();
			NamespaceTable = new NamespaceElementCollection ();
		}

		[ConfigurationProperty ("backupLists", Options = ConfigurationPropertyOptions.None)]
		public BackupListCollection BackupLists { get; private set; }

		[ConfigurationProperty ("filters", Options = ConfigurationPropertyOptions.None)]
		public FilterElementCollection Filters { get; private set; }

		[ConfigurationProperty ("filterTables", Options = ConfigurationPropertyOptions.None)]
		public FilterTableCollection FilterTables { get; private set; }

		[ConfigurationProperty ("namespaceTable", Options = ConfigurationPropertyOptions.None)]
		public NamespaceElementCollection NamespaceTable { get; private set; }
	}
}
