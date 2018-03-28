//
// xmldocdiff.cs
//
// Author:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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

#if !MOBILE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

public class XmlComparer
{
	public class ComparisonException : Exception
	{
		public ComparisonException (string message)
			: base (message)
		{
		}
	}

	static readonly bool debug = false;
	readonly string start;

	public XmlComparer (string startNode)
	{
		this.start = "/" + startNode;
	}

	public void Compare (string reference, string output)
	{
		XmlDocument doc1 = new XmlDocument ();
		doc1.Load (reference);
		XmlDocument doc2 = new XmlDocument ();
		doc2.Load (output);

		var memberList1 = doc1.SelectSingleNode (start);
		var memberList2 = doc2.SelectSingleNode (start);

		CompareNodes (memberList1, memberList2);
	}

	static bool CompareNodes (XmlNode reference, XmlNode output)
	{
		List<Tuple<string, XmlNode>> ref_nodes = new List<Tuple<string, XmlNode>> ();
		foreach (XmlNode node in reference.ChildNodes) {
			if (node.NodeType == XmlNodeType.Comment)
				continue;

			ref_nodes.Add (Tuple.Create (node.Name, node));
		}

		foreach (XmlNode node in output.ChildNodes) {
			if (node.NodeType == XmlNodeType.Comment)
				continue;

			Tuple<string, XmlNode> found = null;
			foreach (var entry in ref_nodes.Where (l => l.Item1 == node.Name)) {
				if (node.Attributes == null) {
					if (node.Attributes != entry.Item2.Attributes)
						continue;
				} else {
					List<XmlAttribute> attrs = node.Attributes.Cast<XmlAttribute> ().ToList ();
					XmlAttribute missing = null;
					foreach (XmlAttribute attr in entry.Item2.Attributes) {
						var match = attrs.Find (l => l.Name == attr.Name);
						if (match == null) {
							missing = attr;
							break;
						}

						if (match.Value == attr.Value)
							attrs.Remove (match);
					}

					if (missing != null || attrs.Count > 0) {
						continue;
					}
				}

				if (node.HasChildNodes != entry.Item2.HasChildNodes)
					continue;

				if (!node.HasChildNodes || CompareNodes (node, entry.Item2)) {
					found = entry;
					break;
				}
			}

			if (found == null) {
				Report ("Expected node: " + node.OuterXml);
				return false;
			}

			ref_nodes.Remove (found);
		}

		if (ref_nodes.Count > 0) {
			Report ("Unexpected node: " + ref_nodes[0].Item2.OuterXml);
			return false;
		}

		return true;
	}

	static void Report (string format, params object[] args)
	{
		if (debug)
			Console.WriteLine (format, args);
		else
			throw new ComparisonException (String.Format (format, args));
	}
}

#endif
