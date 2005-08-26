//
// System.Web.HttpApplicationFactory
//
// TODO:
//   bin_watcher must work.
//   
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// (c) Copyright 2004 Novell, Inc. (http://www.novell.com)
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.SessionState;
using System.Web.Configuration;

#if !TARGET_J2EE
using System.Web.Compilation;
#else
using vmw.common;
#endif

namespace System.Web {
	class HttpApplicationFactory {
		// Initialized in InitType
		static bool needs_init = true;
		static Type app_type;
		static HttpApplicationState app_state;
		static FileSystemWatcher app_file_watcher;
		static FileSystemWatcher bin_watcher;
		static Stack available = new Stack ();
		
		// Watch this thing out when getting an instance
		static IHttpHandler custom_application;

		static bool IsEventHandler (MethodInfo m)
		{
			if (m.ReturnType != typeof (void))
				return false;

			ParameterInfo [] pi = m.GetParameters ();
			int length = pi.Length;
			if (length == 0)
				return true;

			if (length != 2)
				return false;

			if (pi [0].ParameterType != typeof (object) ||
			    pi [1].ParameterType != typeof (EventArgs))
				return false;
			
			return true;
		}

		static void AddEvent (MethodInfo method, Hashtable appTypeEventHandlers)
		{
			string name = method.Name.Replace ("_On", "_");
			if (appTypeEventHandlers [name] == null) {
				appTypeEventHandlers [name] = method;
				return;
			}

			MethodInfo old_method = appTypeEventHandlers [name] as MethodInfo;
			ArrayList list;
			if (old_method != null){
				list = new ArrayList (4);
				list.Add (old_method);
				appTypeEventHandlers [name] = list;
			} else 
				list = appTypeEventHandlers [name] as ArrayList;

			list.Add (method);
		}
		
		static Hashtable GetApplicationTypeEvents (HttpApplication app)
		{
			Type appType = app.GetType ();
			Hashtable appTypeEventHandlers = new Hashtable ();
			BindingFlags flags = BindingFlags.Public    |
					     BindingFlags.NonPublic | 
					     BindingFlags.Instance |
					     BindingFlags.Static;

			MethodInfo [] methods = appType.GetMethods (flags);
			foreach (MethodInfo m in methods) {
				if (IsEventHandler (m))
					AddEvent (m, appTypeEventHandlers);
			}

			return appTypeEventHandlers;
		}

		static bool FireEvent (string method_name, object target, object [] args)
		{
			Hashtable possibleEvents = GetApplicationTypeEvents ((HttpApplication) target);
			MethodInfo method = possibleEvents [method_name] as MethodInfo;
			if (method == null)
				return false;

			if (method.GetParameters ().Length == 0)
				args = null;

			method.Invoke (target, args);

			return true;
		}

		internal static void FireOnAppStart (HttpContext context)
		{
			HttpApplication app = (HttpApplication) Activator.CreateInstance (app_type, true);
			app.SetContext (context);
			object [] args = new object [] {app, EventArgs.Empty};
			FireEvent ("Application_Start", app, args);
			Recycle (app);
		}

		static void FireOnAppEnd ()
		{
			if (app_type == null)
				return; // we didn't even get an application

			HttpApplication app = (HttpApplication) Activator.CreateInstance (app_type, true);
			FireEvent ("Application_End", app, new object [] {new object (), EventArgs.Empty});
			app.Dispose ();
		}

		//
		// This is invoked by HttpRuntime.Dispose, when we unload an AppDomain
		// To reproduce this in action, touch "global.asax" while XSP is running.
		//
		public static void Dispose ()
		{
			FireOnAppEnd ();
		}
			
		static FileSystemWatcher CreateWatcher (string file, FileSystemEventHandler hnd)
		{
			FileSystemWatcher watcher = new FileSystemWatcher ();

			watcher.Path = Path.GetFullPath (Path.GetDirectoryName (file));
			watcher.Filter = Path.GetFileName (file);

			watcher.Changed += hnd;
			watcher.Created += hnd;
			watcher.Deleted += hnd;

			watcher.EnableRaisingEvents = true;

			return watcher;
		}

		static void OnAppFileChanged (object sender, FileSystemEventArgs args)
		{
			bin_watcher.EnableRaisingEvents = false;
			app_file_watcher.EnableRaisingEvents = false;
			HttpRuntime.UnloadAppDomain ();
		}

		internal static void AttachEvents (HttpApplication app)
		{
			Hashtable possibleEvents = GetApplicationTypeEvents (app);
			foreach (string key in possibleEvents.Keys) {
				int pos = key.IndexOf ('_');
				if (pos == -1 || key.Length <= pos + 1)
					continue;

				string moduleName = key.Substring (0, pos);
				object target;
				if (moduleName == "Application") {
					target = app;
				} else {
					target = app.Modules [moduleName];
					if (target == null)
						continue;
				}

				string eventName = key.Substring (pos + 1);
				EventInfo evt = target.GetType ().GetEvent (eventName);
				if (evt == null)
					continue;
			
				string usualName = moduleName + "_" + eventName;
				object methodData = possibleEvents [usualName];
				if (methodData == null)
					continue;

				if (methodData is MethodInfo) {
					AddHandler (evt, target, app, (MethodInfo) methodData);
					continue;
				}

				ArrayList list = (ArrayList) methodData;
				foreach (MethodInfo method in list)
					AddHandler (evt, target, app, method);
			}
		}

		static void AddHandler (EventInfo evt, object target, HttpApplication app, MethodInfo method)
		{
			int length = method.GetParameters ().Length;

			if (length == 0) {
				NoParamsInvoker npi = new NoParamsInvoker (app, method.Name);
				evt.AddEventHandler (target, npi.FakeDelegate);
			} else {
				evt.AddEventHandler (target, Delegate.CreateDelegate (
							typeof (EventHandler), app, method.Name));
			}
		}

		static HttpStaticObjectsCollection MakeStaticCollection (ArrayList list)
		{
			if (list == null || list.Count == 0)
				return null;

			HttpStaticObjectsCollection coll = new HttpStaticObjectsCollection ();
			foreach (ObjectTagBuilder tag in list) {
				coll.Add (tag);
			}

			return coll;
		}
		
		static internal HttpApplicationState ApplicationState {
			get {
				if (app_state == null) {
					HttpStaticObjectsCollection app = MakeStaticCollection (GlobalAsaxCompiler.ApplicationObjects);
					HttpStaticObjectsCollection ses = MakeStaticCollection (GlobalAsaxCompiler.SessionObjects);

					app_state = new HttpApplicationState (app, ses);
				}

				return app_state;
			}
		}

		public static void SetCustomApplication (IHttpHandler customApplication)
		{
			custom_application = customApplication;
		}

		static internal Type AppType {
			get {
				return app_type;
			}
		}

		static void InitType (HttpContext context)
		{
			lock (typeof (HttpApplicationFactory)){
				if (!needs_init)
					return;
				
				string physical_app_path = context.Request.PhysicalApplicationPath;
				string app_file;
				
				app_file = Path.Combine (physical_app_path, "Global.asax");
				if (!File.Exists (app_file))
					app_file = Path.Combine (physical_app_path, "global.asax");
				
				WebConfigurationSettings.Init (context);
				
				if (File.Exists (app_file)) {
					app_type = ApplicationFileParser.GetCompiledApplicationType (app_file, context);
					if (app_type == null) {
						string msg = String.Format ("Error compiling application file ({0}).", app_file);
						throw new ApplicationException (msg);
					}
					
					app_file_watcher = CreateWatcher (app_file, new FileSystemEventHandler (OnAppFileChanged));
				} else {
					app_type = typeof (System.Web.HttpApplication);
					app_state = new HttpApplicationState ();
				}
				needs_init = false;

				//
				// Now init the settings
				//

			}
		}
		
		//
		// Multiple-threads might hit this one on startup, and we have
		// to delay-initialize until we have the HttpContext
		//
		internal static HttpApplication GetApplication (HttpContext context)
		{
			if (needs_init){
				InitType (context);
				FireOnAppStart (context);
			}

			lock (typeof (HttpApplicationFactory)){
				if (available.Count > 0)
					return (HttpApplication) available.Pop ();
			}
			
			HttpApplication app = (HttpApplication) Activator.CreateInstance (app_type, true);

			return app;
		}

		internal static void Recycle (HttpApplication app)
		{
			lock (typeof (HttpApplication)){
				if (available.Count < 32)
					available.Push (app);
				else
					app.Dispose ();
			}
		}
	}
}
