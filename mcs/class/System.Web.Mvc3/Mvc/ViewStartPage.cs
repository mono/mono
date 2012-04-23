namespace System.Web.Mvc {
    using System.Web.Mvc.Resources;
    using System.Web.WebPages;

    public abstract class ViewStartPage : StartPage, IViewStartPageChild {
        private IViewStartPageChild _viewStartPageChild;

        public HtmlHelper<object> Html {
            get {
                return ViewStartPageChild.Html;
            }
        }

        public UrlHelper Url {
            get {
                return ViewStartPageChild.Url;
            }
        }

        public ViewContext ViewContext {
            get {
                return ViewStartPageChild.ViewContext;
            }
        }

        internal IViewStartPageChild ViewStartPageChild {
            get {
                if (_viewStartPageChild == null) {
                    IViewStartPageChild child = base.ChildPage as IViewStartPageChild;
                    if (child == null) {
                        throw new InvalidOperationException(MvcResources.ViewStartPage_RequiresMvcRazorView);
                    }
                    _viewStartPageChild = child;
                }

                return _viewStartPageChild;
            }
        }
    }
}
