using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.application;
using java.util;
using javax.faces.context;
using javax.faces.component;
using javax.faces.render;
using System.Web.UI;

namespace Mainsoft.Web.Hosting
{
	public abstract class BaseFacesViewHandler : ViewHandler
	{
		readonly ViewHandler _viewHandler;

		public BaseFacesViewHandler (ViewHandler viewHandler) {
			_viewHandler = viewHandler;
		}

		public override string getActionURL (FacesContext facesContext, string viewId) {
			return _viewHandler.getActionURL (facesContext, viewId);
		}

		public override Locale calculateLocale (FacesContext facesContext) {
			return _viewHandler.calculateLocale (facesContext);
		}

		public override String calculateRenderKitId (FacesContext facesContext) {
			return _viewHandler.calculateRenderKitId (facesContext);
		}

		public override UIViewRoot createView (FacesContext facesContext, String viewId) {
			UIViewRoot uiViewRoot = _viewHandler.createView (facesContext, viewId);
			
			// create instance of Page by viewId
			StringBuilder sb = new StringBuilder ();
			sb.Append (facesContext.getExternalContext ().getRequestContextPath ());
			int index = viewId.IndexOf ('?');
			if (index >= 0)
				sb.Append (viewId, 0, index);
			else
				sb.Append (viewId);
			UIComponent page = (UIComponent) PageParser.GetCompiledPageInstance (sb.ToString (), null, ((AspNetFacesContext) facesContext).Context);
			uiViewRoot.getChildren ().add (0, page);
			
			return uiViewRoot;
		}

		public override String getResourceURL (FacesContext facesContext, String path) {
			return _viewHandler.getResourceURL (facesContext, path);
		}

		public override void renderView (FacesContext facesContext, UIViewRoot viewToRender) {
			if (viewToRender == null)
				throw new ArgumentNullException ("viewToRender", "viewToRender must not be null");
			((UIComponent) viewToRender.getChildren ().get (0)).encodeChildren (facesContext);
		}


		public override UIViewRoot restoreView (FacesContext facesContext, String viewId) {
			return _viewHandler.restoreView (facesContext, viewId);
		}

		public override void writeState (FacesContext facesContext) {
			StateManager manager = facesContext.getApplication ().getStateManager ();
			StateManager.SerializedView serializedView = manager.saveSerializedView (facesContext);
			manager.writeState (facesContext, serializedView);
		}

	}
}
