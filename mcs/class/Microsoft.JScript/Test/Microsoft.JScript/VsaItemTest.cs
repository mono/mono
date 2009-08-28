//
// VsaItemTest.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using NUnit.Framework;
using System;
using Microsoft.Vsa;
using Microsoft.JScript.Vsa;
using Microsoft.JScript;

namespace MonoTests.Microsoft.JScript {

	[TestFixture]
	public class VsaItemTest {

		[Test]
		public void IsDirtyOnEngineClosed ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine.RootMoniker = "foo://nowhere/path";
			engine.Site = new Site ();
			engine.InitNew ();

			items = engine.Items;			

			item = items.CreateItem ("item1", VsaItemType.Reference, VsaItemFlag.None);
			engine.Close ();

			try {
				bool dirty = item.IsDirty;
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.EngineClosed, e.ErrorCode, "#1");
			}
		}

		[Test]
		public void IsDirtyOnCreateItem ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine.RootMoniker = "foo://nowhere/path";
			engine.Site = new Site ();
			engine.InitNew ();

			items = engine.Items;			

			item = items.CreateItem ("item1", VsaItemType.Reference, VsaItemFlag.None);
			Assert.AreEqual (true,item.IsDirty, "#2");

			item = items.CreateItem ("item2", VsaItemType.AppGlobal, VsaItemFlag.None);
			Assert.AreEqual (true, item.IsDirty, "#3");

			item = items.CreateItem ("item3", VsaItemType.Code, VsaItemFlag.Module);
			Assert.AreEqual (true, item.IsDirty, "#4");
		}

		[Test]
		public void ItemTypeOnEngineClosed ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine.RootMoniker = "foo://nowhere/path";
			engine.Site = new Site ();
			engine.InitNew ();

			items = engine.Items;			

			item = items.CreateItem ("item1", VsaItemType.Reference, VsaItemFlag.None);

			engine.Close ();

			try {
				VsaItemType type = item.ItemType;
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.EngineClosed, e.ErrorCode, "#5");
			}
		}

		[Test]
		public void GetOptionOnEngineClosed ()
		{		
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine.RootMoniker = "foo://nowhere/path";
			engine.Site = new Site ();
			engine.InitNew ();

			items = engine.Items;			

			item = items.CreateItem ("item1", VsaItemType.Reference, VsaItemFlag.None);

			engine.Close ();

			try {
				object opt = item.GetOption ("AlwaysGenerateIL");
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.EngineClosed, e.ErrorCode, "#6");
			}
		}

		[Test]
		public void GetOptionOnOptionNotSupported ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine.RootMoniker = "foo://nowhere/path";
			engine.Site = new Site ();
			engine.InitNew ();

			items = engine.Items;			

			item = items.CreateItem ("item1", VsaItemType.Reference, VsaItemFlag.None);

			try {
				item.GetOption ("OptionNotSupportedByThisScriptingEngine");
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.OptionNotSupported, e.ErrorCode, "#7");
			}
		}

		[Test]
		public void SetOptionOnEngineClosed ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine.RootMoniker = "foo://nowhere/path";
			engine.Site = new Site ();
			engine.InitNew ();

			items = engine.Items;			

			item = items.CreateItem ("item1", VsaItemType.Reference, VsaItemFlag.None);

			engine.Close ();

			try {
				item.SetOption ("AlwaysGenerateIL", true);
			} catch (VsaException e) {
				Assert.AreEqual (VsaError.EngineClosed, e.ErrorCode, "#8");
			}
		}

		[Test]
		public void SetOptionOnOptionNotSupported ()
		{
			VsaEngine engine = new VsaEngine ();
			IVsaItems items;
			IVsaItem item;

			engine.RootMoniker = "foo://nowhere/path";
			engine.Site = new Site ();
			engine.InitNew ();
  
			items = engine.Items;			

			item = items.CreateItem ("item1", VsaItemType.Reference, VsaItemFlag.None);

			try {
				item.SetOption ("OptionNotSupportedByThisScriptingEngine", true);
  			} catch (VsaException e) {
				Assert.AreEqual (VsaError.OptionNotSupported, e.ErrorCode, "#9");
			}
		}					
	}
}

public class Site : BaseVsaSite
{}
