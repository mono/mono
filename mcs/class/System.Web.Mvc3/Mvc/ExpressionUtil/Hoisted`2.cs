namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Collections.Generic;

    internal delegate TValue Hoisted<TModel, TValue>(TModel model, List<object> capturedConstants);

}
