//
// Mono.Xml.Schema.XsdKeyTable.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
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
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml.Schema
{
	internal class XsdKeyEntryCollection : CollectionBase
	{
		public void Add (XsdKeyEntry entry)
		{
			List.Add (entry);
		}

		public XsdKeyEntry this [int i] {
			get { return (XsdKeyEntry) List [i]; }
			set { List [i] = value; }
		}
	}

	// Created per constraining element.
	internal class XsdKeyTable
	{
		// FIXME: no need after #70419
		public readonly bool alwaysTrue = true;

		private XsdIdentitySelector selector;
		private XmlSchemaIdentityConstraint source;
		private XmlQualifiedName qname;
		private XmlQualifiedName refKeyName;

		public XsdKeyEntryCollection Entries =
			new XsdKeyEntryCollection ();
		public XsdKeyEntryCollection FinishedEntries =
			new XsdKeyEntryCollection ();

		public int StartDepth;
		public XsdKeyTable ReferencedKey;

		public XsdKeyTable (XmlSchemaIdentityConstraint source)
		{
			Reset (source);
		}

		public XmlQualifiedName QualifiedName {
			get { return qname; }
		}

		public XmlQualifiedName RefKeyName {
			get { return refKeyName; }
		}

		public XmlSchemaIdentityConstraint SourceSchemaIdentity {
			get { return source; }
		}

		public XsdIdentitySelector Selector {
			get { return selector; }
		}

		public void Reset (XmlSchemaIdentityConstraint source)
		{
			this.source = source;
			this.selector = source.CompiledSelector;
			this.qname = source.QualifiedName;
			XmlSchemaKeyref kr = source as XmlSchemaKeyref;
			if (kr != null)
				this.refKeyName = kr.Refer;
			StartDepth = 0;
		}

		// In this method, attributes are ignored.
		public XsdIdentityPath SelectorMatches (ArrayList qnameStack, int depth)
		{
			for (int i = 0; i < Selector.Paths.Length; i++) {
				XsdIdentityPath path = Selector.Paths [i];
				// Only "." hits.
				if (depth == this.StartDepth) {
					if (path.OrderedSteps.Length == 0)
						return path;
					else
						continue;
				}
				// It does not hit as yet (too shallow to hit).
				if (depth - this.StartDepth < path.OrderedSteps.Length - 1)
					continue;

				int iter = path.OrderedSteps.Length;
				if (path.OrderedSteps [iter-1].IsAttribute)
					iter--;

				if (path.Descendants && depth < this.StartDepth + iter)
					continue;
				else if (!path.Descendants && depth != this.StartDepth + iter)
					continue;

				iter--;

				XsdIdentityStep step;
				for (int x = 0; 0 <= iter; x++, iter--) {
					step = path.OrderedSteps [iter];
					if (step.IsAnyName)
						continue;
					XmlQualifiedName qname = (XmlQualifiedName) qnameStack [qnameStack.Count - x - 1];
					if (step.NsName != null && qname.Namespace == step.NsName)
						continue;
					if (step.Name == qname.Name && step.Namespace == qname.Namespace)
						continue;
					if (alwaysTrue)
						break;
				}
				if (iter >= 0)	// i.e. did not match against the path.
					continue;
				return path;
			}
			return null;
		}
	}
}
