//
// Commons.Xml.Relaxng.RngGrammar.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Xml;
using Commons.Xml.Relaxng.Derivative;
//using System.Xml.Serialization;

namespace Commons.Xml.Relaxng
{
//	[XmlRoot (ElementName="grammar", Namespace="http://relaxng.org/ns/structure/1.0")]
	public class RngGrammar : RngPattern
	{
		// field
		public static string NamespaceURI =
			"http://relaxng.org/ns/structure/1.0";

		// object model fields
		ArrayList starts = new ArrayList ();
		ArrayList defs = new ArrayList ();
		ArrayList includes = new ArrayList ();
		ArrayList divs = new ArrayList ();

		// compiled fields.
		RdpPattern startPattern;

		// compile cache fields.
		Hashtable assembledDefs = new Hashtable ();
		RngPattern assembledStart;
		RdpPattern compiledStart;

		Hashtable includedUris = new Hashtable ();
		RngGrammar parentGrammar;
		ArrayList includedGrammars;
		Hashtable refPatterns = new Hashtable (); // key = RdpPattern of assembledDefs

		// only for checkRecursion()
		Hashtable checkedDefs = new Hashtable ();

		// this should be checked after its compilation finished to complete
		// missing-at-the-tracking patterns (especially of parent grammars).
		// key = RdpPattern, value = ArrayList of unresolvedPatterns.
		Hashtable unresolvedPatternsTable = new Hashtable ();
		ArrayList unresolvedPatterns = new ArrayList ();

		// contents key = RdpElement and value = name of the parent define.
		private Hashtable ElementDefMap = new Hashtable ();

		// Public

		public RngGrammar ()
		{
		}

//		[XmlIgnore]
		public override RngPatternType PatternType {
			get { return RngPatternType.Grammar; }
		}

//		[XmlElement("start")]
		public IList Starts {
			get { return starts; }
		}

//		[XmlElement("define")]
		public IList Defines {
			get { return defs; }
		}

//		[XmlElement("include")]
		public IList Includes {
			get { return includes; }
		}

//		[XmlElement("div")]
		public IList Divs {
			get { return divs; }
		}

		internal Hashtable IncludedUris {
			get { return includedUris; }
		}

		// Internal

		internal void CheckIncludeRecursion (string href)
		{
			if (this.includedUris [href] != null)
				throw new RngException ("Include recursion found. href: " + href);
			if (parentGrammar != null)
				parentGrammar.CheckIncludeRecursion (href);
		}

		// Compile from this simplified syntax to derivatives.
		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			parentGrammar = grammar;

			// First, process includes and divs. Up to RELAX NG 4.16.
			foreach (RngDiv div in divs)
				div.Compile (this);
			foreach (RngInclude inc in includes)
				inc.Compile (this);

			// Assemble combine into the same name defines/start.
			// see RELAX NG 4.17.
			assembleCombine ();

			compiledStart = assembledStart.Compile (this);

			// Assemble all define components into top grammar and
			// return start patterns for descendant grammars.
			// see RELAX NG 4.18.
			collectGrammars ();
			if (parentGrammar != null)
				return assembledStart.Compile (this);

			// 4.19 (a) remove non-reachable defines
			// 4.19 (b) check illegal recursion
			checkRecursion (compiledStart, 0);
			// 4.19 (c) expandRef
			startPattern = compiledStart.ExpandRef (assembledDefs);
			// 4.20,21 reduce notAllowed and empty.
			bool b;
			do {
				b = false;
				startPattern = startPattern.ReduceEmptyAndNotAllowed (ref b, new Hashtable ());
			} while (b);

			Hashtable ht = new Hashtable ();
			startPattern.setInternTable (ht);
			RdpNotAllowed.Instance.setInternTable (ht);
			RdpEmpty.Instance.setInternTable (ht);
			RdpText.Instance.setInternTable (ht);

			// return its start pattern.
			IsCompiled = true;
			return startPattern;
		}

		private void checkRecursion (RdpPattern p, int depth)
		{

			RdpAbstractBinary binary = p as RdpAbstractBinary;
			if (binary != null) {
				// choice, interleave, group
				checkRecursion (binary.LValue, depth);
				checkRecursion (binary.RValue, depth);
				return;
			}
			RdpAbstractSingleContent single = p as RdpAbstractSingleContent;
			if (single != null) {
				checkRecursion (single.Child, depth);
				return;
			}

			switch (p.PatternType) {
			case RngPatternType.Ref:
				// get checkRecursionDepth from table.
				int checkRecursionDepth = -1;
				object checkedDepth = checkedDefs [p];
				if (checkedDepth != null)
					checkRecursionDepth = (int) checkedDepth;
				// get refPattern
				RdpUnresolvedRef pref = p as RdpUnresolvedRef;
				RngGrammar target = pref.IsParentRef ? parentGrammar : this;
				RdpPattern refPattern = target.assembledDefs [pref.Name] as RdpPattern;
				if (refPattern == null)
					throw new RngException ("No matching define found for " + pref.Name);

				if (checkRecursionDepth == -1) {
					checkedDefs [p] = depth;
					checkRecursion (refPattern, depth);
					checkedDefs [p] = -2;
				}
				else if (depth == checkRecursionDepth)
					throw new RngException (String.Format ("Detected illegal recursion. Ref name is {0}.", pref.Name));

				break;

			case RngPatternType.Attribute:
				checkRecursion (((RdpAttribute) p).Children, depth);
				break;

			case RngPatternType.DataExcept:
				checkRecursion (((RdpDataExcept) p).Except, depth);
				break;

			case RngPatternType.Element:
				RdpElement el = p as RdpElement;
				checkRecursion (el.Children, depth + 1);	// +1
				break;
			case RngPatternType.List:
				checkRecursion (((RdpList) p).Child, depth);
				break;
			}
		}

		private void completeReference ()
		{
			foreach (RdpUnresolvedRef pref in this.unresolvedPatterns) {
				RdpPattern defP = assembledDefs [pref.Name] as RdpPattern;
				ArrayList al = refPatterns [defP] as ArrayList;
				if (al == null) {
					al = new ArrayList ();
					refPatterns [defP] = al;
				}
				al.Add (pref);
			}
		}

		private void collectGrammars ()
		{
			// collect ref and parentRef for each define.
			includedGrammars = (parentGrammar != null) ?
				parentGrammar.includedGrammars : new ArrayList ();

			// FIXME: This should be assembledStart.
			checkReferences (compiledStart);
			completeReference ();

			Hashtable ht = assembledDefs.Clone () as Hashtable;
			foreach (string name in ht.Keys) {
				RdpPattern p = (RdpPattern) assembledDefs [name];
				this.unresolvedPatternsTable [p] = unresolvedPatterns;
				checkReferences (p);
				completeReference ();
			}

			// If it is child of any other pattern:
			// * Remove all definitions under descendant grammars,
			//   replacing ref names, and
			// * Then return its start pattern.
			if (parentGrammar != null) {
				// TODO: reachable check is incomplete.
				foreach (string name in assembledDefs.Keys) {
					ArrayList al = 
						refPatterns [assembledDefs [name] ] as ArrayList;
					if (al == null)
						continue; // Not referenced.

					if (parentGrammar.assembledDefs [name] == null)
						parentGrammar.assembledDefs [name] =
							this.assembledDefs [name];
					else
						replaceDefines (name, al);
				}
			}
		}

		private void replaceDefines (string name, ArrayList al)
		{
			int idx = 0;
			while (true) {
				string newName = "define" + idx;
				if (parentGrammar.assembledDefs [newName] == null) {
					parentGrammar.assembledDefs [newName] = 
						assembledDefs [name];
					foreach (RdpUnresolvedRef pref in al)
						pref.Name = newName;
					break;
				}
				idx++;
			}
		}

		// remove ref and parentRef.
		// add new defines for each elements.
		private void checkReferences (RdpPattern p)
		{
			RdpAbstractBinary binary = p as RdpAbstractBinary;
			if (binary != null) {
				// choice, interleave, group
				checkReferences (binary.LValue);
				checkReferences (binary.RValue);
				return;
			}
			RdpAbstractSingleContent single = p as RdpAbstractSingleContent;
			if (single != null) {
				checkReferences (single.Child);
				return;
			}

			switch (p.PatternType) {
			case RngPatternType.Ref:
				RdpUnresolvedRef pref = p as RdpUnresolvedRef;
				RngGrammar target = pref.IsParentRef ? parentGrammar : this;
				RdpPattern defP = target.assembledDefs [pref.Name] as RdpPattern;
				if (defP == null)
					target.unresolvedPatterns.Add (p);
				else {
					ArrayList al = target.refPatterns [defP] as ArrayList;
					if (al == null) {
						al = new ArrayList ();
						target.refPatterns [defP] = al;
					}
					al.Add (p);
				}
				break;

			case RngPatternType.Attribute:
				checkReferences (((RdpAttribute) p).Children);
				break;

			case RngPatternType.DataExcept:
				checkReferences (((RdpDataExcept) p).Except);
				break;

			case RngPatternType.Element:
				RdpElement el = p as RdpElement;
				checkReferences (el.Children);
				string name = ElementDefMap [el] as string;
				if (name == null) {
					// add new define
					int idx = 0;
					string newName = "element0";
					if (el.NameClass is RdpName)
						newName = ((RdpName) el.NameClass).LocalName;
					while (true) {
                                                if (assembledDefs [newName] == null) {
							assembledDefs.Add (newName, el.Children);
							break;
						}
						newName = "element" + ++idx;
					}
				}
				// Even though the element is replaced with ref,
				// derivative of ref is RdpElement in fact...
				break;

			case RngPatternType.List:
				checkReferences (((RdpList) p).Child);
				break;

			case RngPatternType.Empty:
			case RngPatternType.NotAllowed:
			case RngPatternType.Text:
			case RngPatternType.Value:
				break;

			//case RngPatternType.ExternalRef:
			//case RngPatternType.Include:
			// Mixed, Optional, ZeroOrMore are already removed.
			// Choice, Group, Interleave, OneOrMore are already proceeded.
			}
		}

#if false
		// remove ref and parentRef.
		// add new defines for each elements.
		private void checkReferences (RngPattern p)
		{
			RngBinaryContentPattern binary = 
				p as RngBinaryContentPattern;
			if (binary != null) {
				// choice, interleave, group
				foreach (RngPattern cp in binary.Patterns)
					checkReferences (cp);
				return;
			}

			RngSingleContentPattern single =
				p as RngSingleContentPattern;
			if (single != null) {
				// list, element, oneOrMore, zeroOrMore, optional, mixed
				foreach (RngPattern cp in single.Patterns)
					checkReferences (cp);
				return;
			}

			switch (p.PatternType) {
			case RngPatternType.Ref:
				RdpUnresolvedRef pref = p as RdpUnresolvedRef;
				RngGrammar target = pref.IsParentRef ? parentGrammar : this;
				RdpPattern defP = target.assembledDefs [pref.Name] as RdpPattern;
				if (defP == null)
					throw new RngException ("No matching define found for " + pref.Name);
				ArrayList al = target.refPatterns [defP] as ArrayList;
				if (al == null) {
					al = new ArrayList ();
					target.refPatterns [defP] = al;
				}
				al.Add (p);
				break;

			case RngPatternType.Attribute:
				checkReferences (((RngAttribute) p).Pattern);
				break;

			case RngPatternType.DataExcept:
				foreach (RngPattern cp in ((RngData) p).Except.Patterns)
					checkReferences (cp);
				break;

			case RngPatternType.Element:
				RngElement el = p as RngElement;
				checkReferences (el.Children);
				string name = ElementDefMap [el] as string;
				if (name == null) {
					// add new define
					int idx = 0;
					string newName = "element0";
					if (el.NameClass is RngName)
						newName = ((RngName) el.NameClass).LocalName;
					while (true) {
                                                if (assembledDefs [newName] == null) {
							assembledDefs.Add (newName, el.Children);
							break;
						}
						newName = "element" + ++idx;
					}
				}
				// Even though the element is replaced with ref,
				// derivative of ref is RdpElement in fact...
				break;

			case RngPatternType.List:
				checkReferences (((RdpList) p).Child);
				break;

			case RngPatternType.Empty:
			case RngPatternType.NotAllowed:
			case RngPatternType.Text:
			case RngPatternType.Value:
				break;

			//case RngPatternType.ExternalRef:
			//case RngPatternType.Include:
			// Mixed, Optional, ZeroOrMore are already removed.
			// Choice, Group, Interleave, OneOrMore are already proceeded.
			}
		}
#endif

		// Assemble combines.
		private void assembleCombine ()
		{
			// calculate combines.
			bool haveHeadStart = false;
			string combineStart = null;
			Hashtable haveHeadDefs = new Hashtable ();
			Hashtable combineDefs = new Hashtable ();

			// 1.calculate combine for starts.
			foreach (RngStart start in starts)
				checkCombine (ref haveHeadStart, 
					ref combineStart, start.Combine, "start");
			// 2.calculate combine for defines.
			foreach (RngDefine def in defs) {
				bool haveHead = 
					haveHeadDefs.ContainsKey (def.Name) ?
					haveHead = (bool) haveHeadDefs [def.Name]
					: false;
				string combine = combineDefs [def.Name] as string;
				checkCombine (ref haveHead, ref combine,
					def.Combine, String.Format ("define name={0}", def.Name));
				haveHeadDefs [def.Name] = haveHead;
				combineDefs [def.Name] = combine;
				continue;
			}

			// assemble starts and defines with "combine" attribute.

			// 3.assemble starts.
			if (starts.Count == 0)
				throw new RngException ("grammar must have at least one start component.");
			assembledStart = ((RngStart)starts [0]).Pattern;
			for (int i=1; i<starts.Count; i++) {
				RngPattern p2 = ((RngStart) starts [i]).Pattern;;
				if (combineStart == "interleave") {
					RngInterleave intlv = new RngInterleave ();
					intlv.Patterns.Add (assembledStart);
					intlv.Patterns.Add (p2);
					assembledStart = intlv;
				} else {
					RngChoice c = new RngChoice ();
					c.Patterns.Add (assembledStart);
					c.Patterns.Add (p2);
					assembledStart = c;
				}
			}

			// 4.assemble defines
			foreach (RngDefine def in defs) {
				string combine = combineDefs [def.Name] as string;
				RdpPattern p1 = 
					assembledDefs [def.Name] as RdpPattern;
				RdpPattern p2 = def.Compile (this);
				if (p1 != null) {
					if (combine == "interleave") {
						assembledDefs [def.Name] =
							new RdpInterleave (p1, p2);
					} else {
						assembledDefs [def.Name] =
							new RdpChoice (p1, p2);
					}
				} else {
					assembledDefs [def.Name] = p2;
				}
			}

		}

		// check combine attributes.
		private void checkCombine (ref bool haveHead, ref string combine, string newCombine, string targetSpec)
		{
			switch (newCombine) {
			case "interleave":
				if (combine == "choice")
					throw new RngException ("\"combine\" was already specified \"choice\"");
				else
					combine = "interleave";
				break;
			case "choice":
				if (combine == "interleave")
					throw new RngException ("\"combine\" was already specified \"interleave\"");
				else
					combine = "choice";
				break;
			case null:
				if (haveHead)
					throw new RngException (String.Format ("There was already \"{0}\" element without \"combine\" attribute.", targetSpec));
				haveHead = true;
				break;
			}
		}
	}
}
