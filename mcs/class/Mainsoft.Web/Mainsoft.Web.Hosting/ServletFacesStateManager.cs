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

namespace Mainsoft.Web.Hosting
{
	public sealed class ServletFacesStateManager : BaseFacesStateManager
	{
		static RenderKitFactory RenderKitFactory = (RenderKitFactory) FactoryFinder.getFactory (FactoryFinder.RENDER_KIT_FACTORY);

		public override void writeState (FacesContext facesContext, StateManager.SerializedView serializedView) {
			if (serializedView != null) {
				UIViewRoot uiViewRoot = facesContext.getViewRoot ();
				//save state in response (client-side: full state; server-side: sequence)
				RenderKit renderKit = RenderKitFactory.getRenderKit (facesContext, uiViewRoot.getRenderKitId ());
				// not us.
				renderKit.getResponseStateManager ().writeState (facesContext, serializedView);
			}
		}

		protected override void restoreComponentState (FacesContext facesContext,
												  javax.faces.component.UIViewRoot uiViewRoot,
												  String renderKitId) {

			Console.WriteLine ("Entering restoreComponentState");

			Object serializedComponentStates;

			if (isSavingStateInClient (facesContext)) {
				RenderKit renderKit = RenderKitFactory.getRenderKit (facesContext, renderKitId);
				ResponseStateManager responseStateManager = renderKit.getResponseStateManager ();
				serializedComponentStates = responseStateManager.getComponentStateToRestore (facesContext);
			}
			else {
				throw new NotImplementedException ();
			}

			if (serializedComponentStates == null) {
				Console.WriteLine ("No serialized component state found!");
				facesContext.renderResponse ();
				return;
			}

			if (uiViewRoot.getRenderKitId () == null) {
				//Just to be sure...
				uiViewRoot.setRenderKitId (renderKitId);
			}

			// now ask the view root component to restore its state
			((UIComponent) uiViewRoot.getChildren ().get (0)).processRestoreState (facesContext, serializedComponentStates);

			Console.WriteLine ("Exiting restoreComponentState");
		}

	}
}
