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
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;

using Mono.Xml.Xsl.Operations;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	public class CompiledStylesheet	{
		XslStylesheet style;
		Hashtable globalVariables;
		Hashtable attrSets;
		ExpressionStore exprStore;
		XmlNamespaceManager nsMgr;
		ArrayList keys;
		
		public CompiledStylesheet (XslStylesheet style, Hashtable globalVariables, Hashtable attrSets, ExpressionStore exprStore, XmlNamespaceManager nsMgr, ArrayList keys)
		{
			this.style = style;
			this.globalVariables = globalVariables;
			this.attrSets = attrSets;
			this.exprStore = exprStore;
			this.nsMgr = nsMgr;
			this.keys = keys;
		}
		public Hashtable Variables {get{return globalVariables;}}
		public XslStylesheet Style { get { return style; }}
		public ExpressionStore ExpressionStore {get{return exprStore;}}
		public XmlNamespaceManager NamespaceManager {get{return nsMgr;}}
		public ArrayList Keys {get { return keys;}}
		
		public XslGeneralVariable ResolveVariable (QName name)
		{
			return (XslGeneralVariable)globalVariables [name];
		}
		
		public XslAttributeSet ResolveAttributeSet (QName name)
		{
			return (XslAttributeSet)attrSets [name];
		}
	}
	
	public class Compiler {
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
				
		public CompiledStylesheet Compile (XPathNavigator nav, XmlResolver res)
		{
			this.res = res;
			if (!nav.MoveToFirstChild ()) throw new Exception ("WTF?");
				
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
			
			return new CompiledStylesheet (rootStyle, globalVariables, attrSets, exprStore, nsMgr, keys);
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
			if (s == null || s == "") return null;
				
			string [] names = s.Split (new char [] {' ', '\r', '\n', '\t'});
			QName [] ret = new QName [names.Length];
			
			for (int i = 0; i < names.Length; i++)
				ret [i] = XslNameUtil.FromString (names [i], Input);
			
			return ret;
		}
		
		public string GetAttribute (string localName)
		{
			return GetAttribute (localName, String.Empty);
		}
		
		public string GetAttribute (string localName, string ns)
		{
			return Input.GetAttribute (localName, ns);
		}
		public XslAvt ParseAvtAttribute (string localName)
		{
			return ParseAvtAttribute (localName, String.Empty);
		}
		public XslAvt ParseAvtAttribute (string localName, string ns)
		{
			return ParseAvt (Input.GetAttribute (localName, ns));
		}
		
		public void AssertAttribute (string localName)
		{
			AssertAttribute (localName, "");
		}
		public void AssertAttribute (string localName, string ns)
		{
			if (Input.GetAttribute (localName, ns) == String.Empty)
				throw new Exception ("Was expecting the " + localName + " attribute.");
		}
		
		public XslAvt ParseAvt (string s)
		{
			if (s == null) return null;
			return new XslAvt (s, this);
		}
		
		
#endregion
#region Compile
		public XPathExpression CompilePattern (string pattern)
		{
			return CompileExpression (pattern); // TODO validate, get priority
		}
		
		public XPathExpression CompileExpression (string expression)
		{
			if (expression == null || expression == "") return null;
			XPathExpression e = Input.Compile (expression);
			
			
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
			if (existing != null)
				set.Merge (existing);
			
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
#region Key
		ArrayList keys = new ArrayList ();
		
		public void AddKeyPattern (XslKey key)
		{
			keys.Add (key);
		}
#endregion
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
		
		public XslLocalVariable Resolve (XslTransformProcessor p, QName name)
		{
			for (VariableScope s = this; s != null; s = s.Parent)			
				if (s.variables != null && s.variables.Contains (name)) {
					XslLocalVariable v = (XslLocalVariable)s.variables [name];								
					if (v.IsEvaluated (p))
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
	
		Hashtable exprToVarCtx = new Hashtable ();
		Hashtable exprToSorts = new Hashtable ();
		
		public void AddExpression (XPathExpression e, Compiler c)
		{
			exprToVarCtx [e] = c.CurrentVariableScope;
		}
		
		public void AddSort (XPathExpression e, Sort s)
		{
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
			XPathExpression expr = e.Clone ();

			expr.SetContext (new XsltCompiledContext (p, (VariableScope)exprToVarCtx [e]));
			if (exprToSorts.Contains (e))
			{
				foreach (Sort s in (ArrayList)exprToSorts [e])
					s.AddToExpr (expr,p);
			}
			return expr;
		}
	}
	
	public class XslNameUtil
	{
		public static QName FromString (string name, XPathNavigator current)
		{
			int colon = name.IndexOf (':');
			if (colon > 0)
				return new QName (name.Substring (colon, name.Length - colon), current.GetNamespace (name.Substring (0, colon)));
			else if (colon < 0)
				return new QName (name, current.GetNamespace (""));
			else
				throw new ArgumentException ("Invalid name: " + name);
		}
		
		public static QName FromString (string name, XmlNamespaceManager ctx)
		{
			int colon = name.IndexOf (':');
			if (colon > 0)
				return new QName (name.Substring (colon, name.Length - colon), ctx.LookupNamespace (name.Substring (0, colon)));
			else if (colon < 0)
				return new QName (name, ctx.LookupNamespace (""));
			else
				throw new ArgumentException ("Invalid name: " + name);
		}
		
		public static string LocalNameOf (string name)
		{
			int colon = name.IndexOf (':');
			if (colon > 0)
				return name.Substring (colon, name.Length - colon);
			else if (colon < 0)
				return name;
			else
				throw new ArgumentException ("Invalid name: " + name);
		}
	}
}