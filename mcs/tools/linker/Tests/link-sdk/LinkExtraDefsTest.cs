//
// Unit tests for the mtouch's --xml linker option
//
// Authors:
//	Sebastien Pouliot <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved.
//

using System;
using System.Reflection;
#if XAMCORE_2_0
using Foundation;
#else
#if MONOTOUCH
using MonoTouch.Foundation;
#endif
#endif
using NUnit.Framework;

namespace LinkSdk {

	[TestFixture]
	[Preserve (AllMembers = true)]
	public class LinkExtraDefsTest {
		
		// ensure the types in extra-linker-defs.xml are included
		// even if they are:
		// * part of SDK / product assemblies, where adding [Preserve] is not possible;
		// * not used anywhere in this application
		
		// note: reflection is used so we're testing the XML-based preservation
		// 	not the normal linking process
		
		[Test]
		public void Corlib ()
		{
			Type t = Type.GetType ("System.Security.PermissionSet, mscorlib");
			Assert.NotNull (t, "System.Security.PermissionSet");
		}

		[Test]
		public void System ()
		{
			Type t = Type.GetType ("System.Net.Mime.ContentType, System");
			Assert.NotNull (t, "System.Net.Mime.ContentType");
			// we asked for ParseValue to be preserved
			Assert.NotNull (t.GetMethod ("ParseValue", BindingFlags.Instance | BindingFlags.NonPublic), "Parse");
		}

#if MONOTOUCH
#if !__WATCHOS__
		[Test]
		public void MonoTouch ()
		{
#if XAMCORE_2_0
			Type t = Type.GetType ("CoreBluetooth.CBUUID, " + typeof(NSObject).Assembly.ToString ());
#else
			Type t = Type.GetType ("MonoTouch.CoreBluetooth.CBUUID, monotouch, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
#endif
			Assert.NotNull (t, "[MonoTouch.]CoreBluetooth.CBUUID");
			// check (generated) fields since we instructed the linker to keep them
			var f = t.GetFields (BindingFlags.NonPublic | BindingFlags.Static);
			Assert.That (f.Length, Is.Not.EqualTo (0), "fields were preserved");
		}
#endif
#endif
	}
}