//------------------------------------------------------------------------------
// <copyright file="MenuRenderer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    public partial class Menu {
        internal abstract class MenuRenderer {
            protected MenuRenderer(Menu menu) {
                Menu = menu;
            }

            protected Menu Menu { get; private set; }

            public abstract void PreRender(bool registerScript);
            public abstract void RenderBeginTag(HtmlTextWriter writer, bool staticOnly);
            public abstract void RenderContents(HtmlTextWriter writer, bool staticOnly);
            public abstract void RenderEndTag(HtmlTextWriter writer, bool staticOnly);

            public virtual void Render(HtmlTextWriter writer, bool staticOnly) {
                RenderBeginTag(writer, staticOnly);
                RenderContents(writer, staticOnly);
                RenderEndTag(writer, staticOnly);
            }
        }
    }
}
