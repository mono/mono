namespace System.Web.Routing {
    // Represents a URL generated from a ParsedRoute
    internal class BoundUrl {
        public string Url {
            get;
            set;
        }

        public RouteValueDictionary Values {
            get;
            set;
        }
    }
}
