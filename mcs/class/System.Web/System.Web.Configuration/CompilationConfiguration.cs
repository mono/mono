//
// System.Web.Configuration.CompilationConfiguration
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

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

			if (temp_directory == null)
				temp_directory = Path.GetTempPath ();
		}

		static public CompilationConfiguration GetInstance (HttpContext context)
		{
			CompilationConfiguration config;
			config = HttpContext.Context.GetConfig ("system.web/compilation")
							as CompilationConfiguration;

			if (config == null)
				throw new HttpException ("Configuration error.", 500);

			return config;
		}
		
		public CodeDomProvider GetProvider (string language)
		{
			WebCompiler compiler = Compilers [language];
			if (compiler == null)
				return null;

			if (compiler.Provider != null)
				return compiler.Provider;

			Type t = Type.GetType (compiler.Type);
			compiler.Provider = Activator.CreateInstance (t) as CodeDomProvider;
			return compiler.Provider;
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
			compilers = new CompilerCollection (parent.compilers);
			ArrayList p = parent.assemblies;
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
				if (value == null || value == "")
					value = Path.GetTempPath ();

				if (!Directory.Exists (value))
					throw new ArgumentException ("Directory does not exist");

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

