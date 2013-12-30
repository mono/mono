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
// Copyright 2013 Xamarin Inc. http://www.xamarin.com
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
	}

	class Program {

		public static int Main (string[] args)
		{
			if (args.Length < 2) {
				Console.WriteLine ("mono-api-html reference.xml assembly.xml [diff.html]");
				return 1;
			}

			try {
				string input = args [0];
				string output = args [1];
				var ac = new AssemblyComparer (input, output);
				if (args.Length > 2) {
					string diff = String.Empty;
					using (var writer = new StringWriter ()) {
						State.Output = writer;
						ac.Compare ();
						diff = State.Output.ToString ();
					}
					if (diff.Length > 0) {
						using (var file = new StreamWriter (args [2])) {
							if (ac.SourceAssembly == ac.TargetAssembly) {
								file.WriteLine ("<h1>{0}.dll</h1>", ac.SourceAssembly);
							} else {
								file.WriteLine ("<h1>{0}.dll vs {1}.dll</h1>", ac.SourceAssembly, ac.TargetAssembly);
							}
							file.Write (diff);
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