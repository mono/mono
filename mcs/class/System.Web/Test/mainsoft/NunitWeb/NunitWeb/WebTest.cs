#if TARGET_JVM_FOR_WEBTEST
#define TARGET_JVM
#endif

using System;
using System.Reflection;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Threading;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// The most important class from user perspective. See <see cref="Request"/>,
	/// <see cref="Response"/>, <see cref="Invoker"/>, <see cref="Run"/> for
	/// more information.
	/// </summary>
	/// <seealso cref="Request"/>
	/// <seealso cref="Response"/>
	/// <seealso cref="Invoker"/>
	/// <seealso cref="Run"/>
	[Serializable]
	public partial class WebTest
	{
		/// <summary>
		/// Thrown when trying to copy a resource after appdomain was created. Please call
		/// WebTest.Unload before copying resource.
		/// </summary>
		public class DomainUpException : Exception
		{
		}

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
		/// The result of the last <see cref="Run"/>. See <see cref="MonoTests.SystemWeb.Framework.Response"/>,
		/// <see cref="FormRequest"/>.
		/// </summary>
		/// <seealso cref="Run"/>
		/// <seealso cref="MonoTests.SystemWeb.Framework.Response"/>
		/// <seealso cref="FormRequest"/>
		public Response Response
		{
			get { return _response; }
			set { _response = value; }
		}

		BaseInvoker _invoker;
		/// <summary>
		/// Set the invoker, which is executed in the web context by <see cref="Invoke"/>
		/// method. Most commonly used <see cref="PageInvoker"/>. See also: <see cref="BaseInvoker"/>,
		/// <see cref="HandlerInvoker"/>
		/// </summary>
		/// <seealso cref="Invoke"/>
		/// <seealso cref="PageInvoker"/>
		/// <seealso cref="BaseInvoker"/>
		/// <seealso cref="HandlerInvoker"/>
		public BaseInvoker Invoker
		{
			get { return _invoker; }
			set { _invoker = value; }
		}

		BaseRequest _request;
		/// <summary>
		/// Contains all the data necessary to create an <see cref="System.Web.HttpWorkerRequest"/> in
		/// the application appdomain. See also <see cref="BaseRequest"/>,
		/// <see cref="PostableRequest"/>, <see cref="FormRequest"/>.
		/// </summary>
		/// <seealso cref="System.Web.HttpWorkerRequest"/>
		/// <seealso cref="BaseRequest"/>
		/// <seealso cref="PostableRequest"/>
		/// <seealso cref="FormRequest"/>
		public BaseRequest Request
		{
			get { return _request; }
			set { _request = value; }
		}

		static MyHost host;
		internal static MyHost Host
		{
			get {
				EnsureHosting ();
				return host;
			}
		}

		/// <summary>
		/// Run the request using <see cref="Request"/> and <see cref="Invoker"/>
		/// values. Keep the result of the request in <see cref="Response"/> property.
		/// </summary>
		/// <returns>The body of the HTTP response (<see cref="MonoTests.SystemWeb.Framework.Response.Body"/>).</returns>
		/// <seealso cref="Request"/>
		/// <seealso cref="Invoker"/>
		/// <seealso cref="Response"/>
		/// <seealso cref="MonoTests.SystemWeb.Framework.Response.Body"/>
		public string Run ()
		{
#if !DOTNET
			SystemWebTestShim.BuildManager.SuppressDebugModeMessages ();
#endif
			if (Request.Url == null)
				Request.Url = Invoker.GetDefaultUrl ();
			_unloadHandler.StartingRequest();
			try {
				WebTest newTestInstance = Host.Run (this);
				CopyFrom (newTestInstance);
			} finally {
				_unloadHandler.FinishedRequest();
			}
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
		/// This method must be called when custom <see cref="System.Web.IHttpHandler.ProcessRequest"/> or aspx code behind is used,
		/// to allow the framework to invoke all user supplied delegates.
		/// </summary>
		/// <param name="param">Parameter defined by the <see cref="BaseInvoker"/> subclass. For example,
		/// <see cref="PageInvoker"/> expects to receive a <see cref="System.Web.UI.Page"/> instance here.</param>
		/// <seealso cref="System.Web.IHttpHandler.ProcessRequest"/>
		/// <seealso cref="BaseInvoker"/>
		/// <seealso cref="PageInvoker"/>
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

		public void SendHeaders ()
		{
			Host.SendHeaders (this);
		}

		/// <summary>
		/// This method is intended for use from <see cref="MonoTests.SystemWeb.Framework.BaseInvoker.DoInvoke"/> when
		/// the invocation causes an exception. In such cases, the exception must be registered
		/// with this method, and then swallowed. Before returning, <see cref="WebTest.Run"/>
		/// will rethrow this exception. This is done to hide the exception from <see cref="System.Web.HttpRuntime"/>,
		/// which normally swallows the exception and returns 500 ERROR http result.
		/// </summary>
		/// <param name="ex">The exception to be registered and rethrown.</param>
		/// <seealso cref="MonoTests.SystemWeb.Framework.BaseInvoker.DoInvoke"/>
		/// <seealso cref="WebTest.Run"/>
		/// <seealso cref="System.Web.HttpRuntime"/>
		public static void RegisterException (Exception ex)
		{
			Host.RegisterException (ex);
		}

		/// <summary>
		/// Unload the web appdomain and delete the temporary application root
		/// directory.
		/// </summary>
		public static void CleanApp ()
		{
#if !TARGET_JVM
			if (host != null) {
				lock (_appUnloadedSync) {
					EventHandler handler = new EventHandler(PulseAppUnloadedSync);
					WebTest.AppUnloaded += handler;
					WebTest t = new WebTest (PageInvoker.CreateOnLoad (new PageDelegate (UnloadAppDomain_OnLoad)));
					t.Run ();
					Monitor.Wait(_appUnloadedSync);
					WebTest.AppUnloaded -= handler;
				}			
			}
			if (baseDir != null) {
				Directory.Delete (baseDir, true);
				baseDir = null;
				binDir = null;
			}
#endif
		}
		
		private static object _appUnloadedSync = new object();
		
		private static void PulseAppUnloadedSync(object source, EventArgs args)
		{
			lock (_appUnloadedSync)
				Monitor.PulseAll(_appUnloadedSync);
		}

		public static void UnloadAppDomain_OnLoad (Page p) 
		{
			HttpRuntime.UnloadAppDomain();
		}

		public static void Unload () {}

		/// <summary>
		/// Default constructor. Initializes <see cref="Invoker"/> with a new
		/// <see cref="BaseInvoker"/> and <see cref="Request"/> with an empty
		/// <see cref="BaseRequest"/>.
		/// </summary>
		/// <seealso cref="Invoker"/>
		/// <seealso cref="BaseInvoker"/>
		/// <seealso cref="Request"/>
		/// <seealso cref="BaseRequest"/>
		public WebTest ()
		{
			Invoker = new BaseInvoker ();
			Request = new BaseRequest ();
		}

		/// <summary>
		/// Same as <see cref="WebTest()"/>, and set <see cref="MonoTests.SystemWeb.Framework.BaseRequest.Url"/> to
		/// the specified Url.
		/// </summary>
		/// <param name="url">The URL used for the next <see cref="Run"/></param>
		/// <seealso cref="MonoTests.SystemWeb.Framework.BaseRequest.Url"/>
		/// <seealso cref="Run"/>
		public WebTest (string url)
			: this ()
		{
			Request.Url = url;
		}

		/// <summary>
		/// Create a new instance, initializing <see cref="Invoker"/> with the given
		/// value, and the <see cref="Request"/> with <see cref="BaseRequest"/>.
		/// </summary>
		/// <param name="invoker">The invoker used for this test.</param>
		/// <seealso cref="Invoker"/>
		/// <seealso cref="Request"/>
		/// <seealso cref="BaseRequest"/>
		public WebTest (BaseInvoker invoker)
			: this ()
		{
			Invoker = invoker;
		}

		/// <summary>
		/// Create a new instance, initializing <see cref="Request"/> with the given
		/// value, and the <see cref="Invoker"/> with <see cref="BaseInvoker"/>.
		/// </summary>
		/// <param name="request">The request used for this test.</param>
		/// <seealso cref="Request"/>
		/// <seealso cref="Invoker"/>
		/// <seealso cref="BaseInvoker"/>
		public WebTest (BaseRequest request)
			: this ()
		{
			Request = request;
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
			if (type == null)
				throw new ArgumentNullException ("type");
#if !TARGET_JVM
			using (Stream source = type.Assembly.GetManifestResourceStream (resourceName)) {
				if (source == null)
					throw new ArgumentException ("resource not found: " + resourceName, "resourceName");
				byte[] array = new byte[source.Length];
				source.Read (array, 0, array.Length);
				CopyBinary (array, targetUrl);
			}
#endif
		}

		public static void CopyPrefixedResources (Type type, string namePrefix, string targetDir)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			
			string[] manifestResources = type.Assembly.GetManifestResourceNames ();
			if (manifestResources == null || manifestResources.Length == 0)
				return;

			foreach (string resource in manifestResources) {
				if (resource == null || resource.Length == 0)
					continue;
				
				if (!resource.StartsWith (namePrefix))
					continue;
				 
				// The Replace part is for VisualStudio which compiles .resx files despite them being marked as
				// embedded resources, which breaks the tests.
				CopyResource (type, resource, Path.Combine (targetDir, resource.Substring (namePrefix.Length).Replace (".remove_extension", String.Empty)));
			}
		}
		
		/// <summary>
		/// Copy a chunk of data as a file into the web application.
		/// </summary>
		/// <param name="sourceArray">The array that contains the data to be written.</param>
		/// <param name="targetUrl">The URL where the data will be available.</param>
		/// <returns>The target filename where the data was stored.</returns>
		/// <example><code>CopyBinary (System.Text.Encoding.UTF8.GetBytes ("Hello"), "App_Data/Greeting.txt");</code></example>
		public static string CopyBinary (byte[] sourceArray, string targetUrl)
		{
#if TARGET_JVM
			return null;
#else
			EnsureWorkingDirectories ();
			EnsureDirectoryExists (Path.Combine (baseDir, Path.GetDirectoryName (targetUrl)));
			string targetFile = Path.Combine (baseDir, targetUrl);

			if (File.Exists(targetFile)) {
				using (FileStream existing = File.OpenRead(targetFile)) {
					bool equal = false;
					if (sourceArray.Length == existing.Length) {
						byte[] existingArray = new byte[sourceArray.Length];
						existing.Read (existingArray, 0, existingArray.Length);
						
						equal = true;
						for (int i = 0; i < sourceArray.Length; i ++) {
							if (sourceArray[i] != existingArray[i]) {
								equal = false;
								break;
							}
						}
					}
					
					if (equal) {
						existing.Close ();
						File.SetLastWriteTime (targetFile, DateTime.Now);
						return targetFile;
					}
					
				}
				
				CheckDomainIsDown ();
			}

			using (FileStream target = new FileStream (targetFile, FileMode.Create)) {
				target.Write (sourceArray, 0, sourceArray.Length);
			}

			return targetFile;
#endif
		}

		static WebTestResourcesSetupAttribute.SetupHandler CheckResourcesSetupHandler ()
		{
			// It is assumed WebTest is included in the same assembly which contains the
			// tests themselves
			object[] attributes = typeof (WebTest).Assembly.GetCustomAttributes (typeof (WebTestResourcesSetupAttribute), true);
			if (attributes == null || attributes.Length == 0)
				return null;
			
			WebTestResourcesSetupAttribute attr = attributes [0] as WebTestResourcesSetupAttribute;
			if (attr == null)
				return null;

			return attr.Handler;
		}
		
		public static void EnsureHosting ()
		{
			if (host != null)
				return;
#if TARGET_JVM
			host = new MyHost ();
			return;
#else
			host = AppDomain.CurrentDomain.GetData (HOST_INSTANCE_NAME) as MyHost;
			if (host == null)
				SetupHosting ();
#endif
		}
		
		public static void SetupHosting ()
		{
			SetupHosting (null);
		}
		
		public static void SetupHosting (WebTestResourcesSetupAttribute.SetupHandler resHandler)
		{
#if !TARGET_JVM
			if (host == null)
				host = AppDomain.CurrentDomain.GetData (HOST_INSTANCE_NAME) as MyHost;
#endif
			if (host != null)
				CleanApp ();
#if TARGET_JVM
			host = new MyHost ();
			return;
#else
			if (resHandler == null)
				resHandler = CheckResourcesSetupHandler ();
			if (resHandler == null)
				CopyResources ();
			else
				resHandler ();
			
			foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies ())
				LoadAssemblyRecursive (ass);

			foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies ())
				CopyAssembly (ass, binDir);

			host = (MyHost) ApplicationHost.CreateApplicationHost (typeof (MyHost), VIRTUAL_BASE_DIR, baseDir);
			AppDomain.CurrentDomain.SetData (HOST_INSTANCE_NAME, host);
			host.AppDomain.SetData (HOST_INSTANCE_NAME, host);
 			host.AppDomain.DomainUnload += new EventHandler (_unloadHandler.OnUnload);
#endif
		}

		private static UnloadHandler _unloadHandler = new UnloadHandler();
				
		public class UnloadHandler : MarshalByRefObject
		{
			AutoResetEvent _unloaded = new AutoResetEvent(false);
			
			int _numRequestsPending = 0;
			object _syncUnloading = new object();
			object _syncNumRequestsPending = new object();
			
			internal void StartingRequest()
			{
				// If the app domain is about to unload, wait
				lock (_syncUnloading)
					lock (_syncNumRequestsPending)
						_numRequestsPending++;
			}
			
			internal void FinishedRequest()
			{
				// Let any unloading continue once there are not requests pending
				lock (_syncNumRequestsPending) {
					_numRequestsPending--;
					if (_numRequestsPending == 0)
						Monitor.PulseAll(_syncNumRequestsPending);
				}
			}
			
			public void OnUnload (object o, EventArgs args)
			{
#if !TARGET_JVM
                // Block new requests from starting
				lock (_syncUnloading) {
					// Wait for pending requests to finish
					lock (_syncNumRequestsPending) {
						while (_numRequestsPending > 0)
							Monitor.Wait(_syncNumRequestsPending);
					}
					// Clear the host so that it will be created again on the next request
					AppDomain.CurrentDomain.SetData (HOST_INSTANCE_NAME, null);
					WebTest.host = null;
					
					EventHandler handler = WebTest.AppUnloaded;
					if (handler != null)
						handler(this, null);
				}
#endif
            }
		}

		public static event EventHandler AppUnloaded;

		public static string TestBaseDir {
			get {
#if !TARGET_JVM
				return baseDir;
#else
				return String.Empty;
#endif
			}
		}
		
#if !TARGET_JVM
		const string VIRTUAL_BASE_DIR = "/NunitWeb";
		private static string baseDir;
		private static string binDir;
		const string HOST_INSTANCE_NAME = "MonoTests/SysWeb/Framework/Host";
		
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
			if (ass.GlobalAssemblyCache || ass.FullName.StartsWith ("mscorlib"))
				return;
			string oldfn = ass.Location;
			if (oldfn.EndsWith (".exe"))
				return;
			string newfn = Path.Combine (dir, Path.GetFileName (oldfn));
			if (File.Exists (newfn))
				return;
			EnsureDirectoryExists (dir);
			File.Copy (oldfn, newfn);
			if (File.Exists (oldfn + ".mdb"))
				File.Copy (oldfn + ".mdb", newfn + ".mdb");
			if (File.Exists (oldfn + ".pdb"))
				File.Copy (oldfn + ".pdb", newfn + ".pdb");
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

		private static void CheckDomainIsDown ()
		{
			if (host != null)
				throw new DomainUpException ();
		}

		private static void EnsureWorkingDirectories ()
		{
			if (baseDir != null)
				return;
			CreateWorkingDirectories ();
		}

		private static void CreateWorkingDirectories ()
		{
			string tmpFile = Path.GetTempFileName ();
			File.Delete (tmpFile);
			baseDir = tmpFile;
			Directory.CreateDirectory (tmpFile);
			binDir = Path.Combine (baseDir, "bin");
			Directory.CreateDirectory (binDir);
		}

		public static void CopyResources ()
		{
			Type myself = typeof (WebTest);
			
			CopyResource (myself, "My.ashx", "My.ashx");
			CopyResource (myself, "Global.asax", "Global.asax");
			CopyResource (myself, "MyPage.aspx", "MyPage.aspx");
			CopyResource (myself, "MyPage.aspx.cs", "MyPage.aspx.cs");
			CopyResource (myself, "MyPageWithMaster.aspx", "MyPageWithMaster.aspx");
			CopyResource (myself, "My.master", "My.master");

			CopyResourcesLocal ();
		}

		static partial void CopyResourcesLocal ();
#endif
	}
}
