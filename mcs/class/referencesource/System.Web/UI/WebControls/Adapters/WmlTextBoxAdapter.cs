//------------------------------------------------------------------------------
// <copyright file="WmlTextBoxAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.Adapters;
    using System.Globalization;
    using System.Web.UI.WebControls;

    // Provides adaptive rendering for the TextBox control.
    public class WmlTextBoxAdapter : TextBoxAdapter {
        private String _staticValue;

        // Called during the Init lifecycle phase.
        protected internal override void OnInit(EventArgs e) {
            _staticValue = Control.Text;
            base.OnInit(e);
        }

        // Renders the control.
        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter)markupWriter;

            String value = Control.Text;
            bool requiresRandomID = false;
            bool password = Control.TextMode == TextBoxMode.Password;

            // writer.EnterLayout(Style);

            if (Control.TextMode == TextBoxMode.Password) {
                value = String.Empty;
                // 
                requiresRandomID = true;
            }

            ((WmlPageAdapter)PageAdapter).RegisterPostField(writer, Control.UniqueID, Control.ClientID, true /* isDynamic */, requiresRandomID);
            
            if (!password) {
                // Add a form variable for the textbox to populate it.  
                // We need to do this since OnEnterForward values override the value attribute -see WML 1.1 spec.
                // Do not add the form variable if the text mode is password, since we don't want it to be displayed on
                // the client.
                ((WmlPageAdapter)PageAdapter).AddFormVariable(writer, Control.ClientID, value, requiresRandomID);
                // Note: AddFormVariable calls MapClientIDToShortName.
            }
            else {
                // This is to make sure an id is determined in the first
                // pass, and this is done in AddFormVariable as well.
                writer.MapClientIDToShortName(Control.ClientID, requiresRandomID);
            }

            // 

            RenderTextBox((WmlTextWriter)writer, Control.ClientID, 
                          value,
                          null /* format */, 
                          null /* title */,
                          password, 
                          Control.Columns /* size */, 
                          Control.MaxLength /* maxLength */, 
                          requiresRandomID);
            // writer.ExitLayout(Style);
        }

        // Renders the TextBox.
        public virtual void RenderTextBox(WmlTextWriter writer, String id, String value, String format, String title, bool password, int size, int maxLength, bool generateRandomID) {
            if (!writer.AnalyzeMode) {
                // 
                                
                // VSWhidbey 147458.  Close any style tags.
                writer.CloseCurrentStyleTags();
                writer.WriteBeginTag("input");
                // Map the client ID to a short name. See
                // MapClientIDToShortName for details.
                writer.WriteAttribute("name", writer.MapClientIDToShortName(id, generateRandomID));
                if (password) {
                    writer.WriteAttribute("type", "password");
                }
                if (!String.IsNullOrEmpty(format)) {
                    writer.WriteAttribute("format", format);
                }
                if (!String.IsNullOrEmpty(title)) {
                    writer.WriteAttribute("title", title, true);
                }
                if (size > 0) {
                    writer.WriteAttribute("size", size.ToString(CultureInfo.InvariantCulture));
                }
                if (maxLength > 0) {
                    writer.WriteAttribute("maxlength", maxLength.ToString(CultureInfo.InvariantCulture));
                }
                // We do not need a value attribute.  The Text value is populated by the client side var set in onenterforward.
                writer.WriteLine(" />");
                writer.OpenCurrentStyleTags();
            }
        }
    }
}

#endif

