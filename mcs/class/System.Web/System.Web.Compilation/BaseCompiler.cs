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

namespace System.Web.Compilation
{
	abstract class BaseCompiler
	{
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
				return "/r:System.Web.dll /r:System.Data.dll /r:System.Drawing.dll";
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

