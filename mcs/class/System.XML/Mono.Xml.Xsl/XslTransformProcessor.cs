//
// XslTransformProcessor.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
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
using System.IO;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.Xsl.Operations;
using Mono.Xml.XPath;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	internal class XslTransformProcessor {
		XsltDebuggerWrapper debugger;
		
		CompiledStylesheet compiledStyle;
		
		XslStylesheet style;
		
		Stack currentTemplateStack = new Stack ();
		
		XPathNavigator root;
		XsltArgumentList args;
		XmlResolver resolver;
//		bool outputStylesheetXmlns;
		string currentOutputUri;
		
		internal readonly XsltCompiledContext XPathContext;

		// Store the values of global params
		internal Hashtable globalVariableTable = new Hashtable ();
		
		public XslTransformProcessor (CompiledStylesheet style, object debugger)
		{
			this.XPathContext = new XsltCompiledContext (this);
			this.compiledStyle = style;
			this.style = style.Style;
			if (debugger != null)
				this.debugger = new XsltDebuggerWrapper (debugger);
		}

		public void Process (XPathNavigator root, Outputter outputtter, XsltArgumentList args, XmlResolver resolver)
		{
			this.args = args;
			this.root = root;
			this.resolver = resolver != null ? resolver : new XmlUrlResolver ();
//			this.outputStylesheetXmlns = true;
			this.currentOutputUri = String.Empty;

			PushNodeset (new SelfIterator (root, this.XPathContext));
CurrentNodeset.MoveNext ();
			
			// have to evaluate the params first, as Global vars may
			// be dependant on them
			if (args != null)
			{
				foreach (XslGlobalVariable v in CompiledStyle.Variables.Values)
				{
					if (v is XslGlobalParam)
					{
						object p = args.GetParam(v.Name.Name, v.Name.Namespace);
						if (p != null)
							((XslGlobalParam)v).Override (this, p);

						v.Evaluate (this);
					}
				}
			}

			foreach (XslGlobalVariable v in CompiledStyle.Variables.Values)	{
				if (args == null || !(v is XslGlobalParam)) {
					v.Evaluate (this);
				}
			}
			
			PopNodeset ();
			
			this.PushOutput (outputtter);
			this.ApplyTemplates (new SelfIterator (root, this.XPathContext), QName.Empty, null);
			this.PopOutput ();
		}

		public XsltDebuggerWrapper Debugger {
			get { return debugger; }
		}

		public CompiledStylesheet CompiledStyle { get { return compiledStyle; }}
		public XsltArgumentList Arguments {get{return args;}}

		public XPathNavigator Root { get { return root; } }

		public MSXslScriptManager ScriptManager {
			get { return compiledStyle.ScriptManager; }
		}

		#region Document Resolution
		public XmlResolver Resolver {get{return resolver;}}
		
		Hashtable docCache;
		
		public XPathNavigator GetDocument (Uri uri)
		{
			XPathNavigator result;
			
			if (docCache != null) {
				result = docCache [uri] as XPathNavigator;
				if (result != null)
					return result.Clone();
			} else {
				docCache = new Hashtable();
			}

			XmlReader rdr = null;
			try {
				rdr = new XmlTextReader (uri.ToString(), (Stream) resolver.GetEntity (uri, null, null), root.NameTable);
				XmlValidatingReader xvr = new XmlValidatingReader (rdr);
				xvr.ValidationType = ValidationType.None;
				result = new XPathDocument (xvr, XmlSpace.Preserve).CreateNavigator ();
			} finally {
				if (rdr != null)
					rdr.Close ();
			}
			docCache [uri] = result.Clone ();
			return result;
		}
		
		#endregion
		
		#region Output
		Stack outputStack = new Stack ();
		
		public Outputter Out { get { return (Outputter)outputStack.Peek(); }}
		
		public void PushOutput (Outputter newOutput)
		{
			this.outputStack.Push (newOutput);
		}
		
		public Outputter PopOutput ()
		{
			Outputter ret = (Outputter)this.outputStack.Pop ();
			ret.Done ();
			return ret;
		}
		
		public Hashtable Outputs { get { return compiledStyle.Outputs; }}

		public XslOutput Output { get { return Outputs [currentOutputUri] as XslOutput; } }

		public string CurrentOutputUri { get { return currentOutputUri; } }

		public bool InsideCDataElement { get { return this.XPathContext.IsCData; } }
		#endregion
		
		#region AVT StringBuilder
		StringBuilder avtSB;
	
		#if DEBUG
		bool avtSBlock = false;
		#endif
	
		public StringBuilder GetAvtStringBuilder ()
		{
			#if DEBUG
				if (avtSBlock)
					throw new XsltException ("String Builder was locked", null);
				avtSBlock = true;
			#endif
			
			if (avtSB == null)
				avtSB = new StringBuilder ();
			
			return avtSB;
		}
		
		public string ReleaseAvtStringBuilder ()
		{
			#if DEBUG
				if (!avtSBlock)
					throw new XsltException ("you never locked the string builder", null);
				avtSBlock = false;
			#endif
			
			string ret = avtSB.ToString ();
			avtSB.Length = 0;
			return ret;
		}
		#endregion
		
		#region Templates -- Apply/Call
		Stack paramPassingCache = new Stack ();
		
		Hashtable GetParams (ArrayList withParams)
		{
			if (withParams == null) return null;
			Hashtable ret;
			
			if (paramPassingCache.Count != 0) {
				ret = (Hashtable)paramPassingCache.Pop ();
				ret.Clear ();
			} else
				ret = new Hashtable ();
			
			int len = withParams.Count;
			for (int i = 0; i < len; i++) {
				XslVariableInformation param = (XslVariableInformation)withParams [i];
				ret.Add (param.Name, param.Evaluate (this));
			}
			return ret;
		}
		
		public void ApplyTemplates (XPathNodeIterator nodes, QName mode, ArrayList withParams)
		{

			Hashtable passedParams = GetParams (withParams);
			
			while (NodesetMoveNext (nodes)) {
				PushNodeset (nodes);
				XslTemplate t = FindTemplate (CurrentNode, mode);
				currentTemplateStack.Push (t);
				t.Evaluate (this, passedParams);
				currentTemplateStack.Pop ();
				PopNodeset ();
			}
			
			if (passedParams != null) paramPassingCache.Push (passedParams);
		}
		
		public void CallTemplate (QName name, ArrayList withParams)
		{
			Hashtable passedParams = GetParams (withParams);
			
			XslTemplate t = FindTemplate (name);
			currentTemplateStack.Push (null);
			t.Evaluate (this, passedParams);
			currentTemplateStack.Pop ();
			
			if (passedParams != null) paramPassingCache.Push (passedParams);
		}
		
		public void ApplyImports ()
		{	

			XslTemplate currentTemplate = (XslTemplate)currentTemplateStack.Peek();
			if (currentTemplate == null)
				throw new XsltException ("Invalid context for apply-imports", null, CurrentNode);
			XslTemplate t;
			
			for (int i = currentTemplate.Parent.Imports.Count - 1; i >= 0; i--) {
				XslStylesheet s = (XslStylesheet)currentTemplate.Parent.Imports [i];
				t = s.Templates.FindMatch (CurrentNode, currentTemplate.Mode, this);
				if (t != null) {					
					currentTemplateStack.Push (t);
					t.Evaluate (this);
					currentTemplateStack.Pop ();
					return;
				}
			}
			
			switch (CurrentNode.NodeType) {
			case XPathNodeType.Root:
			case XPathNodeType.Element:
				if (currentTemplate.Mode == QName.Empty)
					t = XslDefaultNodeTemplate.Instance;
				else
					t = new XslDefaultNodeTemplate(currentTemplate.Mode);
			
				break;
			case XPathNodeType.Attribute:
			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Text:
			case XPathNodeType.Whitespace:
				t = XslDefaultTextTemplate.Instance;
				break;
			
			case XPathNodeType.Comment:
			case XPathNodeType.ProcessingInstruction:
				t = XslEmptyTemplate.Instance;
				break;
			
			default:
				t = XslEmptyTemplate.Instance;
				break;
			}
			currentTemplateStack.Push (t);
			t.Evaluate (this);
			currentTemplateStack.Pop ();
		}

		// Outputs Literal namespace nodes described in spec 7.7.1
		internal void OutputLiteralNamespaceUriNodes (Hashtable nsDecls, ArrayList excludedPrefixes, string localPrefixInCopy)
		{
			if (nsDecls == null)
				return;

			foreach (DictionaryEntry cur in nsDecls) {
				string name = (string)cur.Key;
				string value = (string)cur.Value;

				// See XSLT 1.0 errata E25
				if (localPrefixInCopy == name)
					continue;
				if (localPrefixInCopy != null &&
					name.Length == 0 &&
					XPathContext.ElementNamespace.Length == 0)
					continue;

				// exclude-result-prefixes, see the spec 7.1.1
				bool skip = false;
				if (style.ExcludeResultPrefixes != null) {
					foreach (XmlQualifiedName exc in style.ExcludeResultPrefixes) {
						if (exc.Namespace == value) {
							skip = true;
							continue;
						}
					}
				}
				if (skip)
					continue;

				if (style.NamespaceAliases [name] != null)
					continue;

				switch (value) {//FIXME: compare names by reference
				case "http://www.w3.org/1999/XSL/Transform":
//					if ("xsl" == name)
						continue;
//					else
//						goto default;
				case XmlNamespaceManager.XmlnsXml:
					if (XmlNamespaceManager.PrefixXml == name)
						continue;
					else
						goto default;
				case XmlNamespaceManager.XmlnsXmlns:
					if (XmlNamespaceManager.PrefixXmlns == name)
						continue;
					else
						goto default;
				default:
					if (excludedPrefixes == null || !excludedPrefixes.Contains (name))
						Out.WriteNamespaceDecl (name, value);
					break;
				}
			}
		}

		XslTemplate FindTemplate (XPathNavigator node, QName mode)
		{
			XslTemplate ret = style.Templates.FindMatch (CurrentNode, mode, this);
			
			if (ret != null) return ret;

			switch (node.NodeType) {
			case XPathNodeType.Root:
			case XPathNodeType.Element:
				if (mode == QName.Empty)
					return XslDefaultNodeTemplate.Instance;
				else
					return new XslDefaultNodeTemplate(mode);
			
			case XPathNodeType.Attribute:
			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Text:
			case XPathNodeType.Whitespace:
				return XslDefaultTextTemplate.Instance;
			
			case XPathNodeType.Comment:
			case XPathNodeType.ProcessingInstruction:
				return XslEmptyTemplate.Instance;
			
			default:
				return XslEmptyTemplate.Instance;
			}
		}
		
		XslTemplate FindTemplate (QName name)
		{
			XslTemplate ret = style.Templates.FindTemplate (name);
			if (ret != null) return ret;
				
			throw new XsltException ("Could not resolve named template " + name, null, CurrentNode);
		}
		
		#endregion

		public void PushForEachContext ()
		{
			currentTemplateStack.Push (null);
		}
		
		public void PopForEachContext ()
		{
			currentTemplateStack.Pop ();
		}
		

		#region Nodeset Context
		ArrayList nodesetStack = new ArrayList ();
		
		public XPathNodeIterator CurrentNodeset {
			get { return (XPathNodeIterator) nodesetStack [nodesetStack.Count - 1]; }
		}
		
		public XPathNavigator CurrentNode {
			get {
				XPathNavigator nav = CurrentNodeset.Current;
				if (nav != null)
					return nav;
				// Inside for-each context, CurrentNodeset.Current may be null
				for (int i = nodesetStack.Count - 2; i >= 0; i--) {
					nav = ((XPathNodeIterator) nodesetStack [i]).Current;
					if (nav != null)
						return nav;
				}
				return null;
			}
		}
		
		public bool NodesetMoveNext ()
		{
			return NodesetMoveNext (CurrentNodeset);
		}

		public bool NodesetMoveNext (XPathNodeIterator iter)
		{
			if (!iter.MoveNext ())
				return false;
			// FIXME: this check should not be required.
			// Since removal of this check causes some regressions,
			// there should be some wrong assumption on our
			// BaseIterator usage. Actually BaseIterator should
			// not do whitespace check and every PreserveWhitespace
			// evaluation in XslTransform should be done at
			// different level. One possible solution is to wrap
			// the input XmlReader by a new XmlReader that takes
			// whitespace stripping into consideration.
			if (iter.Current.NodeType == XPathNodeType.Whitespace && !XPathContext.PreserveWhitespace (iter.Current))
				return NodesetMoveNext (iter);
			return true;
		}

		public void PushNodeset (XPathNodeIterator itr)
		{
			BaseIterator bi = itr as BaseIterator;
			bi = bi != null ? bi : new WrapperIterator (itr, null);
			bi.NamespaceManager = XPathContext;
			nodesetStack.Add (bi);
		}
		
		public void PopNodeset ()
		{
			nodesetStack.RemoveAt (nodesetStack.Count - 1);
		}
		#endregion
		
		#region Evaluate
		
		public bool Matches (Pattern p, XPathNavigator n)
		{
			return p.Matches (n, this.XPathContext);
		}
		
		public object Evaluate (XPathExpression expr)
		{
			XPathNodeIterator itr = CurrentNodeset;
			BaseIterator bi = (BaseIterator) itr;
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (bi.NamespaceManager == null)
				bi.NamespaceManager = cexpr.NamespaceManager;
			return cexpr.Evaluate (bi);
		}
		
		public string EvaluateString (XPathExpression expr)
		{
			XPathNodeIterator itr = CurrentNodeset;
#if true
			return itr.Current.EvaluateString (expr, itr, XPathContext);
#else
			BaseIterator bi = (BaseIterator) itr;
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (bi.NamespaceManager == null)
				bi.NamespaceManager = cexpr.NamespaceManager;
			return cexpr.EvaluateString (bi);
#endif
		}
				
		public bool EvaluateBoolean (XPathExpression expr)
		{
			XPathNodeIterator itr = CurrentNodeset;
#if true
			return itr.Current.EvaluateBoolean (expr, itr, XPathContext);
#else
			BaseIterator bi = (BaseIterator) itr;
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (bi.NamespaceManager == null)
				bi.NamespaceManager = cexpr.NamespaceManager;
			return cexpr.EvaluateBoolean (bi);
#endif
		}
		
		public double EvaluateNumber (XPathExpression expr)
		{
			XPathNodeIterator itr = CurrentNodeset;
#if true
			return itr.Current.EvaluateNumber (expr, itr, XPathContext);
#else
			BaseIterator bi = (BaseIterator) itr;
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (bi.NamespaceManager == null)
				bi.NamespaceManager = cexpr.NamespaceManager;
			return cexpr.EvaluateNumber (bi);
#endif
		}
		
		public XPathNodeIterator Select (XPathExpression expr)
		{
#if true
			return CurrentNodeset.Current.Select (expr, XPathContext);
#else
			BaseIterator bi = (BaseIterator) CurrentNodeset;
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (bi.NamespaceManager == null)
				bi.NamespaceManager = cexpr.NamespaceManager;
			return cexpr.EvaluateNodeSet (bi);
#endif
		}
		
		#endregion
		
		public XslAttributeSet ResolveAttributeSet (QName name)
		{
			return CompiledStyle.ResolveAttributeSet (name);
		}
		
		#region Variable Stack
		Stack variableStack = new Stack ();
		object [] currentStack;
		public int StackItemCount {
			get {
				if (currentStack == null)
				      return 0;
				for (int i = 0; i < currentStack.Length; i++)
					if (currentStack [i] == null)
						return i;
				return currentStack.Length;
			}
		}

		public object GetStackItem (int slot)
		{
			return currentStack [slot];
		}
		
		public void SetStackItem (int slot, object o)
		{
			currentStack [slot] = o;
		}
		
		public void PushStack (int stackSize)
		{
			variableStack.Push (currentStack);
			currentStack = new object [stackSize];
		}
		
		public void PopStack ()
		{
			currentStack = (object[])variableStack.Pop();
		}
		
		#endregion
		
		#region Free/Busy
		Hashtable busyTable = new Hashtable ();
		static object busyObject = new object ();
		
		public void SetBusy (object o)
		{
			busyTable [o] = busyObject;
		}
		
		public void SetFree (object o)
		{
			busyTable.Remove (o);
		}
		
		public bool IsBusy (object o)
		{
			return busyTable [o] == busyObject;
		}
		#endregion

		public bool PushElementState (string prefix, string name, string ns, bool preserveWhitespace)
		{
			bool b = IsCData (name, ns);
			XPathContext.PushScope ();
			Out.InsideCDataSection = XPathContext.IsCData = b;
			XPathContext.WhitespaceHandling = true;//preserveWhitespace;
			XPathContext.ElementPrefix = prefix;
			XPathContext.ElementNamespace = ns;
			return b;
		}

		bool IsCData (string name, string ns)
		{
			for (int i = 0; i < Output.CDataSectionElements.Length; i++) {
				XmlQualifiedName qname = Output.CDataSectionElements [i];
				if (qname.Name == name && qname.Namespace == ns) {
					return true;
				}
			}
			return false;
		}

		public void PopCDataState (bool isCData)
		{
			XPathContext.PopScope ();
			Out.InsideCDataSection = XPathContext.IsCData;
		}

		public bool PreserveOutputWhitespace {
			get { return XPathContext.Whitespace; }
		}
	}
}
