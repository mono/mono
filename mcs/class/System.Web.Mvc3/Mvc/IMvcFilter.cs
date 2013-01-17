namespace System.Web.Mvc {
    public interface IMvcFilter {
        bool AllowMultiple { get; }
        int Order { get; }
    }
}
