//
// System.Web.Compilation.GlobalAsaxCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Util;

namespace System.Web.Compilation
{
	class GlobalAsaxCompiler : BaseCompiler
	{
		Hashtable options;
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
			if (compiler.Compile (result) == false)
				throw new CompilationException (result);
				
			Assembly assembly = Assembly.LoadFrom (result.OutputFile);
			Type [] types = assembly.GetTypes ();
			if (types.Length != 1)
				throw new CompilationException ("More than 1 Type in an application?", result);

			result.Data = types [0];
			return types [0];
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

		public override string CompilerOptions {
			get {
				if (options == null)
					return base.CompilerOptions;

				StringBuilder sb = new StringBuilder (base.CompilerOptions);
				string compilerOptions = options ["CompilerOptions"] as string;
				if (compilerOptions != null) {
					sb.Append (' ');
					sb.Append (compilerOptions);
				}

				string references = options ["References"] as string;
				if (references == null)
					return sb.ToString ();

				string [] split = references.Split (' ');
				foreach (string s in split)
					sb.AppendFormat (" /r:{0}", s);

				return sb.ToString ();
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

