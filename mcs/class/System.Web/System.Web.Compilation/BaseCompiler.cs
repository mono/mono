//
// System.Web.Compilation.BaseCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
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
				return null;
			}
		}
		
		public virtual string CompilerOptions {
			get {
				string assemblies = default_assemblies;
				string privatePath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
				//FIXME: remove the next line once multiple appdomains can work together
				if (privatePath == null) privatePath = "bin";
				//
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

