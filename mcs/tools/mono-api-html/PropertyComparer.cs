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
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Mono.ApiTools {

	class PropertyComparer : MemberComparer {

		public PropertyComparer (State state)
			: base (state)
		{
		}

		public override string GroupName {
			get { return "properties"; }
		}

		public override string ElementName {
			get { return "property"; }
		}

		public override bool Find (XElement e)
		{
			if (!base.Find (e))
				return false;
			// the same Item (indexer) property can have different parameters
			return e.GetAttribute ("params") == Source.GetAttribute ("params");
		}

		void GetAccessors (XElement element, out XElement getter, out XElement setter)
		{
			var methods = element.Element ("methods");

			getter = null;
			setter = null;

			if (methods == null)
				return;
				
			foreach (var m in methods.Elements ("method")) {
				var n = m.GetAttribute ("name");
				if (n.StartsWith ("get_", StringComparison.Ordinal)) {
					getter = m;
				} else if (n.StartsWith ("set_", StringComparison.Ordinal)) {
					setter = m;
				}
			}
		}

		MethodAttributes GetMethodAttributes (XElement getter, XElement setter)
		{
			if (getter == null)
				return setter.GetMethodAttributes ();
			else if (setter == null)
				return getter.GetMethodAttributes ();

			var gAttr = getter.GetMethodAttributes ();
			var sAttr = setter.GetMethodAttributes ();
			var g = gAttr & MethodAttributes.MemberAccessMask;
			var s = sAttr & MethodAttributes.MemberAccessMask;
			// Visibility is ordered numerically (higher value = more visible).
			// We want the most visible.
			var visibility = (MethodAttributes) System.Math.Max ((int) g, (int) s);
			// Do a bitwise or with the rest of the flags
			var g_no_visibility = gAttr & ~MethodAttributes.MemberAccessMask;
			var s_no_visibility = sAttr & ~MethodAttributes.MemberAccessMask;
			return g_no_visibility | s_no_visibility | visibility;
		}

		void RenderPropertyType (XElement source, XElement target, ApiChange change)
		{
			var srcType = source.GetTypeName ("ptype", State);
			var tgtType = target.GetTypeName ("ptype", State);

			if (srcType == tgtType) {
				change.Append (tgtType);
			} else {
				change.AppendModified (srcType, tgtType, true);
			}
			change.Append (" ");
		}

		void RenderAccessors (XElement srcGetter, XElement tgtGetter, XElement srcSetter, XElement tgtSetter, ApiChange change)
		{
			// FIXME: this doesn't render changes in the accessor visibility (a protected setter can become public for instance).
			change.Append (" {");
			if (tgtGetter != null) {
				if (srcGetter != null) {
					change.Append (" ").Append ("get;");
				} else {
					change.Append (" ").AppendAdded ("get;");
				}
			} else if (srcGetter != null) {
				change.Append (" ").AppendRemoved ("get;");
			}

			if (tgtSetter != null) {
				if (srcSetter != null) {
					change.Append (" ").Append ("set;");
				} else {
					change.Append (" ").AppendAdded ("set;");
				}
			} else if (srcSetter != null) {
				change.Append (" ").AppendRemoved ("set;");
			}

			change.Append (" }");

			// Ignore added property setters if asked to
			if (srcSetter == null && tgtSetter != null && State.IgnoreAddedPropertySetters && !change.Breaking) {
				change.AnyChange = false;
				change.HasIgnoredChanges = true;
			}
		}

		void RenderIndexers (List<XElement> srcIndexers, List<XElement> tgtIndexers, ApiChange change)
		{
			change.Append ("this [");
			for (int i = 0; i < srcIndexers.Count; i++) {
				var source = srcIndexers [i];
				var target = tgtIndexers [i];

				if (i > 0)
					change.Append (", ");

				var srcType = source.GetTypeName ("type", State);
				var tgtType = target.GetTypeName ("type", State);
				if (srcType == tgtType) {
					change.Append (tgtType);
				} else {
					change.AppendModified (srcType, tgtType, true);
				}
				change.Append (" ");

				var srcName = source.GetAttribute ("name");
				var tgtName = target.GetAttribute ("name");
				if (srcName == tgtName) {
					change.Append (tgtName);
				} else {
					change.AppendModified (srcName, tgtName, true);
				}
			}
			change.Append ("]");
		}

		public override bool Equals (XElement source, XElement target, ApiChanges changes)
		{
			if (base.Equals (source, target, changes))
				return true;

			XElement srcGetter, srcSetter;
			XElement tgtGetter, tgtSetter;
			GetAccessors (source, out srcGetter, out srcSetter);
			GetAccessors (target, out tgtGetter, out tgtSetter);

			List<XElement> srcIndexers = null;
			List<XElement> tgtIndexers = null;
			bool isIndexer = false;
			if (srcGetter != null) {
				srcIndexers = srcGetter.DescendantList ("parameters", "parameter");
				tgtIndexers = tgtGetter.DescendantList ("parameters", "parameter");
				isIndexer = srcIndexers != null && srcIndexers.Count > 0;
			}

			var change = new ApiChange (GetDescription (source), State);
			change.Header = "Modified " + GroupName;
			RenderMethodAttributes (GetMethodAttributes (srcGetter, srcSetter), GetMethodAttributes (tgtGetter, tgtSetter), change);
			RenderPropertyType (source, target, change);
			if (isIndexer) {
				RenderIndexers (srcIndexers, tgtIndexers, change);
			} else {
				RenderName (source, target, change);
			}
			RenderGenericParameters (source, target, change);
			RenderAccessors (srcGetter, tgtGetter, srcSetter, tgtSetter, change);

			changes.Add (source, target, change);

			return false;
		}

		void GetProperties (XElement e, out bool @virtual, out bool @override, out bool @static, out bool getter, out bool setter, out bool family)
		{
			@virtual = @override = @static = getter = setter = family = false;

			var methods = e.Element ("methods");
			if (methods != null) {
				foreach (var m in methods.Elements ("method")) {
					@virtual |= m.IsTrue ("virtual");
					@static |= m.IsTrue ("static");
					var n = m.GetAttribute ("name");
					getter |= n.StartsWith ("get_", StringComparison.Ordinal);
					setter |= n.StartsWith ("set_", StringComparison.Ordinal);
					var attribs = (MethodAttributes) Int32.Parse (m.GetAttribute ("attrib"));
					family = ((attribs & MethodAttributes.Public) != MethodAttributes.Public);
					@override |= (attribs & MethodAttributes.NewSlot) == 0;
				}
			}
		}

		public override string GetDescription (XElement e)
		{
			string name = e.Attribute ("name").Value;
			string ptype = e.GetTypeName ("ptype", State);

			bool virt = false;
			bool over = false;
			bool stat = false;
			bool getter = false;
			bool setter = false;
			bool family = false;
			GetProperties (e, out virt, out over, out stat, out getter, out setter, out family);

			var sb = new StringBuilder ();

			sb.Append (family ? "protected " : "public ");
			if (virt && !State.IgnoreVirtualChanges)
				sb.Append (over ? "override " : "virtual ");
			else if (stat)
				sb.Append ("static ");
			sb.Append (ptype).Append (' ').Append (name).Append (" { ");
			if (getter)
				sb.Append ("get; ");
			if (setter)
				sb.Append ("set; ");
			sb.Append ("}");

			return sb.ToString ();
		}
	}
}