#if NET_2_0
using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.Hosting;
using System.Threading;
using System.Web.Util;
using System.Collections.Specialized;

namespace NunitWeb
{
	public class Helper : MarshalByRefObject
	{
		public delegate void AnyMethod (HttpContext context, object anyParam);
		public delegate void AnyMethodInPage (HttpContext context, Page page, object anyParam);

		static Helper _instance;

		MyHost host;
		string baseDir;
		string binDir;

		const string VIRTUAL_BASE_DIR = "/NunitWeb";

		private Helper ()
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

#if VISUAL_STUDIO
			CopyResource (Assembly.GetExecutingAssembly (), "NunitWeb.Resources.Web.config", "Web.config");
			CopyResource (Assembly.GetExecutingAssembly (), "NunitWeb.Resources.MyPage.aspx", "MyPage.aspx");
			CopyResource (Assembly.GetExecutingAssembly (), "NunitWeb.Resources.MyPage.aspx.cs", "MyPage.aspx.cs");
			CopyResource (Assembly.GetExecutingAssembly (), "NunitWeb.Resources.MyPageWithMaster.aspx", "MyPageWithMaster.aspx");
			CopyResource (Assembly.GetExecutingAssembly (), "NunitWeb.Resources.MyPageWithMaster.aspx.cs", "MyPageWithMaster.aspx.cs");
			CopyResource (Assembly.GetExecutingAssembly (), "NunitWeb.Resources.My.master", "My.master");
#else
			CopyResource (Assembly.GetExecutingAssembly (), "Web.config", "Web.config");
			CopyResource (Assembly.GetExecutingAssembly (), "MyPage.aspx", "MyPage.aspx");
			CopyResource (Assembly.GetExecutingAssembly (), "MyPage.aspx.cs", "MyPage.aspx.cs");
			CopyResource (Assembly.GetExecutingAssembly (), "MyPageWithMaster.aspx", "MyPageWithMaster.aspx");
			CopyResource (Assembly.GetExecutingAssembly (), "MyPageWithMaster.aspx.cs", "MyPageWithMaster.aspx.cs");
			CopyResource (Assembly.GetExecutingAssembly (), "My.master", "My.master");
#endif

			host = (MyHost) ApplicationHost.CreateApplicationHost (typeof (MyHost), VIRTUAL_BASE_DIR, baseDir);

			host.Initialize (this);
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
			string oldfn = ass.ManifestModule.FullyQualifiedName;
			if (oldfn.EndsWith (".exe"))
				return;
			string newfn = Path.Combine (dir, Path.GetFileName (oldfn));
			if (File.Exists (newfn))
				return;
			File.Copy (oldfn, newfn);
		}




		static void EnsureDirectoryExists (string directory)
		{
			if (directory == string.Empty)
				return;
			if (Directory.Exists (directory))
				return;
			EnsureDirectoryExists (Path.GetDirectoryName (directory));
			Directory.CreateDirectory (directory);
		}

		public void CopyResource (Assembly ass, string resourceName, string newName)
		{
			EnsureDirectoryExists (Path.Combine (baseDir,
				Path.GetDirectoryName (newName)));
			using (Stream source = ass.GetManifestResourceStream (resourceName)) {
				if (source == null)
					throw new ArgumentException ("resource not found: "+resourceName, "resourceName");
				using (FileStream target = new FileStream (Path.Combine (baseDir, newName), FileMode.CreateNew)) {
					byte[] array = new byte[source.Length];
					source.Read (array, 0, array.Length);
					target.Write (array, 0, array.Length);
				}
			}
		}

		static public Helper Instance
		{
			get
			{
				if (_instance != null)
					return _instance;

				_instance = AppDomain.CurrentDomain.GetData (MyHost.HELPER_INSTANCE_NAME) as Helper;
				if (_instance == null)
					_instance = new Helper ();
				return _instance;
			}
		}

		static public void Unload ()
		{
			if (_instance == null)
				return;
			_instance.DoUnload ();
			_instance = null;
		}

		void DoUnload ()
		{
			AppDomain.Unload (host.AppDomain);
			Directory.Delete (baseDir, true);
		}

		public string RunUrl (string url, Delegate method, object anyParam)
		{
			try {
				return host.DoRun (url, method, anyParam);
			}
			catch (TargetInvocationException e) {
				if (e.InnerException != null)
					throw e.InnerException;
				else
					throw;
			}
		}

		public string Run (AnyMethod method)
		{
			return Run (method, null);
		}

		public string Run (AnyMethod method, object anyParam)
		{
			return RunUrl ("page.fake", method, anyParam);
		}

		public string RunInPage (AnyMethodInPage method)
		{
			return RunInPage (method, null);
		}

		public string RunInPage (AnyMethodInPage method, object param)
		{
			return RunUrl ("MyPage.aspx", method, param);
		}

		public string RunInPageWithMaster (AnyMethodInPage method)
		{
			return RunInPageWithMaster (method, null);
		}

		public string RunInPageWithMaster (AnyMethodInPage method, object param)
		{
			return RunUrl ("MyPageWithMaster.aspx", method, param);
		}

		public string GetResourceUrl (Type type, string resourceName)
		{
			string filename = Path.Combine (baseDir, resourceName);
			if (!File.Exists (filename))
				CopyResource (type.Assembly, resourceName, resourceName);
			return "/" + resourceName;
		}
	}
}
#endif
