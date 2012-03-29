namespace System.Web.Mvc {
    public static class GlobalFilters {
        static GlobalFilters() {
            Filters = new GlobalFilterCollection();
        }

        public static GlobalFilterCollection Filters {
            get;
            private set;
        }
    }
}
