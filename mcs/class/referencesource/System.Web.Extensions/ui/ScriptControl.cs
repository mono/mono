//------------------------------------------------------------------------------
// <copyright file="ScriptControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public abstract class ScriptControl : WebControl, IScriptControl {
        private IScriptManagerInternal _scriptManager;
        private new IPage _page;

        protected ScriptControl() {
        }

        internal ScriptControl(IScriptManagerInternal scriptManager, IPage page) {
            _scriptManager = scriptManager;
            _page = page;
        }

        private IPage IPage {
            get {
                if (_page != null) {
                    return _page;
                }
                else {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    return new PageWrapper(page);
                }
            }
        }

        private IScriptManagerInternal ScriptManager {
            get {
                if (_scriptManager == null) {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    _scriptManager = System.Web.UI.ScriptManager.GetCurrent(page);
                    if (_scriptManager == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.Common_ScriptManagerRequired, ID));
                    }
                }
                return _scriptManager;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            ScriptManager.RegisterScriptControl(this);
        }

        protected internal override void Render(HtmlTextWriter writer) {
            base.Render(writer);

            // DevDiv 97460: ScriptDescriptors only render if in server form, verify to avoid silently failing.
            IPage.VerifyRenderingInServerForm(this);

            // ScriptManager cannot be found in DesignMode, so do not attempt to register scripts.
            if (!DesignMode) {
                ScriptManager.RegisterScriptDescriptors(this);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Implementation will likely return a new collection, which is too slow for a property")]
        protected abstract IEnumerable<ScriptDescriptor> GetScriptDescriptors();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Implementation will likely return a new collection, which is too slow for a property")]
        protected abstract IEnumerable<ScriptReference> GetScriptReferences();

        #region IScriptControl Members
        IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors() {
            return GetScriptDescriptors();
        }

        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences() {
            return GetScriptReferences();
        }
        #endregion
    }
}
