//
// System.Web.Compilation.BaseCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Web.Compilation
{
	abstract class BaseCompiler
	{
		//FIXME: configurable?
		static string default_assemblies = "System.Web.dll, System.Data.dll, System.Drawing.dll";
		static Random rnd = new Random ((int) DateTime.Now.Ticks);
		string randomName;
		protected Hashtable options;
		protected ArrayList dependencies;

		protected BaseCompiler ()
		{
		}

		public virtual Type GetCompiledType ()
		{
			return null;
		}

		public virtual string Key {
			get {
				return null;
			}
		}

		public virtual string SourceFile {
			get {
				return null;
			}
		}

		public virtual string [] Dependencies {
			get {
				if (dependencies == null)
					return new string [0];

				return (string []) dependencies.ToArray (typeof (string));
			}
		}

		public virtual void AddDependency (string filename)
		{
			if (dependencies == null)
				dependencies = new ArrayList ();

			dependencies.Add (filename);
		}
		
		public virtual string CompilerOptions {
			get {
				string assemblies = default_assemblies;
				string privatePath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
				string appBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
				//HACK: we should use Uri (appBase).LocalPath once Uri works fine.
				if (appBase.StartsWith ("file://")) {
					appBase = appBase.Substring (7);
					if (Path.DirectorySeparatorChar == '\\')
						appBase = appBase.Replace ('/', '\\');
				}

				privatePath = Path.Combine (appBase, privatePath);

				if (privatePath != null && Directory.Exists (privatePath)) {
					StringBuilder sb = new StringBuilder (assemblies);
					foreach (string fileName in Directory.GetFiles (privatePath, "*.dll"))
						sb.AppendFormat (", {0}", fileName);
					assemblies = sb.ToString ();
					sb = null;
				}

				string [] split = assemblies.Split (',');
				StringBuilder result = new StringBuilder ();
				foreach (string assembly in split)
					result.AppendFormat ("/r:{0} ", assembly.TrimStart ());
				
				if (options == null)
					return result.ToString ();

				string compilerOptions = options ["CompilerOptions"] as string;
				if (compilerOptions != null) {
					result.Append (' ');
					result.Append (compilerOptions);
				}

				string references = options ["References"] as string;
				if (references == null)
					return result.ToString ();

				split = references.Split ('|');
				foreach (string s in split)
					result.AppendFormat (" /r:\"{0}\"", s);

				return result.ToString ();
			}
		}

		static string GetRandomFileName ()
		{
			string output;

			do { 
				output = "tmp" + rnd.Next () + ".dll";
			} while (File.Exists (output));

			return output;
		}

		public virtual string TargetFile {
			get {
				if (randomName == null)
					randomName = GetRandomFileName ();

				return randomName;
			}
		}
	}
}

