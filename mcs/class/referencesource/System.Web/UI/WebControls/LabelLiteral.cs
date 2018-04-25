using System;
using System.Collections.Generic;
using System.Text;

namespace System.Web.UI.WebControls {
    // Used in the login controls for accessibility
    internal sealed class LabelLiteral : Literal {
        internal Control _for;
        internal bool _renderAsLabel = false;

        internal LabelLiteral(Control forControl) {
            _for = forControl;
        }

        internal bool RenderAsLabel {
            get {
                return _renderAsLabel;
            }
            set {
                _renderAsLabel = value;
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            // Render as a label in designer for accessibility
            if (RenderAsLabel) {
                // Total hack for accessibility of labels for login controls!
                writer.Write("<asp:label runat=\"server\" AssociatedControlID=\"");
                writer.Write(_for.ID);
                writer.Write("\" ID=\"");
                writer.Write(_for.ID);
                writer.Write("Label\">");
                writer.Write(Text);
                writer.Write("</asp:label>");
            }
            else {
                writer.AddAttribute(HtmlTextWriterAttribute.For, _for.ClientID);
                writer.RenderBeginTag(HtmlTextWriterTag.Label);
                base.Render(writer);
                writer.RenderEndTag();
            }
        }
    }
}
