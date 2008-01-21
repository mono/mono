using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.application;
using javax.faces.component;
using javax.faces.context;
using System.Diagnostics;
using System.Web.UI;
using javax.faces.render;
using javax.faces;

namespace Mainsoft.Web.Hosting
{
	public abstract class BaseFacesStateManager : StateManager
	{
		protected static readonly string VIEWSTATE = "__VIEWSTATE";
		protected static readonly RenderKitFactory RenderKitFactory = (RenderKitFactory) FactoryFinder.getFactory (FactoryFinder.RENDER_KIT_FACTORY);

		public override StateManager.SerializedView saveSerializedView (FacesContext facesContext) {
			Object treeStruct = getTreeStructureToSave (facesContext);
			Object compStates = getComponentStateToSave (facesContext);
			SerializedView serializedView = new SerializedView (this, treeStruct, compStates);
			return serializedView;
		}

		protected override sealed Object getTreeStructureToSave (FacesContext facesContext) {
			return String.Empty;
		}

		public override UIViewRoot restoreView (FacesContext facesContext,
																 String viewId,
																String renderKitId) {

			UIViewRoot uiViewRoot = restoreTreeStructure (facesContext, viewId, renderKitId);
			Page page = (Page) uiViewRoot.getChildren ().get (0);
			if (page.IsPostBack || page.IsCallback)
				restoreComponentState (facesContext, uiViewRoot, renderKitId);
			else
				facesContext.renderResponse ();
			return uiViewRoot;
		}

		protected override sealed UIViewRoot restoreTreeStructure (FacesContext facesContext, string viewId, string renderKitId) {
			return facesContext.getApplication ().getViewHandler ().createView (facesContext, viewId);
		}

		protected override Object getComponentStateToSave (FacesContext facesContext) {
			Trace.WriteLine ("Entering getComponentStateToSave");

			UIViewRoot viewRoot = facesContext.getViewRoot ();
			if (viewRoot.isTransient ()) {
				return null;
			}

			Object serializedComponentStates = ((UIComponent) viewRoot.getChildren ().get (0)).processSaveState (facesContext);
			//Locale is a state attribute of UIViewRoot and need not be saved explicitly
			Trace.WriteLine ("Exiting getComponentStateToSave");
			return serializedComponentStates;
		}
	}
}
