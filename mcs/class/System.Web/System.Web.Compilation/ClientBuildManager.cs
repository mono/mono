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
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Util;

namespace System.Web.Compilation {
	public sealed class ClientBuildManager : MarshalByRefObject, IDisposable {
		static readonly object appDomainShutdownEvent = new object ();
		static readonly object appDomainStartedEvent = new object ();
		static readonly object appDomainUnloadedEvent = new object ();
		
		string virt_dir;
		string phys_src_dir;
		//string phys_target_dir;
		//ClientBuildManagerParameter build_params;
		BareApplicationHost host;
		ApplicationManager manager;
		string app_id;
		string cache_path;

		EventHandlerList events = new EventHandlerList ();
		
		public event BuildManagerHostUnloadEventHandler AppDomainShutdown {
			add { events.AddHandler (appDomainShutdownEvent, value); }
			remove { events.RemoveHandler (appDomainShutdownEvent, value); }
		}
		
		public event EventHandler AppDomainStarted {
			add { events.AddHandler (appDomainStartedEvent, value); }
			remove { events.RemoveHandler (appDomainStartedEvent, value); }
		}
		
		public event BuildManagerHostUnloadEventHandler AppDomainUnloaded {
			add { events.AddHandler (appDomainUnloadedEvent, value); }
			remove { events.RemoveHandler (appDomainUnloadedEvent, value); }
		}

		public ClientBuildManager (string appVirtualDir, string appPhysicalSourceDir)
		{
			if (appVirtualDir == null || appVirtualDir == "")
				throw new ArgumentNullException ("appVirtualDir");
			if (appPhysicalSourceDir == null || appPhysicalSourceDir == "")
				throw new ArgumentNullException ("appPhysicalSourceDir");

			virt_dir = appVirtualDir; // TODO: adjust vpath (it allows 'blah' that turns into '/blah', '////blah', '\\blah'...
			phys_src_dir = appPhysicalSourceDir;
			manager = ApplicationManager.GetApplicationManager ();
		}

		public ClientBuildManager (string appVirtualDir, string appPhysicalSourceDir,
					   string appPhysicalTargetDir)
			: this (appVirtualDir, appPhysicalSourceDir)
		{
			if (appPhysicalTargetDir == null || appPhysicalTargetDir == "")
				throw new ArgumentNullException ("appPhysicalTargetDir");

			//phys_target_dir = appPhysicalTargetDir;
		}

		public ClientBuildManager (string appVirtualDir, string appPhysicalSourceDir,
					string appPhysicalTargetDir, ClientBuildManagerParameter parameter)
			: this (appVirtualDir, appPhysicalSourceDir, appPhysicalTargetDir)
		{
			//build_params = parameter;
		}

		BareApplicationHost Host {
			get {
				if (host != null)
					return host;

				int hashcode = virt_dir.GetHashCode ();
				if (app_id != null)
					hashcode ^= Int32.Parse (app_id);

				app_id = hashcode.ToString (Helpers.InvariantCulture);
				host = manager.CreateHostWithCheck (app_id, virt_dir, phys_src_dir);
				cache_path = "";
				//cache_path = Path.Combine (Path.GetTempPath (),
					//String.Format ("{0}-temp-aspnet-{1:x}", Environment.UserName, i));

				int hash = virt_dir.GetHashCode () << 5 + phys_src_dir.GetHashCode ();
				cache_path = Path.Combine (cache_path, hash.ToString (Helpers.InvariantCulture));
				Directory.CreateDirectory (cache_path);
				OnAppDomainStarted ();
				return host;
			}
		}

		void OnAppDomainStarted ()
		{
			EventHandler eh = events [appDomainStartedEvent] as EventHandler;
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		void OnAppDomainShutdown (ApplicationShutdownReason reason)
		{
			BuildManagerHostUnloadEventHandler eh = events [appDomainShutdownEvent] as BuildManagerHostUnloadEventHandler;
			if (eh != null) {
				BuildManagerHostUnloadEventArgs args = new BuildManagerHostUnloadEventArgs (reason);
				eh (this, args);
			}
		}

//		void OnDomainUnload (object sender, EventArgs args)
//		{
//			OnAppDomainUnloaded (0); // FIXME: set a reason?
//		}
//
//		void OnAppDomainUnloaded (ApplicationShutdownReason reason)
//		{
//			if (AppDomainUnloaded != null) {
//				BuildManagerHostUnloadEventArgs args = new BuildManagerHostUnloadEventArgs (reason);
//				AppDomainUnloaded (this, args);
//			}
//		}

		[MonoTODO ("Not implemented")]
		public void CompileApplicationDependencies ()
		{
			throw new NotImplementedException ();
		}

		public void CompileFile (string virtualPath)
		{
			CompileFile (virtualPath, null);
		}

		[MonoTODO ("Not implemented")]
		public void CompileFile (string virtualPath, ClientBuildManagerCallback callback)
		{
			// 1. Creates the Host
			// This creates a .compiled file + an assembly
			// App_Code reported to be built *before* anything else (with progress callback)
			throw new NotImplementedException ();
		}

		public IRegisteredObject CreateObject (Type type, bool failIfExists)
		{
			return manager.CreateObject (app_id, type, virt_dir, phys_src_dir, failIfExists);
		}

		[MonoTODO("Currently does not return the GeneratedCode")]
		public string GenerateCode (string virtualPath, string virtualFileString, out IDictionary linePragmasTable)
		{
			// This thing generates a .ccu (CodeCompileUnit?) file (reported as TrueType font data by 'file'!)
			// resultType=7 vs. resultType=3 for assemblies reported in the .compiled file
			// The virtual path is just added to the dependencies list, but is checked to be an absolute path.
			// IsHostCreated returns true after calling this method.
			//
			// A null file string causes virtualPath to be mapped and read to generate the code
			//

			if (String.IsNullOrEmpty (virtualPath))
				throw new ArgumentNullException ("virtualPath");

			Type cprovider_type;
			CompilerParameters parameters;
			GenerateCodeCompileUnit (virtualPath, virtualFileString, out cprovider_type,
						 out parameters, out linePragmasTable);
			return null;
		}

		[MonoTODO ("Not implemented")]
		public CodeCompileUnit GenerateCodeCompileUnit (string virtualPath,
								string virtualFileString,
								out Type codeDomProviderType,
								out CompilerParameters compilerParameters,
								out IDictionary linePragmasTable)
		{
			throw new NotImplementedException ();
		}

		public CodeCompileUnit GenerateCodeCompileUnit (string virtualPath,
								out Type codeDomProviderType,
								out CompilerParameters compilerParameters,
								out IDictionary linePragmasTable)
		{
			return GenerateCodeCompileUnit (virtualPath, out codeDomProviderType,
							out compilerParameters, out linePragmasTable);
		}

		static string [] shutdown_directories = new string [] {
						"bin", "App_GlobalResources", "App_Code",
						"App_WebReferences", "App_Browsers" };

		public string [] GetAppDomainShutdownDirectories ()
		{
			return shutdown_directories;
		}

		[MonoTODO ("Not implemented")]
		public IDictionary GetBrowserDefinitions ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public void GetCodeDirectoryInformation (string virtualCodeDir,
							out Type codeDomProviderType,
							out CompilerParameters compilerParameters,
							out string generatedFilesDir)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public Type GetCompiledType (string virtualPath)
		{
			// CompileFile + get the type based on .compiled file information
			// Throws if virtualPath is a special virtual directory (App_Code et al)
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public void GetCompilerParameters (string virtualPath,
						out Type codeDomProviderType,
						out CompilerParameters compilerParameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public string GetGeneratedFileVirtualPath (string filePath)
		{
			// returns empty string for any vpath. Test with real paths.
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public string GetGeneratedSourceFile (string virtualPath)
		{
			// This one takes a directory name /xxx and /xxx/App_Code throw either
			// a KeyNotFoundException or an InvalidOperationException
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public string [] GetTopLevelAssemblyReferences (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		public string [] GetVirtualCodeDirectories ()
		{
			// Host is created here when needed. (Unload()+GetVirtualCodeDirectories()+IsHostCreated -> true)
			//return Host.
			throw new NotImplementedException ();
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}

		[MonoTODO ("Not implemented")]
		public bool IsCodeAssembly (string assemblyName)
		{
			// Trying all the assemblies loaded by FullName and GetName().Name yield false here :-?
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public void PrecompileApplication ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public void PrecompileApplication (ClientBuildManagerCallback callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public void PrecompileApplication (ClientBuildManagerCallback callback, bool forceCleanBuild)
		{
			throw new NotImplementedException ();
		}

		public bool Unload ()
		{
			if (host != null) {
				host.Shutdown ();
				OnAppDomainShutdown (0);
				host = null;
			}

			return true; // FIXME: may be we should do this synch. + timeout? Test!
		}

		public string CodeGenDir {
			get { return Host.GetCodeGenDir (); }
		}

		public bool IsHostCreated {
			get { return host != null; }
		}

		void IDisposable.Dispose ()
		{
			Unload ();
		}
	}

}
#endif

