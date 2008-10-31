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
// Author:
// 	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//

using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DragEventArgsTest : TestHelper
	{
		[Test]
		public void EffectTest ()
		{
			DragDropEffects allowed_effects = DragDropEffects.Copy | DragDropEffects.Link;
			DragEventArgs args = new DragEventArgs (null, 0, 0, 0, allowed_effects, DragDropEffects.Copy);

			Assert.AreEqual (allowed_effects, args.AllowedEffect, "#A1");
			Assert.AreEqual (DragDropEffects.Copy, args.Effect, "#A2");

			// An effect not part of AllowedEffect
			args.Effect = DragDropEffects.Move;
			Assert.AreEqual (DragDropEffects.Move, args.Effect, "#B1");
		}
	}
}

