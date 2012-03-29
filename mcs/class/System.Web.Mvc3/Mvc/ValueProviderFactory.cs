namespace System.Web.Mvc {
    using System;

    public abstract class ValueProviderFactory {
        public abstract IValueProvider GetValueProvider(ControllerContext controllerContext);
    }
}
