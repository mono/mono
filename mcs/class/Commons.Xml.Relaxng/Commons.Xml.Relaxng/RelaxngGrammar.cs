//
// Commons.Xml.Relaxng.RelaxngGrammar.cs
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

namespace Commons.Xml.Relaxng
{
	public class RelaxngGrammar : RelaxngPattern
	{
		// field
		public static string NamespaceURI =
			"http://relaxng.org/ns/structure/1.0";

		// object model fields
		ArrayList starts = new ArrayList ();
		ArrayList defs = new ArrayList ();
		ArrayList includes = new ArrayList ();
		ArrayList divs = new ArrayList ();

		RelaxngDatatypeProvider provider;

		// compiled fields.
		RdpPattern startPattern;

		// compile cache fields.
		Hashtable assembledDefs = new Hashtable (); // [defName] = RelaxngDefine
		RelaxngPattern assembledStart;
		RdpPattern compiledStart;
		Hashtable elementReplacedDefs = new Hashtable ();

		Hashtable includedUris = new Hashtable ();
		RelaxngGrammar parentGrammar;
		ArrayList includedGrammars;
		Hashtable refPatterns = new Hashtable (); // key = RdpPattern of assembledDefs

		// only for checkRecursion()
		Hashtable checkedDefs = new Hashtable ();

		// this should be checked after its compilation finished to complete
		// missing-at-the-tracking patterns (especially of parent grammars).
		// key = RdpPattern, value = ArrayList of unresolvedPatterns.
		ArrayList unresolvedPatterns = new ArrayList ();

		// contents key = RdpElement and value = name of the parent define.
		private Hashtable ElementDefMap = new Hashtable ();

		// Public

		public RelaxngGrammar ()
		{
		}

		internal RelaxngGrammar ParentGrammar {
			get { return parentGrammar; }
			set { parentGrammar = value; }
		}

		internal RelaxngDatatypeProvider Provider {
			get { return parentGrammar != null ? parentGrammar.Provider : provider; }
			set { provider = value; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Grammar; }
		}

		public IList Starts {
			get { return starts; }
		}

		public IList Defines {
			get { return defs; }
		}

		public IList Includes {
			get { return includes; }
		}

		public IList Divs {
			get { return divs; }
		}

		internal Hashtable IncludedUris {
			get { return includedUris; }
		}

		// Internal
		internal override void CheckConstraints ()
		{
			// do nothing here.
		}

		internal void CheckIncludeRecursion (string href)
		{
			if (this.includedUris [href] != null)
				throw new RelaxngException ("Include recursion found. href: " + href);
			if (parentGrammar != null)
				parentGrammar.CheckIncludeRecursion (href);
		}

		// Compile from this simplified syntax to derivatives.
		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			parentGrammar = grammar;

			// First, process includes and divs. RELAX NG 4.1 - 4.15.
			ArrayList compiledDivs = new ArrayList ();
			foreach (RelaxngInclude inc in includes)
				compiledDivs.Add (inc.Compile (this));
			compiledDivs.AddRange (divs);
			foreach (RelaxngDiv div in compiledDivs)
				div.Compile (this);

			// Check constraints. RELAX NG 4.16
			foreach (RelaxngStart start in starts)
				start.Pattern.CheckConstraints ();
			foreach (RelaxngDefine define in defs)
				foreach (RelaxngPattern p in define.Patterns)
					p.CheckConstraints ();

			// Assemble combine into the same name defines/start.
			// see RELAX NG 4.17.
			AssembleCombine ();

			// FIXME: It should not return NotAllowed
			if (assembledStart != null)
				compiledStart = assembledStart.Compile (this);
			else
				return RdpNotAllowed.Instance;

			// Assemble all define components into top grammar and
			// return start patterns for descendant grammars.
			// see RELAX NG 4.18.
			CollectGrammars ();
			if (parentGrammar != null)
				return compiledStart;
			assembledStart = null; // no use anymore

			// 4.19 (a) remove non-reachable defines
/*
			compiledStart.MarkReachableDefs ();
			ArrayList tmp = new ArrayList ();
			foreach (DictionaryEntry entry in this.assembledDefs)
				if (!reachableDefines.ContainsKey (entry.Key))
					tmp.Add (entry.Key);
			foreach (string key in tmp)
				assembledDefs.Remove (key);
*/
			// 4.19 (b) check illegal recursion
			CheckRecursion (compiledStart, 0);
			// here we collected element-replaced definitions
			foreach (DictionaryEntry entry in elementReplacedDefs)
				assembledDefs.Add (entry.Key, entry.Value);
#if REPLACE_IN_ADVANCE
// Well, actual expandRef should be done after checking constraints
			// 4.19 (c) expandRef
			startPattern = compiledStart.ExpandRef (assembledDefs);
#else
			startPattern = compiledStart;
#endif
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

			// Check Constraints: RELAX NG spec 7
			// 7.1.1-4 & 7.4
			startPattern.CheckConstraints (false, false, false, false, false, false);
			// 7.1.5
			CheckStartPatternContent (startPattern);

			// 7.2
			RdpContentType ct = startPattern.ContentType;

			// TODO: 7.3

#if !REPLACE_IN_ADVANCE
			// 4.19 (c) expandRef - actual replacement
			startPattern = compiledStart.ExpandRef (assembledDefs);
#endif

			// return its start pattern.
			IsCompiled = true;
			return startPattern;
		}

		private void CheckStartPatternContent (RdpPattern p)
		{
			switch (p.PatternType) {
#if !REPLACE_IN_ADVANCE
			case RelaxngPatternType.Ref:
				CheckStartPatternContent (((RdpUnresolvedRef) p).RefPattern);
#endif
				break;
			case RelaxngPatternType.Element:
				break;
			case RelaxngPatternType.Choice:
				RdpChoice c = p as RdpChoice;
				CheckStartPatternContent (c.LValue);
				CheckStartPatternContent (c.RValue);
				break;
			case RelaxngPatternType.NotAllowed:
				break;
			default:
				throw new RelaxngException ("Start pattern contains an invalid content pattern.");
			}
		}

		Hashtable reachableDefines = new Hashtable ();

		// for step 4.19
		internal void MarkReacheableDefine (string name)
		{
			if (reachableDefines.ContainsKey (name))
				return;
			RdpPattern p = assembledDefs [name] as RdpPattern;
			reachableDefines.Add (name, p);
			p.MarkReachableDefs ();
		}

		// 4.19 (b)
		private void CheckRecursion (RdpPattern p, int depth)
		{

			RdpAbstractBinary binary = p as RdpAbstractBinary;
			if (binary != null) {
				// choice, interleave, group
				CheckRecursion (binary.LValue, depth);
				CheckRecursion (binary.RValue, depth);
				return;
			}
			RdpAbstractSingleContent single = p as RdpAbstractSingleContent;
			if (single != null) {
				CheckRecursion (single.Child, depth);
				return;
			}

			switch (p.PatternType) {
			case RelaxngPatternType.Ref:
				// get checkRecursionDepth from table.
				int checkRecursionDepth = -1;
				object checkedDepth = checkedDefs [p];
				if (checkedDepth != null)
					checkRecursionDepth = (int) checkedDepth;
				// get refPattern
				RdpUnresolvedRef pref = p as RdpUnresolvedRef;
				RelaxngGrammar target = pref.TargetGrammar;//pref.IsParentRef ? parentGrammar : this;
				RdpPattern refPattern = pref.RefPattern;//target.assembledDefs [pref.Name] as RdpPattern;
				if (refPattern == null)
					throw new RelaxngException ("No matching define found for " + pref.Name);

				if (checkRecursionDepth == -1) {
					checkedDefs [p] = depth;
/*test*/					if (refPattern.PatternType != RelaxngPatternType.Element)
						CheckRecursion (refPattern, depth);
					checkedDefs [p] = -2;
				}
				else if (depth == checkRecursionDepth)
					throw new RelaxngException (String.Format ("Detected illegal recursion. Ref name is {0}.", pref.Name));

				break;

			case RelaxngPatternType.Attribute:
				CheckRecursion (((RdpAttribute) p).Children, depth);
				break;

			case RelaxngPatternType.DataExcept:
				CheckRecursion (((RdpDataExcept) p).Except, depth);
				break;

			case RelaxngPatternType.Element:
				RdpElement el = p as RdpElement;
				CheckRecursion (el.Children, depth + 1);	// +1
				break;
			case RelaxngPatternType.List:
				CheckRecursion (((RdpList) p).Child, depth);
				break;
			}
		}

		// 4.18
		private void CollectGrammars ()
		{
			// collect ref and parentRef for each define.
			includedGrammars = (parentGrammar != null) ?
				parentGrammar.includedGrammars : new ArrayList ();

			// FIXME: This should be assembledStart.
			CheckReferences (compiledStart);
			FixupReference ();
			this.unresolvedPatterns.Clear ();

//			Hashtable ht = assembledDefs.Clone () as Hashtable;
//			foreach (string name in ht.Keys) {
			foreach (string name in assembledDefs.Keys) {
				RdpPattern p = (RdpPattern) assembledDefs [name];
				CheckReferences (p);
				FixupReference ();
				this.unresolvedPatterns.Clear ();
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

		private void FixupReference ()
		{
			foreach (RdpUnresolvedRef pref in this.unresolvedPatterns) {
				RdpPattern defP = assembledDefs [pref.Name] as RdpPattern;
				if (defP == null)
					throw new RelaxngException ("Target definition was not found.");
				ArrayList al = refPatterns [defP] as ArrayList;
				if (al == null) {
					al = new ArrayList ();
					refPatterns [defP] = al;
				}
				al.Add (pref);
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
		private void CheckReferences (RdpPattern p)
		{
			RdpAbstractBinary binary = p as RdpAbstractBinary;
			if (binary != null) {
				// choice, interleave, group
				CheckReferences (binary.LValue);
				CheckReferences (binary.RValue);
				return;
			}
			RdpAbstractSingleContent single = p as RdpAbstractSingleContent;
			if (single != null) {
				CheckReferences (single.Child);
				return;
			}

			switch (p.PatternType) {
			case RelaxngPatternType.Ref:
				// FIXME: This should not re-expand ref
				RdpUnresolvedRef pref = p as RdpUnresolvedRef;
				if (pref.RefPattern != null)
					break;

				RelaxngGrammar target = pref.TargetGrammar;
				if (target == null)
					throw new RelaxngException ("Referenced definition was not found.");
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
					pref.RefPattern = defP;
				}
				break;

			case RelaxngPatternType.Attribute:
				CheckReferences (((RdpAttribute) p).Children);
				break;

			case RelaxngPatternType.DataExcept:
				CheckReferences (((RdpDataExcept) p).Except);
				break;

			case RelaxngPatternType.Element:
				RdpElement el = p as RdpElement;
				CheckReferences (el.Children);
				string name = ElementDefMap [el] as string;
				if (name == null) {
					// add new define
					int idx = 0;
					string newName = "element0";
					if (el.NameClass is RdpName)
						newName = ((RdpName) el.NameClass).LocalName;
					while (true) {
                                                if (assembledDefs [newName] == null) {
							elementReplacedDefs [newName] = el.Children;
							break;
						}
						newName = "element" + ++idx;
					}
					ElementDefMap [el] = newName;
				}
				// Even though the element is replaced with ref,
				// derivative of ref is RdpElement in fact...
				break;

			case RelaxngPatternType.List:
				CheckReferences (((RdpList) p).Child);
				break;

			case RelaxngPatternType.Empty:
			case RelaxngPatternType.NotAllowed:
			case RelaxngPatternType.Text:
			case RelaxngPatternType.Value:
				break;

			//case RelaxngPatternType.ExternalRef:
			//case RelaxngPatternType.Include:
			// Mixed, Optional, ZeroOrMore are already removed.
			// Choice, Group, Interleave, OneOrMore are already proceeded.
			}
		}

		#region 4.17 - Combine
		private void AssembleCombine ()
		{
			// calculate combines.
			bool haveHeadStart = false;
			string combineStart = null;
			Hashtable haveHeadDefs = new Hashtable ();
			Hashtable combineDefs = new Hashtable ();

			// 1.calculate combine for starts.
			foreach (RelaxngStart start in starts)
				CheckCombine (ref haveHeadStart, 
					ref combineStart, start.Combine, "start");
			// 2.calculate combine for defines.
			foreach (RelaxngDefine def in defs) {
				bool haveHead = 
					haveHeadDefs.ContainsKey (def.Name) ?
					haveHead = (bool) haveHeadDefs [def.Name]
					: false;
				string combine = combineDefs [def.Name] as string;
				CheckCombine (ref haveHead, ref combine,
					def.Combine, String.Format ("define name={0}", def.Name));
				haveHeadDefs [def.Name] = haveHead;
				combineDefs [def.Name] = combine;
				continue;
			}

			// assemble starts and defines with "combine" attribute.

			// 3.assemble starts.
			if (starts.Count == 0) {
				if (ParentGrammar == null)
					throw new RelaxngException ("grammar must have at least one start component.");
			} else {
				assembledStart = ((RelaxngStart)starts [0]).Pattern;
				for (int i=1; i<starts.Count; i++) {
					RelaxngPattern p2 = ((RelaxngStart) starts [i]).Pattern;;
					if (combineStart == "interleave") {
						RelaxngInterleave intlv = new RelaxngInterleave ();
						intlv.Patterns.Add (assembledStart);
						intlv.Patterns.Add (p2);
						assembledStart = intlv;
					} else {
						RelaxngChoice c = new RelaxngChoice ();
						c.Patterns.Add (assembledStart);
						c.Patterns.Add (p2);
						assembledStart = c;
					}
				}
			}

			// 4.assemble defines
			foreach (RelaxngDefine def in defs) {
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
		private void CheckCombine (ref bool haveHead, ref string combine, string newCombine, string targetSpec)
		{
			switch (newCombine) {
			case "interleave":
				if (combine == "choice")
					throw new RelaxngException ("\"combine\" was already specified \"choice\"");
				else
					combine = "interleave";
				break;
			case "choice":
				if (combine == "interleave")
					throw new RelaxngException ("\"combine\" was already specified \"interleave\"");
				else
					combine = "choice";
				break;
			case null:
				if (haveHead)
					throw new RelaxngException (String.Format ("There was already \"{0}\" element without \"combine\" attribute.", targetSpec));
				haveHead = true;
				break;
			}
		}
		#endregion
	}
}
