namespace System.Web.Mvc {

    public static class ViewEngines {

        private readonly static ViewEngineCollection _engines = new ViewEngineCollection {
            new WebFormViewEngine(),
            new RazorViewEngine(),
        };

        public static ViewEngineCollection Engines {
            get {
                return _engines;
            }
        }
    }
}
