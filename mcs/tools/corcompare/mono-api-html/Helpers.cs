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
using System.Xml.Linq;

namespace Xamarin.ApiDiff {

	public static class Helper {

		public static bool IsTrue (this XElement self, string name)
		{
			return (self.GetAttribute (name) == "true");
		}

		public static string GetAttribute (this XElement self, string name)
		{
			var n = self.Attribute (name);
			if (n == null)
				return null;
			return n.Value;
		}

		// null == no obsolete, String.Empty == no description
		public static string GetObsoleteMessage (this XElement self)
		{
			var cattrs = self.Element ("attributes");
			if (cattrs == null)
				return null;

			foreach (var ca in cattrs.Elements ("attribute")) {
				if (ca.GetAttribute ("name") != "System.ObsoleteAttribute")
					continue;
				var props = ca.Element ("properties");
				if (props == null)
					return String.Empty; // no description
				foreach (var p in props.Elements ("property")) {
					if (p.GetAttribute ("name") != "Message")
						continue;
					return p.GetAttribute ("value");
				}
			}
			return null;
		}

		// make it beautiful (.NET -> C#)
		public static string GetTypeName (this XElement self, string name)
		{
			string type = self.GetAttribute (name);
			if (type == null)
				return null;

			// inner types
			type = type.Replace ('+', '.');

			if (type.StartsWith ("System.Nullable`1[", StringComparison.Ordinal))
				return type.Substring (18, type.Length - 19) + "?";

			int pos = type.IndexOf ('`');
			if (pos >= 0)
				return type.Substring (0, pos) + "&lt;" + type.Substring (pos + 3).Replace ("]", "&gt;");

			switch (type) {
			case "System.String":
				return "string";
			case "System.Int32":
				return "int";
			case "System.UInt32":
				return "uint";
			case "System.Int64":
				return "long";
			case "System.UInt64":
				return "ulong";
			case "System.Void":
				return "void";
			case "System.Boolean":
				return "bool";
			case "System.Object":
				return "object";
			case "System.Single":
				return "float";
			case "System.Double":
				return "double";
			case "System.Byte":
				return "byte";
			case "System.SByte":
				return "sbyte";
			case "System.Int16":
				return "short";
			case "System.UInt16":
				return "ushort";
			default:
				if (type.StartsWith (State.Namespace, StringComparison.Ordinal))
					type = type.Substring (State.Namespace.Length + 1);
				return type;
			}
		}
	}
}