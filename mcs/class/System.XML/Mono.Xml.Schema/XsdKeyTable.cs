//
// Mono.Xml.Schema.XsdKeyTable.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml.Schema
{
	// Created per constraining element.
	public class XsdKeyTable
	{
		/*
		private ArrayList entries = new ArrayList ();
		private ArrayList finishedEntries = new ArrayList ();
		private XsdKeyEntry [] cachedEntries;
		private XsdKeyEntry [] cachedFinishedEntries;
		*/

		private XsdIdentitySelector selector;
		private XmlSchemaIdentityConstraint source;
		private XmlQualifiedName qname;
		private XmlQualifiedName refKeyName;

		public ArrayList Entries = new ArrayList ();
		public ArrayList FinishedEntries = new ArrayList ();

		public int StartDepth;
		public XsdKeyTable ReferencedKey;

		public XsdKeyTable (XmlSchemaIdentityConstraint source, XmlReader reader)
		{
			Reset (source, reader);
		}

		/*
		public XsdKeyEntry [] Entries {
			get {
				if (cachedEntries == null)
					cachedEntries = entries.ToArray (typeof (XsdKeyEntry)) as XsdKeyEntry [];
				return cachedEntries;
			}
		}

		public XsdKeyEntry [] FinishedEntries {
			get {
				if (cachedFinishedEntries == null)
					cachedFinishedEntries = finishedEntries.ToArray (typeof (XsdKeyEntry)) as XsdKeyEntry [];
				return cachedFinishedEntries;
			}
		}
		*/

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

		public void Reset (XmlSchemaIdentityConstraint source, XmlReader reader)
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
		public XsdIdentityPath SelectorMatches (ArrayList qnameStack, XmlReader reader)
		{
			foreach (XsdIdentityPath path in Selector.Paths) {
				// Only "." hits.
				if (reader.Depth == this.StartDepth) {
					if (path.OrderedSteps.Length == 0)
						return path;
					else
						continue;
				}
				// It does not hit as yet (too shallow to hit).
				if (reader.Depth - this.StartDepth < path.OrderedSteps.Length - 1)
					continue;

				int iter = path.OrderedSteps.Length;
				if (path.OrderedSteps [iter-1].IsAttribute)
					iter--;

				if (path.Descendants && reader.Depth < this.StartDepth + iter)
					continue;
				else if (!path.Descendants && reader.Depth != this.StartDepth + iter)
					continue;

				iter--;

				XsdIdentityStep step;
				for (int x = 0; x <= iter; x++, iter--) {
					step = path.OrderedSteps [iter - x];
					if (step.IsAnyName)
						continue;
					XmlQualifiedName qname = (XmlQualifiedName) qnameStack [qnameStack.Count - x - 1];
					if (step.NsName != null && qname.Namespace == step.NsName)
						continue;
					if (step.Name == qname.Name && step.Namespace == qname.Namespace)
						continue;
					else
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
