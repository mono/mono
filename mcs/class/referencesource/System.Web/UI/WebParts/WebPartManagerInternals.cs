//------------------------------------------------------------------------------
// <copyright file="WebPartManagerInternals.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    public sealed class WebPartManagerInternals {

        private WebPartManager _manager;

        internal WebPartManagerInternals(WebPartManager manager) {
            _manager = manager;
        }

        public void AddWebPart(WebPart webPart) {
            _manager.AddWebPart(webPart);
        }

        public void CallOnClosing(WebPart webPart) {
            webPart.OnClosing(EventArgs.Empty);
        }

        public void CallOnConnectModeChanged(WebPart webPart) {
            webPart.OnConnectModeChanged(EventArgs.Empty);
        }

        public void CallOnDeleting(WebPart webPart) {
            webPart.OnDeleting(EventArgs.Empty);
        }

        public void CallOnEditModeChanged(WebPart webPart) {
            webPart.OnEditModeChanged(EventArgs.Empty);
        }

        public object CreateObjectFromType(Type type) {
            return WebPartUtil.CreateObjectFromType(type);
        }

        public bool ConnectionDeleted(WebPartConnection connection) {
            return connection.Deleted;
        }

        public void DeleteConnection(WebPartConnection connection) {
            connection.Deleted = true;
        }

        public string GetZoneID(WebPart webPart) {
            return webPart.ZoneID;
        }

        public void LoadConfigurationState(WebPartTransformer transformer, object savedState) {
            transformer.LoadConfigurationState(savedState);
        }

        public void RemoveWebPart(WebPart webPart) {
            _manager.RemoveWebPart(webPart);
        }

        public object SaveConfigurationState(WebPartTransformer transformer) {
            return transformer.SaveConfigurationState();
        }

        public void SetConnectErrorMessage(WebPart webPart, string connectErrorMessage) {
            webPart.SetConnectErrorMessage(connectErrorMessage);
        }

        public void SetHasUserData(WebPart webPart, bool hasUserData) {
            webPart.SetHasUserData(hasUserData);
        }

        public void SetHasSharedData(WebPart webPart, bool hasSharedData) {
            webPart.SetHasSharedData(hasSharedData);
        }

        public void SetIsClosed(WebPart webPart, bool isClosed) {
            webPart.SetIsClosed(isClosed);
        }

        public void SetIsShared(WebPartConnection connection, bool isShared) {
            connection.SetIsShared(isShared);
        }

        public void SetIsShared(WebPart webPart, bool isShared) {
            webPart.SetIsShared(isShared);
        }

        public void SetIsStandalone(WebPart webPart, bool isStandalone) {
            webPart.SetIsStandalone(isStandalone);
        }

        public void SetIsStatic(WebPartConnection connection, bool isStatic) {
            connection.SetIsStatic(isStatic);
        }

        public void SetIsStatic(WebPart webPart, bool isStatic) {
            webPart.SetIsStatic(isStatic);
        }

        public void SetTransformer(WebPartConnection connection, WebPartTransformer transformer) {
            connection.SetTransformer(transformer);
        }

        public void SetZoneID(WebPart webPart, string zoneID) {
            webPart.ZoneID = zoneID;
        }

        public void SetZoneIndex(WebPart webPart, int zoneIndex) {
            webPart.SetZoneIndex(zoneIndex);
        }
    }
}

