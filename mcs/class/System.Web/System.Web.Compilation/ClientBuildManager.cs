//
// System.Web.Compilation.ClientBuildManager
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
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace System.Web.Compilation {
	public sealed class ClientBuildManager : MarshalByRefObject, IDisposable {
		string virt_dir;
		string phys_src_dir;
		string phys_target_dir;
		ClientBuildManagerParameter build_params;

		public ClientBuildManager (string appVirtualDir, string appPhysicalSourceDir)
		{
			virt_dir = appVirtualDir; // TODO: adjust vpath (it allows 'blah' that turns into '/blah', '////blah', '\\blah'...
			phys_src_dir = appPhysicalSourceDir;
		}

		public ClientBuildManager (string appVirtualDir, string appPhysicalSourceDir,
					string appPhysicalTargetDir)
			: this (appVirtualDir, appPhysicalSourceDir)
		{
			phys_target_dir = appPhysicalTargetDir;
		}

		public ClientBuildManager (string appVirtualDir, string appPhysicalSourceDir,
					string appPhysicalTargetDir, ClientBuildManagerParameter parameter)
			: this (appVirtualDir, appPhysicalSourceDir, appPhysicalTargetDir)
		{
			build_params = parameter;
		}

		string CreateCodeGenDir ()
		{
			return null;
		/*
			string appname = null;
			if (virt_dir == "/") {
				appname = "root";
			} else {
				appname = virt_dir.Subtring (1).Replace ('/', '_');
			}

			string dynamic_dir = null;
			string user = Environment.UserName;
			for (int i = 0; ; i++){
				string d = Path.Combine (Path.GetTempPath (),
					String.Format ("{0}-temp-aspnet-{1:x}", user, i));
			
				try {
					Directory.CreateDirectory (d);
					string stamp = Path.Combine (d, "stamp");
					Directory.CreateDirectory (stamp);
					dynamic_dir = d;
					Directory.Delete (stamp);
					break;
				} catch (UnauthorizedAccessException){
					continue;
				}
			}
			setup.DynamicBase = dynamic_dir;
			Directory.CreateDirectory (setup.DynamicBase);
			*/
		}

		[MonoTODO]
		public void CompileApplicationDependencies ()
		{
			throw new NotImplementedException ();
		}

		public void CompileFile (string virtualPath)
		{
			CompileFile (virtualPath, null);
		}

		[MonoTODO]
		public void CompileFile (string virtualPath, ClientBuildManagerCallback callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IRegisteredObject CreateObject (Type type, bool failIfExists)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GenerateCode (string virtualPath, string virtualFileString, out IDictionary linePragmasTable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public CodeCompileUnit GenerateCodeCompileUnit (string virtualPath, string virtualFileString, out Type codeDomProviderType, out CompilerParameters compilerParameters, out IDictionary linePragmasTable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public CodeCompileUnit GenerateCodeCompileUnit (string virtualPath, out Type codeDomProviderType, out CompilerParameters compilerParameters, out IDictionary linePragmasTable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string [] GetAppDomainShutdownDirectories ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDictionary GetBrowserDefinitions ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetCodeDirectoryInformation (string virtualCodeDir, out Type codeDomProviderType, out CompilerParameters compilerParameters, out string generatedFilesDir)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Type GetCompiledType (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetCompilerParameters (string virtualPath, out Type codeDomProviderType, out CompilerParameters compilerParameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetGeneratedFileVirtualPath (string filePath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetGeneratedSourceFile (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string[ ] GetTopLevelAssemblyReferences (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string[ ] GetVirtualCodeDirectories ()
		{
			throw new NotImplementedException ();
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}

		[MonoTODO]
		public bool IsCodeAssembly (string assemblyName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PrecompileApplication ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PrecompileApplication (ClientBuildManagerCallback callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PrecompileApplication (ClientBuildManagerCallback callback, bool forceCleanBuild)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Unload ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string CodeGenDir {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool IsHostCreated {
			get {
				throw new NotImplementedException ();
			}
		}

		public event BuildManagerHostUnloadEventHandler AppDomainShutdown;
		public event EventHandler AppDomainStarted;
		public event BuildManagerHostUnloadEventHandler AppDomainUnloaded;


		[MonoTODO]
		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}

	}

}

#endif
