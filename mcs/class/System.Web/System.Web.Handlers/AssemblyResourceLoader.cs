//
// System.Web.Handlers.AssemblyResourceLoader
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

using System.Web;
using System.Web.UI;
using System.Reflection;
using System.IO;

namespace System.Web.Handlers {
	[MonoTODO ("Should we cache stuff?")]
	#if NET_1_2
	public
	#else
	internal // since this is in the .config file, we need to support it, since we dont have versoned support.
	#endif
		class AssemblyResourceLoader : IHttpHandler {
		
		internal static string GetResourceUrl (Type type, string resourceName)
		{
			return "WebResource.axd?assembly=" 
				+ HttpUtility.UrlEncode (type.Assembly.GetName ().FullName) 
				+ "&resource=" 
				+ HttpUtility.UrlEncode (resourceName);
		}

			
		[MonoTODO ("Substitution not implemented")]
		private void System.Web.IHttpHandler.ProcessRequest (HttpContext context)
		{
			string resourceName = context.Request.QueryString ["resource"];
			Assembly assembly = Assembly.Load (context.Request.QueryString ["assembly"]);
			
			bool found = false;
			foreach (WebResourceAttribute wra in assembly.GetCustomAttributes (typeof (WebResourceAttribute), false)) {
				if (wra.WebResource == resourceName) {
					context.Response.ContentType = wra.ContentType;
					
					if (wra.PerformSubstitution)
						throw new NotImplementedException ("Substitution not implemented");
					
					found = true;
					break;
				}
			}
			if (!found)
				return;
			
			Stream s = assembly.GetManifestResourceStream (resourceName);
			
			byte [] buf = new byte [1024];
			Stream output = context.Response.OutputStream;
			int c;
			do {
				c = s.Read (buf, 0, 1024);
				output.Write (buf, 0, c);
			} while (c > 0);
		}
		
		private bool System.Web.IHttpHandler.IsReusable { get { return true; } }
	}
}

