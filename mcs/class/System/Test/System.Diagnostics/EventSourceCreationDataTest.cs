//
// EventSourceCreationDataTest.cs -
// NUnit Test Cases for System.Diagnostics.EventSourceCreationData
//
// Author:
//	Gert Driesen <driesen@users.sourceforge.net>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

#if !MOBILE

using System;
using System.Diagnostics;

using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class EventLogCreationDataTest
	{
		[Test]
		public void Constructor1 ()
		{
			EventSourceCreationData crd;

			crd = new EventSourceCreationData (null, null);
			Assert.AreEqual (0, crd.CategoryCount, "#A1");
			Assert.IsNull (crd.CategoryResourceFile, "#A2");
			Assert.IsNull (crd.LogName, "#A3");
			Assert.IsNotNull (crd.MachineName, "#A4");
			Assert.AreEqual (".", crd.MachineName, "#A5");
			Assert.IsNull (crd.MessageResourceFile, "#A6");
			Assert.IsNull (crd.ParameterResourceFile, "#A7");
			Assert.IsNull (crd.Source, "#A8");

			crd = new EventSourceCreationData ("src", null);
			Assert.AreEqual (0, crd.CategoryCount, "#B1");
			Assert.IsNull (crd.CategoryResourceFile, "#B2");
			Assert.IsNull (crd.LogName, "#B3");
			Assert.IsNotNull (crd.MachineName, "#B4");
			Assert.AreEqual (".", crd.MachineName, "#B5");
			Assert.IsNull (crd.MessageResourceFile, "#B6");
			Assert.IsNull (crd.ParameterResourceFile, "#B7");
			Assert.IsNotNull (crd.Source, "#B8");
			Assert.AreEqual ("src", crd.Source, "#B9");

			crd = new EventSourceCreationData (null, "log");
			Assert.AreEqual (0, crd.CategoryCount, "#C1");
			Assert.IsNull (crd.CategoryResourceFile, "#C2");
			Assert.IsNotNull (crd.LogName, "#C3");
			Assert.AreEqual ("log", crd.LogName, "#C4");
			Assert.IsNotNull (crd.MachineName, "#C5");
			Assert.AreEqual (".", crd.MachineName, "#C6");
			Assert.IsNull (crd.MessageResourceFile, "#C7");
			Assert.IsNull (crd.ParameterResourceFile, "#C8");
			Assert.IsNull (crd.Source, "#C9");

			crd = new EventSourceCreationData ("src", "log");
			Assert.AreEqual (0, crd.CategoryCount, "#C1");
			Assert.IsNull (crd.CategoryResourceFile, "#C2");
			Assert.IsNotNull (crd.LogName, "#C3");
			Assert.AreEqual ("log", crd.LogName, "#C4");
			Assert.IsNotNull (crd.MachineName, "#C5");
			Assert.AreEqual (".", crd.MachineName, "#C6");
			Assert.IsNull (crd.MessageResourceFile, "#C7");
			Assert.IsNull (crd.ParameterResourceFile, "#C8");
			Assert.IsNotNull (crd.Source, "#C9");
			Assert.AreEqual ("src", crd.Source, "#C10");
		}

		[Test]
		public void CategoryCount ()
		{
			EventSourceCreationData crd = new EventSourceCreationData ("src", "log");
			crd.CategoryCount = 15;
			Assert.AreEqual (15, crd.CategoryCount, "#1");
			crd.CategoryCount = 0;
			Assert.AreEqual (0, crd.CategoryCount, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CategoryCount_Negative ()
		{
			EventSourceCreationData crd = new EventSourceCreationData ("src", "log");
			crd.CategoryCount = -1;
			Assert.AreEqual (-1, crd.CategoryCount);
		}

		[Test]
		public void CategoryResourceFile ()
		{
			EventSourceCreationData crd = new EventSourceCreationData ("src", "log");
			crd.CategoryResourceFile = "catRes";
			Assert.AreEqual ("catRes", crd.CategoryResourceFile, "#1");
			crd.CategoryResourceFile = null;
			Assert.IsNull (crd.CategoryResourceFile, "#2");
			crd.CategoryResourceFile = string.Empty;
			Assert.AreEqual (string.Empty, crd.CategoryResourceFile, "#3");
		}

		[Test]
		public void LogName ()
		{
			EventSourceCreationData crd = new EventSourceCreationData ("src", "log");
			Assert.AreEqual ("log", crd.LogName, "#1");
			crd.LogName = "newLog";
			Assert.AreEqual ("newLog", crd.LogName, "#2");
			crd.LogName = null;
			Assert.IsNull (crd.LogName, "#3");
			crd.LogName = string.Empty;
			Assert.AreEqual (string.Empty, crd.LogName, "#4");
		}

		[Test]
		public void MachineName ()
		{
			EventSourceCreationData crd = new EventSourceCreationData ("src", "log");
			Assert.AreEqual (".", crd.MachineName, "#1");
			crd.MachineName = "go-mono";
			Assert.AreEqual ("go-mono", crd.MachineName, "#2");
			crd.MachineName = null;
			Assert.IsNull (crd.MachineName, "#3");
			crd.MachineName = string.Empty;
			Assert.AreEqual (string.Empty, crd.MachineName, "#4");
		}

		[Test]
		public void MessageResourceFile ()
		{
			EventSourceCreationData crd = new EventSourceCreationData ("src", "log");
			Assert.IsNull (crd.MessageResourceFile, "#1");
			crd.MessageResourceFile = "msgRes";
			Assert.AreEqual ("msgRes", crd.MessageResourceFile, "#2");
			crd.MessageResourceFile = null;
			Assert.IsNull (crd.MessageResourceFile, "#3");
			crd.MessageResourceFile = string.Empty;
			Assert.AreEqual (string.Empty, crd.MessageResourceFile, "#4");
		}

		[Test]
		public void ParameterResourceFile ()
		{
			EventSourceCreationData crd = new EventSourceCreationData ("src", "log");
			Assert.IsNull (crd.ParameterResourceFile, "#1");
			crd.ParameterResourceFile = "paramRes";
			Assert.AreEqual ("paramRes", crd.ParameterResourceFile, "#2");
			crd.ParameterResourceFile = null;
			Assert.IsNull (crd.ParameterResourceFile, "#3");
			crd.ParameterResourceFile = string.Empty;
			Assert.AreEqual (string.Empty, crd.ParameterResourceFile, "#4");
		}

		[Test]
		public void Source ()
		{
			EventSourceCreationData crd = new EventSourceCreationData ("src", "log");
			Assert.AreEqual ("src", crd.Source, "#1");
			crd.Source = "newSrc";
			Assert.AreEqual ("newSrc", crd.Source, "#2");
			crd.Source = null;
			Assert.IsNull (crd.Source, "#3");
			crd.Source = string.Empty;
			Assert.AreEqual (string.Empty, crd.Source, "#4");
		}
	}
}
#endif
