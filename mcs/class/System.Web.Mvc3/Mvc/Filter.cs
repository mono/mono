namespace System.Web.Mvc {
    using System.Diagnostics.CodeAnalysis;

    public class Filter {
        public const int DefaultOrder = -1;

        public Filter(object instance, FilterScope scope, int? order) {
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }

            if (order == null) {
                IMvcFilter mvcFilter = instance as IMvcFilter;
                if (mvcFilter != null) {
                    order = mvcFilter.Order;
                }
            }

            Instance = instance;
            Order = order ?? DefaultOrder;
            Scope = scope;
        }

        public object Instance {
            get;
            protected set;
        }

        public int Order {
            get;
            protected set;
        }

        public FilterScope Scope {
            get;
            protected set;
        }
    }
}
