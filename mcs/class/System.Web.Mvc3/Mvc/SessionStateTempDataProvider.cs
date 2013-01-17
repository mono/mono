namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc.Resources;

    public class SessionStateTempDataProvider : ITempDataProvider {
        internal const string TempDataSessionStateKey = "__ControllerTempData";

        public virtual IDictionary<string, object> LoadTempData(ControllerContext controllerContext) {
            HttpSessionStateBase session = controllerContext.HttpContext.Session;

            if (session != null) {
                Dictionary<string, object> tempDataDictionary = session[TempDataSessionStateKey] as Dictionary<string, object>;

                if (tempDataDictionary != null) {
                    // If we got it from Session, remove it so that no other request gets it
                    session.Remove(TempDataSessionStateKey);
                    return tempDataDictionary;
                }
            }

            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public virtual void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }

            HttpSessionStateBase session = controllerContext.HttpContext.Session;
            bool isDirty = (values != null && values.Count > 0);

            if (session == null) {
                if (isDirty) {
                    throw new InvalidOperationException(MvcResources.SessionStateTempDataProvider_SessionStateDisabled);
                }
            }
            else {
                if (isDirty) {
                    session[TempDataSessionStateKey] = values;
                }
                else {
                    // Since the default implementation of Remove() (from SessionStateItemCollection) dirties the
                    // collection, we shouldn't call it unless we really do need to remove the existing key.
                    if (session[TempDataSessionStateKey] != null) {
                        session.Remove(TempDataSessionStateKey);
                    }
                }
            }
        }

    }
}
