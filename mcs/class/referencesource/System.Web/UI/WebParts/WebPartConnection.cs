//------------------------------------------------------------------------------
// <copyright file="WebPartConnection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;

    [
    TypeConverter(typeof(ExpandableObjectConverter)),
    ParseChildren(true, "Transformers")
    ]
    public sealed class WebPartConnection {

        private string _consumerConnectionPointID;
        private string _consumerID;
        private bool _deleted;
        private string _id;
        private bool _isActive;
        private bool _isShared;
        private bool _isStatic;
        private string _providerConnectionPointID;
        private string _providerID;
        private WebPartTransformerCollection _transformers;
        private WebPartManager _webPartManager;

        public WebPartConnection() {
            _isStatic = true;
            _isShared = true;
        }

        // PERF: Consider caching
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public WebPart Consumer {
            get {
                string consumerID = ConsumerID;
                if (consumerID.Length == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_ConsumerIDNotSet));
                }

                if (_webPartManager != null) {
                    return _webPartManager.WebParts[consumerID];
                }
                else {
                    return null;
                }
            }
        }

        // PERF: Consider caching
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public ConsumerConnectionPoint ConsumerConnectionPoint {
            get {
                WebPart consumer = Consumer;
                if (consumer != null && _webPartManager != null) {
                    return _webPartManager.GetConsumerConnectionPoint(consumer, ConsumerConnectionPointID);
                }
                else {
                    return null;
                }
            }
        }

        [
        DefaultValue(ConnectionPoint.DefaultIDInternal)
        ]
        public string ConsumerConnectionPointID {
            get {
                // 
                return (!String.IsNullOrEmpty(_consumerConnectionPointID)) ?
                    _consumerConnectionPointID : ConnectionPoint.DefaultID;
            }
            set {
                _consumerConnectionPointID = value;
            }
        }

        [
        DefaultValue("")
        ]
        public string ConsumerID {
            get {
                return (_consumerID != null) ? _consumerID : String.Empty;
            }
            set {
                _consumerID = value;
            }
        }

        // True if this connection has been disconnected, but can not actually be removed.
        // Either a static or a shared connection.
        internal bool Deleted {
            get {
                return _deleted;
            }
            set {
                _deleted = value;
            }
        }

        [
        DefaultValue("")
        ]
        public string ID {
            get {
                return (_id != null) ? _id : String.Empty;
            }
            set {
                _id = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool IsActive {
            get {
                return _isActive;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool IsShared {
            get {
                return _isShared;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool IsStatic {
            get {
                return _isStatic;
            }
        }

        // PERF: Consider caching
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public WebPart Provider {
            get {
                string providerID = ProviderID;
                if (providerID.Length == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_ProviderIDNotSet));
                }

                if (_webPartManager != null) {
                    return _webPartManager.WebParts[providerID];
                }
                else {
                    return null;
                }
            }
        }

        // PERF: Consider caching
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public ProviderConnectionPoint ProviderConnectionPoint {
            get {
                WebPart provider = Provider;
                if (provider != null && _webPartManager != null) {
                    return _webPartManager.GetProviderConnectionPoint(provider, ProviderConnectionPointID);
                }
                else {
                    return null;
                }
            }
        }

        [
        DefaultValue(ConnectionPoint.DefaultIDInternal)
        ]
        public string ProviderConnectionPointID {
            get {
                // 
                return (!String.IsNullOrEmpty(_providerConnectionPointID)) ?
                    _providerConnectionPointID : ConnectionPoint.DefaultID;
            }
            set {
                _providerConnectionPointID = value;
            }
        }

        [
        DefaultValue("")
        ]
        public string ProviderID {
            get {
                return (_providerID != null) ? _providerID : String.Empty;
            }
            set {
                _providerID = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public WebPartTransformer Transformer {
            get {
                if (_transformers == null || _transformers.Count == 0) {
                    return null;
                }
                else {
                    return _transformers[0];
                }
            }
        }

        /// <internalonly/>
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        PersistenceMode(PersistenceMode.InnerDefaultProperty),
        ]
        public WebPartTransformerCollection Transformers {
            get {
                if (_transformers == null) {
                    _transformers = new WebPartTransformerCollection();
                }

                return _transformers;
            }
        }

        internal void Activate() {
            // This method should only be called on WebPartConnections in the WebPartManager, so
            // _webPartManager should never be null.
            Debug.Assert(_webPartManager != null);

            Transformers.SetReadOnly();

            WebPart providerWebPart = Provider;
            // Cannot be null because Activate() is only called on valid Connections
            Debug.Assert(providerWebPart != null);

            WebPart consumerWebPart = Consumer;
            // Cannot be null because Activate() is only called on valid Connections
            Debug.Assert(consumerWebPart != null);

            Control providerControl = providerWebPart.ToControl();
            Control consumerControl = consumerWebPart.ToControl();

            ProviderConnectionPoint providerConnectionPoint = ProviderConnectionPoint;
            // Cannot be null because Activate() is only called on valid Connections
            Debug.Assert(providerConnectionPoint != null);

            if (!providerConnectionPoint.GetEnabled(providerControl)) {
                consumerWebPart.SetConnectErrorMessage(SR.GetString(SR.WebPartConnection_DisabledConnectionPoint, providerConnectionPoint.DisplayName, providerWebPart.DisplayTitle));
                return;
            }

            ConsumerConnectionPoint consumerConnectionPoint = ConsumerConnectionPoint;
            // Cannot be null because Activate() is only called on valid Connections
            Debug.Assert(consumerConnectionPoint != null);

            if (!consumerConnectionPoint.GetEnabled(consumerControl)) {
                consumerWebPart.SetConnectErrorMessage(SR.GetString(SR.WebPartConnection_DisabledConnectionPoint, consumerConnectionPoint.DisplayName, consumerWebPart.DisplayTitle));
                return;
            }

            // Do not activate connections involving closed WebParts
            if (!providerWebPart.IsClosed && !consumerWebPart.IsClosed) {
                WebPartTransformer transformer = Transformer;
                if (transformer == null) {
                    if (providerConnectionPoint.InterfaceType == consumerConnectionPoint.InterfaceType) {
                        ConnectionInterfaceCollection secondaryInterfaces = providerConnectionPoint.GetSecondaryInterfaces(providerControl);
                        if (consumerConnectionPoint.SupportsConnection(consumerControl, secondaryInterfaces)) {
                            object dataObject = providerConnectionPoint.GetObject(providerControl);
                            consumerConnectionPoint.SetObject(consumerControl, dataObject);
                            _isActive = true;
                        }
                        else {
                            consumerWebPart.SetConnectErrorMessage(SR.GetString(SR.WebPartConnection_IncompatibleSecondaryInterfaces, new string[] {
                                    consumerConnectionPoint.DisplayName, consumerWebPart.DisplayTitle,
                                    providerConnectionPoint.DisplayName, providerWebPart.DisplayTitle}));
                        }
                    }
                    else {
                        consumerWebPart.SetConnectErrorMessage(SR.GetString(SR.WebPartConnection_NoCommonInterface, new string[] {
                                providerConnectionPoint.DisplayName, providerWebPart.DisplayTitle,
                                consumerConnectionPoint.DisplayName, consumerWebPart.DisplayTitle}));
                    }
                }
                else {
                    Type transformerType = transformer.GetType();

                    if (!_webPartManager.AvailableTransformers.Contains(transformerType)) {
                        string errorMessage;
                        if (_webPartManager.Context != null && _webPartManager.Context.IsCustomErrorEnabled) {
                            errorMessage = SR.GetString(SR.WebPartConnection_TransformerNotAvailable);
                        }
                        else {
                            errorMessage = SR.GetString(SR.WebPartConnection_TransformerNotAvailableWithType, transformerType.FullName);
                        }
                        consumerWebPart.SetConnectErrorMessage(errorMessage);
                        
                        // 
                    }

                    // Check matching interfaces on connection points and transformer attribute
                    Type transformerConsumerType = WebPartTransformerAttribute.GetConsumerType(transformerType);
                    Type transformerProviderType = WebPartTransformerAttribute.GetProviderType(transformerType);

                    if (providerConnectionPoint.InterfaceType == transformerConsumerType &&
                        transformerProviderType == consumerConnectionPoint.InterfaceType) {

                        // A transformer never provides any secondary interfaces
                        if (consumerConnectionPoint.SupportsConnection(consumerControl, ConnectionInterfaceCollection.Empty)) {
                            object dataObject = providerConnectionPoint.GetObject(providerControl);
                            object transformedObject = transformer.Transform(dataObject);
                            consumerConnectionPoint.SetObject(consumerControl, transformedObject);
                            _isActive = true;
                        }
                        else {
                            consumerWebPart.SetConnectErrorMessage(SR.GetString(SR.WebPartConnection_ConsumerRequiresSecondaryInterfaces,
                                consumerConnectionPoint.DisplayName, consumerWebPart.DisplayTitle));
                        }
                    }
                    else if (providerConnectionPoint.InterfaceType != transformerConsumerType) {
                        string errorMessage;
                        if (_webPartManager.Context != null && _webPartManager.Context.IsCustomErrorEnabled) {
                            errorMessage = SR.GetString(SR.WebPartConnection_IncompatibleProviderTransformer,
                                providerConnectionPoint.DisplayName, providerWebPart.DisplayTitle);
                        }
                        else {
                            errorMessage = SR.GetString(SR.WebPartConnection_IncompatibleProviderTransformerWithType,
                                providerConnectionPoint.DisplayName, providerWebPart.DisplayTitle, transformerType.FullName);

                        }
                        consumerWebPart.SetConnectErrorMessage(errorMessage);
                    }
                    else {
                        string errorMessage;
                        if (_webPartManager.Context != null && _webPartManager.Context.IsCustomErrorEnabled) {
                            errorMessage = SR.GetString(SR.WebPartConnection_IncompatibleConsumerTransformer,
                                consumerConnectionPoint.DisplayName, consumerWebPart.DisplayTitle);
                        }
                        else {
                            errorMessage = SR.GetString(SR.WebPartConnection_IncompatibleConsumerTransformerWithType,
                                transformerType.FullName, consumerConnectionPoint.DisplayName, consumerWebPart.DisplayTitle);
                        }
                        consumerWebPart.SetConnectErrorMessage(errorMessage);
                    }
                }
            }
        }

        internal bool ConflictsWith(WebPartConnection otherConnection) {
            return (ConflictsWithConsumer(otherConnection) || ConflictsWithProvider(otherConnection));
        }

        internal bool ConflictsWithConsumer(WebPartConnection otherConnection) {
            return (!ConsumerConnectionPoint.AllowsMultipleConnections &&
                    Consumer == otherConnection.Consumer &&
                    ConsumerConnectionPoint == otherConnection.ConsumerConnectionPoint);
        }

        internal bool ConflictsWithProvider(WebPartConnection otherConnection) {
            return (!ProviderConnectionPoint.AllowsMultipleConnections &&
                    Provider == otherConnection.Provider &&
                    ProviderConnectionPoint == otherConnection.ProviderConnectionPoint);
        }

        internal void SetIsShared(bool isShared) {
            _isShared = isShared;
        }

        internal void SetIsStatic(bool isStatic) {
            _isStatic = isStatic;
        }

        internal void SetTransformer(WebPartTransformer transformer) {
            if (Transformers.Count == 0) {
                Transformers.Add(transformer);
            }
            else {
                Transformers[0] = transformer;
            }
        }

        internal void SetWebPartManager(WebPartManager webPartManager) {
            _webPartManager = webPartManager;
        }

        // Return the short typename, to improve the look of the collection editor in the designer
        public override string ToString () {
            return GetType().Name;
        }
    }
}

