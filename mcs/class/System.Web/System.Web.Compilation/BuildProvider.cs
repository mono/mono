//
// System.Web.Compilation.BuildProvider
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.IO;

namespace System.Web.Compilation {

	public abstract class BuildProvider {
		protected BuildProvider()
		{
		}

		[MonoTODO]
		public virtual void GenerateCode (AssemblyBuilder assemblyBuilder)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetCustomString (CompilerResults results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected CompilerType GetDefaultCompilerType ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected CompilerType GetDefaultCompilerTypeForLanguage (string language)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Type GetGeneratedType (CompilerResults results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual BuildProviderResultFlags GetResultFlags (CompilerResults results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected TextReader OpenReader ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected TextReader OpenReader (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected Stream OpenStream ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected Stream OpenStream (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual CompilerType CodeCompilerType {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected ICollection ReferencedAssemblies {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected internal string VirtualPath {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual ICollection VirtualPathDependencies {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

#endif
