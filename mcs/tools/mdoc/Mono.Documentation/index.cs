//
// index.cs
// Index maker (both normal and Lucene-based)
//
// Authors:
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
//
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//
using System;
using System.Collections.Generic;

using Mono.Options;
using Monodoc;

namespace Mono.Documentation {

	class MDocIndex : MDocCommand {
		public override void Run (IEnumerable<string> args)
		{
			string rootPath = null;

			var options = new OptionSet () {
				{ "r=|root=", "Specify which documentation root to use. Default is $libdir/monodoc", v => rootPath = v },
			};

			var extra = Parse (options, args, "index", 
			                   "[OPTIONS]+ ACTION",
			                   "Create Monodoc indexes depending on ACTION. Possible values are \"tree\" or \"search\" for, respectively, mdoc tree and lucene search");
			if (extra == null)
				return;

			var root = string.IsNullOrEmpty (rootPath) ? RootTree.LoadTree () : RootTree.LoadTree (rootPath);

			foreach (var action in extra) {
				switch (action) {
				case "tree":
					root.GenerateIndex ();
					break;
				case "search":
					root.GenerateSearchIndex ();
					break;
				}
			}
		}
	}
}
