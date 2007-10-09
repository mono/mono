//
// MenuCommandsTest.cs
//
// Author:
//	  Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.

//
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

#if NET_2_0

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms.Design;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms.Design
{
	[TestFixture]
	public class MenuCommandsTest
	{
		[Test]
		public void Commands ()
		{
			Guid wfMenuGroup = new Guid ("{74D21312-2AEE-11d1-8BFB-00A0C90F26F7}");
			Guid wfCommandSet = new Guid ("{74D21313-2AEE-11d1-8BFB-00A0C90F26F7}");
			Guid guidVSStd2K = new Guid ("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}");
			Guid guidVSStd97 = new Guid ("{5efc7975-14bc-11cf-9b2b-00aa00573819}");
			Assert.AreEqual (guidVSStd97, MenuCommands.EditLabel.Guid, "#1-1");
			Assert.AreEqual (338, MenuCommands.EditLabel.ID, "#1-2");
			Assert.AreEqual (guidVSStd2K, MenuCommands.KeyEnd.Guid, "#2-1");
			Assert.AreEqual (17, MenuCommands.KeyEnd.ID, "#2-2");
			Assert.AreEqual (guidVSStd2K, MenuCommands.KeyHome.Guid, "#3-1");
			Assert.AreEqual (15, MenuCommands.KeyHome.ID, "#3-2");
			Assert.AreEqual (guidVSStd2K, MenuCommands.KeyInvokeSmartTag.Guid, "#4-1");
			Assert.AreEqual (147, MenuCommands.KeyInvokeSmartTag.ID, "#4-2");
			Assert.AreEqual (guidVSStd2K, MenuCommands.KeyShiftEnd.Guid, "#5-1");
			Assert.AreEqual (18, MenuCommands.KeyShiftEnd.ID, "#5-2");
			Assert.AreEqual (guidVSStd2K, MenuCommands.KeyShiftHome.Guid, "#6-1");
			Assert.AreEqual (16, MenuCommands.KeyShiftHome.ID, "#6-2");
			Assert.AreEqual (wfCommandSet, MenuCommands.SetStatusRectangle.Guid, "#7-1");
			Assert.AreEqual (16388, MenuCommands.SetStatusRectangle.ID, "#7-2");
			Assert.AreEqual (wfCommandSet, MenuCommands.SetStatusText.Guid, "#8-1");
			Assert.AreEqual (16387, MenuCommands.SetStatusText.ID, "#8-2");
		}
	}
}
#endif
