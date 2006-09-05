//
// XQueryModuleProlog.cs - abstract syntax tree for XQuery
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
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using Mono.Xml.XQuery;

namespace Mono.Xml.XPath2
{
	internal abstract class XQueryModule
	{
		string version;
		Prolog prolog;
		IXmlNamespaceResolver nsResolver;

		public string Version {
			get { return version; }
			set { version = value; }
		}

		public Prolog Prolog {
			get { return prolog; }
			set { prolog = value; }
		}

		public IXmlNamespaceResolver NSResolver {
			get { return nsResolver; }
			set { nsResolver = value; }
		}
	}

	internal class XQueryMainModule : XQueryModule
	{
		ExprSequence queryBody;

		public ExprSequence QueryBody {
			get { return queryBody; }
			set { queryBody = value; }
		}
	}

	internal class XQueryLibraryModule : XQueryModule
	{
		ModuleDecl moduleDecl;

		public ModuleDecl ModuleDecl {
			get { return moduleDecl; }
			set { moduleDecl = value; }
		}
	}

	internal class ModuleDecl
	{
		string prefix;
		string ns;
		
		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}
	}

	internal class Prolog
	{
		public Prolog ()
		{
			namespaceDecls = new StringDictionary ();
			schemaImports = new SchemaImportCollection ();
			moduleImports = new ModuleImportCollection ();
			variables = new XQueryVariableTable ();
			functions = new FunctionCollection ();
		}

		string version;
		StringDictionary namespaceDecls;
		XmlSpace xmlSpaceDecl;
		XmlSpace constructorDecl;
		string defaultElementNamespace;
		string defaultFunctionNamespace;
		string defaultCollation;
		string baseUri;
		bool defaultOrdered; // false by default
		SchemaImportCollection schemaImports;
		ModuleImportCollection moduleImports;
		XQueryVariableTable variables;
		XmlSchemaContentProcessing validationType;
		FunctionCollection functions;

		public string Version {
			get { return version; }
			set { version = value; }
		}

		public StringDictionary NamespaceDecls {
			get { return namespaceDecls; }
		}

		public XmlSpace XmlSpace {
			get { return xmlSpaceDecl; }
			set { xmlSpaceDecl = value; }
		}

		public XmlSpace Constructor {
			get { return constructorDecl; }
			set { constructorDecl = value; }
		}

		public bool DefaultOrdered {
			get { return defaultOrdered; }
			set { defaultOrdered = value; }
		}

		public string DefaultElementNamespace {
			get { return defaultElementNamespace; }
			set { defaultElementNamespace = value; }
		}

		public string DefaultFunctionNamespace {
			get { return defaultFunctionNamespace; }
			set { defaultFunctionNamespace = value; }
		}

		public string DefaultCollation {
			get { return defaultCollation; }
			set { defaultCollation = value; }
		}

		public string BaseUri {
			get { return baseUri; }
			set { baseUri = value; }
		}

		public SchemaImportCollection SchemaImports {
			get { return schemaImports; }
		}

		public ModuleImportCollection ModuleImports {
			get { return moduleImports; }
		}

		public XQueryVariableTable Variables {
			get { return variables; }
		}

		public XmlSchemaContentProcessing ValidationType {
			get { return validationType; }
			set { validationType = value; }
		}

		public FunctionCollection Functions {
			get { return functions; }
		}

		public void Add (object item)
		{
			if (item is bool)
				DefaultOrdered = (bool) item;
			else if (item is XmlQualifiedName) {
				XmlQualifiedName q = (XmlQualifiedName) item;
				NamespaceDecls.Add (q.Name, q.Namespace);
			} else if (item is XmlSpaceDecl) {
				XmlSpace = ((XmlSpaceDecl) item).Value;
			} else if (item is ConstructionDecl) {
				Constructor = ((ConstructionDecl) item).Value;
			} else if (item is SimplePrologContent) {
				SimplePrologContent c = (SimplePrologContent) item;
				string s = c.LiteralValue;
				switch (c.Type) {
					case PrologContentType.DefaultElementNamespace:
						DefaultElementNamespace = s;
						break;
					case PrologContentType.DefaultFunctionNamespace:
						DefaultFunctionNamespace = s;
						break;
					case PrologContentType.DefaultCollation:
						DefaultCollation = s;
						break;
					case PrologContentType.BaseUri:
						BaseUri = s;
						break;
					default:
						throw new XmlQueryCompileException ("Invalid XQuery prolog content was found.");
				}
			} else if (item is SchemaImport) {
				SchemaImports.Add (item as SchemaImport);
			} else if (item is ModuleImport) {
				ModuleImports.Add (item as ModuleImport);
			} else if (item is XQueryVariable) {
				XQueryVariable var = item  as XQueryVariable;
				Variables.Add (var);
			} else if (item is XmlSchemaContentProcessing) {
				ValidationType = (XmlSchemaContentProcessing) item;
			} else if (item is FunctionDeclaration) {
				Functions.Add (item as FunctionDeclaration);
			} else
				throw new XmlQueryCompileException ("Invalid XQuery prolog content item was found.");
		}
	}

	class XmlSpaceDecl
	{
		public XmlSpace Value;

		public XmlSpaceDecl (XmlSpace value)
		{
			Value = value;
		}
	}

	class ConstructionDecl
	{
		public XmlSpace Value;

		public ConstructionDecl (XmlSpace value)
		{
			Value = value;
		}
	}

	public class ModuleImportCollection : CollectionBase
	{
		public void Add (ModuleImport import)
		{
			List.Add (import);
		}
	}

	public class SchemaImportCollection : CollectionBase
	{
		public void Add (SchemaImport import)
		{
			List.Add (import);
		}
	}

	public enum PrologContentType {
		DefaultElementNamespace,
		DefaultFunctionNamespace,
		DefaultCollation,
		BaseUri
	}

	public class SimplePrologContent
	{
		public SimplePrologContent (PrologContentType type, string literalValue)
		{
			this.type = type;
			this.literalValue = literalValue;
		}

		PrologContentType type;
		string literalValue;

		public PrologContentType Type {
			get { return type; }
			set { type = value; }
		}

		public string LiteralValue {
			get { return literalValue; }
			set { literalValue = value; }
		}
	}

	public abstract class AbstractImport
	{
		public AbstractImport (string prefix, string ns, ICollection locations)
		{
			this.prefix = prefix;
			this.ns = ns;
			this.locations = locations;
			if (locations == null)
				this.locations = new ArrayList (); // empty list
		}

		string prefix, ns;
		ICollection locations;

		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public ICollection Locations {
			get { return locations; }
			set { locations = value != null ? value : new ArrayList (); }
		}
	}

	public class SchemaImport : AbstractImport
	{
		public SchemaImport (string prefix, string ns, ICollection schemaLocations)
			: base (prefix == "default element namespace" ? String.Empty : prefix, ns, schemaLocations)
		{
			// Prefix might 1) String.Empty for non-specified prefix,
			// 2) "default element namespace" that is as is 
			// specified in xquery.
			if (prefix == "default element namespace")
				useDefaultElementNamespace = true;
		}

		bool useDefaultElementNamespace;

		public bool UseDefaultElementNamespace {
			get { return useDefaultElementNamespace; }
			set { useDefaultElementNamespace = value; }
		}
	}

	public class ModuleImport : AbstractImport
	{
		public ModuleImport (string prefix, string ns, ICollection moduleLocations)
			: base (prefix, ns, moduleLocations)
		{
		}
	}

	public class XQueryVariableTable : DictionaryBase
	{
		public void Add (XQueryVariable decl)
		{
			Dictionary.Add (decl.Name, decl);
		}

		public ICollection Keys {
			get { return Dictionary.Keys; }
		}

		public ICollection Values {
			get { return Dictionary.Values; }
		}

		public XQueryVariable this [XmlQualifiedName name] {
			get { return Dictionary [name] as XQueryVariable; }
		}
	}

	public class XQueryVariable
	{
		public XQueryVariable (XmlQualifiedName name, SequenceType type, ExprSequence varBody)
		{
			this.name = name;
			this.type = type;
			this.varBody = varBody; // might be null (just declaration).
		}

		XmlQualifiedName name;
		SequenceType type;
		ExprSequence varBody;

		public XmlQualifiedName Name {
			get { return name; }
		}

		public SequenceType VariableType {
			get { return type; }
		}

		public bool External {
			get { return varBody == null; }
		}

		public ExprSequence VariableBody {
			get { return varBody; }
		}
	}

	internal class FunctionCollection : DictionaryBase
	{
		public void Add (FunctionDeclaration decl)
		{
			Dictionary.Add (decl.Name, decl);
		}

		public ICollection Keys {
			get { return Dictionary.Keys; }
		}

		public ICollection Values {
			get { return Dictionary.Values; }
		}

		public FunctionDeclaration this [XmlQualifiedName name] {
			get { return Dictionary [name] as FunctionDeclaration; }
		}
	}

	internal class FunctionDeclaration
	{
		public FunctionDeclaration (XmlQualifiedName name,
			XQueryFunctionArgumentList parameters,
			SequenceType type,
			EnclosedExpr expr)
		{
			this.name = name;
			this.parameters = parameters;
			this.returnType = type;
			this.funcBody = expr;
		}

		XmlQualifiedName name;
		SequenceType returnType;
		XQueryFunctionArgumentList parameters;
		EnclosedExpr funcBody;

		public XmlQualifiedName Name {
			get { return name; }
		}

		public SequenceType ReturnType {
			get { return returnType; }
		}

		public bool External {
			get { return funcBody == null; }
		}

		public XQueryFunctionArgumentList Parameters {
			get { return parameters; }
		}

		public EnclosedExpr FunctionBody {
			get { return funcBody; }
		}
	}

	public class XQueryFunctionArgumentList : CollectionBase
	{
		public void Add (XQueryFunctionArgument p)
		{
			List.Add (p);
		}

		public void Insert (int pos, XQueryFunctionArgument p)
		{
			List.Insert (pos, p);
		}

		public XQueryFunctionArgument this [int i] {
			get { return (XQueryFunctionArgument) List [i]; }
		}

		public XQueryFunctionArgument [] ToArray ()
		{
			XQueryFunctionArgument [] arr = new XQueryFunctionArgument [List.Count];
			List.CopyTo (arr, 0);
			return arr;
		}
	}

	public abstract class PragmaMUExtensionBase
	{
		XmlQualifiedName name;
		string text;

		protected PragmaMUExtensionBase (XmlQualifiedName name, string text)
		{
			this.name = name;
			this.text = text;
		}

		public XmlQualifiedName Name {
			get { return name; }
		}

		public string Text {
			get { return text; }
		}
	}

	public class Pragma : PragmaMUExtensionBase
	{
		public Pragma (XmlQualifiedName name, string text)
			: base (name, text)
		{
		}
	}

	public class MUExtension : PragmaMUExtensionBase
	{
		public MUExtension (XmlQualifiedName name, string text)
			: base (name, text)
		{
		}
	}
}

#endif
