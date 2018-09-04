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
	public class CommandSetTest : ListContract<Command>
	{
		protected override ICollection<Command> CreateCollection (IEnumerable<Command> values)
		{
			var set = new CommandSet ("test");
			foreach (var value in values)
				set.Add (value);
			return set;
		}

		protected override Command CreateValueA ()
		{
			return new Command (
				"foo",
				"foo help");
		}

		protected override Command CreateValueB ()
		{
			return new Command (
				"bar",
				"bar help");
		}

		protected override Command CreateValueC ()
		{
			return new Command (
				"baz",
				"baz help");
		}

		static IEnumerable<string> _ (params string [] a)
		{
			return a;
		}

		[Test]
		public void Constructor_SuiteRequired ()
		{
			Assert.Throws<ArgumentNullException> (() => new CommandSet (null));
		}

		[Test]
		public void Add_NullCommand ()
		{
			var c = new CommandSet ("cs");
			Assert.Throws<ArgumentNullException> (() => c.Add ((Command)null));
			Assert.Throws<ArgumentNullException> (() => c.Add ((CommandSet)null));
		}

		[Test]
		public void Add_CommandCanBeAddedToOnlyOneSet ()
		{
			var cs1 = new CommandSet ("cs1");
			var cs2 = new CommandSet ("cs2");
			var c   = new Command ("command", "help");
			cs1.Add (c);
			Assert.Throws<ArgumentException> (() => cs2.Add (c));
		}

		[Test]
		public void Add_SetsCommandSet ()
		{
			var cs  = new CommandSet ("cs");
			var c   = new Command ("command");
			Assert.IsNull (c.CommandSet);
			cs.Add (c);
			Assert.AreSame (cs, c.CommandSet);
		}

		[Test]
		public void Add_DuplicateCommand ()
		{
			var s = new CommandSet ("set");
			s.Add (new Command ("value"));
			Assert.Throws<ArgumentException> (() => s.Add (new Command ("value")));
		}

		[Test]
		public void GetCompletions ()
		{
			var commands = new CommandSet ("example") {
				new Command ("a"),
				new Command ("aa"),
				new Command ("a a"),
				new Command ("cs c"),
				new CommandSet ("cs") {
					new CommandSet ("cs2") {
						new CommandSet ("cs3") {
							new Command ("cs-cs2-cs3-c"),
						},
					},
				},
			};
			Assert.IsTrue (new[]{
					"a",
					"aa",
					"a a",
					"cs c",
					"cs cs2 cs3 cs-cs2-cs3-c",
			}.SequenceEqual (commands.GetCompletions ()));

			Assert.IsTrue (new[]{
					"a",
					"aa",
					"a a",
			}.SequenceEqual (commands.GetCompletions ("a")));

			Assert.IsTrue (new[]{
					"cs c",
					"cs cs2 cs3 cs-cs2-cs3-c",
			}.SequenceEqual (commands.GetCompletions ("cs")));
		}

		[Test]
		public void Run_Help ()
		{
			var o = new StringWriter ();
			var e = new StringWriter ();

			var showVersion = false;
			var showHelp    = false;

			var git = new CommandSet ("git", output: o, error: e) {
				"usage: git [--version] ... <command> [<args>]",
				"",
				"Common Options:",
				{ "version",
				  "show version info",
				  v => showVersion = v != null },
				{ "help",
				  "show this message and exit",
				  v => showHelp = v != null },
				"",
				"These are common Git commands used in various situations:",
				"",
				"start a working area (see also: git help tutorial)",
				new Command ("clone", "Clone a repository into a new directory"),
				new Command ("init",  "Create an empty Git repository or reinitialize an existing one"),
				new Command ("this\thas spaces", "Spaces in command names?!"),
				new Command ("thisIsAVeryLongCommandNameInOrderToInduceWrapping", "Create an empty Git repository or reinitialize an existing one. Let's make this really long to cause a line wrap, shall we?"),
				new CommandSet ("nested") {
					"Surely nested commands need flavor text?",
					new Command ("foo", "nested foo help"),
					"And more text?",
					new Command ("bar", "nested bar help"),
					new CommandSet ("again") {
						"Yet more text!",
						new Command ("and again", "Wee! Nesting!"),
					}
				},
				new CommandSet ("another") {
					new Command ("foo", "another foo help"),
					new Command ("bar", "another bar help"),
				},
			};

			var expectedHelp = new StringWriter ();

			expectedHelp.WriteLine ("usage: git [--version] ... <command> [<args>]");
			expectedHelp.WriteLine ("");
			expectedHelp.WriteLine ("Common Options:");
			expectedHelp.WriteLine ("      --version              show version info");
			expectedHelp.WriteLine ("      --help                 show this message and exit");
			expectedHelp.WriteLine ("");
			expectedHelp.WriteLine ("These are common Git commands used in various situations:");
			expectedHelp.WriteLine ("");
			expectedHelp.WriteLine ("start a working area (see also: git help tutorial)");
			expectedHelp.WriteLine ("        clone                Clone a repository into a new directory");
			expectedHelp.WriteLine ("        init                 Create an empty Git repository or reinitialize an");
			expectedHelp.WriteLine ("                               existing one");
			expectedHelp.WriteLine ("        this has spaces      Spaces in command names?!");
			expectedHelp.WriteLine ("        thisIsAVeryLongCommandNameInOrderToInduceWrapping");
			expectedHelp.WriteLine ("                             Create an empty Git repository or reinitialize an");
			expectedHelp.WriteLine ("                               existing one. Let's make this really long to");
			expectedHelp.WriteLine ("                               cause a line wrap, shall we?");
			expectedHelp.WriteLine ("Surely nested commands need flavor text?");
			expectedHelp.WriteLine ("        nested foo           nested foo help");
			expectedHelp.WriteLine ("And more text?");
			expectedHelp.WriteLine ("        nested bar           nested bar help");
			expectedHelp.WriteLine ("Yet more text!");
			expectedHelp.WriteLine ("        nested again and again");
			expectedHelp.WriteLine ("                             Wee! Nesting!");
			expectedHelp.WriteLine ("        another foo          another foo help");
			expectedHelp.WriteLine ("        another bar          another bar help");

			Assert.AreEqual (0, git.Run (new [] { "help" }));
			Assert.AreEqual (expectedHelp.ToString (), o.ToString ());

			var expectedHelpHelp    = new StringWriter ();
			expectedHelpHelp.WriteLine ("Usage: git COMMAND [OPTIONS]");
			expectedHelpHelp.WriteLine ("Use `git help COMMAND` for help on a specific command.");
			expectedHelpHelp.WriteLine ();
			expectedHelpHelp.WriteLine ("Available commands:");
			expectedHelpHelp.WriteLine ();
			expectedHelpHelp.WriteLine ("        another bar          another bar help");
			expectedHelpHelp.WriteLine ("        another foo          another foo help");
			expectedHelpHelp.WriteLine ("        clone                Clone a repository into a new directory");
			expectedHelpHelp.WriteLine ("        init                 Create an empty Git repository or reinitialize an");
			expectedHelpHelp.WriteLine ("                               existing one");
			expectedHelpHelp.WriteLine ("        nested again and again");
			expectedHelpHelp.WriteLine ("                             Wee! Nesting!");
			expectedHelpHelp.WriteLine ("        nested bar           nested bar help");
			expectedHelpHelp.WriteLine ("        nested foo           nested foo help");
			expectedHelpHelp.WriteLine ("        this has spaces      Spaces in command names?!");
			expectedHelpHelp.WriteLine ("        thisIsAVeryLongCommandNameInOrderToInduceWrapping");
			expectedHelpHelp.WriteLine ("                             Create an empty Git repository or reinitialize an");
			expectedHelpHelp.WriteLine ("                               existing one. Let's make this really long to");
			expectedHelpHelp.WriteLine ("                               cause a line wrap, shall we?");
			expectedHelpHelp.WriteLine ("        help                 Show this message and exit");

			o.GetStringBuilder ().Clear ();
			Assert.AreEqual (0, git.Run (new [] { "help", "--help" }));
			Assert.AreEqual (expectedHelpHelp.ToString (), o.ToString ());
		}

		[Test]
		public void Run_Command ()
		{
			var a = 0;
			var b = 0;
			var d = 0;
			var g = 0;
			var c = new CommandSet ("set") {
				new Command ("a") { Run = v => a = v.Count () },
				new Command ("b") { Run = v => b = v.Count () },
				new Command ("c d")       { Run = v => d = v.Count () },
				new Command ("e\t f\ng")  { Run = v => g = v.Count () },
			};
			Assert.AreEqual (0, c.Run (new [] { "a", "extra" }));
			Assert.AreEqual (1, a);
			Assert.AreEqual (0, b);

			a = b = 0;
			Assert.AreEqual (0, c.Run (new [] { "b" }));
			Assert.AreEqual (0, a);
			Assert.AreEqual (0, b);

			Assert.AreEqual (0, c.Run (new [] { "b", "one", "two" }));
			Assert.AreEqual (0, a);
			Assert.AreEqual (2, b);

			Assert.AreEqual (1, c.Run (new [] { "c"}));

			Assert.AreEqual (0, c.Run (new [] { "c d", "one"}));
			Assert.AreEqual (1, d);

			Assert.AreEqual (0, c.Run (new [] { "c", "d", "one", "two"}));
			Assert.AreEqual (2, d);

			Assert.AreEqual (1, c.Run (new [] { "e" }));
			Assert.AreEqual (1, c.Run (new [] { "e f" }));
			Assert.AreEqual (1, c.Run (new [] { "e", "f" }));

			Assert.AreEqual (0, c.Run (new [] { "e f g"}));
			Assert.AreEqual (0, g);

			Assert.AreEqual (0, c.Run (new [] { "e f g", "one"}));
			Assert.AreEqual (1, g);

			Assert.AreEqual (0, c.Run (new [] { "e", "f", "g", "one", "two", "three"}));
			Assert.AreEqual (3, g);
		}

		[Test]
		public void Run_Command_NestedCommandSets ()
		{
			var a     = 0;
			var i_b   = 0;
			var i_i_c = 0;
			var outer = new CommandSet ("outer") {
				new Command ("a") { Run = v => a = v.Count () },
				new CommandSet ("intermediate") {
					new Command ("b") { Run = v => i_b = v.Count () },
					new CommandSet ("inner") {
						new Command ("c") { Run = v => i_i_c = v.Count () },
					}
				},
			};
			Assert.AreEqual (0, outer.Run (new[]{"a", "1"}));
			Assert.AreEqual (1, a);

			Assert.AreEqual (1, outer.Run (new[]{"intermediate"}));
			Assert.AreEqual (0, outer.Run (new[]{"intermediate", "b", "1", "2"}));
			Assert.AreEqual (2, i_b);

			Assert.AreEqual (1, outer.Run (new[]{"intermediate inner"}));
			Assert.AreEqual (0, outer.Run (new[]{"intermediate", "inner", "c", "1", "2", "3"}));
			Assert.AreEqual (3, i_i_c);
		}

		[Test]
		public void Run_HelpCommandSendsHelpOption ()
		{
			var e = new Command ("echo");
			e.Run = (args) => e.CommandSet.Out.WriteLine (string.Join (" ", args));

			var o = new StringWriter ();
			var c = new CommandSet ("set", output:o, error: Console.Error) {
				e,
			};
			Assert.AreEqual (0, c.Run (new [] { "help", "echo" }));

			var expected    = $"--help{Environment.NewLine}";
			var actual      = o.ToString ();
			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void Run_NullArgument ()
		{
			var c = new CommandSet ("c");
			Assert.Throws<ArgumentNullException> (() => c.Run (null));
		}
	}
}

