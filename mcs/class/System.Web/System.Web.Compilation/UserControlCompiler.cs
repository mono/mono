//
// System.Web.Compilation.UserControlCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Compilation
{
	class UserControlCompiler : BaseCompiler
	{
		UserControlParser userControlParser;
		string sourceFile;
		string targetFile;

		internal UserControlCompiler (UserControlParser userControlParser)
		{
			this.userControlParser = userControlParser;
		}

		public override Type GetCompiledType ()
		{
			string inputFile = userControlParser.InputFile;
			sourceFile = GenerateSourceFile (userControlParser);

			CachingCompiler compiler = new CachingCompiler (this);
			CompilationResult result = new CompilationResult (sourceFile);
			result.Options = options;
			if (compiler.Compile (result) == false)
				throw new CompilationException (result);
			
			result.Dependencies = dependencies;
			if (result.Data is Type) {
				targetFile = result.OutputFile;
				return (Type) result.Data;
			}

			Assembly assembly = Assembly.LoadFrom (result.OutputFile);
			Type [] types = assembly.GetTypes ();
			foreach (Type t in types) {
				if (t.IsSubclassOf (typeof (UserControl))) {
					if (result.Data != null)
						throw new CompilationException ("More that 1 user control!!!", result);
					result.Data = t;
				}
			}

			return result.Data as Type;
		}

		public override string Key {
			get {
				return userControlParser.InputFile;
			}
		}

		public override string SourceFile {
			get {
				return sourceFile;
			}
		}

		public override string TargetFile {
			get {
				if (targetFile == null)
					targetFile = Path.ChangeExtension (sourceFile, ".dll");

				return targetFile;
			}
		}

		public static Type CompileUserControlType (UserControlParser userControlParser)
		{
			CompilationCacheItem item = CachingCompiler.GetCached (userControlParser.InputFile);
			if (item != null && item.Result != null) {
				if (item.Result != null) {
					userControlParser.Options = item.Result.Options;
					return item.Result.Data as Type;
				}

				throw new CompilationException (item.Result);
			}

			UserControlCompiler pc = new UserControlCompiler (userControlParser);
			return pc.GetCompiledType ();
		}

		string GenerateSourceFile (UserControlParser userControlParser)
		{
			string inputFile = userControlParser.InputFile;

			AspGenerator generator = new AspGenerator (inputFile);
			generator.Context = userControlParser.Context;
			generator.BaseType = userControlParser.BaseType.ToString ();
			generator.ProcessElements ();
			userControlParser.Text = generator.GetCode ().ReadToEnd ();
			options = generator.Options;
			dependencies = generator.Dependencies;

			//FIXME: should get Tmp dir for this application
			string csName = Path.GetTempFileName () + ".cs";
			WebTrace.WriteLine ("Writing {0}", csName);
			StreamWriter output = new StreamWriter (File.OpenWrite (csName));
			output.Write (userControlParser.Text);
			output.Close ();
			return csName;
		}
	}
}

