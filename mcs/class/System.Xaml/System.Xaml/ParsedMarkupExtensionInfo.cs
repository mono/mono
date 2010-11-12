//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Xml;
using System.Xaml.Schema;

namespace System.Xaml
{
	internal class ParsedMarkupExtensionInfo
	{
		Dictionary<XamlMember,object> args = new Dictionary<XamlMember,object> ();
		public Dictionary<XamlMember,object> Arguments {
			get { return args; }
		}
	
		public XamlType Type { get; set; }

		public static ParsedMarkupExtensionInfo Parse (string raw, IXamlNamespaceResolver nsResolver, XamlSchemaContext sctx)
		{
			if (raw == null)
				throw new ArgumentNullException ("raw");
			if (raw.Length == 0 || raw [0] != '{')
				throw Error ("Invalid markup extension attribute. It should begin with '{{', but was {0}", raw);
			var ret = new ParsedMarkupExtensionInfo ();
			int idx = raw.IndexOf ('}');
			if (idx < 0)
				throw Error ("Expected '}}' in the markup extension attribute: '{0}'", raw);
			raw = raw.Substring (1, idx - 1);
			idx = raw.IndexOf (' ');
			string name = idx < 0 ? raw : raw.Substring (0, idx);

			XamlTypeName xtn;
			if (!XamlTypeName.TryParse (name, nsResolver, out xtn))
				throw Error ("Failed to parse type name '{0}'", name);
			var xt = sctx.GetXamlType (xtn);
			ret.Type = xt;

			if (idx < 0)
				return ret;

			string [] vpairs = raw.Substring (idx + 1, raw.Length - idx - 1).Split (',');
			List<string> posPrms = null;
			foreach (string vpair in vpairs) {
				idx = vpair.IndexOf ('=');
				// FIXME: unescape string (e.g. comma)
				if (idx < 0) {
					if (posPrms == null) {
						posPrms = new List<string> ();
						ret.Arguments.Add (XamlLanguage.PositionalParameters, posPrms);
					}
					posPrms.Add (vpair.Trim ());
				} else {
					var key = vpair.Substring (0, idx).Trim ();
					// FIXME: is unknown member always isAttacheable = false?
					var xm = xt.GetMember (key) ?? new XamlMember (key, xt, false);
					ret.Arguments.Add (xm, vpair.Substring (idx + 1).Trim ());
				}
			}
			return ret;
		}
	
		static Exception Error (string format, params object [] args)
		{
			return new XamlParseException (String.Format (format, args));
		}
	}
}
