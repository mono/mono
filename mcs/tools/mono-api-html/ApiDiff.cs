//
// The main differences with mono-api-diff are:
// * this tool directly produce HTML similar to gdiff.sh used for Xamarin.iOS
// * this tool reports changes in an "evolutionary" way, not in a breaking way,
//   i.e. it does not assume the source assembly is right (but simply older)
// * the diff .xml output was not easy to convert back into the HTML format
//   that gdiff.sh produced
// 
// Authors
//    Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013-2014 Xamarin Inc. http://www.xamarin.com
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
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mono.Options;

namespace Xamarin.ApiDiff {

	public static class State {
		static TextWriter output;

		public static TextWriter Output { 
			get {
				if (output == null)
					output = Console.Out;
				return output;
			}
			set { output = value; } 
		}

		public static string Assembly { get; set; }
		public static string Namespace { get; set; }
		public static string Type { get; set; }
		public static string BaseType { get; set; }

		public static int Indent { get; set; }

		static List<Regex> ignoreAdded = new List<Regex> ();
		public static List<Regex> IgnoreAdded {
			get { return ignoreAdded; }
		}

		static List<Regex> ignoreNew = new List<Regex> ();
		public static List<Regex> IgnoreNew {
			get { return ignoreNew; }
		}

		static List<Regex> ignoreRemoved = new List<Regex> ();
		public static List<Regex> IgnoreRemoved {
			get { return ignoreRemoved; }
		}

		public  static  bool    IgnoreParameterNameChanges  { get; set; }
		public  static  bool    IgnoreVirtualChanges        { get; set; }
		public  static  bool    IgnoreAddedPropertySetters  { get; set; }

		public static bool IgnoreNonbreaking { get; set; }

		public static bool Lax;
		public static bool Colorize = true;
	}
	class Program {

		public static int Main (string[] args)
		{
			var showHelp = false;
			string diff = null;
			List<string> extra = null;

			var options = new OptionSet {
				{ "h|help", "Show this help", v => showHelp = true },
				{ "d|diff=", "HTML diff file out output (omit for stdout)", v => diff = v },
				{ "i|ignore=", "Ignore new, added, and removed members whose description matches a given C# regular expression (see below).",
					v => {
						var r = new Regex (v);
						State.IgnoreAdded.Add (r);
						State.IgnoreRemoved.Add (r);
						State.IgnoreNew.Add (r);
					}
				},
				{ "a|ignore-added=", "Ignore added members whose description matches a given C# regular expression (see below).",
					v => State.IgnoreAdded.Add (new Regex (v))
				},
				{ "r|ignore-removed=", "Ignore removed members whose description matches a given C# regular expression (see below).",
					v => State.IgnoreRemoved.Add (new Regex (v))
				},
				{ "n|ignore-new=", "Ignore new namespaces and types whose description matches a given C# regular expression (see below).",
					v => State.IgnoreNew.Add (new Regex (v))
				},
				{ "ignore-changes-parameter-names", "Ignore changes to parameter names for identically prototyped methods.",
					v => State.IgnoreParameterNameChanges   = v != null
				},
				{ "ignore-changes-property-setters", "Ignore adding setters to properties.",
					v => State.IgnoreAddedPropertySetters = v != null
				},
				{ "ignore-changes-virtual", "Ignore changing non-`virtual` to `virtual` or adding `override`.",
					v => State.IgnoreVirtualChanges = v != null
				},
				{ "c|colorize:", "Colorize HTML output", v => State.Colorize = string.IsNullOrEmpty (v) ? true : bool.Parse (v) },
				{ "x|lax", "Ignore duplicate XML entries", v => State.Lax = true },
				{ "ignore-nonbreaking", "Ignore all nonbreaking changes", v => State.IgnoreNonbreaking = true }
			};

			try {
				extra = options.Parse (args);
			} catch (OptionException e) {
				Console.WriteLine ("Option error: {0}", e.Message);
				showHelp = true;
			}

			if (State.IgnoreNonbreaking) {
				State.IgnoreAddedPropertySetters = true;
				State.IgnoreVirtualChanges = true;
				State.IgnoreNew.Add (new Regex (".*"));
				State.IgnoreAdded.Add (new Regex (".*"));
			}

			if (showHelp || extra == null || extra.Count < 2 || extra.Count > 3) {
				Console.WriteLine (@"Usage: mono-api-html [options] <reference.xml> <assembly.xml> [diff.html]");
				Console.WriteLine ();
				Console.WriteLine ("Available options:");
				options.WriteOptionDescriptions (Console.Out);
				Console.WriteLine ();
				Console.WriteLine ("Ignoring Members:");
				Console.WriteLine ();
				Console.WriteLine ("  Members that were added can be filtered out of the diff by using the");
				Console.WriteLine ("  -i, --ignore-added option. The option takes a C# regular expression");
				Console.WriteLine ("  to match against member descriptions. For example, to ignore the");
				Console.WriteLine ("  introduction of the interfaces 'INSCopying' and 'INSCoding' on types");
				Console.WriteLine ("  pass the following to mono-api-html:");
				Console.WriteLine ();
				Console.WriteLine ("    mono-api-html ... -i 'INSCopying$' -i 'INSCoding$'");
				Console.WriteLine ();
				Console.WriteLine ("  The regular expressions will match any member description ending with");
				Console.WriteLine ("  'INSCopying' or 'INSCoding'.");
				Console.WriteLine ();
				return 1;
			}

			var input = extra [0];
			var output = extra [1];
			if (extra.Count == 3 && diff == null)
				diff = extra [2];

			try {
				var ac = new AssemblyComparer (input, output);
				if (diff != null) {
					string diffHtml = String.Empty;
					using (var writer = new StringWriter ()) {
						State.Output = writer;
						ac.Compare ();
						diffHtml = State.Output.ToString ();
					}
					if (diffHtml.Length > 0) {
						using (var file = new StreamWriter (diff)) {
							file.WriteLine ("<div>");
							if (State.Colorize) {
								file.WriteLine ("<style scoped>");
								file.WriteLine ("\t.obsolete { color: gray; }");
								file.WriteLine ("\t.added { color: green; }");
								file.WriteLine ("\t.removed-inline { text-decoration: line-through; }");
								file.WriteLine ("\t.removed-breaking-inline { color: red;}");
								file.WriteLine ("\t.added-breaking-inline { text-decoration: underline; }");
								file.WriteLine ("\t.nonbreaking { color: black; }");
								file.WriteLine ("\t.breaking { color: red; }");
								file.WriteLine ("</style>");
							}
							file.WriteLine (
@"<script type=""text/javascript"">
	// Only some elements have 'data-is-[non-]breaking' attributes. Here we
	// iterate over all descendents elements, and set 'data-is-[non-]breaking'
	// depending on whether there are any descendents with that attribute.
	function propagateDataAttribute (element)
	{
		if (element.hasAttribute ('data-is-propagated'))
			return;

		var i;
		var any_breaking = element.hasAttribute ('data-is-breaking');
		var any_non_breaking = element.hasAttribute ('data-is-non-breaking');
		for (i = 0; i < element.children.length; i++) {
			var el = element.children [i];
			propagateDataAttribute (el);
			any_breaking |= el.hasAttribute ('data-is-breaking');
			any_non_breaking |= el.hasAttribute ('data-is-non-breaking');
		}
		
		if (any_breaking)
			element.setAttribute ('data-is-breaking', null);
		else if (any_non_breaking)
			element.setAttribute ('data-is-non-breaking', null);
		element.setAttribute ('data-is-propagated', null);
	}

	function hideNonBreakingChanges ()
	{
		var topNodes = document.querySelectorAll ('[data-is-topmost]');
		var n;
		var i;
		for (n = 0; n < topNodes.length; n++) {
			propagateDataAttribute (topNodes [n]);
			var elements = topNodes [n].querySelectorAll ('[data-is-non-breaking]');
			for (i = 0; i < elements.length; i++) {
				var el = elements [i];
				if (!el.hasAttribute ('data-original-display'))
					el.setAttribute ('data-original-display', el.style.display);
				el.style.display = 'none';
			}
		}
		
		var links = document.getElementsByClassName ('hide-nonbreaking');
		for (i = 0; i < links.length; i++)
			links [i].style.display = 'none';
		links = document.getElementsByClassName ('restore-nonbreaking');
		for (i = 0; i < links.length; i++)
			links [i].style.display = '';
	}

	function showNonBreakingChanges ()
	{
		var elements = document.querySelectorAll ('[data-original-display]');
		var i;
		for (i = 0; i < elements.length; i++) {
			var el = elements [i];
			el.style.display = el.getAttribute ('data-original-display');
		}

		var links = document.getElementsByClassName ('hide-nonbreaking');
		for (i = 0; i < links.length; i++)
			links [i].style.display = '';
		links = document.getElementsByClassName ('restore-nonbreaking');
		for (i = 0; i < links.length; i++)
			links [i].style.display = 'none';
	}
</script>");
							if (ac.SourceAssembly == ac.TargetAssembly) {
								file.WriteLine ("<h1>{0}.dll</h1>", ac.SourceAssembly);
							} else {
								file.WriteLine ("<h1>{0}.dll vs {1}.dll</h1>", ac.SourceAssembly, ac.TargetAssembly);
							}
							if (!State.IgnoreNonbreaking) {
								file.WriteLine ("<a href='javascript: hideNonBreakingChanges (); ' class='hide-nonbreaking'>Hide non-breaking changes</a>");
								file.WriteLine ("<a href='javascript: showNonBreakingChanges (); ' class='restore-nonbreaking' style='display: none;'>Show non-breaking changes</a>");
								file.WriteLine ("<br/>");
							}
							file.WriteLine ("<div data-is-topmost>");
							file.Write (diffHtml);
							file.WriteLine ("</div> <!-- end topmost div -->");
							file.WriteLine ("</div>");
						}
					}
				} else {
					State.Output = Console.Out;
					ac.Compare ();
				}
			}
			catch (Exception e) {
				Console.WriteLine (e);
				return 1;
			}
			return 0;
		}
	}
}
