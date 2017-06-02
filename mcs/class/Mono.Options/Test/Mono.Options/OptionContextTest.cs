//
// OptionContextTest.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
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

#if NDESK_OPTIONS
using NDesk.Options;
#else
using Mono.Options;
#endif

using NUnit.Framework;

#if NDESK_OPTIONS
namespace Tests.NDesk.Options
#else
namespace MonoTests.Mono.Options
#endif
{
	[TestFixture]
	public class OptionContextTest {
		[Test]
		public void Exceptions ()
		{
			OptionSet p = new OptionSet () {
				{ "a=", v => { /* ignore */ } },
			};
			OptionContext c = new OptionContext (p);
			Utils.AssertException (typeof(InvalidOperationException),
					"OptionContext.Option is null.",
					c, v => { string ignore = v.OptionValues [0]; });
			c.Option = p [0];
			Utils.AssertException (typeof(ArgumentOutOfRangeException),
					$"Specified argument was out of the range of valid values.{Environment.NewLine}Parameter name: index",
					c, v => { string ignore = v.OptionValues [2]; });
			c.OptionName = "-a";
			Utils.AssertException (typeof(OptionException),
					"Missing required value for option '-a'.",
					c, v => { string ignore = v.OptionValues [0]; });
		}
	}
}

