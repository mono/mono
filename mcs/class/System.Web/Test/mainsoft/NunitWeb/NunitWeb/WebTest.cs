using System;
using System.Reflection;
using System.IO;
using System.Web.Hosting;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// The most important class from user perspective. See <seealso cref="Request"/>,
	/// <seealso cref="Response"/>, <seealso cref="Invoker"/>, <seealso cref="Run"/> for
	/// more information.
	/// </summary>
	[Serializable]
	public class WebTest
	{
		object _userData;
		/// <summary>
		/// Any user-defined data. Must be serializable to pass between appdomains.
		/// </summary>
		/// <example>
		/// [Test]
		/// public void SampleTest ()
		/// {
		///	WebTest t = new WebTest (new HandlerInvoker (MyCallback));
		///	t.Run ();
		///	Assert.AreEqual ("Was here", t.UserData.ToString());
		/// }
		/// 
		/// static public void MyCallback ()
		/// {
		///	WebTest.CurrentTest.UserData = "Was here";
		/// }
		/// </example>

		public object UserData
		{
			get { return _userData; }
			set { _userData = value; }
		}

		Response _response;
		/// <summary>
		/// The result of the last <seealso cref="Run"/>. See <seealso cref="MonoTests.SystemWeb.Framework.Response"/>,
		/// <seealso cref="FormRequest"/>.
		/// </summary>
		public Response Response
		{
			get { return _response; }
			set { _response = value; }
		}

		BaseInvoker _invoker;
		/// <summary>
		/// Set the invoker, which is executed in the web context by <seealso cref="Invoke"/>
		/// method. Most commonly used <seealso cref="PageInvoker"/>. See also: <seealso cref="BaseInvoker"/>,
		/// <seealso cref="HandlerInvoker"/>
		/// </summary>
		public BaseInvoker Invoker
		{
			get { return _invoker; }
			set { _invoker = value; }
		}

		BaseRequest _request;
		/// <summary>
		/// Contains all the data necessary to create an <seealso cref="System.Web.HttpWorkerRequest"/> in
		/// the application appdomain. See also <seealso cref="BaseRequest"/>,
		/// <seealso cref="PostableRequest"/>, <seealso cref="FormRequest"/>.
		/// </summary>
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
#if !TARGET_JVM
				host = AppDomain.CurrentDomain.GetData (HOST_INSTANCE_NAME) as MyHost;
				if (host != null)
					return host;
#endif
				try {
					host = new MyHost (); //Fake instance to make EnsureHosting happy
					host = CreateHosting ();
				}
				catch {
					host = null; //Remove the fake instance if CreateHosting failed
					throw;
				}
				return host;
			}
		}

		/// <summary>
		/// Run the request using <seealso cref="Request"/> and <seealso cref="Invoker"/>
		/// values. Keep the result of the request in <seealso cref="Response"/> property.
		/// </summary>
		/// <returns>The body of the HTTP response (<seealso cref="MonoTests.SystemWeb.Framework.Response.Body"/>).</returns>
		public string Run ()
		{
			if (Request.Url == null)
				Request.Url = Invoker.GetDefaultUrl ();
			WebTest newTestInstance = Host.Run (this);
			CopyFrom (newTestInstance);
			return _response.Body;
		}

		private void CopyFrom (WebTest newTestInstance)
		{
			this._invoker = newTestInstance._invoker;
			this._request = newTestInstance._request;
			this._response = newTestInstance._response;
			this._userData = newTestInstance._userData;
		}

		/// <summary>
		/// The instance of the currently running test. Defined only in the web appdomain.
		/// In different threads this property may have different values.
		/// </summary>
		public static WebTest CurrentTest
		{
			get { return MyHost.GetCurrentTest (); }
		}

		/// <summary>
		/// This method must be called when custom <seealso cref="System.Web.IHttpHandler.ProcessRequest"/> or aspx code behind is used,
		/// to allow the framework to invoke all user supplied delegates.
		/// </summary>
		/// <param name="param">Parameter defined by the <seealso cref="BaseInvoker"/> subclass. For example,
		/// <seealso cref="PageInvoker"/> expects to receive a <seealso cref="System.Web.UI.Page"/> instance here.</param>
		public void Invoke (object param)
		{
			try {
				Invoker.DoInvoke (param);
			}
			catch (Exception ex) {
				RegisterException (ex);
				throw;
			}
		}

		/// <summary>
		/// This method is intended for use from <seealso cref="MonoTests.SystemWeb.Framework.BaseInvoker.DoInvoke"/> when
		/// the invocation causes an exception. In such cases, the exception must be registered
		/// with this method, and then swallowed. Before returning, <seealso cref="WebTest.Run"/>
		/// will rethrow this exception. This is done to hide the exception from <seealso cref="System.Web.HttpRuntime"/>,
		/// which normally swallows the exception and returns 500 ERROR http result.
		/// </summary>
		/// <param name="ex">The exception to be registered and rethrown.</param>
		public static void RegisterException (Exception ex)
		{
			Host.RegisterException (ex);
		}

		/// <summary>
		/// Unload the web appdomain and delete the temporary application root
		/// directory.
		/// </summary>
		public static void Unload ()
		{
			if (host == null)
				return;

			AppDomain oldDomain = host.AppDomain;
			host = null;
#if !TARGET_JVM
			AppDomain.CurrentDomain.SetData (HOST_INSTANCE_NAME, null);
			AppDomain.Unload (oldDomain);
#endif
			Directory.Delete (baseDir, true);
		}

		/// <summary>
		/// Default constructor. Initializes <seealso cref="Invoker"/> with a new
		/// <seealso cref="BaseInvoker"/> and <seealso cref="Request"/> with an empty
		/// <seealso cref="BaseRequest"/>.
		/// </summary>
		public WebTest ()
		{
			Invoker = new BaseInvoker ();
			Request = new BaseRequest ();
		}

		/// <summary>
		/// Same as <seealso cref="WebTest()"/>, and set <seealso cref="MonoTests.SystemWeb.Framework.BaseRequest.Url"/> to
		/// the specified Url.
		/// </summary>
		/// <param name="url">The URL used for the next <seealso cref="Run"/></param>
		public WebTest (string url)
			: this ()
		{
			Request.Url = url;
		}

		/// <summary>
		/// Create a new instance, initializing <seealso cref="Invoker"/> with the given
		/// value, and the <seealso cref="Request"/> with <seealso cref="BaseRequest"/>.
		/// </summary>
		/// <param name="invoker">The invoker used for this test.</param>
		public WebTest (BaseInvoker invoker)
			: this ()
		{
			Invoker = invoker;
		}

		/// <summary>
		/// Create a new instance, initializing <seealso cref="Request"/> with the given
		/// value, and the <seealso cref="Invoker"/> with <seealso cref="BaseInvoker"/>.
		/// </summary>
		/// <param name="request">The request used for this test.</param>
		public WebTest (BaseRequest request)
			: this ()
		{
			Request = request;
		}


#if !TARGET_JVM
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
#endif

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
#if !TARGET_JVM
		const string HOST_INSTANCE_NAME = "MonoTests/SysWeb/Framework/Host";
#endif

		private static MyHost CreateHosting ()
		{
			string tmpFile = Path.GetTempFileName ();
			File.Delete (tmpFile);
			Directory.CreateDirectory (tmpFile);
			baseDir = tmpFile;
			binDir = Directory.CreateDirectory (Path.Combine (baseDir, "bin")).FullName;

			CopyResources ();
#if !TARGET_JVM
			foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies ())
				LoadAssemblyRecursive (ass);

			foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies ())
				CopyAssembly (ass, binDir);

			MyHost host = (MyHost) ApplicationHost.CreateApplicationHost (typeof (MyHost), VIRTUAL_BASE_DIR, baseDir);
			AppDomain.CurrentDomain.SetData (HOST_INSTANCE_NAME, host);
			host.AppDomain.SetData (HOST_INSTANCE_NAME, host);
#else
			host = new MyHost ();
			AppDomain.CurrentDomain.SetData (".appVPath", VIRTUAL_BASE_DIR);
			AppDomain.CurrentDomain.SetData (".appPath", baseDir);
#endif
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
#if TARGET_JVM
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.AspxParser.params",
				"AspxParser.params");
#endif
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
