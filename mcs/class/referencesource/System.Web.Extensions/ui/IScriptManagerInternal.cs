//------------------------------------------------------------------------------
// <copyright file="IScriptManagerInternal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Reflection;
    using System.Web.UI;

    internal interface IScriptManagerInternal {
        string AsyncPostBackSourceElementID {
            get;
        }

        bool SupportsPartialRendering {
            get;
        }

        bool IsInAsyncPostBack {
            get;
        }

        void RegisterAsyncPostBackControl(Control control);
        void RegisterExtenderControl<TExtenderControl>(TExtenderControl extenderControl, Control targetControl)
            where TExtenderControl : Control, IExtenderControl;
        void RegisterPostBackControl(Control control);
        void RegisterProxy(ScriptManagerProxy proxy);
        void RegisterScriptControl<TScriptControl>(TScriptControl scriptControl)
            where TScriptControl : Control, IScriptControl;
        void RegisterScriptDescriptors(IExtenderControl extenderControl);
        void RegisterScriptDescriptors(IScriptControl scriptControl);
        void RegisterUpdatePanel(UpdatePanel updatePanel);
        void UnregisterUpdatePanel(UpdatePanel updatePanel);
    }
}
