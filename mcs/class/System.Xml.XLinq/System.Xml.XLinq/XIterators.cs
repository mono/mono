#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using XPI = System.Xml.XLinq.XProcessingInstruction;


namespace System.Xml.XLinq
{
	// Iterators
	internal class XFilterIterator <T> : IEnumerable <T>
	{
		IEnumerable <object> source;
		XName name;

		public XFilterIterator (IEnumerable <object> source, XName name)
		{
			this.source = source;
			this.name = name;
		}

		public IEnumerator <T> GetEnumerator ()
		{
			foreach (object o in source) {
				if (! (o is T))
					continue;
				if (name != null) {
					if (o is XElement) {
						if (((XElement) o).Name != name)
							continue;
					}
					else if (o is XAttribute) {
						if (((XAttribute) o).Name != name)
							continue;
					}
				}
				yield return (T) o;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}

	internal class XDescendantIterator <T> : IEnumerable <T>
	{
		IEnumerable <T> source;

		public XDescendantIterator (IEnumerable <object> source)
		{
			this.source = new XFilterIterator <T> (source, null);
		}

		public IEnumerator <T> GetEnumerator ()
		{
			foreach (T t1 in source) {
				yield return t1;
				XContainer xc = t1 as XContainer;
				if (xc == null || xc.FirstChild == null)
					continue;
				foreach (T t2 in new XDescendantIterator <T> (xc.Content ()))
					yield return t2;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}

#if !LIST_BASED
	internal class XChildrenIterator : IEnumerable <object>
	{
		XContainer source;
		XNode n;

		public XChildrenIterator (XContainer source)
		{
			this.source = source;
		}

		public IEnumerator <object> GetEnumerator ()
		{
			if (n == null) {
				n = source.FirstChild;
				if (n == null)
					yield break;
			}
			do {
				yield return n;
				n = n.NextSibling;
			} while (n != source.LastChild);
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
#endif
}

#endif
