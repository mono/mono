using System;
using System.Collections;
using System.IO;
using System.Xml;
using Commons.Xml.Nvdl.Simplified;

namespace Commons.Xml.Nvdl
{
	internal class NvdlDebug
	{
//		public static TextWriter Writer = TextWriter.Null;
		public static TextWriter Writer = Console.Out;
	}

	internal class NvdlDispatcher
	{
		NvdlValidatingReader validator;
		SimpleRules rules;
		NvdlSection section;
		// Actually this dispatcher puts possibly identical sections
		// onto this stack at every StartElement() (unless
		// IsEmptyElement is true).
		NvdlSectionStack sectionStack = new NvdlSectionStack ();

		public NvdlDispatcher (SimpleRules rules, NvdlValidatingReader source)
		{
			this.validator = source;
			this.rules = rules;
		}

		internal NvdlValidatingReader Validator {
			get { return validator; }
		}

		internal XmlReader Reader {
			get { return validator.Reader; }
		}

		public SimpleRules Rules {
			get { return rules; }
		}

		public void StartElement ()
		{
NvdlDebug.Writer.WriteLine ("  <dispatcher.StartElement {0}. stack depth: {1}. current section ns {2}",
Reader.Name, sectionStack.Count, section == null ? "(none)" : section.Namespace);
			if (section == null)
				section = new NvdlSection (this, null);
			else if (section.Namespace != Reader.NamespaceURI)
				section = new NvdlSection (this, section);

			sectionStack.Push (section);
			section.StartElement ();
			if (Reader.IsEmptyElement)
				sectionStack.Pop ().EndSection ();
		}

		public void EndElement ()
		{
NvdlDebug.Writer.WriteLine ("  <dispatcher.EndElement {0}. depth: {1}",
Reader.Name, sectionStack.Count);
			if (section != null) {
				section = sectionStack.Pop ();
				section.EndElement ();
				section.EndSection ();
				if (sectionStack.Count > 0)
					section = sectionStack.Peek ();
				else
					section = null;
			}
		}

		public void Text ()
		{
			if (section != null)
				section.Text ();
		}

		public void Whitespace ()
		{
			if (section != null)
				section.Whitespace ();
		}
	}

	//
	// This class is instantiated for every NVDL section.
	//
	// For interpretations, the list might share the same
	// NvdlInterpretation I with other sections (as specified in
	// step 4 (8.6) of the spec.
	//
	internal class NvdlSection
	{
		readonly NvdlDispatcher dispatcher;
		readonly string ns;
		readonly NvdlInterpretationList ilist = new NvdlInterpretationList ();
		readonly ArrayList elementNameStack = new ArrayList ();

		public NvdlSection (NvdlDispatcher dispatcher,
			NvdlSection parentState)
		{
			this.dispatcher = dispatcher;
			this.ns = dispatcher.Reader.NamespaceURI;

			if (parentState == null) {
				foreach (SimpleAction a in FindElementRule (
					dispatcher.Rules.StartMode,
					dispatcher.Reader).Actions)
					ilist.Add (GetInterp (a, dispatcher));
			} else {
				foreach (NvdlInterpretation pi in
					parentState.ilist) {
					PopulateInterp (dispatcher, pi);
				}
			}

NvdlDebug.Writer.WriteLine ("New section: ns {0} / interp.count {1} / loc: {2}", ns, ilist.Count, ((IXmlLineInfo) dispatcher.Reader).LineNumber);
		}

		private NvdlInterpretation GetInterp (
			SimpleAction a, NvdlDispatcher d)
		{
			return CreateInterp (d, d.Rules.StartMode, a, null);
		}

		private NvdlInterpretation CreateInterp (NvdlDispatcher d,
			SimpleMode m, SimpleAction a, NvdlInterpretation p)
		{
NvdlDebug.Writer.WriteLine ("***** new interp from action {0} from mode {1}", a.Location, m.Location);
			SimpleValidate v = a as SimpleValidate;
			if (v != null)
				return new NvdlValidateInterp (d, m, v, p);
			return new NvdlResultInterp (d, m, (SimpleResultAction) a, p);
		}

		private void PopulateInterp (
			NvdlDispatcher d, NvdlInterpretation i)
		{
			SimpleMode m = FindContextMode (i.Action);
			SimpleRule rule = FindElementRule (m, dispatcher.Reader);
NvdlDebug.Writer.WriteLine ("***** populate interp from action {0} whose mode is {1}. Rule is {2} whose actions are {3}", i.Action.Location, m.Location, rule.Location, rule.Actions.Length);
			foreach (SimpleAction a in rule.Actions) {
				NvdlInterpretation cur = i;
				for (;cur != null; cur = cur.Parent)
					if (cur.CreatedMode == m && cur.Action == a) {
NvdlDebug.Writer.WriteLine ("------- corresponding PlanElem already exists.");
						break;
					}
				if (cur == null)
					cur = CreateInterp (d, m, a, i);
				ilist.Add (cur);
			}
		}

		private SimpleMode FindContextMode (SimpleAction a)
		{
			if (a.Contexts != null)
				foreach (SimpleContext ctx in a.Contexts)
					foreach (SimplePath path in ctx.Path)
						if (MatchPath (path))
							return ctx.UseMode;
			return a.DefaultMode;
		}

		private bool MatchPath (SimplePath path)
		{
			int elemStep = elementNameStack.Count - 1;
			for (int i = path.Steps.Length; i >= 0 && elemStep >= 0;) {
				SimplePathStep ps = path.Steps [i];
				if (ps.Name != elementNameStack [elemStep]) {
					if (!ps.Descendants)
						return false;
					if (--elemStep < 0)
					 	return false;
				}
				else {
					i--;
					elemStep--;
				}
			}
			return false;
		}

		public NvdlDispatcher Dispatcher {
			get { return dispatcher; }
		}

		public NvdlInterpretationList Interpretations {
			get { return ilist; }
		}

		public string Namespace {
			get { return ns; }
		}

		public XmlReader Reader {
			get { return dispatcher.Reader; }
		}

		private SimpleRule FindElementRule (SimpleMode mode, XmlReader reader)
		{
			SimpleRule any = null;
			foreach (SimpleRule rule in mode.ElementRules) {
				if (rule.MatchNS (reader.NamespaceURI)) {
					if (!rule.IsAny)
						return rule;
					else
						any = rule;
				}
			}
NvdlDebug.Writer.WriteLine (" : : : : anyNamespace rule being applied.");
			if (any != null)
				return any;
			throw new NvdlValidationException ("NVDL internal error: should not happen. No matching rule was found.", Reader as IXmlLineInfo);
		}

		// It is invoked regardless of IsEmptyElement.
		public void EndSection ()
		{
			foreach (NvdlInterpretation i in ilist)
				i.EndSection ();
		}

		public void StartElement ()
		{
NvdlDebug.Writer.WriteLine ("    <state.StartElement {0}", Reader.Name);
			elementNameStack.Add (Reader.LocalName);
			ValidateStartElement ();
			if (Reader.IsEmptyElement)
				elementNameStack.RemoveAt (elementNameStack.Count - 1);
		}

		private void ValidateStartElement ()
		{
			foreach (NvdlInterpretation i in ilist)
				i.StartElement ();
		}

		public void EndElement ()
		{
NvdlDebug.Writer.WriteLine ("    <state.EndElement {0} (for {2}). {1} interp.", Reader.Name, ilist.Count, Namespace);
			ValidateEndElement ();
			elementNameStack.RemoveAt (elementNameStack.Count - 1);
		}

		private void ValidateEndElement ()
		{
			foreach (NvdlInterpretation i in ilist)
				i.EndElement ();
		}

		public void Text ()
		{
			ValidateText ();
		}

		private void ValidateText ()
		{
			foreach (NvdlInterpretation i in ilist)
				i.Text ();
		}

		public void Whitespace ()
		{
			ValidateWhitespace ();
		}

		private void ValidateWhitespace ()
		{
			foreach (NvdlInterpretation i in ilist)
				i.Whitespace ();
		}
	}

	internal class NvdlSectionStack : CollectionBase
	{
		public void Push (NvdlSection state)
		{
			List.Add (state);
		}

		public NvdlSection this [int i] {
			get { return (NvdlSection) List [i]; }
		}

		public NvdlSection Peek ()
		{
			return (NvdlSection) List [List.Count - 1];
		}

		public NvdlSection Pop ()
		{
			NvdlSection ret = this [List.Count - 1];
			List.RemoveAt (List.Count - 1);
			return ret;
		}
	}

	internal class NvdlInterpretationList : CollectionBase
	{
		public void Add (NvdlInterpretation i)
		{
			List.Add (i);
		}

		public NvdlInterpretation this [int i] {
			get { return (NvdlInterpretation) List [i]; }
		}

		public void Remove (NvdlInterpretation i)
		{
			List.Remove (i);
		}
	}
	internal abstract class NvdlInterpretation
	{
		NvdlDispatcher dispatcher;
		SimpleMode createdMode; // IM(s)
		SimpleAction action; // IA(s)
		NvdlInterpretation parent;

		public NvdlInterpretation (NvdlDispatcher dispatcher,
			SimpleMode createdMode, SimpleAction action,
			NvdlInterpretation parent)
		{
			this.dispatcher = dispatcher;
			this.createdMode = createdMode;
			this.action = action;
			this.parent = parent;
		}

		internal NvdlDispatcher Dispatcher {
			get { return dispatcher; }
		}

		public SimpleMode CreatedMode {
			get { return createdMode; }
		}

		public SimpleAction Action {
			get { return action; }
		}

		public NvdlInterpretation Parent {
			get { return parent; }
		}

		public abstract void AttachPlaceHolder ();
		public abstract void DetachPlaceHolder ();
		public abstract void StartElement ();
		public abstract void EndElement ();
		public abstract void Text ();
		public abstract void Whitespace ();
		public abstract void ValidateStartElement ();
		public abstract void ValidateEndElement ();
		public abstract void ValidateText ();
		public abstract void ValidateWhitespace ();
		public abstract void EndSection ();
	}

	internal class NvdlResultInterp : NvdlInterpretation
	{
		NvdlResultType type;

		public NvdlResultInterp (NvdlDispatcher dispatcher,
			SimpleMode createdMode,
			SimpleResultAction resultAction,
			NvdlInterpretation parent)
			: base (dispatcher, createdMode, resultAction, parent)
		{
NvdlDebug.Writer.WriteLine ("++++++ new resultAction " + resultAction.Location);
			type = resultAction.ResultType;

			if (type == NvdlResultType.AttachPlaceHolder && parent != null)
				parent.AttachPlaceHolder ();
		}

		public override void EndSection ()
		{
			if (type == NvdlResultType.AttachPlaceHolder && Parent != null)
				Parent.DetachPlaceHolder ();
		}

		public override void AttachPlaceHolder ()
		{
			if (type == NvdlResultType.Unwrap)
				Parent.AttachPlaceHolder ();
		}

		public override void DetachPlaceHolder ()
		{
			if (type == NvdlResultType.Unwrap)
				Parent.DetachPlaceHolder ();
		}

		public override void StartElement ()
		{
NvdlDebug.Writer.WriteLine ("            <result.StartElement : " + type + "/"+ Action.Location);
			if (type != NvdlResultType.Unwrap)
				ValidateStartElement (); // unwrap itself does not dispatch to parent interpretation
NvdlDebug.Writer.WriteLine ("            </result>");
		}

		public override void EndElement ()
		{
NvdlDebug.Writer.WriteLine ("            <result.EndElement : " + type + "/" + ((IXmlLineInfo) Dispatcher.Reader).LineNumber);
			if (type != NvdlResultType.Unwrap)
				ValidateEndElement (); // unwrap itself does not dispatch to parent interpretation
NvdlDebug.Writer.WriteLine ("            </result>");
		}

		public override void Text ()
		{
NvdlDebug.Writer.WriteLine ("            <result.Text : " + type + "/" + ((IXmlLineInfo) Dispatcher.Reader).LineNumber);
			if (type != NvdlResultType.Unwrap)
				ValidateText (); // unwrap itself does not dispatch to parent interpretation
NvdlDebug.Writer.WriteLine ("            </result>");
		}

		public override void Whitespace ()
		{
NvdlDebug.Writer.WriteLine ("            <result.Whitespace : " + type + "/" + ((IXmlLineInfo) Dispatcher.Reader).LineNumber);
			if (type != NvdlResultType.Unwrap)
				ValidateWhitespace (); // unwrap itself does not dispatch to parent interpretation
NvdlDebug.Writer.WriteLine ("            </result>");
		}

		public override void ValidateStartElement ()
		{
			switch (type) {
			case NvdlResultType.Unwrap:
NvdlDebug.Writer.WriteLine (": : : : Unwrapping StartElement ");
				goto case NvdlResultType.Attach;
			case NvdlResultType.Attach:
				Parent.ValidateStartElement ();
				break;
			case NvdlResultType.AttachPlaceHolder:
				throw new NotImplementedException ();
			}
		}
		public override void ValidateEndElement ()
		{
			switch (type) {
			case NvdlResultType.Unwrap:
NvdlDebug.Writer.WriteLine (": : : : Unwrapping EndElement ");
				goto case NvdlResultType.Attach;
			case NvdlResultType.Attach:
				Parent.ValidateEndElement ();
				break;
			case NvdlResultType.AttachPlaceHolder:
				throw new NotImplementedException ();
			}
		}
		public override void ValidateText ()
		{
			switch (type) {
			case NvdlResultType.Unwrap:
NvdlDebug.Writer.WriteLine (": : : : Unwrapping Text ");
				goto case NvdlResultType.Attach;
			case NvdlResultType.Attach:
				Parent.ValidateText ();
				break;
			case NvdlResultType.AttachPlaceHolder:
				throw new NotImplementedException ();
			}
		}
		public override void ValidateWhitespace ()
		{
			switch (type) {
			case NvdlResultType.Unwrap:
NvdlDebug.Writer.WriteLine (": : : : Unwrapping Whitespace ");
				goto case NvdlResultType.Attach;
			case NvdlResultType.Attach:
				Parent.ValidateWhitespace ();
				break;
			case NvdlResultType.AttachPlaceHolder:
				throw new NotImplementedException ();
			}
		}
	}

	internal class NvdlValidateInterp : NvdlInterpretation
	{
		NvdlFilteredXmlReader reader; // s
		XmlReader validator;

		public NvdlValidateInterp (NvdlDispatcher dispatcher,
			SimpleMode createdMode, SimpleValidate validate,
			NvdlInterpretation parent)
			: base (dispatcher, createdMode, validate, parent)
		{
NvdlDebug.Writer.WriteLine ("++++++ new validate " + validate.Location);
			this.reader = new NvdlFilteredXmlReader (dispatcher.Reader, this);
			validator = validate.CreateValidator (this.reader);

			dispatcher.Validator.OnMessage (validate.Messages);
		}

		public override void AttachPlaceHolder ()
		{
			reader.AttachPlaceHolder ();
			validator.Read (); // check start Element
		}

		public override void DetachPlaceHolder ()
		{
			reader.DetachPlaceHolder ();
			validator.Read (); // check EndElement
		}

		public override void EndSection ()
		{
		}

		public override void StartElement ()
		{
			ValidateStartElement ();
		}

		public override void EndElement ()
		{
			ValidateEndElement ();
		}

		public override void Text ()
		{
			ValidateText ();
		}

		public override void Whitespace ()
		{
			ValidateWhitespace ();
		}

		public override void ValidateStartElement ()
		{
NvdlDebug.Writer.WriteLine ("###### interp.ValidateStartElement : " + Action.Location);
			validator.Read ();
		}

		public override void ValidateEndElement ()
		{
NvdlDebug.Writer.WriteLine ("###### interp.ValidateEndElement : " + Action.Location);
			validator.Read ();
		}

		public override void ValidateText ()
		{
NvdlDebug.Writer.WriteLine ("###### interp.ValidateText : " + Action.Location);
			validator.Read ();
		}

		public override void ValidateWhitespace ()
		{
NvdlDebug.Writer.WriteLine ("###### interp.Whitespace : " + Action.Location);
			validator.Read ();
		}
	}
}

