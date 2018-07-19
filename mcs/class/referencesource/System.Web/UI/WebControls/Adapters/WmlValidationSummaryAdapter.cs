//------------------------------------------------------------------------------
// <copyright file="WmlValidationSummaryAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Collections;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class WmlValidationSummaryAdapter : ValidationSummaryAdapter {

        private BulletedList bulletedList;
        private bool singleParagraph = false;

        protected internal override void OnInit(EventArgs e) {
            bulletedList = new BulletedList();
            Control.Controls.Add(bulletedList);
        }

        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter)markupWriter;
            String[] errorMessages = null;
            bool inError = false;
            bool enteredStyle = false;

            if (!Control.Enabled || !Control.Visible || !Control.ShowSummary) {
                return;
            }
            
            errorMessages = Control.GetErrorMessages(out inError);
            if (!inError) {
                return;
            }

            if (Control.DisplayMode == ValidationSummaryDisplayMode.SingleParagraph) {
                singleParagraph = true;
            }
    
            if (Control.HeaderText.Length > 0) {
                writer.EnterStyle(Control.ControlStyle);
                enteredStyle = true;
                writer.WriteEncodedText(Control.HeaderText);
                WriteSeparator(writer);
            }

            if (!String.IsNullOrEmpty(errorMessages)) {
                if (!enteredStyle) {
                    writer.EnterStyle(Control.ControlStyle);
                    enteredStyle = true;
                }

                if (singleParagraph) {
                    foreach (String errorMessage in errorMessages) {
                        Debug.Assert(errorMessage != null && errorMessage.Length > 0, "Bad Error Messages");
                        writer.WriteEncodedText(errorMessage);
                        WriteSeparator(writer);
                    }
                    writer.WriteBreak();
                }
                else {
                    ArrayList arr = new ArrayList();
                    foreach (String errorMessage in errorMessages) {
                        Debug.Assert(errorMessage != null && errorMessage.Length > 0, "Bad Error Messages");
                        arr.Add(errorMessage);
                    }

                    bulletedList.DisplayMode = BulletedListDisplayMode.Text;
                    bulletedList.DataSource = arr;
                    bulletedList.DataBind();

                    RenderChildren(writer);
                }
            }

            if (enteredStyle) {
                writer.ExitStyle(Control.ControlStyle);
            }
        }

        private void WriteSeparator(HtmlTextWriter writer) {
            if (singleParagraph) {
                writer.Write(" ");
            }
            else {
                writer.WriteBreak();
            }
        }
    }
}

#endif
