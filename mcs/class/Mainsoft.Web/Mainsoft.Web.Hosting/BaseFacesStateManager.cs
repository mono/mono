using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.application;
using javax.faces.component;
using javax.faces.context;

namespace Mainsoft.Web.Hosting
{
	public abstract class BaseFacesStateManager : StateManager
	{
		public override StateManager.SerializedView saveSerializedView (FacesContext facesContext) {
			Object treeStruct = getTreeStructureToSave (facesContext);
			Object compStates = getComponentStateToSave (facesContext);
			SerializedView serializedView = new SerializedView (this, treeStruct, compStates);
			return serializedView;
		}

		protected override Object getTreeStructureToSave (FacesContext facesContext) {
			return null;
		}

		public override UIViewRoot restoreView (FacesContext facesContext,
																 String viewId,
																String renderKitId) {

			UIViewRoot uiViewRoot = restoreTreeStructure (facesContext, viewId, renderKitId);
			restoreComponentState (facesContext, uiViewRoot, renderKitId);
			return uiViewRoot;
		}

		protected override UIViewRoot restoreTreeStructure (FacesContext facesContext, string viewId, string renderKitId) {
			return facesContext.getApplication ().getViewHandler ().createView (facesContext, viewId);
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
	}
}
