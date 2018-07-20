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

	struct DefaultStruct {
		public int never_used;
		public int used;
	}

	[StructLayout (LayoutKind.Auto)]
	struct AutoStruct {
		public int never_used;
		public int used;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct SequentialStruct {
		public int never_used;
		public int used;
	}

	[StructLayout (LayoutKind.Explicit)]
	struct ExplicitStruct {
		[FieldOffset (0)]
		public int never_used;
		[FieldOffset (4)]
		public int used;
		[FieldOffset (8)]
		public int never_ever_used;
	}

	[TestFixture]
	// we want the tests to be available because we use the linker
	[Preserve (AllMembers = true)]
	public class StructLayoutTest {

		[Test]
		public void DefaultLayoutStruct () // sequential
		{
			DefaultStruct c = new DefaultStruct ();
			c.used = 1;
			Assert.That (Marshal.SizeOf (c), Is.EqualTo (8), "2 fields");
			var t = typeof (DefaultStruct);
			var fields = t.GetFields ();
			Assert.That (fields.Length, Is.EqualTo (2), "Length");

			Assert.False (t.IsAutoLayout, "IsAutoLayout");
			Assert.False (t.IsExplicitLayout, "IsExplicitLayout");
			Assert.True (t.IsLayoutSequential, "IsLayoutSequential");
		}

		[Test]
		public void AutoLayoutStruct ()
		{
			AutoStruct c = new AutoStruct ();
			c.used = 1;
			// can't ask SizeOf on Auto
			var t = typeof (AutoStruct);
			var fields = t.GetFields ();
			Assert.That (fields.Length, Is.EqualTo (2), "Length");

			Assert.True (t.IsAutoLayout, "IsAutoLayout");
			Assert.False (t.IsExplicitLayout, "IsExplicitLayout");
			Assert.False (t.IsLayoutSequential, "IsLayoutSequential");
		}

		[Test]
		public void LayoutSequential ()
		{
			SequentialStruct c = new SequentialStruct ();
			c.used = 1;
			Assert.That (Marshal.SizeOf (c), Is.EqualTo (8), "2 fields");
			var t = typeof (SequentialStruct);
			var fields = t.GetFields ();
			Assert.That (fields.Length, Is.EqualTo (2), "Length");

			Assert.False (t.IsAutoLayout, "IsAutoLayout");
			Assert.False (t.IsExplicitLayout, "IsExplicitLayout");
			Assert.True (t.IsLayoutSequential, "IsLayoutSequential");
		}

		[Test]
		public void ExplicitLayout ()
		{
			ExplicitStruct c = new ExplicitStruct ();
			c.used = 1;
			Assert.That (Marshal.SizeOf (c), Is.GreaterThanOrEqualTo (12), "3 fields");
			var t = typeof (ExplicitStruct);
			var fields = t.GetFields ();
			Assert.That (fields.Length, Is.EqualTo (3), "Length");

			Assert.False (t.IsAutoLayout, "IsAutoLayout");
			Assert.True (t.IsExplicitLayout, "IsExplicitLayout");
			Assert.False (t.IsLayoutSequential, "IsLayoutSequential");
		}
	}
}