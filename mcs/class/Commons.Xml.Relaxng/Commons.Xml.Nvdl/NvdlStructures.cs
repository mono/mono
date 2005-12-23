using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using Commons.Xml.Relaxng;

//using Map = Commons.Xml.Relaxng.ObjectMapping.RelaxngMapping;
//using Choice = Commons.Xml.Relaxng.ObjectMapping.RelaxngMappingChoice;

namespace Commons.Xml.Nvdl
{
	public class Nvdl
	{
		private Nvdl () {}

		public const string Namespace = "http://purl.oclc.org/dsdl/nvdl/ns/structure/1.0";
		public const string BuiltInValidationNamespace = "http://purl.oclc.org/dsdl/nvdl/ns/predefinedSchema/1.0";

		public const string InstanceNamespace = "http://purl.oclc.org/dsdl/nvdl/ns/instance/1.0";

		internal const string XmlNamespaceUri = "http://www.w3.org/xml/1998/namespace";

		private static void OnDefaultEvent (object o, 
			NvdlMessageEventArgs e)
		{
			Console.WriteLine (e.Message);
		}

		internal static NvdlMessageEventHandler HandlePrintMessage =
			new NvdlMessageEventHandler (OnDefaultEvent);

		readonly static NvdlConfig defaultConfig;

		static Nvdl ()
		{
			defaultConfig = new NvdlConfig ();
			defaultConfig.AddProvider (new NvdlXsdValidatorProvider ());
			defaultConfig.AddProvider (new NvdlRelaxngValidatorProvider ());
		}

		internal static NvdlConfig DefaultConfig {
			get { return defaultConfig; }
		}

		internal static readonly char [] Whitespaces =
			new char [] {' ', '\r', '\n', '\t'};

		// See 6.4.12.
		internal static bool NSMatches (string n1, int i1, string w1,
			string n2, int i2, string w2)
		{
			// quick check
			if (n1 == n2)
				return true;

			// Case 1:
			if (n1.Length <= i1 && n2.Length <= i2)
				return true;
			// Case 2:
			if (n1.Length <= i1 && n2 == w2 ||
				n2.Length <= i2 && n1 == w1)
				return true;
			// Case 3:
			if (n1.Length > i1 && n2.Length > i2 &&
				n1 [i1] == n2 [i2] &&
				(w1.Length == 0 || n1 [i1] != w1 [0]) &&
				(w2.Length == 0 || n2 [i2] != w2 [0]) &&
				NSMatches (n1, i1 + 1, w1, n2, i2 + 1, w2))
				return true;
			// Case 4:
			if (w1 != "" &&
				n1.Length > i1 && n1 [i1] == w1 [0] &&
				NSMatches (n1, i1, w1, n2, i2 + 1, w2))
				return true;
			// Case 5:
			if (w2 != "" &&
				n2.Length > i2 && n2 [i2] == w2 [0] &&
				NSMatches (n1, i1 + 1, w1, n2, i2, w2))
				return true;
			return false;
		}
	}

	public class NvdlMessageEventArgs : EventArgs
	{
		string message;

		public NvdlMessageEventArgs (string message)
		{
			this.message = message;
		}

		public string Message {
			get { return message; }
		}
	}

	public delegate void NvdlMessageEventHandler (object o, NvdlMessageEventArgs e);

	public class NvdlElementBase : IXmlLineInfo
	{
		int line, column;
		string sourceUri;

		public int LineNumber {
			get { return line; }
			set { line = value; }
		}
		
		public int LinePosition {
			get { return column; }
			set { column = value; }
		}
		
		public bool HasLineInfo ()
		{
			return line > 0 && column > 0;
		}

		public string SourceUri {
			get { return sourceUri; }
			set { sourceUri = value; }
		}
	}

	public class NvdlAttributable : NvdlElementBase
	{
		ArrayList foreign = new ArrayList ();

		public ArrayList Foreign {
			get { return foreign; }
		}
	}

	/*
	element rules {
		(schemaType?,
		trigger*,
		(rule* | (attribute startMode { xsd:NCName }, mode+)))
		& foreign
	}
	*/
	public class NvdlRules : NvdlAttributable
	{
		string schemaType;
		NvdlTriggerList triggers = new NvdlTriggerList ();
		NvdlRuleList rules = new NvdlRuleList ();
		NvdlModeList modes = new NvdlModeList ();
		string startMode;

//		[Map.Optional]
//		[Map.Attribute]
		public string SchemaType {
			get { return schemaType; }
			set { schemaType = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

//		[Map.ZeroOrMore]
		public NvdlTriggerList Triggers {
			get { return triggers; }
		}

//		[Map.ZeroOrMore]
		public NvdlRuleList Rules {
			get { return rules; }
		}

//		[Map.Attribute]
//		[MapType ("NCName", XmlSchema.Namespace)]
		public string StartMode {
			get { return startMode; }
			set { startMode = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

//		[Map.OneOrMore]
		public NvdlModeList Modes {
			get { return modes; }
		}
	}

	/*
	element trigger {
		(attribute ns { xsd:string },
		attribute nameList { list { xsd:NCName } })
		& foreign
	}
	*/
	public class NvdlTrigger : NvdlAttributable
	{
		string ns;
		string nameList;

//		[Map.Attribute]
		public string NS {
			get { return ns; }
			set { ns = value; }
		}

//		[Map.Attribute]
//		[Map.List]
		public string NameList {
			get { return nameList; }
			set { nameList = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}
	}

	/*
	element mode {
		(attribute name { xsd:NCName },
		includedMode*,
		rule*)
		& foreign
	}
	*/
	public abstract class NvdlModeBase : NvdlAttributable
	{
		NvdlModeList includedModes = new NvdlModeList ();
		NvdlRuleList rules = new NvdlRuleList ();

//		[Map.ZeroOrMore]
		public NvdlModeList IncludedModes {
			get { return includedModes; }
		}

//		[Map.ZeroOrMore]
		public NvdlRuleList Rules {
			get { return rules; }
		}
	}

	public class NvdlNestedMode : NvdlModeBase
	{
	}

	public class NvdlMode : NvdlModeBase
	{
		string name;

//		[Map.Attribute]
//		[MapType ("NCName", XmlSchema.Namespace)]
		public string Name {
			get { return name; }
			set { name = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}
	}

	public class NvdlIncludedMode : NvdlModeBase
	{
		string name;

//		[Map.Attribute]
//		[Map.Optional]
//		[MapType ("NCName", XmlSchema.Namespace)]
		public string Name {
			get { return name; }
			set { name = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}
	}

	public enum NvdlRuleTarget {
		None,
		Elements,
		Attributes,
		Both
	}

	public abstract class NvdlRule : NvdlAttributable
	{
		NvdlRuleTarget match;
		NvdlActionList actions = new NvdlActionList ();

		public NvdlRuleTarget Match {
			get { return match; }
			set { match = value; }
		}

		public NvdlActionList Actions {
			get { return actions; }
		}
	}

	/*
	element namespace {
		(attribute ns { xsd:string },
		attribute wildCard {xsd:string{maxLength = "1"}}?,
		ruleModel)
		& foreign
	}
	*/
	public class NvdlNamespace : NvdlRule
	{
		string ns;
		string wildcard;

//		[Map.Attribute]
		public string NS {
			get { return ns; }
			set { ns = value; }
		}

//		[Map.Attribute]
		public string Wildcard {
			get { return wildcard; }
			set {
				if (value != null && value.Length > 1)
					throw new ArgumentException ("wildCard attribute can contain at most one character.");
				wildcard = value;
			}
		}
	}

	/*
	element anyNamespace { ruleModel & foreign}
	*/
	public class NvdlAnyNamespace : NvdlRule
	{
	}

	public abstract class NvdlAction : NvdlAttributable
	{
	}

	/*
	element cancelNestedActions {foreign}
	*/
	public class NvdlCancelAction : NvdlAction
	{
	}

	public abstract class NvdlNoCancelAction : NvdlAction
	{
		NvdlModeUsage modeUsage;
		string messageAttr;
		NvdlMessageList messages = new NvdlMessageList ();

		public NvdlModeUsage ModeUsage {
			get { return modeUsage; }
			set { modeUsage = value; }
		}

		public string SimpleMessage {
			get { return messageAttr; }
			set { messageAttr = value; }
		}

		public NvdlMessageList Messages {
			get { return messages; }
		}
	}

	public abstract class NvdlNoResultAction : NvdlNoCancelAction
	{
	}

	public enum NvdlResultType {
		Attach,
		AttachPlaceholder,
		Unwrap
	}

	public abstract class NvdlResultAction : NvdlNoCancelAction
	{
		public abstract NvdlResultType ResultType { get; }
	}

	public class NvdlAttach : NvdlResultAction
	{
		public override NvdlResultType ResultType {
			get { return NvdlResultType.Attach; }
		}
	}

	public class NvdlAttachPlaceholder : NvdlResultAction
	{
		public override NvdlResultType ResultType {
			get { return NvdlResultType.AttachPlaceholder; }
		}
	}

	public class NvdlUnwrap : NvdlResultAction
	{
		public override NvdlResultType ResultType {
			get { return NvdlResultType.Unwrap; }
		}
	}

	/*
	element validate {
		(schemaType?,
		(message | option)*,
		schema,
		modeUsage) & foreign
	}

	schema =
		attribute schema { xsd:anyURI } |
		element schema {(text | foreignElement), foreignAttribute*}
	*/
	public class NvdlValidate : NvdlNoResultAction
	{
		string schemaType;
		NvdlOptionList options = new NvdlOptionList ();
		string schemaUri;
		XmlElement schemaBody;

//		[Map.Attribute]
//		[MapType ("NCName", XmlSchema.Namespace)]
		public string SchemaType {
			get { return schemaType; }
			set { schemaType = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

		public NvdlOptionList Options {
			get { return options; }
		}

//		[MapType ("anyURI", XmlSchema.Namespace)]
		public string SchemaUri {
			get { return schemaUri; }
			set { schemaUri = value; }
		}

		public XmlElement SchemaBody {
			get { return schemaBody; }
			set { schemaBody = value; }
		}
	}

	public class NvdlAllow : NvdlNoResultAction
	{
	}

	public class NvdlReject : NvdlNoResultAction
	{
	}

	public class NvdlMessage : NvdlElementBase
	{
		string text;
		string xmlLang;
		ArrayList foreignAttributes = new ArrayList ();

		public string Text {
			get { return text; }
			set { text = value; }
		}

		public string XmlLang {
			get { return xmlLang; }
			set { xmlLang = value; }
		}

		public ArrayList ForeignAttributes {
			get { return foreignAttributes; }
		}
	}

	public class NvdlOption : NvdlAttributable
	{
		string name;
		string arg;
		string mustSupport;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Arg {
			get { return arg; }
			set { arg = value; }
		}

		public string MustSupport {
			get { return mustSupport; }
			set { mustSupport = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}
	}

	/*
	(attribute useMode { xsd:NCName } | nestedMode)?,
	element context {
		(attribute path { path },
		(attribute useMode { xsd:NCName } | nestedMode)?)
		& foreign
	}*
	*/
	public class NvdlModeUsage
	{
		string useMode;
		NvdlNestedMode nestedMode;
		NvdlContextList contexts = new NvdlContextList ();

		public string UseMode {
			get { return useMode; }
			set { useMode = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

		public NvdlNestedMode NestedMode {
			get { return nestedMode; }
			set { nestedMode = value; }
		}

		public NvdlContextList Contexts {
			get { return contexts; }
		}
	}

	public class NvdlContext : NvdlAttributable
	{
		string path;
		string useMode;
		NvdlNestedMode nestedMode;

		public string Path {
			get { return path; }
			set { path = value; }
		}

		public string UseMode {
			get { return useMode; }
			set { useMode = value != null ? value.Trim (Nvdl.Whitespaces) : null; }
		}

		public NvdlNestedMode NestedMode {
			get { return nestedMode; }
			set { nestedMode = value; }
		}
	}

	public class NvdlTriggerList : CollectionBase
	{
		public NvdlTrigger this [int i] {
			get { return (NvdlTrigger) List [i]; }
			set { List [i] = (NvdlTrigger) value; }
		}

		public void Add (NvdlTrigger item)
		{
			List.Add (item);
		}

		public void Remove (NvdlTrigger item)
		{
			List.Add (item);
		}
	}

	public class NvdlRuleList : CollectionBase
	{
		public NvdlRule this [int i] {
			get { return (NvdlRule) List [i]; }
			set { List [i] = (NvdlRule) value; }
		}

		public void Add (NvdlRule item)
		{
			List.Add (item);
		}

		public void Remove (NvdlRule item)
		{
			List.Add (item);
		}
	}

	public class NvdlModeList : CollectionBase
	{
		public NvdlModeBase this [int i] {
			get { return (NvdlModeBase) List [i]; }
			set { List [i] = (NvdlModeBase) value; }
		}

		public void Add (NvdlModeBase item)
		{
			List.Add (item);
		}

		public void Remove (NvdlModeBase item)
		{
			List.Add (item);
		}
	}

	public class NvdlContextList : CollectionBase
	{
		public NvdlContext this [int i] {
			get { return (NvdlContext) List [i]; }
			set { List [i] = (NvdlContext) value; }
		}

		public void Add (NvdlContext item)
		{
			List.Add (item);
		}

		public void Remove (NvdlContext item)
		{
			List.Add (item);
		}
	}

	public class NvdlActionList : CollectionBase
	{
		public NvdlAction this [int i] {
			get { return (NvdlAction) List [i]; }
			set { List [i] = (NvdlAction) value; }
		}

		public void Add (NvdlAction item)
		{
			List.Add (item);
		}

		public void Remove (NvdlAction item)
		{
			List.Add (item);
		}
	}

	public class NvdlOptionList : CollectionBase
	{
		public NvdlOption this [int i] {
			get { return (NvdlOption) List [i]; }
			set { List [i] = (NvdlOption) value; }
		}

		public void Add (NvdlOption item)
		{
			List.Add (item);
		}

		public void Remove (NvdlOption item)
		{
			List.Add (item);
		}
	}

	public class NvdlMessageList : CollectionBase
	{
		public NvdlMessage this [int i] {
			get { return (NvdlMessage) List [i]; }
			set { List [i] = (NvdlMessage) value; }
		}

		public void Add (NvdlMessage item)
		{
			List.Add (item);
		}

		public void Remove (NvdlMessage item)
		{
			List.Add (item);
		}
	}
}
