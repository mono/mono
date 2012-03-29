namespace System.Web.Mvc {

    internal interface IViewStartPageChild {
        HtmlHelper<object> Html { get; }
        UrlHelper Url { get; }
        ViewContext ViewContext { get; }
    }
}
