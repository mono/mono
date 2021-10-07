// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// (C) iain@mccoy.id.au
//
// Authors:
//	Iain McCoy (iain@mccoy.id.au)
//	Chris Toshok (toshok@ximian.com)
//	Loïc Rebmeister (fox2code@gmail.com)
//

using System;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	class FreezablePoker : Freezable {
		public bool changedCalled = false;
		public bool onChangedCalled = false;

		public FreezablePoker ()
		{
			changed += new EventHandler (changedHandler);
		}

		private void changedHandler (object obj, EventArgs args)
		{
			changedCalled = true;
		}

		protected override void OnChanged ()
		{
			base.OnChanged ();
			onChangedCalled = true;
		}

		protected override Freezable CreateInstanceCore () {
			return new FreezablePoker ();
		}
	};

	class Unfreezable : Freezable {

		protected override bool FreezeCore (bool isChecking) {
			return false;
		}

		protected override Freezable CreateInstanceCore () {
			return new Unfreezable ();
		}
	}

	[TestFixture]
	public class FreezableTest {
		public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached ("Value", typeof (string), typeof (FreezableTest));

		// Test that the Frezzable.Freeze () and that a cloned object with Clone () is not frozen even if the source is frozen
		[Test]
		public void TestFreezeClone ()
		{
			var freezable = new FreezablePoker ();
			freezable.SetValue (ValueProperty, "test");
			freezable.Freeze ();
			Assert.IsTrue (freezable.IsFrozen, "Freezable is not frozen!");
			var freezableClone = freezable.Clone ();
			Assert.IsFalse (freezableClone.IsFrozen, "FreezableClone is frozen!");
			Assert.AreEqual (freezableClone.GetValue (ValueProperty), "test",
				"FreezableClone ValueProperty wasn't copied properly!");
		}

		// Test that event are properly propagated
		[Test]
		public void TestEvents ()
		{
			var freezable = new FreezablePoker ();
			testEventHelper (freezable, false, false, "init");
			freezable.SetValue (ValueProperty, "test");
			testEventHelper (freezable, true, false, false, "SetValue");
			freezable.WritePostscript ();
			testEventHelper (freezable, true, true, "WritePostscript");
		}

		private void testEventHelper (FreezablePoker freezable, bool changed,bool onChanged,string state)
		{
			if (changed)
				Assert.IsTrue (freezable.changedCalled, "changed wasn't raised on " + state);
			else
				Assert.IsFalse (freezable.changedCalled, "changed was raised on " + state);
			if (onChanged)
				Assert.IsTrue (freezable.onChangedCalled, "OnChanged wasn't called on " + state);
			else
				Assert.IsFalse (freezable.onChangedCalled, "OnChanged was called on " + state);
			freezable.changedCalled = false;
			freezable.onChangedCalled = false;
		}

		// Test that CanFreeze test child freezable properties and that Freeze also freeze all Freezable childs
		[Test]
		public void TestFreezePropagation ()
		{
			var freezable = new FreezablePoker ();
			freezable.SetValue (FreezableProperty, new Unfreezable ());
			Assert.IsFalse (freezable.CanFreeze, "CanFreeze return true with an unfreezable children!");
			var childFreezable = new FreezablePoker ();
			freezable.SetValue (FreezableProperty, childFreezable);
			freezable.Freeze ();
			Assert.IsFalse (childFreezable.IsFrozen, "child Freezable is not frozen after it's parent was forzen!");
		}
	}
}
