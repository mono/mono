namespace System.Web.Mvc {
    using System.Collections.Generic;

    public class ControllerInstanceFilterProvider : IFilterProvider {
        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor) {
            if (controllerContext.Controller != null) {
                // Use FilterScope.First and Order of Int32.MinValue to ensure controller instance methods always run first
                yield return new Filter(controllerContext.Controller, FilterScope.First, Int32.MinValue);
            }
        }
    }
}
