//
// System.Web.Compilation.SimpleBuildProvider
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc
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
	internal abstract class SimpleBuildProvider : GenericBuildProvider <SimpleWebHandlerParser>
	{
		bool _parsed;
		bool _needLoadFromBin;
		
		protected override SimpleWebHandlerParser Parse ()
		{
			SimpleWebHandlerParser parser = Parser;
			
			if (_parsed)
				return parser;
			
			_parsed = true;
			return parser;
		}

		protected override void GenerateCode (AssemblyBuilder assemblyBuilder, SimpleWebHandlerParser parser, BaseCompiler compiler)
		{
			if (assemblyBuilder == null || parser == null)
				return;
			
			string programCode = parser.Program.Trim ();
			if (String.IsNullOrEmpty (programCode)) {
				_needLoadFromBin = true;
				return;
			}
			
			_needLoadFromBin = false;
			using (TextWriter writer = assemblyBuilder.CreateCodeFile (this))
				writer.WriteLine (programCode);
		}

		protected override Type LoadTypeFromBin (BaseCompiler compiler, SimpleWebHandlerParser parser)
		{
			return parser.GetTypeFromBin (parser.ClassName);
		}
		
		protected override string GetClassType (BaseCompiler compiler, SimpleWebHandlerParser parser)
		{
			if (parser != null)
				return parser.ClassName;

			return null;
		}
		
		protected override ICollection GetParserDependencies (SimpleWebHandlerParser parser)
		{
			if (parser != null)
				return parser.Dependencies;

			return null;
		}
		
		protected override string GetParserLanguage (SimpleWebHandlerParser parser)
		{
			if (parser != null)
				return parser.Language;

			return null;
		}
		
		protected override string GetCodeBehindSource (SimpleWebHandlerParser parser)
		{
			return null;
		}
		
		protected override AspGenerator CreateAspGenerator (SimpleWebHandlerParser parser)
		{
			return null;
		}

		protected override BaseCompiler CreateCompiler (SimpleWebHandlerParser parser)
		{
			return new WebServiceCompiler (parser);
		}

		protected override List <string> GetReferencedAssemblies (SimpleWebHandlerParser parser)
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
		
		protected override bool NeedsConstructType {
			get { return false; }
		}

		protected override bool NeedsLoadFromBin {
			get { return _needLoadFromBin; }
		}
	}
}
#endif