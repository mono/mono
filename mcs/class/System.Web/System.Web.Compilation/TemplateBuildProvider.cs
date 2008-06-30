//
// System.Web.Compilation.TemplateBuildProvider
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
#if NET_2_0
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace System.Web.Compilation
{
	internal abstract class TemplateBuildProvider : GenericBuildProvider <TemplateParser>
	{
		protected override string GetClassType (BaseCompiler compiler, TemplateParser parser)
		{
			if (compiler != null)
				return compiler.MainClassType;

			return null;
		}
		
		protected override ICollection GetParserDependencies (TemplateParser parser)
		{
			if (parser != null)
				return parser.Dependencies;
			
			return null;
		}
		
		protected override string GetParserLanguage (TemplateParser parser)
		{
			if (parser != null)
				return parser.Language;

			return null;
		}
		
		protected override string GetCodeBehindSource (TemplateParser parser)
		{
			if (parser != null) {
				string codeBehind = parser.CodeBehindSource;
				if (String.IsNullOrEmpty (codeBehind))
					return null;				

				return parser.CodeBehindSource;
			}
			
			return null;
		}
		
		protected override AspGenerator CreateAspGenerator (TemplateParser parser)
		{
			if (parser != null)
				return new AspGenerator (parser);

			return null;
		}

		protected override List <string> GetReferencedAssemblies (TemplateParser parser)
		{
			if (parser == null)
				return null;
			
			ArrayList al = parser.Assemblies;
			if (al == null || al.Count == 0)
				return null;

			List <string> ret = new List <string> ();
			string loc;
			
			foreach (object o in al) {
				loc = o as string;
				if (loc == null)
					continue;

				if (ret.Contains (loc))
					continue;

				ret.Add (loc);
			}

			return ret;
		}
	}
}
#endif