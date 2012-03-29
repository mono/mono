namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web.Routing;
    using System.Web.Script.Serialization;

    public class AjaxHelper {

        private static string _globalizationScriptPath;

        public AjaxHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, RouteTable.Routes) {
        }

        public AjaxHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection) {
            if (viewContext == null) {
                throw new ArgumentNullException("viewContext");
            }
            if (viewDataContainer == null) {
                throw new ArgumentNullException("viewDataContainer");
            }
            if (routeCollection == null) {
                throw new ArgumentNullException("routeCollection");
            }
            ViewContext = viewContext;
            ViewDataContainer = viewDataContainer;
            RouteCollection = routeCollection;
        }

        public static string GlobalizationScriptPath {
            get {
                if (String.IsNullOrEmpty(_globalizationScriptPath)) {
                    _globalizationScriptPath = "~/Scripts/Globalization";
                }
                return _globalizationScriptPath;
            }
            set {
                _globalizationScriptPath = value;
            }
        }

        public RouteCollection RouteCollection {
            get;
            private set;
        }

        public ViewContext ViewContext {
            get;
            private set;
        }

        public ViewDataDictionary ViewData {
            get {
                return ViewDataContainer.ViewData;
            }
        }

        public IViewDataContainer ViewDataContainer {
            get;
            private set;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Instance method for consistency with other helpers.")]
        public string JavaScriptStringEncode(string message) {
            if (String.IsNullOrEmpty(message)) {
                return message;
            }

            StringBuilder builder = new StringBuilder();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.Serialize(message, builder);
            return builder.ToString(1, builder.Length - 2); // remove first + last quote
        }
    }
}
