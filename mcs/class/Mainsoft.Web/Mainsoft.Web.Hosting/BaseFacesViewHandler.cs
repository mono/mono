using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.application;
using java.util;
using javax.faces.context;
using javax.faces.component;
using javax.faces.render;

namespace Mainsoft.Web.Hosting
{
	public abstract class BaseFacesViewHandler : ViewHandler
	{

		public override Locale calculateLocale (FacesContext facesContext) {
			Iterator locales = facesContext.getExternalContext ().getRequestLocales ();
			while (locales.hasNext ()) {
				Locale locale = (Locale) locales.next ();
				for (Iterator it = facesContext.getApplication ().getSupportedLocales (); it.hasNext (); ) {
					Locale supportLocale = (Locale) it.next ();
					// higher priority to a language match over an exact match
					// that occures further down (see Jstl Reference 1.0 8.3.1)
					if (locale.getLanguage ().Equals (supportLocale.getLanguage ()) &&
						(supportLocale.getCountry () == null ||
							supportLocale.getCountry ().Length == 0)) {
						return supportLocale;
					}
					else if (supportLocale.Equals (locale)) {
						return supportLocale;
					}
				}
			}

			Locale defaultLocale = facesContext.getApplication ().getDefaultLocale ();
			return defaultLocale != null ? defaultLocale : Locale.getDefault ();
		}

		public override String calculateRenderKitId (FacesContext facesContext) {
			String renderKitId = facesContext.getApplication ().getDefaultRenderKitId ();
			return (renderKitId != null) ? renderKitId : RenderKitFactory.HTML_BASIC_RENDER_KIT;
			//TODO: how to calculate from client?
		}

		/**
		 */
		public override UIViewRoot createView (FacesContext facesContext, String viewId) {
			Application application = facesContext.getApplication ();
			ViewHandler applicationViewHandler = application.getViewHandler ();

			Locale currentLocale = null;
			String currentRenderKitId = null;
			UIViewRoot uiViewRoot = facesContext.getViewRoot ();
			if (uiViewRoot != null) {
				//Remember current locale and renderKitId
				currentLocale = uiViewRoot.getLocale ();
				currentRenderKitId = uiViewRoot.getRenderKitId ();
			}

			uiViewRoot = (UIViewRoot) application.createComponent (UIViewRoot.COMPONENT_TYPE);
			//      as of JSF spec page 7-16:
			//      "It is the callers responsibility to ensure that setViewId() is called
			//      on the returned view, passing the same viewId value."
			//      so we do not set the viewId here

			//      ok, but the RI does so, so let's do it, too.
			uiViewRoot.setViewId (viewId);

			if (currentLocale != null) {
				//set old locale
				uiViewRoot.setLocale (currentLocale);
			}
			else {
				//calculate locale
				uiViewRoot.setLocale (applicationViewHandler.calculateLocale (facesContext));
			}

			if (currentRenderKitId != null) {
				//set old renderKit
				uiViewRoot.setRenderKitId (currentRenderKitId);
			}
			else {
				//calculate renderKit
				uiViewRoot.setRenderKitId (applicationViewHandler.calculateRenderKitId (facesContext));
			}

			AspNetFacesContext aspNetFacesContext = (AspNetFacesContext) facesContext;
			uiViewRoot.getChildren ().add (0, (UIComponent) (object) aspNetFacesContext.Handler);
			return uiViewRoot;
		}

		public override String getResourceURL (FacesContext facesContext, String path) {
			if (path.Length > 0 && path [0] == '/') {
				return facesContext.getExternalContext ().getRequestContextPath () + path;
			}
			else {
				return path;
			}
		}

		public override void renderView (FacesContext facesContext, UIViewRoot viewToRender) {
			if (viewToRender == null) {
				throw new ArgumentNullException ("viewToRender", "viewToRender must not be null");
			}

			AspNetFacesContext aspNetFacesContext = (AspNetFacesContext) facesContext;
			((UIComponent) (object) aspNetFacesContext.Handler).encodeChildren (facesContext);
		}


		public override UIViewRoot restoreView (FacesContext facesContext, String viewId) {
			Application application = facesContext.getApplication ();
			ViewHandler applicationViewHandler = application.getViewHandler ();
			String renderKitId = applicationViewHandler.calculateRenderKitId (facesContext);
			UIViewRoot viewRoot = application.getStateManager ().restoreView (facesContext,
																			viewId,
																			renderKitId);
			return viewRoot;
		}

		/**
		 * Writes a state marker that is replaced later by one or more hidden form
		 * inputs.
		 *
		 * @param facesContext
		 * @throws IOException
		 */
		public override void writeState (FacesContext facesContext) {
			throw new NotImplementedException ();
		}

	}
}
