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
	/// <summary>
	/// This class provides a high-level interface to NunitWeb framework.
	/// </summary>
	/// <remarks>This class is typically used from Nunit testcases</remarks>
	public class Helper : MarshalByRefObject
	{
		/// <summary>
		/// User callback that is executed in a web context.
		/// </summary>
		/// <param name="context">HttpContext</param>
		/// <param name="param">user data</param>
		/// <remarks>param must be serializable or MarshalByRef to pass AppDomain boundaries</remarks>
		public delegate void AnyMethod (HttpContext context, object param);
		/// <summary>
		/// User callback that is executed during page lifecycle.
		/// </summary>
		/// <param name="context">HttpContext</param>
		/// <param name="page">Page instance</param>
		/// <param name="anyParam">user data</param>
		/// <remarks>param must be serializable or MarshalByRef to pass AppDomain boundaries</remarks>
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
			CopyResource (Assembly.GetExecutingAssembly (), "NunitWeb.Resources.My.master", "My.master");
#else
			CopyResource (Assembly.GetExecutingAssembly (), "Web.config", "Web.config");
			CopyResource (Assembly.GetExecutingAssembly (), "MyPage.aspx", "MyPage.aspx");
			CopyResource (Assembly.GetExecutingAssembly (), "MyPage.aspx.cs", "MyPage.aspx.cs");
			CopyResource (Assembly.GetExecutingAssembly (), "MyPageWithMaster.aspx", "MyPageWithMaster.aspx");
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

		/// <summary>
		/// Copy a resource embedded in the assembly into the web application
		/// </summary>
		/// <param name="ass">assembly containing the resource</param>
		/// <param name="resourceName">name of the resource</param>
		/// <param name="targetUrl">the URL where the resource will be available</param>
		/// <exception cref="System.ArgumentException">Thrown when resource with name resourceName is not found.</exception>
		/// <example><code>CopyResource (Assembly.GetExecutingAssembly (), "Default.skin", "App_Themes/Black/Default.skin");</code></example>
		public void CopyResource (Assembly ass, string resourceName, string targetUrl)
		{
			EnsureDirectoryExists (Path.Combine (baseDir,
				Path.GetDirectoryName (targetUrl)));
			using (Stream source = ass.GetManifestResourceStream (resourceName)) {
				if (source == null)
					throw new ArgumentException ("resource not found: "+resourceName, "resourceName");
				using (FileStream target = new FileStream (Path.Combine (baseDir, targetUrl), FileMode.CreateNew)) {
					byte[] array = new byte[source.Length];
					source.Read (array, 0, array.Length);
					target.Write (array, 0, array.Length);
				}
			}
		}

		/// <summary>
		/// The instance of the Helper class.
		/// </summary>
		/// <remarks>If this property is accessed from a regular AppDomain,
		/// a new web application domain is created. If this property is accessed from
		/// the created AppDomain itself, this property returns the transparent proxy of
		/// the helper instance that created this AppDomain. The bottom line is, the instance
		/// is shared between the original AppDomain and the web application domain, and should
		/// work correctly from both of them.</remarks>
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

		/// <summary>
		/// Unload the web appplication domain and remove all files copied there.
		/// </summary>
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

		/// <summary>
		/// Request <paramref name="url"/> and run <paramref name="method"/> on the page Load event.
		/// </summary>
		/// <param name="url">The URL of the ASPX page to access.</param>
		/// <param name="method">user defined method that runs on the page Load event </param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunUrl (string url, AnyMethodInPage method)
		{
			return RunUrl (url, method, null);
		}

		/// <summary>
		/// Request <paramref name="url"/> and run <paramref name="method"/> on the page Load event, passing <paramref name="anyParam"/> to it.
		/// </summary>
		/// <param name="url">The URL of the ASPX page to access.</param>
		/// <param name="method">user defined method that runs on the page Load event </param>
		/// <param name="anyParam">User data passed to the <paramref name="method"/></param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunUrl (string url, AnyMethodInPage method, object anyParam)
		{
			PageDelegates pd = new PageDelegates ();
			pd.Param = anyParam;
			pd.Load += method;
			return RunUrlDelegates (url, pd);
		}

		/// <summary>
		/// Request <paramref name="url"/> and run <paramref name="method"/> on the page PreInit event.
		/// </summary>
		/// <param name="url">The URL of the ASPX page to access.</param>
		/// <param name="method">user defined method that runs on the page Load event </param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunUrlPreInit (string url, AnyMethodInPage method)
		{
			return RunUrlPreInit (url, method, null);
		}

		/// <summary>
		/// Request <paramref name="url"/> and run <paramref name="method"/> on the page Load event,  passing <paramref name="anyParam"/> to it.
		/// </summary>
		/// <param name="url">The URL of the ASPX page to access.</param>
		/// <param name="method">user defined method that runs on the page PreInit event </param>
		/// <param name="anyParam">User data passed to the <paramref name="method"/></param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunUrlPreInit (string url, AnyMethodInPage method, object anyParam)
		{
			PageDelegates pd = new PageDelegates ();
			pd.Param = anyParam;
			pd.PreInit += method;
			return RunUrlDelegates (url, pd);
		}

		/// <summary>
		/// Request the default page and run <paramref name="pd"/> on the corresponding page events.
		/// </summary>
		/// <param name="pd">Parameter delegates that run on corresponding page events.<see cref="NunitWeb.PageDelegates"/></param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunDelegates (PageDelegates pd)
		{
			return RunUrlDelegates ("MyPage.aspx", pd);
		}

		/// <summary>
		/// Request <paramref name="url"/> and run <paramref name="pd"/> on the corresponding page events.
		/// </summary>
		/// <param name="url">The URL of the ASPX page to access.</param>
		/// <param name="pd">Parameter delegates that run on corresponding page events.<see cref="NunitWeb.PageDelegates"/></param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunUrlDelegates (string url, PageDelegates pd)
		{
			try {
				return host.DoRun (url, pd);
			}
			catch (TargetInvocationException e) {
				if (e.InnerException != null)
					throw e.InnerException;
				else
					throw;
			}
		}

		/// <summary>
		/// Call the <paramref name="method"/> from the IHttpHandler.ProcessRequest.
		/// </summary>
		/// <param name="method">user defined method</param>
		/// <remarks>
		/// This method unlike <seealso cref="RunInPage (AnyMethodInPage)"/> does not compile and
		/// does not pass <see cref="System.Web.UI.Page"/> to the <paramref name="method"/>.
		/// </remarks>
		/// <returns>The response contents.</returns>
		public string Run (AnyMethod method)
		{
			return Run (method, null);
		}

		/// <summary>
		/// Call the <paramref name="method"/> from the IHttpHandler.ProcessRequest,
		/// passing <paramref name="anyParam"/> to it.
		/// </summary>
		/// <param name="method">user defined method</param>
		/// <param name="anyParam">user data passed to the <paramref name="method"/></param>
		/// <remarks>
		/// This method unlike <seealso cref="RunInPage(AnyMethodInPage, object)"/> does not compile and
		/// does not pass <see cref="System.Web.UI.Page"/> to the <paramref name="method"/>.
		/// </remarks>
		/// <returns>The response contents.</returns>
		public string Run (AnyMethod method, object anyParam)
		{
			PageDelegates pd = new PageDelegates ();
			pd.MyHandlerCallback = method;
			pd.Param = anyParam;
			return RunUrlDelegates ("page.fake", pd);
		}

		/// <summary>
		/// Request the default page and run <paramref name="method"/> on the page PreInit event.
		/// </summary>
		/// <param name="method">user defined method that runs on the page PreInit event </param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunInPagePreInit (AnyMethodInPage method)
		{
			return RunInPagePreInit (method, null);
		}

		/// <summary>
		/// Request the default page and run <paramref name="method"/> on the page PreInit event,
		/// passing <paramref name="anyParam"/> to it.
		/// </summary>
		/// <param name="method">user defined method that runs on the page PreInit event </param>
		/// <param name="anyParam">user data passed to the <paramref name="method"/></param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunInPagePreInit (AnyMethodInPage method, object anyParam)
		{
			return RunUrlPreInit ("MyPage.aspx", method, anyParam);
		}

		/// <summary>
		/// Request the default page and run <paramref name="method"/> on the page Load event.
		/// </summary>
		/// <param name="method">user defined method that runs on the page PreInit event </param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunInPage (AnyMethodInPage method)
		{
			return RunInPage (method, null);
		}

		/// <summary>
		/// Request the default page and run <paramref name="method"/> on the page Load event,
		/// passing <paramref name="anyParam"/> to it.
		/// </summary>
		/// <param name="method">user defined method that runs on the page PreInit event </param>
		/// <param name="anyParam">user data passed to the <paramref name="method"/></param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunInPage (AnyMethodInPage method, object anyParam)
		{
			return RunUrl ("MyPage.aspx", method, anyParam);
		}

		/// <summary>
		/// Request the default page with the default master page and run <paramref name="method"/>
		/// on the page Load event.
		/// </summary>
		/// <param name="method">user defined method that runs on the page PreInit event </param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunInPageWithMaster (AnyMethodInPage method)
		{
			return RunInPageWithMaster (method, null);
		}

		/// <summary>
		/// Request the default page with the default master page and run <paramref name="method"/>
		/// on the page Load event, passing <paramref name="anyParam"/> to it.
		/// </summary>
		/// <param name="method">user defined method that runs on the page PreInit event </param>
		/// <param name="anyParam">user data passed to the <paramref name="method"/></param>
		/// <returns>The response contents (usually HTML rendered by the page).</returns>
		public string RunInPageWithMaster (AnyMethodInPage method, object anyParam)
		{
			return RunUrl ("MyPageWithMaster.aspx", method, anyParam);
		}

		/// <summary>
		/// Returns URL that can be used to access the specified resource
		/// </summary>
		/// <param name="type">
		/// The type in the assembly that contains the embedded resource.
		/// </param>
		/// <param name="resourceName">
		/// The name of the resource to retrieve.
		/// </param>
		/// <returns>The URL string</returns>
		public string GetResourceUrl (Type type, string resourceName)
		{
			string filename = Path.Combine (baseDir, resourceName);
			if (!File.Exists (filename))
				CopyResource (type.Assembly, resourceName, resourceName);
			return "/" + resourceName;
		}

		/// <summary>
		/// This function is called from the custom page code behind constructor
		/// to init all the delegates, passed by <c>RunXXXDelegates</c>.
		/// </summary>
		/// <param name="context"><see cref="Page.Context"/> value.</param>
		/// <param name="page"><see cref="Page"/> instance.</param>
		static public void InitDelegates (HttpContext context, Page page)
		{
			new MyHost.DelegateInvoker (context, page);
		}
	}
}
#endif
