//
// Commons.Xml.Relaxng.RngPattern.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//

using System;
using System.Collections;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng.Derivative;

namespace Commons.Xml.Relaxng
{
	#region Common abstract
	public abstract class RngElementBase
	{
		bool isCompiled;

		internal protected bool IsCompiled {
			get { return isCompiled; }
			set { isCompiled = value; }
		}
	}

	public abstract class RngSingleContentPattern : RngPattern
	{
		private ArrayList patterns = new ArrayList ();

		public IList Patterns {
			get { return patterns; }
		}

		internal protected RdpPattern makeSingle (RngGrammar g)
		{
			// Flatten patterns into RdpGroup. See 4.12.
			if (patterns.Count == 0)
				throw new RngException ("No pattern contents.");
			RdpPattern p = ((RngPattern) patterns [0]).Compile (g);
			if (patterns.Count == 1)
				return p;
			for (int i=1; i<patterns.Count; i++) {
				p = new RdpGroup (p,
					((RngPattern) patterns [i]).Compile (g));
			}
			return p;
		}
	}

	public abstract class RngBinaryContentPattern : RngPattern
	{
		private ArrayList patterns = new ArrayList ();

		public IList Patterns {
			get { return patterns; }
		}

		internal protected RdpPattern makeBinary (RngGrammar g)
		{
			// Flatten patterns. See 4.12.
			if (patterns.Count == 0)
				throw new RngException ("No pattern contents.");

			RdpPattern p = ((RngPattern) patterns [0]).Compile (g);
			if (patterns.Count == 1)
				return p;

			for (int i=1; i<patterns.Count; i++) {
				RdpPattern cp =
					((RngPattern) patterns [i]).Compile (g);
				switch (this.PatternType) {
				case RngPatternType.Choice:
					p = new RdpChoice (p, cp);
					break;
				case RngPatternType.Group:
					p = new RdpGroup (p, cp);
					break;
				case RngPatternType.Interleave:
					p = new RdpInterleave (p, cp);
					break;
				}
			}

			return p;
		}
	}

	public abstract class RngCombinableElement : RngElementBase
	{
		string combine;

		public string Combine {
			get { return combine; }
			set { combine = value; }
		}
	}
	#endregion

	#region Grammatical elements
	public class RngStart : RngCombinableElement
	{
		RngPattern p;

		public RngStart ()
		{
		}

		public RngPattern Pattern {
			get { return p; }
			set { p = value; }
		}

		internal RdpPattern Compile (RngGrammar grammar)
		{
			return p.Compile (grammar);
		}
	}

	public class RngDefine : RngCombinableElement
	{
		string name;
		private ArrayList patterns = new ArrayList ();

		public IList Patterns {
			get { return patterns; }
		}

		private RdpPattern makeSingle (RngGrammar g)
		{
			// Flatten patterns into RdpGroup. See 4.12.
			if (patterns.Count == 0)
				throw new RngException ("No pattern contents.");
			RdpPattern p = ((RngPattern) patterns [0]).Compile (g);
			if (patterns.Count == 1)
				return p;
			for (int i=1; i<patterns.Count; i++) {
				p = new RdpGroup (p,
					((RngPattern) patterns [i]).Compile (g));
			}
			return p;
		}

		public RngDefine ()
		{
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		internal RdpPattern Compile (RngGrammar grammar)
		{
			return makeSingle (grammar);
		}
	}

	public class RngInclude : RngElementBase
	{
		string href;
		IList starts = new ArrayList ();
		IList defines = new ArrayList ();
		IList divs = new ArrayList ();

		public RngInclude ()
		{
		}

		public string Href {
			get { return href; }
			set { href = value; }
		}

		public IList Starts {
			get { return starts; }
		}

		public IList Defines {
			get { return defines; }
		}

		public IList Divs {
			get { return divs; }
		}

		// compile into div
		public void Compile (RngGrammar grammar)
		{
			grammar.CheckIncludeRecursion (Href);
			grammar.IncludedUris.Add (Href, Href);
			RelaxngReader r = new RelaxngReader (new XmlTextReader (href));
			r.MoveToContent ();
			RngGrammar g = r.ReadPattern () as RngGrammar;
			if (g == null)
				throw new RngException ("Included syntax must start with \"grammar\" element.");
			grammar.IncludedUris [Href] = null;

			// first, process this own div children.
			// each div subelements are also compiled.
			foreach (RngDiv cdiv in divs)
				cdiv.Compile (grammar);

			// replace redifinitions into div.
			// starts.
			IList appliedStarts = (this.starts.Count > 0) ?
				this.starts : g.Starts;
			foreach (RngStart start in appliedStarts)
				grammar.Starts.Add (start);

			// defines.
			Hashtable haveDefs = new Hashtable ();
			foreach (RngDefine def in defines) {
				haveDefs.Add (def.Name, def.Name);
				grammar.Defines.Add (def);
			}
			foreach (RngDefine def in g.Defines) {
				if (haveDefs [def.Name] == null)
					grammar.Defines.Add (def);
				// else discard.
			}
		}
	}

	public class RngDiv : RngElementBase
	{
		IList starts = new ArrayList ();
		IList defines = new ArrayList ();
		IList includes = new ArrayList ();
		IList divs = new ArrayList ();

		public RngDiv ()
		{
		}

		public IList Starts {
			get { return starts; }
		}

		public IList Defines {
			get { return defines; }
		}

		public IList Includes {
			get { return includes; }
		}

		public IList Divs {
			get { return divs; }
		}

		public void Compile (RngGrammar grammar)
		{
			foreach (RngDiv div in divs)
				div.Compile (grammar);
			foreach (RngInclude inc in includes)
				inc.Compile (grammar);
			foreach (RngStart start in starts)
				grammar.Starts.Add (start);
			foreach (RngDefine define in defines)
				grammar.Defines.Add (define);
		}
	}
	#endregion

	#region RngPatterns
	public abstract class RngPattern : RngElementBase
	{
		// static

		public static RngPattern Read (XmlReader xmlReader)
		{
			RelaxngReader r = new RelaxngReader (xmlReader);
			if (r.ReadState == ReadState.Initial)
				r.Read ();
			r.MoveToContent ();
			return r.ReadPattern ();
		}

		// Private Fields
		RdpPattern startRngPattern;

		// Public
		public abstract RngPatternType PatternType { get; }

		public void Compile ()
		{
			RngGrammar g = new RngGrammar ();
			RngStart st = new RngStart ();
			st.Pattern = this;
			g.Starts.Add (st);
			startRngPattern = g.Compile (null);
			this.IsCompiled = true;
		}


		// Internal

		internal protected RngPattern () 
		{
		}

		internal protected abstract RdpPattern Compile (RngGrammar grammar);

		internal RdpPattern StartPattern {
			get { return startRngPattern; }
		}
	}

	// strict to say, it's not a pattern ;)
	public class RngNotAllowed : RngPattern
	{
		static RngNotAllowed instance;
		static RngNotAllowed ()
		{
			instance = new RngNotAllowed ();
		}

		private RngNotAllowed () 
		{
			IsCompiled = true;
		}

		public static RngNotAllowed Instance {
			get { return instance; }
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.NotAllowed; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			return RdpNotAllowed.Instance;
		}
	}

	public class RngEmpty : RngPattern
	{
		static RngEmpty instance;
		static RngEmpty ()
		{
			instance = new RngEmpty ();
		}

		private RngEmpty ()
		{
			IsCompiled = true;
		}

		public static RngEmpty Instance {
			get { return instance; }
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Empty; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			return RdpEmpty.Instance;
		}
	}

	public class RngText : RngPattern
	{
		static RngText instance;
		static RngText ()
		{
			instance = new RngText ();
		}

		private RngText () 
		{
			IsCompiled = true;
		}

		public static RngText Instance {
			get { return instance; }
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Text; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			return RdpText.Instance;
		}
	}

	public class RngData : RngPattern
	{
		string type;
		string datatypeLibrary;
		ArrayList paramList = new ArrayList ();
		RngExcept except;

		public RngData ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Data; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			RdpParamList rdpl = new RdpParamList ();
			foreach (RngParam prm in this.paramList)
				rdpl.Add (prm.Compile (grammar));
			RdpPattern p = null;
			if (this.except != null) {
				if (except.Patterns.Count == 0)
					throw new RngException ("data except pattern have no children.");
				p = ((RngPattern) except.Patterns [0]).Compile (grammar);
				for (int i=1; i<except.Patterns.Count; i++)
					p = new RdpChoice (p,
						((RngPattern) except.Patterns [i]).Compile (grammar));
			}
			IsCompiled = true;
			if (this.except != null)
				return new RdpDataExcept (new RdpDatatype (datatypeLibrary, type), rdpl, p);
			else
				return new RdpData (new RdpDatatype (datatypeLibrary, type), rdpl);
		}

		public string Type {
			get { return type; }
			set { type = value; }
		}

		public string DatatypeLibrary {
			get { return datatypeLibrary; }
			set { datatypeLibrary = value; }
		}

		public ArrayList ParamList {
			get { return paramList; }
			set { paramList = value; }
		}

		public RngExcept Except {
			get { return except; }
			set { except = value; }
		}
	}

	public class RngValue : RngPattern
	{
		string type;
		string datatypeLibrary;
		string ns;
		string value;

		public override RngPatternType PatternType {
			get { return RngPatternType.Value; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpValue (new RdpDatatype (datatypeLibrary,
				type), value, null);
		}

		public string Type {
			get { return type; }
			set { type = value; }
		}

		public string DatatypeLibrary {
			get { return datatypeLibrary; }
			set { datatypeLibrary = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public string Value {
			get { return value; }
			set { this.value = value; }
		}

	}

	public class RngList : RngSingleContentPattern
	{
		IList patterns = new ArrayList ();

		internal RngList ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.List; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{

			IsCompiled = true;
			return new RdpList (makeSingle (grammar));
		}

	}

	public class RngElement : RngSingleContentPattern
	{
		RngNameClass nc;
		ArrayList patterns = new ArrayList ();

		public RngElement ()
		{
		}

		public RngNameClass NameClass {
			get { return nc; }
			set { nc = value; }
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Element; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			return new RdpElement (
				nc.Compile (grammar), this.makeSingle (grammar));
		}
	}

	public class RngAttribute : RngPattern
	{
		RngNameClass nc;
		RngPattern p;

		public RngAttribute ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Attribute; }
		}

		private void checkInvalidAttrNameClass (RdpNameClass nc)
		{
			string xmlnsNS = "http://www.w3.org/2000/xmlns";
			RdpNameClassChoice choice = nc as RdpNameClassChoice;
			if (choice != null) {
				checkInvalidAttrNameClass (choice.LValue);
				checkInvalidAttrNameClass (choice.RValue);
				return;
			}
			if (nc is RdpAnyName || nc is RdpAnyNameExcept)
				return;

			RdpName n = nc as RdpName;
			if (n != null) {
				if (n.NamespaceURI == xmlnsNS)
					throw new RngException ("cannot specify \"" + xmlnsNS + "\" for name of attribute.");
				if (n.LocalName == "xmlns" && n.NamespaceURI == "")
					throw new RngException ("cannot specify \"xmlns\" inside empty ns context.");
			} else {
				RdpNsName nn = nc as RdpNsName;
				if (nn.NamespaceURI == "http://www.w3.org/2000/xmlns")
					throw new RngException ("cannot specify \"" + xmlnsNS + "\" for name of attribute.");
			}
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			RdpNameClass cnc = nc.Compile (grammar);
			this.checkInvalidAttrNameClass (cnc);

			return new RdpAttribute (cnc,
				(p != null) ?
					p.Compile (grammar) :
					RdpText.Instance);
		}

		public RngPattern Pattern {
			get { return p; }
			set { p = value; }
		}

		public RngNameClass NameClass {
			get { return nc; }
			set { nc = value; }
		}
	}

	internal class RdpUnresolvedRef : RdpPattern
	{
		string name;
		bool parentRef;

		public RdpUnresolvedRef (string name, bool parentRef)
		{
			this.name = name;
			this.parentRef = parentRef;
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public bool IsParentRef {
			get { return parentRef; }
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Ref; }
		}

		public override bool Nullable {
			get {
				throw new InvalidOperationException ();
			}
		}

		public override object Clone ()
		{
			throw new InvalidOperationException ();
		}

		internal protected override RdpPattern ExpandRef (Hashtable defs)
		{
			RdpPattern target = (RdpPattern) defs [this.name];
			if (target == null)
				throw new RngException ("Target definition " + name + " not found.");
			return target.ExpandRef (defs);
		}
	}

	public class RngRef : RngPattern
	{
		string name;

		public RngRef ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Ref; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			// Important!! This compile method only generates stub.
			IsCompiled = false;
			return new RdpUnresolvedRef (name, false);
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}
	}

	public class RngParentRef : RngPattern
	{
		string name;

		public RngParentRef ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.ParentRef; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = false;
			return new RdpUnresolvedRef (name, true);
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}
	}

	public class RngExternalRef : RngPattern
	{
		string href;
		string ns;

		public RngExternalRef ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.ExternalRef; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			grammar.CheckIncludeRecursion (Href);
			grammar.IncludedUris.Add (Href, Href);
			RelaxngReader r = new RelaxngReader (new XmlTextReader (href), ns);
			r.MoveToContent ();
			RngPattern p = r.ReadPattern ();
			grammar.IncludedUris [Href] = null;

			return p.Compile (grammar);
		}

		public string Href {
			get { return href; }
			set { href = value; }
		}

		public string NSContext {
			get { return ns; }
			set { ns = value; }
		}
	}

	public class RngOneOrMore : RngSingleContentPattern
	{
		public RngOneOrMore ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.OneOrMore; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpOneOrMore (makeSingle (grammar));
		}
	}

	public class RngZeroOrMore : RngSingleContentPattern
	{
		public RngZeroOrMore ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.ZeroOrMore; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpChoice (
				new RdpOneOrMore (makeSingle (grammar)),
				RdpEmpty.Instance);
		}
	}

	public class RngOptional : RngSingleContentPattern
	{
		public RngOptional ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Optional; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpChoice (
				makeSingle (grammar), RdpEmpty.Instance);
		}
	}

	public class RngMixed : RngSingleContentPattern
	{
		public RngMixed ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Mixed; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpInterleave (makeSingle (grammar), RdpText.Instance);
		}
	}

	public class RngChoice : RngBinaryContentPattern
	{
		public RngChoice ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Choice; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			return makeBinary (grammar);
		}
	}

	public class RngGroup : RngBinaryContentPattern
	{
		public RngGroup ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Group; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			return makeBinary (grammar);
		}
	}

	public class RngInterleave : RngBinaryContentPattern
	{
		public RngInterleave ()
		{
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Interleave; }
		}

		internal protected override RdpPattern Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			return makeBinary (grammar);
		}
	}

	public class RngParam : RngElementBase
	{
		string name;
		string value;

		public RngParam ()
		{
		}

		public RngParam (string name, string value)
		{
			this.name = name;
			this.value = value;
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Value {
			get { return value; }
			set { this.value = value; }
		}

		internal RdpParam Compile (RngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpParam (name, value);
		}
	}

	public class RngExcept : RngElementBase
	{
		ArrayList patterns = new ArrayList ();

		public RngExcept ()
		{
		}

		internal IList Patterns {
			get { return patterns; }
		}
	}

	public class RngRefPattern
	{
		RngPattern patternRef;
		string name;

		// When we found ref, use it.
		public RngRefPattern (string name)
		{
			this.name = name;
		}

		// When we found define, use it.
		public RngRefPattern (RngPattern patternRef)
		{
			this.patternRef = patternRef;
		}

		public string Name {
			get { return name; }
		}

		public RngPattern PatternRef {
			get { return patternRef; }
			set { patternRef = value; }
		}
	}
	#endregion
}
