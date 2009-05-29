//
// System.Web.Compilation.BuildProvider
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Util;

namespace System.Web.Compilation
{
	public abstract class BuildProvider
	{
		ArrayList ref_assemblies;
		
		ICollection vpath_deps;
		CompilationSection compilationSection;

		VirtualPath vpath;
		
		CompilationSection CompilationConfig {
			get {
				if (compilationSection == null)
					compilationSection = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
				return compilationSection;
			}
		}
		
		internal virtual string LanguageName {
			get { return CompilationConfig.DefaultLanguage; }
		}
		
		protected BuildProvider()
		{
			ref_assemblies = new ArrayList ();
		}

		internal void SetVirtualPath (VirtualPath virtualPath)
		{
			vpath = virtualPath;
		}

		internal virtual void GenerateCode ()
		{
		}

		internal virtual IDictionary <string, bool> ExtractDependencies ()
		{
			return null;
		}
		
		public virtual void GenerateCode (AssemblyBuilder assemblyBuilder)
		{
		}

		public virtual string GetCustomString (CompilerResults results)
		{
			return null;
		}

		protected CompilerType GetDefaultCompilerType ()
		{
			return BuildManager.GetDefaultCompilerTypeForLanguage (CompilationConfig.DefaultLanguage, CompilationConfig);
		}
		
		protected CompilerType GetDefaultCompilerTypeForLanguage (string language)
		{
			return BuildManager.GetDefaultCompilerTypeForLanguage (language, null);
		}

		public virtual Type GetGeneratedType (CompilerResults results)
		{
			return null;
		}

		public virtual BuildProviderResultFlags GetResultFlags (CompilerResults results)
		{
			return BuildProviderResultFlags.Default;
		}

		protected TextReader OpenReader ()
		{
			return OpenReader (VirtualPath);
		}

		protected TextReader OpenReader (string virtualPath)
		{
			Stream st = OpenStream (virtualPath);
			return new StreamReader (st, WebEncoding.FileEncoding);
		}

		protected Stream OpenStream ()
		{
			return OpenStream (VirtualPath);
		}

		protected Stream OpenStream (string virtualPath)
		{
			// MS also throws a NullReferenceException here when not hosted.
			return VirtualPathProvider.OpenFile (virtualPath);
		}
		
		public virtual CompilerType CodeCompilerType {
			get { return null; } // Documented to return null
		}

		protected ICollection ReferencedAssemblies {
			get { return ref_assemblies; }
		}

		protected internal string VirtualPath {
			get { return vpath != null ? vpath.Absolute : null; }
		}

		internal VirtualPath VirtualPathInternal {
			get { return vpath; }
		}
		
		public virtual ICollection VirtualPathDependencies {
			get {
				if (vpath_deps == null)
					vpath_deps = new OneNullCollection ();

				return vpath_deps;
			}
		}

		internal virtual CodeCompileUnit CodeUnit {
			get { return null; }
		}
	}

	class OneNullCollection : ICollection {
		public int Count {
			get { return 1; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ();

			if (index < 0)
				throw new ArgumentOutOfRangeException ();

			if (array.Rank > 1)
				throw new ArgumentException ();

			int length = array.Length;
			if (index >= length || index > length - 1)
				throw new ArgumentException ();

			array.SetValue (null, index);
		}

		public IEnumerator GetEnumerator ()
		{
			yield return null;
		}
	}
}
#endif

