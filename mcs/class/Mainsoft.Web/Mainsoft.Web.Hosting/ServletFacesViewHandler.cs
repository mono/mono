using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.application;
using javax.servlet.http;
using java.util;
using javax.faces.context;
using javax.faces.component;
using java.lang;
using javax.faces.render;

namespace Mainsoft.Web.Hosting
{
	public class ServletFacesViewHandler : BaseFacesViewHandler
	{
		public ServletFacesViewHandler (ViewHandler viewHandler)
			: base (viewHandler) {
		}

		public override string getActionURL (FacesContext facesContext, string viewId) {
			if (viewId.Length > 0 && viewId [0] == '/') {
				return facesContext.getExternalContext ().getRequestContextPath () + viewId;
			}
			else {
				return viewId;
			}
		}
	}
}
