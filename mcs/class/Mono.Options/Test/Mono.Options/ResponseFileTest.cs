//
// ResponseFileTest.cs
//
// Authors:
//  Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (C) 2018 Microsoft (http://www.microsoft.com)
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
	public class ResponseFileTest
	{
		[Test]
		public void ResponseFileParser ()
		{
            const string text = "--arg1=value1\n\"--arg2=\\\"value2 contains nested quoting\\\"\"\n\"--arg3=value3\"\n";
            var source = new ResponseFileSource ();
            var path = Path.GetTempFileName ();
            IEnumerable<string> replacement;

            File.WriteAllText (path, text);

            try {
                Assert.IsTrue (source.GetArguments ("@" + path, out replacement));
            } catch {
                throw;
            } finally {
                File.Delete (path);
            }

            var args = replacement.ToList ();
            Assert.AreEqual (3, args.Count);
            Assert.AreEqual ("--arg1=value1", args[0]);
            Assert.AreEqual ("--arg2=\"value2 contains nested quoting\"", args[1]);
            Assert.AreEqual ("--arg3=value3", args[2]);
		}
	}
}
