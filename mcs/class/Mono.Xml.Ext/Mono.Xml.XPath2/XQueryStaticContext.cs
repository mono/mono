//
// XQueryStaticContext.cs - XQuery static context components
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Policy;
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using Mono.Xml.XPath2;

namespace Mono.Xml.XPath2
{
	// Holds static context, that is created for each module.
	internal class XQueryStaticContext
	{
		public static XQueryStaticContext Optimize (XQueryStaticContext ctx)
		{
			// FIXME: do type promotion and expression reduction
			return ctx;
		}

		// Don't keep XQueryCompileOptions and XQueryMainModule
		// inside this class. I don't want them affect this instance
		// by being modified externally after the compilation.

		public XQueryStaticContext (
			XQueryCompileOptions options,
			XQueryCompileContext compileContext,
			ExprSequence queryBody,
			XmlSchemaSet inScopeSchemas,
			IDictionary inScopeVariables,
			XQueryFunctionTable functionSignatures,
			IXmlNamespaceResolver nsResolver,
			string defaultFunctionNamespace,
			bool preserveWhitespace,
			bool constructionSpace,
			bool defaultOrdered,
			string baseUri,
			Evidence evidence,
			XQueryCommandImpl commandImpl)
		{
			// Initialization phase.
			compat = options.Compatibility;
			nameTable = options.NameTable;
			this.queryBody = queryBody;
			this.nsResolver = nsResolver;
			this.defaultFunctionNamespace = defaultFunctionNamespace;
//			elemNSManager = new XmlNamespaceManager (nameTable);
//			funcNSManager = new XmlNamespaceManager (nameTable);
			xqueryFlagger = options.XQueryFlagger;
			xqueryStaticFlagger = options.XQueryStaticFlagger;
//			xqueryResolver = options.KnownDocumentResolver;
			knownCollections = (IDictionary) options.KnownCollections.Clone ();
			functions = functionSignatures;
			this.compileContext = compileContext;
			this.inScopeSchemas = inScopeSchemas;
			this.inScopeVariables = inScopeVariables;
			this.preserveWhitespace = preserveWhitespace;
			this.preserveConstructionSpace = constructionSpace;
			this.defaultOrdered = defaultOrdered;
			this.baseUri = baseUri;
			this.defaultCollation = options.DefaultCollation;
			// FIXME: set contextItemStaticType
			// FIXME: set extDocResolver

			this.evidence = evidence;
			this.commandImpl = commandImpl;
		}

		// It holds in-effect components et. al.
		XQueryCompileContext compileContext;

		XmlNameTable nameTable;
		Evidence evidence; // for safe custom function execution / safe assembly loading
		XQueryCommandImpl commandImpl; // for event delegate

		ExprSequence queryBody;

		// See XQuery 1.0, 2.1.1 "Static Context"
		XmlQueryDialect compat; // XPath 1.0 compatibility mode
		IXmlNamespaceResolver nsResolver;	// Manages "statically known namespaces" and "default element/type namespace" 
		string defaultFunctionNamespace; // default function namespace
		XmlSchemaSet inScopeSchemas;	// in-scope schemas
		IDictionary inScopeVariables;
		Type contextItemStaticType;	// TODO: context item static type?
		XQueryFunctionTable functions;

		// Statically known collations is not defined here. It is equal to all supported CultureInfo.
//		IDictionary staticallyKnownCollations;

		CultureInfo defaultCollation; // or TextInfo ?
		bool preserveConstructionSpace; // construction mode
		bool defaultOrdered; // Ordering mode
		bool preserveWhitespace; // Xml space policy
		string baseUri;
//		XmlResolver extDocResolver; // statically known documents
		IDictionary knownCollections; // statically known collections
		bool xqueryFlagger;
		bool xqueryStaticFlagger;

		// Properties

		public XQueryCompileContext CompileContext {
			get { return compileContext; }
		}

		public XmlQueryDialect Compatibility {
			get { return compat; }
		}

		public ExprSequence QueryBody {
			get { return queryBody; }
		}

		public XmlNameTable NameTable {
			get { return nameTable; }
		}

		public Evidence Evidence {
			get { return evidence; }
		}

		public CultureInfo DefaultCollation {
			get { return defaultCollation; }
		}

		public XmlSchemaSet InScopeSchemas {
			get { return inScopeSchemas; }
		}

		// in-scope functions.
		public XQueryFunctionTable InScopeFunctions {
			get { return functions; }
		}

		// in-scope variables. XmlQualifiedName to XPathItem
		public IDictionary InScopeVariables {
			get { return inScopeVariables; }
		}

		public bool PreserveWhitespace {
			get { return preserveWhitespace; }
		}

		public bool PreserveConstructionSpace {
			get { return preserveConstructionSpace; }
		}

		public bool DefaultOrdered {
			get { return defaultOrdered; }
		}

		// statically known collections. string to ICollection (or XPathItemIterator, or XPathNodeIterator).
		public IDictionary KnownCollections {
			get { return knownCollections; }
		}

		public bool XQueryFlagger {
			get { return xqueryFlagger; }
		}

		public bool XQueryStaticFlagger {
			get { return xqueryStaticFlagger; }
		}

		public string BaseUri {
			get { return baseUri; }
		}

		public IXmlNamespaceResolver NSResolver {
			get { return nsResolver; }
		}

		public string DefaultFunctionNamespace {
			get { return defaultFunctionNamespace; }
			set { defaultFunctionNamespace = value; }
		}

		// FIXME: consider those from imported modules
		public XQueryFunction ResolveFunction (XmlQualifiedName name)
		{
			XQueryFunction f = functions [name];
			if (f != null)
				return f;
			return null;
		}

		// FIXME: wait for W3C clarification.
		internal CultureInfo GetCulture (string collation)
		{
			return null;
		}

		internal void OnMessageEvent (object sender, QueryEventArgs e)
		{
			commandImpl.ProcessMessageEvent (sender, e);
		}
	}
}
#endif
