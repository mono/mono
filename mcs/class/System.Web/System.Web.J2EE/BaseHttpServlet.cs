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
using System.Web.Configuration;
using System.Threading;
using System.Web.Hosting;

using javax.servlet;
using javax.servlet.http;
using vmw.common;

namespace System.Web.J2EE
{
	public class BaseHttpServlet : HttpServlet
	{
		//private AppDomain _servletDomain;
		static LocalDataStoreSlot _servletRequestSlot = Thread.GetNamedDataSlot(J2EEConsts.SERVLET_REQUEST);
		static LocalDataStoreSlot _servletResponseSlot = Thread.GetNamedDataSlot(J2EEConsts.SERVLET_RESPONSE);
		static LocalDataStoreSlot _servletSlot = Thread.GetNamedDataSlot(J2EEConsts.CURRENT_SERVLET);


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
			try 
			{
				AppDomain servletDomain = createServletDomain(config);

				//GH Infromation Initizalization
				servletDomain.SetData(J2EEConsts.CLASS_LOADER, vmw.common.TypeUtils.ToClass(this).getClassLoader());
				servletDomain.SetData(J2EEConsts.SERVLET_CONFIG, config);
				servletDomain.SetData(J2EEConsts.RESOURCE_LOADER, new vmw.@internal.j2ee.ServletResourceLoader(config.getServletContext()));

				config.getServletContext().setAttribute(J2EEConsts.APP_DOMAIN, servletDomain);
			}
			finally 
			{
				vmw.@internal.EnvironmentUtils.cleanTLS();
			}
		}

		override protected void service(HttpServletRequest req, HttpServletResponse resp)
		{
			try 
			{
				// Very important - to update Virtual Path!!!
				AppDomain servletDomain = (AppDomain)this.getServletContext().getAttribute(J2EEConsts.APP_DOMAIN);
				servletDomain.SetData(IAppDomainConfig.APP_VIRT_DIR, req.getContextPath());
				servletDomain.SetData(".hostingVirtualPath", req.getContextPath());
				//put request to the TLS
				Thread.SetData(_servletRequestSlot, req);
				//put response to the TLS
				Thread.SetData(_servletResponseSlot, resp);
				//put the servlet object to the TLS
				Thread.SetData(_servletSlot, this);
				// Put to the TLS current AppDomain of the servlet, so anyone can use it.
				vmw.@internal.EnvironmentUtils.setAppDomain(servletDomain);
				resp.setHeader("X-Powered-By", "ASP.NET");
				resp.setHeader("X-AspNet-Version", "1.1.4322");

//				PageMapper.LoadFileList();

				resp.setContentType("text/html");
				HttpWorkerRequest gwr = new ServletWorkerRequest(this, req, resp, resp.getOutputStream());
				HttpRuntime.ProcessRequest(gwr);
			}
			finally 
			{
				HttpContext.Current = null;
				Thread.SetData(_servletRequestSlot, null);
				Thread.SetData(_servletResponseSlot, null);
				Thread.SetData(_servletSlot, null);
				vmw.@internal.EnvironmentUtils.clearAppDomain();
				//cleaning
				//vmw.Utils.cleanTLS(); //clean up all TLS entries for current Thread.
				//java.lang.Thread.currentThread().setContextClassLoader(null);
			}
		}

		override public void destroy()
		{
			try 
			{
				AppDomain servletDomain = (AppDomain)this.getServletContext().getAttribute(J2EEConsts.APP_DOMAIN);
				vmw.@internal.EnvironmentUtils.setAppDomain(servletDomain);
				Console.WriteLine("Destroy of GhHttpServlet");
				base.destroy();
				HttpRuntime.Close();
				vmw.@internal.EnvironmentUtils.cleanAllBeforeServletDestroy(this);
				this.getServletContext().removeAttribute(J2EEConsts.APP_DOMAIN);
				java.lang.Thread.currentThread().setContextClassLoader(null);
			}
			catch(Exception e) 
			{
				Console.WriteLine("ERROR in Servlet Destroy {0},{1}",e.GetType(), e.Message);
				Console.WriteLine(e.StackTrace);
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
				domainSetup.ConfigurationFile = rootPath + "/Web.config";

				AppDomain servletDomain = AppDomain.CreateDomain(name, null, domainSetup);
				int nowInt = DateTime.Now.ToString().GetHashCode();
				servletDomain.SetData(".domainId", nowInt.ToString("x"));
				nowInt += "/".GetHashCode ();
				servletDomain.SetData(".appId", nowInt.ToString("x"));
				servletDomain.SetData(".appName", nowInt.ToString("x"));
				//servletDomain.SetData(".appPath", "/"); - Only for Exe
				//servletDomain.SetData(".appVPath", "/"); - Only for Exe
		
				servletDomain.SetData(IAppDomainConfig.APP_PHYS_DIR, J2EEUtils.GetApplicationPhysicalPath(config));
				servletDomain.SetData(IAppDomainConfig.WEB_APP_DIR, rootPath);

				// The BaseDir is the full path to the physical dir of the app
				// and allows the application to modify files in the case of
				// open deployment.
				string webApp_baseDir = config.getServletContext().getRealPath("");
				if (webApp_baseDir == null || webApp_baseDir == "")
					webApp_baseDir = rootPath;
				servletDomain.SetData(IAppDomainConfig.APP_BASE_DIR , webApp_baseDir);
				Console.WriteLine("Initialization of webapp " + webApp_baseDir);

				// Mordechai : setting the web app deserializer object.
				servletDomain.SetData(J2EEConsts.DESERIALIZER_CONST , this.GetDeserializer());

				//servletDomain.SetData(".hostingVirtualPath", "/");
				servletDomain.SetData(".hostingInstallDir", "/");
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
	public class BaseHttpServlet : System.Web.J2EE.BaseHttpServlet
	{
	}

}
