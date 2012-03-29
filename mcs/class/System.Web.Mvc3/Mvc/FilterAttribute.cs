namespace System.Web.Mvc {
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Web.Mvc.Resources;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class FilterAttribute : Attribute, IMvcFilter {
        private readonly static ConcurrentDictionary<Type, bool> _multiuseAttributeCache = new ConcurrentDictionary<Type, bool>();
        private int _order = Filter.DefaultOrder;

        private static bool AllowsMultiple(Type attributeType) {
            return _multiuseAttributeCache.GetOrAdd(
                attributeType,
                type => type.GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                            .Cast<AttributeUsageAttribute>()
                            .First()
                            .AllowMultiple
            );
        }

        public bool AllowMultiple {
            get {
                return AllowsMultiple(GetType());
            }
        }

        public int Order {
            get {
                return _order;
            }
            set {
                if (value < Filter.DefaultOrder) {
                    throw new ArgumentOutOfRangeException("value", MvcResources.FilterAttribute_OrderOutOfRange);
                }
                _order = value;
            }
        }
    }
}
