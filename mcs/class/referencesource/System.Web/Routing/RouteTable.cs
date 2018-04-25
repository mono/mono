namespace System.Web.Routing {
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors",
        Justification = "There is no explicit public constructor defined, and we don't want to add a private one.")]
    public class RouteTable {
        private static RouteCollection _instance = new RouteCollection();

        public static RouteCollection Routes {
            get {
                return _instance;
            }
        }
    }
}
