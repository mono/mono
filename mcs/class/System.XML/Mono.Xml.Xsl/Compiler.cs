//
// Compiler.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Policy;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;

using Mono.Xml.Xsl.Operations;
using Mono.Xml.XPath;
using Mono.Xml.Xsl.Functions;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	public class CompiledStylesheet {
		XslStylesheet style;
		Hashtable globalVariables;
		Hashtable attrSets;
		ExpressionStore exprStore;
		XmlNamespaceManager nsMgr;
		Hashtable keys;
		Hashtable outputs;
		Hashtable decimalFormats;
		
		public CompiledStylesheet (XslStylesheet style, Hashtable globalVariables, Hashtable attrSets, ExpressionStore exprStore, XmlNamespaceManager nsMgr, Hashtable keys, Hashtable outputs, Hashtable decimalFormats)
		{
			this.style = style;
			this.globalVariables = globalVariables;
			this.attrSets = attrSets;
			this.exprStore = exprStore;
			this.nsMgr = nsMgr;
			this.keys = keys;
			this.outputs = outputs;
			this.decimalFormats = decimalFormats;
		}
		public Hashtable Variables {get{return globalVariables;}}
		public XslStylesheet Style { get { return style; }}
		public ExpressionStore ExpressionStore {get{return exprStore;}}
		public XmlNamespaceManager NamespaceManager {get{return nsMgr;}}
		public Hashtable Keys {get { return keys;}}
		public Hashtable Outputs { get { return outputs; }}
		
		public XslDecimalFormat LookupDecimalFormat (QName name)
		{
			XslDecimalFormat ret = decimalFormats [name] as XslDecimalFormat;
			if (ret == null && name == QName.Empty)
				return XslDecimalFormat.Default;
			return ret;
		}
		
		public XslGeneralVariable ResolveVariable (QName name)
		{
			return (XslGeneralVariable)globalVariables [name];
		}
		
		public XslAttributeSet ResolveAttributeSet (QName name)
		{
			return (XslAttributeSet)attrSets [name];
		}
	}
	
	public class Compiler : IStaticXsltContext {
		public const string XsltNamespace = "http://www.w3.org/1999/XSL/Transform";
			
		Stack inputStack = new Stack ();
		XPathNavigator currentInput;
		
		Stack styleStack = new Stack ();
		XslStylesheet currentStyle;
		
		Hashtable globalVariables = new Hashtable ();
		Hashtable attrSets = new Hashtable ();
	
		ExpressionStore exprStore = new ExpressionStore ();
		XmlNamespaceManager nsMgr = new XmlNamespaceManager (new NameTable ());
				
		XmlResolver res;
		
		XslStylesheet rootStyle;
		Hashtable outputs = new Hashtable ();
				
		public CompiledStylesheet Compile (XPathNavigator nav, XmlResolver res, Evidence evidence)
		{
			this.parser = new XPathParser (this);
			this.res = res;
			if (res == null)
				this.res = new XmlUrlResolver ();

			if (!nav.MoveToFirstChild ()) throw new Exception ("WTF?");
				
			outputs [""] = new XslOutput ("");
				
			while (nav.NodeType != XPathNodeType.Element) nav.MoveToNext();
			
			PushInputDocument (nav);
			if (nav.MoveToFirstNamespace (XPathNamespaceScope.ExcludeXml))
			{
				do {
					nsMgr.AddNamespace (nav.LocalName, nav.Value);
				} while (nav.MoveToNextNamespace (XPathNamespaceScope.ExcludeXml));
				nav.MoveToParent ();
			}
			this.rootStyle = new XslStylesheet (this);
			
			return new CompiledStylesheet (rootStyle, globalVariables, attrSets, exprStore, nsMgr, rootStyle.Keys, outputs, decimalFormats);
		}
		
#region Input
		public XPathNavigator Input {
			get { return currentInput; }
		}
		
		public XslStylesheet CurrentStylesheet {
			get { return currentStyle; }
		}
		
		public void PushStylesheet (XslStylesheet style)
		{
			if (currentStyle != null) styleStack.Push (currentStyle);
			currentStyle = style;
		}
		
		public void PopStylesheet ()
		{
			if (styleStack.Count == 0)
				currentStyle = null;
			else
				currentStyle = (XslStylesheet)styleStack.Pop ();
		}
		
		public void PushInputDocument (string url)
		{
			// todo: detect recursion
			Uri absUri = res.ResolveUri (new Uri (Input.BaseURI), url);
			using (Stream s = (Stream)res.GetEntity (absUri, null, typeof(Stream)))
			{

				XPathNavigator n = new XPathDocument (new XmlTextReader (absUri.ToString (), s)).CreateNavigator ();
				n.MoveToFirstChild ();
				PushInputDocument (n);
			}
		}
		
		private void PushInputDocument (XPathNavigator nav)
		{
			inputStack.Push (currentInput);
			currentInput = nav;
		}
		
		public void PopInputDocument ()
		{
			currentInput = (XPathNavigator)inputStack.Pop ();
		}
		
		public QName ParseQNameAttribute (string localName)
		{
			return ParseQNameAttribute (localName, String.Empty);
		}
		public QName ParseQNameAttribute (string localName, string ns)
		{
			return XslNameUtil.FromString (Input.GetAttribute (localName, ns), Input);
		}
		
		public QName [] ParseQNameListAttribute (string localName)
		{
			return ParseQNameListAttribute (localName, String.Empty);
		}
		
		public QName [] ParseQNameListAttribute (string localName, string ns)
		{
			string s = GetAttribute (localName, ns);
			if (s == null) return null;
				
			string [] names = s.Split (new char [] {' ', '\r', '\n', '\t'});
			QName [] ret = new QName [names.Length];
			
			for (int i = 0; i < names.Length; i++)
				ret [i] = XslNameUtil.FromString (names [i], Input);
			
			return ret;
		}
		
		public bool ParseYesNoAttribute (string localName, bool defaultVal)
		{
			return ParseYesNoAttribute (localName, String.Empty, defaultVal);
		}
		
		public bool ParseYesNoAttribute (string localName, string ns, bool defaultVal)
		{
			string s = GetAttribute (localName, ns);

			switch (s) {
			case null: return defaultVal;
			case "yes": return true;
			case "no": return false;
			default: throw new Exception ("invalid value for " + localName);
			}
		}
		
		public string GetAttribute (string localName)
		{
			return GetAttribute (localName, String.Empty);
		}
		
		public string GetAttribute (string localName, string ns)
		{
			if (!Input.MoveToAttribute (localName, ns))
				return null;
			
			string ret = Input.Value;
			Input.MoveToParent ();
			return ret;
		}
		public XslAvt ParseAvtAttribute (string localName)
		{
			return ParseAvtAttribute (localName, String.Empty);
		}
		public XslAvt ParseAvtAttribute (string localName, string ns)
		{
			return ParseAvt (GetAttribute (localName, ns));
		}
		
		public void AssertAttribute (string localName)
		{
			AssertAttribute (localName, "");
		}
		public void AssertAttribute (string localName, string ns)
		{
			if (Input.GetAttribute (localName, ns) == null)
				throw new Exception ("Was expecting the " + localName + " attribute.");
		}
		
		public XslAvt ParseAvt (string s)
		{
			if (s == null) return null;
			return new XslAvt (s, this);
		}
		
		
#endregion
#region Compile
		public Pattern CompilePattern (string pattern)
		{
			if (pattern == null || pattern == "") return null;
			Pattern p = Pattern.Compile (pattern, this);
			
			exprStore.AddPattern (p, this);
			
			return p;
		}

		internal XPathParser parser;
		public XPathExpression CompileExpression (string expression)
		{
			if (expression == null || expression == "") return null;

			XPathExpression e = new CompiledExpression (parser.Compile (expression));
			
			exprStore.AddExpression (e, this);
			
			return e;
		}
		
		public XslOperation CompileTemplateContent ()
		{
			return new XslTemplateContent (this);
		}
#endregion
#region Variables
		public void AddGlobalVariable (XslGlobalVariable var)
		{
			globalVariables [var.Name] = var;
		}
		
		public void AddAttributeSet (XslAttributeSet set)
		{
			XslAttributeSet existing = attrSets [set.Name] as XslAttributeSet;
			// The latter set will have higher priority
			if (existing != null) {
				existing.Merge (set);
				attrSets [set.Name] = existing;
			}
			else
				attrSets [set.Name] = set;
		}
		
		VariableScope curVarScope;
		
		public void PushScope ()
		{
			curVarScope = new VariableScope (curVarScope);
		}
		
		public VariableScope PopScope ()
		{
			curVarScope.giveHighTideToParent ();
			VariableScope cur = curVarScope;
			curVarScope = curVarScope.Parent;
			return cur;
		}
		
		public int AddVariable (XslLocalVariable v)
		{
			if (curVarScope == null)
				throw new Exception ("not inited");
			
			return curVarScope.AddVariable (v);
		}
		
		public void AddSort (XPathExpression e, Sort s)
		{
			exprStore.AddSort (e, s);
		}
		public VariableScope CurrentVariableScope { get { return curVarScope; }}
#endregion
		
#region Scope (version, {excluded, extension} namespaces)
		[MonoTODO ("This will work, but is *very* slow")]
		public bool IsExtensionNamespace (string nsUri)
		{
			if (nsUri == XsltNamespace) return true;
				
			XPathNavigator nav = Input.Clone ();
			XPathNavigator nsScope = nav.Clone ();
			do {
				bool isXslt = nav.NamespaceURI == XsltNamespace;
				nsScope.MoveTo (nav);
				if (nav.MoveToFirstAttribute ()) {
					do {
						if (nav.LocalName == "extension-element-prefixes" &&
							nav.NamespaceURI == (isXslt ? String.Empty : XsltNamespace))
						{
						
							foreach (string ns in nav.Value.Split (' '))
								if (nsScope.GetNamespace (ns == "#default" ? "" : ns) == nsUri)
									return true;
						}
					} while (nav.MoveToNextAttribute ());
					nav.MoveToParent ();
				}
			} while (nav.MoveToParent ());
				
			return false;
		}
		
		public Hashtable GetNamespacesToCopy ()
		{
			Hashtable ret = new Hashtable ();
			
			XPathNavigator nav = Input.Clone ();
			XPathNavigator nsScope = nav.Clone ();
			
			if (nav.MoveToFirstNamespace (XPathNamespaceScope.Local)) {
				do {
					if (nav.Value != XsltNamespace && !ret.Contains (nav.Name))
						ret.Add (nav.Name, nav.Value);
				} while (nav.MoveToNextNamespace (XPathNamespaceScope.Local));
				nav.MoveToParent ();
			}
			
			do {
				bool isXslt = nav.NamespaceURI == XsltNamespace;
				nsScope.MoveTo (nav);

				if (nav.MoveToFirstAttribute())	{
					do {
						if ((nav.LocalName == "extension-element-prefixes" || nav.LocalName == "exclude-result-prefixes") &&
							nav.NamespaceURI == (isXslt ? String.Empty : XsltNamespace))
						{
							foreach (string ns in nav.Value.Split (' ')) {
								string realNs = ns == "#default" ? "" : ns;
								
								if ((string)ret [realNs] == nsScope.GetNamespace (realNs))
									ret.Remove (realNs);
							}
						}
					} while (nav.MoveToNextAttribute ());
					nav.MoveToParent();
				}
			} while (nav.MoveToParent ());
			
			return ret;
		}
#endregion
		
#region Decimal Format
		Hashtable decimalFormats = new Hashtable ();
		
		public void CompileDecimalFormat ()
		{
			QName nm = ParseQNameAttribute ("name");
			
			if (decimalFormats.Contains (nm))
				((XslDecimalFormat)decimalFormats [nm]).CheckSameAs (this);
			else
				decimalFormats [nm] = new XslDecimalFormat (this);
		}
#endregion
#region Static XSLT context
		Expression IStaticXsltContext.TryGetVariable (string nm)
		{
			if (curVarScope == null)
				return null;
			
			XslLocalVariable var = curVarScope.ResolveStatic (XslNameUtil.FromString (nm, Input));
			
			if (var == null)
				return null;
			
			return new XPathVariableBinding (var);
		}
		
		Expression IStaticXsltContext.TryGetFunction (QName name, FunctionArguments args)
		{
			if (name.Namespace != null && name.Namespace != "")
				return null;
			
			switch (name.Name) {
				case "current": return new XsltCurrent (args);
				case "unparsed-entity-uri": return new XsltUnparsedEntityUri (args);
				case "element-available": return new XsltElementAvailable (args, this);
				case "system-property": return new XsltSystemProperty (args, this);
				case "function-available": return new XsltFunctionAvailable (args, this);
				case "generate-id": return new XsltGenerateId (args);
				case "format-number": return new XsltFormatNumber (args, this);
				case "key": return new XsltKey (args, this);
				case "document": return new XsltDocument (args, this);
			}
			
			return null;
		}
		
		QName IStaticXsltContext.LookupQName (string s)
		{
			return XslNameUtil.FromString (s, Input);
		}
		
		XmlNamespaceManager IStaticXsltContext.GetNsm ()
		{
			return new XPathNavigatorNsm (Input);
		}
		
		public XmlNamespaceManager GetNsm ()
		{
			return new XPathNavigatorNsm (Input);
		}
#endregion
		public void CompileOutput ()
		{
			XPathNavigator n = Input;
			string uri = n.GetAttribute ("href", "");
			XslOutput output = outputs [uri] as XslOutput;
			if (output == null) {
				output = new XslOutput (uri);
				outputs.Add (uri, output);
			}
			output.Fill (n);
		}
	}
	
	public class VariableScope {
		Hashtable variables;
		VariableScope parent;
		int nextSlot = 0;
		int highTide = 0; // this will be the size of the stack frame
		
		internal void giveHighTideToParent ()
		{
			if (parent != null)
				parent.highTide = System.Math.Max (VariableHighTide, parent.VariableHighTide);
		}

		public int VariableHighTide { get { return  System.Math.Max (highTide, nextSlot); }}
		
		public VariableScope (VariableScope parent)
		{
			this.parent = parent;
			if (parent != null)
				this.nextSlot = parent.nextSlot;
		}
		
		public VariableScope Parent { get { return parent; }}
		
		public int AddVariable (XslLocalVariable v)
		{
			if (variables == null)
				variables = new Hashtable ();
			
			variables [v.Name] = v;
			return nextSlot++;
		}
		
		public XslLocalVariable ResolveStatic (QName name)
		{
			for (VariableScope s = this; s != null; s = s.Parent) {
				if (s.variables == null) continue;
				XslLocalVariable v = s.variables [name] as XslLocalVariable;
				if (v != null) return v;
			}
			return null;
		}
		
		public XslLocalVariable Resolve (XslTransformProcessor p, QName name)
		{
			for (VariableScope s = this; s != null; s = s.Parent) {
				if (s.variables == null) continue;
				XslLocalVariable v = s.variables [name] as XslLocalVariable;
				if (v != null && v.IsEvaluated (p))
					return v;

			}
			return null;
		}
	}
	
	public class Sort {
		string lang;
		XmlDataType dataType;
		XmlSortOrder order;
		XmlCaseOrder caseOrder;
		
		XslAvt langAvt, dataTypeAvt, orderAvt, caseOrderAvt;
		XPathExpression expr;
			
		public Sort (Compiler c)
		{
			expr = c.CompileExpression (c.GetAttribute ("select"));
			if (expr == null)
				expr = c.CompileExpression ("string(.)");
			
			langAvt = c.ParseAvtAttribute ("lang");
			dataTypeAvt = c.ParseAvtAttribute ("data-type");
			orderAvt = c.ParseAvtAttribute ("order");
			caseOrderAvt = c.ParseAvtAttribute ("case-order");
			
			// Precalc whatever we can
			lang = ParseLang (XslAvt.AttemptPreCalc (ref langAvt));
			dataType = ParseDataType (XslAvt.AttemptPreCalc (ref dataTypeAvt));
			order = ParseOrder (XslAvt.AttemptPreCalc (ref orderAvt));
			caseOrder = ParseCaseOrder (XslAvt.AttemptPreCalc (ref caseOrderAvt));
		}
		
		
		string ParseLang (string value)
		{
			return value;
		}

		XmlDataType ParseDataType (string value)
		{
			switch (value)
			{
			case "number":
				return XmlDataType.Number;
			case "text":
			case null:
			default:
				return XmlDataType.Text;
			}
		}

		XmlSortOrder ParseOrder (string value)
		{
			switch (value)
			{
			case "descending":
				return XmlSortOrder.Descending;
			case "ascending":
			case null:
			default:
				return XmlSortOrder.Ascending;
			}	  
		}

		XmlCaseOrder ParseCaseOrder (string value)
		{
			switch (value)
			{
			case "upper-first":
				return XmlCaseOrder.UpperFirst;
			case "lower-first":
				return XmlCaseOrder.LowerFirst;
			case null:
			default:
				return XmlCaseOrder.None;
			}	  
		}
		
		
		public void AddToExpr (XPathExpression e, XslTransformProcessor p)
		{
			e.AddSort (
				expr,
				orderAvt == null ? order : ParseOrder (orderAvt.Evaluate (p)),
				caseOrderAvt == null ? caseOrder: ParseCaseOrder (caseOrderAvt.Evaluate (p)),
				langAvt == null ? lang : ParseLang (langAvt.Evaluate (p)),
				dataTypeAvt == null ? dataType : ParseDataType (dataTypeAvt.Evaluate (p))
			);
		}
	}
	
	public class ExpressionStore {
		Hashtable exprToSorts;
		
		public void AddExpression (XPathExpression e, Compiler c)
		{		
		}
		
		public void AddPattern (Pattern p, Compiler c)
		{
		}
		
		public void AddSort (XPathExpression e, Sort s)
		{
			if (exprToSorts == null)
				exprToSorts = new Hashtable ();
			
			if (exprToSorts.Contains (e))
				((ArrayList)exprToSorts [e]).Add (s);
			else {
				ArrayList a = new ArrayList ();
				a.Add (s);
				exprToSorts [e] = a;
			}
		}
		
		public XPathExpression PrepForExecution (XPathExpression e, XslTransformProcessor p)
		{
			if (exprToSorts != null && exprToSorts.Contains (e))
			{
				XPathExpression expr = e.Clone ();
				foreach (Sort s in (ArrayList)exprToSorts [e])
					s.AddToExpr (expr,p);
				return expr;
			}
			return e;
		}
		
		public bool PatternMatches (Pattern p, XslTransformProcessor proc, XPathNavigator n)
		{
			return p.Matches (n, proc.XPathContext);
		}
	}
	
	public class XslNameUtil
	{
		public static QName FromString (string name, XPathNavigator current)
		{
			if (current.NodeType == XPathNodeType.Attribute)
				(current = current.Clone ()).MoveToParent ();
			
			int colon = name.IndexOf (':');
			if (colon > 0)
				return new QName (name.Substring (colon+ 1), current.GetNamespace (name.Substring (0, colon)));
			else if (colon < 0)
				// Default namespace is not used for unprefixed names.
				return new QName (name, "");
			else
				throw new ArgumentException ("Invalid name: " + name);
		}
		
		public static QName FromString (string name, XmlNamespaceManager ctx)
		{
			int colon = name.IndexOf (':');
			if (colon > 0)
				return new QName (name.Substring (colon + 1), ctx.LookupNamespace (name.Substring (0, colon)));
			else if (colon < 0)
				// Default namespace is not used for unprefixed names.
				return new QName (name, "");
			else
				throw new ArgumentException ("Invalid name: " + name);
		}
		
		public static string LocalNameOf (string name)
		{
			int colon = name.IndexOf (':');
			if (colon > 0)
				return name.Substring (colon + 1);
			else if (colon < 0)
				return name;
			else
				throw new ArgumentException ("Invalid name: " + name);
		}
	}
	
	public class XPathNavigatorNsm : XmlNamespaceManager {
		XPathNavigator nsScope;
		
		public XPathNavigatorNsm (XPathNavigator n) : base () {
			nsScope = n.Clone ();
			if (nsScope.NodeType == XPathNodeType.Attribute)
				nsScope.MoveToParent ();
		}
		
		public override string DefaultNamespace { get { return String.Empty; }}

		public override string LookupNamespace (string prefix)
		{
			if (prefix == "" || prefix == null)
				return "";
			
			return nsScope.GetNamespace (prefix);
		}
	}
}