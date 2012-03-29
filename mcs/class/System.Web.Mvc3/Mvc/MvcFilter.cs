namespace System.Web.Mvc {
    public abstract class MvcFilter : IMvcFilter {
        protected MvcFilter() {
        }

        protected MvcFilter(bool allowMultiple, int order) {
            AllowMultiple = allowMultiple;
            Order = order;
        }

        public bool AllowMultiple {
            get;
            private set;
        }

        public int Order {
            get;
            private set;
        }
    }
}
