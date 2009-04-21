//
// System.Web.Compilation.PageCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.SessionState;
using System.Web.Util;
#if NET_2_0
using System.Collections.Generic;
using System.Web.Profile;
#endif

namespace System.Web.Compilation
{
	class PageCompiler : TemplateControlCompiler
	{
		PageParser pageParser;
		static CodeTypeReference intRef = new CodeTypeReference (typeof (int));

		public PageCompiler (PageParser pageParser)
			: base (pageParser)
		{
			this.pageParser = pageParser;
		}

		protected override void CreateStaticFields ()
		{
			base.CreateStaticFields ();
			
			CodeMemberField fld = new CodeMemberField (
#if NET_2_0
				typeof (object),
#else
				typeof (ArrayList),
#endif
				"__fileDependencies");
			fld.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			fld.InitExpression = new CodePrimitiveExpression (null);
			mainClass.Members.Add (fld);

#if NET_2_0
			if (pageParser.OutputCache) {
				fld = new CodeMemberField (typeof (OutputCacheParameters), "__outputCacheSettings");
				fld.Attributes = MemberAttributes.Private | MemberAttributes.Static;
				fld.InitExpression = new CodePrimitiveExpression (null);
				mainClass.Members.Add (fld);
			}
#endif
		}
		
		protected override void CreateConstructor (CodeStatementCollection localVars,
							   CodeStatementCollection trueStmt)
		{
#if NET_2_0
			if (!String.IsNullOrEmpty (pageParser.MasterPageFile))
				// This is here just to trigger master page build, so that its type
				// is available when compiling the page itself.
				BuildManager.GetCompiledType (pageParser.MasterPageFile);
#endif
			if (pageParser.ClientTarget != null) {
				CodeExpression prop;
				prop = new CodePropertyReferenceExpression (thisRef, "ClientTarget");
				CodeExpression ct = new CodePrimitiveExpression (pageParser.ClientTarget);
				if (localVars == null)
					localVars = new CodeStatementCollection ();
				localVars.Add (new CodeAssignStatement (prop, ct));
			}

			ArrayList deps = pageParser.Dependencies;
			int depsCount = deps != null ? deps.Count : 0;
			
			if (depsCount > 0) {
				if (localVars == null)
					localVars = new CodeStatementCollection ();
				if (trueStmt == null)
					trueStmt = new CodeStatementCollection ();

				CodeAssignStatement assign;
#if NET_2_0
				localVars.Add (
					new CodeVariableDeclarationStatement (
						typeof (string[]),
						"dependencies")
				);

				CodeVariableReferenceExpression dependencies = new CodeVariableReferenceExpression ("dependencies");
				trueStmt.Add (
					new CodeAssignStatement (dependencies, new CodeArrayCreateExpression (typeof (string), depsCount))
				);
				
				CodeArrayIndexerExpression arrayIndex;
				object o;
				
				for (int i = 0; i < depsCount; i++) {
					o = deps [i];
					arrayIndex = new CodeArrayIndexerExpression (dependencies, new CodeExpression[] {new CodePrimitiveExpression (i)});
					assign = new CodeAssignStatement (arrayIndex, new CodePrimitiveExpression (o));
					trueStmt.Add (assign);
				}
				
				CodeMethodInvokeExpression getDepsCall = new CodeMethodInvokeExpression (
					thisRef,
					"GetWrappedFileDependencies",
					new CodeExpression[] {dependencies}
				);
				assign = new CodeAssignStatement (GetMainClassFieldReferenceExpression ("__fileDependencies"), getDepsCall);
#else
				localVars.Add (new CodeVariableDeclarationStatement (
						typeof (ArrayList),
						"dependencies")
				);

				CodeVariableReferenceExpression dependencies = new CodeVariableReferenceExpression ("dependencies");
				trueStmt.Add (
					new CodeAssignStatement (dependencies, new CodeObjectCreateExpression (typeof (ArrayList), new CodeExpression[] {new CodePrimitiveExpression (depsCount)}))
				);

				CodeMethodInvokeExpression invoke;
				for (int i = 0; i < depsCount; i++) {
					invoke = new CodeMethodInvokeExpression (dependencies, "Add", new CodeExpression[] {new CodePrimitiveExpression (deps [i])});
					trueStmt.Add (invoke);
				}
				assign = new CodeAssignStatement (GetMainClassFieldReferenceExpression ("__fileDependencies"), dependencies);
#endif

				trueStmt.Add (assign);
			}

			base.CreateConstructor (localVars, trueStmt);
		}
		
		protected override void AddInterfaces () 
		{
			base.AddInterfaces ();
			CodeTypeReference cref;
			
			if (pageParser.EnableSessionState) {
				cref = new CodeTypeReference (typeof (IRequiresSessionState));
#if NET_2_0
				if (partialClass != null)
					partialClass.BaseTypes.Add (cref);
				else
#endif
					mainClass.BaseTypes.Add (cref);
			}
			
			if (pageParser.ReadOnlySessionState) {
				cref = new CodeTypeReference (typeof (IReadOnlySessionState));
#if NET_2_0
				if (partialClass != null)
					partialClass.BaseTypes.Add (cref);					
				else
#endif
					mainClass.BaseTypes.Add (cref);
			}

#if NET_2_0
			if (pageParser.Async)
				mainClass.BaseTypes.Add (new CodeTypeReference (typeof (System.Web.IHttpAsyncHandler)));
			
			mainClass.BaseTypes.Add (new CodeTypeReference (typeof (System.Web.IHttpHandler)));
#endif
		}

		void CreateGetTypeHashCode () 
		{
			CodeMemberMethod method = new CodeMemberMethod ();
			method.ReturnType = intRef;
			method.Name = "GetTypeHashCode";
			method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
			Random rnd = new Random (pageParser.InputFile.GetHashCode ());
			method.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (rnd.Next ())));
			mainClass.Members.Add (method);
		}

		static CodeAssignStatement CreatePropertyAssign (CodeExpression expr, string name, object value)
		{
			CodePropertyReferenceExpression prop;
			prop = new CodePropertyReferenceExpression (expr, name);
			CodePrimitiveExpression prim;
			prim = new CodePrimitiveExpression (value);
			return new CodeAssignStatement (prop, prim);
		}

		static CodeAssignStatement CreatePropertyAssign (string name, object value)
		{
			return CreatePropertyAssign (thisRef, name, value);
		}

		void AddStatementsFromDirective (CodeMemberMethod method)
		{
			string responseEncoding = pageParser.ResponseEncoding;
			if (responseEncoding != null)
				method.Statements.Add (CreatePropertyAssign ("ResponseEncoding", responseEncoding));
			
			int codepage = pageParser.CodePage;
			if (codepage != -1)
				method.Statements.Add (CreatePropertyAssign ("CodePage", codepage));

			string contentType = pageParser.ContentType;
			if (contentType != null)
				method.Statements.Add (CreatePropertyAssign ("ContentType", contentType));

#if !NET_2_0
			if (pageParser.OutputCache) {
				CodeMethodReferenceExpression init = new CodeMethodReferenceExpression (null,
						"InitOutputCache");
				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (init,
						OutputCacheParams ());
				method.Statements.Add (invoke);

			}
#endif
			
			int lcid = pageParser.LCID;
			if (lcid != -1)
				method.Statements.Add (CreatePropertyAssign ("LCID", lcid));

			string culture = pageParser.Culture;
			if (culture != null)
				method.Statements.Add (CreatePropertyAssign ("Culture", culture));

			culture = pageParser.UICulture;
			if (culture != null)
				method.Statements.Add (CreatePropertyAssign ("UICulture", culture));

			string errorPage = pageParser.ErrorPage;
			if (errorPage != null)
				method.Statements.Add (CreatePropertyAssign ("ErrorPage", errorPage));

                        if (pageParser.HaveTrace) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                stmt.Left = new CodePropertyReferenceExpression (thisRef, "TraceEnabled");
                                stmt.Right = new CodePrimitiveExpression (pageParser.Trace);
                                method.Statements.Add (stmt);
                        }

                        if (pageParser.TraceMode != TraceMode.Default) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                CodeTypeReferenceExpression tm = new CodeTypeReferenceExpression ("System.Web.TraceMode");
                                stmt.Left = new CodePropertyReferenceExpression (thisRef, "TraceModeValue");
                                stmt.Right = new CodeFieldReferenceExpression (tm, pageParser.TraceMode.ToString ());
                                method.Statements.Add (stmt);
                        }

                        if (pageParser.NotBuffer) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                stmt.Left = new CodePropertyReferenceExpression (thisRef, "Buffer");
                                stmt.Right = new CodePrimitiveExpression (false);
                                method.Statements.Add (stmt);
                        }

#if NET_2_0
			if (!pageParser.EnableEventValidation) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                CodePropertyReferenceExpression prop;
                                prop = new CodePropertyReferenceExpression (thisRef, "EnableEventValidation");
				stmt.Left = prop;
				stmt.Right = new CodePrimitiveExpression (pageParser.EnableEventValidation);
				method.Statements.Add (stmt);
			}

			if (pageParser.MaintainScrollPositionOnPostBack) {
				CodeAssignStatement stmt = new CodeAssignStatement ();
				CodePropertyReferenceExpression prop;
                                prop = new CodePropertyReferenceExpression (thisRef, "MaintainScrollPositionOnPostBack");
				stmt.Left = prop;
				stmt.Right = new CodePrimitiveExpression (pageParser.MaintainScrollPositionOnPostBack);
				method.Statements.Add (stmt);
			}
#endif
		}

#if NET_2_0
		protected override void AddStatementsToConstructor (CodeConstructor ctor)
		{
			base.AddStatementsToConstructor (ctor);
			if (pageParser.OutputCache)
				OutputCacheParamsBlock (ctor);
		}
#endif
		
		protected override void AddStatementsToInitMethod (CodeMemberMethod method)
		{
#if NET_2_0
			AddStatementsFromDirective (method);
			ILocation directiveLocation = pageParser.DirectiveLocation;

			CodeArgumentReferenceExpression ctrlVar = new CodeArgumentReferenceExpression("__ctrl");
			if (pageParser.Title != null)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "Title", pageParser.Title), directiveLocation));

			if (pageParser.MasterPageFile != null)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "MasterPageFile", pageParser.MasterPageFile), directiveLocation));

			if (pageParser.Theme != null)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "Theme", pageParser.Theme), directiveLocation));

			if (pageParser.StyleSheetTheme != null)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "StyleSheetTheme", pageParser.StyleSheetTheme), directiveLocation));

			if (pageParser.Async != false)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "AsyncMode", pageParser.Async), directiveLocation));

			if (pageParser.AsyncTimeout != -1)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "AsyncTimeout",
											    TimeSpan.FromSeconds (pageParser.AsyncTimeout)), directiveLocation));

			CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression (thisRef, "InitializeCulture");
			method.Statements.Add (AddLinePragma (new CodeExpressionStatement (expr), directiveLocation));
#endif
		}

		protected override void PrependStatementsToFrameworkInitialize (CodeMemberMethod method)
		{
			base.PrependStatementsToFrameworkInitialize (method);
#if NET_2_0
			if (pageParser.StyleSheetTheme != null)
				method.Statements.Add (CreatePropertyAssign ("StyleSheetTheme", pageParser.StyleSheetTheme));
#endif
		}

		
		protected override void AppendStatementsToFrameworkInitialize (CodeMemberMethod method)
		{
			base.AppendStatementsToFrameworkInitialize (method);

			ArrayList deps = pageParser.Dependencies;
			int depsCount = deps != null ? deps.Count : 0;

			if (depsCount > 0) {
				CodeFieldReferenceExpression fileDependencies = GetMainClassFieldReferenceExpression ("__fileDependencies");

				method.Statements.Add (
#if NET_2_0
					new CodeMethodInvokeExpression (
						thisRef,
						"AddWrappedFileDependencies",
						new CodeExpression[] {fileDependencies})
#else
					new CodeAssignStatement (
						new CodeFieldReferenceExpression (thisRef, "FileDependencies"),
						fileDependencies
					)
#endif
				);

			}

#if NET_2_0
			if (pageParser.OutputCache) {
				CodeMethodReferenceExpression init = new CodeMethodReferenceExpression (thisRef, "InitOutputCache");
				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (init, GetMainClassFieldReferenceExpression ("__outputCacheSettings"));
				method.Statements.Add (invoke);
			}
#endif

#if ONLY_1_1
			AddStatementsFromDirective (method);
#endif
			
#if NET_1_1
			if (pageParser.ValidateRequest) {
				CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression ();
                                CodePropertyReferenceExpression prop;
                                prop = new CodePropertyReferenceExpression (thisRef, "Request");
				expr.Method = new CodeMethodReferenceExpression (prop, "ValidateInput");
				method.Statements.Add (expr);
			}
#endif
		}

#if NET_2_0
		CodeAssignStatement AssignOutputCacheParameter (CodeVariableReferenceExpression variable, string propName, object value)
		{
			var ret = new CodeAssignStatement ();

			ret.Left = new CodeFieldReferenceExpression (variable, propName);
			ret.Right = new CodePrimitiveExpression (value);
			return ret;
		}
		
		void OutputCacheParamsBlock (CodeMemberMethod method)
		{
			var statements = new List <CodeStatement> ();
			var localSettingsDecl = new CodeVariableDeclarationStatement (typeof (OutputCacheParameters), "outputCacheSettings");
			var localSettings = new CodeVariableReferenceExpression ("outputCacheSettings");
			
			statements.Add (localSettingsDecl);
			statements.Add (
				new CodeAssignStatement (
					localSettings,
					new CodeObjectCreateExpression (typeof (OutputCacheParameters), new CodeExpression[] {})
				)
			);
			
			TemplateParser.OutputCacheParsedParams parsed = pageParser.OutputCacheParsedParameters;
			if ((parsed & TemplateParser.OutputCacheParsedParams.CacheProfile) != 0)
				statements.Add (AssignOutputCacheParameter (localSettings, "CacheProfile", pageParser.OutputCacheCacheProfile));
			statements.Add (AssignOutputCacheParameter (localSettings, "Duration", pageParser.OutputCacheDuration));
			if ((parsed & TemplateParser.OutputCacheParsedParams.Location) != 0)
				statements.Add (AssignOutputCacheParameter (localSettings, "Location", pageParser.OutputCacheLocation));
			if ((parsed & TemplateParser.OutputCacheParsedParams.NoStore) != 0)
				statements.Add (AssignOutputCacheParameter (localSettings, "NoStore", pageParser.OutputCacheNoStore));
			if ((parsed & TemplateParser.OutputCacheParsedParams.SqlDependency) != 0)
				statements.Add (AssignOutputCacheParameter (localSettings, "SqlDependency", pageParser.OutputCacheSqlDependency));
			if ((parsed & TemplateParser.OutputCacheParsedParams.VaryByContentEncodings) != 0)
				statements.Add (AssignOutputCacheParameter (localSettings, "VaryByContentEncoding", pageParser.OutputCacheVaryByContentEncodings));
			if ((parsed & TemplateParser.OutputCacheParsedParams.VaryByControl) != 0)
				statements.Add (AssignOutputCacheParameter (localSettings, "VaryByControl", pageParser.OutputCacheVaryByControls));
			if ((parsed & TemplateParser.OutputCacheParsedParams.VaryByCustom) != 0)
				statements.Add (AssignOutputCacheParameter (localSettings, "VaryByCustom", pageParser.OutputCacheVaryByCustom));
			if ((parsed & TemplateParser.OutputCacheParsedParams.VaryByHeader) != 0)
				statements.Add (AssignOutputCacheParameter (localSettings, "VaryByHeader", pageParser.OutputCacheVaryByHeader));
			statements.Add (AssignOutputCacheParameter (localSettings, "VaryByParam", pageParser.OutputCacheVaryByParam));

			CodeFieldReferenceExpression outputCacheSettings = GetMainClassFieldReferenceExpression ("__outputCacheSettings");
			statements.Add (new CodeAssignStatement (outputCacheSettings, localSettings));
			
			var cond = new CodeConditionStatement (
				new CodeBinaryOperatorExpression (
					outputCacheSettings,
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression (null)
				),
				statements.ToArray ()
			);

			method.Statements.Add (cond);
		}
#else
		CodeExpression[] OutputCacheParams ()
		{
			return new CodeExpression [] {
				new CodePrimitiveExpression (pageParser.OutputCacheDuration),
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByHeader),
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByCustom),
				new CodeSnippetExpression (typeof (OutputCacheLocation).ToString () +
						"." + pageParser.OutputCacheLocation.ToString ()),
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByParam)
				};
		}
#endif
		
#if NET_2_0
		void CreateStronglyTypedProperty (Type type, string name)
		{
			if (type == null)
				return;
			
			CodeMemberProperty mprop = new CodeMemberProperty ();
			mprop.Name = name;
			mprop.Type = new CodeTypeReference (type);
			mprop.Attributes = MemberAttributes.Public | MemberAttributes.New;
			CodeExpression prop = new CodePropertyReferenceExpression (new CodeBaseReferenceExpression (), name);
			prop = new CodeCastExpression (type, prop);
			mprop.GetStatements.Add (new CodeMethodReturnStatement (prop));
			if (partialClass != null)
				partialClass.Members.Add (mprop);
			else
				mainClass.Members.Add (mprop);

			AddReferencedAssembly (type.Assembly);
		}
#endif
		
		protected internal override void CreateMethods ()
		{
			base.CreateMethods ();

#if NET_2_0
			CreateProfileProperty ();
			CreateStronglyTypedProperty (pageParser.MasterType, "Master");
			CreateStronglyTypedProperty (pageParser.PreviousPageType, "PreviousPage");
#endif
			
			CreateGetTypeHashCode ();

#if NET_2_0
			if (pageParser.Async)
				CreateAsyncMethods ();
#endif
		}

#if NET_2_0
		void CreateAsyncMethods ()
		{
			CodeMemberMethod method = new CodeMemberMethod ();
			CodeParameterDeclarationExpression arg;
			CodeMethodInvokeExpression invoke;

			// public virtual System.IAsyncResult BeginProcessRequest(System.Web.HttpContext context, System.AsyncCallback cb, object data);
			method.ReturnType = new CodeTypeReference (typeof (IAsyncResult));
			method.Name = "BeginProcessRequest";
			method.Attributes = MemberAttributes.Public;
			
			arg = new CodeParameterDeclarationExpression ();
			arg.Type = new CodeTypeReference (typeof (HttpContext));
			arg.Name = "context";
			method.Parameters.Add (arg);

			arg = new CodeParameterDeclarationExpression ();
			arg.Type = new CodeTypeReference (typeof (AsyncCallback));
			arg.Name = "cb";
			method.Parameters.Add (arg);

			arg = new CodeParameterDeclarationExpression ();
			arg.Type = new CodeTypeReference (typeof (object));
			arg.Name = "data";
			method.Parameters.Add (arg);

			invoke = new CodeMethodInvokeExpression (thisRef, "AsyncPageBeginProcessRequest");
			invoke.Parameters.Add (new CodeArgumentReferenceExpression ("context"));
			invoke.Parameters.Add (new CodeArgumentReferenceExpression ("cb"));
			invoke.Parameters.Add (new CodeArgumentReferenceExpression ("data"));

			method.Statements.Add (new CodeMethodReturnStatement (invoke));
			mainClass.Members.Add (method);

			// public virtual void EndProcessRequest(System.IAsyncResult ar);
			method = new CodeMemberMethod ();
			method.ReturnType = new CodeTypeReference (typeof (void));
			method.Name = "EndProcessRequest";
			method.Attributes = MemberAttributes.Public;

			arg = new CodeParameterDeclarationExpression ();
			arg.Type = new CodeTypeReference (typeof (IAsyncResult));
			arg.Name = "ar";
			method.Parameters.Add (arg);

			invoke = new CodeMethodInvokeExpression (thisRef, "AsyncPageEndProcessRequest");
			invoke.Parameters.Add (new CodeArgumentReferenceExpression ("ar"));

			method.Statements.Add (invoke);
			mainClass.Members.Add (method);

			// public override void ProcessRequest(System.Web.HttpContext context);
			method = new CodeMemberMethod ();
			method.ReturnType = new CodeTypeReference (typeof (void));
			method.Name = "ProcessRequest";
			method.Attributes = MemberAttributes.Public | MemberAttributes.Override;

			arg = new CodeParameterDeclarationExpression ();
			arg.Type = new CodeTypeReference (typeof (HttpContext));
			arg.Name = "context";
			method.Parameters.Add (arg);
			
			invoke = new CodeMethodInvokeExpression (new CodeBaseReferenceExpression (), "ProcessRequest");
			invoke.Parameters.Add (new CodeArgumentReferenceExpression ("context"));

			method.Statements.Add (invoke);
			mainClass.Members.Add (method);
		}
#endif
		
		public static Type CompilePageType (PageParser pageParser)
		{
			PageCompiler compiler = new PageCompiler (pageParser);
			return compiler.GetCompiledType ();
		}
	}
}


