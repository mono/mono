//
// System.Web.Compilation.BaseCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Web.UI;
//temp:
using Microsoft.CSharp;
using System.IO;

namespace System.Web.Compilation
{
	abstract class BaseCompiler
	{
		TemplateParser parser;
		CodeDomProvider provider;
		ICodeCompiler compiler;
		CodeCompileUnit unit;
		CodeNamespace mainNS;
		CompilerParameters compilerParameters;
		protected CodeTypeDeclaration mainClass;
		protected CodeTypeReferenceExpression mainClassExpr;
		protected static CodeThisReferenceExpression thisRef = new CodeThisReferenceExpression ();

		protected BaseCompiler (TemplateParser parser)
		{
			compilerParameters = new CompilerParameters ();
			this.parser = parser;
		}

		void Init ()
		{
			unit = new CodeCompileUnit ();
			mainNS = new CodeNamespace ("ASP");
			unit.Namespaces.Add (mainNS);
			mainClass = new CodeTypeDeclaration (parser.ClassName);
			mainClass.TypeAttributes = TypeAttributes.Public;
			mainNS.Types.Add (mainClass);
			mainClass.BaseTypes.Add (new CodeTypeReference (parser.BaseType.FullName));
			mainClassExpr = new CodeTypeReferenceExpression ("ASP." + parser.ClassName);
			foreach (object o in parser.Imports) {
				if (o is string)
					mainNS.Imports.Add (new CodeNamespaceImport ((string) o));
			}

			if (parser.Assemblies != null) {
				foreach (object o in parser.Assemblies) {
					if (o is string)
						unit.ReferencedAssemblies.Add ((string) o);
				}
			}

			AddInterfaces ();
			CreateStaticFields ();
			AddScripts ();
			CreateConstructor (null, null);
		}

		protected virtual void CreateStaticFields ()
		{
			CodeMemberField fld = new CodeMemberField (typeof (bool), "__intialized");
			fld.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			fld.InitExpression = new CodePrimitiveExpression (false);
			mainClass.Members.Add (fld);
		}

		protected virtual void CreateConstructor (CodeStatementCollection localVars, CodeStatementCollection trueStmt)
		{
			CodeConstructor ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			mainClass.Members.Add (ctor);

			if (localVars != null)
				ctor.Statements.AddRange (localVars);

			CodeTypeReferenceExpression r = new CodeTypeReferenceExpression (mainNS.Name + "." + mainClass.Name);
			CodeFieldReferenceExpression intialized = new CodeFieldReferenceExpression (r, "__intialized");
			
			CodeBinaryOperatorExpression bin = new CodeBinaryOperatorExpression (intialized,
											     CodeBinaryOperatorType.ValueEquality,
											     new CodePrimitiveExpression (false));

			CodeAssignStatement assign = new CodeAssignStatement (intialized,
									      new CodePrimitiveExpression (true));

			CodeConditionStatement cond = new CodeConditionStatement (bin, assign);
			if (trueStmt != null)
				cond.TrueStatements.AddRange (trueStmt);
			
			ctor.Statements.Add (cond);
		}
		
		void AddScripts ()
		{
			if (parser.Scripts == null || parser.Scripts.Count == 0)
				return;

			foreach (object o in parser.Scripts) {
				if (o is string)
					mainClass.Members.Add (new CodeSnippetTypeMember ((string) o));
			}
		}
		
		protected virtual void CreateMethods ()
		{
		}

		protected virtual void AddInterfaces ()
		{
			if (parser.Interfaces == null)
				return;

			foreach (object o in parser.Interfaces) {
				if (o is string)
					mainClass.BaseTypes.Add (new CodeTypeReference ((string) o));
			}
		}

		protected virtual void ProcessObjectTag (ObjectTagBuilder tag)
		{
		}

		void CheckCompilerErrors (CompilerResults results)
		{
			if (results.NativeCompilerReturnValue == 0)
				return;

			StringWriter writer = new StringWriter();
			provider.CreateGenerator().GenerateCodeFromCompileUnit (unit, writer, null);
			throw new CompilationException (parser.InputFile, results.Errors, writer.ToString ());
		}

		public virtual Type GetCompiledType () 
		{
			Init ();
			CompilationCacheItem item = CachingCompiler.GetCached (parser.InputFile);
			if (item != null) {
				Assembly a = item.Result.CompiledAssembly;
				if (a != null)
					return a.GetType (mainClassExpr.Type.BaseType, true);
			}

			//TODO: get the compiler and default options from system.web/compileroptions
			provider = new CSharpCodeProvider ();
			compiler = provider.CreateCompiler ();

			CreateMethods ();
			compilerParameters.IncludeDebugInformation = parser.Debug;
			CompilerResults results = CachingCompiler.Compile (this);
			CheckCompilerErrors (results);

			return results.CompiledAssembly.GetType (mainClassExpr.Type.BaseType, true);
		}

		internal CompilerParameters CompilerParameters {
			get { return compilerParameters; }
		}

		internal CodeCompileUnit Unit {
			get { return unit; }
		}

		internal virtual ICodeCompiler Compiler {
			get { return compiler; }
		}

		internal TemplateParser Parser {
			get { return parser; }
		}
	}
}

