namespace System.Web.Mvc {

    public interface IExceptionFilter {
        void OnException(ExceptionContext filterContext);
    }
}
