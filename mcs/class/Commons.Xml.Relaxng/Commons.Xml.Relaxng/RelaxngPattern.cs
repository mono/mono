//
// Commons.Xml.Relaxng.RelaxngPattern.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
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
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng.Derivative;
using Commons.Xml.Relaxng.Rnc;

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
#endif

namespace Commons.Xml.Relaxng
{
	#region Common abstract
	public abstract class RelaxngElementBase
	{
		bool isCompiled;
		int lineNumber, linePosition;
		string baseUri;

		internal bool IsCompiled {
			get { return isCompiled; }
			set { isCompiled = value; }
		}

		public int LineNumber {
			get { return lineNumber; }
			set { lineNumber = value; }
		}

		public int LinePosition {
			get { return linePosition; }
			set { linePosition = value; }
		}

		public string BaseUri {
			get { return baseUri; }
			set { baseUri = value; }
		}

		public abstract void Write (XmlWriter writer);

		internal abstract void WriteRnc (RncWriter writer);

		public RelaxngPattern ReadExternalResource (RelaxngGrammar grammar, Uri uri, string nsContext)
		{
			XmlTextReader xtr = null;
			RelaxngGrammar g = null;
			RelaxngPattern p;
			try {
				if (uri.AbsolutePath.EndsWith(".rnc", StringComparison.InvariantCultureIgnoreCase)) {
					p = RncParser.ParseRnc (new StreamReader ((Stream) grammar.Resolver.GetEntity (uri, null, typeof (Stream))), null, uri.AbsoluteUri, nsContext);
				} else {
					xtr = new XmlTextReader (uri.AbsoluteUri, (Stream) grammar.Resolver.GetEntity (uri, null, typeof (Stream)));
					RelaxngReader r = new RelaxngReader (xtr, nsContext, grammar.Resolver);
					r.MoveToContent ();
					p = r.ReadPattern ();
				}
			} catch (Exception ex) { // umm, bad catch though :-(
				throw new RelaxngException (this, String.Format("Could not include grammar {0}: {1}", uri.AbsoluteUri, ex.Message), ex);
			} finally {
				if (xtr != null)
					xtr.Close ();
			}
			p.XmlResolver = grammar.Resolver;
			return p;
		}
	}

	public abstract class RelaxngSingleContentPattern : RelaxngPattern
	{
		private RelaxngPatternList patterns = new RelaxngPatternList ();

		public RelaxngPatternList Patterns {
			get { return patterns; }
		}

		internal RdpPattern makeSingle (RelaxngGrammar g)
		{
			// Flatten patterns into RdpGroup. See 4.12.
			if (patterns.Count == 0)
				throw new RelaxngException (this, "No pattern contents.");
			RdpPattern p = ((RelaxngPattern) patterns [0]).Compile (g);
			if (patterns.Count == 1)
				return p;
			for (int i=1; i<patterns.Count; i++) {
				p = new RdpGroup (p,
					((RelaxngPattern) patterns [i]).Compile (g));
			}
			return p;
		}

		internal override void CheckConstraints () 
		{
			foreach (RelaxngPattern p in Patterns)
				p.CheckConstraints ();
		}
	}

	public abstract class RelaxngBinaryContentPattern : RelaxngPattern
	{
		private RelaxngPatternList patterns = new RelaxngPatternList ();

		public RelaxngPatternList Patterns {
			get { return patterns; }
		}

		internal RdpPattern makeBinary (RelaxngGrammar g)
		{
			// Flatten patterns. See 4.12.
			if (patterns.Count == 0)
				throw new RelaxngException (this, "No pattern contents.");

			RdpPattern p = ((RelaxngPattern) patterns [0]).Compile (g);
			if (patterns.Count == 1)
				return p;

			for (int i=1; i<patterns.Count; i++) {
				RdpPattern cp =
					((RelaxngPattern) patterns [i]).Compile (g);
				switch (this.PatternType) {
				case RelaxngPatternType.Choice:
					p = new RdpChoice (p, cp);
					break;
				case RelaxngPatternType.Group:
					p = new RdpGroup (p, cp);
					break;
				case RelaxngPatternType.Interleave:
					p = new RdpInterleave (p, cp);
					break;
				}
			}

			return p;
		}

		internal override void CheckConstraints () 
		{
			foreach (RelaxngPattern p in Patterns)
				p.CheckConstraints ();
		}
	}
	#endregion

	#region Grammatical elements
	public interface IGrammarContent
	{
	}

	public class RelaxngStart : RelaxngElementBase, IGrammarContent
	{
		RelaxngPattern p;
		string combine;

		public RelaxngStart ()
		{
		}

		public string Combine {
			get { return combine; }
			set { combine = value; }
		}

		public RelaxngPattern Pattern {
			get { return p; }
			set { p = value; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "start", RelaxngGrammar.NamespaceURI);
			if (combine != null)
				writer.WriteAttributeString ("combine", combine);
			p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteStart (this);
		}

		internal RdpPattern Compile (RelaxngGrammar grammar)
		{
			return p.Compile (grammar);
		}
	}

	public class RelaxngDefine : RelaxngElementBase, IGrammarContent
	{
		string name;
		private RelaxngPatternList patterns = new RelaxngPatternList ();
		string combine;

		public RelaxngDefine ()
		{
		}

		public RelaxngPatternList Patterns {
			get { return patterns; }
		}

		public string Combine {
			get { return combine; }
			set { combine = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "define", RelaxngGrammar.NamespaceURI);
			writer.WriteAttributeString ("name", name);
			if (combine != null)
				writer.WriteAttributeString ("combine", combine);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteDefine (this);
		}

		internal RdpPattern Compile (RelaxngGrammar grammar)
		{
			return makeSingle (grammar);
		}

		private RdpPattern makeSingle (RelaxngGrammar g)
		{
			// Flatten patterns into RdpGroup. See 4.12.
			if (patterns.Count == 0)
				throw new RelaxngException (this, "No pattern contents.");
			RdpPattern p = ((RelaxngPattern) patterns [0]).Compile (g);
			if (patterns.Count == 1)
				return p;
			for (int i=1; i<patterns.Count; i++) {
				p = new RdpGroup (p,
					((RelaxngPattern) patterns [i]).Compile (g));
			}
			return p;
		}
	}

	public class RelaxngInclude : RelaxngElementBase, IGrammarContent
	{
		string href;
		RelaxngGrammarContentList starts = new RelaxngGrammarContentList ();
		RelaxngGrammarContentList defines = new RelaxngGrammarContentList ();
		RelaxngGrammarContentList divs = new RelaxngGrammarContentList ();
		string ns;

		public RelaxngInclude ()
		{
		}

		public string Href {
			get { return href; }
			set { href = value; }
		}

		public RelaxngGrammarContentList Starts {
			get { return starts; }
		}

		public RelaxngGrammarContentList Defines {
			get { return defines; }
		}

		public RelaxngGrammarContentList Divs {
			get { return divs; }
		}

		public string NSContext {
			get { return ns; }
			set { ns = value; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "include", RelaxngGrammar.NamespaceURI);
			writer.WriteAttributeString ("href", href);
			foreach (RelaxngStart start in Starts)
				start.Write (writer);
			foreach (RelaxngDefine define in Defines)
				define.Write (writer);
			foreach (RelaxngDiv div in Divs)
				div.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteInclude (this);
		}

		// compile into div
		internal RelaxngDiv Compile (RelaxngGrammar grammar)
		{
			grammar.CheckIncludeRecursion (Href);
			grammar.IncludedUris.Add (Href, Href);
			if (grammar.Resolver == null)
				throw new RelaxngException (this, "To compile 'include' element, XmlResolver is required.");
			Uri baseUri = null;
			try {
				if (BaseUri != null && BaseUri != String.Empty)
					baseUri = new Uri (BaseUri);
			} catch (UriFormatException) {
				if (File.Exists (BaseUri))
					baseUri = new Uri (Path.GetFullPath (BaseUri));
			}
			Uri uri = grammar.Resolver.ResolveUri (baseUri, Href);

			RelaxngGrammar g = ReadExternalResource (grammar, uri, (ns != null && ns.Length != 0) || !grammar.IsSourceCompactSyntax ? ns : grammar.DefaultNamespace) as RelaxngGrammar;
			if (g == null)
				throw new RelaxngException (this, "Included syntax must start with \"grammar\" element.");
			g.DataProvider = grammar.Provider;
			g.IsSourceCompactSyntax = grammar.IsSourceCompactSyntax;

			// process recursive inclusions.
			foreach (RelaxngInclude inc in g.Includes)
				g.Divs.Add (inc.Compile (grammar));

			// process this own div children.
			// each div subelements are also compiled.
			foreach (RelaxngDiv cdiv in divs)
				cdiv.Compile (g);
			foreach (RelaxngDiv cdiv in g.Divs)
				cdiv.Compile (g);

			// replace redifinitions into div.
			// starts.
			if (this.Starts.Count > 0 && g.Starts.Count == 0)
				throw new RelaxngException (this, "When the included grammar does not contain start components, this include component must not contain start components.");
			RelaxngGrammarContentList appliedStarts = (this.starts.Count > 0) ?
				this.starts : g.Starts;

			RelaxngDiv div = new RelaxngDiv ();
			div.BaseUri = this.BaseUri;
			div.LinePosition = this.LinePosition;
			div.LineNumber = this.LineNumber;

			foreach (RelaxngStart start in appliedStarts)
				div.Starts.Add (start);

			// defines.
			Hashtable overrides = new Hashtable ();
			Hashtable originalDefs = new Hashtable ();
			foreach (RelaxngDefine def in defines) {
				overrides.Add (def.Name, def.Name);
				div.Defines.Add (def);
			}
			foreach (RelaxngDefine def in g.Defines) {
				originalDefs [def.Name] = def.Name;
				if (!overrides.ContainsKey (def.Name)) {
					div.Defines.Add (def);
				}
				// else discard.
			}

			foreach (string name in overrides.Values)
				if (!originalDefs.ContainsKey (name))
					throw new RelaxngException (this, "The include component must not contain define components whose name does not appear in the included grammar component.");

			grammar.IncludedUris.Remove (Href);
			return div;
		}
	}

	public class RelaxngDiv : RelaxngElementBase, IGrammarContent
	{
		RelaxngGrammarContentList starts = new RelaxngGrammarContentList ();
		RelaxngGrammarContentList defines = new RelaxngGrammarContentList ();
		RelaxngGrammarContentList includes = new RelaxngGrammarContentList ();
		RelaxngGrammarContentList divs = new RelaxngGrammarContentList ();

		public RelaxngDiv ()
		{
		}

		public RelaxngGrammarContentList Starts {
			get { return starts; }
		}

		public RelaxngGrammarContentList Defines {
			get { return defines; }
		}

		public RelaxngGrammarContentList Includes {
			get { return includes; }
		}

		public RelaxngGrammarContentList Divs {
			get { return divs; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "div", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngStart start in Starts)
				start.Write (writer);
			foreach (RelaxngDefine define in Defines)
				define.Write (writer);
			foreach (RelaxngInclude include in Includes)
				include.Write (writer);
			foreach (RelaxngDiv div in Divs)
				div.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteDiv (this);
		}

		internal void Compile (RelaxngGrammar grammar)
		{
			foreach (RelaxngDiv div in divs)
				div.Compile (grammar);
			foreach (RelaxngInclude inc in includes)
				inc.Compile (grammar).Compile (grammar); // compile compiled divs
			foreach (RelaxngStart start in starts)
				grammar.Starts.Add (start);
			foreach (RelaxngDefine define in defines)
				grammar.Defines.Add (define);
		}
	}
	#endregion

	#region RelaxngPatterns
	public abstract class RelaxngPattern : RelaxngElementBase
	{
		// static

		public static RelaxngPattern Read (XmlReader xmlReader)
		{
			return Read (xmlReader, null);
		}

		public static RelaxngPattern Read (XmlReader xmlReader, RelaxngDatatypeProvider provider)
		{
			return Read (xmlReader, provider, new XmlUrlResolver ());
		}
		
		public static RelaxngPattern Read (XmlReader xmlReader, RelaxngDatatypeProvider provider, XmlResolver xmlResolver)
		{
			RelaxngReader r = new RelaxngReader (xmlReader, null, xmlResolver);
			if (r.ReadState == ReadState.Initial)
				r.Read ();
			r.MoveToContent ();
			RelaxngPattern p = r.ReadPattern ();
			p.DataProvider = provider;
			p.XmlResolver = xmlResolver;
			return p;
		}

		// Private Fields
		RdpPattern startRelaxngPattern;
		RelaxngDatatypeProvider provider;
		XmlResolver resolver;
		bool nullResolver;

		// Public
		public XmlResolver XmlResolver {
			set {
				nullResolver = value == null;
				resolver = value;
			}
		}

		public abstract RelaxngPatternType PatternType { get; }
		public RelaxngDatatypeProvider DataProvider {
			get {
				return provider; 
			}
			set { 
				provider = value; 
			}
		}

		public void Compile ()
		{
			RelaxngGrammar g = null;
			if (this is RelaxngGrammar)
				g = (RelaxngGrammar) this;
			else {
				g = new RelaxngGrammar ();
				g.XmlResolver = this.Resolver;
				g.BaseUri = this.BaseUri;
				g.LineNumber = this.LineNumber;
				g.LinePosition = this.LinePosition;
				RelaxngStart st = new RelaxngStart ();
				st.BaseUri = this.BaseUri;
				st.LineNumber = this.LineNumber;
				st.LinePosition = this.LinePosition;
				st.Pattern = this;
				g.Starts.Add (st);
				g.Provider = provider;
			}
			startRelaxngPattern = g.Compile (null);
			this.IsCompiled = true;
		}

		public void WriteCompact (TextWriter writer)
		{
			WriteCompact (new RncWriter (writer));
		}

		public void WriteCompact (TextWriter writer, NSResolver res)
		{
			WriteCompact (new RncWriter (writer, res));
		}

		void WriteCompact (RncWriter writer)
		{
			RelaxngGrammar g = this as RelaxngGrammar;
			string ns = (g != null ? g.DefaultNamespace : null);
			writer.WriteNamespaces (ns);
			WriteRnc (writer);
		}

		// Internal
		internal XmlResolver Resolver {
			get {
				if (nullResolver)
					return null;
				if (resolver == null)
					resolver = new XmlUrlResolver ();
				return resolver;
			}
		}

		internal abstract void CheckConstraints ();

		protected RelaxngPattern () 
		{
		}

		internal abstract RdpPattern Compile (RelaxngGrammar grammar);

		internal RdpPattern StartPattern {
			get { return startRelaxngPattern; }
		}
	}

	public class RelaxngPatternList : CollectionBase
	{
		public RelaxngPatternList ()
		{
		}

		public void Add (RelaxngPattern p)
		{
			List.Add (p);
		}

		public RelaxngPattern this [int i] {
			get { return this.List [i] as RelaxngPattern; }
			set { this.List [i] = value; }
		}

		public void Insert (int pos, RelaxngPattern p)
		{
			List.Insert (pos, p);
		}

		public void Remove (RelaxngPattern p)
		{
			List.Remove (p);
		}
	}

	public class RelaxngGrammarContentList : CollectionBase
	{
		public RelaxngGrammarContentList ()
		{
		}

		public void Add (IGrammarContent p)
		{
			List.Add (p);
		}

		public IGrammarContent this [int i] {
			get { return this.List [i] as IGrammarContent; }
			set { this.List [i] = value; }
		}

		public void Insert (int pos, IGrammarContent p)
		{
			List.Insert (pos, p);
		}

		public void Remove (IGrammarContent p)
		{
			List.Remove (p);
		}
	}

	// strict to say, it's not a pattern ;)
	public class RelaxngNotAllowed : RelaxngPattern
	{
		public RelaxngNotAllowed () 
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.NotAllowed; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "notAllowed", RelaxngGrammar.NamespaceURI);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteNotAllowed (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			return RdpNotAllowed.Instance;
		}

		internal override void CheckConstraints () 
		{
			// nothing to check
		}
	}

	public class RelaxngEmpty : RelaxngPattern
	{
		public RelaxngEmpty ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Empty; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "empty", RelaxngGrammar.NamespaceURI);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteEmpty (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			return RdpEmpty.Instance;
		}

		internal override void CheckConstraints () 
		{
			// nothing to check
		}
	}

	public class RelaxngText : RelaxngPattern
	{
		public RelaxngText () 
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Text; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "text", RelaxngGrammar.NamespaceURI);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteText (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			return RdpText.Instance;
		}

		internal override void CheckConstraints () 
		{
			// nothing to check
		}
	}

	public abstract class RelaxngDataSupport : RelaxngPattern
	{
		string type;
		string datatypeLibrary;

		public string Type {
			get { return type; }
			set { type = value; }
		}

		public string DatatypeLibrary {
			get { return datatypeLibrary; }
			set { datatypeLibrary = value; }
		}

		internal void CheckDatatypeName ()
		{
			// Data type name check is done in RdpData(Except) derivative creation.
		}
	}

	public class RelaxngData : RelaxngDataSupport
	{
		RelaxngParamList paramList = new RelaxngParamList ();
		RelaxngExcept except;

		public RelaxngData ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Data; }
		}

		public RelaxngParamList ParamList {
			get { return paramList; }
		}

		public RelaxngExcept Except {
			get { return except; }
			set { except = value; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "data", RelaxngGrammar.NamespaceURI);
			if (DatatypeLibrary != null && DatatypeLibrary != String.Empty)
				writer.WriteAttributeString ("datatypeLibrary", DatatypeLibrary);
			writer.WriteAttributeString ("type", Type);

			foreach (RelaxngParam p in ParamList)
				p.Write (writer);

			if (Except != null)
				Except.Write (writer);

			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteData (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
//			RdpParamList rdpl = new RdpParamList ();
//			foreach (RelaxngParam prm in this.paramList)
//				rdpl.Add (prm.Compile (grammar));
			RdpPattern p = null;
			if (this.except != null) {
				if (except.Patterns.Count == 0)
					throw new RelaxngException (this, "data except pattern have no children.");
				p = except.Patterns [0].Compile (grammar);
				for (int i=1; i<except.Patterns.Count; i++)
					p = new RdpChoice (p,
						except.Patterns [i].Compile (grammar));
			}

			IsCompiled = true;
			if (this.except != null)
				return new RdpDataExcept (new RdpDatatype (DatatypeLibrary, Type, ParamList, grammar.Provider), p);
			else
				return new RdpData (new RdpDatatype (DatatypeLibrary, Type, ParamList, grammar.Provider));
		}

		internal override void CheckConstraints () 
		{
			CheckDatatypeName ();
		}
	}

	public class RelaxngValue : RelaxngDataSupport
	{
		string value;

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Value; }
		}

		public string Value {
			get { return value; }
			set { this.value = value; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "value", RelaxngGrammar.NamespaceURI);
			if (Type != null) {
				writer.WriteStartAttribute ("type", String.Empty);
				if (DatatypeLibrary != null && DatatypeLibrary != String.Empty)
					writer.WriteAttributeString ("datatypeLibrary", DatatypeLibrary);
				writer.WriteAttributeString ("type", Type);
			}
			writer.WriteString (Value);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteValue (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpValue (new RdpDatatype (DatatypeLibrary,
				Type, null, grammar.Provider), value);
		}

		internal override void CheckConstraints () 
		{
			CheckDatatypeName ();
		}
	}

	public class RelaxngList : RelaxngSingleContentPattern
	{
		internal RelaxngList ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.List; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "list", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteList (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{

			IsCompiled = true;
			return new RdpList (makeSingle (grammar));
		}

		internal override void CheckConstraints () 
		{
			// nothing to check
		}
	}

	public class RelaxngElement : RelaxngSingleContentPattern
	{
		RelaxngNameClass nc;

		public RelaxngElement ()
		{
		}

		public RelaxngNameClass NameClass {
			get { return nc; }
			set { nc = value; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Element; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "element", RelaxngGrammar.NamespaceURI);
			nc.Write (writer);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteElement (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			return new RdpElement (
				nc.Compile (grammar), this.makeSingle (grammar));
		}

		internal override void CheckConstraints () 
		{
			NameClass.CheckConstraints (false, false);

			foreach (RelaxngPattern p in Patterns)
				p.CheckConstraints ();
		}
	}

	public class RelaxngAttribute : RelaxngPattern
	{
		RelaxngNameClass nc;
		RelaxngPattern p;

		public RelaxngAttribute ()
		{
		}

		public RelaxngPattern Pattern {
			get { return p; }
			set { p = value; }
		}

		public RelaxngNameClass NameClass {
			get { return nc; }
			set { nc = value; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Attribute; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "attribute", RelaxngGrammar.NamespaceURI);
			nc.Write (writer);
			if (p != null)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteAttribute (this);
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
			RdpAnyNameExcept except = nc as RdpAnyNameExcept;
			if (except != null) {
				checkInvalidAttrNameClass (except.ExceptNameClass);
				return;
			}
			if (nc is RdpAnyName)
				return;

			RdpName n = nc as RdpName;
			if (n != null) {
				if (n.NamespaceURI == xmlnsNS)
					throw new RelaxngException (this, "cannot specify \"" + xmlnsNS + "\" for name of attribute.");
				if (n.LocalName == "xmlns" && n.NamespaceURI == "")
					throw new RelaxngException (this, "cannot specify \"xmlns\" inside empty ns context.");
			} else {
				RdpNsName nn = nc as RdpNsName;
				if (nn.NamespaceURI == "http://www.w3.org/2000/xmlns")
					throw new RelaxngException (this, "cannot specify \"" + xmlnsNS + "\" for name of attribute.");
				RdpNsNameExcept x = nc as RdpNsNameExcept;
				if (x != null)
					checkInvalidAttrNameClass (x.ExceptNameClass);
			}
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			RdpNameClass cnc = nc.Compile (grammar);
			this.checkInvalidAttrNameClass (cnc);

			return new RdpAttribute (cnc,
				(p != null) ?
					p.Compile (grammar) :
					RdpText.Instance);
		}

		internal override void CheckConstraints () 
		{
			NameClass.CheckConstraints (false, false);

			if (p != null)
				p.CheckConstraints ();
		}
	}

	internal class RdpUnresolvedRef : RdpPattern
	{
		string name;
//		bool parentRef;
		RelaxngGrammar targetGrammar;
		RdpPattern referencedPattern;

		public RdpUnresolvedRef (string name, RelaxngGrammar g)
		{
			this.name = name;
//			this.parentRef = parentRef;
			targetGrammar = g;
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public RdpPattern RefPattern {
			get { return referencedPattern; }
			set { referencedPattern = value; }
		}

//		public bool IsParentRef {
//			get { return parentRef; }
//		}

		public RelaxngGrammar TargetGrammar {
			get { return targetGrammar; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Ref; }
		}

		public override RdpContentType ContentType {
			get { return RdpContentType.Empty; }
		}


		public override bool Nullable {
			get {
				throw new InvalidOperationException ("Internal error: should not reach.");
			}
		}

		public override void GetLabels (Hashtable elements, Hashtable attributes, bool collectNameClass)
		{
			// Now it could reach (CheckNameOverlap) so comment out here.
//			throw new InvalidOperationException ("Internal error: should not reach.");
		}

		internal override RdpPattern ExpandRef (Hashtable defs)
		{
			return referencedPattern.ExpandRef (defs);
		}

		internal override void MarkReachableDefs () 
		{
			TargetGrammar.MarkReacheableDefine (this.name);
		}

		internal override RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			referencedPattern =
				referencedPattern.ReduceEmptyAndNotAllowed (
				ref result, visited);
			return this;
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept) 
		{
//			throw new InvalidOperationException ();
		}

		internal override bool ContainsText ()
		{
			return false;
//			throw new InvalidOperationException ();
		}
	}

	public class RelaxngRef : RelaxngPattern
	{
		string name;

		public RelaxngRef ()
		{
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Ref; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "ref", RelaxngGrammar.NamespaceURI);
			writer.WriteAttributeString ("name", name);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteRef (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			// Important!! This compile method only generates stub.
			IsCompiled = false;
			return new RdpUnresolvedRef (name, grammar);
		}

		internal override void CheckConstraints () 
		{
			// nothing to check
		}
	}

	public class RelaxngParentRef : RelaxngPattern
	{
		string name;

		public RelaxngParentRef ()
		{
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.ParentRef; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "parentRef", RelaxngGrammar.NamespaceURI);
			writer.WriteAttributeString ("name", name);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteParentRef (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = false;
			return new RdpUnresolvedRef (name, grammar.ParentGrammar);
		}

		internal override void CheckConstraints () 
		{
			// nothing to check
		}
	}

	public class RelaxngExternalRef : RelaxngPattern
	{
		string href;
		string ns;

		public RelaxngExternalRef ()
		{
		}

		public string Href {
			get { return href; }
			set { href = value; }
		}

		public string NSContext {
			get { return ns; }
			set { ns = value; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.ExternalRef; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "externalRef", RelaxngGrammar.NamespaceURI);
			writer.WriteAttributeString ("href", Href);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteExternalRef (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			grammar.CheckIncludeRecursion (Href);
			grammar.IncludedUris.Add (Href, Href);
			if (grammar.Resolver == null)
				throw new RelaxngException (this, "To compile 'include' element, XmlResolver is required.");
			Uri uri = grammar.Resolver.ResolveUri (BaseUri != String.Empty ? new Uri (BaseUri) : null, Href);
			RelaxngPattern p = ReadExternalResource (grammar, uri, ns);

			p.DataProvider = grammar.Provider;
			RdpPattern ret = p.Compile (grammar);

			grammar.IncludedUris.Remove (Href);

			return ret;

		}

		internal override void CheckConstraints () 
		{
			// nothing to check
		}
	}

	public class RelaxngOneOrMore : RelaxngSingleContentPattern
	{
		public RelaxngOneOrMore ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.OneOrMore; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "oneOrMore", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteOneOrMore (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpOneOrMore (makeSingle (grammar));
		}
	}

	public class RelaxngZeroOrMore : RelaxngSingleContentPattern
	{
		public RelaxngZeroOrMore ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.ZeroOrMore; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "zeroOrMore", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteZeroOrMore (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpChoice (
				new RdpOneOrMore (makeSingle (grammar)),
				RdpEmpty.Instance);
		}
	}

	public class RelaxngOptional : RelaxngSingleContentPattern
	{
		public RelaxngOptional ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Optional; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "optional", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteOptional (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpChoice (
				makeSingle (grammar), RdpEmpty.Instance);
		}
	}

	public class RelaxngMixed : RelaxngSingleContentPattern
	{
		public RelaxngMixed ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Mixed; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "mixed", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteMixed (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpInterleave (makeSingle (grammar), RdpText.Instance);
		}
	}

	public class RelaxngChoice : RelaxngBinaryContentPattern
	{
		public RelaxngChoice ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Choice; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "choice", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteChoice (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			return makeBinary (grammar);
		}
	}

	public class RelaxngGroup : RelaxngBinaryContentPattern
	{
		public RelaxngGroup ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Group; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "group", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteGroup (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			return makeBinary (grammar);
		}
	}

	public class RelaxngInterleave : RelaxngBinaryContentPattern
	{
		public RelaxngInterleave ()
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Interleave; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "interleave", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteInterleave (this);
		}

		internal override RdpPattern Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			return makeBinary (grammar);
		}
	}

	public class RelaxngParam : RelaxngElementBase
	{
		string name;
		string value;

		public RelaxngParam ()
		{
		}

		public RelaxngParam (string name, string value)
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

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "param", RelaxngGrammar.NamespaceURI);
			writer.WriteAttributeString ("name", name);
			writer.WriteString (Value);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteParam (this);
		}

		internal RdpParam Compile (RelaxngGrammar grammar)
		{
			IsCompiled = true;
			return new RdpParam (name, value);
		}
	}

	public class RelaxngParamList : CollectionBase
	{
		public RelaxngParamList ()
		{
		}

		public void Add (RelaxngParam p)
		{
			List.Add (p);
		}

		public RelaxngParam this [int i] {
			get { return this.List [i] as RelaxngParam; }
			set { this.List [i] = value; }
		}

		public void Insert (int pos, RelaxngParam p)
		{
			List.Insert (pos, p);
		}

		public void Remove (RelaxngParam p)
		{
			List.Remove (p);
		}
	}

	public class RelaxngExcept : RelaxngElementBase
	{
		RelaxngPatternList patterns = new RelaxngPatternList ();

		public RelaxngExcept ()
		{
		}

		public RelaxngPatternList Patterns {
			get { return patterns; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "except", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngPattern p in Patterns)
				p.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteDataExcept (this);
		}
	}

	internal class RelaxngRefPattern
	{
		RelaxngPattern patternRef;
		string name;

		// When we found ref, use it.
		public RelaxngRefPattern (string name)
		{
			this.name = name;
		}

		// When we found define, use it.
		public RelaxngRefPattern (RelaxngPattern patternRef)
		{
			this.patternRef = patternRef;
		}

		public string Name {
			get { return name; }
		}

		public RelaxngPattern PatternRef {
			get { return patternRef; }
			set { patternRef = value; }
		}
	}
	#endregion
}
