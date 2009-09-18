using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.DynamicData;
using System.Web.UI;

using MonoTests.SystemWeb.Framework;
using MonoTests.DataSource;

namespace MonoTests.Common
{
	public class TestsBasePage<DataContextType> : global::System.Web.UI.Page where DataContextType : new ()
	{
		Type containerType;
		bool? outsideTestSuite;

		public event EventHandler ItemDataBinding;

		public bool OutsideTestSuite
		{
			get
			{
				if (outsideTestSuite == null) {
					object o = WebConfigurationManager.AppSettings["OutsideTestSuite"];
					string s = o as string;
					bool b;
					if (s == null)
						outsideTestSuite = false;
					else if (Boolean.TryParse (s, out b))
						outsideTestSuite = b;
					else
						outsideTestSuite = false;
				}

				return (bool)outsideTestSuite;
			}

			set
			{
				outsideTestSuite = value;
			}
		}

		public virtual Type ContextType
		{
			get { return typeof (DataContextType); }
		}

		public virtual Type ContainerType
		{
			get
			{
				if (containerType == null) {
					Type genType = typeof (TestDataContainer<>).GetGenericTypeDefinition ();
					containerType = genType.MakeGenericType (new Type[] { ContextType });
				}

				return containerType;
			}
		}

		public virtual string ContainerTypeName
		{
			get { return ContainerType.AssemblyQualifiedName; }
		}

		protected virtual IDynamicDataContainer<DataContextType> CreateContainerInstance ()
		{
			return Activator.CreateInstance (ContainerType) as IDynamicDataContainer<DataContextType>;
		}

		protected virtual void InitializeDataSource (DynamicDataSource ds, string tableName)
		{
			ds.DataContainerTypeName = ContainerTypeName;
			ds.EntitySetName = tableName;
			ds.ContextType = typeof (DataContextType);
			ds.DataContainerInstance = CreateContainerInstance ();

			PopulateDataSource (ds);
		}

		protected virtual void PopulateDataSource (DynamicDataSource ds)
		{
		}

		protected override void OnPreInit (EventArgs e)
		{
			if (!OutsideTestSuite) {
				WebTest t = WebTest.CurrentTest;
				if (t != null)
					t.Invoke (this);
			}
		}

		public void Item_DataBinding (object sender, EventArgs args)
		{
			EventHandler eh = ItemDataBinding;
			if (eh != null)
				eh (sender, args);
		}
	}
}
