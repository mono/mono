//
// XQueryContext.cs - XQuery/XPath2 dynamic context
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Query;

namespace Mono.Xml.XPath2
{
	internal class XQueryContextManager
	{
		XQueryStaticContext staticContext;

		// Fixed dynamic context during evaluation
		XmlArgumentList args;
		XmlResolver extDocResolver;

		Stack<XQueryContext> contextStack = new Stack<XQueryContext> ();
		XQueryContext currentContext;
#if SEEMS_CONTEXT_FOR_CURRENT_REQURED
#else
		Stack<XPathSequence> contextSequenceStack = new Stack<XPathSequence> ();
#endif
		XmlWriter currentWriter;
		XPathItem input; // source input item(node)
		XPathSequence currentSequence;
		XmlNamespaceManager namespaceManager;
		Hashtable localCollationCache = new Hashtable ();

		internal XQueryContextManager (XQueryStaticContext ctx, XPathItem input, XmlWriter writer, XmlResolver resolver, XmlArgumentList args)
		{
			this.input = input;
			this.staticContext = ctx;
			this.args = args;
			currentWriter = writer;
			this.extDocResolver = resolver;

			namespaceManager = new XmlNamespaceManager (ctx.NameTable);
			foreach (DictionaryEntry de in ctx.NSResolver.GetNamespacesInScope (XmlNamespaceScope.ExcludeXml))
				namespaceManager.AddNamespace (de.Key.ToString (), de.Value.ToString ());
			namespaceManager.PushScope ();

			currentContext = new XQueryContext (this, null, new Hashtable ());
			if (input != null) {
				currentSequence = new SingleItemIterator (input, currentContext);
				currentSequence.MoveNext ();
			}
			currentContext = new XQueryContext (this, currentSequence, new Hashtable ());
		}

		public bool Initialized {
			get { return currentContext != null; }
		}

		public XmlResolver ExtDocResolver {
			get { return extDocResolver; }
		}

		public XmlArgumentList Arguments {
			get { return args; }
		}

		public XmlWriter Writer {
			get { return currentWriter; }
			// FIXME: might be better avoid setter as public
			set { currentWriter = value; }
		}

		internal XQueryContext CurrentContext {
			get { return currentContext; }
		}

		internal XQueryStaticContext StaticContext {
			get { return staticContext; }
		}

		internal CultureInfo GetCulture (string collation)
		{
			CultureInfo ci = staticContext.GetCulture (collation);
			if (ci == null)
				ci = (CultureInfo) localCollationCache [collation];
			if (ci != null)
				return ci;
			ci = new CultureInfo (collation);
			localCollationCache [collation] = ci;
			return ci;
		}

		public void PushCurrentSequence (XPathSequence sequence)
		{
			if (sequence == null)
				throw new ArgumentNullException ();
			sequence = sequence.Clone ();
#if SEEMS_CONTEXT_FOR_CURRENT_REQURED
			contextStack.Push (currentContext);
			currentsequence = sequence;
			currentContext = new XQueryContext (this);
#else
			contextSequenceStack.Push (currentSequence);
			currentSequence = sequence;
#endif
		}

		public void PopCurrentSequence ()
		{
#if SEEMS_CONTEXT_FOR_CURRENT_REQURED
			PopContext ();
#else
			currentSequence = contextSequenceStack.Pop ();
#endif
			if (currentSequence == null)
				throw new SystemException ("XQuery error: should not happen.");
		}

		internal void PushContext ()
		{
			contextStack.Push (currentContext);
			currentContext = new XQueryContext (this);
		}

		internal void PopContext ()
		{
			currentContext = contextStack.Pop ();
		}

		internal void PushVariable (XmlQualifiedName name, object iter)
		{
			PushContext ();
			CurrentContext.SetVariable (name, iter);
		}

		internal void PopVariable ()
		{
			PopContext ();
		}

		internal XmlNamespaceManager NSManager {
			get { return namespaceManager; }
		}
	}

	public class XQueryContext : IXmlNamespaceResolver
	{
		XQueryContextManager contextManager;
		Hashtable currentVariables;
		XPathSequence currentSequence;

		internal XQueryContext (XQueryContextManager manager)
			: this (manager,
				manager.CurrentContext.currentSequence,
				(Hashtable) manager.CurrentContext.currentVariables.Clone ())
		{
		}

		internal XQueryContext (XQueryContextManager manager, XPathSequence currentSequence, Hashtable currentVariables)
		{
			contextManager = manager;
			this.currentSequence = currentSequence;
/*
			if (manager.CurrentContext != null)
				currentVariables = (Hashtable) manager.CurrentContext.currentVariables.Clone ();
			else
				currentVariables = new Hashtable ();
*/
			this.currentVariables = currentVariables;
		}

		internal XmlWriter Writer {
			get { return contextManager.Writer; }
			// FIXME: might be better avoid public setter.
			set { contextManager.Writer = value; }
		}

		internal XQueryStaticContext StaticContext {
			get { return contextManager.StaticContext; }
		}

		internal CultureInfo DefaultCollation {
			get { return StaticContext.DefaultCollation; }
		}

		internal XQueryContextManager ContextManager {
			get { return contextManager; }
		}

		public XPathItem CurrentItem {
			get {
				if (currentSequence == null)
					throw new XmlQueryException ("This XQuery dynamic context has no context item.");
				return CurrentSequence.Current;
			}
		}

		public XPathNavigator CurrentNode {
			get { return CurrentItem as XPathNavigator; }
		}

		public XPathSequence CurrentSequence {
			get { return currentSequence; }
		}

		internal CultureInfo GetCulture (string collation)
		{
			return contextManager.GetCulture (collation);
		}

		internal void PushVariable (XmlQualifiedName name, object iter)
		{
			contextManager.PushVariable (name, iter);
		}

		// FIXME: Hmm... this design is annoying.
		internal void SetVariable (XmlQualifiedName name, object iter)
		{
			currentVariables [name] = iter;
		}

		internal void PopVariable ()
		{
			contextManager.PopVariable ();
		}

		internal XPathSequence ResolveVariable (XmlQualifiedName name)
		{
			object obj = currentVariables [name];
			if (obj == null && contextManager.Arguments != null)
				obj = contextManager.Arguments.GetParameter (name.Name, name.Namespace);
			if (obj == null)
				return new XPathEmptySequence (this);
			XPathSequence seq = obj as XPathSequence;
			if (seq != null)
				return seq;
			XPathItem item = obj as XPathItem;
			if (item == null)
				item = new XPathAtomicValue (obj, XmlSchemaType.GetBuiltInType (XPathAtomicValue.XmlTypeCodeFromRuntimeType (obj.GetType (), true)));
			return new SingleItemIterator (item, this);
		}

		internal XPathSequence ResolveCollection (string name)
		{
			// FIXME: support later.
			return new XPathEmptySequence (currentSequence.Context);
		}

		public IXmlNamespaceResolver NSResolver {
			get { return contextManager.NSManager; }
		}

		#region IXmlNamespaceResolver implementation
		public XmlNameTable NameTable {
			get { return contextManager.NSManager.NameTable; }
		}

		public string LookupPrefix (string ns)
		{
			return contextManager.NSManager.LookupPrefix (ns);
		}

		public string LookupPrefix (string ns, bool atomized)
		{
			return contextManager.NSManager.LookupPrefix (ns, atomized);
		}

		public string LookupNamespace (string prefix)
		{
			return contextManager.NSManager.LookupNamespace (prefix);
		}

		public string LookupNamespace (string prefix, bool atomized)
		{
			return contextManager.NSManager.LookupNamespace (prefix, atomized);
		}

		public IDictionary GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return contextManager.NSManager.GetNamespacesInScope (scope);
		}
		#endregion
	}
}

#endif
