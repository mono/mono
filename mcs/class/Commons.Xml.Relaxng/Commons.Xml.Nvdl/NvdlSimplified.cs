using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace Commons.Xml.Nvdl.Simplified
{
	public class SimplifiedItem : IXmlLineInfo
	{
		int line;
		int column;
		string sourceUri = String.Empty;

		public int LineNumber {
			get { return line; }
			set { line = value; }
		}

		public int LinePosition {
			get { return column; }
			set { column = value; }
		}

		public string SourceUri {
			get { return sourceUri; }
			set { sourceUri = value != null ? value : String.Empty; }
		}

		internal void FillLocation (NvdlElementBase e)
		{
			line = e.LineNumber;
			column = e.LinePosition;
			sourceUri = e.SourceUri;
		}

		public bool HasLineInfo ()
		{
			return line != 0;
		}

		public string Location {
			get { return line != 0 ? String.Format ("{0} ({1},{2})", sourceUri, line, column) : String.Empty; }
		}
	}

	public class SimpleRules : SimplifiedItem
	{
		SimpleMode startMode;
		XmlQualifiedName [] triggers;

		// FIXME: It is not used in validation step, so move it to
		// compile context
		SimpleMode [] modes;

		public SimpleRules (NvdlCompileContext context)
		{
			FillLocation (context.Rules);
			SimplifyPhase1 (context); // 6.4.1 - 10.
			SimplifyPhase2 (context); // 6.4.11 - 14.
			ResolveModes (context); // 6.4.15.
		}

		public SimpleMode StartMode {
			get { return startMode; }
		}

		public XmlQualifiedName [] Triggers {
			get { return triggers; }
		}

		#region Simplification
		private void SimplifyPhase1 (NvdlCompileContext ctx)
		{
			NvdlRules rules = ctx.Rules;
			// 6.4.1 : just ignore "Foreign" property.
			// 6.4.2 : already ignored on reading nvdl.
			// 6.4.3 : done in SOM
			// 6.4.4 : FIXME: must be done.
			// 6.4.5 : FIXME: considered in compiler.
			// 6.4.6 : FIXME: considered in compiler.
			// 6.4.7 : FIXME: considered in compiler.

			// 6.4.8 : here
			NvdlModeList list = rules.Modes;
			NvdlMode startMode = null;

			if (rules.Modes.Count > 0) {
				if (rules.Rules.Count > 0)
					throw new NvdlCompileException ("Modes and rules cannot coexist in 'rules' element.", rules);
				else if (rules.StartMode == null)
					throw new NvdlCompileException ("startMode is missing in 'rules' element when modes are specified.", rules);
				foreach (NvdlMode m in rules.Modes) {
					if (m.Name == rules.StartMode) {
						startMode = m;
						break;
					}
				}
				if (startMode == null)
					throw new NvdlCompileException ("Matching 'mode' element specified by 'startMode' does not exist.", rules);
			} else {
				if (rules.Rules.Count == 0)
					throw new NvdlCompileException ("Neither modes nor rules exists in 'rules' element.", rules);
				list = new NvdlModeList ();
				startMode = new NvdlMode ();
				startMode.SourceUri = rules.SourceUri;
				startMode.LineNumber = rules.LineNumber;
				startMode.LinePosition = rules.LinePosition;
				startMode.Name = "(startMode)";
				list.Add (startMode);
				foreach (NvdlRule rule in rules.Rules)
					startMode.Rules.Add (rule);
			}

			// 6.4.9 : done in SimpleMode.ctor() and
			// SimpleRule.ctor(), using ctx.CompiledModes.
			foreach (NvdlMode m in list) {
				SimpleMode sm = new SimpleMode (m, ctx);
				ctx.AddCompiledMode (sm.Name, sm);
				if (m == startMode)
					this.startMode = sm;
			}

			// 6.4.10 : done in SimpleRule.Simplify

			triggers = new XmlQualifiedName [rules.Triggers.Count];
			for (int i = 0; i < triggers.Length; i++) {
				NvdlTrigger t = rules.Triggers [i];
				triggers [i] = new XmlQualifiedName (
					t.Name, t.NS);
			}

			modes = (SimpleMode [])
				new ArrayList (ctx.GetCompiledModes ())
				.ToArray (typeof (SimpleMode));
		}

		private void SimplifyPhase2 (NvdlCompileContext ctx)
		{
			foreach (SimpleMode mode in modes)
				mode.SimplifyPhase2 (ctx);
		}

		private void ResolveModes (NvdlCompileContext ctx)
		{
			foreach (SimpleMode mode in modes)
				mode.ResolveModes (ctx);
		}
		#endregion
	}

	public class SimpleMode : SimplifiedItem
	{
		string name;
		SimpleRule [] rules;

		// They are available only after complete simplification.
		SimpleRule [] elementRules;
		SimpleRule [] attributeRules;

		public SimpleMode (NvdlMode mode, NvdlCompileContext ctx)
		{
			FillLocation (mode);

			if (mode.Name == null)
				throw new NvdlCompileException (
					"'mode' element must have a name.", mode);
			this.name = mode.Name;
			SimplifyPhase1 (mode, ctx);
		}

		public SimpleMode (string name, NvdlNestedMode mode,
			NvdlCompileContext ctx)
		{
			FillLocation (mode);

			this.name = name;
			SimplifyPhase1 (mode, ctx);
		}

		public SimpleMode (NvdlIncludedMode mode, NvdlCompileContext ctx)
		{
			FillLocation (mode);

			// name doesn't matter here.
			SimplifyPhase1 (mode, ctx);
		}

		public string Name {
			get { return name; }
		}

		public SimpleRule [] ElementRules {
			get { return elementRules; }
		}

		public SimpleRule [] AttributeRules {
			get { return attributeRules; }
		}

		private void SimplifyPhase1 (NvdlModeBase mode,
			NvdlCompileContext ctx)
		{
			NvdlModeCompileContext mctx =
				new NvdlModeCompileContext (mode);
			ctx.AddModeContext (this, mctx);
			ArrayList al = new ArrayList ();
			foreach (NvdlRule r in mode.Rules) {
				switch (r.Match) {
				case NvdlRuleTarget.Both:
					al.Add (new SimpleRule (r, true, ctx));
					al.Add (new SimpleRule (r, false, ctx));
					break;
				case NvdlRuleTarget.None:
				case NvdlRuleTarget.Elements:
					al.Add (new SimpleRule (r, false, ctx));
					break;
				case NvdlRuleTarget.Attributes:
					al.Add (new SimpleRule (r, true, ctx));
					break;
				}
			}
			foreach (NvdlIncludedMode inc in mode.IncludedModes)
				mctx.Included.Add (new SimpleMode (inc, ctx));
			// The rule table is just a dummy store that might
			// erase because of removal of inclusion.
			rules = (SimpleRule []) al.ToArray (typeof (SimpleRule));
		}

		internal void SimplifyPhase2 (NvdlCompileContext ctx)
		{
			ArrayList al = new ArrayList ();
			ConsumeIncludes (al, ctx);
			SimpleRule anyElement = null;
			SimpleRule anyAttribute = null;
			// 6.4.12 + part of 6.4.13
			CheckCollision (al, ref anyElement, ref anyAttribute);
			// 6.4.13
			if (anyElement == null) {
				NvdlAnyNamespace ann = new NvdlAnyNamespace ();
				ann.SourceUri = this.SourceUri;
				ann.LineNumber = this.LineNumber;
				ann.LinePosition = this.LinePosition;

				NvdlReject reject = new NvdlReject ();
				reject.SourceUri = this.SourceUri;
				reject.LineNumber = this.LineNumber;
				reject.LinePosition = this.LinePosition;
				ann.Actions.Add (reject);
				ann.Match = NvdlRuleTarget.Elements;

				al.Add (new SimpleRule (ann, false, ctx));
			}
			if (anyAttribute == null) {
				NvdlAnyNamespace ann = new NvdlAnyNamespace ();
				ann.SourceUri = this.SourceUri;
				ann.LineNumber = this.LineNumber;
				ann.LinePosition = this.LinePosition;

				NvdlAllow allow = new NvdlAllow ();
				allow.SourceUri = this.SourceUri;
				allow.LineNumber = this.LineNumber;
				allow.LinePosition = this.LinePosition;
				ann.Match = NvdlRuleTarget.Attributes;
				ann.Actions.Add (allow);

				al.Add (new SimpleRule (ann, true, ctx));
			}
			rules = (SimpleRule []) al.ToArray (typeof (SimpleRule));
		}

		private void ConsumeIncludes (ArrayList al,
			NvdlCompileContext ctx)
		{
			// The reason why we limit the check to current count
			// is to add invalid siblings (according to 6.4.12).
			int checkMax = al.Count;
			NvdlModeCompileContext mctx = ctx.GetModeContext (this);
			foreach (SimpleRule rule in rules) {
				if (ctx.CancelledRules [rule] != null)
					continue;
				bool exclude = false;
				for (int i = 0; i < checkMax; i++) {
					SimpleRule r = (SimpleRule) al [i];
					if (rule.IsAny == r.IsAny &&
						rule.MatchAttributes == r.MatchAttributes &&
						rule.NS == r.NS &&
						rule.Wildcard == r.Wildcard) {
						exclude = true;
						break;
					}
				}
				if (exclude)
					break;
				al.Add (rule);
			}
			foreach (SimpleMode mode in mctx.Included)
				mode.ConsumeIncludes (al, ctx);
		}

		private void CheckCollision (ArrayList al, ref SimpleRule el, ref SimpleRule attr)
		{
			for (int i = 0; i < al.Count; i++) {
				SimpleRule r1 = (SimpleRule) al [i];
				if (r1.IsAny) {
					if (r1.MatchAttributes)
						attr = r1;
					else
						el = r1;
				}
				for (int j = i + 1; j < al.Count; j++) {
					SimpleRule r2 = (SimpleRule) al [j];
					if (r1.MatchAttributes != r2.MatchAttributes)
						continue;
					if (r1.IsAny && r2.IsAny)
						throw new NvdlCompileException ("collision in mode was found. Two anyNamespace elements.", this);
					if (r1.IsAny || r2.IsAny)
						continue;
					if (Nvdl.NSMatches (r1.NS, 0, r1.Wildcard,
						r2.NS, 0, r2.Wildcard))
						throw new NvdlCompileException ("collision in mode was found.", this);
				}
			}
		}

		internal void ResolveModes (NvdlCompileContext ctx)
		{
			// Resolve moces and fill element/attributeRules.
			ArrayList e = new ArrayList ();
			ArrayList a = new ArrayList ();
			foreach (SimpleRule rule in rules) {
				rule.ResolveModes (ctx, this);
				if (rule.MatchAttributes)
					a.Add (rule);
				else
					e.Add (rule);
			}

			elementRules = (SimpleRule []) e.ToArray (typeof (SimpleRule));
			attributeRules = (SimpleRule []) a.ToArray (typeof (SimpleRule));
		}
	}

	public class SimpleRule : SimplifiedItem
	{
		bool matchAttributes;
		SimpleAction [] actions;

		readonly string ns;
		readonly string wildcard;
		bool isAny;

		public SimpleRule (NvdlRule rule, bool matchAttributes,
			NvdlCompileContext ctx)
		{
			FillLocation (rule);

			this.matchAttributes = matchAttributes;
			NvdlNamespace nss = rule as NvdlNamespace;
			if (nss == null)
				this.isAny = true;
			else {
				this.ns = nss.NS;
				if (nss.Wildcard == null)
					wildcard = "*";
				else if (nss.Wildcard.Length > 1)
					throw new NvdlCompileException ("'wildcard' attribute can specify at most one character string.", rule);
				else
					wildcard = nss.Wildcard;
			}

			SimplifyPhase1 (rule, ctx);
		}

		public bool MatchAttributes {
			get { return matchAttributes; }
		}

		public SimpleAction [] Actions {
			get { return actions; }
		}

		public string NS {
			get { return ns; }
		}

		public string Wildcard {
			get { return wildcard; }
		}

		public bool IsAny {
			get { return isAny; }
		}

		public bool MatchNS (string target)
		{
			if (isAny)
				return true;
			return Nvdl.NSMatches (ns, 0, wildcard, target, 0, "");
		}

		private void SimplifyPhase1 (NvdlRule r, NvdlCompileContext ctx)
		{
			ctx.AddRuleContext (this, r);
			// 6.4.9
			ArrayList al = new ArrayList ();
			foreach (NvdlAction a in r.Actions) {
				NvdlNoCancelAction nca =
					a as NvdlNoCancelAction;
				if (nca != null) {
					if (nca.ModeUsage != null)
						SimplifyModeUsage (nca, ctx);
					NvdlResultAction ra = nca as NvdlResultAction;
					if (ra != null)
						al.Add (new SimpleResultAction (ra, ctx));
					else if (nca is NvdlValidate)
						al.Add (new SimpleValidate (
							(NvdlValidate) nca, ctx));
					else if (nca is NvdlAllow)
						al.Add (new SimpleValidate (
							(NvdlAllow) nca, ctx));
					else
						al.Add (new SimpleValidate (
							(NvdlReject) nca, ctx));
				}
				else if (nca == null)
					ctx.CancelledRules.Add (this, this);
			}
			actions = (SimpleAction []) al.ToArray (
				typeof (SimpleAction));
		}

		private void SimplifyModeUsage (
			NvdlNoCancelAction nca, NvdlCompileContext ctx)
		{
			NvdlModeUsage usage = nca.ModeUsage;
			if (usage.NestedMode != null && ctx.GetCompiledMode (usage) == null) {
				SimpleMode sm = new SimpleMode (String.Empty,
					usage.NestedMode, ctx);
				ctx.AddCompiledMode (usage, sm);
			}
			foreach (NvdlContext c in usage.Contexts) {
				if (c.NestedMode != null) {
					SimpleMode sm = new SimpleMode (
						String.Empty, c.NestedMode, ctx);
					ctx.AddCompiledMode (c, sm);
				}
			}
		}

		internal void ResolveModes (NvdlCompileContext ctx, SimpleMode current)
		{
			foreach (SimpleAction a in actions)
				a.ResolveModes (ctx, current);
		}
	}

	public abstract class SimpleAction : SimplifiedItem
	{
		readonly ListDictionary messages;
		readonly SimpleModeUsage modeUsage;
		SimpleMode mode;

		protected SimpleAction (NvdlNoCancelAction action)
		{
			FillLocation (action);

			if (action.ModeUsage != null)
				modeUsage = new SimpleModeUsage (action.ModeUsage);
			messages = new ListDictionary ();
			if (action.SimpleMessage != null)
				messages.Add ("", action.SimpleMessage);
			foreach (NvdlMessage msg in action.Messages)
				messages.Add (msg.XmlLang, msg.Text);
		}

		public abstract bool NoResult { get; }

		public ListDictionary Messages {
			get { return messages; }
		}

		public SimpleMode DefaultMode {
			get { return mode; }
		}

		public SimpleContext [] Contexts {
			get { return modeUsage.Contexts; }
		}

		internal void ResolveModes (NvdlCompileContext ctx, SimpleMode current)
		{
			if (modeUsage != null) {
				modeUsage.ResolveModes (ctx, current);
				mode = modeUsage.UseMode;
			}
			if (mode == null)
				mode = current;
		}
	}

	public class SimpleValidate : SimpleAction
	{
		readonly NvdlValidatorGenerator generator;
		XmlResolver resolver;

		static NvdlValidate CreateBuiltInValidate (NvdlAction a)
		{
			bool allow = a is NvdlAllow;
			NvdlValidate v = new NvdlValidate ();
			v.SourceUri = a.SourceUri;
			v.LineNumber = a.LineNumber;
			v.LinePosition = a.LinePosition;
			v.ModeUsage = new NvdlModeUsage ();
			XmlDocument doc = new XmlDocument ();
			XmlElement el = doc.CreateElement (
				allow ? "allow" : "reject",
				Nvdl.BuiltInValidationUri);
			doc.AppendChild (doc.CreateElement ("schema",
				Nvdl.Namespace));
			doc.DocumentElement.AppendChild (el);
			v.SchemaBody = doc.DocumentElement;
			return v;
		}

		// 6.4.14
		public SimpleValidate (NvdlAllow allow, NvdlCompileContext ctx)
			: this (CreateBuiltInValidate (allow), ctx)
		{
		}

		// 6.4.14
		public SimpleValidate (NvdlReject reject, NvdlCompileContext ctx)
			: this (CreateBuiltInValidate (reject), ctx)
		{
		}

		public SimpleValidate (
			NvdlValidate validate,
			NvdlCompileContext ctx)
			: base (validate)
		{
			// 6.4.7
			generator = ctx.Config.GetGenerator (validate,
				ctx.Rules.SchemaType);
/*
			this.resolver = ctx.Config.XmlResolverInternal;

			// 6.4.7
			string schemaType = validate.SchemaType;
			if (schemaType == null)
				schemaType = ctx.Rules.SchemaType;
			if (schemaType == null && validate.SchemaBody != null && !ElementHasElementChild (validate.SchemaBody))
				schemaType = validate.SchemaBody.InnerText;
			if (schemaType == null)
				schemaType = "text/xml";

			// FIXME: this part must be totally rewritten.

			XmlReader schemaReader = null;
			if (schemaType == "text/xml") {
				if (validate.SchemaUri != null) {
					if (validate.SchemaBody != null)
						throw new NvdlCompileException ("Both 'schema' attribute and 'schema' element are specified in a 'validate' element.", validate);
					// FIXME: use NvdlConfig
					schemaReader = new XmlTextReader (validate.SchemaUri);
				}
				else if (validate.SchemaBody != null) {
					schemaReader = new XmlNodeReader (validate.SchemaBody);
					schemaReader.MoveToContent ();
					schemaReader.Read (); // Skip "schema" element
				}
				else
					throw new NvdlCompileException ("Neither 'schema' attribute nor 'schema' element is specified in a 'validate' element.", validate);
			}
			else
				throw new NvdlCompileException (String.Format ("MIME type '{0}' is not supported at this moment.", schemaType), validate);

			schemaReader.MoveToContent ();

			NvdlValidationProvider provider =
				ctx.Config.GetProvider (schemaReader.NamespaceURI);

			if (provider == null)
				throw new NvdlCompileException (String.Format ("Schema type '{0}' is not supported in this configuration. Use custom provider that supports this schema type.", schemaType), validate);

			generator = provider.CreateGenerator (schemaReader, ctx.Config.XmlResolverInternal);

			foreach (NvdlOption option in validate.Options) {
				bool mustSupport = option.MustSupport != null ?
					XmlConvert.ToBoolean (option.MustSupport) : false;
				if (!generator.AddOption (option.Name,
					option.Arg) && mustSupport)
					throw new NvdlCompileException (String.Format ("Option '{0}' with argument '{1}' is not supported for schema type '{2}'.",
						option.Name, option.Arg,
						schemaType), validate);
			}
*/
		}

		internal NvdlValidatorGenerator Generator {
			get { return generator; }
		}

		public override bool NoResult {
			get { return true; }
		}

		public XmlReader CreateValidator (XmlReader reader)
		{
			return generator.CreateValidator (reader, resolver);
		}
	}

	public class SimpleResultAction : SimpleAction
	{
		readonly NvdlResultType resultType;

		public SimpleResultAction (NvdlResultAction ra,
			NvdlCompileContext ctx)
			: base (ra)
		{
			this.resultType = ra.ResultType;
		}

		public override bool NoResult {
			get { return false; }
		}

		public NvdlResultType ResultType {
			get { return resultType; }
		}
	}

	public class SimpleModeUsage : SimplifiedItem
	{
		// It will never be used in validation.
		NvdlModeUsage source; // FIXME: put this into CompileContext
		readonly SimpleContext [] contexts;
		SimpleMode mode;

		public SimpleModeUsage (NvdlModeUsage usage)
		{
			this.source = usage;
			contexts = new SimpleContext [usage.Contexts.Count];
			for (int i = 0; i < contexts.Length; i++)
				contexts [i] = new SimpleContext (
					usage.Contexts [i]);
		}

		internal void ResolveModes (NvdlCompileContext ctx, SimpleMode current)
		{
			if (source.UseMode != null) {
				mode = ctx.GetCompiledMode (source.UseMode);
			}
			else if (source.NestedMode != null)
				mode = ctx.GetCompiledMode (source);
			else
				mode = current;

			for (int i = 0; i < contexts.Length; i++)
				contexts [i].ResolveModes (ctx, mode);

			// FIXME: get location by some way
			if (mode == null)
				throw new NvdlCompileException (
					"mode does not contain either referenced modeUsage or nested mode.", null);
		}

		public SimpleMode UseMode {
			get { return mode; }
		}

		public SimpleContext [] Contexts {
			get { return contexts; }
		}
	}

	internal class SimplePath
	{
		readonly SimplePathStep [] steps;

		public SimplePath (SimplePathStep [] steps)
		{
			this.steps = steps;
		}

		public SimplePathStep [] Steps {
			get { return steps; }
		}
	}

	internal class SimplePathStep
	{
		readonly string name;
		readonly bool descendants;

		public SimplePathStep (string name, bool descendants)
		{
			this.name = name;
			this.descendants = descendants;
		}

		public string Name {
			get { return name; }
		}

		public bool Descendants {
			get { return descendants; }
		}
	}

	public class SimpleContext : SimplifiedItem
	{
		readonly string useModeName; // It is never used in validation.
		SimpleMode useMode;
		SimplePath [] path;

		public SimpleContext (NvdlContext context)
		{
			FillLocation (context);

			this.useModeName = context.UseMode;

			try {
				string [] spaths = context.Path.Split ('|');
				ArrayList al = new ArrayList ();
				foreach (string spathws in spaths) {
					string spath = spathws.Trim (
						Nvdl.Whitespaces);
					if (spath.Length == 0)
						continue;
					ParsePath (al, spath);
				}
				path = (SimplePath []) al.ToArray (
					typeof (SimplePath));
			} catch (XmlException ex) {
				throw new NvdlCompileException (String.Format ("Invalid path string: {0}", path), ex, context);
			}
		}

		private void ParsePath (ArrayList al, string path)
		{
			ArrayList steps = new ArrayList ();
			int start = 0;
			do {
				int idx = path.IndexOf ('/', start);
				if (idx < 0) {
					steps.Add (new SimplePathStep (path.Substring (start), false));
					start = path.Length;
				}
				else if (path.Length > idx + 1 && path [idx + 1] == '/') {
					steps.Add (new SimplePathStep (path.Substring (start, idx - start), true));
					start = idx + 2;
				} else {
					steps.Add (new SimplePathStep (path.Substring (start, idx - start), false));
					start = idx + 1;
				}
				
			} while (start < path.Length);
			al.Add (new SimplePath (steps.ToArray (typeof (SimplePathStep)) as SimplePathStep []));
		}

		internal SimplePath [] Path {
			get { return path; }
		}

		public SimpleMode UseMode {
			get { return useMode; }
		}

		internal void ResolveModes (NvdlCompileContext ctx, SimpleMode current)
		{
			if (useModeName != null)
				useMode = ctx.GetCompiledMode (useModeName);
			else
				useMode = current;

			if (useMode == null)
				throw new NvdlCompileException (String.Format ("Specified mode '{0}' was not found.",
					useModeName), this);
		}
	}

/*
	// After simplification, each mode name "shall be different from any
	// other mode name" (6.4.8)
	public class SimpleModeTable : DictionaryBase
	{
		public SimpleModeTable (SimpleMode [] modes)
		{
			foreach (SimpleMode mode in modes)
				Dictionary.Add (mode.Name, mode);
		}

		public SimpleMode this [string name] {
			get { return (SimpleMode) Dictionary [name]; }
		}

		public ICollection Keys {
			get { return Dictionary.Keys; }
		}

		public ICollection Values {
			get { return Dictionary.Values; }
		}
	}
*/
}
