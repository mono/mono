namespace System.Web.Routing {

    // Represents a "/" separator in a URL
    internal sealed class SeparatorPathSegment : PathSegment {
#if ROUTE_DEBUGGING
        public override string LiteralText {
            get {
                return "/";
            }
        }

        public override string ToString() {
            return "\"/\"";
        }
#endif
    }
}
