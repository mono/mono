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
	public class BaseHttpServlet : HttpServlet
	{
		//private AppDomain _servletDomain;
		static readonly LocalDataStoreSlot _servletRequestSlot = Thread.GetNamedDataSlot(J2EEConsts.SERVLET_REQUEST);
		static readonly LocalDataStoreSlot _servletResponseSlot = Thread.GetNamedDataSlot(J2EEConsts.SERVLET_RESPONSE);
		static readonly LocalDataStoreSlot _servletSlot = Thread.GetNamedDataSlot(J2EEConsts.CURRENT_SERVLET);

		bool _appVirDirInited = false;

		public BaseHttpServlet()
		{
		}

		override public void init(ServletConfig config)
		{
			base.init(config);
			InitServlet(config);
			
		}

		protected virtual void InitServlet(ServletConfig config)
		{
			if (config.getServletContext().getAttribute(J2EEConsts.APP_DOMAIN) != null)
				return;

			try 
			{
				AppDomain servletDomain = createServletDomain(config);
				vmw.@internal.EnvironmentUtils.setAppDomain(servletDomain);

				//GH Infromation Initizalization
				int nowInt = DateTime.Now.ToString().GetHashCode();
				servletDomain.SetData(".domainId", nowInt.ToString("x"));
				nowInt += "/".GetHashCode ();
				servletDomain.SetData(".appId", nowInt.ToString("x"));
				servletDomain.SetData(".appName", nowInt.ToString("x"));

				servletDomain.SetData(J2EEConsts.CLASS_LOADER, vmw.common.TypeUtils.ToClass(this).getClassLoader());
				servletDomain.SetData(J2EEConsts.SERVLET_CONFIG, config);
				servletDomain.SetData(J2EEConsts.RESOURCE_LOADER, new vmw.@internal.j2ee.ServletResourceLoader(config.getServletContext()));

				config.getServletContext().setAttribute(J2EEConsts.APP_DOMAIN, servletDomain);
				config.getServletContext ().setAttribute (J2EEConsts.CURRENT_SERVLET, this);
			}
			finally 
			{
				vmw.@internal.EnvironmentUtils.cleanTLS();
				vmw.@internal.EnvironmentUtils.clearAppDomain();
			}
		}

		protected override void service (HttpServletRequest req, HttpServletResponse resp)
		{
			const string assemblies = "/assemblies";
			const string getping = "getping";
			const string setping = "setping";
			string servletPath = req.getServletPath ();

			if (String.CompareOrdinal (assemblies, 0, servletPath, 0, assemblies.Length) == 0) {
				if (servletPath.Length == assemblies.Length ||
					servletPath [assemblies.Length] == '/') {
					string requestURI = req.getRequestURI ();
					bool getp = requestURI.EndsWith (getping, StringComparison.Ordinal);
					if (!getp && requestURI.EndsWith (setping, StringComparison.Ordinal)) {
						getServletContext ().setAttribute (getping, "1");
						getp = true;
					}

					if (getp) {
						string ping = (string) getServletContext ().getAttribute (getping);
						if (ping == null)
							ping = "0";
						resp.getOutputStream ().print (ping);
						return;
					}
				}
			}
			resp.setContentType("text/html");
			service(req, resp, resp.getOutputStream());
		}

		public virtual void service(HttpServletRequest req, HttpServletResponse resp, java.io.OutputStream output)
		{
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
				Thread.SetData(_servletRequestSlot, req);
				// put response to the TLS
				Thread.SetData(_servletResponseSlot, resp);
				// put the servlet object to the TLS
				Thread.SetData(_servletSlot, this);

				resp.setHeader("X-Powered-By", "ASP.NET");
				resp.setHeader("X-AspNet-Version", "1.1.4322");

				HttpWorkerRequest gwr = new ServletWorkerRequest(this, req, resp, output);
				CultureInfo culture = (CultureInfo) vmw.@internal.EnvironmentUtils.getCultureInfoFromLocale (req.getLocale ());
				Thread currentTread = Thread.CurrentThread;
				currentTread.CurrentCulture = culture;
				currentTread.CurrentUICulture = culture;
				HttpRuntime.ProcessRequest(gwr);
			}
			finally 
			{
				HttpContext.Current = null;
				Thread.SetData(_servletRequestSlot, null);
				Thread.SetData(_servletResponseSlot, null);
				Thread.SetData(_servletSlot, null);
				vmw.@internal.EnvironmentUtils.clearAppDomain();
			}
		}

		override public void destroy()
		{
			base.destroy();
			AppDomain servletDomain = (AppDomain) getServletContext ().getAttribute (J2EEConsts.APP_DOMAIN);
			if (servletDomain == null)
				return;

			try 
			{
				vmw.@internal.EnvironmentUtils.setAppDomain(servletDomain);
#if DEBUG
				Console.WriteLine("Destroy of GhHttpServlet");
#endif
				HttpRuntime.Close();
				vmw.@internal.EnvironmentUtils.cleanAllBeforeServletDestroy(this);
				this.getServletContext().removeAttribute(J2EEConsts.APP_DOMAIN);
				java.lang.Thread.currentThread().setContextClassLoader(null);
			}
			catch(Exception e) 
			{
#if DEBUG
				Console.WriteLine("ERROR in Servlet Destroy {0},{1}",e.GetType(), e.Message);
				Console.WriteLine(e.StackTrace);
#endif
			}
			finally
			{
				vmw.@internal.EnvironmentUtils.clearAppDomain();
			}
		}

		private AppDomain createServletDomain(ServletConfig config)
		{
				string rootPath = J2EEUtils.GetApplicationRealPath(config);
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

				servletDomain.SetData(IAppDomainConfig.APP_PHYS_DIR, J2EEUtils.GetApplicationPhysicalPath(config));
				servletDomain.SetData(IAppDomainConfig.WEB_APP_DIR, rootPath);

				//Set DataDirectory substitution string (http://blogs.msdn.com/dataaccess/archive/2005/10/28/486273.aspx)
				string dataDirectory = J2EEUtils.GetInitParameterByHierarchy(config, "DataDirectory");
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
				// Mordechai : setting the web app deserializer object.
				servletDomain.SetData(J2EEConsts.DESERIALIZER_CONST , this.GetDeserializer());
				servletDomain.SetData(vmw.@internal.EnvironmentUtils.GH_DRIVER_UTILS_CONST, this.getDriverUtils());
				//servletDomain.SetData(".hostingVirtualPath", "/");
				//servletDomain.SetData(".hostingInstallDir", "/");
				return servletDomain;
		}
	
		virtual protected vmw.@internal.io.IObjectsDeserializer GetDeserializer()
		{
			if (m_deseializer == null)
				m_deseializer = new GHWebDeseserializer();
			return m_deseializer;
		}

		protected vmw.@internal.io.IObjectsDeserializer m_deseializer = null;
		/// Mordechai: This class comes to solve a problem in class deserialize
		/// within web application. The problem is that the classloader that created 
		/// some user web class (for example aspx page) is not the class loader
		/// that de-serialize it - thus we end with ClassDefNotFoundException.
		/// To prevent this situation we delegate the serialization back the the 
		/// web app (which has the correct class loader...)
		/// 

		virtual protected vmw.@internal.IDriverUtils getDriverUtils()
		{
			//by default no driver utils, the specific servlet will override this method
			return null;
		}
	}

	public class GHWebDeseserializer : vmw.@internal.io.IObjectsDeserializer 
	{

			Object vmw.@internal.io.IObjectsDeserializer.Deserialize(java.io.ObjectInputStream stream)
			{
				object obj = stream.readObject();
				return obj;
			}
	}
}

namespace System.Web.GH
{
	public class BaseHttpServlet : Mainsoft.Web.Hosting.BaseHttpServlet
	{
	}

}

namespace System.Web.J2EE
{
	public class BaseHttpServlet : Mainsoft.Web.Hosting.BaseHttpServlet
	{
	}

}
