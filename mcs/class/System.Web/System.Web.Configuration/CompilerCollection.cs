//
// System.Web.Configuration.CompilerCollection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;

namespace System.Web.Configuration
{
	sealed class CompilerCollection
	{
		Hashtable compilers;

		public CompilerCollection () : this (null) {}

		public CompilerCollection (CompilerCollection parent)
		{
			compilers = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
						   CaseInsensitiveComparer.Default);

			if (parent != null && parent.compilers != null) {
				foreach (DictionaryEntry entry in parent.compilers)
					compilers [entry.Key] = entry.Value;
			}
		}

		public WebCompiler this [string language] {
			get { return compilers [language] as WebCompiler; }
			set {
				compilers [language] = value;
				string [] langs = language.Split (';');
				foreach (string s in langs) {
					string x = s.Trim ();
					if (x != "")
						compilers [x] = value;
				}
			}
		}
	}
}

