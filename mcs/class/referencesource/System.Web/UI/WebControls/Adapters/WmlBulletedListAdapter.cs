//------------------------------------------------------------------------------
// <copyright file="WmlBulletedListAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Globalization;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    
    public class WmlBulletedListAdapter : BulletedListAdapter {

        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter)markupWriter;
            writer.EnterStyle(Control.ControlStyle);
            IItemPaginationInfo itemPaginationInfo = (IItemPaginationInfo)Control;
            int firstIndex = itemPaginationInfo.FirstVisibleItemIndex;
            for (int i = firstIndex; i < firstIndex + itemPaginationInfo.VisibleItemCount; i++) {
                RenderBulletText(Control.Items, i, writer);
            }
            writer.ExitStyle(Control.ControlStyle);
        }

        // Writes the text of each bullet according to the list's display mode.
        protected virtual void RenderBulletText (ListItemCollection items, int index, HtmlTextWriter writer) {
            switch (Control.DisplayMode) {
                case BulletedListDisplayMode.Text:
                    writer.WriteEncodedText(items[index].Text);
                    writer.WriteBreak();
                    break;
                case BulletedListDisplayMode.HyperLink:
                    // TODO: if index == 0, set accesskey.  Needs a new RenderBeginHyperlink method.
                    string targetURL = Control.ResolveClientUrl(items[index].Value);
                    if (items[index].Enabled) {
                        PageAdapter.RenderBeginHyperlink(writer, targetURL, true /* encode */, items[index].Text);
                        writer.Write(items[index].Text);
                        PageAdapter.RenderEndHyperlink(writer);
                    } else {
                        writer.WriteEncodedText(items[index].Text);
                    }
                    writer.WriteBreak();
                    break;
                case BulletedListDisplayMode.LinkButton:
                    if (items[index].Enabled) {
                        // TODO: if index == 0, set accesskey.  Needs a new RenderPostBackEvent method.               
                        PageAdapter.RenderPostBackEvent(writer, Control.UniqueID, index.ToString(CultureInfo.InvariantCulture),
                                                            items[index].Text, items[index].Text);
                    } else {
                        writer.WriteEncodedText(items[index].Text);
                    }
                    writer.WriteBreak();
                    break;
                default:
                    Debug.Assert(false, "Invalid BulletedListDisplayMode");
                    break;
            }
        }
    }
}

#endif

