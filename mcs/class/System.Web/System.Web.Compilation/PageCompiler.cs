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

