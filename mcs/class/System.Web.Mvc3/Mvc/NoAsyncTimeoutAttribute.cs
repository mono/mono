namespace System.Web.Mvc {
    using System.Threading;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class NoAsyncTimeoutAttribute : AsyncTimeoutAttribute {

        public NoAsyncTimeoutAttribute()
            : base(Timeout.Infinite) {
        }

    }
}
