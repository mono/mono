//
// XQueryASTCompiler.cs - XQuery static context compiler
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
using System.IO;
using System.Security.Policy;
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using Mono.Xml.XQuery;
using Mono.Xml.XQuery.Parser;

namespace Mono.Xml.XPath2
{
	internal class XQueryASTCompiler
	{
		// Static method

		public static XQueryStaticContext Compile (XQueryModule module, XQueryCompileOptions options, Evidence evidence, XQueryCommandImpl commandImpl)
		{
			if (options == null)
				options = new XQueryCompileOptions ();
			return new XQueryASTCompiler (module, options, new XQueryCompileContext (), evidence, commandImpl).Compile ();
		}

		// Constructor

		private XQueryASTCompiler (XQueryModule module, XQueryCompileOptions options, XQueryCompileContext compileContext, Evidence evidence, XQueryCommandImpl commandImpl)
		{
			this.module = module;
			this.options = options;
			this.compileContext = compileContext;
			this.evidence = evidence;
			this.commandImpl = commandImpl;

			inScopeSchemas = new XmlSchemaSet ();
			localVariables = new Hashtable ();
			localFunctions = new XQueryFunctionTable ();
		}

		XQueryModule module;
		XQueryCompileOptions options;
		XQueryCompileContext compileContext;

		IXmlNamespaceResolver nsResolver;
		string defaultFunctionNamespace;

		// FIXME: Is it OK for an XmlSchema to be in two or more set?
		XmlSchemaSet inScopeSchemas;
		ArrayList libModuleContexts = new ArrayList ();

		Hashtable localVariables;
		XQueryFunctionTable localFunctions;

		bool preserveWhitespace; // Xml space policy
		bool constructionSpace; // construction mode
		bool defaultOrdered; // Ordering mode
		string baseUri;
		Evidence evidence;
		XQueryCommandImpl commandImpl;

		// methods.

		private XQueryStaticContext Compile ()
		{
			CompileProlog ();

			XQueryMainModule main = module as XQueryMainModule;
			ExprSequence expr = (main != null) ?
				CompileExprSequence (main.QueryBody) : null;

			return new XQueryStaticContext (
				options,
				compileContext,
				expr,
				inScopeSchemas,
				localVariables,
				localFunctions,
				module.NSResolver,
				module.Prolog.DefaultFunctionNamespace,
				preserveWhitespace,
				constructionSpace,
				defaultOrdered,
				baseUri,
				evidence,
				commandImpl);
		}

		private void CompileProlog ()
		{
			Prolog p = module.Prolog;

			// resolve external modules
			// FIXME: check if external queries are allowed by default.
			// FIXME: check recursion
			XmlUrlResolver res = new XmlUrlResolver ();
			foreach (ModuleImport modimp in p.ModuleImports) {
				foreach (string uri in modimp.Locations) {
					Stream s = res.GetEntity (res.ResolveUri (null, uri), null, typeof (Stream)) as Stream;
					XQueryLibraryModule ext = XQueryParser.Parse (new StreamReader (s)) as XQueryLibraryModule;
					if (ext == null)
						throw new XmlQueryCompileException (String.Format ("External module {0} is resolved as a main module, while it should be a library module."));
					XQueryStaticContext sctx = new XQueryASTCompiler (ext, options, compileContext, evidence, commandImpl).Compile ();
					libModuleContexts.Add (sctx);
				}
			}

			// resolve and compile in-scope schemas
			foreach (SchemaImport xsimp in p.SchemaImports) {
				foreach (string uri in xsimp.Locations) {
					XmlSchema schema = inScopeSchemas.Add (xsimp.Namespace, uri);
					compileContext.InEffectSchemas.Add (schema);
				}
			}
			inScopeSchemas.Compile ();

			CheckReferences ();

			ResolveVariableReferences ();

			// compile FunctionDeclaration into XQueryFunction
			foreach (FunctionDeclaration func in p.Functions.Values) {
				XQueryFunction cfunc = CompileFunction (func);
				localFunctions.Add (cfunc);
			}
		}

		private void CheckReferences ()
		{
			XQueryMainModule main = module as XQueryMainModule;
			if (main != null)
				main.QueryBody.CheckReference (this);
			foreach (FunctionDeclaration func in module.Prolog.Functions.Values) {
				if (!func.External)
					func.FunctionBody.CheckReference (this);
				CheckSchemaType (func.ReturnType);
				foreach (XQueryFunctionArgument param in func.Parameters)
					CheckSchemaType (param.Type);
			}
		}

		internal void CheckSchemaType (SequenceType type)
		{
			if (type == null)
				return;
			type.ItemType.CheckReference (this);
		}

		internal void CheckSchemaTypeName (XmlQualifiedName name)
		{
			XmlSchemaType type = XmlSchemaType.GetBuiltInType (name);
			if (type != null)
				return;
			throw new XmlQueryCompileException (String.Format ("Unresolved schema type name: {0}", name));
		}

		internal void CheckVariableName (XmlQualifiedName name)
		{
			// This should not be done, since unresolved QName
			// may be still valid in context of XmlArgumentList
			// which is supplied at dynamic evaluation phase.
			/*
			if (module.Prolog.Variables [name] != null)
				return;
			if (localVariables [name] != null)
				return;
			foreach (XQueryStaticContext ctx in libModuleContexts)
				if (ctx.InScopeVariables [name] != null)
					return;
			throw new XmlQueryCompileException (String.Format ("Unresolved variable name: {0}", name));
			*/
		}

		internal void CheckFunctionName (XmlQualifiedName name)
		{
			if (XQueryFunction.FindKnownFunction (name) != null)
				return;
			if (module.Prolog.Functions [name] != null)
				return;
			foreach (XQueryStaticContext ctx in libModuleContexts)
				if (ctx.InScopeFunctions [name] != null)
					return;
			throw new XmlQueryCompileException (String.Format ("Unresolved function name: {0}", name));
		}

		private void ResolveVariableReferences ()
		{
			// TODO
		}

		internal XmlSchemaType ResolveSchemaType (XmlQualifiedName name)
		{
			XmlSchemaType type = XmlSchemaType.GetBuiltInType (name);
			if (type != null)
				return type;
			type = inScopeSchemas.GlobalTypes [name] as XmlSchemaType;
			if (type != null)
				return type;
			return null;
		}

		private XQueryFunction CompileFunction (FunctionDeclaration func)
		{
			if (func.External)
				return XQueryFunction.FromQName (func.Name);
			return new XQueryUserFunction (func.Name, func.Parameters.ToArray (), func.FunctionBody.Expr, func.ReturnType);
		}

		private ExprSequence CompileExprSequence (ExprSequence expr)
		{
			for (int i = 0; i < expr.Count; i++)
				expr [i] = expr [i].Compile (this);
			return expr;
		}

		internal void CheckType (ExprSingle expr, SequenceType type)
		{
			if (!expr.StaticType.CanConvertTo (type))
				throw new XmlQueryCompileException (String.Format ("Cannot convert type from {0} to {1}", expr.StaticType, type));
		}

		internal XQueryFunction ResolveFunction (XmlQualifiedName name)
		{
			XQueryFunction func = XQueryFunction.FindKnownFunction (name);
			if (func == null)
				func = localFunctions [name];

			if (func != null)
				return func;
			else
				throw new XmlQueryCompileException ("Could not find specified function.");
		}
	}
}
#endif
