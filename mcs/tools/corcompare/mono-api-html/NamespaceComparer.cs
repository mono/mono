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
using System.Linq;
using System.Xml.Linq;

namespace Xamarin.ApiDiff {

	public class NamespaceComparer : Comparer {

		ClassComparer comparer;

		public NamespaceComparer ()
		{
			comparer =  new ClassComparer ();
		}

		public void Compare (XElement source, XElement target)
		{
			var s = source.Element ("namespaces");
			var t = target.Element ("namespaces");
			if (XNode.DeepEquals (s, t))
				return;
			Compare (s.Elements ("namespace"), t.Elements ("namespace"));
		}

		public override void SetContext (XElement current)
		{
			State.Namespace = current.Attribute ("name").Value;
		}

		public override void Added (XElement target)
		{
			string name = target.Attribute ("name").Value;
			if (State.IgnoreNew.Any (re => re.IsMatch (name)))
				return;

			Output.WriteLine ("<h2>New Namespace {0}</h2>", name);
			Output.WriteLine ();
			// list all new types
			foreach (var addedType in target.Element ("classes").Elements ("class"))
				comparer.Added (addedType);
			Output.WriteLine ();
		}

		public override void Modified (XElement source, XElement target, ApiChanges differences)
		{
			var output = Output;
			State.Output = new StringWriter ();
			comparer.Compare (source, target);

			var s = Output.ToString ();
			State.Output = output;
			if (s.Length > 0) {
				Output.WriteLine ("<h2>Namespace {0}</h2>", target.Attribute ("name").Value);
				Output.WriteLine (s);
			}
		}

		public override void Removed (XElement source)
		{
			Output.WriteLine ("<h2>Removed Namespace {0}</h2>", source.Attribute ("name").Value);
			Output.WriteLine ();
			// list all removed types
			foreach (var removedType in source.Element ("classes").Elements ("class"))
				comparer.Removed (removedType);
			Output.WriteLine ();
		}
	}
}