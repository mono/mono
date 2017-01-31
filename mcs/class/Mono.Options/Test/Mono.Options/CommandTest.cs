//
// CommandSetTest.cs
//
// Authors:
//  Jonathan Pryor <Jonathan.Pryor@microsoft.com>
//
// Copyright (C) 2017 Microsoft (http://www.microsoft.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;

#if NDESK_OPTIONS
using NDesk.Options;
#else
using Mono.Options;
#endif

using Cadenza.Collections.Tests;

using NUnit.Framework;

#if NDESK_OPTIONS
namespace Tests.NDesk.Options
#else
namespace MonoTests.Mono.Options
#endif
{
	[TestFixture]
	public class CommandTest
	{
		[Test]
		public void Constructor_NameRequired ()
		{
			Assert.Throws<ArgumentNullException> (() => new Command (name: null, help: null));
		}

		[Test]
		public void Constructor ()
		{
			var c = new Command ("command", "help");
			Assert.AreEqual ("command", c.Name);
			Assert.AreEqual ("help",    c.Help);
		}

		[Test]
		public void Invoke_CallsRun ()
		{
			bool runInvoked = false;
			var c = new Command ("command") {
				Run = v => runInvoked = true,
			};
			Assert.AreEqual (0, c.Invoke (null));
			Assert.IsTrue (runInvoked);
		}

		[Test]
		public void Invoke_RequiresNothing ()
		{
			var c = new Command ("c");
			Assert.AreEqual (0, c.Invoke (null));
		}

		[Test]
		public void Invoke_UsesOptions ()
		{
			bool showHelp = false;
			var c = new Command ("c") {
				Options = new OptionSet {
					{ "help", v => showHelp = v != null },
				},
			};
			Assert.AreEqual (0, c.Invoke (new [] { "--help" }));
			Assert.IsTrue (showHelp);
		}
	}
}

