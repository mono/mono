//
// TraceSourceTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//

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

using NUnit.Framework;
using System;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Diagnostics;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class TraceSourceTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullName ()
		{
			new TraceSource (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorEmpty ()
		{
			new TraceSource ("");
		}
		
		[Test]
		public void DefaultValues ()
		{
			TraceSource ts = new TraceSource ("foo");
			Assert.AreEqual ("foo", ts.Name, "#1");
			Assert.IsNotNull (ts.Switch, "#2");
			Assert.AreEqual (SourceLevels.Off, ts.Switch.Level, "#3");
			Assert.IsNotNull (ts.Listeners, "#4");
			Assert.AreEqual (1, ts.Listeners.Count, "#5");
			Assert.IsNotNull (ts.Attributes, "#6");
			Assert.AreEqual (0, ts.Attributes.Count, "#7");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetSourceSwitchNull ()
		{
			TraceSource ts = new TraceSource ("foo");
			ts.Switch = null;
		}
		
		[Test]
		public void SwitchLevel ()
		{
			TraceSource s = new TraceSource ("Source1");
			Assert.AreEqual (SourceLevels.Off, s.Switch.Level, "#1");

			s = new TraceSource("Source2", SourceLevels.All);
			Assert.AreEqual (SourceLevels.All, s.Switch.Level, "#2");

			s = new TraceSource("Source3");
			s.Switch.Level = SourceLevels.All;
			Assert.AreEqual (SourceLevels.All, s.Switch.Level, "#3");
		}
	}
}

#endif
