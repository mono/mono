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
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Xamarin.ApiDiff {

	public class PropertyComparer : MemberComparer {

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

		public override string GetDescription (XElement e)
		{
			string name = e.Attribute ("name").Value;
			string ptype = e.GetTypeName ("ptype");

			bool virt = false;
			bool over = false;
			bool stat = false;
			bool getter = false;
			bool setter = false;
			bool family = false;
			var methods = e.Element ("methods");
			if (methods != null) {
				foreach (var m in methods.Elements ("method")) {
					virt |= m.IsTrue ("virtual");
					stat |= m.IsTrue ("static");
					var n = m.GetAttribute ("name");
					getter |= n.StartsWith ("get_", StringComparison.Ordinal);
					setter |= n.StartsWith ("set_", StringComparison.Ordinal);
					var attribs = (MethodAttributes) Int32.Parse (m.GetAttribute ("attrib"));
					family = ((attribs & MethodAttributes.Public) != MethodAttributes.Public);
					over |= (attribs & MethodAttributes.VtableLayoutMask) == 0;
				}
			}

			var sb = new StringBuilder ();

			sb.Append (family ? "protected " : "public ");
			if (virt)
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