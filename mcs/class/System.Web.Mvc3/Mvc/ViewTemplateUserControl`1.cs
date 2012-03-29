namespace System.Web.Mvc {
    public class ViewTemplateUserControl<TModel> : ViewUserControl<TModel> {
        protected string FormattedModelValue {
            get { return ViewData.TemplateInfo.FormattedModelValue.ToString(); }
        }
    }
}
