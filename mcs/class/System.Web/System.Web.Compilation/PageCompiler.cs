//
// System.Web.Compilation.PageCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Compilation
{
	class PageCompiler : BaseCompiler
	{
		PageParser pageParser;
		string sourceFile;

		private PageCompiler (PageParser pageParser)
		{
			this.pageParser = pageParser;
		}

		public override Type GetCompiledType ()
		{
			string inputFile = pageParser.InputFile;
			sourceFile = GenerateSourceFile ();

			CachingCompiler compiler = new CachingCompiler (this);
			CompilationResult result = new CompilationResult ();
			result.Options = options;
			if (compiler.Compile (result) == false)
				throw new CompilationException (result);
				
			Assembly assembly = Assembly.LoadFrom (result.OutputFile);
			Type [] types = assembly.GetTypes ();
			foreach (Type t in types) {
				if (t.IsSubclassOf (typeof (Page))) {
					if (result.Data != null)
						throw new CompilationException ("More that 1 page!!!", result);
					result.Data = t;
				}
			}

			return result.Data as Type;
		}

		public override string Key {
			get {
				return pageParser.InputFile;
			}
		}

		public override string SourceFile {
			get {
				return sourceFile;
			}
		}

		public static Type CompilePageType (PageParser pageParser)
		{
			CompilationCacheItem item = CachingCompiler.GetCached (pageParser.InputFile);
			if (item != null && item.Result != null) {
				if (item.Result != null) {
					pageParser.Options = item.Result.Options;
					return item.Result.Data as Type;
				}

				throw new CompilationException (item.Result);
			}

			PageCompiler pc = new PageCompiler (pageParser);
			return pc.GetCompiledType ();
		}

		string GenerateSourceFile ()
		{
			AspGenerator generator = new AspGenerator (pageParser.InputFile);
			generator.Context = pageParser.Context;
			generator.BaseType = pageParser.BaseType.ToString ();
			generator.ProcessElements ();
			pageParser.Text = generator.GetCode ().ReadToEnd ();
			options = generator.Options;

			//FIXME: should get Tmp dir for this application
			string csName = Path.GetTempFileName () + ".cs";
			WebTrace.WriteLine ("Writing {0}", csName);
			StreamWriter output = new StreamWriter (File.OpenWrite (csName));
			output.Write (pageParser.Text);
			output.Close ();
			return csName;
		}
	}
}

