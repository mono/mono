namespace System.Web.Mvc {

    // represents a result that doesn't do anything, like a controller action returning null
    public class EmptyResult : ActionResult {

        private static readonly EmptyResult _singleton = new EmptyResult();

        internal static EmptyResult Instance {
            get {
                return _singleton;
            }
        }

        public override void ExecuteResult(ControllerContext context) {
        }
    }
}
