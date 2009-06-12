using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;

using MonoTests.DataSource;

namespace MonoTests.ModelProviders
{
	public class DynamicDataContainerModelProvider : DataModelProvider
	{
		IDynamicDataContainer container;
		Type containerType;
		ReadOnlyCollection<TableProvider> tables;

		IDynamicDataContainer Container
		{
			get
			{
				if (container != null)
					return container;

				container = Activator.CreateInstance (containerType) as IDynamicDataContainer;
				if (container == null)
					throw new InvalidOperationException ("Failed to create an instance of container type '" + ContextType + "'.");

				return container;
			}
		}

		public override Type ContextType
		{
			get
			{
				return Container.ContainedType;
			}
			protected set
			{
				throw new InvalidOperationException ("Setting the context type on this provider is not supported.");
			}
		}

		public DynamicDataContainerModelProvider (Type containerType)
		{
			if (containerType == null)
				throw new ArgumentNullException ("contextType");

			if (!typeof (IDynamicDataContainer).IsAssignableFrom (containerType))
				throw new ArgumentException ("Container type must implement the IDynamicDataContainer interface.", "contextType");

			this.containerType = containerType;
		}

		public DynamicDataContainerModelProvider (IDynamicDataContainer container)
		{
			if (container == null)
				throw new ArgumentNullException ("container");

			this.container = container;
		}

		public override object CreateContext ()
		{
			return Activator.CreateInstance (ContextType);
		}

		public override ReadOnlyCollection<TableProvider> Tables
		{
			get
			{
				if (tables != null)
					return tables;
				tables = LoadTables ();

				return tables;
			}
		}

		void ResolveAssociations ()
		{
			foreach (var t in Tables) {
				var table = t as DynamicDataContainerTableProvider;
				if (t == null)
					continue;
				table.ResolveAssociations ();
			}
		}

		ReadOnlyCollection<TableProvider> LoadTables ()
		{
			List<DynamicDataTable> containerTables = Container.GetTables ();

			if (containerTables == null || containerTables.Count == 0)
				return new ReadOnlyCollection<TableProvider> (new List<TableProvider> ());

			var tables = new List<TableProvider> ();
			foreach (var table in containerTables)
				tables.Add (new DynamicDataContainerTableProvider (this, table));

			return new ReadOnlyCollection<TableProvider> (tables);
		}
	}
}
