namespace System.Web.Mvc {
    using System.Collections.Generic;

    public interface IFilterProvider {
        IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor);
    }
}
