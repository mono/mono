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

	class ClassComparer : Comparer {

		InterfaceComparer icomparer;
		ConstructorComparer ccomparer;
		FieldComparer fcomparer;
		PropertyComparer pcomparer;
		EventComparer ecomparer;
		MethodComparer mcomparer;
		ClassComparer kcomparer;

		public ClassComparer (State state)
			: base (state)
		{
			icomparer = new InterfaceComparer (state);
			ccomparer = new ConstructorComparer (state);
			fcomparer = new FieldComparer (state);
			pcomparer = new PropertyComparer (state);
			ecomparer = new EventComparer (state);
			mcomparer = new MethodComparer (state);
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

		public override void Added (XElement target, bool wasParentAdded)
		{
			var addedDescription  = $"{State.Namespace}.{State.Type}: Added type";
			State.LogDebugMessage ($"Possible -n value: {addedDescription}");
			if (State.IgnoreNew.Any (re => re.IsMatch (addedDescription)))
				return;

			Formatter.BeginTypeAddition (Output);
			State.Indent = 0;
			AddedInner (target);
			Formatter.EndTypeAddition (Output);
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

			State.Indent++;
			var t = target.Element ("constructors");
			if (t != null) {
				Indent ().WriteLine ("// constructors");
				foreach (var ctor in t.Elements ("constructor"))
					ccomparer.Added (ctor, true);
			}

			t = target.Element ("fields");
			if (t != null) {
				if (type != "enum")
					Indent ().WriteLine ("// fields");
				else
					SetContext (target);
				foreach (var field in t.Elements ("field"))
					fcomparer.Added (field, true);
			}

			t = target.Element ("properties");
			if (t != null) {
				Indent ().WriteLine ("// properties");
				foreach (var property in t.Elements ("property"))
					pcomparer.Added (property, true);
			}

			t = target.Element ("events");
			if (t != null) {
				Indent ().WriteLine ("// events");
				foreach (var evnt in t.Elements ("event"))
					ecomparer.Added (evnt, true);
			}

			t = target.Element ("methods");
			if (t != null) {
				Indent ().WriteLine ("// methods");
				foreach (var method in t.Elements ("method"))
					mcomparer.Added (method, true);
			}

			t = target.Element ("classes");
			if (t != null) {
				Output.WriteLine ();
				Indent ().WriteLine ("// inner types");
				State.Parent = State.Type;
				kcomparer = new NestedClassComparer (State);
				foreach (var inner in t.Elements ("class"))
					kcomparer.AddedInner (inner);
				State.Type = State.Parent;
			}
			State.Indent--;
			Indent ().WriteLine ("}");
		}

		//HACK: we don't have hierarchy information here so just check some basic heuristics for now
		bool IsBaseChangeCompatible (string source, string target)
		{
			if (source == "System.Object")
				return true;
			if (source == "System.Exception" && target.EndsWith ("Exception", StringComparison.Ordinal))
				return true;
			if (source == "System.EventArgs" && target.EndsWith ("EventArgs", StringComparison.Ordinal))
				return true;
			if (source == "System.Runtime.InteropServices.SafeHandle" && target.StartsWith ("Microsoft.Win32.SafeHandles.SafeHandle", StringComparison.Ordinal))
				return true;
			return false;
		}

		public override void Modified (XElement source, XElement target, ApiChanges diff)
		{
			// hack - there could be changes that we're not monitoring (e.g. attributes properties)
			var output = Output;
			State.Output = new StringWriter ();

			var sb = source.GetAttribute ("base");
			var tb = target.GetAttribute ("base");
			var rm = $"{State.Namespace}.{State.Type}: Modified base type: '{sb}' to '{tb}'";
			State.LogDebugMessage ($"Possible -r value: {rm}");
			if (sb != tb &&
					!State.IgnoreRemoved.Any (re => re.IsMatch (rm)) &&
					!(State.IgnoreNonbreaking && IsBaseChangeCompatible (sb, tb))) {
				Formatter.BeginMemberModification (Output, "Modified base type");
				var apichange = new ApiChange ($"{State.Namespace}.{State.Type}", State).AppendModified (sb, tb, true);
				Formatter.Diff (Output, apichange);
				Formatter.EndMemberModification (Output);
			}

			ccomparer.Compare (source, target);
			icomparer.Compare (source, target);
			fcomparer.Compare (source, target);
			pcomparer.Compare (source, target);
			ecomparer.Compare (source, target);
			mcomparer.Compare (source, target);

			var si = source.Element ("classes");
			if (si != null) {
				var ti = target.Element ("classes");
				kcomparer = new NestedClassComparer (State);
				State.Parent = State.Type;
				kcomparer.Compare (si.Elements ("class"), ti == null ? null : ti.Elements ("class"));
				State.Type = State.Parent;
			}

			var s = (Output as StringWriter).ToString ();
			State.Output = output;
			if (s.Length > 0) {
				SetContext (target);
				Formatter.BeginTypeModification (Output);
				Output.WriteLine (s);
				Formatter.EndTypeModification (Output);
			}
		}

		public override void Removed (XElement source)
		{
			if (source.Elements ("attributes").SelectMany (a => a.Elements ("attribute")).Any (c => c.Attribute ("name")?.Value == "System.ObsoleteAttribute"))
				return;

			string name = State.Namespace + "." + State.Type;

			var memberDescription = $"{name}: Removed type";
			State.LogDebugMessage ($"Possible -r value: {memberDescription}");
			if (State.IgnoreRemoved.Any (re => re.IsMatch (name)))
				return;

			Formatter.BeginTypeRemoval (Output);
			Formatter.EndTypeRemoval (Output);
		}

		public virtual string GetTypeName (XElement type)
		{
			return type.GetAttribute ("name");
		}
	}

	class NestedClassComparer : ClassComparer {

		public NestedClassComparer (State state)
			: base (state)
		{
		}

		public override void SetContext (XElement current)
		{
			State.Type = State.Parent + "." + current.GetAttribute ("name");
			State.BaseType = current.GetAttribute ("base");
		}

		public override string GetTypeName (XElement type)
		{
			return State.Parent + "." + base.GetTypeName (type);
		}
	}
}