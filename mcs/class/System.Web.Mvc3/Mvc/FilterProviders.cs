namespace System.Web.Mvc {
    public static class FilterProviders {
        static FilterProviders() {
            Providers = new FilterProviderCollection();
            Providers.Add(GlobalFilters.Filters);
            Providers.Add(new FilterAttributeFilterProvider());
            Providers.Add(new ControllerInstanceFilterProvider());
        }

        public static FilterProviderCollection Providers {
            get;
            private set;
        }
    }
}