using System;
using System.Collections;
using System.Web; 

namespace TestMonoWeb
{
	public class SyncModule : IHttpModule {
		public String ModuleName { 
			get { return "HelloWorldModule"; } 
		}    
		//In the Init function, register for HttpApplication 
		//events by adding your handlers.
		public void Init(HttpApplication application) { 
			application.BeginRequest += (new EventHandler(this.Application_BeginRequest));
			application.EndRequest += (new EventHandler(this.Application_EndRequest));
		}
    
		//Your BeginRequest event handler.
		private void Application_BeginRequest(Object source, EventArgs e) {
			HttpApplication application = (HttpApplication)source;
			HttpContext context = application.Context;
        
			context.Response.Write("SyncModule.Application_BeginRequest()<br>\n");
		}
    
		//Your EndRequest event handler.
		private void Application_EndRequest(Object source, EventArgs e) {
			HttpApplication application = (HttpApplication)source;
			HttpContext context = application.Context;
        
			context.Response.Write("SyncModule.Application_EndRequest()<br>\n");
		}  
      
		public void Dispose() {
		}
	}
}
