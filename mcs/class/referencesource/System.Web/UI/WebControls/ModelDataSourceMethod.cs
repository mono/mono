using System.Collections.Specialized;
using System.Reflection;

namespace System.Web.UI.WebControls {
    /// <summary>
    /// Encapsulates the information about a Data Method used by <see cref='System.Web.UI.WebControls.ModelDataSourceView' />.
    /// </summary>
    public class ModelDataSourceMethod {

        private Lazy<OrderedDictionary> _parameters = new Lazy<OrderedDictionary>();

        /// <summary>
        /// The instance on which the method will be invoked. For static methods, this will be null.
        /// </summary>
        public object Instance {
            get;
            internal set;
        }

        /// <summary>
        /// Method parameter values.
        /// </summary>
        public OrderedDictionary Parameters {
            get {
                return _parameters.Value;
            }
        }

        /// <summary>
        /// The method to be invoked.
        /// </summary>
        public MethodInfo MethodInfo {
            get;
            private set;
        }

        public ModelDataSourceMethod(object instance, MethodInfo methodInfo) {
            if (methodInfo == null) {
                throw new ArgumentNullException("methodInfo");
            }

            Instance = instance;
            MethodInfo = methodInfo;
        }
    }
}
