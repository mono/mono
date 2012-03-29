namespace System.Web.Mvc {
    using System;

    public static class ValueProviderFactories {

        private static readonly ValueProviderFactoryCollection _factories = new ValueProviderFactoryCollection() {
            new ChildActionValueProviderFactory(),
            new FormValueProviderFactory(),
            new JsonValueProviderFactory(),
            new RouteDataValueProviderFactory(),
            new QueryStringValueProviderFactory(),
            new HttpFileCollectionValueProviderFactory(),
        };

        public static ValueProviderFactoryCollection Factories {
            get {
                return _factories;
            }
        }

    }
}
