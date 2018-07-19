using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
#if XAMCORE_2_0
using Foundation;
#else
#if __MONODROID__
using Android.Runtime;
#else
#if MONOTOUCH
using MonoTouch.Foundation;
#endif
#endif
#endif
using NUnit.Framework;

namespace LinkAll.Mef {

	// From Desk Case 70807
	public interface IStorageType
	{
	}

	[System.ComponentModel.Composition.Export(typeof (IStorageType))]
	[Preserve (AllMembers = true)]
	public class Storage : IStorageType
	{
	}

	[Preserve (AllMembers = true)]
	[TestFixture]
	public class MEFTests
	{
		CompositionContainer _container;

		[ImportMany]
		public IEnumerable<Lazy<IStorageType>> StorageTypes { get; set; }

#if MOBILE // TODO: fails on Mono Desktop, investigate
		[Test]
		public void MEF_Basic_Import_Test ()
		{
			var catalog = new AggregateCatalog ();
			//Adds all the parts found in the same assembly as the Program class
			catalog.Catalogs.Add (new AssemblyCatalog (typeof (Application).Assembly));

			//Create the CompositionContainer with the parts in the catalog
			_container = new CompositionContainer (catalog);

			this._container.SatisfyImportsOnce (this);

			Assert.IsTrue (StorageTypes.Count () > 0, "No MEF imports found?");
		}
#endif

#if MOBILE // TODO: fails on Mono Desktop, investigate
		[Test]
		public void ExportFactoryCreator ()
		{
			// the above code makes sure that ExportFactoryCreator is present
			var efc = Helper.GetType ("System.ComponentModel.Composition.ReflectionModel.ExportFactoryCreator, System.ComponentModel.Composition");
			Assert.NotNull (efc, "ExportFactoryCreator");

			// and there's nothing else that refers to them - hence bug: https://bugzilla.xamarin.com/show_bug.cgi?id=29063
			// as it's used thru reflection in CreateStronglyTypedExportFactoryFactory
			var t = efc.GetMethod ("CreateStronglyTypedExportFactoryOfT", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); // same binding flags as MS code
			Assert.NotNull (t, "CreateStronglyTypedExportFactoryOfT");
			var tm = efc.GetMethod ("CreateStronglyTypedExportFactoryOfTM", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); // same binding flags as MS code
			Assert.NotNull (tm, "CreateStronglyTypedExportFactoryOfTM");
		}
#endif

		[Test]
		public void ExportServices ()
		{
			var es = Helper.GetType ("System.ComponentModel.Composition.ExportServices, System.ComponentModel.Composition");
			Assert.NotNull (es, "ExportServices");
			// unlike the test code for ExportFactoryCreator the method can be marked by other call site, so this test is not 100% conclusive

			// used, thru reflection, from CreateStronglyTypedLazyFactory method 
			var t = es.GetMethod ("CreateStronglyTypedLazyOfT", BindingFlags.NonPublic | BindingFlags.Static); // same binding flags as MS code
			Assert.NotNull (t, "CreateStronglyTypedLazyOfT");
			var tm = es.GetMethod ("CreateStronglyTypedLazyOfTM", BindingFlags.NonPublic | BindingFlags.Static); // same binding flags as MS code
			Assert.NotNull (tm, "CreateStronglyTypedLazyOfTM");

			// used, thru reflection, from CreateSemiStronglyTypedLazyFactory method 
			var l = es.GetMethod ("CreateSemiStronglyTypedLazy", BindingFlags.NonPublic | BindingFlags.Static); // same binding flags as MS code
			Assert.NotNull (l, "CreateSemiStronglyTypedLazy");
		}
	}
}
