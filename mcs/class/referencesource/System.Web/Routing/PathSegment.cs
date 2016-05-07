namespace System.Web.Routing {

    // Represents a segment of a URL such as a separator or content
    internal abstract class PathSegment {
#if ROUTE_DEBUGGING
        public abstract string LiteralText {
            get;
        }
#endif
    }
}
