//
// System.Web.Compilation.GlobalAsaxCompiler
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
using System.Web;
using System.Web.Util;

namespace System.Web.Compilation
{
	class GlobalAsaxCompiler : BaseCompiler
	{
		string filename;
		string sourceFile;

		private GlobalAsaxCompiler (string filename)
		{
			this.filename = filename;
		}

		public override Type GetCompiledType ()
		{
			sourceFile = GenerateSourceFile ();

			CachingCompiler compiler = new CachingCompiler (this);
			CompilationResult result = new CompilationResult ();
			result.Options = options;
			if (compiler.Compile (result) == false)
				throw new CompilationException (result);
				
			Assembly assembly = Assembly.LoadFrom (result.OutputFile);
			Type [] types = assembly.GetTypes ();
			foreach (Type t in types) {
				if (t.IsSubclassOf (typeof (HttpApplication))) {
					if (result.Data != null)
						throw new CompilationException ("More that 1 app!!!", result);
					result.Data = t;
				}
			}

			return result.Data as Type;
		}

		public override string Key {
			get {
				return filename;
			}
		}

		public override string SourceFile {
			get {
				return sourceFile;
			}
		}

		public static Type CompileApplicationType (string filename)
		{
			CompilationCacheItem item = CachingCompiler.GetCached (filename);
			if (item != null && item.Result != null) {
				if (item.Result != null)
					return item.Result.Data as Type;

				throw new CompilationException (item.Result);
			}

			GlobalAsaxCompiler gac = new GlobalAsaxCompiler (filename);
			return gac.GetCompiledType ();
		}

		string GenerateSourceFile ()
		{
			Stream input = File.OpenRead (filename);
			AspParser parser = new AspParser (filename, input);
			parser.Parse ();
			AspGenerator generator = new AspGenerator (filename, parser.Elements);
			generator.BaseType = typeof (HttpApplication).ToString ();
			generator.ProcessElements ();
			string generated = generator.GetCode ().ReadToEnd ();
			options = generator.Options;

			//FIXME: should get Tmp dir for this application
			string csName = Path.GetTempFileName () + ".cs";
			WebTrace.WriteLine ("Writing {0}", csName);
			StreamWriter output = new StreamWriter (File.OpenWrite (csName));
			output.Write (generated);
			output.Close ();
			return csName;
		}
	}
}

