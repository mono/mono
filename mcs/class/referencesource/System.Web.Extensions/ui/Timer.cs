//------------------------------------------------------------------------------
// <copyright file="Timer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;
    using System.Web.Resources;

    [
    DefaultEvent("Tick"),
    DefaultProperty("Interval"),
    Designer("System.Web.UI.Design.TimerDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
    NonVisualControl,
    ToolboxBitmap(typeof(EmbeddedResourceFinder), "System.Web.Resources.Timer.bmp"),
    SupportsEventValidation
    ]
    public class Timer : Control, IPostBackEventHandler, IScriptControl {
        private static readonly object TickEventKey = new object();
        private bool _stateDirty;
        private new IPage _page;
        private ScriptManager _scriptManager;

        public Timer() {}

        private IPage IPage {
            get {
                if (null == _page) {
                    Page page = Page;
                    if (null == page) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    _page = new PageWrapper(page);
                }
                return _page;
            }
        }

        [
        ResourceDescription("Timer_TimerEnable"),
        Category("Behavior"),
        DefaultValue(true)
        ]
        public bool Enabled {
            get {
                object o = ViewState["Enabled"];
                return (o != null) ? (bool)o : true;
            }
            set {
                if (!_stateDirty && IsTrackingViewState) {
                    object o = ViewState["Enabled"];
                    _stateDirty = (null == o) ? true : (value != (bool)o);
                }
                ViewState["Enabled"] = value;
            }
        }

        [
        ResourceDescription("Timer_TimerInterval"),
        Category("Behavior"),
        DefaultValue(60000)
        ]
        public int Interval {
            get {
                object o = ViewState["Interval"];
                return (o != null) ? (int)o : 60000;
            }
            set {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException("value", AtlasWeb.Timer_IntervalMustBeGreaterThanZero);
                }
                if (!_stateDirty && IsTrackingViewState) {
                    object o = ViewState["Interval"];
                    _stateDirty = (null == o) ? true : (value != (int)o);
                }
                ViewState["Interval"] = value;
            }
        }

        internal ScriptManager ScriptManager {
            get {
                if (_scriptManager == null) {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    _scriptManager = ScriptManager.GetCurrent(page);
                    if (_scriptManager == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.Common_ScriptManagerRequired, ID));
                    }
                }
                return _scriptManager;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool Visible {
            get {
                return base.Visible;
            }
            set {
                throw new NotImplementedException();
            }
        }

        [
        ResourceDescription("Timer_TimerTick"),
        Category("Action")
        ]
        public event EventHandler<EventArgs> Tick {
            add {
                Events.AddHandler(TickEventKey, value);
            }
            remove {
                Events.RemoveHandler(TickEventKey, value);
            }
        }

        private string GetJsonState() {
            return "[" + ((Enabled) ? "true" : "false") + "," + Interval.ToString(CultureInfo.InvariantCulture) + "]";
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Matches IScriptControl interface.")]
        protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors() {
            ScriptComponentDescriptor s = new ScriptControlDescriptor("Sys.UI._Timer", this.ClientID);
            s.AddProperty("interval", Interval);
            s.AddProperty("enabled", Enabled);
            s.AddProperty("uniqueID",this.UniqueID);
            yield return s;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Matches IScriptControl interface.")]
        protected virtual IEnumerable<ScriptReference> GetScriptReferences() {
            yield return new ScriptReference("MicrosoftAjaxTimer.js", Assembly.GetAssembly(typeof(Timer)).FullName) ;
        }

        #region IPostBackEventHandler Members
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion

        #region IScriptControl Members

        IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors() {
            return GetScriptDescriptors();

        }

        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences() {
            return GetScriptReferences();
        }
        #endregion

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            this.ScriptManager.RegisterScriptControl(this);

            if (_stateDirty && this.ScriptManager.IsInAsyncPostBack) {
                _stateDirty = false;
                this.ScriptManager.RegisterDataItem(this, GetJsonState(), true);
            }
            // Get a postback event reference to ensure that the postback script is generated
            // and to make sure this is an expected name/value pair from an event validation
            // perspective.

            IPage.ClientScript.GetPostBackEventReference(new PostBackOptions(this, String.Empty));
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected virtual void OnTick(EventArgs e) {
            EventHandler<EventArgs> handler = (EventHandler<EventArgs>)Events[TickEventKey];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
            Justification = "Matches IPostBackEventHandler interface.")]
        protected virtual void RaisePostBackEvent(string eventArgument) {
            if (Enabled) {
                OnTick(EventArgs.Empty);
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            // Make sure we are in a form tag with runat=server.
            IPage.VerifyRenderingInServerForm(this);

            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.RenderEndTag(); // Span

            if (!DesignMode) {
                ScriptManager.RegisterScriptDescriptors(this);
            }
        }
    }
}
