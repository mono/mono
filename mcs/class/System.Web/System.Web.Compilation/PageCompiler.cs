//
// System.Web.Compilation.PageCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.CodeDom;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.SessionState;
using System.Web.Util;

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

		protected override void AddInterfaces () 
		{
			base.AddInterfaces ();
			if (pageParser.EnableSessionState)
				mainClass.BaseTypes.Add (new CodeTypeReference (typeof(IRequiresSessionState)));

			if (pageParser.ReadOnlySessionState)
				mainClass.BaseTypes.Add (new CodeTypeReference (typeof (IReadOnlySessionState)));
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

		static CodeAssignStatement CreatePropertyAssign (string name, object value)
		{
			CodePropertyReferenceExpression prop;
			prop = new CodePropertyReferenceExpression (thisRef, name);
			CodePrimitiveExpression prim;
			prim = new CodePrimitiveExpression (value);
			return new CodeAssignStatement (prop, prim);
		}

		protected override void AddStatementsToFrameworkInitialize (CodeMemberMethod method)
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

			if (pageParser.OutputCache) {
				CodeMethodReferenceExpression init = new CodeMethodReferenceExpression (null,
						"InitOutputCache");
				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (init,
						OutputCacheParams ());
				method.Statements.Add (invoke);
			}
			
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

                        if (pageParser.Trace) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                stmt.Left = new CodePropertyReferenceExpression (thisRef, "TraceEnabled");
                                stmt.Right = new CodePrimitiveExpression (true);
                                method.Statements.Add (stmt);
                        }

                        if (pageParser.TraceMode != TraceMode.Default) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                CodeTypeReferenceExpression tm = new CodeTypeReferenceExpression ("System.Web.TraceMode");
                                stmt.Left = new CodePropertyReferenceExpression (thisRef, "TraceModeValue");
                                stmt.Right = new CodeFieldReferenceExpression (tm, pageParser.TraceMode.ToString ());
                                method.Statements.Add (stmt);
                        }

#if NET_1_1
			if (pageParser.ValidateRequest) {
				CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression ();
                                CodePropertyReferenceExpression prop;
                                prop = new CodePropertyReferenceExpression (thisRef, "Request");
				expr.Method = new CodeMethodReferenceExpression (prop, "ValidateInput");
				method.Statements.Add (expr);
			}
#endif
                        
			base.AddStatementsToFrameworkInitialize (method);
		}

		private CodeExpression[] OutputCacheParams ()
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
                
		protected override void CreateMethods ()
		{
			base.CreateMethods ();

			CreateGetTypeHashCode ();
		}

		public static Type CompilePageType (PageParser pageParser)
		{
			PageCompiler compiler = new PageCompiler (pageParser);
			return compiler.GetCompiledType ();
		}
	}
}

