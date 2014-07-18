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

namespace Xamarin.ApiDiff {

	public class ConstructorComparer : MemberComparer {

		public override string GroupName {
			get { return "constructors"; }
		}

		public override string ElementName {
			get { return "constructor"; }
		}

		public override bool Find (XElement e)
		{
			return (e.Attribute ("name").Value == Source.Attribute ("name").Value);
		}

		public override string GetDescription (XElement e)
		{
			var sb = new StringBuilder ();

			var attribs = e.Attribute ("attrib");
			if (attribs != null) {
				var attr = (MethodAttributes) Int32.Parse (attribs.Value);
				if ((attr & MethodAttributes.Public) != MethodAttributes.Public) {
					sb.Append ("protected ");
				} else {
					sb.Append ("public ");
				}

				if ((attr & MethodAttributes.Static) != 0) {
					sb.Append ("static ");
				} else if ((attr & MethodAttributes.Virtual) != 0) {
					if ((attr & MethodAttributes.VtableLayoutMask) == 0)
						sb.Append ("override ");
					else
						sb.Append ("virtual ");
				}
			}

			string name = e.GetAttribute ("name");

			var r = e.GetTypeName ("returntype");
			if (r != null) {
				// ctor dont' have a return type
				sb.Append (r).Append (' ');
			} else {
				// show the constructor as it would be defined in C#
				name = name.Replace (".ctor", State.Type);
			}

			// the XML file `name` does not contain parameter names, so we must process them ourselves
			// which gives us the opportunity to simplify type names
			sb.Append (name.Substring (0, name.IndexOf ('(')));

			var genericp = e.Element ("generic-parameters");
			if (genericp != null) {
				var list = new List<string> ();
				foreach (var p in genericp.Elements ("generic-parameter")) {
					list.Add (p.GetTypeName ("name"));
				}
				sb.Append ("&lt;").Append (String.Join (", ", list)).Append ("&gt;");
			}

			sb.Append (" (");
			var parameters = e.Element ("parameters");
			if (parameters != null) {
				var list = new List<string> ();
				foreach (var p in parameters.Elements ("parameter")) {
					list.Add (p.GetTypeName ("type") + " " + p.GetAttribute ("name"));
				}
				sb.Append (String.Join (", ", list));
			}
			sb.Append (");");

			return sb.ToString ();
		}
	}
}