using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

using javax.faces;
using javax.faces.application;
using javax.faces.render;
using javax.faces.component;
using javax.faces.context;
using System.Web.Hosting;
using System.Web;
using java.util;
using System.Diagnostics;
using javax.servlet.http;

namespace Mainsoft.Web.Hosting
{
	public sealed class ServletFacesStateManager : BaseFacesStateManager
	{
		public override void writeState (FacesContext facesContext, StateManager.SerializedView serializedView) {
			Trace.WriteLine ("Entering writeState");

			if (serializedView != null) {
				if (isSavingStateInClient (facesContext)) {
					UIViewRoot uiViewRoot = facesContext.getViewRoot ();
					//save state in response (client-side: full state; server-side: sequence)
					RenderKit renderKit = RenderKitFactory.getRenderKit (facesContext, uiViewRoot.getRenderKitId ());
					// not us.
					renderKit.getResponseStateManager ().writeState (facesContext, serializedView);
				}
				else {
					HttpSession session = (HttpSession) facesContext.getExternalContext ().getSession (true);
					string key = ((IExtendedViewHandler) facesContext.getApplication ().getViewHandler ()).EncodeNamespace (facesContext, VIEWSTATE);
					session.setAttribute (key, serializedView);
				}
			}

			Trace.WriteLine ("Exiting writeState");
		}

		protected override void restoreComponentState (FacesContext facesContext,
												  javax.faces.component.UIViewRoot uiViewRoot,
												  String renderKitId) {

			Trace.WriteLine ("Entering restoreComponentState");

			Object serializedComponentStates;
			if (isSavingStateInClient (facesContext)) {
				RenderKit renderKit = RenderKitFactory.getRenderKit (facesContext, renderKitId);
				ResponseStateManager responseStateManager = renderKit.getResponseStateManager ();
				responseStateManager.getTreeStructureToRestore (facesContext, viewId); //ignore result. Must call for compatibility with sun implementation.
				serializedComponentStates = responseStateManager.getComponentStateToRestore (facesContext);
			}
			else {
				HttpSession session = (HttpSession) facesContext.getExternalContext ().getSession (false);
				if (session == null)
					serializedComponentStates = null;
				else {
					string key = ((IExtendedViewHandler) facesContext.getApplication ().getViewHandler ()).EncodeNamespace (facesContext, VIEWSTATE);
					SerializedView serializedView = session.getAttribute (key) as SerializedView;
					if (serializedView == null)
						serializedComponentStates = null;
					else
						serializedComponentStates = serializedView.getState ();
				}
			}
			((UIComponent) uiViewRoot.getChildren ().get (0)).processRestoreState (facesContext, serializedComponentStates);

			Trace.WriteLine ("Exiting restoreComponentState");
		}
	}
}
