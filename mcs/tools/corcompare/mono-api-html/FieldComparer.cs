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

	public class FieldComparer : MemberComparer {

		public override string GroupName {
			get { return "fields"; }
		}

		public override string ElementName {
			get { return "field"; }
		}

		public override string GetDescription (XElement e)
		{
			var sb = GetObsoleteMessage (e);
			bool obsolete = sb.Length > 0;

			string name = e.GetAttribute ("name");
			string value = e.GetAttribute ("value");

			if (State.BaseType == "System.Enum") {
				sb.Append (name).Append (" = ").Append (value).Append (',');
			} else {
				var attribs = e.Attribute ("attrib");
				if (attribs != null) {
					var attr = (FieldAttributes)Int32.Parse (attribs.Value);
					if ((attr & FieldAttributes.Public) != FieldAttributes.Public) {
						sb.Append ("protected ");
					} else {
						sb.Append ("public ");
					}

					if ((attr & FieldAttributes.Literal) != 0)
						sb.Append ("const ");
				}

				string ftype = e.GetTypeName ("fieldtype");
				sb.Append (ftype).Append (' ');
				sb.Append (name);
				if (ftype == "string")
					sb.Append (" = \"").Append (e.Attribute ("value").Value).Append ("\";");
			}

			if (obsolete)
				sb.AppendLine (); // more readable output
			return sb.ToString ();
		}

		public override void BeforeAdding ()
		{
			if (State.BaseType == "System.Enum")
				Output.WriteLine ("<p>Added values:</p><pre>");
			else
				base.BeforeAdding ();
		}

		public override void BeforeRemoving ()
		{
			if (State.BaseType == "System.Enum")
				Output.WriteLine ("<p>Removed values:</p><pre>");
			else
				base.BeforeRemoving ();
		}
	}
}