// Copyright 2016 Xamarin Inc. All rights reserved.

using System;
#if XAMCORE_2_0
using Foundation;
#else
#if MONOTOUCH
using MonoTouch.Foundation;
#endif
#endif
using NUnit.Framework;

namespace Linker.Sealer {

	[Preserve (AllMembers = true)]
	public class Unsealable {

		public virtual bool A () { return true; }
		public virtual bool B () { return false; }
	}

	[Preserve (AllMembers = true)]
	public class Sealable : Unsealable {
		public override bool B () { return true; }
		public virtual bool C () { return false; }
	}

	interface Interface {
		bool A ();
	}

	[Preserve (AllMembers = true)]
	public class Base {
		public bool A () { return true; }
	}

	[Preserve (AllMembers = true)]
	public class Subclass : Base, Interface {
	}

	[TestFixture]
	[Preserve (AllMembers = true)]
	public class SealerTest {

		[Test]
		public void Sealed ()
		{
			// this can not be optimized into a sealed type
			Assert.False (typeof (Unsealable).IsSealed, "Unsealed");
#if DEBUG || !MOBILE // TODO: fails on Mono Desktop, sealed optimization is part of SealerSubStep in XI
			// this is not a sealed type (in the source)
			Assert.False (typeof (Sealable).IsSealed, "Sealable");
			Assert.False (typeof (Base).IsSealed, "Base");
			Assert.False (typeof (Subclass).IsSealed, "Subclass");
			Assert.False (typeof (Interface).IsSealed, "Interface");
#else
			// Sealable can be optimized / sealed as nothing else is (or can) subclass it
			Assert.True (typeof (Sealable).IsSealed, "Sealable");
			// Base is subclassed so it can't be sealed
			Assert.False (typeof (Base).IsSealed, "Base");
			// Subclass is not subclassed anymore and can be sealed
			Assert.True (typeof (Subclass).IsSealed, "Subclass");
			// interface can not be sealed
			Assert.False (typeof (Interface).IsSealed, "Interface");
#endif
		}

		[Test]
		public void Final ()
		{
			var t = typeof (Sealable);
			var a = t.GetMethod ("A");
			var b = t.GetMethod ("B");
			var c = t.GetMethod ("C");
#if DEBUG || !MOBILE // TODO: fails on Mono Desktop, sealed optimization is part of SealerSubStep in XI
			// this is not a sealed (C#) method (in the source)
			Assert.False (a.IsFinal, "A");
			Assert.False (b.IsFinal, "B");
			Assert.False (c.IsFinal, "C");
#else
			// but it can be optimized / sealed as nothing else is (or can) overrides it
			Assert.True (a.IsFinal, "A");
			Assert.True (b.IsFinal, "B");
			Assert.True (c.IsFinal, "C");
#endif
		}

		[Test]
		public void Virtual ()
		{
			var t = typeof (Sealable);
			var a = t.GetMethod ("A");
			var b = t.GetMethod ("B");
			var c = t.GetMethod ("C");
#if DEBUG || !MOBILE // TODO: fails on Mono Desktop, sealed optimization is part of SealerSubStep in XI
			// both methods are virtual (both in C# and IL)
			Assert.True (a.IsVirtual, "A");
			Assert.True (b.IsVirtual, "B");
			Assert.True (c.IsVirtual, "C");
#else
			// calling A needs dispatch to base type Unsealable
			Assert.True (a.IsVirtual, "A");
			// B is an override and must remain virtual
			Assert.True (b.IsVirtual, "B");
			// C has no special requirement and can be de-virtualized
			Assert.False (c.IsVirtual, "C");
#endif
		}

		[Test]
		public void Interface ()
		{
			var t = typeof (Subclass);
			var a = t.GetMethod ("A");
			// A cannot be de-virtualized since Concrete must satisfy Interface thru Base
			Assert.True (a.IsVirtual, "A");
		}
	}
}
