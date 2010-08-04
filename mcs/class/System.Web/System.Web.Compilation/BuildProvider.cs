//
// System.Web.Compilation.BuildProvider
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2006-2009 Novell, Inc (http://www.novell.com)
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
#if NET_4_0
		static Dictionary <string, Type> registeredBuildProviderTypes;
#endif
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
#if NET_4_0
		public static void RegisterBuildProvider (string extension, Type providerType)
		{
			if (String.IsNullOrEmpty (extension))
				throw new ArgumentException ("The string parameter 'extension' cannot be null or empty.", "extension");

			if (providerType == null)
				throw new ArgumentNullException ("providerType");

			if (!typeof (BuildProvider).IsAssignableFrom (providerType))
				throw new ArgumentException ("The parameter 'providerType' is invalid", "providerType");

			if (!BuildManager.PreStartMethodsRunning)
				throw new InvalidOperationException ("This method cannot be called during the application's pre-start initialization stage.");

			if (registeredBuildProviderTypes == null)
				registeredBuildProviderTypes = new Dictionary <string, Type> (StringComparer.OrdinalIgnoreCase);

			registeredBuildProviderTypes [extension] = providerType;
		}

		internal static Type GetProviderTypeForExtension (string extension)
		{
			if (String.IsNullOrEmpty (extension))
				return null;

			Type type = null;
			if (registeredBuildProviderTypes == null || !registeredBuildProviderTypes.TryGetValue (extension, out type) || type == null) {
				var cs = WebConfigurationManager.GetSection ("system.web/compilation") as CompilationSection;
				BuildProviderCollection bpcoll = cs != null ? cs.BuildProviders : null;
				global::System.Web.Configuration.BuildProvider bpcfg = bpcoll != null ? bpcoll [extension] : null;
				if (bpcfg != null)
					type = HttpApplication.LoadType (bpcfg.Type);
			}

			return type;
		}
		
		internal static BuildProvider GetProviderInstanceForExtension (string extension)
		{
			Type type = GetProviderTypeForExtension (extension);
			if (type == null)
				return null;
			
			return Activator.CreateInstance (type, null) as global::System.Web.Compilation.BuildProvider;
		}
#endif
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


