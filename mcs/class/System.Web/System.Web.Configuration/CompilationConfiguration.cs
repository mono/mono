//
// System.Web.Configuration.CompilationConfiguration
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2003 Ximian, Inc (http://www.ximian.com)
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

#if !NET_2_0
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml;

namespace System.Web.Configuration
{
	sealed class CompilationConfiguration
	{
		bool debug = false;
		bool batch = false;
		int batch_timeout;
		string default_language = "c#";
		bool _explicit = true;
		int max_batch_size = 30;
		int max_batch_file_size = 3000;
		int num_recompiles_before_app_restart = 15;
		bool strict = false;
		string temp_directory;
		CompilerCollection compilers;
		ArrayList assemblies;
		bool assembliesInBin = false;

		/* Only the config. handler should create instances of this. Use GetInstance (context) */
		public CompilationConfiguration (object p)
		{
			CompilationConfiguration parent = p as CompilationConfiguration;
			if (parent != null)
				Init (parent);

			if (compilers == null)
				compilers = new CompilerCollection ();

			if (assemblies == null)
				assemblies = new ArrayList ();

			if (temp_directory == null || temp_directory == "")
				temp_directory = AppDomain.CurrentDomain.SetupInformation.DynamicBase;
		}

		static public CompilationConfiguration GetInstance (HttpContext context)
		{
			CompilationConfiguration config;
			if (context == null)
				context = HttpContext.Current;

			if (context != null) {
				config = context.GetConfig ("system.web/compilation") as CompilationConfiguration;
				if (config == null)
					throw new Exception ("Configuration error.");
			} else {
				// empty config (as used in unit tests)
				config = new CompilationConfiguration (null);
			}

			return config;
		}
		
		public CodeDomProvider GetProvider (string language)
		{
			Compiler compiler = Compilers [language];
			if (compiler == null)
				return null;

			if (compiler.Provider != null)
				return compiler.Provider;

			Type t = Type.GetType (compiler.Type, true);
			compiler.Provider = Activator.CreateInstance (t) as CodeDomProvider;
			return compiler.Provider;
		}

		public string GetCompilerOptions (string language)
		{
			Compiler compiler = Compilers [language];
			if (compiler == null)
				return null;

			return compiler.CompilerOptions;
		}

		public int GetWarningLevel (string language)
		{
			Compiler compiler = Compilers [language];
			if (compiler == null)
				return 0;

			return compiler.WarningLevel;
		}

		void Init (CompilationConfiguration parent)
		{
			debug = parent.debug;
			batch = parent.batch;
			batch_timeout = parent.batch_timeout;
			default_language = parent.default_language;
			_explicit = parent._explicit;
			max_batch_size = parent.max_batch_size;
			max_batch_file_size = parent.max_batch_file_size;
			num_recompiles_before_app_restart = parent.num_recompiles_before_app_restart;
			strict = parent.strict;
			temp_directory = parent.temp_directory;
#if NET_2_0
			compilers = new CompilerCollection ();
#else
			compilers = new CompilerCollection (parent.compilers);
#endif
			ArrayList p = parent.assemblies;
			assembliesInBin = parent.assembliesInBin;
			ICollection coll = (p == null) ? (ICollection) new object [0] : p;
			assemblies = new ArrayList (coll);
		}

		public bool Debug {
			get { return debug; }
			set { debug = value; }
		}

		public bool Batch {
			get { return batch; }
			set { batch = value; }
		}

		public int BatchTimeout {
			get { return batch_timeout; }
			set { batch_timeout = value; }
		}

		public string DefaultLanguage {
			get { return default_language; }
			set { default_language = value; }
		}

		public bool Explicit {
			get { return _explicit; }
			set { _explicit = value; }
		}

		public int MaxBatchSize {
			get { return max_batch_size; }
			set { max_batch_size = value; }
		}

		public int MaxBatchFileSize {
			get { return max_batch_file_size; }
			set { max_batch_file_size = value; }
		}

		public int NumRecompilesBeforeAppRestart {
			get { return num_recompiles_before_app_restart; }
			set { num_recompiles_before_app_restart = value; }
		}

		public bool Strict {
			get { return strict; }
			set { strict = value; }
		}

		public string TempDirectory {
			get { return temp_directory; }
			set {
				if (value != null && !Directory.Exists (value))
					throw new ArgumentException ("Directory does not exist");

				Console.WriteLine ("Dos: '{0}'\n{1}", value, Environment.StackTrace);
				temp_directory = value;
			}
		}

		public CompilerCollection Compilers {
			get { return compilers; }
		}

		public ArrayList Assemblies {
			get { return assemblies; }
		}

		public bool AssembliesInBin {
			get { return assembliesInBin; }
			set { assembliesInBin = value; }
		}
	}
}
#endif
