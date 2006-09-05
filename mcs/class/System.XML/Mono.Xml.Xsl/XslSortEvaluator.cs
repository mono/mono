//
// XslSortEvaluator.cs
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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

//
// This class handles xsl:sort to have XslTransformProcessor involved in
// the evaluation of resulting nodeset. Beyond XPathExpression.AddSort(),
// it handles current() by having .
//
using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace Mono.Xml.Xsl
{
	class XslSortEvaluator
	{
		public XslSortEvaluator (XPathExpression select, Sort [] sorterTemplates)
		{
			this.select = select;
			this.sorterTemplates = sorterTemplates;
			PopulateConstantSorters ();
			sortRunner = new XPathSorters ();
		}

		XPathExpression select;
		Sort [] sorterTemplates;
		XPathSorter [] sorters;
		XPathSorters sortRunner;
		bool isSorterContextDependent;

		void PopulateConstantSorters ()
		{
			sorters = new XPathSorter [sorterTemplates.Length];
			for (int i = 0; i < sorterTemplates.Length; i++) {
				Sort sort = sorterTemplates [i];
				if (sort.IsContextDependent)
					isSorterContextDependent = true;
				else
					sorters [i] = sort.ToXPathSorter (null);
			}
		}

		public BaseIterator SortedSelect (XslTransformProcessor p)
		{
			if (isSorterContextDependent) {
				for (int i = 0; i < sorters.Length; i++)
					if (sorterTemplates [i].IsContextDependent)
						sorters [i] = sorterTemplates [i].ToXPathSorter (p);
			}
			BaseIterator iter = (BaseIterator) p.Select (select);
			p.PushNodeset (iter);
			p.PushForEachContext ();
			ArrayList list = new ArrayList (iter.Count);
			while (iter.MoveNext ()) {
				XPathSortElement item = new XPathSortElement ();
				item.Navigator = iter.Current.Clone ();
				item.Values = new object [sorters.Length];
				for (int i = 0; i < sorters.Length; i++)
					item.Values [i] = sorters [i].Evaluate (iter);
				list.Add (item);
			}
			p.PopForEachContext ();
			p.PopNodeset ();

			sortRunner.CopyFrom (sorters);
			return sortRunner.Sort (list, iter.NamespaceManager);
		}
	}
}
