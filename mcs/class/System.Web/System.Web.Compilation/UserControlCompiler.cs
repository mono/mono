//
// System.Web.Compilation.UserControlCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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

		public static Type CompileUserControlType (UserControlParser userControlParser)
		{
			UserControlCompiler pc = new UserControlCompiler (userControlParser);
			return pc.GetCompiledType ();
		}

		protected override void AddClassAttributes ()
		{
			if (parser.OutputCache)
				AddOutputCacheAttribute ();
				
		}

		private void AddOutputCacheAttribute ()
		{
			CodeAttributeDeclaration cad = new CodeAttributeDeclaration ("System.Web.UI.PartialCachingAttribute");
			AddPrimitiveAttribute (cad, parser.OutputCacheDuration);
			AddPrimitiveAttribute (cad, parser.OutputCacheVaryByParam);
			AddPrimitiveAttribute (cad, parser.OutputCacheVaryByControls);
			AddPrimitiveAttribute (cad, parser.OutputCacheVaryByCustom);
			AddPrimitiveAttribute (cad, parser.OutputCacheShared);
			mainClass.CustomAttributes.Add (cad);
		}
	
		private void AddPrimitiveAttribute (CodeAttributeDeclaration cad, object obj)
		{
			cad.Arguments.Add (new CodeAttributeArgument (new CodePrimitiveExpression (obj)));
		}
	
							   
	}
}

