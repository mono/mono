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
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Mono.ApiTools {

	class AssemblyComparer : Comparer {

		XDocument source;
		XDocument target;
		NamespaceComparer comparer;

		public AssemblyComparer (string sourceFile, string targetFile, State state)
			: this (XDocument.Load(sourceFile), XDocument.Load(targetFile), state)
		{
		}

		public AssemblyComparer (Stream sourceFile, Stream targetFile, State state)
			: this (XDocument.Load(sourceFile), XDocument.Load(targetFile), state)
		{
		}

		public AssemblyComparer (XDocument sourceFile, XDocument targetFile, State state)
			: base (state)
		{
			source = sourceFile;
			target = targetFile;
			comparer =  new NamespaceComparer (state);
		}

		public string SourceAssembly { get; private set; }
		public string TargetAssembly { get; private set; }

		public void Compare ()
		{
			Compare (source.Element ("assemblies").Elements ("assembly"), 
			         target.Element ("assemblies").Elements ("assembly"));
		}

		public override void SetContext (XElement current)
		{
			State.Assembly = current.GetAttribute ("name");
		}

		public override void Added (XElement target, bool wasParentAdded)
		{
			// one assembly per xml file
		}

		public override void Modified (XElement source, XElement target, ApiChanges diff)
		{
			SourceAssembly = source.GetAttribute ("name");
			TargetAssembly = target.GetAttribute ("name");

			var sb = source.GetAttribute ("version");
			var tb = target.GetAttribute ("version");
			if (sb != tb) {
				Output.WriteLine ("<h4>Assembly Version Changed: {0} vs {1}</h4>", tb, sb);
			}

			// ? custom attributes ?
			comparer.Compare (source, target);
		}

		public override void Removed (XElement source)
		{
			// one assembly per xml file
		}
	}
}