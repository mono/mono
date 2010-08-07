//
// System.Web.Compilation.UserControlCompiler
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
using System.Web.UI;

namespace System.Web.Compilation
{
	class UserControlCompiler : TemplateControlCompiler
	{
		UserControlParser parser;

		public UserControlCompiler (UserControlParser parser)
			: base (parser)
		{
			this.parser = parser;
		}

		public static Type CompileUserControlType (UserControlParser parser)
		{
			UserControlCompiler pc = new UserControlCompiler (parser);
			return pc.GetCompiledType ();
		}

		protected override void AddClassAttributes ()
		{
			if (parser.OutputCache)
				AddOutputCacheAttribute ();
		}

		protected internal override void CreateMethods ()
		{
			base.CreateMethods ();
			CreateProfileProperty ();
		}
		
		void AddOutputCacheAttribute ()
		{
			CodeAttributeDeclaration cad;
			cad = new CodeAttributeDeclaration ("System.Web.UI.PartialCachingAttribute");
			CodeAttributeArgumentCollection arguments = cad.Arguments;
			
			AddPrimitiveArgument (arguments, parser.OutputCacheDuration);
			AddPrimitiveArgument (arguments, parser.OutputCacheVaryByParam);
			AddPrimitiveArgument (arguments, parser.OutputCacheVaryByControls);
			AddPrimitiveArgument (arguments, parser.OutputCacheVaryByCustom);
			AddPrimitiveArgument (arguments, parser.OutputCacheSqlDependency);
			AddPrimitiveArgument (arguments, parser.OutputCacheShared);
#if NET_4_0
			arguments.Add (new CodeAttributeArgument ("ProviderName", new CodePrimitiveExpression (parser.ProviderName)));
#endif
			mainClass.CustomAttributes.Add (cad);
		}

		void AddPrimitiveArgument (CodeAttributeArgumentCollection arguments, object obj)
		{
			arguments.Add (new CodeAttributeArgument (new CodePrimitiveExpression (obj)));
		}

		protected override void AddStatementsToInitMethodTop (ControlBuilder builder, CodeMemberMethod method)
		{
			base.AddStatementsToInitMethodTop (builder, method);
			if (parser.MasterPageFile != null) {
				CodeExpression prop;
				prop = new CodePropertyReferenceExpression (new CodeArgumentReferenceExpression("__ctrl"), "MasterPageFile");
				CodeExpression ct = new CodePrimitiveExpression (parser.MasterPageFile);
				method.Statements.Add (AddLinePragma (new CodeAssignStatement (prop, ct), parser.DirectiveLocation));
			}
		}
	}
}

