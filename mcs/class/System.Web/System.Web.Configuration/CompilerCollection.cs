//
// System.Web.Configuration.CompilerCollection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003,2005 Novell, Inc (http://www.novell.com)
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
/* when NET_2_0 is defined, this class is found in System.Web.Configuration_2.0/ */

using System;
using System.Collections;
using System.Configuration;

namespace System.Web.Configuration
{
	sealed class CompilerCollection
	{
		Hashtable compilers;

		public CompilerCollection () : this (null) {}

		public CompilerCollection (CompilerCollection parent)
		{
			compilers = new Hashtable (CaseInsensitiveHashCodeProvider.DefaultInvariant,
						   CaseInsensitiveComparer.DefaultInvariant);

			if (parent != null && parent.compilers != null) {
				foreach (DictionaryEntry entry in parent.compilers)
					compilers [entry.Key] = entry.Value;
			}
		}

		public Compiler this [string language] {
			get { return compilers [language] as Compiler; }
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

		public bool CompareLanguages (string lang1, string lang2)
		{
			return (this [lang1] == this [lang2]);
		}
	}
}

#endif
