namespace System.Web.UI.HtmlControls {
    using System;
    using System.ComponentModel;

    public class HtmlTitle : HtmlControl {
        private string _text;


        public HtmlTitle() : base("title") {
        }


        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        Localizable(true),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public virtual string Text {
            get {
                if (_text == null) {
                    return String.Empty;
                }
                return _text;
            }
            set {
                _text = value;
            }
        }


        protected override void AddParsedSubObject(object obj) {
            if (obj is LiteralControl) {
                _text = ((LiteralControl)obj).Text;
            }
            else {
                base.AddParsedSubObject(obj);
            }
        }

        // Allow child controls to support databinding expressions as inner text.
        protected override ControlCollection CreateControlCollection() {
            return new ControlCollection(this);
        }


        protected internal override void Render(HtmlTextWriter writer) {
            writer.RenderBeginTag(HtmlTextWriterTag.Title);

            if (HasControls() || HasRenderDelegate()) {
                RenderChildren(writer);
            }
            else if (_text != null) {
                writer.Write(_text);
            }

            writer.RenderEndTag();
        }
    }
}
