//
// OptionTest.cs
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
	class DefaultOption : Option {
		public DefaultOption (string prototypes, string description)
			: base (prototypes, description)
		{
		}

		public DefaultOption (string prototypes, string description, int c)
			: base (prototypes, description, c)
		{
		}

		protected override void OnParseComplete (OptionContext c)
		{
			throw new NotImplementedException ();
		}
	}

	[TestFixture]
	public class OptionTest {
		[Test]
		public void Exceptions ()
		{
			object p = null;
			Utils.AssertException (typeof(ArgumentNullException), 
					$"Value cannot be null.{Environment.NewLine}Parameter name: prototype", 
					p, v => { new DefaultOption (null, null); });
			Utils.AssertException (typeof(ArgumentException), 
					$"Cannot be the empty string.{Environment.NewLine}Parameter name: prototype",
					p, v => { new DefaultOption ("", null); });
			Utils.AssertException (typeof(ArgumentException),
					$"Empty option names are not supported.{Environment.NewLine}Parameter name: prototype",
					p, v => { new DefaultOption ("a|b||c=", null); });
			Utils.AssertException (typeof(ArgumentException),
					$"Conflicting option types: '=' vs. ':'.{Environment.NewLine}Parameter name: prototype",
					p, v => { new DefaultOption ("a=|b:", null); });
			Utils.AssertException (typeof(ArgumentException),
					$"The default option handler '<>' cannot require values.{Environment.NewLine}Parameter name: prototype",
					p, v => { new DefaultOption ("<>=", null); });
			Utils.AssertException (typeof(ArgumentException),
					$"The default option handler '<>' cannot require values.{Environment.NewLine}Parameter name: prototype",
					p, v => { new DefaultOption ("<>:", null); });
			Utils.AssertException (null, null,
					p, v => { new DefaultOption ("t|<>=", null, 1); });
			Utils.AssertException (typeof(ArgumentException),
					$"The default option handler '<>' cannot require values.{Environment.NewLine}Parameter name: prototype",
					p, v => { new DefaultOption ("t|<>=", null, 2); });
			Utils.AssertException (null, null,
					p, v => { new DefaultOption ("a|b=", null, 2); });
			Utils.AssertException (typeof(ArgumentOutOfRangeException),
					$"Specified argument was out of the range of valid values.{Environment.NewLine}Parameter name: maxValueCount",
					p, v => { new DefaultOption ("a", null, -1); });
			Utils.AssertException (typeof(ArgumentException),
					"Cannot provide maxValueCount of 0 for OptionValueType.Required or " +
						$"OptionValueType.Optional.{Environment.NewLine}Parameter name: maxValueCount",
					p, v => { new DefaultOption ("a=", null, 0); });
			Utils.AssertException (typeof(ArgumentException),
					"Ill-formed name/value separator found in \"a={\"." + Environment.NewLine + "Parameter name: prototype",
					p, v => { new DefaultOption ("a={", null); });
			Utils.AssertException (typeof(ArgumentException),
					"Ill-formed name/value separator found in \"a=}\"." + Environment.NewLine + "Parameter name: prototype",
					p, v => { new DefaultOption ("a=}", null); });
			Utils.AssertException (typeof(ArgumentException),
					"Ill-formed name/value separator found in \"a={{}}\"." + Environment.NewLine + "Parameter name: prototype",
					p, v => { new DefaultOption ("a={{}}", null); });
			Utils.AssertException (typeof(ArgumentException),
					"Ill-formed name/value separator found in \"a={}}\"." + Environment.NewLine + "Parameter name: prototype",
					p, v => { new DefaultOption ("a={}}", null); });
			Utils.AssertException (typeof(ArgumentException),
					"Ill-formed name/value separator found in \"a={}{\"." + Environment.NewLine + "Parameter name: prototype",
					p, v => { new DefaultOption ("a={}{", null); });
			Utils.AssertException (typeof(ArgumentException),
					$"Cannot provide key/value separators for Options taking 1 value(s).{Environment.NewLine}Parameter name: prototype",
					p, v => { new DefaultOption ("a==", null); });
			Utils.AssertException (typeof(ArgumentException),
					$"Cannot provide key/value separators for Options taking 1 value(s).{Environment.NewLine}Parameter name: prototype",
					p, v => { new DefaultOption ("a={}", null); });
			Utils.AssertException (typeof(ArgumentException),
					$"Cannot provide key/value separators for Options taking 1 value(s).{Environment.NewLine}Parameter name: prototype",
					p, v => { new DefaultOption ("a=+-*/", null); });
			Utils.AssertException (null, null,
					p, v => { new DefaultOption ("a", null, 0); });
			Utils.AssertException (null, null,
					p, v => { new DefaultOption ("a", null, 0); });
			Utils.AssertException (null, null, 
					p, v => {
						var d = new DefaultOption ("a", null);
						Assert.AreEqual (d.GetValueSeparators ().Length, 0);
					});
			Utils.AssertException (null, null,
					p, v => {
						var d = new DefaultOption ("a=", null, 1);
						string[] s = d.GetValueSeparators ();
						Assert.AreEqual (s.Length, 0);
					});
			Utils.AssertException (null, null,
					p, v => {
						var d = new DefaultOption ("a=", null, 2);
						string[] s = d.GetValueSeparators ();
						Assert.AreEqual (s.Length, 2);
						Assert.AreEqual (s [0], ":");
						Assert.AreEqual (s [1], "=");
					});
			Utils.AssertException (null, null,
					p, v => {
						var d = new DefaultOption ("a={}", null, 2);
						string[] s = d.GetValueSeparators ();
						Assert.AreEqual (s.Length, 0);
					});
			Utils.AssertException (null, null,
					p, v => {
						var d = new DefaultOption ("a={-->}{=>}", null, 2);
						string[] s = d.GetValueSeparators ();
						Assert.AreEqual (s.Length, 2);
						Assert.AreEqual (s [0], "-->");
						Assert.AreEqual (s [1], "=>");
					});
			Utils.AssertException (null, null,
					p, v => {
						var d = new DefaultOption ("a=+-*/", null, 2);
						string[] s = d.GetValueSeparators ();
						Assert.AreEqual (s.Length, 4);
						Assert.AreEqual (s [0], "+");
						Assert.AreEqual (s [1], "-");
						Assert.AreEqual (s [2], "*");
						Assert.AreEqual (s [3], "/");
					});
		}
	}
}

