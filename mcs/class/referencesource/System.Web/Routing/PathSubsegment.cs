namespace System.Web.Routing {

    // Represents a subsegment of a ContentPathSegment such as a parameter or a literal.
    internal abstract class PathSubsegment {
#if ROUTE_DEBUGGING
        public abstract string LiteralText {
            get;
        }
#endif
    }
}
