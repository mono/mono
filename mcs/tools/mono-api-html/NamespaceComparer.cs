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

namespace Mono.ApiTools {

	class NamespaceComparer : Comparer {

		ClassComparer comparer;

		public NamespaceComparer (State state)
			: base (state)
		{
			comparer =  new ClassComparer (state);
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

		public override void Added (XElement target, bool wasParentAdded)
		{
			var namespaceDescription  = $"{State.Namespace}: Added namespace";
			State.LogDebugMessage ($"Possible -n value: {namespaceDescription}");
			if (State.IgnoreNew.Any (re => re.IsMatch (namespaceDescription)))
				return;

			Formatter.BeginNamespace (Output, "New ");
			// list all new types
			foreach (var addedType in target.Element ("classes").Elements ("class")) {
				State.Type = addedType.Attribute ("name").Value;
				comparer.Added (addedType, true);
			}
			Formatter.EndNamespace (Output);
		}

		public override void Modified (XElement source, XElement target, ApiChanges differences)
		{
			var output = Output;
			State.Output = new StringWriter ();
			comparer.Compare (source, target);

			var s = Output.ToString ();
			State.Output = output;
			if (s.Length > 0) {
				var name = target.Attribute ("name").Value;
				Formatter.BeginNamespace (Output);
				Output.WriteLine (s);
				Formatter.EndNamespace (Output);
			}
		}

		public override void Removed (XElement source)
		{
			var name = source.Attribute ("name").Value;

			var namespaceDescription  = $"{name}: Removed namespace";
			State.LogDebugMessage ($"Possible -r value: {namespaceDescription}");
			if (State.IgnoreRemoved.Any (re => re.IsMatch (namespaceDescription)))
				return;

			Formatter.BeginNamespace (Output, "Removed ");
			Output.WriteLine ();
			// list all removed types
			foreach (var removedType in source.Element ("classes").Elements ("class")) {
				State.Type = comparer.GetTypeName (removedType);
				comparer.Removed (removedType);
			}
			Formatter.EndNamespace (Output);
		}
	}
}