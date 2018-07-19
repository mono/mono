//------------------------------------------------------------------------------
// <copyright file="WmlImageMapAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Globalization;
    using System.Web.UI.WebControls;
    using System.Web.UI.Adapters;
    using System.Collections.Specialized;
    using System.Web.Util;

    // Adapts the ImageMap for wml.
    public class WmlImageMapAdapter : ImageMapAdapter, IPostBackDataHandler, IPostBackEventHandler {

        protected internal override void OnInit(EventArgs e) {
            // NB: this is required because the control names do not match the controlID
            Page.RegisterRequiresPostBack(Control);
            base.OnInit(e);
        }


        protected internal override void Render(HtmlTextWriter writer) {
            if (Control.HotSpots.Count > 0) {
                HotSpotMode mapMode = Control.HotSpotMode;
                if (mapMode == HotSpotMode.NotSet) {
                    mapMode = HotSpotMode.Navigate;
                }
                HotSpotMode spotMode;
                int hotSpotIndex = 0;
                string targetURL;
                string text;
                foreach (HotSpot item in Control.HotSpots) {
                    text = item.AlternateText;
                    if (text != null && text.Length == 0) {
                        text = item.NavigateUrl;
                    }
                    spotMode = item.HotSpotMode;
                    if (spotMode == HotSpotMode.NotSet) {
                        spotMode = mapMode;
                    }
                    if (spotMode == HotSpotMode.PostBack) {
                        PageAdapter.RenderPostBackEvent(writer, Control.ClientID, hotSpotIndex.ToString(CultureInfo.InvariantCulture),
                                                        null, text);
                    }
                    else if (spotMode == HotSpotMode.Navigate) {
                        targetURL = Control.ResolveClientUrl(item.NavigateUrl);
                        targetURL = Control.GetCountClickUrl(targetURL);
                        PageAdapter.RenderBeginHyperlink(writer, targetURL, true /* encode */, null, Control.AccessKey);
                        writer.Write(text);
                        PageAdapter.RenderEndHyperlink(writer);
                    }
                    else { //HotSpotMode.Inactive
                        writer.Write(LiteralControlAdapterUtility.ProcessWmlLiteralText(text));
                    }
                    ++hotSpotIndex;
                    writer.WriteBreak();
                }
            }
        }

        /// <internalonly/>
        bool IPostBackDataHandler.LoadPostData(String key, NameValueCollection data) {
            return LoadPostData(key, data);
        }

        /// <internalonly/>
        protected virtual bool LoadPostData(String key, NameValueCollection data) {
            return false;
        }

        /// <internalonly/>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }

        /// <internalonly/>
        protected virtual void RaisePostDataChangedEvent() {
        }


        /// <internalonly/>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }

        /// <internalonly/>
        protected virtual void RaisePostBackEvent(string eventArgument) {
            ((IPostBackEventHandler)Control).RaisePostBackEvent(eventArgument);
        }

    }
}

#endif

