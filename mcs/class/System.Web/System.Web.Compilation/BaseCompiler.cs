//
// System.Web.Compilation.BaseCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.Configuration;
using System.IO;

namespace System.Web.Compilation
{
	abstract class BaseCompiler
	{
		protected static string dynamicBase = AppDomain.CurrentDomain.SetupInformation.DynamicBase;
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
			AddClassAttributes ();
			CreateStaticFields ();
			AddApplicationAndSessionObjects ();
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

		protected virtual void CreateConstructor (CodeStatementCollection localVars,
							  CodeStatementCollection trueStmt)
		{
			CodeConstructor ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			mainClass.Members.Add (ctor);

			if (localVars != null)
				ctor.Statements.AddRange (localVars);

			CodeTypeReferenceExpression r;
			r = new CodeTypeReferenceExpression (mainNS.Name + "." + mainClass.Name);
			CodeFieldReferenceExpression intialized;
			intialized = new CodeFieldReferenceExpression (r, "__intialized");
			
			CodeBinaryOperatorExpression bin;
			bin = new CodeBinaryOperatorExpression (intialized,
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

		protected virtual void AddClassAttributes ()
		{
		}
		
		protected virtual void AddApplicationAndSessionObjects ()
		{
		}

		/* Utility methods for <object> stuff */
		protected void CreateApplicationOrSessionPropertyForObject (Type type,
									    string propName,
									    bool isApplication,
									    bool isPublic)
		{
			/* if isApplication this generates (the 'cachedapp' field is created earlier):
			private MyNS.MyClass app {
				get {
					if ((this.cachedapp == null)) {
						this.cachedapp = ((MyNS.MyClass)
							(this.Application.StaticObjects.GetObject("app")));
					}
					return this.cachedapp;
				}
			}

			else, this is for Session:
			private MyNS.MyClass ses {
				get {
					return ((MyNS.MyClass) (this.Session.StaticObjects.GetObject("ses")));
				}
			}

			*/

			CodeExpression result = null;

			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = new CodeTypeReference (type);
			prop.Name = propName;
			if (isPublic)
				prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			else
				prop.Attributes = MemberAttributes.Private | MemberAttributes.Final;

			CodePropertyReferenceExpression p1;
			if (isApplication)
				p1 = new CodePropertyReferenceExpression (thisRef, "Application");
			else
				p1 = new CodePropertyReferenceExpression (thisRef, "Session");

			CodePropertyReferenceExpression p2;
			p2 = new CodePropertyReferenceExpression (p1, "StaticObjects");

			CodeMethodReferenceExpression getobject;
			getobject = new CodeMethodReferenceExpression (p2, "GetObject");

			CodeMethodInvokeExpression invoker;
			invoker = new CodeMethodInvokeExpression (getobject,
						new CodePrimitiveExpression (propName));

			CodeCastExpression cast = new CodeCastExpression (prop.Type, invoker);

			if (isApplication) {
				CodeFieldReferenceExpression field;
				field = new CodeFieldReferenceExpression (thisRef, "cached" + propName);

				CodeConditionStatement stmt = new CodeConditionStatement();
				stmt.Condition = new CodeBinaryOperatorExpression (field,
							CodeBinaryOperatorType.IdentityEquality,
							new CodePrimitiveExpression (null));

				CodeAssignStatement assign = new CodeAssignStatement ();
				assign.Left = field;
				assign.Right = cast;
				stmt.TrueStatements.Add (assign);
				prop.GetStatements.Add (stmt);
				result = field;
			} else {
				result = cast;
			}
						
			prop.GetStatements.Add (new CodeMethodReturnStatement (result));
			mainClass.Members.Add (prop);
		}

		protected string CreateFieldForObject (Type type, string name)
		{
			string fieldName = "cached" + name;
			CodeMemberField f = new CodeMemberField (type, fieldName);
			f.Attributes = MemberAttributes.Private;
			mainClass.Members.Add (f);
			return fieldName;
		}

		protected void CreatePropertyForObject (Type type, string propName, string fieldName, bool isPublic)
		{
			CodeFieldReferenceExpression field = new CodeFieldReferenceExpression (thisRef, fieldName);
			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = new CodeTypeReference (type);
			prop.Name = propName;
			if (isPublic)
				prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			else
				prop.Attributes = MemberAttributes.Private | MemberAttributes.Final;

			CodeConditionStatement stmt = new CodeConditionStatement();
			stmt.Condition = new CodeBinaryOperatorExpression (field,
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression (null));

			CodeObjectCreateExpression create = new CodeObjectCreateExpression (prop.Type);	
			stmt.TrueStatements.Add (new CodeAssignStatement (field, create));
			prop.GetStatements.Add (stmt);
			prop.GetStatements.Add (new CodeMethodReturnStatement (field));

			mainClass.Members.Add (prop);
		}
		/******/

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
			Type type = CachingCompiler.GetTypeFromCache (parser.InputFile);
			if (type != null)
				return type;

			Init ();
			string lang = parser.Language;
			CompilationConfiguration config;

			config = CompilationConfiguration.GetInstance (parser.Context);
			provider = config.GetProvider (lang);
			if (provider == null)
				throw new HttpException ("Configuration error. Language not supported: " +
							  lang, 500);

			compiler = provider.CreateCompiler ();

			CreateMethods ();
			compilerParameters.IncludeDebugInformation = parser.Debug;
			compilerParameters.CompilerOptions = config.GetCompilerOptions (lang) + " " +
							     parser.CompilerOptions;

			compilerParameters.WarningLevel = config.GetWarningLevel (lang);
			bool keepFiles = (Environment.GetEnvironmentVariable ("MONO_ASPNET_NODELETE") != null);
			TempFileCollection tempcoll = new TempFileCollection (config.TempDirectory, keepFiles);
			compilerParameters.TempFiles = tempcoll;
			string dllfilename = Path.GetFileName (tempcoll.AddExtension ("dll", true));
			if (!Directory.Exists (dynamicBase))
				Directory.CreateDirectory (dynamicBase);

			compilerParameters.OutputAssembly = Path.Combine (dynamicBase, dllfilename);

			CompilerResults results = CachingCompiler.Compile (this);
			CheckCompilerErrors (results);
			if (results.CompiledAssembly == null)
				throw new CompilationException (parser.InputFile, results.Errors,
					"No assembly returned after compilation!?");

			results.TempFiles.Delete ();
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

