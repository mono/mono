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
	public sealed class ServletFacesStateManager : StateManager
	{
		static RenderKitFactory RenderKitFactory = (RenderKitFactory) FactoryFinder.getFactory (FactoryFinder.RENDER_KIT_FACTORY);

		public override StateManager.SerializedView saveSerializedView (FacesContext facesContext) {
			Object treeStruct = getTreeStructureToSave (facesContext);
			Object compStates = getComponentStateToSave (facesContext);
			SerializedView serializedView = new SerializedView (this, treeStruct, compStates);

			//if (!isSavingStateInClient (facesContext))
			//    saveSerializedViewInSession (facesContext, facesContext.getViewRoot ().getViewId (), serializedView);

			return serializedView;
		}

		static readonly object [] emptyArray = new object [0];

		protected override Object getTreeStructureToSave (FacesContext facesContext) {
			UIViewRoot viewRoot = facesContext.getViewRoot ();
			//kostat ???
			//if (((StateHolder) viewRoot).isTransient ()) {
			//    return null;
			//}

			Console.WriteLine ("saving root:" + viewRoot.getViewId ());
			return new int [] { 1, 2, 3 };

			//myfaces save id. What is it for?
			// save ViewRoot tree
			//return new String [] { viewRoot.GetType().FullName, viewRoot.getId () };
		}

		protected override Object getComponentStateToSave (FacesContext facesContext) {
			Console.WriteLine ("Entering getComponentStateToSave");

			UIViewRoot viewRoot = facesContext.getViewRoot ();
			if (viewRoot.isTransient ()) {
				return null;
			}

			Object serializedComponentStates = viewRoot.processSaveState (facesContext);
			//Locale is a state attribute of UIViewRoot and need not be saved explicitly
			Console.WriteLine ("Exiting getComponentStateToSave");
			return serializedComponentStates;
		}

		public override void writeState (FacesContext facesContext,
										StateManager.SerializedView serializedView) {
			if (serializedView != null) {
				UIViewRoot uiViewRoot = facesContext.getViewRoot ();
				//save state in response (client-side: full state; server-side: sequence)
				RenderKit renderKit = RenderKitFactory.getRenderKit (facesContext, uiViewRoot.getRenderKitId ());
				// not us.
				renderKit.getResponseStateManager ().writeState (facesContext, serializedView);
			}
		}

		public override UIViewRoot restoreView (FacesContext facesContext,
																 String viewId,
																String renderKitId) {

			UIViewRoot uiViewRoot = restoreTreeStructure (facesContext, viewId, renderKitId);
			if (uiViewRoot != null) {
				uiViewRoot.setViewId (viewId);
				restoreComponentState (facesContext, uiViewRoot, renderKitId);
				String restoredViewId = uiViewRoot.getViewId ();
				if (restoredViewId == null || !(restoredViewId.Equals (viewId))) {
					return null;
				}
			}
			return uiViewRoot;
		}

		protected override UIViewRoot restoreTreeStructure (FacesContext facesContext,
																			 String viewId,
																			 String renderKitId) {
			Console.WriteLine ("Entering restoreTreeStructure");

			UIViewRoot uiViewRoot;
			if (isSavingStateInClient (facesContext)) {
				RenderKit rk = RenderKitFactory.getRenderKit (facesContext, renderKitId);
				ResponseStateManager responseStateManager = rk.getResponseStateManager ();
				Object treeStructure = responseStateManager.getTreeStructureToRestore (facesContext, viewId);
				if (treeStructure == null) {
					Console.WriteLine ("Exiting restoreTreeStructure - No tree structure state found in client request");
					return null;
				}

				AspNetFacesContext aspNetFacesContext = (AspNetFacesContext) facesContext;
				UIComponent page = aspNetFacesContext.Handler as UIComponent;
				if (page == null)
					return null;
				
				uiViewRoot = facesContext.getApplication ().getViewHandler ().createView (facesContext, viewId);
				uiViewRoot.getChildren ().add (0, page);
			}
			else {
				throw new NotImplementedException ();
			}

			Console.WriteLine ("Exiting restoreTreeStructure");
			return uiViewRoot;
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
				uiViewRoot.setViewId (null);
				return;
			}

			if (uiViewRoot.getRenderKitId () == null) {
				//Just to be sure...
				uiViewRoot.setRenderKitId (renderKitId);
			}

			// now ask the view root component to restore its state
			uiViewRoot.processRestoreState (facesContext, serializedComponentStates);

			Console.WriteLine ("Exiting restoreComponentState");
		}

		//readonly object _stateKey = new object ();


		//SerializedView getSerializedViewFromServletSession (FacesContext facesContext, string viewId) {
		//    Map sessionMap = facesContext.getExternalContext ().getSessionMap ();
		//    System.Collections.Hashtable states = sessionMap.get (_stateKey) as System.Collections.Hashtable;
		//    if (states == null)
		//        return null;

		//    return states [viewId] as SerializedView;
		//}

		//void saveSerializedViewInSession (FacesContext context, string viewId, SerializedView serializedView) {
		//    Map sessionMap = context.getExternalContext ().getSessionMap ();

		//    System.Collections.Hashtable states = sessionMap.get (_stateKey) as System.Collections.Hashtable;
		//    if (states == null) {
		//        states = new System.Collections.Hashtable ();
		//        sessionMap.put (_stateKey, states);
		//    }

		//    states [viewId] = serializedView;
		//}

	}
}
