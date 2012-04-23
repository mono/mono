namespace System.Web.Mvc {
    using System.ComponentModel;
    using System.Web.WebPages.Scope;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStartCode {
        private static bool _startWasCalled;

        public static void Start() {
            // Guard against multiple calls. All Start calls are made on same thread, so no lock needed here
            if (_startWasCalled) {
                return;
            }
            _startWasCalled = true;

            System.Web.WebPages.Razor.PreApplicationStartCode.Start();
            System.Web.WebPages.PreApplicationStartCode.Start();

            ViewContext.GlobalScopeThunk = () => ScopeStorage.CurrentScope;
        }
    }
}
