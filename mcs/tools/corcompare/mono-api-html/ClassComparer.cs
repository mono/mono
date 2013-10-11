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

namespace Xamarin.ApiDiff {

	public class ClassComparer : Comparer {

		InterfaceComparer icomparer;
		ConstructorComparer ccomparer;
		FieldComparer fcomparer;
		PropertyComparer pcomparer;
		EventComparer ecomparer;
		MethodComparer mcomparer;
		ClassComparer kcomparer;

		public ClassComparer ()
		{
			icomparer = new InterfaceComparer ();
			ccomparer = new ConstructorComparer ();
			fcomparer = new FieldComparer ();
			pcomparer = new PropertyComparer ();
			ecomparer = new EventComparer ();
			mcomparer = new MethodComparer ();
		}

		public override void SetContext (XElement current)
		{
			State.Type = current.GetAttribute ("name");
			State.BaseType = current.GetAttribute ("base");
		}

		public void Compare (XElement source, XElement target)
		{
			var s = source.Element ("classes");
			var t = target.Element ("classes");
			if (XNode.DeepEquals (s, t))
				return;
			Compare (s.Elements ("class"), t.Elements ("class"));
		}

		public override void Added (XElement target)
		{
			Output.WriteLine ("<h3>New Type {0}.{1}</h3>", State.Namespace, target.Attribute ("name").Value);
			Output.WriteLine ("<pre>");
			State.Indent = 0;
			AddedInner (target);
			Output.WriteLine ("</pre>");
		}

		public void AddedInner (XElement target)
		{
			SetContext (target);
			if (target.IsTrue ("serializable"))
				Indent ().WriteLine ("[Serializable]");

			var type = target.Attribute ("type").Value;

			if (type == "enum") {
				// check if [Flags] is present
				var cattrs = target.Element ("attributes");
				if (cattrs != null) {
					foreach (var ca in cattrs.Elements ("attribute")) {
						if (ca.GetAttribute ("name") == "System.FlagsAttribute") {
							Indent ().WriteLine ("[Flags]");
							break;
						}
					}
				}
			}

			Indent ().Write ("public");

			if (type != "enum") {
				bool seal = target.IsTrue ("sealed");
				bool abst = target.IsTrue ("abstract");
				if (seal && abst)
					Output.Write (" static");
				else if (seal && type != "struct")
					Output.Write (" sealed");
				else if (abst && type != "interface")
					Output.Write (" abstract");
			}

			Output.Write (' ');
			Output.Write (type);
			Output.Write (' ');
			Output.Write (target.GetAttribute ("name"));

			var baseclass = target.GetAttribute ("base");
			if ((type != "enum") && (type != "struct")) {
				if (baseclass != null) {
					if (baseclass == "System.Object") {
						// while true we do not need to be reminded every time...
						baseclass = null;
					} else {
						Output.Write (" : ");
						Output.Write (baseclass);
					}
				}
			}

			// interfaces on enums are "standard" not user provided - so we do not want to show them
			if (type != "enum") {
				var i = target.Element ("interfaces");
				if (i != null) {
					var interfaces = new List<string> ();
					foreach (var iface in i.Elements ("interface"))
						interfaces.Add (icomparer.GetDescription (iface));
					Output.Write ((baseclass == null) ? " : " : ", ");
					Output.Write (String.Join (", ", interfaces));
				}
			}

			Output.WriteLine (" {");

			var t = target.Element ("constructors");
			if (t != null) {
				Indent ().WriteLine ("\t// constructors");
				foreach (var ctor in t.Elements ("constructor"))
					ccomparer.Added (ctor);
			}

			t = target.Element ("fields");
			if (t != null) {
				if (type != "enum")
					Indent ().WriteLine ("\t// fields");
				else
					SetContext (target);
				foreach (var field in t.Elements ("field"))
					fcomparer.Added (field);
			}

			t = target.Element ("properties");
			if (t != null) {
				Indent ().WriteLine ("\t// properties");
				foreach (var property in t.Elements ("property"))
					pcomparer.Added (property);
			}

			t = target.Element ("events");
			if (t != null) {
				Indent ().WriteLine ("\t// events");
				foreach (var evnt in t.Elements ("event"))
					ecomparer.Added (evnt);
			}

			t = target.Element ("methods");
			if (t != null) {
				Indent ().WriteLine ("\t// methods");
				foreach (var method in t.Elements ("method"))
					mcomparer.Added (method);
			}

			t = target.Element ("classes");
			if (t != null) {
				Output.WriteLine ();
				Indent ().WriteLine ("\t// inner types");
				kcomparer = new NestedClassComparer ();
				State.Indent++;
				foreach (var inner in t.Elements ("class"))
					kcomparer.AddedInner (inner);
				State.Indent--;
			}
			Indent ().WriteLine ("}");
		}

		public override void Modified (XElement source, XElement target)
		{
			// hack - there could be changes that we're not monitoring (e.g. attributes properties)
			var output = Output;
			State.Output = new StringWriter ();

			ccomparer.Compare (source, target);
			icomparer.Compare (source, target);
			fcomparer.Compare (source, target);
			pcomparer.Compare (source, target);
			ecomparer.Compare (source, target);
			mcomparer.Compare (source, target);

			var si = source.Element ("classes");
			if (si != null) {
				var ti = target.Element ("classes");
				kcomparer = new NestedClassComparer ();
				kcomparer.Compare (si.Elements ("class"), ti == null ? null : ti.Elements ("class"));
			}

			var s = (Output as StringWriter).ToString ();
			State.Output = output;
			if (s.Length > 0) {
				Output.WriteLine ("<h3>Type Changed: {0}.{1}</h3>", State.Namespace, GetTypeName (target));
				Output.WriteLine (s);
			}
		}

		public override void Removed (XElement source)
		{
			Output.WriteLine ("<h3>Removed Type {0}.{1}", State.Namespace, GetTypeName (source));
		}

		public virtual string GetTypeName (XElement type)
		{
			return type.GetAttribute ("name");
		}
	}

	public class NestedClassComparer : ClassComparer {

		public override void SetContext (XElement current)
		{
		}

		public override string GetTypeName (XElement type)
		{
			return State.Type + "." + base.GetTypeName (type);
		}
	}
}