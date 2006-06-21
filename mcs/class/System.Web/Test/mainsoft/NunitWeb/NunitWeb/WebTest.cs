using System;
using System.Reflection;
using System.IO;
using System.Web.Hosting;

namespace MonoTests.SystemWeb.Framework
{
	public class WebTest
	{
		BaseInvoker _invoker;
		public BaseInvoker Invoker
		{
			get { return _invoker; }
			set { _invoker = value; }
		}

		BaseRequest _request;
		public BaseRequest Request
		{
			get { return _request; }
			set { _request = value; }
		}

		static MyHost host;
		private static MyHost Host
		{
			get {
				if (host != null)
					return host;
				host = AppDomain.CurrentDomain.GetData (HOST_INSTANCE_NAME) as MyHost;
				if (host != null)
					return host;
				host = CreateHosting ();
				CopyResources ();
				return host;
			}
		}

		public string Run ()
		{
			if (Request.Url == null)
				Request.Url = Invoker.GetDefaultUrl ();
			return Host.Run (Invoker, Request);
		}

		public static void Invoke (object param)
		{
			Host.Invoke (param);
		}

		public static void Unload ()
		{
			if (host == null)
				return;
			AppDomain.Unload (Host.AppDomain);
			AppDomain.CurrentDomain.SetData (HOST_INSTANCE_NAME, null);
			host = null;
		}

		public WebTest ()
		{
			Invoker = new BaseInvoker ();
			Request = new BaseRequest ();
		}

		public WebTest (string url)
			: this ()
		{
			Request.Url = url;
		}

		public WebTest (BaseInvoker invoker)
			: this ()
		{
			Invoker = invoker;
		}

		public WebTest (BaseRequest request)
			: this ()
		{
			Request = request;
		}

		static void LoadAssemblyRecursive (Assembly ass)
		{
			if (ass.GlobalAssemblyCache)
				return;
			foreach (AssemblyName ran in ass.GetReferencedAssemblies ()) {
				bool found = false;
				foreach (Assembly domain_ass in AppDomain.CurrentDomain.GetAssemblies ()) {
					if (domain_ass.FullName == ran.FullName) {
						found = true;
						break;
					}
				}
				if (found)
					continue;
				Assembly ra = Assembly.Load (ran, null);
				LoadAssemblyRecursive (ra);
			}
		}

		private static void CopyAssembly (Assembly ass, string dir)
		{
			if (ass.GlobalAssemblyCache)
				return;
			string oldfn = ass.Location;
			if (oldfn.EndsWith (".exe"))
				return;
			string newfn = Path.Combine (dir, Path.GetFileName (oldfn));
			if (File.Exists (newfn))
				return;
			File.Copy (oldfn, newfn);
		}

		private static void EnsureDirectoryExists (string directory)
		{
			if (directory == string.Empty)
				return;
			if (Directory.Exists (directory))
				return;
			EnsureDirectoryExists (Path.GetDirectoryName (directory));
			Directory.CreateDirectory (directory);
		}

		/// <summary>
		/// Copy a resource embedded in the assembly into the web application
		/// </summary>
		/// <param name="type">A type in the assembly that contains the embedded resource.</param>
		/// <param name="resourceName">The name of the resource.</param>
		/// <param name="targetUrl">The URL where the resource will be available</param>
		/// <exception cref="System.ArgumentException">Thrown when resource with name resourceName is not found.</exception>
		/// <example><code>CopyResource (GetType (), "Default.skin", "App_Themes/Black/Default.skin");</code></example>
		public static void CopyResource (Type type, string resourceName, string targetUrl)
		{
			EnsureHosting ();
			EnsureDirectoryExists (Path.Combine (baseDir,
				Path.GetDirectoryName (targetUrl)));
			using (Stream source = type.Assembly.GetManifestResourceStream (resourceName)) {
				if (source == null)
					throw new ArgumentException ("resource not found: " + resourceName, "resourceName");
				using (FileStream target = new FileStream (Path.Combine (baseDir, targetUrl), FileMode.CreateNew)) {
					byte[] array = new byte[source.Length];
					source.Read (array, 0, array.Length);
					target.Write (array, 0, array.Length);
				}
			}
		}

		private static void EnsureHosting ()
		{
			MyHost h = Host;
		}

		private static string baseDir;
		private static string binDir;
		const string VIRTUAL_BASE_DIR = "/NunitWeb";
		const string HOST_INSTANCE_NAME = "MonoTests/SysWeb/Framework/Host";

		private static MyHost CreateHosting ()
		{
			string tmpFile = Path.GetTempFileName ();
			File.Delete (tmpFile);
			Directory.CreateDirectory (tmpFile);
			baseDir = tmpFile;
			binDir = Directory.CreateDirectory (Path.Combine (baseDir, "bin")).FullName;

			foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies ())
				LoadAssemblyRecursive (ass);

			foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies ())
				CopyAssembly (ass, binDir);

			MyHost host = (MyHost) ApplicationHost.CreateApplicationHost (typeof (MyHost), VIRTUAL_BASE_DIR, baseDir);
			AppDomain.CurrentDomain.SetData (HOST_INSTANCE_NAME, host);
			host.AppDomain.SetData (HOST_INSTANCE_NAME, host);
			return host;
		}

		private static void CopyResources ()
		{
#if VISUAL_STUDIO
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.Web.config",
				"Web.config");
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.MyPage.aspx",
				"MyPage.aspx");
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.MyPage.aspx.cs",
				"MyPage.aspx.cs");
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.MyPageWithMaster.aspx",
				"MyPageWithMaster.aspx");
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.My.master",
				"My.master");
#else
			CopyResource (typeof (WebTest), "Web.config", "Web.config");
			CopyResource (typeof (WebTest), "MyPage.aspx", "MyPage.aspx");
			CopyResource (typeof (WebTest), "MyPage.aspx.cs", "MyPage.aspx.cs");
			CopyResource (typeof (WebTest), "MyPageWithMaster.aspx", "MyPageWithMaster.aspx");
			CopyResource (typeof (WebTest), "My.master", "My.master");
#endif
		}
	}
}
