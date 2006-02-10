//
// System.Web.Compilation.BuildManager
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
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
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace System.Web.Compilation {
	public sealed class BuildManager {
		internal BuildManager ()
		{
		}

		internal static void ThrowNoProviderException (string extension)
		{
			string msg = "No registered provider for extension '{0}'.";
			throw new HttpException (String.Format (msg, extension));
		}

		public static object CreateInstanceFromVirtualPath (string virtualPath, Type requiredBaseType)
		{
			// virtualPath + Exists done in GetCompiledType()
			if (requiredBaseType == null)
				throw new NullReferenceException (); // This is what MS does, but from somewhere else.

			// Get the Type.
			Type type = GetCompiledType (virtualPath);
			if (!requiredBaseType.IsAssignableFrom (type)) {
				string msg = String.Format ("Type '{0}' does not inherit from '{1}'.",
								type.FullName, requiredBaseType.FullName);
				throw new HttpException (500, msg);
			}

			return Activator.CreateInstance (type, null);
		}

		[MonoTODO]
		public static BuildDependencySet GetCachedBuildDependencySet (HttpContext context, string virtualPath)
		{
			return null; // null is ok here until we store the dependency set in the Cache.
		}

		public static Assembly GetCompiledAssembly (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "")
				throw new ArgumentNullException ("virtualPath");

			if (virtualPath [0] != '/')
				throw new ArgumentException ("The virtual path is not rooted", "virtualPath");

			if (!HostingEnvironment.VirtualPathProvider.FileExists (virtualPath))
				throw new HttpException (String.Format ("The file '{0}' does not exist.", virtualPath));

			object o = WebConfigurationManager.GetSection ("system.web/compilation/buildProviders", virtualPath);
			BuildProviderCollection coll = (BuildProviderCollection) o;
			string extension = VirtualPathUtility.GetExtension (virtualPath);
			if (coll == null || coll.Count == 0)
				ThrowNoProviderException (extension);

			BuildProvider build_provider = coll.GetProviderForExtension (extension);
			if (build_provider == null)
				ThrowNoProviderException (extension);

			CompilerType compiler_type = build_provider.CodeCompilerType;
			Type ctype = compiler_type.CodeDomProviderType;
			CodeDomProvider dom_provider = (CodeDomProvider) Activator.CreateInstance (ctype, null);

			AssemblyBuilder abuilder = new AssemblyBuilder (virtualPath, dom_provider);
			build_provider.SetVirtualPath (virtualPath);
			build_provider.GenerateCode (abuilder);

			CompilerResults results;
			CompilerParameters parameters;
			parameters = PrepareParameters (abuilder.TempFiles, virtualPath, compiler_type.CompilerParameters);

			CodeCompileUnit [] units = abuilder.GetUnitsAsArray ();
			results = dom_provider.CompileAssemblyFromDom (parameters, units);
			// FIXME: generate the code and display it
			if (results.NativeCompilerReturnValue != 0)
				throw new CompilationException (virtualPath, results.Errors, "");

			Assembly assembly = results.CompiledAssembly;
			if (assembly == null) {
				if (!File.Exists (parameters.OutputAssembly)) {
					results.TempFiles.Delete ();
					throw new CompilationException (virtualPath, results.Errors,
						"No assembly returned after compilation!?");
				}

				assembly = Assembly.LoadFrom (parameters.OutputAssembly);
			}

			results.TempFiles.Delete ();
			return assembly;
		}

		[MonoTODO]
		public static string GetCompiledCustomString (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		static CompilerParameters PrepareParameters (TempFileCollection temp_files,
								string virtualPath,
								CompilerParameters base_params)
		{
			CompilerParameters res = new CompilerParameters ();
			res.TempFiles = temp_files;
			res.CompilerOptions = base_params.CompilerOptions;
			res.IncludeDebugInformation = base_params.IncludeDebugInformation;
			res.TreatWarningsAsErrors = base_params.TreatWarningsAsErrors;
			res.WarningLevel = base_params.WarningLevel;
			string dllfilename = Path.GetFileName (temp_files.AddExtension ("dll", true));
			res.OutputAssembly = Path.Combine (temp_files.TempDir, dllfilename);
			return res;
		}

		public static Type GetCompiledType (string virtualPath)
		{
			Assembly assembly = GetCompiledAssembly (virtualPath);
			Type [] types = assembly.GetTypes ();
			if (types.Length != 1)
				throw new CompilationException (virtualPath, null, "");

			return types [0];
		}

		// The 3 GetType() overloads work on the global.asax, App_GlobalResources, App_WebReferences or App_Browsers
		[MonoTODO]
		public static Type GetType (string typeName, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Type GetType (string typeName, bool throwOnError, bool ignoreCase)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ICollection GetVirtualPathDependencies (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		// Assemblies built from the App_Code directory
		[MonoTODO]
		public static IList CodeAssemblies {
			get {
				throw new NotImplementedException ();
			}
		}

	}
}

#endif

