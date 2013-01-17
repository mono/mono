//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Xml.Linq
{
	public static class Extensions
	{
		public static IEnumerable<XElement> Ancestors<T> (this IEnumerable<T> source) where T : XNode
		{
			foreach (T item in source)
				for (XElement n = item.Parent as XElement; n != null; n = n.Parent as XElement)
					yield return n;
		}

		public static IEnumerable<XElement> Ancestors<T> (this IEnumerable<T> source, XName name) where T : XNode
		{
			foreach (T item in source)
				for (XElement n = item.Parent as XElement; n != null; n = n.Parent as XElement)
					if (n.Name == name)
						yield return n;
		}

		public static IEnumerable<XElement> AncestorsAndSelf (this IEnumerable<XElement> source)
		{
			foreach (XElement item in source)
				for (XElement n = item as XElement; n != null; n = n.Parent as XElement)
					yield return n;
		}

		public static IEnumerable<XElement> AncestorsAndSelf (this IEnumerable<XElement> source, XName name)
		{
			foreach (XElement item in source)
				for (XElement n = item as XElement; n != null; n = n.Parent as XElement)
					if (n.Name == name)
						yield return n;
		}

		public static IEnumerable <XAttribute> Attributes (this IEnumerable <XElement> source)
		{
			foreach (XElement item in source)
				foreach (XAttribute attr in item.Attributes ())
					yield return attr;
		}

		public static IEnumerable <XAttribute> Attributes (this IEnumerable <XElement> source, XName name)
		{
			foreach (XElement item in source)
				foreach (XAttribute attr in item.Attributes (name))
					yield return attr;
		}

		public static IEnumerable<XNode> DescendantNodes<T> (
			this IEnumerable<T> source) where T : XContainer
		{
			foreach (XContainer item in source)
				foreach (XNode n in item.DescendantNodes ())
					yield return n;
		}

		public static IEnumerable<XNode> DescendantNodesAndSelf (
			this IEnumerable<XElement> source)
		{
			foreach (XElement item in source)
				foreach (XNode n in item.DescendantNodesAndSelf ())
					yield return n;
		}

		public static IEnumerable<XElement> Descendants<T> (
			this IEnumerable<T> source) where T : XContainer
		{
			foreach (XContainer item in source)
				foreach (XElement n in item.Descendants ())
					yield return n;
		}

		public static IEnumerable<XElement> Descendants<T> (
			this IEnumerable<T> source, XName name) where T : XContainer
		{
			foreach (XContainer item in source)
				foreach (XElement n in item.Descendants (name))
					yield return n;
		}

		public static IEnumerable<XElement> DescendantsAndSelf (
			this IEnumerable<XElement> source)
		{
			foreach (XElement item in source)
				foreach (XElement n in item.DescendantsAndSelf ())
					yield return n;
		}

		public static IEnumerable<XElement> DescendantsAndSelf (
			this IEnumerable<XElement> source, XName name)
		{
			foreach (XElement item in source)
				foreach (XElement n in item.DescendantsAndSelf (name))
					yield return n;
		}

		public static IEnumerable<XElement> Elements<T> (
			this IEnumerable<T> source) where T : XContainer
		{
			foreach (XContainer item in source)
				foreach (XElement n in item.Elements ())
					yield return n;
		}

		public static IEnumerable<XElement> Elements<T> (
			this IEnumerable<T> source, XName name) where T : XContainer
		{
			foreach (XContainer item in source)
				foreach (XElement n in item.Elements (name))
					yield return n;
		}

		public static IEnumerable<T> InDocumentOrder<T> (
			this IEnumerable<T> source) where T : XNode
		{
			List<XNode> list = new List<XNode> ();
			foreach (XNode n in source)
				list.Add (n);
			list.Sort (XNode.DocumentOrderComparer);
			foreach (T n in list)
				yield return n;
		}

		public static IEnumerable<XNode> Nodes<T> (
			this IEnumerable<T> source) where T : XContainer
		{
			foreach (XContainer item in source)
				foreach (XNode n in item.Nodes ())
					yield return n;
		}

		public static void Remove (this IEnumerable<XAttribute> source)
		{
			foreach (XAttribute item in source)
				item.Remove ();
		}

		public static void Remove<T> (this IEnumerable<T> source) where T : XNode
		{
			var l = new List<T> (source);
			foreach (T item in l)
				item.Remove ();
		}
	}
}
