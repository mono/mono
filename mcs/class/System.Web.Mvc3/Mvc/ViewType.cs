namespace System.Web.Mvc {
    using System.ComponentModel;
    using System.Web.UI;

    [ControlBuilder(typeof(ViewTypeControlBuilder))]
    [NonVisualControl]
    public class ViewType : Control {
        private string _typeName;

        [DefaultValue("")]
        public string TypeName {
            get {
                return _typeName ?? String.Empty;
            }
            set {
                _typeName = value;
            }
        }
    }
}
