//
// Link All [Regression] Tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc. All rights reserved.
//

using System;
using System.Runtime.InteropServices;
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

namespace LinkAll.Layout {

	class DefaultClass {
		public int never_used;		// the linker will remove this
		public int used;
	}

	[StructLayout (LayoutKind.Auto)]
	class AutoClass {
		public int never_used;		// the linker will remove this
		public int used;
	}

	[StructLayout (LayoutKind.Sequential)]
	class SequentialClass {
		public int never_used;		// the linker MUST NOT remove this
		public int used;
	}

	[StructLayout (LayoutKind.Explicit)]
	class ExplicitClass {
		[FieldOffset (0)]
		public int never_used;		// the linker could remove this
		[FieldOffset (4)]
		public int used;
		[FieldOffset (8)]
		public int never_ever_used;	// the linker MUST NOT remove this
	}

	[TestFixture]
	// we want the tests to be available because we use the linker
	[Preserve (AllMembers = true)]
	public class ClassLayoutTest {

		[Test]
		public void DefaultLayoutClass () // auto
		{
			DefaultClass c = new DefaultClass ();
			c.used = 1;
			// can't ask SizeOf on Auto
			var t = typeof (DefaultClass);
			var fields = t.GetFields ();
			Assert.That (fields.Length, Is.EqualTo (1), "Length");
			Assert.That (fields [0].Name, Is.EqualTo ("used"), "Name");

			Assert.True (t.IsAutoLayout, "IsAutoLayout");
			Assert.False (t.IsExplicitLayout, "IsExplicitLayout");
			Assert.False (t.IsLayoutSequential, "IsLayoutSequential");
		}

		[Test]
		public void AutoLayoutClass ()
		{
			AutoClass c = new AutoClass ();
			c.used = 1;
			// can't ask SizeOf on Auto
			var t = typeof (AutoClass);
			var fields = t.GetFields ();
			Assert.That (fields.Length, Is.EqualTo (1), "Length");
			Assert.That (fields [0].Name, Is.EqualTo ("used"), "Name");

			Assert.True (t.IsAutoLayout, "IsAutoLayout");
			Assert.False (t.IsExplicitLayout, "IsExplicitLayout");
			Assert.False (t.IsLayoutSequential, "IsLayoutSequential");
		}

		[Test]
		public void LayoutSequential ()
		{
			SequentialClass c = new SequentialClass ();
			c.used = 1;
			Assert.That (Marshal.SizeOf (c), Is.EqualTo (8), "2 fields");
			var t = typeof (SequentialClass);
			var fields = t.GetFields ();
			Assert.That (fields.Length, Is.EqualTo (2), "Length");

			Assert.False (t.IsAutoLayout, "IsAutoLayout");
			Assert.False (t.IsExplicitLayout, "IsExplicitLayout");
			Assert.True (t.IsLayoutSequential, "IsLayoutSequential");
		}

		[Test]
		public void ExplicitLayout ()
		{
			ExplicitClass c = new ExplicitClass ();
			c.used = 1;
			Assert.That (Marshal.SizeOf (c), Is.GreaterThanOrEqualTo (12), "3 fields");
			var t = typeof (ExplicitClass);
			var fields = t.GetFields ();
			Assert.That (fields.Length, Is.EqualTo (3), "Length");

			Assert.False (t.IsAutoLayout, "IsAutoLayout");
			Assert.True (t.IsExplicitLayout, "IsExplicitLayout");
			Assert.False (t.IsLayoutSequential, "IsLayoutSequential");
		}
	}
}