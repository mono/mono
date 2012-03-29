namespace System.Web.Mvc {
    using System.Collections.Generic;

    public interface ITempDataProvider {
        IDictionary<string, object> LoadTempData(ControllerContext controllerContext);
        void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values);
    }
}
