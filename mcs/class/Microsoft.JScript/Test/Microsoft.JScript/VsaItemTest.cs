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
	public class VsaItemTest : Assertion {

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
				AssertEquals ("#1", VsaError.EngineClosed, e.ErrorCode);
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
			AssertEquals ("#2", true,item.IsDirty);

			item = items.CreateItem ("item2", VsaItemType.AppGlobal, VsaItemFlag.None);
			AssertEquals ("#3", true, item.IsDirty);

			item = items.CreateItem ("item3", VsaItemType.Code, VsaItemFlag.Module);
			AssertEquals ("#4", true, item.IsDirty);
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
				AssertEquals ("#5", VsaError.EngineClosed, e.ErrorCode);
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
				AssertEquals ("#6", VsaError.EngineClosed, e.ErrorCode);
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
				AssertEquals ("#7", VsaError.OptionNotSupported, e.ErrorCode);
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
				AssertEquals ("#8", VsaError.EngineClosed, e.ErrorCode);
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
				AssertEquals ("#9", VsaError.OptionNotSupported, e.ErrorCode);
			}
		}					
	}
}

public class Site : BaseVsaSite
{}
