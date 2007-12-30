//-----------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------
// MicrosoftAjaxWebForms.js
// Microsoft AJAX ASP.NET WebForms Framework.
Type.registerNamespace('Sys.WebForms');
Sys.WebForms.BeginRequestEventArgs = function Sys$WebForms$BeginRequestEventArgs(request, postBackElement) {
    /// <summary locid="M:J#Sys.WebForms.BeginRequestEventArgs.#ctor" />
    /// <param name="request" type="Sys.Net.WebRequest"></param>
    /// <param name="postBackElement" domElement="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "request", type: Sys.Net.WebRequest},
        {name: "postBackElement", mayBeNull: true, domElement: true}
    ]);
    if (e) throw e;
    Sys.WebForms.BeginRequestEventArgs.initializeBase(this);
    this._request = request;
    this._postBackElement = postBackElement;
}
    function Sys$WebForms$BeginRequestEventArgs$get_postBackElement() {
        /// <value domElement="true" mayBeNull="true" locid="P:J#Sys.WebForms.BeginRequestEventArgs.postBackElement"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._postBackElement;
    }
    function Sys$WebForms$BeginRequestEventArgs$get_request() {
        /// <value type="Sys.Net.WebRequest" locid="P:J#Sys.WebForms.BeginRequestEventArgs.request"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._request;
    }
Sys.WebForms.BeginRequestEventArgs.prototype = {
    get_postBackElement: Sys$WebForms$BeginRequestEventArgs$get_postBackElement,
    get_request: Sys$WebForms$BeginRequestEventArgs$get_request
}
Sys.WebForms.BeginRequestEventArgs.registerClass('Sys.WebForms.BeginRequestEventArgs', Sys.EventArgs);
 
Sys.WebForms.EndRequestEventArgs = function Sys$WebForms$EndRequestEventArgs(error, dataItems, response) {
    /// <summary locid="M:J#Sys.WebForms.EndRequestEventArgs.#ctor" />
    /// <param name="error" type="Error" mayBeNull="true"></param>
    /// <param name="dataItems" type="Object" mayBeNull="true"></param>
    /// <param name="response" type="Sys.Net.WebRequestExecutor"></param>
    var e = Function._validateParams(arguments, [
        {name: "error", type: Error, mayBeNull: true},
        {name: "dataItems", type: Object, mayBeNull: true},
        {name: "response", type: Sys.Net.WebRequestExecutor}
    ]);
    if (e) throw e;
    Sys.WebForms.EndRequestEventArgs.initializeBase(this);
    this._errorHandled = false;
    this._error = error;
    this._dataItems = dataItems || new Object();
    this._response = response;
}
    function Sys$WebForms$EndRequestEventArgs$get_dataItems() {
        /// <value type="Object" locid="P:J#Sys.WebForms.EndRequestEventArgs.dataItems"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._dataItems;
    }
    function Sys$WebForms$EndRequestEventArgs$get_error() {
        /// <value type="Error" locid="P:J#Sys.WebForms.EndRequestEventArgs.error"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._error;
    }
    function Sys$WebForms$EndRequestEventArgs$get_errorHandled() {
        /// <value type="Boolean" locid="P:J#Sys.WebForms.EndRequestEventArgs.errorHandled"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._errorHandled;
    }
    function Sys$WebForms$EndRequestEventArgs$set_errorHandled(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;
        this._errorHandled = value;
    }
    function Sys$WebForms$EndRequestEventArgs$get_response() {
        /// <value type="Sys.Net.WebRequestExecutor" locid="P:J#Sys.WebForms.EndRequestEventArgs.response"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._response;
    }
Sys.WebForms.EndRequestEventArgs.prototype = {
    get_dataItems: Sys$WebForms$EndRequestEventArgs$get_dataItems,
    get_error: Sys$WebForms$EndRequestEventArgs$get_error,
    get_errorHandled: Sys$WebForms$EndRequestEventArgs$get_errorHandled,
    set_errorHandled: Sys$WebForms$EndRequestEventArgs$set_errorHandled,
    get_response: Sys$WebForms$EndRequestEventArgs$get_response
}
Sys.WebForms.EndRequestEventArgs.registerClass('Sys.WebForms.EndRequestEventArgs', Sys.EventArgs);
Sys.WebForms.InitializeRequestEventArgs = function Sys$WebForms$InitializeRequestEventArgs(request, postBackElement) {
    /// <summary locid="M:J#Sys.WebForms.InitializeRequestEventArgs.#ctor" />
    /// <param name="request" type="Sys.Net.WebRequest"></param>
    /// <param name="postBackElement" domElement="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "request", type: Sys.Net.WebRequest},
        {name: "postBackElement", mayBeNull: true, domElement: true}
    ]);
    if (e) throw e;
    Sys.WebForms.InitializeRequestEventArgs.initializeBase(this);
    this._request = request;
    this._postBackElement = postBackElement;
}
    function Sys$WebForms$InitializeRequestEventArgs$get_postBackElement() {
        /// <value domElement="true" mayBeNull="true" locid="P:J#Sys.WebForms.InitializeRequestEventArgs.postBackElement"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._postBackElement;
    }
    function Sys$WebForms$InitializeRequestEventArgs$get_request() {
        /// <value type="Sys.Net.WebRequest" locid="P:J#Sys.WebForms.InitializeRequestEventArgs.request"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._request;
    }
Sys.WebForms.InitializeRequestEventArgs.prototype = {
    get_postBackElement: Sys$WebForms$InitializeRequestEventArgs$get_postBackElement,
    get_request: Sys$WebForms$InitializeRequestEventArgs$get_request
}
Sys.WebForms.InitializeRequestEventArgs.registerClass('Sys.WebForms.InitializeRequestEventArgs', Sys.CancelEventArgs);
 
Sys.WebForms.PageLoadedEventArgs = function Sys$WebForms$PageLoadedEventArgs(panelsUpdated, panelsCreated, dataItems) {
    /// <summary locid="M:J#Sys.WebForms.PageLoadedEventArgs.#ctor" />
    /// <param name="panelsUpdated" type="Array"></param>
    /// <param name="panelsCreated" type="Array"></param>
    /// <param name="dataItems" type="Object" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "panelsUpdated", type: Array},
        {name: "panelsCreated", type: Array},
        {name: "dataItems", type: Object, mayBeNull: true}
    ]);
    if (e) throw e;
    Sys.WebForms.PageLoadedEventArgs.initializeBase(this);
    this._panelsUpdated = panelsUpdated;
    this._panelsCreated = panelsCreated;
    this._dataItems = dataItems || new Object();
}
    function Sys$WebForms$PageLoadedEventArgs$get_dataItems() {
        /// <value type="Object" locid="P:J#Sys.WebForms.PageLoadedEventArgs.dataItems"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._dataItems;
    }
    function Sys$WebForms$PageLoadedEventArgs$get_panelsCreated() {
        /// <value type="Array" locid="P:J#Sys.WebForms.PageLoadedEventArgs.panelsCreated"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._panelsCreated;
    }
    function Sys$WebForms$PageLoadedEventArgs$get_panelsUpdated() {
        /// <value type="Array" locid="P:J#Sys.WebForms.PageLoadedEventArgs.panelsUpdated"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._panelsUpdated;
    }
Sys.WebForms.PageLoadedEventArgs.prototype = {
    get_dataItems: Sys$WebForms$PageLoadedEventArgs$get_dataItems,
    get_panelsCreated: Sys$WebForms$PageLoadedEventArgs$get_panelsCreated,
    get_panelsUpdated: Sys$WebForms$PageLoadedEventArgs$get_panelsUpdated
}
Sys.WebForms.PageLoadedEventArgs.registerClass('Sys.WebForms.PageLoadedEventArgs', Sys.EventArgs);
Sys.WebForms.PageLoadingEventArgs = function Sys$WebForms$PageLoadingEventArgs(panelsUpdating, panelsDeleting, dataItems) {
    /// <summary locid="M:J#Sys.WebForms.PageLoadingEventArgs.#ctor" />
    /// <param name="panelsUpdating" type="Array"></param>
    /// <param name="panelsDeleting" type="Array"></param>
    /// <param name="dataItems" type="Object" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "panelsUpdating", type: Array},
        {name: "panelsDeleting", type: Array},
        {name: "dataItems", type: Object, mayBeNull: true}
    ]);
    if (e) throw e;
    Sys.WebForms.PageLoadingEventArgs.initializeBase(this);
    this._panelsUpdating = panelsUpdating;
    this._panelsDeleting = panelsDeleting;
    this._dataItems = dataItems || new Object();
}
    function Sys$WebForms$PageLoadingEventArgs$get_dataItems() {
        /// <value type="Object" locid="P:J#Sys.WebForms.PageLoadingEventArgs.dataItems"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._dataItems;
    }
    function Sys$WebForms$PageLoadingEventArgs$get_panelsDeleting() {
        /// <value type="Array" locid="P:J#Sys.WebForms.PageLoadingEventArgs.panelsDeleting"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._panelsDeleting;
    }
    function Sys$WebForms$PageLoadingEventArgs$get_panelsUpdating() {
        /// <value type="Array" locid="P:J#Sys.WebForms.PageLoadingEventArgs.panelsUpdating"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._panelsUpdating;
    }
Sys.WebForms.PageLoadingEventArgs.prototype = {
    get_dataItems: Sys$WebForms$PageLoadingEventArgs$get_dataItems,
    get_panelsDeleting: Sys$WebForms$PageLoadingEventArgs$get_panelsDeleting,
    get_panelsUpdating: Sys$WebForms$PageLoadingEventArgs$get_panelsUpdating
}
Sys.WebForms.PageLoadingEventArgs.registerClass('Sys.WebForms.PageLoadingEventArgs', Sys.EventArgs);
 
Sys.WebForms.PageRequestManager = function Sys$WebForms$PageRequestManager() {
    this._form = null;
    this._activeDefaultButton = null;
    this._activeDefaultButtonClicked = false;
    this._updatePanelIDs = null;
    this._updatePanelClientIDs = null;
    this._oldUpdatePanelIDs = null;
    this._childUpdatePanelIDs = null;
    this._panelsToRefreshIDs = null;
    this._updatePanelHasChildrenAsTriggers = null;
    this._asyncPostBackControlIDs = null;
    this._asyncPostBackControlClientIDs = null;
    this._postBackControlIDs = null;
    this._postBackControlClientIDs = null;
    this._scriptManagerID = null;
    this._pageLoadedHandler = null;
    this._additionalInput = null;
    this._onsubmit = null;
    this._onSubmitStatements = [];
    this._originalDoPostBack = null;
    this._originalDoPostBackWithOptions = null;
    this._originalFireDefaultButton = null;
    this._originalDoCallback = null;
    this._isCrossPost = false;
    this._postBackSettings = null;
    this._request = null;
    this._onFormSubmitHandler = null;
    this._onFormElementClickHandler = null;
    this._onWindowUnloadHandler = null;
    this._asyncPostBackTimeout = null;
    this._controlIDToFocus = null;
    this._scrollPosition = null;
    this._dataItems = null;
    this._updateContext = null;
    this._processingRequest = false;
    this._scriptDisposes = {};
}
    function Sys$WebForms$PageRequestManager$_get_eventHandlerList() {
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }
    function Sys$WebForms$PageRequestManager$get_isInAsyncPostBack() {
        /// <value type="Boolean" locid="P:J#Sys.WebForms.PageRequestManager.isInAsyncPostBack"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._request !== null;
    }
    function Sys$WebForms$PageRequestManager$add_beginRequest(handler) {
        /// <summary locid="E:J#Sys.WebForms.PageRequestManager.beginRequest" />
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().addHandler("beginRequest", handler);
    }
    function Sys$WebForms$PageRequestManager$remove_beginRequest(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().removeHandler("beginRequest", handler);
    }
    function Sys$WebForms$PageRequestManager$add_endRequest(handler) {
        /// <summary locid="E:J#Sys.WebForms.PageRequestManager.endRequest" />
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().addHandler("endRequest", handler);
    }
    function Sys$WebForms$PageRequestManager$remove_endRequest(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().removeHandler("endRequest", handler);
    }
    function Sys$WebForms$PageRequestManager$add_initializeRequest(handler) {
        /// <summary locid="E:J#Sys.WebForms.PageRequestManager.initializeRequest" />
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().addHandler("initializeRequest", handler);
    }
    function Sys$WebForms$PageRequestManager$remove_initializeRequest(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().removeHandler("initializeRequest", handler);
    }
    function Sys$WebForms$PageRequestManager$add_pageLoaded(handler) {
        /// <summary locid="E:J#Sys.WebForms.PageRequestManager.pageLoaded" />
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().addHandler("pageLoaded", handler);
    }
    function Sys$WebForms$PageRequestManager$remove_pageLoaded(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().removeHandler("pageLoaded", handler);
    }
    function Sys$WebForms$PageRequestManager$add_pageLoading(handler) {
        /// <summary locid="E:J#Sys.WebForms.PageRequestManager.pageLoading" />
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().addHandler("pageLoading", handler);
    }
    function Sys$WebForms$PageRequestManager$remove_pageLoading(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
        this._get_eventHandlerList().removeHandler("pageLoading", handler);
    }
    function Sys$WebForms$PageRequestManager$abortPostBack() {
        if (!this._processingRequest && this._request) {
            this._request.get_executor().abort();
            this._request = null;
        }
    }
    function Sys$WebForms$PageRequestManager$_cancelPendingCallbacks() {
        for (var i = 0, l = window.__pendingCallbacks.length; i < l; i++) {
            var callback = window.__pendingCallbacks[i];
            if (callback) {
                if (!callback.async) {
                    window.__synchronousCallBackIndex = -1;
                }
                window.__pendingCallbacks[i] = null;
                var callbackFrameID = "__CALLBACKFRAME" + i;
                var xmlRequestFrame = document.getElementById(callbackFrameID);
                if (xmlRequestFrame) {
                    xmlRequestFrame.parentNode.removeChild(xmlRequestFrame);
                }
            }
        }
    }
    function Sys$WebForms$PageRequestManager$_createPageRequestManagerTimeoutError() {
        var displayMessage = "Sys.WebForms.PageRequestManagerTimeoutException: " + Sys.WebForms.Res.PRM_TimeoutError;
        var e = Error.create(displayMessage, {name: 'Sys.WebForms.PageRequestManagerTimeoutException'});
        e.popStackFrame();
        return e;
    }
    function Sys$WebForms$PageRequestManager$_createPageRequestManagerServerError(httpStatusCode, message) {
        var displayMessage = "Sys.WebForms.PageRequestManagerServerErrorException: " +
            (message || String.format(Sys.WebForms.Res.PRM_ServerError, httpStatusCode));
        var e = Error.create(displayMessage, {
            name: 'Sys.WebForms.PageRequestManagerServerErrorException',
            httpStatusCode: httpStatusCode
        });
        e.popStackFrame();
        return e;
    }
    function Sys$WebForms$PageRequestManager$_createPageRequestManagerParserError(parserErrorMessage) {
        var displayMessage = "Sys.WebForms.PageRequestManagerParserErrorException: " + String.format(Sys.WebForms.Res.PRM_ParserError, parserErrorMessage);
        var e = Error.create(displayMessage, {name: 'Sys.WebForms.PageRequestManagerParserErrorException'});
        e.popStackFrame();
        return e;
    }
    function Sys$WebForms$PageRequestManager$_createPostBackSettings(async, panelID, sourceElement) {
        return { async:async, panelID:panelID, sourceElement:sourceElement };
    }
    function Sys$WebForms$PageRequestManager$_convertToClientIDs(source, destinationIDs, destinationClientIDs) {
        if (source) {
            for (var i = 0; i < source.length; i++) {
                Array.add(destinationIDs, source[i]);
                Array.add(destinationClientIDs, this._uniqueIDToClientID(source[i]));
            }
        }
    }
    function Sys$WebForms$PageRequestManager$_destroyTree(element) {
        if (element.nodeType === 1) {
            var childNodes = element.childNodes;
            for (var i = childNodes.length - 1; i >= 0; i--) {
                var node = childNodes[i];
                if (node.nodeType === 1) {
                    if (node.dispose && typeof(node.dispose) === "function") {
                        node.dispose();
                    }
                    else if (node.control && typeof(node.control.dispose) === "function") {
                        node.control.dispose();
                    }
                    var behaviors = Sys.UI.Behavior.getBehaviors(node);
                    for (var j = behaviors.length - 1; j >= 0; j--) {
                        behaviors[j].dispose();
                    }
                    this._destroyTree(node);
                }
            }
        }
    }
    function Sys$WebForms$PageRequestManager$dispose() {
        if (this._form) {
            Sys.UI.DomEvent.removeHandler(this._form, 'submit', this._onFormSubmitHandler);
            Sys.UI.DomEvent.removeHandler(this._form, 'click', this._onFormElementClickHandler);
            Sys.UI.DomEvent.removeHandler(window, 'unload', this._onWindowUnloadHandler);
            Sys.UI.DomEvent.removeHandler(window, 'load', this._pageLoadedHandler);
        }
        if (this._originalDoPostBack) {
            window.__doPostBack = this._originalDoPostBack;
            this._originalDoPostBack = null;
        }
        if (this._originalDoPostBackWithOptions) {
            window.WebForm_DoPostBackWithOptions = this._originalDoPostBackWithOptions;
            this._originalDoPostBackWithOptions = null;
        }
        if (this._originalFireDefaultButton) {
            window.WebForm_FireDefaultButton = this._originalFireDefaultButton;
            this._originalFireDefaultButton = null;
        }
        if (this._originalDoCallback) {
            window.WebForm_DoCallback = this._originalDoCallback;
            this._originalDoCallback = null;
        }
        this._form = null;
        this._updatePanelIDs = null;
        this._oldUpdatePanelIDs = null;
        this._childUpdatePanelIDs = null;
        this._updatePanelClientIDs = null;
        this._asyncPostBackControlIDs = null;
        this._asyncPostBackControlClientIDs = null;
        this._postBackControlIDs = null;
        this._postBackControlClientIDs = null;
        this._asyncPostBackTimeout = null;
        this._scrollPosition = null;
        this._dataItems = null;
    }
    function Sys$WebForms$PageRequestManager$_doCallback(eventTarget, eventArgument, eventCallback, context, errorCallback, useAsync) {
        if (!this.get_isInAsyncPostBack()) {
            this._originalDoCallback(eventTarget, eventArgument, eventCallback, context, errorCallback, useAsync);
        }
    }
    function Sys$WebForms$PageRequestManager$_doPostBack(eventTarget, eventArgument) {
        this._additionalInput = null;
        var form = this._form;
        if ((eventTarget === null) || (typeof(eventTarget) === "undefined") || (this._isCrossPost)) {
            this._postBackSettings = this._createPostBackSettings(false, null, null);
            this._isCrossPost = false;
        }
        else {
            var clientID = this._uniqueIDToClientID(eventTarget);
            var postBackElement = document.getElementById(clientID);
            if (!postBackElement) {
                if (Array.contains(this._asyncPostBackControlIDs, eventTarget)) {
                    this._postBackSettings = this._createPostBackSettings(true, this._scriptManagerID + '|' + eventTarget, null);
                }
                else {
                    if (Array.contains(this._postBackControlIDs, eventTarget)) {
                        this._postBackSettings = this._createPostBackSettings(false, null, null);
                    }
                    else {
                        var nearestUniqueIDMatch = this._findNearestElement(eventTarget);
                        if (nearestUniqueIDMatch) {
                            this._postBackSettings = this._getPostBackSettings(nearestUniqueIDMatch, eventTarget);
                        }
                        else {
                            this._postBackSettings = this._createPostBackSettings(false, null, null);
                        }
                    }
                }
            }
            else {
                this._postBackSettings = this._getPostBackSettings(postBackElement, eventTarget);
            }
        }
        if (!this._postBackSettings.async) {
            form.onsubmit = this._onsubmit;
            this._originalDoPostBack(eventTarget, eventArgument);
            form.onsubmit = null;
            return;
        }
        form.__EVENTTARGET.value = eventTarget;
        form.__EVENTARGUMENT.value = eventArgument;
        this._onFormSubmit();
    }
    function Sys$WebForms$PageRequestManager$_doPostBackWithOptions(options) {
        this._isCrossPost = options && options.actionUrl;
        this._originalDoPostBackWithOptions(options);
    }
    function Sys$WebForms$PageRequestManager$_elementContains(container, element) {
        while (element) {
            if (element === container) {
                return true;
            }
            element = element.parentNode;
        }
        return false;
    }
    function Sys$WebForms$PageRequestManager$_endPostBack(error, response) {
        if (this._request === response.get_webRequest()) {
            this._processingRequest = false;
            this._additionalInput = null;
            this._request = null;
        }
        var handler = this._get_eventHandlerList().getHandler("endRequest");
        var errorHandled = false;
        if (handler) {
            var eventArgs = new Sys.WebForms.EndRequestEventArgs(error, this._dataItems, response);
            handler(this, eventArgs);
            errorHandled = eventArgs.get_errorHandled();
        }
        if (!this._processingRequest) {
            this._dataItems = null;
        }
        if (error && !errorHandled) {
            throw error;
        }
    }
    function Sys$WebForms$PageRequestManager$_findNearestElement(uniqueID) {
        while (uniqueID.length > 0) {
            var clientID = this._uniqueIDToClientID(uniqueID);
            var element = document.getElementById(clientID);
            if (element) {
                return element;
            }
            var indexOfLastDollar = uniqueID.lastIndexOf('$');
            if (indexOfLastDollar === -1) {
                return null;
            }
            uniqueID = uniqueID.substring(0, indexOfLastDollar);
        }
        return null;
    }
    function Sys$WebForms$PageRequestManager$_findText(text, location) {
        var startIndex = Math.max(0, location - 20);
        var endIndex = Math.min(text.length, location + 20);
        return text.substring(startIndex, endIndex);
    }
    function Sys$WebForms$PageRequestManager$_fireDefaultButton(event, target) {
        if ((event.keyCode === 13) && !(event.srcElement && (event.srcElement.tagName.toLowerCase() === "textarea"))) {
            var defaultButton = document.getElementById(target);
            if (defaultButton && (typeof(defaultButton.click) !== "undefined")) {
                
                
                this._activeDefaultButton = defaultButton;
                this._activeDefaultButtonClicked = false;
                try {
                    defaultButton.click();
                }
                finally {
                    this._activeDefaultButton = null;
                }
                
                
                event.cancelBubble = true;
                if (typeof(event.stopPropagation) === "function") {
                    event.stopPropagation();
                }
                return false;
            }
        }
        return true;
    }
    function Sys$WebForms$PageRequestManager$_getPageLoadedEventArgs(initialLoad) {
        var updated = [];
        var created = [];
        var oldIDs = this._oldUpdatePanelIDs || []; 
        var newIDs = this._updatePanelIDs; 
        var childIDs = this._childUpdatePanelIDs || []; 
        var refreshedIDs = this._panelsToRefreshIDs || []; 
        for (var i = 0; i < refreshedIDs.length; i++) {
            Array.add(updated, document.getElementById(this._uniqueIDToClientID(refreshedIDs[i])));
        }
        for (var i = 0; i < newIDs.length; i++) {
            if (initialLoad || Array.indexOf(childIDs, newIDs[i]) !== -1) {
                Array.add(created, document.getElementById(this._uniqueIDToClientID(newIDs[i])));
            }
        }
        return new Sys.WebForms.PageLoadedEventArgs(updated, created, this._dataItems);
    }
    function Sys$WebForms$PageRequestManager$_getPageLoadingEventArgs() {
        var updated = [];
        var deleted = [];
        var oldIDs = this._oldUpdatePanelIDs;
        var newIDs = this._updatePanelIDs;
        var childIDs = this._childUpdatePanelIDs;
        var refreshedIDs = this._panelsToRefreshIDs;
        for (var i = 0; i < refreshedIDs.length; i++) {
            Array.add(updated, document.getElementById(this._uniqueIDToClientID(refreshedIDs[i])));
        }
        for (var i = 0; i < oldIDs.length; i++) {
            if (Array.indexOf(refreshedIDs, oldIDs[i]) === -1 &&
                (Array.indexOf(newIDs, oldIDs[i]) === -1 || Array.indexOf(childIDs, oldIDs[i]) > -1)) {
                Array.add(deleted, document.getElementById(this._uniqueIDToClientID(oldIDs[i])));
            }
        }
        return new Sys.WebForms.PageLoadingEventArgs(updated, deleted, this._dataItems);
    }
    function Sys$WebForms$PageRequestManager$_getPostBackSettings(element, elementUniqueID) {
        var originalElement = element;
        var proposedSettings = null;
        while (element) {
            if (element.id) {
                if (!proposedSettings && Array.contains(this._asyncPostBackControlClientIDs, element.id)) {
                    proposedSettings = this._createPostBackSettings(true, this._scriptManagerID + '|' + elementUniqueID, originalElement);
                }
                else {
                    if (!proposedSettings && Array.contains(this._postBackControlClientIDs, element.id)) {
                        return this._createPostBackSettings(false, null, null);
                    }
                    else {
                        var indexOfPanel = Array.indexOf(this._updatePanelClientIDs, element.id);
                        if (indexOfPanel !== -1) {
                            if (this._updatePanelHasChildrenAsTriggers[indexOfPanel]) {
                                return this._createPostBackSettings(true, this._updatePanelIDs[indexOfPanel] + '|' + elementUniqueID, originalElement);
                            }
                            else {
                                return this._createPostBackSettings(true, this._scriptManagerID + '|' + elementUniqueID, originalElement);
                            }
                        }
                    }
                }
                if (!proposedSettings && this._matchesParentIDInList(element.id, this._asyncPostBackControlClientIDs)) {
                    proposedSettings = this._createPostBackSettings(true, this._scriptManagerID + '|' + elementUniqueID, originalElement);
                }
                else {
                    if (!proposedSettings && this._matchesParentIDInList(element.id, this._postBackControlClientIDs)) {
                        return this._createPostBackSettings(false, null, null);
                    }
                }
            }
            element = element.parentNode;
        }
        if (!proposedSettings) {
            return this._createPostBackSettings(false, null, null);
        }
        else {
            return proposedSettings;
        }
    }
    function Sys$WebForms$PageRequestManager$_getScrollPosition() {
        var d = document.documentElement;
        if (d && (this._validPosition(d.scrollLeft) || this._validPosition(d.scrollTop))) {
            return {
                x: d.scrollLeft,
                y: d.scrollTop
            };
        }
        else {
            d = document.body;
            if (d && (this._validPosition(d.scrollLeft) || this._validPosition(d.scrollTop))) {
                return {
                    x: d.scrollLeft,
                    y: d.scrollTop
                };
            }
            else {
                if (this._validPosition(window.pageXOffset) || this._validPosition(window.pageYOffset)) {
                    return {
                        x: window.pageXOffset,
                        y: window.pageYOffset
                    };
                }
                else {
                    return {
                        x: 0,
                        y: 0
                    };
                }
            }
        }
    }
    function Sys$WebForms$PageRequestManager$_initializeInternal(scriptManagerID, formElement) {
        if (this._prmInitialized) {
            throw Error.invalidOperation(Sys.WebForms.Res.PRM_CannotRegisterTwice);
        }
        this._prmInitialized = true;
        this._scriptManagerID = scriptManagerID;
        this._form = formElement;
        this._onsubmit = this._form.onsubmit;
        this._form.onsubmit = null;
        this._onFormSubmitHandler = Function.createDelegate(this, this._onFormSubmit);
        this._onFormElementClickHandler = Function.createDelegate(this, this._onFormElementClick);
        this._onWindowUnloadHandler = Function.createDelegate(this, this._onWindowUnload);
        Sys.UI.DomEvent.addHandler(this._form, 'submit', this._onFormSubmitHandler);
        Sys.UI.DomEvent.addHandler(this._form, 'click', this._onFormElementClickHandler);
        Sys.UI.DomEvent.addHandler(window, 'unload', this._onWindowUnloadHandler);
        this._originalDoPostBack = window.__doPostBack;
        if (this._originalDoPostBack) {
            window.__doPostBack = Function.createDelegate(this, this._doPostBack);
        }
        this._originalDoPostBackWithOptions = window.WebForm_DoPostBackWithOptions;
        if (this._originalDoPostBackWithOptions) {
            window.WebForm_DoPostBackWithOptions = Function.createDelegate(this, this._doPostBackWithOptions);
        }
        this._originalFireDefaultButton = window.WebForm_FireDefaultButton;
        if (this._originalFireDefaultButton) {
            window.WebForm_FireDefaultButton = Function.createDelegate(this, this._fireDefaultButton);
        }
        this._originalDoCallback = window.WebForm_DoCallback;
        if (this._originalDoCallback) {
            window.WebForm_DoCallback = Function.createDelegate(this, this._doCallback);
        }
        this._pageLoadedHandler = Function.createDelegate(this, this._pageLoadedInitialLoad);
        Sys.UI.DomEvent.addHandler(window, 'load', this._pageLoadedHandler);
    }
    function Sys$WebForms$PageRequestManager$_matchesParentIDInList(clientID, parentIDList) {
        for (var i = 0; i < parentIDList.length; i++) {
            if (clientID.startsWith(parentIDList[i] + "_")) {
                return true;
            }
        }
        return false;
    }
    function Sys$WebForms$PageRequestManager$_onFormElementActive(element, offsetX, offsetY) {
        if (element.disabled) {
            return;
        }
        this._postBackSettings = this._getPostBackSettings(element, element.name);
        if (element.name) {
            if (element.tagName === 'INPUT') {
                var type = element.type;
                if (type === 'submit') {
                    this._additionalInput = encodeURIComponent(element.name) + '=' + encodeURIComponent(element.value);
                }
                else if (type === 'image') {
                    this._additionalInput = encodeURIComponent(element.name) + '.x=' + offsetX + '&' + encodeURIComponent(element.name) + '.y=' + offsetY;
                }
            }
            else if ((element.tagName === 'BUTTON') && (element.name.length !== 0) && (element.type === 'submit')) {
                this._additionalInput = encodeURIComponent(element.name) + '=' + encodeURIComponent(element.value);
            }
        }
    }
    function Sys$WebForms$PageRequestManager$_onFormElementClick(evt) {
        this._activeDefaultButtonClicked = (evt.target === this._activeDefaultButton);
        this._onFormElementActive(evt.target, evt.offsetX, evt.offsetY);
    }
    function Sys$WebForms$PageRequestManager$_onFormSubmit(evt) {
        var continueSubmit = true;
        var isCrossPost = this._isCrossPost;
        this._isCrossPost = false;
        if (this._onsubmit) {
            continueSubmit = this._onsubmit();
        }
        if (continueSubmit) {
            for (var i = 0; i < this._onSubmitStatements.length; i++) {
                if (!this._onSubmitStatements[i]()) {
                    continueSubmit = false;
                    break;
                }
            }
        }
        if (!continueSubmit) {
            if (evt) {
                evt.preventDefault();
            }
            return;
        }
        var form = this._form;
        if (isCrossPost) {
            return;
        }
        if (this._activeDefaultButton && !this._activeDefaultButtonClicked) {
            this._onFormElementActive(this._activeDefaultButton, 0, 0);
        }
        if (!this._postBackSettings.async) {
            return;
        }
        var formBody = new Sys.StringBuilder();
        formBody.append(encodeURIComponent(this._scriptManagerID) + '=' + encodeURIComponent(this._postBackSettings.panelID) + '&');
        var count = form.elements.length;
        for (var i = 0; i < count; i++) {
            var element = form.elements[i];
            var name = element.name;
            if (typeof(name) === "undefined" || (name === null) || (name.length === 0)) {
                continue;
            }
            var tagName = element.tagName;
            if (tagName === 'INPUT') {
                var type = element.type;
                if ((type === 'text') ||
                    (type === 'password') ||
                    (type === 'hidden') ||
                    (((type === 'checkbox') || (type === 'radio')) && element.checked)) {
                    formBody.append(encodeURIComponent(name));
                    formBody.append('=');
                    formBody.append(encodeURIComponent(element.value));
                    formBody.append('&');
                }
            }
            else if (tagName === 'SELECT') {
                var optionCount = element.options.length;
                for (var j = 0; j < optionCount; j++) {
                    var option = element.options[j];
                    if (option.selected) {
                        formBody.append(encodeURIComponent(name));
                        formBody.append('=');
                        formBody.append(encodeURIComponent(option.value));
                        formBody.append('&');
                    }
                }
            }
            else if (tagName === 'TEXTAREA') {
                formBody.append(encodeURIComponent(name));
                formBody.append('=');
                formBody.append(encodeURIComponent(element.value));
                formBody.append('&');
            }
        }
        if (this._additionalInput) {
            formBody.append(this._additionalInput);
            this._additionalInput = null;
        }
        
        var request = new Sys.Net.WebRequest();
        var action = form.action;
        if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
            var queryIndex = action.indexOf('?');
            if (queryIndex !== -1) {
                var path = action.substr(0, queryIndex);
                if (path.indexOf("%") === -1) {
                    action = encodeURI(path) + action.substr(queryIndex);
                }
            }
            else if (action.indexOf("%") === -1) {
                action = encodeURI(action);
            }
        }
        request.set_url(action);
        request.get_headers()['X-MicrosoftAjax'] = 'Delta=true';
        request.get_headers()['Cache-Control'] = 'no-cache';
        request.set_timeout(this._asyncPostBackTimeout);
        request.add_completed(Function.createDelegate(this, this._onFormSubmitCompleted));
        request.set_body(formBody.toString());
        var handler = this._get_eventHandlerList().getHandler("initializeRequest");
        if (handler) {
            var eventArgs = new Sys.WebForms.InitializeRequestEventArgs(request, this._postBackSettings.sourceElement);
            handler(this, eventArgs);
            continueSubmit = !eventArgs.get_cancel();
        }
        if (!continueSubmit) {
            if (evt) {
                evt.preventDefault();
            }
            return;
        }
        this._scrollPosition = this._getScrollPosition();
        this.abortPostBack();
        handler = this._get_eventHandlerList().getHandler("beginRequest");
        if (handler) {
            var eventArgs = new Sys.WebForms.BeginRequestEventArgs(request, this._postBackSettings.sourceElement);
            handler(this, eventArgs);
        }
        
        if (this._originalDoCallback) {
            this._cancelPendingCallbacks();
        }
        this._request = request;
        request.invoke();
        if (evt) {
            evt.preventDefault();
        }
    }
    function Sys$WebForms$PageRequestManager$_onFormSubmitCompleted(sender, eventArgs) {
        this._processingRequest = true;
        var delimitByLengthDelimiter = '|';
        if (sender.get_timedOut()) {
            this._endPostBack(this._createPageRequestManagerTimeoutError(), sender);
            return;
        }
        if (sender.get_aborted()) {
            this._endPostBack(null, sender);
            return;
        }
        if (!this._request || sender.get_webRequest() !== this._request) {
            return;
        }
        var errorMessage;
        var delta = [];
        if (sender.get_statusCode() !== 200) {
            this._endPostBack(this._createPageRequestManagerServerError(sender.get_statusCode()), sender);
            return;
        }
        var reply = sender.get_responseData();
        var delimiterIndex, len, type, id, content;
        var replyIndex = 0;
        var parserErrorDetails = null;
        while (replyIndex < reply.length) {
            delimiterIndex = reply.indexOf(delimitByLengthDelimiter, replyIndex);
            if (delimiterIndex === -1) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }
            len = parseInt(reply.substring(replyIndex, delimiterIndex), 10);
            if ((len % 1) !== 0) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }
            replyIndex = delimiterIndex + 1;
            delimiterIndex = reply.indexOf(delimitByLengthDelimiter, replyIndex);
            if (delimiterIndex === -1) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }
            type = reply.substring(replyIndex, delimiterIndex);
            replyIndex = delimiterIndex + 1;
            delimiterIndex = reply.indexOf(delimitByLengthDelimiter, replyIndex);
            if (delimiterIndex === -1) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }
            id = reply.substring(replyIndex, delimiterIndex);
            replyIndex = delimiterIndex + 1;
            if ((replyIndex + len) >= reply.length) {
                parserErrorDetails = this._findText(reply, reply.length);
                break;
            }
            content = reply.substr(replyIndex, len);
            replyIndex += len;
            if (reply.charAt(replyIndex) !== delimitByLengthDelimiter) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }
            replyIndex++;
            Array.add(delta, {type: type, id: id, content: content});
        }
        if (parserErrorDetails) {
            this._endPostBack(this._createPageRequestManagerParserError(String.format(Sys.WebForms.Res.PRM_ParserErrorDetails, parserErrorDetails)), sender);
            return;
        }
        var updatePanelNodes = [];
        var hiddenFieldNodes = [];
        var arrayDeclarationNodes = [];
        var scriptBlockNodes = [];
        var scriptStartupNodes = [];
        var expandoNodes = [];
        var onSubmitNodes = [];
        var dataItemNodes = [];
        var dataItemJsonNodes = [];
        var scriptDisposeNodes = [];
        var asyncPostBackControlIDsNode, postBackControlIDsNode,
            updatePanelIDsNode, asyncPostBackTimeoutNode,
            childUpdatePanelIDsNode, panelsToRefreshNode, formActionNode;
        for (var i = 0; i < delta.length; i++) {
            var deltaNode = delta[i];
            switch (deltaNode.type) {
                case "updatePanel":
                    Array.add(updatePanelNodes, deltaNode);
                    break;
                case "hiddenField":
                    Array.add(hiddenFieldNodes, deltaNode);
                    break;
                case "arrayDeclaration":
                    Array.add(arrayDeclarationNodes, deltaNode);
                    break;
                case "scriptBlock":
                    Array.add(scriptBlockNodes, deltaNode);
                    break;
                case "scriptStartupBlock":
                    Array.add(scriptStartupNodes, deltaNode);
                    break;
                case "expando":
                    Array.add(expandoNodes, deltaNode);
                    break;
                case "onSubmit":
                    Array.add(onSubmitNodes, deltaNode);
                    break;
                case "asyncPostBackControlIDs":
                    asyncPostBackControlIDsNode = deltaNode;
                    break;
                case "postBackControlIDs":
                    postBackControlIDsNode = deltaNode;
                    break;
                case "updatePanelIDs":
                    updatePanelIDsNode = deltaNode;
                    break;
                case "asyncPostBackTimeout":
                    asyncPostBackTimeoutNode = deltaNode;
                    break;
                case "childUpdatePanelIDs":
                    childUpdatePanelIDsNode = deltaNode;
                    break;
                case "panelsToRefreshIDs":
                    panelsToRefreshNode = deltaNode;
                    break;
                case "formAction":
                    formActionNode = deltaNode;
                    break;
                case "dataItem":
                    Array.add(dataItemNodes, deltaNode);
                    break;
                case "dataItemJson":
                    Array.add(dataItemJsonNodes, deltaNode);
                    break;
                case "scriptDispose":
                    Array.add(scriptDisposeNodes, deltaNode);
                    break;
                case "pageRedirect":
                    if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
                        var anchor = document.createElement("a");
                        anchor.style.display = 'none';
                        anchor.attachEvent("onclick", cancelBubble);
                        anchor.href = deltaNode.content;
                        document.body.appendChild(anchor);
                        anchor.click();
                        anchor.detachEvent("onclick", cancelBubble);
                        document.body.removeChild(anchor);
                        
                        function cancelBubble(e) {
                            e.cancelBubble = true;
                        }
                    }
                    else {
                        window.location.href = deltaNode.content;
                    }
                    return;
                case "error":
                    this._endPostBack(this._createPageRequestManagerServerError(Number.parseInvariant(deltaNode.id), deltaNode.content), sender);
                    return;
                case "pageTitle":
                    document.title = deltaNode.content;
                    break;
                case "focus":
                    this._controlIDToFocus = deltaNode.content;
                    break;
                default:
                    this._endPostBack(this._createPageRequestManagerParserError(String.format(Sys.WebForms.Res.PRM_UnknownToken, deltaNode.type)), sender);
                    return;
            }
        }
        var i;
        if (asyncPostBackControlIDsNode && postBackControlIDsNode &&
            updatePanelIDsNode && panelsToRefreshNode &&
            asyncPostBackTimeoutNode && childUpdatePanelIDsNode) {
            this._oldUpdatePanelIDs = this._updatePanelIDs;
            var childUpdatePanelIDsString = childUpdatePanelIDsNode.content;
            this._childUpdatePanelIDs = childUpdatePanelIDsString.length ? childUpdatePanelIDsString.split(',') : [];
            var asyncPostBackControlIDsArray = this._splitNodeIntoArray(asyncPostBackControlIDsNode);
            var postBackControlIDsArray = this._splitNodeIntoArray(postBackControlIDsNode);
            var updatePanelIDsArray = this._splitNodeIntoArray(updatePanelIDsNode);
            this._panelsToRefreshIDs = this._splitNodeIntoArray(panelsToRefreshNode);
            for (i = 0; i < this._panelsToRefreshIDs.length; i++) {
                var panelClientID = this._uniqueIDToClientID(this._panelsToRefreshIDs[i]);
                if (!document.getElementById(panelClientID)) {
                    this._endPostBack(Error.invalidOperation(String.format(Sys.WebForms.Res.PRM_MissingPanel, panelClientID)), sender);
                    return;
                }
            }
            var asyncPostBackTimeout = asyncPostBackTimeoutNode.content;
            this._updateControls(updatePanelIDsArray, asyncPostBackControlIDsArray, postBackControlIDsArray, asyncPostBackTimeout);
        }
        this._dataItems = {};
        for (i = 0; i < dataItemNodes.length; i++) {
            var dataItemNode = dataItemNodes[i];
            this._dataItems[dataItemNode.id] = dataItemNode.content;
        }
        for (i = 0; i < dataItemJsonNodes.length; i++) {
            var dataItemJsonNode = dataItemJsonNodes[i];
            this._dataItems[dataItemJsonNode.id] = Sys.Serialization.JavaScriptSerializer.deserialize(dataItemJsonNode.content);
        }
        var handler = this._get_eventHandlerList().getHandler("pageLoading");
        if (handler) {
            handler(this, this._getPageLoadingEventArgs());
        }
        if (formActionNode) {
            this._form.action = formActionNode.content;
        }
        
        
        Sys._ScriptLoader.readLoadedScripts();
        Sys.Application.beginCreateComponents();
        var scriptLoader = Sys._ScriptLoader.getInstance();
        this._queueScripts(scriptLoader, scriptBlockNodes, true, false);
        
        this._updateContext = {
            response: sender,
            updatePanelNodes: updatePanelNodes,
            scriptBlockNodes: scriptBlockNodes,
            scriptDisposeNodes: scriptDisposeNodes,
            hiddenFieldNodes: hiddenFieldNodes,
            arrayDeclarationNodes: arrayDeclarationNodes,
            expandoNodes: expandoNodes,
            scriptStartupNodes: scriptStartupNodes,
            onSubmitNodes: onSubmitNodes
        };
        scriptLoader.loadScripts(0,
            Function.createDelegate(this, this._scriptIncludesLoadComplete),
            Function.createDelegate(this, this._scriptIncludesLoadFailed),
            null);
    }
    function Sys$WebForms$PageRequestManager$_onWindowUnload(evt) {
        this.dispose();
    }
    function Sys$WebForms$PageRequestManager$_pageLoaded(initialLoad) {
        var handler = this._get_eventHandlerList().getHandler("pageLoaded");
        if (handler) {
            handler(this, this._getPageLoadedEventArgs(initialLoad));
        }
        if (!initialLoad) {
            Sys.Application.raiseLoad();
        }
    }
    function Sys$WebForms$PageRequestManager$_pageLoadedInitialLoad(evt) {
        this._pageLoaded(true);
    }
    function Sys$WebForms$PageRequestManager$_queueScripts(scriptLoader, scriptBlockNodes, queueIncludes, queueBlocks) {
        
        for (i = 0; i < scriptBlockNodes.length; i++) {
            var scriptBlockType = scriptBlockNodes[i].id;
            switch (scriptBlockType) {
                case "ScriptContentNoTags":
                    if (!queueBlocks) {
                        continue;
                    }
                    scriptLoader.queueScriptBlock(scriptBlockNodes[i].content);
                    break;
                case "ScriptContentWithTags":
                    var scriptTagAttributes;
                    eval("scriptTagAttributes = " + scriptBlockNodes[i].content);
                    if (scriptTagAttributes.src) {
                        if (!queueIncludes || Sys._ScriptLoader.isScriptLoaded(scriptTagAttributes.src)) {
                            continue;
                        }
                    }
                    else if (!queueBlocks) {
                        continue;
                    }
                    scriptLoader.queueCustomScriptTag(scriptTagAttributes);
                    break;
                case "ScriptPath":
                    if (!queueIncludes || Sys._ScriptLoader.isScriptLoaded(scriptBlockNodes[i].content)) {
                        continue;
                    }
                    scriptLoader.queueScriptReference(scriptBlockNodes[i].content);
                    break;
            }
        }        
    }
    function Sys$WebForms$PageRequestManager$_registerDisposeScript(panelID, disposeScript) {
        if (!this._scriptDisposes[panelID]) {
            this._scriptDisposes[panelID] = [disposeScript];
        }
        else {
            Array.add(this._scriptDisposes[panelID], disposeScript);
        }
    }
    function Sys$WebForms$PageRequestManager$_scriptIncludesLoadComplete() {
        var ctx = this._updateContext;
        for (i = 0; i < ctx.updatePanelNodes.length; i++) {
            var deltaUpdatePanel = ctx.updatePanelNodes[i];
            var deltaPanelID = deltaUpdatePanel.id;
            var deltaPanelRendering = deltaUpdatePanel.content;
            var updatePanelElement = document.getElementById(deltaPanelID);
            if (!updatePanelElement) {
                this._endPostBack(Error.invalidOperation(String.format(Sys.WebForms.Res.PRM_MissingPanel, deltaPanelID)), ctx.response);
                return;
            }
            this._updatePanel(updatePanelElement, deltaPanelRendering);
        }
        for (i = 0; i < ctx.scriptDisposeNodes.length; i++) {
            var disposePanelId = ctx.scriptDisposeNodes[i].id;
            var disposeScript = ctx.scriptDisposeNodes[i].content;
            this._registerDisposeScript(disposePanelId, disposeScript);
        }
        var viewStateEncrypted = false;
        for (i = 0; i < ctx.hiddenFieldNodes.length; i++) {
            var id = ctx.hiddenFieldNodes[i].id;
            var value = ctx.hiddenFieldNodes[i].content;
            
            if (id === "__VIEWSTATEENCRYPTED") {
                viewStateEncrypted = true;
            }
            var hiddenFieldElement = document.getElementById(id);
            if (!hiddenFieldElement) {
                hiddenFieldElement = document.createElement('input');
                hiddenFieldElement.id = id;
                hiddenFieldElement.name = id;
                hiddenFieldElement.type = 'hidden';
                this._form.appendChild(hiddenFieldElement);
            }
            hiddenFieldElement.value = value;
        }
        
        if (!viewStateEncrypted) {
            var viewStateEncryptedField = document.getElementById("__VIEWSTATEENCRYPTED");
            if (viewStateEncryptedField) {
                viewStateEncryptedField.parentNode.removeChild(viewStateEncryptedField);
            }
        }
        if (ctx.scriptsFailed) {
            throw Sys._ScriptLoader._errorScriptLoadFailed(ctx.scriptsFailed.src, ctx.scriptsFailed.multipleCallbacks);
        }
        
        var scriptLoader = Sys._ScriptLoader.getInstance();
        
        this._queueScripts(scriptLoader, ctx.scriptBlockNodes, false, true);
        var arrayScript = '';
        for (i = 0; i < ctx.arrayDeclarationNodes.length; i++) {
            arrayScript += "Sys.WebForms.PageRequestManager._addArrayElement('" + ctx.arrayDeclarationNodes[i].id + "', " + ctx.arrayDeclarationNodes[i].content + ");\r\n";
        }
        var expandoScript = '';
        for (i = 0; i < ctx.expandoNodes.length; i++) {
            var propertyReference = ctx.expandoNodes[i].id;
            var propertyValue = ctx.expandoNodes[i].content;
            expandoScript += propertyReference + " = " + propertyValue + "\r\n";
        }
        if (arrayScript.length) {
            scriptLoader.queueScriptBlock(arrayScript);
        }
        if (expandoScript.length) {
            scriptLoader.queueScriptBlock(expandoScript);
        }
        
        this._queueScripts(scriptLoader, ctx.scriptStartupNodes, true, true);
        var onSubmitStatementScript = '';
        for (var i = 0; i < ctx.onSubmitNodes.length; i++) {
            if (i === 0) {
                onSubmitStatementScript = 'Array.add(Sys.WebForms.PageRequestManager.getInstance()._onSubmitStatements, function() {\r\n';
            }
            onSubmitStatementScript += ctx.onSubmitNodes[i].content + "\r\n";
        }
        if (onSubmitStatementScript.length) {
            onSubmitStatementScript += "\r\nreturn true;\r\n});\r\n";
            scriptLoader.queueScriptBlock(onSubmitStatementScript);
        }
        scriptLoader.loadScripts(0, Function.createDelegate(this, this._scriptsLoadComplete), null, null);
    }
    function Sys$WebForms$PageRequestManager$_scriptIncludesLoadFailed(scriptLoader, scriptElement, multipleCallbacks) {
        this._updateContext.scriptsFailed = { src: scriptElement.src, multipleCallbacks: multipleCallbacks };
        this._scriptIncludesLoadComplete();
    }
    function Sys$WebForms$PageRequestManager$_scriptsLoadComplete() {
        var response = this._updateContext.response;
        this._updateContext = null;
        if (window.__theFormPostData) {
            window.__theFormPostData = "";
        }
        if (window.__theFormPostCollection) {
            window.__theFormPostCollection = [];
        }
        if (window.WebForm_InitCallback) {
            window.WebForm_InitCallback();
        }
        if (this._scrollPosition) {
            if (window.scrollTo) {
                window.scrollTo(this._scrollPosition.x, this._scrollPosition.y);
            }
            this._scrollPosition = null;
        }
        Sys.Application.endCreateComponents();
        this._pageLoaded(false);
        this._endPostBack(null, response);
        if (this._controlIDToFocus) {
            var focusTarget;
            var oldContentEditableSetting;
            if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
                var targetControl = $get(this._controlIDToFocus);
                focusTarget = targetControl;
                if (targetControl && (!WebForm_CanFocus(targetControl))) {
                    focusTarget = WebForm_FindFirstFocusableChild(targetControl);
                }
                if (focusTarget && (typeof(focusTarget.contentEditable) !== "undefined")) {
                    oldContentEditableSetting = focusTarget.contentEditable;
                    focusTarget.contentEditable = false;
                }
                else {
                    focusTarget = null;
                }
            }
            WebForm_AutoFocus(this._controlIDToFocus);
            if (focusTarget) {
                focusTarget.contentEditable = oldContentEditableSetting;
            }
            this._controlIDToFocus = null;
        }
    }
    function Sys$WebForms$PageRequestManager$_splitNodeIntoArray(node) {
        var str = node.content;
        var arr = str.length ? str.split(',') : [];
        return arr;
    }
    function Sys$WebForms$PageRequestManager$_uniqueIDToClientID(uniqueID) {
        return uniqueID.replace(/\$/g, '_');
    }
    function Sys$WebForms$PageRequestManager$_updateControls(updatePanelIDs, asyncPostBackControlIDs, postBackControlIDs, asyncPostBackTimeout) {
        if (updatePanelIDs) {
            this._updatePanelIDs = new Array(updatePanelIDs.length);
            this._updatePanelClientIDs = new Array(updatePanelIDs.length);
            this._updatePanelHasChildrenAsTriggers = new Array(updatePanelIDs.length);
            for (var i = 0; i < updatePanelIDs.length; i++) {
                var realPanelID = updatePanelIDs[i].substr(1);
                var childrenAsTriggers = (updatePanelIDs[i].charAt(0) === 't');
                this._updatePanelHasChildrenAsTriggers[i] = childrenAsTriggers;
                this._updatePanelIDs[i] = realPanelID;
                this._updatePanelClientIDs[i] = this._uniqueIDToClientID(realPanelID);
            }
            this._asyncPostBackTimeout = asyncPostBackTimeout * 1000;
        }
        else {
            this._updatePanelIDs = [];
            this._updatePanelClientIDs = [];
            this._updatePanelHasChildrenAsTriggers = [];
            this._asyncPostBackTimeout = 0;
        }
        this._asyncPostBackControlIDs = [];
        this._asyncPostBackControlClientIDs = [];
        this._convertToClientIDs(asyncPostBackControlIDs, this._asyncPostBackControlIDs, this._asyncPostBackControlClientIDs);
        this._postBackControlIDs = [];
        this._postBackControlClientIDs = [];
        this._convertToClientIDs(postBackControlIDs, this._postBackControlIDs, this._postBackControlClientIDs);
    }
    function Sys$WebForms$PageRequestManager$_updatePanel(updatePanelElement, rendering) {
        for (var updatePanelID in this._scriptDisposes) {
            if (this._elementContains(updatePanelElement, document.getElementById(updatePanelID))) {
                var disposeScripts = this._scriptDisposes[updatePanelID];
                for (var i = 0; i < disposeScripts.length; i++) {
                    eval(disposeScripts[i]);
                }
                delete this._scriptDisposes[updatePanelID];
            }
        }
        this._destroyTree(updatePanelElement);
        updatePanelElement.innerHTML = rendering;
    }
    function Sys$WebForms$PageRequestManager$_validPosition(position) {
        return (typeof(position) !== "undefined") && (position !== null) && (position !== 0);
    }
Sys.WebForms.PageRequestManager.prototype = {
    _get_eventHandlerList: Sys$WebForms$PageRequestManager$_get_eventHandlerList,
    get_isInAsyncPostBack: Sys$WebForms$PageRequestManager$get_isInAsyncPostBack,
    add_beginRequest: Sys$WebForms$PageRequestManager$add_beginRequest,
    remove_beginRequest: Sys$WebForms$PageRequestManager$remove_beginRequest,
    add_endRequest: Sys$WebForms$PageRequestManager$add_endRequest,
    remove_endRequest: Sys$WebForms$PageRequestManager$remove_endRequest,
    add_initializeRequest: Sys$WebForms$PageRequestManager$add_initializeRequest,
    remove_initializeRequest: Sys$WebForms$PageRequestManager$remove_initializeRequest,
    add_pageLoaded: Sys$WebForms$PageRequestManager$add_pageLoaded,
    remove_pageLoaded: Sys$WebForms$PageRequestManager$remove_pageLoaded,
    add_pageLoading: Sys$WebForms$PageRequestManager$add_pageLoading,
    remove_pageLoading: Sys$WebForms$PageRequestManager$remove_pageLoading,
    abortPostBack: Sys$WebForms$PageRequestManager$abortPostBack,
    _cancelPendingCallbacks: Sys$WebForms$PageRequestManager$_cancelPendingCallbacks,
    _createPageRequestManagerTimeoutError: Sys$WebForms$PageRequestManager$_createPageRequestManagerTimeoutError,
    _createPageRequestManagerServerError: Sys$WebForms$PageRequestManager$_createPageRequestManagerServerError,
    _createPageRequestManagerParserError: Sys$WebForms$PageRequestManager$_createPageRequestManagerParserError,
    _createPostBackSettings: Sys$WebForms$PageRequestManager$_createPostBackSettings,
    _convertToClientIDs: Sys$WebForms$PageRequestManager$_convertToClientIDs,
    _destroyTree: Sys$WebForms$PageRequestManager$_destroyTree,
    dispose: Sys$WebForms$PageRequestManager$dispose,
    _doCallback: Sys$WebForms$PageRequestManager$_doCallback,
    _doPostBack: Sys$WebForms$PageRequestManager$_doPostBack,
    _doPostBackWithOptions: Sys$WebForms$PageRequestManager$_doPostBackWithOptions,
    _elementContains: Sys$WebForms$PageRequestManager$_elementContains,
    _endPostBack: Sys$WebForms$PageRequestManager$_endPostBack,
    _findNearestElement: Sys$WebForms$PageRequestManager$_findNearestElement,
    _findText: Sys$WebForms$PageRequestManager$_findText,
    _fireDefaultButton: Sys$WebForms$PageRequestManager$_fireDefaultButton,
    _getPageLoadedEventArgs: Sys$WebForms$PageRequestManager$_getPageLoadedEventArgs,
    _getPageLoadingEventArgs: Sys$WebForms$PageRequestManager$_getPageLoadingEventArgs,
    _getPostBackSettings: Sys$WebForms$PageRequestManager$_getPostBackSettings,
    _getScrollPosition: Sys$WebForms$PageRequestManager$_getScrollPosition,
    _initializeInternal: Sys$WebForms$PageRequestManager$_initializeInternal,
    _matchesParentIDInList: Sys$WebForms$PageRequestManager$_matchesParentIDInList,
    _onFormElementActive: Sys$WebForms$PageRequestManager$_onFormElementActive,
    _onFormElementClick: Sys$WebForms$PageRequestManager$_onFormElementClick,
    _onFormSubmit: Sys$WebForms$PageRequestManager$_onFormSubmit,
    _onFormSubmitCompleted: Sys$WebForms$PageRequestManager$_onFormSubmitCompleted,
    _onWindowUnload: Sys$WebForms$PageRequestManager$_onWindowUnload,
    _pageLoaded: Sys$WebForms$PageRequestManager$_pageLoaded,
    _pageLoadedInitialLoad: Sys$WebForms$PageRequestManager$_pageLoadedInitialLoad,
    _queueScripts: Sys$WebForms$PageRequestManager$_queueScripts,
    _registerDisposeScript: Sys$WebForms$PageRequestManager$_registerDisposeScript,
    _scriptIncludesLoadComplete: Sys$WebForms$PageRequestManager$_scriptIncludesLoadComplete,
    _scriptIncludesLoadFailed: Sys$WebForms$PageRequestManager$_scriptIncludesLoadFailed,
    _scriptsLoadComplete: Sys$WebForms$PageRequestManager$_scriptsLoadComplete,
    _splitNodeIntoArray: Sys$WebForms$PageRequestManager$_splitNodeIntoArray,
    _uniqueIDToClientID: Sys$WebForms$PageRequestManager$_uniqueIDToClientID,
    _updateControls: Sys$WebForms$PageRequestManager$_updateControls,
    _updatePanel: Sys$WebForms$PageRequestManager$_updatePanel,
    _validPosition: Sys$WebForms$PageRequestManager$_validPosition
}
Sys.WebForms.PageRequestManager.getInstance = function Sys$WebForms$PageRequestManager$getInstance() {
    /// <summary locid="M:J#Sys.WebForms.PageRequestManager.getInstance" />
    /// <returns type="Sys.WebForms.PageRequestManager"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    var prm = Sys.WebForms.PageRequestManager._instance;
    if (!prm) {
        prm = Sys.WebForms.PageRequestManager._instance = new Sys.WebForms.PageRequestManager();
    }
    return prm;
}
Sys.WebForms.PageRequestManager._addArrayElement = function Sys$WebForms$PageRequestManager$_addArrayElement(arrayName) {
    if (!window[arrayName]) {
        window[arrayName] = new Array();
    }
    for (var i = 1, l = arguments.length; i < l; i++) {
        Array.add(window[arrayName], arguments[i]);
    }
}
Sys.WebForms.PageRequestManager._initialize = function Sys$WebForms$PageRequestManager$_initialize(scriptManagerID, formElement) {
    Sys.WebForms.PageRequestManager.getInstance()._initializeInternal(scriptManagerID, formElement);
}
Sys.WebForms.PageRequestManager.registerClass('Sys.WebForms.PageRequestManager');
 
Sys.UI._UpdateProgress = function Sys$UI$_UpdateProgress(element) {
    Sys.UI._UpdateProgress.initializeBase(this,[element]);
    this._displayAfter = 500;
    this._dynamicLayout = true;
    this._associatedUpdatePanelId = null;
    this._beginRequestHandlerDelegate = null;
    this._startDelegate = null;
    this._endRequestHandlerDelegate = null;
    this._pageRequestManager = null;
    this._timerCookie = null;
}
    function Sys$UI$_UpdateProgress$get_displayAfter() {
        /// <value type="Number" locid="P:J#Sys.UI._UpdateProgress.displayAfter"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._displayAfter;
    }
    function Sys$UI$_UpdateProgress$set_displayAfter(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;
        this._displayAfter = value;
    }
    function Sys$UI$_UpdateProgress$get_dynamicLayout() {
        /// <value type="Boolean" locid="P:J#Sys.UI._UpdateProgress.dynamicLayout"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._dynamicLayout;
    }
    function Sys$UI$_UpdateProgress$set_dynamicLayout(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;
        this._dynamicLayout = value;
    }
    function Sys$UI$_UpdateProgress$get_associatedUpdatePanelId() {
        /// <value type="String" mayBeNull="true" locid="P:J#Sys.UI._UpdateProgress.associatedUpdatePanelId"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._associatedUpdatePanelId;
    }
    function Sys$UI$_UpdateProgress$set_associatedUpdatePanelId(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;
        this._associatedUpdatePanelId = value;
    }
    function Sys$UI$_UpdateProgress$_handleBeginRequest(sender, arg) {
        var curElem = arg.get_postBackElement();
        var showProgress = !this._associatedUpdatePanelId; 
        while (!showProgress && curElem) {
            if (curElem.id && this._associatedUpdatePanelId === curElem.id) {
                showProgress = true; 
            }
            curElem = curElem.parentNode; 
        } 
        if (showProgress) {
            this._timerCookie = window.setTimeout(this._startDelegate, this._displayAfter);
        }
    }
    function Sys$UI$_UpdateProgress$_startRequest() {
        if (this._pageRequestManager.get_isInAsyncPostBack()) {
            if (this._dynamicLayout) this.get_element().style.display = 'block';
            else this.get_element().style.visibility = 'visible';
        }
        this._timerCookie = null;
    }
    function Sys$UI$_UpdateProgress$_handleEndRequest(sender, arg) {
        if (this._dynamicLayout) this.get_element().style.display = 'none';
        else this.get_element().style.visibility = 'hidden';
        if (this._timerCookie) {
            window.clearTimeout(this._timerCookie);
            this._timerCookie = null;
        }
    }
    function Sys$UI$_UpdateProgress$dispose() {
       if (this._pageRequestManager !== null) {
           this._pageRequestManager.remove_beginRequest(this._beginRequestHandlerDelegate);
           this._pageRequestManager.remove_endRequest(this._endRequestHandlerDelegate);
       }
       Sys.UI._UpdateProgress.callBaseMethod(this,"dispose");
    }
    function Sys$UI$_UpdateProgress$initialize() {
        Sys.UI._UpdateProgress.callBaseMethod(this, 'initialize');
    	this._beginRequestHandlerDelegate = Function.createDelegate(this, this._handleBeginRequest);
    	this._endRequestHandlerDelegate = Function.createDelegate(this, this._handleEndRequest);
    	this._startDelegate = Function.createDelegate(this, this._startRequest);
    	if (Sys.WebForms && Sys.WebForms.PageRequestManager) {
           this._pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
    	}
    	if (this._pageRequestManager !== null ) {
    	    this._pageRequestManager.add_beginRequest(this._beginRequestHandlerDelegate);
    	    this._pageRequestManager.add_endRequest(this._endRequestHandlerDelegate);
    	}
    }
Sys.UI._UpdateProgress.prototype = {
    get_displayAfter: Sys$UI$_UpdateProgress$get_displayAfter,
    set_displayAfter: Sys$UI$_UpdateProgress$set_displayAfter,
    get_dynamicLayout: Sys$UI$_UpdateProgress$get_dynamicLayout,
    set_dynamicLayout: Sys$UI$_UpdateProgress$set_dynamicLayout,
    get_associatedUpdatePanelId: Sys$UI$_UpdateProgress$get_associatedUpdatePanelId,
    set_associatedUpdatePanelId: Sys$UI$_UpdateProgress$set_associatedUpdatePanelId,
    _handleBeginRequest: Sys$UI$_UpdateProgress$_handleBeginRequest,
    _startRequest: Sys$UI$_UpdateProgress$_startRequest,
    _handleEndRequest: Sys$UI$_UpdateProgress$_handleEndRequest,
    dispose: Sys$UI$_UpdateProgress$dispose,
    initialize: Sys$UI$_UpdateProgress$initialize
}
Sys.UI._UpdateProgress.registerClass('Sys.UI._UpdateProgress', Sys.UI.Control);
Type.registerNamespace('Sys.WebForms');
Sys.WebForms.Res={
'PRM_MissingPanel':'Could not find UpdatePanel with ID \'{0}\'. If it is being updated dynamically then it must be inside another UpdatePanel.',
'PRM_ServerError':'An unknown error occurred while processing the request on the server. The status code returned from the server was: {0}',
'PRM_ParserError':'The message received from the server could not be parsed.',
'PRM_TimeoutError':'The server request timed out.',
'PRM_CannotRegisterTwice':'The PageRequestManager cannot be initialized more than once.',
'PRM_UnknownToken':'Unknown token: \'{0}\'.',
'PRM_MissingPanel':'Could not find UpdatePanel with ID \'{0}\'. If it is being updated dynamically then it must be inside another UpdatePanel.',
'PRM_ServerError':'An unknown error occurred while processing the request on the server. The status code returned from the server was: {0}',
'PRM_ParserError':'The message received from the server could not be parsed. Common causes for this error are when the response is modified by calls to Response.Write(), response filters, HttpModules, or server trace is enabled.\r\nDetails: {0}',
'PRM_TimeoutError':'The server request timed out.',
'PRM_ParserErrorDetails':'Error parsing near \'{0}\'.',
'PRM_CannotRegisterTwice':'The PageRequestManager cannot be initialized more than once.'
};
if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();
