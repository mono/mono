namespace System.Web.Mvc {
    internal interface IResolver<T> {
        T Current { get; }
    }
}
