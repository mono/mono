//
// System.Web.Compilation.BuildManagerCacheItem
//
// Authors:
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2008-2009 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

namespace System.Web.Compilation 
{
	sealed class BuildManagerCacheItem
	{
		public readonly string CompiledCustomString;
		public readonly Assembly BuiltAssembly;
		public readonly string VirtualPath;
		public readonly Type Type;
		
		public BuildManagerCacheItem (Assembly assembly, BuildProvider bp, CompilerResults results)
		{
			this.BuiltAssembly = assembly;
			this.CompiledCustomString = bp.GetCustomString (results);
			this.VirtualPath = bp.VirtualPath;
			this.Type = bp.GetGeneratedType (results);
		}
			
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("BuildCacheItem [");
			bool first = true;
				
			if (!String.IsNullOrEmpty (CompiledCustomString)) {
				sb.Append ("compiledCustomString: " + CompiledCustomString);
				first = false;
			}
				
			if (BuiltAssembly != null) {
				sb.Append ((first ? "" : "; ") + "assembly: " + BuiltAssembly.ToString ());
				first = false;
			}

			if (!String.IsNullOrEmpty (VirtualPath)) {
				sb.Append ((first ? "" : "; ") + "virtualPath: " + VirtualPath);
				first = false;
			}

			sb.Append ("]");
				
			return sb.ToString ();
		}
	}
}

#endif