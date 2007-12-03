//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Threading;
using System.Web.Hosting;
using System.IO;

using javax.servlet;
using javax.servlet.http;
using vmw.common;
using java.util;
using vmw.@internal;
using java.lang.reflect;
using java.net;
using System.Globalization;

namespace Mainsoft.Web.Hosting
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class BaseHttpServlet : HttpServlet
	{
		//private AppDomain _servletDomain;
		//static readonly LocalDataStoreSlot _servletRequestSlot = Thread.GetNamedDataSlot(J2EEConsts.SERVLET_REQUEST);
		//static readonly LocalDataStoreSlot _servletResponseSlot = Thread.GetNamedDataSlot(J2EEConsts.SERVLET_RESPONSE);
		//static readonly LocalDataStoreSlot _servletSlot = Thread.GetNamedDataSlot(J2EEConsts.CURRENT_SERVLET);

		bool _appVirDirInited = false;

		public BaseHttpServlet()
		{
		}

		override public void init(ServletConfig config)
		{
			base.init(config);
			InitRuntime (config, this);
		}

		public static void InitRuntime (ServletConfig config, object evidence) {
			AppDomain servletDomain = createServletDomain (config);
			vmw.@internal.EnvironmentUtils.setAppDomain (servletDomain);

			try {
				ServletContext context = config.getServletContext ();
				//GH Infromation Initizalization
				long currentTime = java.lang.System.currentTimeMillis ();
				servletDomain.SetData (".domainId", currentTime.ToString ("x"));
				currentTime = ~currentTime;
				servletDomain.SetData (".appId", currentTime.ToString ("x"));
				servletDomain.SetData (".appName", servletDomain.SetupInformation.ApplicationName);

				servletDomain.SetData (J2EEConsts.CLASS_LOADER, java.lang.Thread.currentThread ().getContextClassLoader ());
				//servletDomain.SetData (J2EEConsts.CLASS_LOADER, vmw.common.TypeUtils.ToClass (evidence).getClassLoader ());
				//servletDomain.SetData(J2EEConsts.SERVLET_CONFIG, config);
				servletDomain.SetData (J2EEConsts.RESOURCE_LOADER, new vmw.@internal.j2ee.ServletResourceLoader (context));

				lock (evidence) {
					if (context.getAttribute (J2EEConsts.APP_DOMAIN) == null)
						context.setAttribute (J2EEConsts.APP_DOMAIN, servletDomain);
				}
				//config.getServletContext ().setAttribute (J2EEConsts.CURRENT_SERVLET, this);
			}
			finally {
				vmw.@internal.EnvironmentUtils.cleanTLS ();
				vmw.@internal.EnvironmentUtils.clearAppDomain ();
			}
		}

		protected override void service (HttpServletRequest req, HttpServletResponse resp)
		{
			resp.setContentType ("text/html");

			try 
			{
				// Very important - to update Virtual Path!!!
				AppDomain servletDomain = (AppDomain)this.getServletContext().getAttribute(J2EEConsts.APP_DOMAIN);
				if (!_appVirDirInited) {
					servletDomain.SetData (IAppDomainConfig.APP_VIRT_DIR, req.getContextPath ());
					servletDomain.SetData (".hostingVirtualPath", req.getContextPath ());
					_appVirDirInited = true;
				}

				// Put to the TLS current AppDomain of the servlet, so anyone can use it.
				vmw.@internal.EnvironmentUtils.setAppDomain(servletDomain);

				// put request to the TLS
				//Thread.SetData(_servletRequestSlot, req);
				//// put response to the TLS
				//Thread.SetData(_servletResponseSlot, resp);
				//// put the servlet object to the TLS
				//Thread.SetData(_servletSlot, this);

				resp.setHeader("X-Powered-By", "ASP.NET");
				resp.setHeader("X-AspNet-Version", "1.1.4322");

				HttpWorkerRequest gwr = new ServletWorkerRequest (this, req, resp);
				CultureInfo culture = (CultureInfo) vmw.@internal.EnvironmentUtils.getCultureInfoFromLocale (req.getLocale ());
				Thread currentTread = Thread.CurrentThread;
				currentTread.CurrentCulture = culture;
				currentTread.CurrentUICulture = culture;
				HttpRuntime.ProcessRequest(gwr);
			}
			finally 
			{
				HttpContext.Current = null;
				//Thread.SetData(_servletRequestSlot, null);
				//Thread.SetData(_servletResponseSlot, null);
				//Thread.SetData(_servletSlot, null);
				vmw.@internal.EnvironmentUtils.clearAppDomain();
			}
		}

		override public void destroy()
		{
			base.destroy();
			DestroyRuntime (getServletContext (), this);
		}

		public static void DestroyRuntime (ServletContext context, object evidence) {
			AppDomain servletDomain = (AppDomain) context.getAttribute (J2EEConsts.APP_DOMAIN);
			if (servletDomain == null)
				return;

			try {
				vmw.@internal.EnvironmentUtils.setAppDomain (servletDomain);
#if DEBUG
				Console.WriteLine ("Destroy of GhHttpServlet");
#endif
				HttpRuntime.Close ();
				vmw.@internal.EnvironmentUtils.cleanAllBeforeServletDestroy (evidence);
				context.removeAttribute (J2EEConsts.APP_DOMAIN);
				java.lang.Thread.currentThread ().setContextClassLoader (null);
			}
			catch (Exception e) {
#if DEBUG
				Console.WriteLine ("ERROR in Servlet Destroy {0},{1}", e.GetType (), e.Message);
				Console.WriteLine (e.StackTrace);
#endif
			}
			finally {
				vmw.@internal.EnvironmentUtils.clearAppDomain ();
			}
		}

		private static AppDomain createServletDomain(ServletConfig config)
		{
				string rootPath = J2EEUtils.GetApplicationRealPath(config.getServletContext ());
				AppDomainSetup domainSetup = new AppDomainSetup();
				string name = config.getServletName();//.getServletContextName();
				if (name == null)
					name = "GH Application";
				domainSetup.ApplicationName = name;
				domainSetup.ConfigurationFile = Path.Combine (rootPath, "Web.config");
				domainSetup.PrivateBinPath = Path.Combine (rootPath, "WEB-INF/lib");

				AppDomain servletDomain = AppDomain.CreateDomain(name, null, domainSetup);





				//servletDomain.SetData(IAppDomainConfig.APP_PHYS_DIR, J2EEUtils.GetApplicationPhysicalPath(config));
				//servletDomain.SetData(IAppDomainConfig.WEB_APP_DIR, rootPath);

				servletDomain.SetData(IAppDomainConfig.APP_PHYS_DIR, J2EEUtils.GetApplicationPhysicalPath(config.getServletContext ()));
				servletDomain.SetData(IAppDomainConfig.WEB_APP_DIR, rootPath);

				//Set DataDirectory substitution string (http://blogs.msdn.com/dataaccess/archive/2005/10/28/486273.aspx)
				string dataDirectory = config.getServletContext ().getInitParameter ("DataDirectory");
				if (dataDirectory == null)
					dataDirectory = "App_Data";

				if (!Path.IsPathRooted (dataDirectory)) {
					java.io.InputStream inputStream = config.getServletContext ().getResourceAsStream ("/WEB-INF/classes/appData.properties");
					string root;
					if (inputStream != null) {
						try {
							Properties props = new Properties ();
							props.load (inputStream);
							root = props.getProperty ("root.folder");
						}
						finally {
							inputStream.close ();
						}
					}
					else
						root = config.getServletContext ().getRealPath ("/");

					if (root == null)
						root = String.Empty;

					dataDirectory = Path.Combine (root, dataDirectory);
				}

				if (dataDirectory [dataDirectory.Length - 1] != Path.DirectorySeparatorChar)
					dataDirectory += Path.DirectorySeparatorChar;

				servletDomain.SetData ("DataDirectory", dataDirectory);

				if (config.getServletContext ().getRealPath ("/") == null)
					servletDomain.SetData(".appStartTime", DateTime.UtcNow);

				// The BaseDir is the full path to the physical dir of the app
				// and allows the application to modify files in the case of
				// open deployment.
				string webApp_baseDir = config.getServletContext().getRealPath("");
				if (webApp_baseDir == null || webApp_baseDir == "")
					webApp_baseDir = rootPath;
				servletDomain.SetData(IAppDomainConfig.APP_BASE_DIR , webApp_baseDir);
#if DEBUG
				Console.WriteLine("Initialization of webapp " + webApp_baseDir);
#endif
				//servletDomain.SetData(".hostingVirtualPath", "/");
				//servletDomain.SetData(".hostingInstallDir", "/");
				return servletDomain;
		}
	}
}

namespace System.Web.GH
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class BaseHttpServlet : Mainsoft.Web.Hosting.BaseHttpServlet
	{
	}

}

namespace System.Web.J2EE
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class BaseHttpServlet : Mainsoft.Web.Hosting.BaseHttpServlet
	{
	}

}

public class GhDynamicHttpServlet : System.Web.GH.BaseHttpServlet
{
}

public class GhStaticHttpServlet : System.Web.GH.BaseStaticHttpServlet
{ 
}

public class GhHttpServlet : System.Web.GH.BaseHttpServlet
{
	GhStaticHttpServlet staticServlet;

	public GhHttpServlet () {
		staticServlet = new GhStaticHttpServlet ();
	}

	override public void init (ServletConfig config) {
		base.init (config);
		staticServlet.init (config);
	}

	override protected void service (HttpServletRequest req, HttpServletResponse resp) {
		string pathInfo = req.getRequestURI ();
		string contextPath = req.getContextPath ();
		if (pathInfo.Equals (contextPath) ||
			((pathInfo.Length - contextPath.Length) == 1) &&
			pathInfo [pathInfo.Length - 1] == '/' && pathInfo.StartsWith (contextPath))
			pathInfo = contextPath + req.getServletPath ();
		if (pathInfo.EndsWith (".aspx") ||
			pathInfo.EndsWith (".asmx") ||
			pathInfo.EndsWith (".invoke")) {
			base.service (req, resp);
		}
		else {
			staticServlet.service (req, resp);
		}
	}

	override public void destroy () {
		staticServlet.destroy ();
		base.destroy ();
	}
}
