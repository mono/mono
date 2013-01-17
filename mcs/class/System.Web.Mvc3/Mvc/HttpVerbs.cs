namespace System.Web.Mvc {
    using System;

    [Flags]
    public enum HttpVerbs {
        Get = 1 << 0,
        Post = 1 << 1,
        Put = 1 << 2,
        Delete = 1 << 3,
        Head = 1 << 4
    }
}
