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
	public class VsaItemsTest : Assertion {

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
				AssertEquals ("#1", VsaError.EngineClosed, e.ErrorCode);
			}

			try {
				item = items.CreateItem ("itemx", VsaItemType.Code, 
							 VsaItemFlag.Class);
			} catch (VsaException e) {
				AssertEquals ("#2", VsaError.EngineClosed, e.ErrorCode);
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
				AssertEquals ("#3", VsaError.ItemNotFound, e.ErrorCode);
			}


			try {
				item = items [20];
			} catch (VsaException e) {
				AssertEquals ("#4", VsaError.ItemNotFound, e.ErrorCode);
			}

			try {
				item = items ["IamNotHere"];
			} catch (VsaException e) {
				AssertEquals ("#5", VsaError.ItemNotFound, e.ErrorCode);
			}

			try {
				items.Remove (20);
			} catch (VsaException e) {
				AssertEquals ("#6", VsaError.ItemNotFound, e.ErrorCode);
			}

			try {
				items.Remove (-1);
			} catch (VsaException e) {
				AssertEquals ("#7", VsaError.ItemNotFound, e.ErrorCode);
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
				AssertEquals ("#8", VsaError.ItemFlagNotSupported, e.ErrorCode);
			}

			try {
				item = items.CreateItem ("item2", 
							  VsaItemType.Reference,
							  VsaItemFlag.Module);
			} catch (VsaException e) {
				AssertEquals ("#9", VsaError.ItemFlagNotSupported, e.ErrorCode);
			}

			try {
				item = items.CreateItem ("item3", 
							 VsaItemType.AppGlobal,
							 VsaItemFlag.Class);
			} catch (VsaException e) {
				AssertEquals ("#10", VsaError.ItemFlagNotSupported, e.ErrorCode);
			}

			try {
				item = items.CreateItem ("item4", 
							 VsaItemType.AppGlobal,
							 VsaItemFlag.Module);
			} catch (VsaException e) {
				AssertEquals ("#11", VsaError.ItemFlagNotSupported, e.ErrorCode);
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

			AssertEquals ("#12", 3, items.Count);

			try {
				item = items [4];
			} catch (VsaException e) {
				AssertEquals ("#13", VsaError.ItemNotFound, e.ErrorCode);
			}

			string ERASED_ITEM = "item2";

			items.Remove (ERASED_ITEM);

			AssertEquals ("#14", 2, items.Count);

			try {
				item = items [ERASED_ITEM];
			} catch (VsaException e) {
				AssertEquals ("#15", VsaError.ItemNotFound, e.ErrorCode);
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
			AssertEquals ("#16", "item3", item.Name);

			items.Remove ("item6");
			AssertEquals ("#17", "item7", items [5].Name);

			try { 
				items.Remove ("itemNonExistent");
			} catch (VsaException e) {
				AssertEquals ("#18", VsaError.ItemNotFound, e.ErrorCode);
			}
		
			items.Remove (3);
			AssertEquals ("#19", "item5", items [3].Name);
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
				AssertEquals ("#20", VsaError.ItemNameInUse, e.ErrorCode);
			}
		}
	}
}