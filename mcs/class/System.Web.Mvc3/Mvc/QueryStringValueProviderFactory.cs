namespace System.Web.Mvc {
    using System;
    using System.Collections.Specialized;
    using System.Web.Helpers;

    public sealed class QueryStringValueProviderFactory : ValueProviderFactory {

        private readonly UnvalidatedRequestValuesAccessor _unvalidatedValuesAccessor;

        public QueryStringValueProviderFactory()
            : this(null) {
        }

        // For unit testing
        internal QueryStringValueProviderFactory(UnvalidatedRequestValuesAccessor unvalidatedValuesAccessor) {
            _unvalidatedValuesAccessor = unvalidatedValuesAccessor ?? (cc => new UnvalidatedRequestValuesWrapper(cc.HttpContext.Request.Unvalidated()));
        }

        public override IValueProvider GetValueProvider(ControllerContext controllerContext) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }

            return new QueryStringValueProvider(controllerContext, _unvalidatedValuesAccessor(controllerContext));
        }

    }
}
