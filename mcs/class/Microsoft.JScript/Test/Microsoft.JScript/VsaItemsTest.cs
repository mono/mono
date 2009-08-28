//
// VsaItemsTest.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using NUnit.Framework;
using Microsoft.Vsa;
using Microsoft.JScript.Vsa;

namespace MonoTests.Microsoft.JScript {

	[TestFixture]
	public class VsaItemsTest {

		public class Site : BaseVsaSite 
		{}

		[SetUp]
		public void Init ()
		{
		}

		[Test]
 		public void OnEngineClosed ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine = new VsaEngine ();
			engine.RootMoniker = "com.foo://path/to/nowhere";
			engine.Site = new Site ();

			engine.InitNew ();
			items = engine.Items;
			engine.Close ();
		
			int size;

			try {
				size = items.Count;
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.EngineClosed, e.ErrorCode, "#1");
			}

			try {
				item = items.CreateItem ("itemx", VsaItemType.Code, 
							 VsaItemFlag.Class);
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.EngineClosed, e.ErrorCode, "#2");
			}
		}

		[Test]
		public void OnItemNotFound ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine = new VsaEngine ();
			engine.RootMoniker = "com.foo://path/to/nowhere";
			engine.Site = new Site ();

			engine.InitNew ();
			items = engine.Items;

			try {
				item = items [-1];
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemNotFound, e.ErrorCode, "#3");
			}


			try {
				item = items [20];
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemNotFound, e.ErrorCode, "#4");
			}

			try {
				item = items ["IamNotHere"];
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemNotFound, e.ErrorCode, "#5");
			}

			try {
				items.Remove (20);
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemNotFound, e.ErrorCode, "#6");
			}

			try {
				items.Remove (-1);
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemNotFound, e.ErrorCode, "#7");
			}
		}

		[Test]
		public void OnItemFlagNotSupported ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine = new VsaEngine ();
			engine.RootMoniker = "com.foo://path/to/nowhere";
			engine.Site = new Site ();

			engine.InitNew ();
			items = engine.Items;

			try {
				item = items.CreateItem ("item1", 
							  VsaItemType.Reference,
							  VsaItemFlag.Class);
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemFlagNotSupported, e.ErrorCode, "#8");
			}

			try {
				item = items.CreateItem ("item2", 
							  VsaItemType.Reference,
							  VsaItemFlag.Module);
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemFlagNotSupported, e.ErrorCode, "#9");
			}

			try {
				item = items.CreateItem ("item3", 
							 VsaItemType.AppGlobal,
							 VsaItemFlag.Class);
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemFlagNotSupported, e.ErrorCode, "#10");
			}

			try {
				item = items.CreateItem ("item4", 
							 VsaItemType.AppGlobal,
							 VsaItemFlag.Module);
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemFlagNotSupported, e.ErrorCode, "#11");
			}

		}

		[Test]
		public void Remove ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine = new VsaEngine ();
			engine.RootMoniker = "com.foo://path/to/nowhere";
			engine.Site = new Site ();

			engine.InitNew ();
			items = engine.Items;

			items.CreateItem ("item1", VsaItemType.Reference, VsaItemFlag.None);
			items.CreateItem ("item2", VsaItemType.AppGlobal, VsaItemFlag.None);
			items.CreateItem ("item3", VsaItemType.Code, VsaItemFlag.Module);

			Assert.AreEqual (3, items.Count, "#12");

			try {
				item = items [4];
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemNotFound, e.ErrorCode, "#13");
			}

			string ERASED_ITEM = "item2";

			items.Remove (ERASED_ITEM);

			Assert.AreEqual (2, items.Count, "#14");

			try {
				item = items [ERASED_ITEM];
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemNotFound, e.ErrorCode, "#15");
			}
		}

		[Test]
		public void Retrieve ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine = new VsaEngine ();
			engine.RootMoniker = "com.foo://path/to/nowhere";
			engine.Site = new Site ();

			engine.InitNew ();
			items = engine.Items;

			items.CreateItem ("item1", VsaItemType.Reference, VsaItemFlag.None);
			items.CreateItem ("item2", VsaItemType.AppGlobal, VsaItemFlag.None);
			items.CreateItem ("item3", VsaItemType.Code, VsaItemFlag.Module);
			items.CreateItem ("item4", VsaItemType.Code, VsaItemFlag.Module);
			items.CreateItem ("item5", VsaItemType.Code, VsaItemFlag.Module);
			items.CreateItem ("item6", VsaItemType.Code, VsaItemFlag.Module);
			items.CreateItem ("item7", VsaItemType.Code, VsaItemFlag.Module);
			items.CreateItem ("item8", VsaItemType.Code, VsaItemFlag.Module);
			items.CreateItem ("item9", VsaItemType.Code, VsaItemFlag.Module);
			items.CreateItem ("item10", VsaItemType.Code, VsaItemFlag.Module);

			item = items [2];
			Assert.AreEqual ("item3", item.Name, "#16");

			items.Remove ("item6");
			Assert.AreEqual ("item7", items [5].Name, "#17");

			try { 
				items.Remove ("itemNonExistent");
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemNotFound, e.ErrorCode, "#18");
			}
		
			items.Remove (3);
			Assert.AreEqual ("item5", items [3].Name, "#19");
		}

		[Test]
		public void CreateItemOnRepeatedName ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;

			engine = new VsaEngine ();
			engine.RootMoniker = "com.foo://path/to/nowhere";
			engine.Site = new Site ();

			engine.InitNew ();
			items = engine.Items;

			items.CreateItem ("item2", VsaItemType.AppGlobal, VsaItemFlag.None);

			try {
				items.CreateItem ("item2", 
						  VsaItemType.Reference, 
						  VsaItemFlag.None);
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.ItemNameInUse, e.ErrorCode, "#20");
			}
		}
	}
}