//------------------------------------------------------------------------------
// <copyright file="ConnectionsZone.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    [
    Designer("System.Web.UI.Design.WebControls.WebParts.ConnectionsZoneDesigner, " + AssemblyRef.SystemDesign),
    SupportsEventValidation,
    ]
    public class ConnectionsZone : ToolZone {

        private const int baseIndex = 0;
        private const int cancelVerbIndex = 1;
        private const int closeVerbIndex = 2;
        private const int configureVerbIndex = 3;
        private const int connectVerbIndex = 4;
        private const int disconnectVerbIndex = 5;
        private const int viewStateArrayLength = 6;

        private const int modeIndex = 1;
        private const int pendingConnectionPointIDIndex = 2;
        private const int pendingConnectionTypeIndex = 3;
        private const int pendingSelectedValueIndex = 4;
        private const int pendingConsumerIDIndex = 5;
        private const int pendingTransformerTypeNameIndex = 6;
        private const int pendingConnectionIDIndex = 7;
        private const int controlStateArrayLength = 8;

        private WebPartVerb _closeVerb;
        private WebPartVerb _connectVerb;
        private WebPartVerb _disconnectVerb;
        private WebPartVerb _configureVerb;
        private WebPartVerb _cancelVerb;

        private const string connectEventArgument = "connect";
        private const string connectConsumerEventArgument = "connectconsumer";
        private const string connectProviderEventArgument = "connectprovider";
        private const string providerEventArgument = "provider";
        private const string consumerEventArgument = "consumer";
        private const string disconnectEventArgument = "disconnect";
        private const string configureEventArgument = "edit";
        private const string closeEventArgument = "close";
        private const string cancelEventArgument = "cancel";
        private const string providerListIdPrefix = "_providerlist_";
        private const string consumerListIdPrefix = "_consumerlist_";

        // Maps connection points to DropDownLists
        private IDictionary _connectDropDownLists;

        private ArrayList _availableTransformers;

        private WebPartTransformer _pendingTransformer;
        private Control _pendingTransformerConfigurationControl;

        // Error message string. Should not persist as it makes sense for the current request only
        private bool _displayErrorMessage;

        // Currently edited connection data.
        // Only set by EnsurePendingData, which is called from many places.
        private WebPart _pendingConsumer;
        private WebPart _pendingProvider;
        private ConsumerConnectionPoint _pendingConsumerConnectionPoint;
        private ProviderConnectionPoint _pendingProviderConnectionPoint;

        // Maps connection points to IList of ConsumerInfo or ProviderInfo objects
        private IDictionary _connectionPointInfo;

        // ControlState variables:
        //   The current mode of the connections zone
        private ConnectionsZoneMode _mode;
        //   If we currently have a pending connection (we are displaying the transformer
        //   configuration control), this will contain the ID of the connection point on the
        //   WebPartToConnect for the pending connection.
        private string _pendingConnectionPointID;
        //   If we don't have a pending connection, returns None.
        //   If we do have a pending connection, returns Consumer or Provider.
        private ConnectionType _pendingConnectionType;
        //   If we currently have a pending connection (we are displaying the transformer
        //   configuration control), this will contain the selected value of the DropdownList
        //   associated with the pending connection point of the WebPartToConnect.  If there
        //   is no pending connection, returns null.
        private string _pendingSelectedValue;
        //   If we currently have a pending connection reconfiguration (we are displaying the transformer
        //   configuration wizard), this will contain the ID of the consumer web part ID.
        private string _pendingConsumerID;
        //   Saving the pending transformer configuration control's type, just in case it changes between postbacks
        private string _pendingTransformerConfigurationControlTypeName;
        //   Saving the pending connection when editing an existing connection.
        private string _pendingConnectionID;

        public ConnectionsZone() : base(WebPartManager.ConnectDisplayMode) {
            _mode = ConnectionsZoneMode.ExistingConnections;
            _pendingConnectionPointID = string.Empty;
            _pendingConnectionType = ConnectionType.None;
            _pendingSelectedValue = null;
            _pendingConsumerID = string.Empty;
        }

        private ArrayList AvailableTransformers {
            get {
                if (_availableTransformers == null) {
                    _availableTransformers = new ArrayList();
                    TransformerTypeCollection availableTransformerTypes = WebPartManager.AvailableTransformers;
                    foreach (Type type in availableTransformerTypes) {
                        _availableTransformers.Add(WebPartUtil.CreateObjectFromType(type));
                    }
                }
                return _availableTransformers;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.ConnectionsZone_CancelVerb),
        ]
        public virtual WebPartVerb CancelVerb {
            get {
                if (_cancelVerb == null) {
                    _cancelVerb = new WebPartConnectionsCancelVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_cancelVerb).TrackViewState();
                    }
                }

                return _cancelVerb;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.ConnectionsZone_CloseVerb),
        ]
        public virtual WebPartVerb CloseVerb {
            get {
                if (_closeVerb == null) {
                    _closeVerb = new WebPartConnectionsCloseVerb();
                    _closeVerb.EventArgument = closeEventArgument;
                    if (IsTrackingViewState) {
                        ((IStateManager)_closeVerb).TrackViewState();
                    }
                }

                return _closeVerb;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ConfigureConnectionTitleDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ConfigureConnectionTitle),
        ]
        public virtual string ConfigureConnectionTitle {
            get {
                string s = (string)ViewState["ConfigureConnectionTitle"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ConfigureConnectionTitle) : s);
            }
            set {
                ViewState["ConfigureConnectionTitle"] = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.ConnectionsZone_ConfigureVerb),
        ]
        public virtual WebPartVerb ConfigureVerb {
            get {
                if (_configureVerb == null) {
                    _configureVerb = new WebPartConnectionsConfigureVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_configureVerb).TrackViewState();
                    }
                }

                return _configureVerb;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ConnectToConsumerInstructionTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ConnectToConsumerInstructionText),
        ]
        public virtual string ConnectToConsumerInstructionText {
            get {
                string s = (string)ViewState["ConnectToConsumerInstructionText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ConnectToConsumerInstructionText) : s);
            }
            set {
                ViewState["ConnectToConsumerInstructionText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ConnectToConsumerTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ConnectToConsumerText),
        ]
        public virtual string ConnectToConsumerText {
            get {
                string s = (string)ViewState["ConnectToConsumerText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ConnectToConsumerText) : s);
            }
            set {
                ViewState["ConnectToConsumerText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ConnectToConsumerTitleDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ConnectToConsumerTitle),
        ]
        public virtual string ConnectToConsumerTitle {
            get {
                string s = (string)ViewState["ConnectToConsumerTitle"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ConnectToConsumerTitle) : s);
            }
            set {
                ViewState["ConnectToConsumerTitle"] = value;
            }
        }


        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ConnectToProviderInstructionTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ConnectToProviderInstructionText),
        ]
        public virtual string ConnectToProviderInstructionText {
            get {
                string s = (string)ViewState["ConnectToProviderInstructionText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ConnectToProviderInstructionText) : s);
            }
            set {
                ViewState["ConnectToProviderInstructionText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ConnectToProviderTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ConnectToProviderText),
        ]
        public virtual string ConnectToProviderText {
            get {
                string s = (string)ViewState["ConnectToProviderText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ConnectToProviderText) : s);
            }
            set {
                ViewState["ConnectToProviderText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ConnectToProviderTitleDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ConnectToProviderTitle),
        ]
        public virtual string ConnectToProviderTitle {
            get {
                string s = (string)ViewState["ConnectToProviderTitle"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ConnectToProviderTitle) : s);
            }
            set {
                ViewState["ConnectToProviderTitle"] = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.ConnectionsZone_ConnectVerb),
        ]
        public virtual WebPartVerb ConnectVerb {
            get {
                if (_connectVerb == null) {
                    _connectVerb = new WebPartConnectionsConnectVerb();
                    _connectVerb.EventArgument = connectEventArgument;
                    if (IsTrackingViewState) {
                        ((IStateManager)_connectVerb).TrackViewState();
                    }
                }

                return _connectVerb;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ConsumersTitleDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ConsumersTitle),
        ]
        public virtual string ConsumersTitle {
            get {
                string s = (string)ViewState["ConsumersTitle"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ConsumersTitle) : s);
            }
            set {
                ViewState["ConsumersTitle"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ConsumersInstructionTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ConsumersInstructionText),
        ]
        public virtual string ConsumersInstructionText {
            get {
                string s = (string)ViewState["ConsumersInstructionText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ConsumersInstructionText) : s);
            }
            set {
                ViewState["ConsumersInstructionText"] = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.ConnectionsZone_DisconnectVerb),
        ]
        public virtual WebPartVerb DisconnectVerb {
            get {
                if (_disconnectVerb == null) {
                    _disconnectVerb = new WebPartConnectionsDisconnectVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_disconnectVerb).TrackViewState();
                    }
                }

                return _disconnectVerb;
            }
        }

        protected override bool Display {
            get {
                return (base.Display && WebPartToConnect != null);
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        Themeable(false)
        ]
        public override string EmptyZoneText {
            get {
                return base.EmptyZoneText;
            }
            set {
                base.EmptyZoneText = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_WarningMessage),
        WebSysDefaultValue(SR.ConnectionsZone_WarningConnectionDisabled),
        ]
        public virtual string ExistingConnectionErrorMessage {
            get {
                string s = (string)ViewState["ExistingConnectionErrorMessage"];
                return s == null ? SR.GetString(SR.ConnectionsZone_WarningConnectionDisabled) : s;
            }
            set {
                ViewState["ExistingConnectionErrorMessage"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_GetDescription),
        WebSysDefaultValue(SR.ConnectionsZone_Get),
        ]
        public virtual string GetText {
            get {
                string s = (string)ViewState["GetText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_Get) : s);
            }
            set {
                ViewState["GetText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_GetFromTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_GetFromText),
        ]
        public virtual string GetFromText {
            get {
                string s = (string)ViewState["GetFromText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_GetFromText) : s);
            }
            set {
                ViewState["GetFromText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_HeaderTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_HeaderText),
        ]
        public override string HeaderText {
            get {
                string s = (string)ViewState["HeaderText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_HeaderText) : s);
            }
            set {
                ViewState["HeaderText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_InstructionTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_InstructionText),
        ]
        public override string InstructionText {
            get {
                string s = (string)ViewState["InstructionText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_InstructionText) : s);
            }
            set {
                ViewState["InstructionText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_InstructionTitleDescription),
        WebSysDefaultValue(SR.ConnectionsZone_InstructionTitle),
        ]
        public virtual string InstructionTitle {
            get {
                string s = (string)ViewState["InstructionTitle"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_InstructionTitle) : s);
            }
            set {
                ViewState["InstructionTitle"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ErrorMessage),
        WebSysDefaultValue(SR.ConnectionsZone_ErrorCantContinueConnectionCreation),
        ]
        public virtual string NewConnectionErrorMessage {
            get {
                string s = (string)ViewState["NewConnectionErrorMessage"];
                return s == null ? SR.GetString(SR.ConnectionsZone_ErrorCantContinueConnectionCreation) : s;
            }
            set {
                ViewState["NewConnectionErrorMessage"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_NoExistingConnectionInstructionTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_NoExistingConnectionInstructionText),
        ]
        public virtual string NoExistingConnectionInstructionText {
            get {
                string s = (string)ViewState["NoExistingConnectionInstructionText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_NoExistingConnectionInstructionText) : s);
            }
            set {
                ViewState["NoExistingConnectionInstructionText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_NoExistingConnectionTitleDescription),
        WebSysDefaultValue(SR.ConnectionsZone_NoExistingConnectionTitle),
        ]
        public virtual string NoExistingConnectionTitle {
            get {
                string s = (string)ViewState["NoExistingConnectionTitle"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_NoExistingConnectionTitle) : s);
            }
            set {
                ViewState["NoExistingConnectionTitle"] = value;
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        Themeable(false),
        ]
        public override PartChromeType PartChromeType {
            get {
                return base.PartChromeType;
            }
            set {
                base.PartChromeType = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ProvidersTitleDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ProvidersTitle),
        ]
        public virtual string ProvidersTitle {
            get {
                string s = (string)ViewState["ProvidersTitle"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ProvidersTitle) : s);
            }
            set {
                ViewState["ProvidersTitle"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_ProvidersInstructionTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_ProvidersInstructionText),
        ]
        public virtual string ProvidersInstructionText {
            get {
                string s = (string)ViewState["ProvidersInstructionText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_ProvidersInstructionText) : s);
            }
            set {
                ViewState["ProvidersInstructionText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_SendTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_SendText),
        ]
        public virtual string SendText {
            get {
                string s = (string)ViewState["SendText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_SendText) : s);
            }
            set {
                ViewState["SendText"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        WebSysDescription(SR.ConnectionsZone_SendToTextDescription),
        WebSysDefaultValue(SR.ConnectionsZone_SendToText),
        ]
        public virtual string SendToText {
            get {
                string s = (string)ViewState["SendToText"];
                return((s == null) ? SR.GetString(SR.ConnectionsZone_SendToText) : s);
            }
            set {
                ViewState["SendToText"] = value;
            }
        }

        protected WebPart WebPartToConnect {
            get {
                if (WebPartManager != null && WebPartManager.DisplayMode == WebPartManager.ConnectDisplayMode) {
                    return WebPartManager.SelectedWebPart;
                }
                else {
                    return null;
                }
            }
        }

        protected override void Close() {
            if (WebPartToConnect != null) {
                WebPartManager.EndWebPartConnecting();
            }
        }

        private void ClearPendingConnection() {
            _pendingConnectionType = ConnectionType.None;
            _pendingConnectionPointID = string.Empty;
            _pendingSelectedValue = null;
            _pendingConsumerID = string.Empty;
            _pendingConsumer = null;
            _pendingConsumerConnectionPoint = null;
            _pendingProvider = null;
            _pendingProviderConnectionPoint = null;
            _pendingTransformerConfigurationControlTypeName = null;
            _pendingConnectionID = null;
        }

        private void ConnectConsumer(string consumerConnectionPointID) {
            // We don't need to check for AllowConnect on the parts because we're already checking
            // that the data was in the drop-downs in the first place, and these check for AllowConnect.
            WebPart consumer = WebPartToConnect;

            if (consumer == null || consumer.IsClosed) {
                DisplayConnectionError();
                return;
            }

            ConsumerConnectionPoint consumerConnectionPoint =
                WebPartManager.GetConsumerConnectionPoint(consumer, consumerConnectionPointID);

            if (consumerConnectionPoint == null) {
                DisplayConnectionError();
                return;
            }

            EnsureChildControls();

            if (_connectDropDownLists == null ||
                !_connectDropDownLists.Contains(consumerConnectionPoint) ||
                _connectionPointInfo == null ||
                !_connectionPointInfo.Contains(consumerConnectionPoint)) {

                DisplayConnectionError();
                return;
            }

            DropDownList list = (DropDownList)_connectDropDownLists[consumerConnectionPoint];
            // Using Request instead of the control's selected value because some concurrency
            // conditions exist under which the selected item does not exist any more in the list.
            // In this case, we want to display a connection error (VSWhidbey 368543)
            string selectedValue = Page.Request.Form[list.UniqueID];
            if (!String.IsNullOrEmpty(selectedValue)) {
                IDictionary providers = (IDictionary)_connectionPointInfo[consumerConnectionPoint];

                if (providers == null || !providers.Contains(selectedValue)) {
                    DisplayConnectionError();
                    return;
                }

                ProviderInfo provider = (ProviderInfo)providers[selectedValue];
                Type transformerType = provider.TransformerType;
                if (transformerType != null) {
                    Debug.Assert(transformerType.IsSubclassOf(typeof(WebPartTransformer)));
                    WebPartTransformer transformer =
                        (WebPartTransformer)WebPartUtil.CreateObjectFromType(transformerType);
                    if (GetConfigurationControl(transformer) == null) {
                        if (WebPartManager.CanConnectWebParts(provider.WebPart, provider.ConnectionPoint,
                                                                consumer, consumerConnectionPoint, transformer)) {
                            WebPartManager.ConnectWebParts(provider.WebPart, provider.ConnectionPoint,
                                                            consumer, consumerConnectionPoint, transformer);
                        }
                        else {
                            DisplayConnectionError();
                        }
                        Reset();
                    }
                    else {
                        // Control will be created and added on next call to CreateChildControls
                        _pendingConnectionType = ConnectionType.Consumer;
                        _pendingConnectionPointID = consumerConnectionPointID;
                        _pendingSelectedValue = selectedValue;
                        _mode = ConnectionsZoneMode.ConfiguringTransformer;
                        ChildControlsCreated = false;
                    }
                }
                else {
                    if (WebPartManager.CanConnectWebParts(provider.WebPart, provider.ConnectionPoint,
                                                            consumer, consumerConnectionPoint)) {
                        WebPartManager.ConnectWebParts(provider.WebPart, provider.ConnectionPoint,
                                                        consumer, consumerConnectionPoint);
                    }
                    else {
                        DisplayConnectionError();
                    }
                    Reset();
                }
                // Reset the list to the blank selection
                list.SelectedValue = null;
            }
        }

        private void ConnectProvider(string providerConnectionPointID) {
            // We don't need to check for AllowConnect on the parts because we're already checking
            // that the data was in the drop-downs in the first place, and these check for AllowConnect.
            WebPart provider = WebPartToConnect;

            if (provider == null || provider.IsClosed) {
                DisplayConnectionError();
                return;
            }

            ProviderConnectionPoint providerConnectionPoint =
                WebPartManager.GetProviderConnectionPoint(provider, providerConnectionPointID);

            if (providerConnectionPoint == null) {
                DisplayConnectionError();
                return;
            }

            EnsureChildControls();

            if (_connectDropDownLists == null ||
                !_connectDropDownLists.Contains(providerConnectionPoint) ||
                _connectionPointInfo == null ||
                !_connectionPointInfo.Contains(providerConnectionPoint)) {

                DisplayConnectionError();
                return;
            }

            DropDownList list = (DropDownList)_connectDropDownLists[providerConnectionPoint];
            // Using Request instead of the control's selected value because some concurrency
            // conditions exist under which the selected item does not exist any more in the list.
            // In this case, we want to display a connection error (VSWhidbey 368543)
            string selectedValue = Page.Request.Form[list.UniqueID];
            if (!String.IsNullOrEmpty(selectedValue)) {
                IDictionary consumers = (IDictionary)_connectionPointInfo[providerConnectionPoint];

                if (consumers == null || !consumers.Contains(selectedValue)) {
                    DisplayConnectionError();
                    return;
                }

                ConsumerInfo consumer = (ConsumerInfo)consumers[selectedValue];
                Type transformerType = consumer.TransformerType;
                if (transformerType != null) {
                    Debug.Assert(transformerType.IsSubclassOf(typeof(WebPartTransformer)));
                    WebPartTransformer transformer =
                        (WebPartTransformer)WebPartUtil.CreateObjectFromType(transformerType);
                    if (GetConfigurationControl(transformer) == null) {
                        if (WebPartManager.CanConnectWebParts(provider, providerConnectionPoint,
                                                                consumer.WebPart, consumer.ConnectionPoint, transformer)) {
                            WebPartManager.ConnectWebParts(provider, providerConnectionPoint,
                                                            consumer.WebPart, consumer.ConnectionPoint, transformer);
                        }
                        else {
                            DisplayConnectionError();
                        }
                        Reset();
                    }
                    else {
                        // Control will be created and added on next call to CreateChildControls
                        _pendingConnectionType = ConnectionType.Provider;
                        _pendingConnectionPointID = providerConnectionPointID;
                        _pendingSelectedValue = selectedValue;
                        _mode = ConnectionsZoneMode.ConfiguringTransformer;
                        ChildControlsCreated = false;
                    }
                }
                else {
                    if (WebPartManager.CanConnectWebParts(provider, providerConnectionPoint,
                                                            consumer.WebPart, consumer.ConnectionPoint)) {
                        WebPartManager.ConnectWebParts(provider, providerConnectionPoint,
                                                        consumer.WebPart, consumer.ConnectionPoint);
                    }
                    else {
                        DisplayConnectionError();
                    }
                    Reset();
                }
                // Reset the list to the blank selection
                list.SelectedValue = null;
            }
        }

        protected internal override void CreateChildControls() {
            Controls.Clear();
            _connectDropDownLists = new HybridDictionary();
            _connectionPointInfo = new HybridDictionary();
            _pendingTransformerConfigurationControl = null;

            WebPart webPartToConnect = WebPartToConnect;
            if (webPartToConnect != null && !webPartToConnect.IsClosed) {
                WebPartManager webPartManager = WebPartManager;
                Debug.Assert(webPartManager != null);

                ProviderConnectionPointCollection providerConnectionPoints =
                    WebPartManager.GetEnabledProviderConnectionPoints(webPartToConnect);
                foreach (ProviderConnectionPoint providerConnectionPoint in providerConnectionPoints) {
                    DropDownList list = new DropDownList();
                    list.ID = providerListIdPrefix + providerConnectionPoint.ID;
                    // Don't want to track changes to Items collection in ViewState
                    list.EnableViewState = false;
                    _connectDropDownLists[providerConnectionPoint] = list;
                    Controls.Add(list);
                }

                ConsumerConnectionPointCollection consumerConnectionPoints =
                    WebPartManager.GetEnabledConsumerConnectionPoints(webPartToConnect);
                foreach (ConsumerConnectionPoint consumerConnectionPoint in consumerConnectionPoints) {
                    DropDownList list = new DropDownList();
                    list.ID = consumerListIdPrefix + consumerConnectionPoint.ID;
                    // Don't want to track changes to Items collection in ViewState
                    list.EnableViewState = false;
                    _connectDropDownLists[consumerConnectionPoint] = list;
                    Controls.Add(list);
                }

                SetDropDownProperties();

                // Handle pending connection
                if (_pendingConnectionType == ConnectionType.Consumer) {
                    if (EnsurePendingData()) {
                        Control pendingProviderControl = _pendingProvider.ToControl();
                        Control pendingConsumerControl = _pendingConsumer.ToControl();

                        if (_pendingSelectedValue != null) {
                            IDictionary providers = (IDictionary)_connectionPointInfo[_pendingConsumerConnectionPoint];
                            ProviderInfo providerInfo = (ProviderInfo)providers[_pendingSelectedValue];

                            Debug.Assert(providerInfo != null && providerInfo.TransformerType != null && providerInfo.TransformerType.IsSubclassOf(typeof(WebPartTransformer)));
                            _pendingTransformer = (WebPartTransformer)WebPartUtil.CreateObjectFromType(
                                providerInfo.TransformerType);
                        }
                        // Otherwise, we're updating an existing connection and _pendingTransformer has been set by EnsurePendingData.


                        _pendingTransformerConfigurationControl = GetConfigurationControl(_pendingTransformer);
                        if(_pendingTransformerConfigurationControl != null) {

                            ((ITransformerConfigurationControl)_pendingTransformerConfigurationControl).Cancelled +=
                                new EventHandler(OnConfigurationControlCancelled);
                            ((ITransformerConfigurationControl)_pendingTransformerConfigurationControl).Succeeded +=
                                new EventHandler(OnConfigurationControlSucceeded);

                            Controls.Add(_pendingTransformerConfigurationControl);
                        }
                    }
                }
                else if (_pendingConnectionType == ConnectionType.Provider) {
                    if (EnsurePendingData()) {
                        Control pendingProviderControl = _pendingProvider.ToControl();
                        Control pendingConsumerControl = _pendingConsumer.ToControl();

                        ConsumerInfo consumerInfo;
                        Debug.Assert(_pendingSelectedValue != null);

                        IDictionary consumers = (IDictionary)_connectionPointInfo[_pendingProviderConnectionPoint];
                        consumerInfo = (ConsumerInfo)consumers[_pendingSelectedValue];
                        Debug.Assert(consumerInfo != null && consumerInfo.TransformerType != null && consumerInfo.TransformerType.IsSubclassOf(typeof(WebPartTransformer)));
                        _pendingTransformer = (WebPartTransformer)WebPartUtil.CreateObjectFromType(
                            consumerInfo.TransformerType);

                        _pendingTransformerConfigurationControl = GetConfigurationControl(_pendingTransformer);
                        if (_pendingTransformerConfigurationControl != null) {

                            ((ITransformerConfigurationControl)_pendingTransformerConfigurationControl).Cancelled +=
                                new EventHandler(OnConfigurationControlCancelled);
                            ((ITransformerConfigurationControl)_pendingTransformerConfigurationControl).Succeeded +=
                                new EventHandler(OnConfigurationControlSucceeded);

                            Controls.Add(_pendingTransformerConfigurationControl);
                        }
                    }
                }

                SetTransformerConfigurationControlProperties();
            }
        }

        private bool EnsurePendingData() {
            if (WebPartToConnect == null) {
                ClearPendingConnection();
                _mode = ConnectionsZoneMode.ExistingConnections;
                return false;
            }

            if ((_pendingConsumer != null) &&
                (_pendingConsumerConnectionPoint == null ||
                _pendingProvider == null ||
                _pendingProviderConnectionPoint == null)) {

                DisplayConnectionError();
                return false;
            }

            if (_pendingConnectionType == ConnectionType.Provider) {
                Debug.Assert(_pendingSelectedValue != null);

                _pendingProvider = WebPartToConnect;
                _pendingProviderConnectionPoint =
                    WebPartManager.GetProviderConnectionPoint(WebPartToConnect, _pendingConnectionPointID);

                if (_pendingProviderConnectionPoint == null) {
                    DisplayConnectionError();
                    return false;
                }

                IDictionary consumers = (IDictionary)_connectionPointInfo[_pendingProviderConnectionPoint];
                ConsumerInfo consumerInfo = null;
                if (consumers != null) {
                    consumerInfo = (ConsumerInfo)consumers[_pendingSelectedValue];
                }

                if (consumerInfo == null) {
                    DisplayConnectionError();
                    return false;
                }

                _pendingConsumer = consumerInfo.WebPart;
                _pendingConsumerConnectionPoint = consumerInfo.ConnectionPoint;

                return true;
            }

            string consumerID = _pendingConsumerID;
            if (_pendingConnectionType == ConnectionType.Consumer) {
                if (!String.IsNullOrEmpty(_pendingConnectionID)) {
                    // Editing an existing connection
                    WebPartConnection connection = WebPartManager.Connections[_pendingConnectionID];
                    if (connection != null) {
                        _pendingConnectionPointID = connection.ConsumerConnectionPointID;
                        _pendingConsumer = connection.Consumer;
                        _pendingConsumerConnectionPoint = connection.ConsumerConnectionPoint;
                        _pendingConsumerID = connection.Consumer.ID;
                        _pendingProvider = connection.Provider;
                        _pendingProviderConnectionPoint = connection.ProviderConnectionPoint;
                        _pendingTransformer = connection.Transformer;
                        _pendingSelectedValue = null;
                        _pendingConnectionType = ConnectionType.Consumer;
                        return true;
                    }
                    DisplayConnectionError();
                    return false;
                }
                if (String.IsNullOrEmpty(consumerID)) {
                    _pendingConsumer = WebPartToConnect;
                }
                else {
                    _pendingConsumer = WebPartManager.WebParts[consumerID];
                }

                _pendingConsumerConnectionPoint =
                    WebPartManager.GetConsumerConnectionPoint(_pendingConsumer, _pendingConnectionPointID);

                if (_pendingConsumerConnectionPoint == null) {
                    DisplayConnectionError();
                    return false;
                }

                // Get provider
                if (!String.IsNullOrEmpty(_pendingSelectedValue)) {
                    IDictionary providers = (IDictionary)_connectionPointInfo[_pendingConsumerConnectionPoint];
                    ProviderInfo providerInfo = null;
                    if (providers != null) {
                        providerInfo = (ProviderInfo)providers[_pendingSelectedValue];
                    }

                    if (providerInfo == null) {
                        DisplayConnectionError();
                        return false;
                    }

                    _pendingProvider = providerInfo.WebPart;
                    _pendingProviderConnectionPoint = providerInfo.ConnectionPoint;
                }

                return true;
            }
            else {
                // No pending connection
                Debug.Assert(_pendingConnectionType == ConnectionType.None);

                ClearPendingConnection();

                return false;
            }
        }

        private void Disconnect(string connectionID) {
            WebPartConnection connection = WebPartManager.Connections[connectionID];
            if (connection != null) {
                if (connection.Provider != WebPartToConnect && connection.Consumer != WebPartToConnect) {
                    throw new InvalidOperationException(SR.GetString(SR.ConnectionsZone_DisconnectInvalid));
                }
                WebPartManager.DisconnectWebParts(connection);
            }

            // Do nothing if can't find connection, since it may have been deleted by a concurrent user
        }

        private Control GetConfigurationControl(WebPartTransformer transformer) {
            Control configurationControl = transformer.CreateConfigurationControl();

            if (configurationControl == null) {
                return null;
            }
            if (configurationControl is ITransformerConfigurationControl) {
                string configControlTypeName = configurationControl.GetType().AssemblyQualifiedName;

                if (_pendingTransformerConfigurationControlTypeName != null &&
                    _pendingTransformerConfigurationControlTypeName != configControlTypeName) {

                    DisplayConnectionError();
                    return null;
                }
                else {
                    _pendingTransformerConfigurationControlTypeName = configControlTypeName;
                    return configurationControl;
                }
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.ConnectionsZone_MustImplementITransformerConfigurationControl));
            }
        }

        private string GetDisplayTitle(WebPart part, ConnectionPoint connectionPoint, bool isConsumer) {
            if (part == null) {
                return SR.GetString(SR.Part_Unknown);
            }

            int connectionPointCount = isConsumer ?
                WebPartManager.GetConsumerConnectionPoints(part).Count :
                WebPartManager.GetProviderConnectionPoints(part).Count;

            if (connectionPointCount == 1) {
                return part.DisplayTitle;
            }
            else {
                return part.DisplayTitle + " (" +
                    (connectionPoint != null ?
                     connectionPoint.DisplayName :
                     SR.GetString(SR.Part_Unknown)) + ")";
            }
        }

        private IDictionary GetValidConsumers(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                              WebPartCollection webParts) {
            HybridDictionary validConsumers = new HybridDictionary();

            // ConnectionsZone must check the AllowConnect property, since it only affects the UI
            // and is not checked by CanConnectWebParts()
            if (providerConnectionPoint == null || provider == null || !provider.AllowConnect) {
                return validConsumers;
            }

            // PERF: Skip if provider is already connected, and does not allow multiple connections
            if (!providerConnectionPoint.AllowsMultipleConnections &&
                WebPartManager.IsProviderConnected(provider, providerConnectionPoint)) {
                return validConsumers;
            }

            foreach (WebPart consumer in webParts) {
                // ConnectionsZone must check the AllowConnect property, since it only affects the UI
                // and is not checked by CanConnectWebParts()
                if (!consumer.AllowConnect) {
                    continue;
                }

                // PERF: Skip consumer if it equals provider or is closed
                if (consumer == provider || consumer.IsClosed) {
                    continue;
                }

                foreach (ConsumerConnectionPoint consumerConnectionPoint in WebPartManager.GetConsumerConnectionPoints(consumer)) {
                    if (WebPartManager.CanConnectWebParts(provider, providerConnectionPoint,
                                                          consumer, consumerConnectionPoint)) {
                        validConsumers.Add(consumer.ID + ID_SEPARATOR + consumerConnectionPoint.ID,
                            new ConsumerInfo(consumer, consumerConnectionPoint));
                    }
                    else {
                        foreach (WebPartTransformer transformer in AvailableTransformers) {
                            if (WebPartManager.CanConnectWebParts(provider, providerConnectionPoint,
                                                                  consumer, consumerConnectionPoint, transformer)) {
                                validConsumers.Add(consumer.ID + ID_SEPARATOR + consumerConnectionPoint.ID,
                                    new ConsumerInfo(consumer, consumerConnectionPoint, transformer.GetType()));
                                break;
                            }
                        }
                    }
                }
            }

            return validConsumers;
        }

        private IDictionary GetValidProviders(WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint,
                                              WebPartCollection webParts) {
            HybridDictionary validProviders = new HybridDictionary();

            // ConnectionsZone must check the AllowConnect property, since it only affects the UI
            // and is not checked by CanConnectWebParts()
            if (consumerConnectionPoint == null || consumer == null || !consumer.AllowConnect) {
                return validProviders;
            }

            // PERF: Skip if consumer is already connected, and does not allow multiple connections
            if (!consumerConnectionPoint.AllowsMultipleConnections &&
                WebPartManager.IsConsumerConnected(consumer, consumerConnectionPoint)) {
                return validProviders;
            }

            foreach (WebPart provider in webParts) {
                // ConnectionsZone must check the AllowConnect property, since it only affects the UI
                // and is not checked by CanConnectWebParts()
                if (!provider.AllowConnect) {
                    continue;
                }

                // PERF: Skip provider if it equals consumer or is closed
                if (provider == consumer || provider.IsClosed) {
                    continue;
                }

                foreach (ProviderConnectionPoint providerConnectionPoint in WebPartManager.GetProviderConnectionPoints(provider)) {
                    if (WebPartManager.CanConnectWebParts(provider, providerConnectionPoint,
                                                          consumer, consumerConnectionPoint)) {
                        validProviders.Add(provider.ID + ID_SEPARATOR + providerConnectionPoint.ID,
                            new ProviderInfo(provider, providerConnectionPoint));
                    }
                    else {
                        foreach (WebPartTransformer transformer in AvailableTransformers) {
                            if (WebPartManager.CanConnectWebParts(provider, providerConnectionPoint,
                                                                  consumer, consumerConnectionPoint, transformer)) {
                                validProviders.Add(provider.ID + ID_SEPARATOR + providerConnectionPoint.ID,
                                    new ProviderInfo(provider, providerConnectionPoint, transformer.GetType()));
                                break;
                            }
                        }
                    }
                }
            }

            return validProviders;
        }

        private bool HasConfigurationControl(WebPartTransformer transformer) {
            return (transformer.CreateConfigurationControl() != null);
        }

        protected internal override void LoadControlState(object savedState) {
            if (savedState != null) {
                object[] state = (object[])savedState;
                if (state.Length != controlStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.Invalid_ControlState));
                }

                base.LoadControlState(state[baseIndex]);

                if (state[modeIndex] != null) {
                    _mode = (ConnectionsZoneMode)state[modeIndex];
                }
                if (state[pendingConnectionPointIDIndex] != null) {
                    _pendingConnectionPointID = (string)state[pendingConnectionPointIDIndex];
                }
                if (state[pendingConnectionTypeIndex] != null) {
                    _pendingConnectionType = (ConnectionType)state[pendingConnectionTypeIndex];
                }
                if (state[pendingSelectedValueIndex] != null) {
                    _pendingSelectedValue = (string)state[pendingSelectedValueIndex];
                }
                if (state[pendingConsumerIDIndex] != null) {
                    _pendingConsumerID = (string)state[pendingConsumerIDIndex];
                }
                if (state[pendingTransformerTypeNameIndex] != null) {
                    _pendingTransformerConfigurationControlTypeName = (string)state[pendingTransformerTypeNameIndex];
                }
                if (state[pendingConnectionIDIndex] != null) {
                    _pendingConnectionID = (string)state[pendingConnectionIDIndex];
                }
            }
            else {
                base.LoadControlState(null);
            }
        }

        protected override void LoadViewState(object savedState) {
            if (savedState == null) {
                base.LoadViewState(null);
            }
            else {
                object[] myState = (object[]) savedState;
                if (myState.Length != viewStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.ViewState_InvalidViewState));
                }

                base.LoadViewState(myState[baseIndex]);
                if (myState[cancelVerbIndex] != null) {
                    ((IStateManager)CancelVerb).LoadViewState(myState[cancelVerbIndex]);
                }
                if (myState[closeVerbIndex] != null) {
                    ((IStateManager)CloseVerb).LoadViewState(myState[closeVerbIndex]);
                }
                if (myState[configureVerbIndex] != null) {
                    ((IStateManager)ConfigureVerb).LoadViewState(myState[configureVerbIndex]);
                }
                if (myState[connectVerbIndex] != null) {
                    ((IStateManager)ConnectVerb).LoadViewState(myState[connectVerbIndex]);
                }
                if (myState[disconnectVerbIndex] != null) {
                    ((IStateManager)DisconnectVerb).LoadViewState(myState[disconnectVerbIndex]);
                }
            }
        }

        private void OnConfigurationControlCancelled(object sender, EventArgs e) {
            Reset();
        }

        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (Page != null) {
                Page.RegisterRequiresControlState(this);
                Page.PreRenderComplete += new EventHandler(this.OnPagePreRenderComplete);
            }
        }

        private void OnConfigurationControlSucceeded(object sender, EventArgs e) {
            // The pending data came from control state, which came from the available
            // data in the drop-downs, which was checked for AllowConnect.
            Debug.Assert(_pendingTransformer != null);
            EnsurePendingData();
            if (_pendingConnectionType == ConnectionType.Consumer && !String.IsNullOrEmpty(_pendingConnectionID)) {
                // Editing an existing connection.  Just tell the WebPartManager that its personalization
                // data has changed, since the Transformer has already been updated.
                WebPartManager.Personalization.SetDirty();
            }
            else {
                // Creating a new connection
                if (WebPartManager.CanConnectWebParts(_pendingProvider, _pendingProviderConnectionPoint,
                                                      _pendingConsumer, _pendingConsumerConnectionPoint, _pendingTransformer)) {
                    WebPartManager.ConnectWebParts(_pendingProvider, _pendingProviderConnectionPoint,
                                                   _pendingConsumer, _pendingConsumerConnectionPoint, _pendingTransformer);
                }
                else {
                    DisplayConnectionError();
                }
            }

            Reset();
        }

        protected override void OnDisplayModeChanged(object sender, WebPartDisplayModeEventArgs e) {
            Reset();
            base.OnDisplayModeChanged(sender, e);
        }

        private void OnPagePreRenderComplete(object sender, EventArgs e) {
            // The consumer schema may have changed since we created our child controls,
            // so we should set the transformer configuration control properties again.
            // This must be done before SaveControlState is called on the transformer
            // configuration control.
            SetTransformerConfigurationControlProperties();
        }

        /// <devdoc>
        /// This is called when the SelectedWebPart changes.  Need to recreate child controls since
        /// the dropdowns will have changed.
        /// </devdoc>
        protected override void OnSelectedWebPartChanged(object sender, WebPartEventArgs e) {
            if (WebPartManager != null && WebPartManager.DisplayMode == WebPartManager.ConnectDisplayMode) {
                Reset();
            }

            base.OnSelectedWebPartChanged(sender, e);
        }

        private void DisplayConnectionError() {
            _displayErrorMessage = true;
            Reset();
        }

        protected override void RaisePostBackEvent(string eventArgument) {
            if (WebPartToConnect == null) {
                ClearPendingConnection();
                _mode = ConnectionsZoneMode.ExistingConnections;
                return;
            }
            string[] eventArguments = eventArgument.Split(ID_SEPARATOR);
            if (eventArguments.Length == 2 &&
                String.Equals(eventArguments[0], disconnectEventArgument, StringComparison.OrdinalIgnoreCase)) {
                // Disconnecting
                if (DisconnectVerb.Visible && DisconnectVerb.Enabled) {
                    string connectionID = eventArguments[1];

                    Disconnect(connectionID);

                    _mode = ConnectionsZoneMode.ExistingConnections;
                }
            }
            else if (eventArguments.Length == 3 &&
                String.Equals(eventArguments[0], connectEventArgument, StringComparison.OrdinalIgnoreCase)) {
                // Connecting
                if (ConnectVerb.Visible && ConnectVerb.Enabled) {
                    string connectionPointID = eventArguments[2];
                    if (String.Equals(eventArguments[1], providerEventArgument, StringComparison.OrdinalIgnoreCase)) {
                        ConnectProvider(connectionPointID);
                    }
                    else {
                        ConnectConsumer(connectionPointID);
                    }
                }
            }
            else if (eventArguments.Length == 2 &&
                String.Equals(eventArguments[0], configureEventArgument, StringComparison.OrdinalIgnoreCase)) {
                // Displaying transformer UI
                _pendingConnectionID = eventArguments[1];
                _pendingConnectionType = ConnectionType.Consumer;
                _mode = ConnectionsZoneMode.ConfiguringTransformer;
            }
            else if (String.Equals(eventArgument, connectConsumerEventArgument, StringComparison.OrdinalIgnoreCase)) {
                // Create connection to consumer
                _mode = ConnectionsZoneMode.ConnectToConsumer;
            }
            else if (String.Equals(eventArgument, connectProviderEventArgument, StringComparison.OrdinalIgnoreCase)) {
                // Create connection to provider
                _mode = ConnectionsZoneMode.ConnectToProvider;
            }
            else if (String.Equals(eventArgument, closeEventArgument, StringComparison.OrdinalIgnoreCase)) {
                // Closing the zone
                if (CloseVerb.Visible && CloseVerb.Enabled) {
                    Close();
                    _mode = ConnectionsZoneMode.ExistingConnections;
                }
            }
            else if (String.Equals(eventArgument, cancelEventArgument, StringComparison.OrdinalIgnoreCase)) {
                // Cancelling connection creation
                if (CancelVerb.Visible && CancelVerb.Enabled) {
                    _mode = ConnectionsZoneMode.ExistingConnections;
                }
            }
            else {
                base.RaisePostBackEvent(eventArgument);
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            // A connection may have been added or removed since we created our child controls,
            // so we should set the dropdown properties again.  Also, we need to update the display
            // names of the WebParts, which were set by the WebPartManager in Page.PreRenderComplete.
            SetDropDownProperties();

            base.Render(writer);
        }

        private void RenderAddVerbs(HtmlTextWriter writer) {
            WebPart webPartToConnect = WebPartToConnect;

            WebPartCollection webParts = null;
            if (WebPartManager != null) {
                webParts = WebPartManager.WebParts;
            }

            if (webPartToConnect != null || DesignMode) {
                // Are there any compatible consumers?
                bool consumersAvailable = DesignMode;
                if (!consumersAvailable && WebPartManager != null) {
                    ProviderConnectionPointCollection providers =
                        WebPartManager.GetEnabledProviderConnectionPoints(webPartToConnect);
                    foreach (ProviderConnectionPoint provider in providers) {
                        if (GetValidConsumers(webPartToConnect, provider, webParts).Count != 0) {
                            consumersAvailable = true;
                            break;
                        }
                    }
                }
                if (consumersAvailable) {
                    ZoneLinkButton connectConsumerButton = new ZoneLinkButton(this, connectConsumerEventArgument);
                    connectConsumerButton.Text = ConnectToConsumerText;
                    connectConsumerButton.ApplyStyle(VerbStyle);
                    connectConsumerButton.Page = Page;
                    connectConsumerButton.RenderControl(writer);
                    writer.WriteBreak();
                }

                // Are there any compatible providers?
                bool providersAvailable = DesignMode;
                if (!providersAvailable && WebPartManager != null) {
                    ConsumerConnectionPointCollection consumers =
                        WebPartManager.GetEnabledConsumerConnectionPoints(webPartToConnect);
                    foreach (ConsumerConnectionPoint consumer in consumers) {
                        if (GetValidProviders(webPartToConnect, consumer, webParts).Count != 0) {
                            providersAvailable = true;
                            break;
                        }
                    }
                }
                if (providersAvailable) {
                    ZoneLinkButton connectProviderButton = new ZoneLinkButton(this, connectProviderEventArgument);
                    connectProviderButton.Text = ConnectToProviderText;
                    connectProviderButton.ApplyStyle(VerbStyle);
                    connectProviderButton.Page = Page;
                    connectProviderButton.RenderControl(writer);
                    writer.WriteBreak();
                }

                // Add separator if anything was rendered
                if (providersAvailable || consumersAvailable) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                    writer.RenderEndTag();
                }
            }
        }

        protected override void RenderBody(HtmlTextWriter writer) {
            if (this.PartChromeType == PartChromeType.Default ||
                this.PartChromeType == PartChromeType.BorderOnly ||
                this.PartChromeType == PartChromeType.TitleAndBorder) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, "Black");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "1px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "Solid");
            }

            RenderBodyTableBeginTag(writer);

            RenderErrorMessage(writer);

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            switch (_mode) {
            case ConnectionsZoneMode.ConfiguringTransformer:
                if (_pendingTransformerConfigurationControl != null) {
                    RenderTransformerConfigurationHeader(writer);
                    _pendingTransformerConfigurationControl.RenderControl(writer);
                }
                break;
            case ConnectionsZoneMode.ConnectToConsumer:
                RenderConnectToConsumersDropDowns(writer);
                break;
            case ConnectionsZoneMode.ConnectToProvider:
                RenderConnectToProvidersDropDowns(writer);
                break;
            default:
                RenderAddVerbs(writer);
                RenderExistingConnections(writer);
                break;
            }

            writer.RenderEndTag();  // Td
            writer.RenderEndTag();  // Tr
            RenderBodyTableEndTag(writer);
        }

        private void RenderConnectToConsumersDropDowns(HtmlTextWriter writer) {
            WebPart webPartToConnect = WebPartToConnect;

            if (webPartToConnect != null) {
                ProviderConnectionPointCollection providers =
                    WebPartManager.GetEnabledProviderConnectionPoints(webPartToConnect);
                bool first = true;
                Label label = new Label();
                label.Page = Page;
                label.AssociatedControlInControlTree = false;
                foreach (ProviderConnectionPoint provider in providers) {
                    DropDownList list = (DropDownList)_connectDropDownLists[provider];
                    if ((list == null) || !list.Enabled) {
                        continue;
                    }

                    if (first) {
                        string connectToConsumerTitle = ConnectToConsumerTitle;
                        if (!String.IsNullOrEmpty(connectToConsumerTitle)) {
                            label.Text = connectToConsumerTitle;
                            label.ApplyStyle(LabelStyle);
                            label.AssociatedControlID = String.Empty;
                            label.RenderControl(writer);
                            writer.WriteBreak();
                        }

                        string connectToConsumerInstructionText = ConnectToConsumerInstructionText;
                        if (!String.IsNullOrEmpty(connectToConsumerInstructionText)) {
                            writer.WriteBreak();
                            label.Text = connectToConsumerInstructionText;
                            label.ApplyStyle(InstructionTextStyle);
                            label.AssociatedControlID = String.Empty;
                            label.RenderControl(writer);
                            writer.WriteBreak();
                        }

                        first = false;
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);

                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    label.ApplyStyle(LabelStyle);
                    label.Text = SendText;
                    label.AssociatedControlID = String.Empty;
                    label.RenderControl(writer);

                    writer.RenderEndTag(); // TD

                    LabelStyle.AddAttributesToRender(writer, this);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    writer.WriteEncodedText(provider.DisplayName);

                    writer.RenderEndTag(); // TD
                    writer.RenderEndTag(); // TR
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    label.Text = SendToText;
                    label.AssociatedControlID = list.ClientID;
                    label.RenderControl(writer);

                    writer.RenderEndTag(); // TD
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    list.ApplyStyle(EditUIStyle);
                    list.RenderControl(writer);

                    writer.RenderEndTag(); // TD
                    writer.RenderEndTag(); // TR
                    writer.RenderEndTag(); // TABLE

                    WebPartVerb connectVerb = ConnectVerb;
                    connectVerb.EventArgument = String.Join(ID_SEPARATOR.ToString(CultureInfo.InvariantCulture),
                        new string[] { connectEventArgument, providerEventArgument, provider.ID });
                    RenderVerb(writer, connectVerb);

                    writer.RenderEndTag(); // FIELDSET
                }
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "right");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                WebPartVerb cancelVerb = CancelVerb;
                cancelVerb.EventArgument = cancelEventArgument;
                RenderVerb(writer, cancelVerb);
                writer.RenderEndTag();
            }
        }

        private void RenderConnectToProvidersDropDowns(HtmlTextWriter writer) {
            WebPart webPartToConnect = WebPartToConnect;

            if (webPartToConnect != null) {
                ConsumerConnectionPointCollection consumers =
                    WebPartManager.GetEnabledConsumerConnectionPoints(webPartToConnect);
                bool first = true;
                Label label = new Label();
                label.Page = Page;
                label.AssociatedControlInControlTree = false;
                foreach (ConsumerConnectionPoint consumer in consumers) {
                    DropDownList list = (DropDownList)_connectDropDownLists[consumer];
                    if ((list == null) || !list.Enabled) {
                        continue;
                    }

                    if (first) {
                        string connectToProviderTitle = ConnectToProviderTitle;
                        if (!String.IsNullOrEmpty(connectToProviderTitle)) {
                            label.Text = connectToProviderTitle;
                            label.ApplyStyle(LabelStyle);
                            label.AssociatedControlID = String.Empty;
                            label.RenderControl(writer);
                            writer.WriteBreak();
                        }

                        string connectToProviderInstructionText = ConnectToProviderInstructionText;
                        if (!String.IsNullOrEmpty(connectToProviderInstructionText)) {
                            writer.WriteBreak();
                            label.Text = connectToProviderInstructionText;
                            label.ApplyStyle(InstructionTextStyle);
                            label.AssociatedControlID = String.Empty;
                            label.RenderControl(writer);
                            writer.WriteBreak();
                        }

                        first = false;
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);

                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    label.ApplyStyle(LabelStyle);
                    label.Text = GetText;
                    label.AssociatedControlID = String.Empty;
                    label.RenderControl(writer);

                    writer.RenderEndTag(); // TD

                    LabelStyle.AddAttributesToRender(writer, this);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    writer.WriteEncodedText(consumer.DisplayName);

                    writer.RenderEndTag(); // TD
                    writer.RenderEndTag(); // TR
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    label.Text = GetFromText;
                    label.AssociatedControlID = list.ClientID;
                    label.RenderControl(writer);

                    writer.RenderEndTag(); // TD
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    list.ApplyStyle(EditUIStyle);
                    list.RenderControl(writer);

                    writer.RenderEndTag(); // TD
                    writer.RenderEndTag(); // TR
                    writer.RenderEndTag(); // TABLE

                    WebPartVerb connectVerb = ConnectVerb;
                    connectVerb.EventArgument = String.Join(ID_SEPARATOR.ToString(CultureInfo.InvariantCulture),
                        new string[] { connectEventArgument, consumerEventArgument, consumer.ID });
                    RenderVerb(writer, connectVerb);

                    writer.RenderEndTag(); // FIELDSET
                }
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "right");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                WebPartVerb cancelVerb = CancelVerb;
                cancelVerb.EventArgument = cancelEventArgument;
                RenderVerb(writer, cancelVerb);
                writer.RenderEndTag();
            }
        }

        private void RenderErrorMessage(HtmlTextWriter writer) {
            if (_displayErrorMessage) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                TableCell td = new TableCell();
                td.ApplyStyle(ErrorStyle);
                td.Text = NewConnectionErrorMessage;
                td.RenderControl(writer);
                writer.RenderEndTag();
            }
        }

        private void RenderExistingConnections(HtmlTextWriter writer) {
            WebPartManager manager = WebPartManager;
            bool headerRendered = false;
            bool consumersHeaderRendered = false;
            bool providersHeaderRendered = false;
            if (manager != null) {
                WebPart webPartToConnect = WebPartToConnect;
                // First, display connections for which we are providers (connections to consumers)
                WebPartConnectionCollection connections = manager.Connections;
                foreach (WebPartConnection connection in connections) {
                    if (connection.Provider == webPartToConnect) {
                        if (!headerRendered) {
                            RenderInstructionTitle(writer);
                            RenderInstructionText(writer);
                            headerRendered = true;
                        }
                        if (!consumersHeaderRendered) {
                            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
                            LabelStyle.AddAttributesToRender(writer, this);
                            writer.RenderBeginTag(HtmlTextWriterTag.Legend);
                            writer.Write(ConsumersTitle);
                            writer.RenderEndTag(); // Legend
                            string instructionText = ConsumersInstructionText;
                            if (!String.IsNullOrEmpty(instructionText)) {
                                writer.WriteBreak();
                                Label label = new Label();
                                label.Text = instructionText;
                                label.Page = Page;
                                label.ApplyStyle(InstructionTextStyle);
                                label.RenderControl(writer);
                                writer.WriteBreak();
                            }
                            consumersHeaderRendered = true;
                        }
                        RenderExistingConsumerConnection(writer, connection);
                    }
                }
                if (consumersHeaderRendered) {
                    writer.RenderEndTag(); // Fieldset
                }
                // Then, display connections for which we are consumers (connections to providers)
                foreach (WebPartConnection connection in connections) {
                    if (connection.Consumer == webPartToConnect) {
                        if (!headerRendered) {
                            RenderInstructionTitle(writer);
                            RenderInstructionText(writer);
                            headerRendered = true;
                        }
                        if (!providersHeaderRendered) {
                            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
                            LabelStyle.AddAttributesToRender(writer, this);
                            writer.RenderBeginTag(HtmlTextWriterTag.Legend);
                            writer.Write(ProvidersTitle);
                            writer.RenderEndTag(); // Legend
                            string instructionText = ProvidersInstructionText;
                            if (!String.IsNullOrEmpty(instructionText)) {
                                writer.WriteBreak();
                                Label label = new Label();
                                label.Text = instructionText;
                                label.Page = Page;
                                label.ApplyStyle(InstructionTextStyle);
                                label.RenderControl(writer);
                                writer.WriteBreak();
                            }
                            providersHeaderRendered = true;
                        }
                        RenderExistingProviderConnection(writer, connection);
                    }
                }
            }
            if (providersHeaderRendered) {
                writer.RenderEndTag(); // Fieldset
            }
            if (headerRendered) {
                writer.WriteBreak();
            }
            else {
                RenderNoExistingConnection(writer);
            }
        }

        private void RenderExistingConnection(HtmlTextWriter writer,
                                              string connectionPointName,
                                              string partTitle,
                                              string disconnectEventArg,
                                              string editEventArg,
                                              bool consumer,
                                              bool isActive) {

            Label label = new Label();
            label.Page = Page;
            label.ApplyStyle(LabelStyle);

            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);

            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            label.Text = (consumer ? SendText : GetText);
            label.RenderControl(writer);

            writer.RenderEndTag(); // TD
            LabelStyle.AddAttributesToRender(writer, this);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            writer.WriteEncodedText(connectionPointName);

            writer.RenderEndTag(); // TD
            writer.RenderEndTag(); // TR
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            label.Text = (consumer ? SendToText : GetFromText);
            label.RenderControl(writer);

            writer.RenderEndTag(); // TD
            LabelStyle.AddAttributesToRender(writer, this);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            writer.WriteEncodedText(partTitle);

            writer.RenderEndTag(); // TD
            writer.RenderEndTag(); // TR
            writer.RenderEndTag(); // TABLE

            WebPartVerb disconnectVerb = DisconnectVerb;
            disconnectVerb.EventArgument = disconnectEventArg;
            RenderVerb(writer, disconnectVerb);
            if (VerbButtonType == ButtonType.Link) {
                writer.Write("&nbsp;");
            }

            if (isActive) {
                WebPartVerb configureVerb = ConfigureVerb;
                if (editEventArg == null) {
                    configureVerb.Enabled = false;
                }
                else {
                    configureVerb.Enabled = true;
                    configureVerb.EventArgument = editEventArg;
                }
                RenderVerb(writer, configureVerb);
            }
            else {
                writer.WriteBreak();
                label.ApplyStyle(ErrorStyle);
                label.Text = ExistingConnectionErrorMessage;
                label.RenderControl(writer);
            }

            writer.RenderEndTag(); // FIELDSET
        }

        private void RenderExistingConsumerConnection(HtmlTextWriter writer, WebPartConnection connection) {
            WebPart webPartToConnect = WebPartToConnect;

            Debug.Assert(connection.Provider == webPartToConnect);

            ProviderConnectionPoint providerConnectionPoint =
               WebPartManager.GetProviderConnectionPoint(webPartToConnect, connection.ProviderConnectionPointID);

            WebPart consumer = connection.Consumer;
            ConsumerConnectionPoint consumerConnectionPoint = connection.ConsumerConnectionPoint;
            string consumerString = GetDisplayTitle(consumer, consumerConnectionPoint, true);

            // Transformer
            string transformerEventArgs = null;
            WebPartTransformer transformer = connection.Transformer;
            if (transformer != null && HasConfigurationControl(transformer))
            {
                transformerEventArgs = configureEventArgument + ID_SEPARATOR.ToString(CultureInfo.InvariantCulture) + connection.ID;
            }

            bool isActive = providerConnectionPoint != null &&
                consumerConnectionPoint != null &&
                connection.Provider != null &&
                connection.Consumer != null &&
                connection.IsActive;
            // IsActive already checks for those:
            Debug.Assert(!connection.Provider.IsClosed && !connection.Consumer.IsClosed);

            RenderExistingConnection(writer,
                                     (providerConnectionPoint != null ?
                                        providerConnectionPoint.DisplayName :
                                        SR.GetString(SR.Part_Unknown)),
                                     consumerString,
                                     String.Join(ID_SEPARATOR.ToString(CultureInfo.InvariantCulture),
                                                 new string[] { disconnectEventArgument, connection.ID }),
                                     transformerEventArgs,
                                     true,
                                     isActive);
        }

        private void RenderExistingProviderConnection(HtmlTextWriter writer, WebPartConnection connection) {
            WebPart webPartToConnect = WebPartToConnect;

            Debug.Assert(connection.Consumer == webPartToConnect);

            ConsumerConnectionPoint consumerConnectionPoint =
               WebPartManager.GetConsumerConnectionPoint(webPartToConnect, connection.ConsumerConnectionPointID);

            WebPart provider = connection.Provider;
            ProviderConnectionPoint providerConnectionPoint = connection.ProviderConnectionPoint;
            string providerString = GetDisplayTitle(provider, providerConnectionPoint, false);

            // Transformer
            string transformerEventArgs = null;
            WebPartTransformer transformer = connection.Transformer;
            if (transformer != null && HasConfigurationControl(transformer))
            {
                transformerEventArgs = configureEventArgument + ID_SEPARATOR.ToString(CultureInfo.InvariantCulture) + connection.ID;
            }

            bool isActive = providerConnectionPoint != null &&
                consumerConnectionPoint != null &&
                connection.Provider != null &&
                connection.Consumer != null &&
                connection.IsActive;
            // IsActive already checks for these:
            Debug.Assert(!connection.Provider.IsClosed && !connection.Consumer.IsClosed);

            RenderExistingConnection(writer,
                                     (consumerConnectionPoint != null ?
                                      consumerConnectionPoint.DisplayName :
                                      SR.GetString(SR.Part_Unknown)),
                                     providerString,
                                     String.Join(ID_SEPARATOR.ToString(CultureInfo.InvariantCulture),
                                                 new string[] { disconnectEventArgument, connection.ID }),
                                     transformerEventArgs,
                                     false,
                                     isActive);
        }

        private void RenderInstructionText(HtmlTextWriter writer) {
            string instructionText = InstructionText;
            if (!String.IsNullOrEmpty(instructionText)) {
                Label label = new Label();
                label.Text = instructionText;
                label.Page = Page;
                label.ApplyStyle(InstructionTextStyle);
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
            }
        }

        private void RenderInstructionTitle(HtmlTextWriter writer) {
            if (this.PartChromeType == PartChromeType.None ||
                this.PartChromeType == PartChromeType.BorderOnly)
                return;

            string instructionTitle = InstructionTitle;
            if (!String.IsNullOrEmpty(instructionTitle)) {
                Label label = new Label();
                if (WebPartToConnect != null) {
                    label.Text = String.Format(CultureInfo.CurrentCulture, instructionTitle, WebPartToConnect.DisplayTitle);
                }
                else {
                    label.Text = instructionTitle;
                }
                label.Page = Page;
                label.ApplyStyle(LabelStyle);
                label.RenderControl(writer);
                writer.WriteBreak();
            }
        }

        private void RenderNoExistingConnection(HtmlTextWriter writer) {
            string noConnection = NoExistingConnectionTitle;
            if (!String.IsNullOrEmpty(noConnection)) {
                Label label = new Label();
                label.Text = noConnection;
                label.Page = Page;
                label.ApplyStyle(LabelStyle);
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
            }
            string instructions = NoExistingConnectionInstructionText;
            if (!String.IsNullOrEmpty(instructions)) {
                Label label = new Label();
                label.Text = instructions;
                label.Page = Page;
                label.ApplyStyle(InstructionTextStyle);
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
            }
        }

        private void RenderTransformerConfigurationHeader(HtmlTextWriter writer) {
            Debug.Assert(_pendingTransformer != null);

            if (EnsurePendingData()) {
                Debug.Assert(_pendingConsumer != null && _pendingProvider != null
                             && _pendingConsumerConnectionPoint != null
                             && _pendingProviderConnectionPoint != null);

                string connectionPointName = null;
                string partTitle = null;

                bool isConsumer = (_pendingConsumer == WebPartToConnect);
                if (_pendingConnectionType == ConnectionType.Consumer && isConsumer) {
                    // This happens if we are in a consumer, connecting or reconnecting a provider
                    partTitle = _pendingProvider.DisplayTitle;
                    connectionPointName = _pendingConsumerConnectionPoint.DisplayName;
                }
                else {
                    // This happens if we are reconnecting a consumer from a provider
                    partTitle = _pendingConsumer.DisplayTitle;
                    connectionPointName = _pendingProviderConnectionPoint.DisplayName;
                }

                Label label = new Label();
                label.Page = Page;
                label.ApplyStyle(LabelStyle);
                label.Text = (isConsumer ? ConnectToProviderTitle : ConnectToConsumerTitle);
                label.RenderControl(writer);

                writer.WriteBreak();
                writer.WriteBreak();

                label.ApplyStyle(InstructionTextStyle);
                label.Text = (isConsumer ? ConnectToProviderInstructionText : ConnectToConsumerInstructionText);
                label.RenderControl(writer);

                writer.WriteBreak();
                writer.WriteBreak();

                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                label.ApplyStyle(LabelStyle);
                label.Text = (isConsumer ? GetText : SendText);
                label.RenderControl(writer);

                writer.RenderEndTag(); // TD
                LabelStyle.AddAttributesToRender(writer, this);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                writer.WriteEncodedText(connectionPointName);

                writer.RenderEndTag(); // TD
                writer.RenderEndTag(); // TR
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                label.Text = (isConsumer ? GetFromText : SendToText);
                label.RenderControl(writer);

                writer.RenderEndTag(); // TD
                LabelStyle.AddAttributesToRender(writer, this);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                writer.WriteEncodedText(partTitle);

                writer.RenderEndTag(); // TD
                writer.RenderEndTag(); // TR
                writer.RenderEndTag(); // TABLE

                writer.WriteBreak();

                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag(); // HR

                writer.WriteBreak();

                label.ApplyStyle(LabelStyle);
                label.Text = ConfigureConnectionTitle;
                label.RenderControl(writer);

                writer.WriteBreak();
                writer.WriteBreak();
            }
        }

        protected override void RenderVerbs(HtmlTextWriter writer) {
            RenderVerbsInternal(writer, new WebPartVerb[] {CloseVerb});
        }

        private void Reset() {
            ClearPendingConnection();
            ChildControlsCreated = false;
            _mode = ConnectionsZoneMode.ExistingConnections;
        }

        protected internal override object SaveControlState() {
            object baseState = base.SaveControlState();
            // Assuming here that the base class does not have control state
            if (_mode != ConnectionsZoneMode.ExistingConnections || baseState != null) {
                // The control state data only makes sense if we're not in display existing connections mode.
                object[] myState = new object[controlStateArrayLength];

                myState[baseIndex] = baseState;
                myState[modeIndex] = _mode;
                myState[pendingConnectionPointIDIndex] = _pendingConnectionPointID;
                myState[pendingConnectionTypeIndex] = _pendingConnectionType;
                myState[pendingSelectedValueIndex] = _pendingSelectedValue;
                myState[pendingConsumerIDIndex] = _pendingConsumerID;
                myState[pendingTransformerTypeNameIndex] = _pendingTransformerConfigurationControlTypeName;
                myState[pendingConnectionIDIndex] = _pendingConnectionID;

                return myState;
            }
            else {
                return null;
            }
        }

        protected override object SaveViewState() {
            object[] myState = new object[viewStateArrayLength];

            myState[baseIndex] = base.SaveViewState();
            myState[cancelVerbIndex] = (_cancelVerb != null) ? ((IStateManager)_cancelVerb).SaveViewState() : null;
            myState[closeVerbIndex] = (_closeVerb != null) ? ((IStateManager)_closeVerb).SaveViewState() : null;
            myState[configureVerbIndex] = (_configureVerb != null) ? ((IStateManager)_configureVerb).SaveViewState() : null;
            myState[connectVerbIndex] = (_connectVerb != null) ? ((IStateManager)_connectVerb).SaveViewState() : null;
            myState[disconnectVerbIndex] = (_disconnectVerb != null) ? ((IStateManager)_disconnectVerb).SaveViewState() : null;

            for (int i=0; i < viewStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        private void SelectValueInList(ListControl list, string value) {
            if (list == null) {
                DisplayConnectionError();
                return;
            }
            ListItem selectedItem = list.Items.FindByValue(value);
            if (selectedItem != null) {
                selectedItem.Selected = true;
            }
            else {
                DisplayConnectionError();
            }
        }

        private void SetDropDownProperties() {
            bool anyRelevantControl = false;
            WebPart webPartToConnect = WebPartToConnect;
            if (webPartToConnect != null && !webPartToConnect.IsClosed) {
                Debug.Assert(WebPartManager != null);

                WebPartCollection webParts = WebPartManager.WebParts;

                ProviderConnectionPointCollection providerConnectionPoints =
                    WebPartManager.GetEnabledProviderConnectionPoints(webPartToConnect);
                foreach (ProviderConnectionPoint providerConnectionPoint in providerConnectionPoints) {
                    DropDownList list = (DropDownList)_connectDropDownLists[providerConnectionPoint];
                    if (list == null) {
                        continue;
                    }
                    list.Items.Clear();

                    // Set the selected index to 0, in case it was set by ControlState
                    list.SelectedIndex = 0;

                    IDictionary consumers = GetValidConsumers(webPartToConnect, providerConnectionPoint, webParts);
                    if (consumers.Count == 0) {
                        list.Enabled = false;
                        list.Items.Add(new ListItem(SR.GetString(SR.ConnectionsZone_NoConsumers), String.Empty));
                    }
                    else {
                        list.Enabled = true;
                        list.Items.Add(new ListItem());
                        _connectionPointInfo[providerConnectionPoint] = consumers;

                        // If the WebPart is currently connected on this provider point
                        // and does not support multiple connections,
                        // select the current provider and disable the dropdown.
                        WebPartConnection currentConnection = providerConnectionPoint.AllowsMultipleConnections ?
                            null :
                            WebPartManager.GetConnectionForProvider(webPartToConnect, providerConnectionPoint);
                        WebPart currentConsumerWebPart = null;
                        ConsumerConnectionPoint currentConsumerConnectionPoint = null;
                        if (currentConnection != null) {
                            currentConsumerWebPart = currentConnection.Consumer;
                            currentConsumerConnectionPoint = currentConnection.ConsumerConnectionPoint;
                            list.Enabled = false;
                        }
                        else {
                            anyRelevantControl = true;
                        }

                        foreach (DictionaryEntry consumerEntry in consumers) {
                            ConsumerInfo consumer = (ConsumerInfo)consumerEntry.Value;
                            ListItem item = new ListItem();
                            item.Text = GetDisplayTitle(consumer.WebPart, consumer.ConnectionPoint, true);
                            item.Value = (string)consumerEntry.Key;
                            if (currentConnection != null &&
                                consumer.WebPart == currentConsumerWebPart &&
                                consumer.ConnectionPoint == currentConsumerConnectionPoint) {

                                item.Selected = true;
                            }
                            list.Items.Add(item);
                        }
                    }
                }

                ConsumerConnectionPointCollection consumerConnectionPoints =
                    WebPartManager.GetEnabledConsumerConnectionPoints(webPartToConnect);
                foreach (ConsumerConnectionPoint consumerConnectionPoint in consumerConnectionPoints) {
                    DropDownList list = (DropDownList)_connectDropDownLists[consumerConnectionPoint];
                    if (list == null) {
                        continue;
                    }
                    list.Items.Clear();

                    // Set the selected index to 0, in case it was set by ControlState
                    list.SelectedIndex = 0;

                    IDictionary providers = GetValidProviders(webPartToConnect, consumerConnectionPoint, webParts);
                    if (providers.Count == 0) {
                        list.Enabled = false;
                        list.Items.Add(new ListItem(SR.GetString(SR.ConnectionsZone_NoProviders), String.Empty));
                    }
                    else {
                        list.Enabled = true;
                        list.Items.Add(new ListItem());
                        _connectionPointInfo[consumerConnectionPoint] = providers;

                        // If the WebPart is currently connected on this consumer point
                        // and does not support multiple connections,
                        // select the current provider and disable the dropdown.
                        WebPartConnection currentConnection = consumerConnectionPoint.AllowsMultipleConnections ?
                            null :
                            WebPartManager.GetConnectionForConsumer(webPartToConnect, consumerConnectionPoint);
                        WebPart currentProviderWebPart = null;
                        ProviderConnectionPoint currentProviderConnectionPoint = null;
                        if (currentConnection != null) {
                            currentProviderWebPart = currentConnection.Provider;
                            currentProviderConnectionPoint = currentConnection.ProviderConnectionPoint;
                            list.Enabled = false;
                        }
                        else {
                            anyRelevantControl = true;
                        }

                        foreach (DictionaryEntry providerEntry in providers) {
                            ProviderInfo provider = (ProviderInfo)providerEntry.Value;
                            ListItem item = new ListItem();
                            item.Text = GetDisplayTitle(provider.WebPart, provider.ConnectionPoint, false);
                            item.Value = (string)providerEntry.Key;
                            if (currentConnection != null &&
                                provider.WebPart == currentProviderWebPart &&
                                provider.ConnectionPoint == currentProviderConnectionPoint) {
                                item.Selected = true;
                            }
                            list.Items.Add(item);
                        }
                    }
                }

                if (_pendingConnectionType == ConnectionType.Consumer &&
                    _pendingSelectedValue != null &&
                    _pendingSelectedValue.Length > 0) {

                    EnsurePendingData();

                    if (_pendingConsumerConnectionPoint != null) {
                        // Display the pending connection in the appropriate DropDownList
                        Debug.Assert(_pendingSelectedValue != null);
                        DropDownList list = (DropDownList)_connectDropDownLists[_pendingConsumerConnectionPoint];
                        if (list == null) {
                            _mode = ConnectionsZoneMode.ExistingConnections;
                            return;
                        }
                        SelectValueInList(list, _pendingSelectedValue);
                    }
                    else {
                        _mode = ConnectionsZoneMode.ExistingConnections;
                        return;
                    }
                }
                else if (_pendingConnectionType == ConnectionType.Provider) {
                    EnsurePendingData();

                    if (_pendingProviderConnectionPoint != null) {
                        // Display the pending connection in the appropriate DropDownList
                        Debug.Assert(_pendingSelectedValue != null);
                        DropDownList list = (DropDownList)_connectDropDownLists[_pendingProviderConnectionPoint];
                        if (list == null) {
                            _mode = ConnectionsZoneMode.ExistingConnections;
                            return;
                        }
                        SelectValueInList(list, _pendingSelectedValue);
                    }
                    else {
                        _mode = ConnectionsZoneMode.ExistingConnections;
                        return;
                    }
                }

                if (!anyRelevantControl &&
                    (_mode == ConnectionsZoneMode.ConnectToConsumer ||
                    _mode == ConnectionsZoneMode.ConnectToProvider)) {

                    _mode = ConnectionsZoneMode.ExistingConnections;
                }
            }
        }

        private void SetTransformerConfigurationControlProperties() {
            if (EnsurePendingData()) {
                Control pendingProviderControl = _pendingProvider.ToControl();
                Control pendingConsumerControl = _pendingConsumer.ToControl();
                object dataObject = _pendingProviderConnectionPoint.GetObject(pendingProviderControl);
                object transformedObject = _pendingTransformer.Transform(dataObject);
                _pendingConsumerConnectionPoint.SetObject(pendingConsumerControl, transformedObject);

                if ((_pendingConnectionType == ConnectionType.Consumer &&
                        (String.IsNullOrEmpty(_pendingConnectionID) ||
                        _pendingConsumerConnectionPoint.AllowsMultipleConnections)) ||
                    _pendingConnectionType == ConnectionType.Provider) {
                    // "Disconnect" the provider after it set its schema
                    _pendingConsumerConnectionPoint.SetObject(pendingConsumerControl, null);
                }
            }
        }

        protected override void TrackViewState() {
            base.TrackViewState();

            if (_cancelVerb != null) {
                ((IStateManager) _cancelVerb).TrackViewState();
            }
            if (_closeVerb != null) {
                ((IStateManager) _closeVerb).TrackViewState();
            }
            if (_configureVerb != null) {
                ((IStateManager) _configureVerb).TrackViewState();
            }
            if (_connectVerb != null) {
                ((IStateManager) _connectVerb).TrackViewState();
            }
            if (_disconnectVerb != null) {
                ((IStateManager) _disconnectVerb).TrackViewState();
            }
        }

        private abstract class ConnectionPointInfo {
            private WebPart _webPart;
            private Type _transformerType;

            protected ConnectionPointInfo(WebPart webPart) {
                _webPart = webPart;
            }

            protected ConnectionPointInfo(WebPart webPart, Type transformerType) : this(webPart) {
                _transformerType = transformerType;
            }

            public Type TransformerType {
                get {
                    return _transformerType;
                }
            }

            public WebPart WebPart {
                get {
                    return _webPart;
                }
            }
        }

        private sealed class ConsumerInfo : ConnectionPointInfo {
            private ConsumerConnectionPoint _connectionPoint;

            public ConsumerInfo(WebPart webPart, ConsumerConnectionPoint connectionPoint) : base(webPart) {
                _connectionPoint = connectionPoint;
            }

            public ConsumerInfo(WebPart webPart, ConsumerConnectionPoint connectionPoint,
                                Type transformerType) : base(webPart, transformerType) {
                _connectionPoint = connectionPoint;
            }

            public ConsumerConnectionPoint ConnectionPoint {
                get {
                    return _connectionPoint;
                }
            }
        }

        private sealed class ProviderInfo : ConnectionPointInfo {
            private ProviderConnectionPoint _connectionPoint;

            public ProviderInfo(WebPart webPart, ProviderConnectionPoint connectionPoint) : base(webPart) {
                _connectionPoint = connectionPoint;
            }

            public ProviderInfo(WebPart webPart, ProviderConnectionPoint connectionPoint,
                                Type transformerType) : base(webPart, transformerType) {
                _connectionPoint = connectionPoint;
            }

            public ProviderConnectionPoint ConnectionPoint {
                get {
                    return _connectionPoint;
                }
            }
        }

        private enum ConnectionType {
            None = 0,
            Consumer = 1,
            Provider = 2
        }

        private enum ConnectionsZoneMode {
            ExistingConnections = 0,
            ConnectToConsumer = 1,
            ConnectToProvider = 2,
            ConfiguringTransformer = 3
        }
    }
}

