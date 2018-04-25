//------------------------------------------------------------------------------
// <copyright file="ImageMap.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    using System.Web;

    /// <devdoc>
    /// <para>ImageMap class.  Provides support for multiple
    /// region-defined actions within an image.</para>
    /// </devdoc>
    [
    DefaultEvent("Click"),
    DefaultProperty("HotSpots"),
    ParseChildren(true, "HotSpots"),
    SupportsEventValidation,
    ]
    public class ImageMap : Image, IPostBackEventHandler {

        private static readonly object EventClick = new object();
        private bool _hasHotSpots;
        private HotSpotCollection _hotSpots;

        [
        Browsable(true),
        EditorBrowsableAttribute(EditorBrowsableState.Always)
        ]
        public override bool Enabled {
            get {
                return base.Enabled;
            }
            set {
                base.Enabled = value;
            }
        }

        /// <devdoc>
        /// <para>Gets the HotSpotCollection with defines the regions of ImageMap hot spots.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        WebSysDescription(SR.ImageMap_HotSpots),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public HotSpotCollection HotSpots {
            get {
                if (_hotSpots == null) {
                    _hotSpots = new HotSpotCollection();
                    if (IsTrackingViewState) {
                        ((IStateManager)_hotSpots).TrackViewState();
                    }
                }
                return _hotSpots;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the HotSpotMode to either postback or navigation.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(HotSpotMode.NotSet),
        WebSysDescription(SR.HotSpot_HotSpotMode),
        ]
        public virtual HotSpotMode HotSpotMode {
            get {
                object obj = ViewState["HotSpotMode"];
                return (obj == null) ? HotSpotMode.NotSet : (HotSpotMode)obj;
            }
            set {
                if (value < HotSpotMode.NotSet || value > HotSpotMode.Inactive) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["HotSpotMode"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the name of the window for navigation.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.HotSpot_Target),
        ]
        public virtual string Target {
            get {
                object value = ViewState["Target"];
                return (value == null)? String.Empty : (string)value;
            }
            set {
                ViewState["Target"] = value;
            }
        }

        /// <devdoc>
        /// <para>The event raised when a hotspot is clicked.</para>
        /// </devdoc>
        [
        Category("Action"),
        WebSysDescription(SR.ImageMap_Click)
        ]
        public event ImageMapEventHandler Click {
            add {
                Events.AddHandler(EventClick, value);
            }
            remove {
                Events.RemoveHandler(EventClick, value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Overridden to add the "usemap" attribute the the image tag.
        /// Overrides WebControl.AddAttributesToRender.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);

            if (_hasHotSpots) {
                writer.AddAttribute(HtmlTextWriterAttribute.Usemap, "#ImageMap" + ClientID, false);
            }
        }

        /// <devdoc>
        /// <para>Restores view-state information that was saved by SaveViewState.
        /// Implements IStateManager.LoadViewState.</para>
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            object baseState = null;
            object[] myState = null;

            if (savedState != null) {
                myState = (object[])savedState;
                if (myState.Length != 2) {
                    throw new ArgumentException(SR.GetString(SR.ViewState_InvalidViewState));
                }

                baseState = myState[0];
            }

            base.LoadViewState(baseState);

            if ((myState != null) && (myState[1] != null)) {
                ((IStateManager)HotSpots).LoadViewState(myState[1]);
            }
        }


        /// <devdoc>
        /// <para>Called when the user clicks the ImageMap.</para>
        /// </devdoc>
        protected virtual void OnClick(ImageMapEventArgs e) {
            ImageMapEventHandler clickHandler = (ImageMapEventHandler)Events[EventClick];
            if (clickHandler != null) {
                clickHandler(this, e);
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Sends server control content to a provided HtmlTextWriter, which writes the content
        /// to be rendered to the client.
        /// Overrides Control.Render.</para>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            if (Enabled && !IsEnabled && SupportsDisabledAttribute) {
                // We need to do the cascade effect on the server, because the browser
                // only renders as disabled, but doesn't disable the functionality.
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            _hasHotSpots = ((_hotSpots != null) && (_hotSpots.Count > 0));

            base.Render(writer);

            if (_hasHotSpots) {
                string fullClientID = "ImageMap" + ClientID;
                writer.AddAttribute(HtmlTextWriterAttribute.Name, fullClientID);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, fullClientID);
                writer.RenderBeginTag(HtmlTextWriterTag.Map);

                HotSpotMode mapMode = HotSpotMode;
                if (mapMode == HotSpotMode.NotSet) {
                    mapMode = HotSpotMode.Navigate;
                }
                HotSpotMode spotMode;
                int hotSpotIndex = 0;
                string controlTarget = Target;
                foreach (HotSpot item in _hotSpots) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Shape, item.MarkupName, false);
                    writer.AddAttribute(HtmlTextWriterAttribute.Coords, item.GetCoordinates());
                    spotMode = item.HotSpotMode;
                    if (spotMode == HotSpotMode.NotSet) {
                        spotMode = mapMode;
                    }
                    if (spotMode == HotSpotMode.PostBack) {
                        // Make sure the page has a server side form if we are posting back
                        if (Page != null) {
                            Page.VerifyRenderingInServerForm(this);
                        }
                        if ((RenderingCompatibility < VersionUtil.Framework40) || IsEnabled) {
                            string eventArgument = hotSpotIndex.ToString(CultureInfo.InvariantCulture);
                            writer.AddAttribute(HtmlTextWriterAttribute.Href,
                                Page.ClientScript.GetPostBackClientHyperlink(this, eventArgument, true));
                        }
                    }
                    else if (spotMode == HotSpotMode.Navigate) {
                        if ((RenderingCompatibility < VersionUtil.Framework40) || IsEnabled) {
                            String resolvedUrl = ResolveClientUrl(item.NavigateUrl);
                            writer.AddAttribute(HtmlTextWriterAttribute.Href, resolvedUrl);
                        }
                        // Use HotSpot target first, if not specified, use ImageMap's target
                        string target = item.Target;
                        if (target.Length == 0) target = controlTarget;
                        if (target.Length > 0) writer.AddAttribute(HtmlTextWriterAttribute.Target, target);
                    }
                    else if (spotMode == HotSpotMode.Inactive) {
                        writer.AddAttribute("nohref", "true");
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, item.AlternateText);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, item.AlternateText);
                    string s = item.AccessKey;
                    if (s.Length > 0) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, s);
                    }
                    int n = item.TabIndex;
                    if (n != 0) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, n.ToString(NumberFormatInfo.InvariantInfo));
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Area);
                    writer.RenderEndTag();
                    ++hotSpotIndex;
                }
                writer.RenderEndTag();  // Map
            }
        }


        /// <devdoc>
        /// <para>Saves any server control view-state changes that have
        /// occurred since the time the page was posted back to the server.
        /// Implements IStateManager.SaveViewState.</para>
        /// </devdoc>
        protected override object SaveViewState() {
            object baseState = base.SaveViewState();
            object hotSpotsState = null;

            if ((_hotSpots != null) && (_hotSpots.Count > 0)) {
                hotSpotsState = ((IStateManager)_hotSpots).SaveViewState();
            }

            if ((baseState != null) || (hotSpotsState != null)) {
                object[] savedState = new object[2];
                savedState[0] = baseState;
                savedState[1] = hotSpotsState;

                return savedState;
            }

            return null;
        }


        /// <devdoc>
        /// <para>Causes the tracking of view-state changes to the server control.
        /// Implements IStateManager.TrackViewState.</para>
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();
            if (_hotSpots != null) {
                ((IStateManager)_hotSpots).TrackViewState();
            }
        }

        #region Implementation of IPostBackEventHandler

        /// <internalonly/>
        /// <devdoc>
        /// <para>Notifies the server control that caused the postback that
        /// it should handle an incoming post back event.
        /// Implements IPostBackEventHandler.</para>
        /// </devdoc>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Notifies the server control that caused the postback that
        /// it should handle an incoming post back event.
        /// Implements IPostBackEventHandler.</para>
        /// </devdoc>
        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(UniqueID, eventArgument);

            string postBackValue = null;
            if (eventArgument != null && _hotSpots != null) {
                int hotSpotIndex = Int32.Parse(eventArgument, CultureInfo.InvariantCulture);

                if (hotSpotIndex >= 0 && hotSpotIndex < _hotSpots.Count) {
                    HotSpot hotSpot = _hotSpots[hotSpotIndex];
                    HotSpotMode mode = hotSpot.HotSpotMode;
                    if (mode == HotSpotMode.NotSet) {
                        mode = HotSpotMode;
                    }
                    if (mode == HotSpotMode.PostBack) {
                        postBackValue = hotSpot.PostBackValue;
                    }
                }
            }
            // Ignore invalid indexes silently(VSWhidbey 185738)
            if (postBackValue != null) {
                OnClick(new ImageMapEventArgs(postBackValue));
            }
        }
        #endregion
    }
}
