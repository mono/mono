//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
//

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
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Mono.MonoConfig
{
	public class Section
	{
		List <Section> children;
		string name;
		string defaultBlockName;
		bool attachPoint;
		
		public string Name {
			get { return name; }
		}

		public string DefaultBlockName {
			get {
				if (String.IsNullOrEmpty (defaultBlockName))
					return Name;
				return defaultBlockName;
			}
		}
		
		public List <Section> Children {
			get {
				if (children == null)
					children = new List <Section> ();
				return children;
			}
		}

		public bool AttachPoint {
			get { return attachPoint; }
		}
		
		public Section () : this (null)
		{
		}
		
		public Section (XPathNavigator nav)
		{
			if (nav != null) {
				name = Helpers.GetRequiredNonEmptyAttribute (nav, "name");

				string val = Helpers.GetOptionalAttribute (nav, "attachPoint");
				if (!String.IsNullOrEmpty (val))
					attachPoint = true;

				val = Helpers.GetOptionalAttribute (nav, "defaultBlockName");
				if (!String.IsNullOrEmpty (val))
					defaultBlockName = val;
			}
		}
	}
}
