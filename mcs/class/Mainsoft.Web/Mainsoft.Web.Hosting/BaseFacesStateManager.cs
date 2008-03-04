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

		protected void SaveStateInClient (FacesContext facesContext, StateManager.SerializedView serializedView) {
			//UIViewRoot uiViewRoot = facesContext.getViewRoot ();
			////save state in response (client-side: full state; server-side: sequence)
			//RenderKit renderKit = RenderKitFactory.getRenderKit (facesContext, uiViewRoot.getRenderKitId ());
			//// not us.
			//renderKit.getResponseStateManager ().writeState (facesContext, serializedView);

			java.io.ByteArrayOutputStream bytearrayoutputstream = new java.io.ByteArrayOutputStream ();
			java.io.ObjectOutputStream objectoutputstream = new java.io.ObjectOutputStream (bytearrayoutputstream);

			//ignore tree structure
			//objectoutputstream.writeObject (serializedView.getStructure ());
			objectoutputstream.writeObject (serializedView.getState ());
			objectoutputstream.close ();
			bytearrayoutputstream.close ();

			string s = 
@"<div>
	<input type=""hidden"" name=""" + VIEWSTATE + "\" id=\"" + VIEWSTATE + "\" value=\"" +
				Convert.ToBase64String ((byte []) vmw.common.TypeUtils.ToByteArray (bytearrayoutputstream.toByteArray ())) + @""" />
</div>";
			facesContext.getResponseWriter ().write (s);
		}

		protected object GetStateFromClient (FacesContext facesContext, String viewId, String renderKitId) {
			//RenderKit renderKit = RenderKitFactory.getRenderKit (facesContext, renderKitId);
			//ResponseStateManager responseStateManager = renderKit.getResponseStateManager ();
			//responseStateManager.getTreeStructureToRestore (facesContext, viewId); //ignore result. Must call for compatibility with sun implementation.
			//return responseStateManager.getComponentStateToRestore (facesContext);

			java.util.Map map = facesContext.getExternalContext ().getRequestParameterMap ();
			string s1 = (string) map.get (VIEWSTATE);

			byte [] buffer = Convert.FromBase64String (s1);
			java.io.ByteArrayInputStream bytearrayinputstream = new java.io.ByteArrayInputStream (vmw.common.TypeUtils.ToSByteArray (buffer));
			java.io.ObjectInputStream inputStream = new java.io.ObjectInputStream (bytearrayinputstream);
			//ignore tree structure
			//inputStream.readObject ();
			object state = inputStream.readObject ();
			inputStream.close ();
			bytearrayinputstream.close ();

			return state;
		}
	}
}
