function WebForm_PostBackOptions(eventTarget, eventArgument, validation, validationGroup, actionUrl, trackFocus, clientSubmit) {
    this.eventTarget = eventTarget;
    this.eventArgument = eventArgument;
    this.validation = validation;
    this.validationGroup = validationGroup;
    this.actionUrl = actionUrl;
    this.trackFocus = trackFocus;
    this.clientSubmit = clientSubmit;
}

 function WebForm_DoPostBackWithOptions(options) {
    var validationResult = true;

     if (options.validation) {
        if (typeof(Page_ClientValidate) == 'function') {
            validationResult = Page_ClientValidate(options.validationGroup);
        }
    }

     if (validationResult) {
        if ((typeof(options.actionUrl) != "undefined") && (options.actionUrl != null) && (options.actionUrl.length > 0)) {
            theForm.action = options.actionUrl;
        }

         if (options.trackFocus) {
            var lastFocus = theForm.elements["__LASTFOCUS"];
            if ((typeof(lastFocus) != "undefined") && (lastFocus != null)) {
                if (typeof(document.activeElement) == "undefined") {
                    lastFocus.value = options.eventTarget;
                }
                else {
                    var active = document.activeElement;
                    if ((typeof(active) != "undefined") && (active != null)) {
                        if ((typeof(active.id) != "undefined") && (active.id != null) && (active.id.length > 0)) {
                            lastFocus.value = active.id;
                        }
                        else if (typeof(active.name) != "undefined") {
                            lastFocus.value = active.name;
                        }
                    }
                }
            }
        }
    }

     if (options.clientSubmit) {
        __doPostBack(options.eventTarget, options.eventArgument);
    }
}

 var __pendingCallbacks = new Array();
var __synchronousCallBackIndex = -1;

 function WebForm_DoCallback(eventTarget, eventArgument, eventCallback, context, errorCallback, useAsync) {
    // Mozilla:
    // The only available documentation for the Mozilla implementation of XmlHTtp seems to be:
    // unstable.elemental.com/mozilla/build/latest/mozilla/extensions/dox/interfacensIXMLHttpRequest.html.
    // And: unstable.elemental.com/mozilla/build/latest/mozilla/extensions/dox/interfacensIJSXMLHttpRequest.html
    // It says "In general, Microsoft's documentation for IXMLHttpRequest can be used."
    // Things worth mentioning about this implementation:
    // - The async flag is implemented.
    // - No mention of a URL size limit, but there is definitely one
    // (you get Url too long with Firefox somewhere between 10000 and 2000).

     // Opera:
    // So far, Opera has no documentation on the feature, but there are documented bugs floating around.
    // - setRequestHeader does not seem to be supported. NOT fixed in Opera 8.0.
    // - The onreadystatechange function is called twice

     // Then go on with the preparation of the call to the server
    var postData = __theFormPostData +
                "__CALLBACKID=" + WebForm_EncodeCallback(eventTarget) +
                "&__CALLBACKPARAM=" + WebForm_EncodeCallback(eventArgument);
    if (theForm["__EVENTVALIDATION"]) {
        postData += "&__EVENTVALIDATION=" + WebForm_EncodeCallback(theForm["__EVENTVALIDATION"].value);
    }

     var xmlRequest,e;
    try {
        xmlRequest = new XMLHttpRequest();
    }
    catch(e) {
        try {
            xmlRequest = new ActiveXObject("Microsoft.XMLHTTP");
        }
        catch(e) {
        }
    }

     // Check for Opera 8 beta bug (setRequestHeader not implemented) while not creating
    // a js error in IE (throws if asking for the existence of a method name on an ActiveX object)
    var setRequestHeaderMethodExists = true;
    try {
        setRequestHeaderMethodExists = (xmlRequest && xmlRequest.setRequestHeader);
    }
    catch(e) {}

     // Creating the callback structure and queuing it up.
    var callback = new Object();
    callback.eventCallback = eventCallback;
    callback.context = context;
    callback.errorCallback = errorCallback;
    callback.async = useAsync;
    var callbackIndex = WebForm_FillFirstAvailableSlot(__pendingCallbacks, callback);

     // If the new callback is synchronous, check if there is a synchronous call pending.
    // If there is one, throw it away.
    if (!useAsync) {
        if (__synchronousCallBackIndex != -1) {
            __pendingCallbacks[__synchronousCallBackIndex] = null;
        }
        __synchronousCallBackIndex = callbackIndex;
    }

 //    document.body.appendChild(document.createTextNode("Sending " + callbackIndex + "|" + eventArgument + "\r\n"));

     // use the XmlHttp object if possible
    if (setRequestHeaderMethodExists) {
        xmlRequest.onreadystatechange = WebForm_CallbackComplete;
        callback.xmlRequest = xmlRequest;

         // We always use async to prevent browser lockup. Synchronous behavior is simulated by queuing responses.
        // Dev10 Bug 517294: Internet Explorer includes a fragment "#" in the action url, encoding it as %23, which
        // causes a 404 on the server. Normally form.action never includes the fragment even if the orignal url did,
        // but with AJAX History support, it does. It might also happen by some url rewriting schemes. UpdatePanels
        // strip the fragment potion, and so should callbacks. Other browsers ignore the fragment, but we always
        // strip it for consistency.
        // Dev10 bug 840338: The action attribute might be empty when the page is requested via the default document,
        // e.g. http://localhost/repro/ . If so, we need to set the path in order for XHR to work properly.
        var action = theForm.action || document.location.pathname, fragmentIndex = action.indexOf('#');
        if (fragmentIndex !== -1) {
            action = action.substr(0, fragmentIndex);
        }
        if (!__nonMSDOMBrowser) {
            // Dev10 514824: Just like PageRequestManager does for partial postbacks, we must
            // manually encode the uri as in IE, it is not encoded automaticaly by XHR.
            // We only want to encode the path fragment, not the querystring, and
            // we do not want to double-encode the URI in case it is already encoded.
            // A URI that is already encoded and has an encoded character would always have
            // a % symbol in it.
            // DevDiv 1040710: The domain part should never run through encodeURL() in order to support IDN. 
            var domain = "";
            var path = action;
            var query = "";
            var queryIndex = action.indexOf('?');
            if (queryIndex !== -1) {
                // tear off the query, encode, then put the query back
                query = action.substr(queryIndex);
                path = action.substr(0, queryIndex);
            }

             if (path.indexOf("%") === -1) {
                // only encode if the path portion is not already encoded
                // for IDN paths we must NOT encode the domain name.
                // domain may or may not be present (e.g. action of "foo.aspx" vs "http://domain/foo.aspx").
                if (/^https?\:\/\/.*$/gi.test(path)) {
                    var domainPartIndex = path.indexOf("\/\/") + 2;
                    var slashAfterDomain = path.indexOf("/", domainPartIndex);
                    if (slashAfterDomain === -1) {
                        // entire url is the domain (e.g. "http://foo.com")
                        domain = path;
                        path = "";
                    }
                    else {
                        domain = path.substr(0, slashAfterDomain);
                        path = path.substr(slashAfterDomain);
                    }
                }
                action = domain + encodeURI(path) + query;
            }
        }
        xmlRequest.open("POST", action, true);
        xmlRequest.setRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
        xmlRequest.send(postData);
//        document.body.appendChild(document.createTextNode("Callback sent" + xmlRequest.readyState + useAsync + "\r\n"));
        return;
    }
    // XmlHttp failed. Try an iFrame instead
    // Note: while this fallback works well with IE (ActiveX disabled), in Opera 7, there's a bug
    // in that when we try to send multiple callback requests using frames,
    // Opera forgets about the previous requests every time you create a new one.
    // It seems to allow for only one post at a time.
    // there is no working around that, and the results is that asynchronous
    // callbacks behave like synchronous ones with Opera 7.
    // Note: this is fixed in Opera 8.
    callback.xmlRequest = new Object();
    var callbackFrameID = "__CALLBACKFRAME" + callbackIndex;
    var xmlRequestFrame = document.frames[callbackFrameID];
    if (!xmlRequestFrame) {
        xmlRequestFrame = document.createElement("IFRAME");
        xmlRequestFrame.width = "1";
        xmlRequestFrame.height = "1";
        xmlRequestFrame.frameBorder = "0";
        xmlRequestFrame.id = callbackFrameID;
        xmlRequestFrame.name = callbackFrameID;
        // Putting the frame outside of the view
        xmlRequestFrame.style.position = "absolute";
        xmlRequestFrame.style.top = "-100px"
        xmlRequestFrame.style.left = "-100px";
        // Set the src to the smartnav.htm page for https pages
        // (try/catch is here to prevent Opera from issuing an error message if callBackFrameUrl is undefined)
        try {
            if (callBackFrameUrl) {
                xmlRequestFrame.src = callBackFrameUrl;
            }
        }
        catch(e) {}
        document.body.appendChild(xmlRequestFrame);
    }
    // Once the iFrame is created, its document may not be immediately available.
    // We wait for it by 10ms intervals.
    var interval = window.setInterval(function() {
        xmlRequestFrame = document.frames[callbackFrameID];
        // Wait some more if the document is not yet ready.
        if (xmlRequestFrame && xmlRequestFrame.document) {
            window.clearInterval(interval);
//            document.body.appendChild(document.createTextNode("Callback frame " + callbackFrameID + "\r\n"));
            // Empty the document
            xmlRequestFrame.document.write("");
            xmlRequestFrame.document.close();
            // Create a copy of the form to post in the frame
            xmlRequestFrame.document.write('<html><body><form method="post"><input type="hidden" name="__CALLBACKLOADSCRIPT" value="t"></form></body></html>');
            xmlRequestFrame.document.close();
            // Copy the action of the main form to the iFrame form
            xmlRequestFrame.document.forms[0].action = theForm.action;
            // Copy the relevant form fields
            var count = __theFormPostCollection.length;
            var element;
            for (var i = 0; i < count; i++) {
                element = __theFormPostCollection[i];
                if (element) {
                    var fieldElement = xmlRequestFrame.document.createElement("INPUT");
                    fieldElement.type = "hidden";
                    fieldElement.name = element.name;
                    fieldElement.value = element.value;
                    xmlRequestFrame.document.forms[0].appendChild(fieldElement);
                }
            }
            // Create the callback id field
            var callbackIdFieldElement = xmlRequestFrame.document.createElement("INPUT");
            callbackIdFieldElement.type = "hidden";
            callbackIdFieldElement.name = "__CALLBACKID";
            callbackIdFieldElement.value = eventTarget;
            xmlRequestFrame.document.forms[0].appendChild(callbackIdFieldElement);
            // Create the callback param field
            var callbackParamFieldElement = xmlRequestFrame.document.createElement("INPUT");
            callbackParamFieldElement.type = "hidden";
            callbackParamFieldElement.name = "__CALLBACKPARAM";
            callbackParamFieldElement.value = eventArgument;
            xmlRequestFrame.document.forms[0].appendChild(callbackParamFieldElement);
            if (theForm["__EVENTVALIDATION"]) {
                var callbackValidationFieldElement = xmlRequestFrame.document.createElement("INPUT");
                callbackValidationFieldElement.type = "hidden";
                callbackValidationFieldElement.name = "__EVENTVALIDATION";
                callbackValidationFieldElement.value = theForm["__EVENTVALIDATION"].value;
                xmlRequestFrame.document.forms[0].appendChild(callbackValidationFieldElement);
            }
            // Round-trip the callback index because
            // the server must generate a script that references the right callback object.
            // The indices can be recycled, but only after the server has responded.
            var callbackIndexFieldElement = xmlRequestFrame.document.createElement("INPUT");
            callbackIndexFieldElement.type = "hidden";
            callbackIndexFieldElement.name = "__CALLBACKINDEX";
            callbackIndexFieldElement.value = callbackIndex;
            xmlRequestFrame.document.forms[0].appendChild(callbackIndexFieldElement);
            // Finally, submit the form
            xmlRequestFrame.document.forms[0].submit();
        }
    }, 10);
}

 function WebForm_CallbackComplete() {
//    document.body.appendChild(document.createTextNode("Callback change\r\n"));
    // Look through the list of pending callbacks for the ones that are ready.
    for (var i = 0; i < __pendingCallbacks.length; i++) {
        callbackObject = __pendingCallbacks[i];
        if (callbackObject && callbackObject.xmlRequest && (callbackObject.xmlRequest.readyState == 4)) {
//            document.body.appendChild(document.createTextNode("Callback " + i + "|" + callbackObject.xmlRequest.responseText + "\r\n"));
            if (!__pendingCallbacks[i].async) {
                __synchronousCallBackIndex = -1;
            }
            __pendingCallbacks[i] = null;
            // Also remove callback frame if it exists to correct browser history
            var callbackFrameID = "__CALLBACKFRAME" + i;
            var xmlRequestFrame = document.getElementById(callbackFrameID);
            if (xmlRequestFrame) {
                xmlRequestFrame.parentNode.removeChild(xmlRequestFrame);
            }
            // Dev10 505100: ExecuteCallback after removing callback, because the callback may initiate
            // another callback and cause reentrancy. Normally reentrancy wouldn't be possible, but if the
            // callback also performs an alert or causes a mixed security dialog to appear, it is.
            WebForm_ExecuteCallback(callbackObject);
        }
    }
}

 function WebForm_ExecuteCallback(callbackObject) {
    var response = callbackObject.xmlRequest.responseText;
    if (response.charAt(0) == "s") {
        if ((typeof(callbackObject.eventCallback) != "undefined") && (callbackObject.eventCallback != null)) {
            callbackObject.eventCallback(response.substring(1), callbackObject.context);
        }
    }
    else if (response.charAt(0) == "e") {
        if ((typeof(callbackObject.errorCallback) != "undefined") && (callbackObject.errorCallback != null)) {
            callbackObject.errorCallback(response.substring(1), callbackObject.context);
        }
    }
    else {
        var separatorIndex = response.indexOf("|");
        if (separatorIndex != -1) {
            var validationFieldLength = parseInt(response.substring(0, separatorIndex));
            if (!isNaN(validationFieldLength)) {
                var validationField = response.substring(separatorIndex + 1, separatorIndex + validationFieldLength + 1);
                if (validationField != "") {
                    var validationFieldElement = theForm["__EVENTVALIDATION"];
                    if (!validationFieldElement) {
                        validationFieldElement = document.createElement("INPUT");
                        validationFieldElement.type = "hidden";
                        validationFieldElement.name = "__EVENTVALIDATION";
                        theForm.appendChild(validationFieldElement);
                    }
                    validationFieldElement.value = validationField;
                }
                if ((typeof(callbackObject.eventCallback) != "undefined") && (callbackObject.eventCallback != null)) {
                    callbackObject.eventCallback(response.substring(separatorIndex + validationFieldLength + 1), callbackObject.context);
                }
            }
        }
    }
}

 function WebForm_FillFirstAvailableSlot(array, element) {
    var i;
    for (i = 0; i < array.length; i++) {
        if (!array[i]) break;
    }
    array[i] = element;
    return i;
}

 var __nonMSDOMBrowser = (window.navigator.appName.toLowerCase().indexOf('explorer') == -1);
var __theFormPostData = "";
var __theFormPostCollection = new Array();
var __callbackTextTypes = /^(text|password|hidden|search|tel|url|email|number|range|color|datetime|date|month|week|time|datetime-local)$/i;

 function WebForm_InitCallback() {
    // If there can be a callback in the current page, create a simulated postback string,
    // so the server can restore state to the page during the callback
    // The data we send is the original state of the form. If the control developer
    // wants to send updated values, he should use the callback argument.
    var formElements = theForm.elements,
        count = formElements.length,
        element;
    for (var i = 0; i < count; i++) {
        element = formElements[i];
        var tagName = element.tagName.toLowerCase();
        if (tagName == "input") {
            var type = element.type;
            if ((__callbackTextTypes.test(type) || ((type == "checkbox" || type == "radio") && element.checked))
                && (element.id != "__EVENTVALIDATION")) {
                WebForm_InitCallbackAddField(element.name, element.value);
            }
        }
        else if (tagName == "select") {
            var selectCount = element.options.length;
            for (var j = 0; j < selectCount; j++) {
                var selectChild = element.options[j];
                if (selectChild.selected == true) {
                    WebForm_InitCallbackAddField(element.name, element.value);
                }
            }
        }
        else if (tagName == "textarea") {
            WebForm_InitCallbackAddField(element.name, element.value);
        }
    }
}

 function WebForm_InitCallbackAddField(name, value) {
    var nameValue = new Object();
    nameValue.name = name;
    nameValue.value = value;
    __theFormPostCollection[__theFormPostCollection.length] = nameValue;
    __theFormPostData += WebForm_EncodeCallback(name) + "=" + WebForm_EncodeCallback(value) + "&";
}

 function WebForm_EncodeCallback(parameter) {
    if (encodeURIComponent) {
        return encodeURIComponent(parameter);
    }
    else {
        return escape(parameter);
    }
}

 var __disabledControlArray = new Array();

 function WebForm_ReEnableControls() {
    if (typeof(__enabledControlArray) == 'undefined') {
        return false;
    }

     var disabledIndex = 0;
    for (var i = 0; i < __enabledControlArray.length; i++) {
        var c;
        if (__nonMSDOMBrowser) {
            c = document.getElementById(__enabledControlArray[i]);
        }
        else {
            c = document.all[__enabledControlArray[i]];
        }

         if ((typeof(c) != "undefined") && (c != null) && (c.disabled == true)) {
            c.disabled = false;
            __disabledControlArray[disabledIndex++] = c;
        }
    }

     setTimeout("WebForm_ReDisableControls()", 0);

     return true;
}

 function WebForm_ReDisableControls() {
    for (var i = 0; i < __disabledControlArray.length; i++) {
        __disabledControlArray[i].disabled = true;
    }
}

 function WebForm_SimulateClick(element, event) {
    var clickEvent;
    if (element) {
        // Just use the click method if it exists
        if (element.click) {
            element.click();
        } else { // No click method, so try raising a click event instead (i.e. Chrome anchor tags)
            clickEvent = document.createEvent("MouseEvents");
            clickEvent.initMouseEvent("click", true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
            if (!element.dispatchEvent(clickEvent)) {
                return true;
            }
        }
        event.cancelBubble = true;
        if (event.stopPropagation) {
            event.stopPropagation();
        }
        return false;
    }
    return true;
}

 function WebForm_FireDefaultButton(event, target) {
    if (event.keyCode == 13) {
        var src = event.srcElement || event.target;
        if (src &&
            ((src.tagName.toLowerCase() == "input") &&
             (src.type.toLowerCase() == "submit" || src.type.toLowerCase() == "button")) ||
            ((src.tagName.toLowerCase() == "a") &&
             (src.href != null) && (src.href != "")) ||
            (src.tagName.toLowerCase() == "textarea")) {
            return true;
        }
        var defaultButton;
        if (__nonMSDOMBrowser) {
            defaultButton = document.getElementById(target);
        }
        else {
            defaultButton = document.all[target];
        }

         if (defaultButton) {
            return WebForm_SimulateClick(defaultButton, event);
        } 
    }
    return true;
}

 function WebForm_GetScrollX() {
    if (__nonMSDOMBrowser) {
        return window.pageXOffset;
    }
    else {
        if (document.documentElement && document.documentElement.scrollLeft) {
            return document.documentElement.scrollLeft;
        }
        else if (document.body) {
            return document.body.scrollLeft;
        }
    }
    return 0;
}

 function WebForm_GetScrollY() {
    if (__nonMSDOMBrowser) {
        return window.pageYOffset;
    }
    else {
        if (document.documentElement && document.documentElement.scrollTop) {
            return document.documentElement.scrollTop;
        }
        else if (document.body) {
            return document.body.scrollTop;
        }
    }
    return 0;
}

 function WebForm_SaveScrollPositionSubmit() {
    if (__nonMSDOMBrowser) {
        theForm.elements['__SCROLLPOSITIONY'].value = window.pageYOffset;
        theForm.elements['__SCROLLPOSITIONX'].value = window.pageXOffset;
    }
    else {
        theForm.__SCROLLPOSITIONX.value = WebForm_GetScrollX();
        theForm.__SCROLLPOSITIONY.value = WebForm_GetScrollY();
    }

     // not using == "function" because in IE, this may be an object.
    if ((typeof(this.oldSubmit) != "undefined") && (this.oldSubmit != null)) {
        return this.oldSubmit();
    }
    return true;
}

 function WebForm_SaveScrollPositionOnSubmit() {
    theForm.__SCROLLPOSITIONX.value = WebForm_GetScrollX();
    theForm.__SCROLLPOSITIONY.value = WebForm_GetScrollY();

     // not using == "function" because in IE, this may be an object.
    if ((typeof(this.oldOnSubmit) != "undefined") && (this.oldOnSubmit != null)) {
        return this.oldOnSubmit();
    }
    return true;
}

 function WebForm_RestoreScrollPosition() {
    if (__nonMSDOMBrowser) {
        window.scrollTo(theForm.elements['__SCROLLPOSITIONX'].value, theForm.elements['__SCROLLPOSITIONY'].value);
    }
    else {
        window.scrollTo(theForm.__SCROLLPOSITIONX.value, theForm.__SCROLLPOSITIONY.value);
    }

     // not using == "function" because in IE, this may be an object.
    if ((typeof(theForm.oldOnLoad) != "undefined") && (theForm.oldOnLoad != null)) {
        return theForm.oldOnLoad();
    }
    return true;
}

 function WebForm_TextBoxKeyHandler(event) {
    if (event.keyCode == 13) {
        var target;
        if (__nonMSDOMBrowser) {
            target = event.target;
        }
        else {
            target = event.srcElement;
        }
        if ((typeof(target) != "undefined") && (target != null)) {
            if (typeof(target.onchange) != "undefined") {
                target.onchange();
                event.cancelBubble = true;
                if (event.stopPropagation) event.stopPropagation();
                return false;
            }
        }
    }

     return true;
}

 function WebForm_TrimString(value) {
    return value.replace(/^\s+|\s+$/g, '')
}

 function WebForm_AppendToClassName(element, className) {
    var currentClassName = ' ' + WebForm_TrimString(element.className) + ' ';
    className = WebForm_TrimString(className);
    var index = currentClassName.indexOf(' ' + className + ' ');
    if (index === -1) {
        element.className = (element.className === '') ? className : element.className + ' ' + className;
    }
}

 // Removes the first occurence of className from the element's className
function WebForm_RemoveClassName(element, className) {
    var currentClassName = ' ' + WebForm_TrimString(element.className) + ' ';
    className = WebForm_TrimString(className);
    var index = currentClassName.indexOf(' ' + className + ' ');
    if (index >= 0) {
        element.className = WebForm_TrimString(currentClassName.substring(0, index) + ' ' +
            currentClassName.substring(index + className.length + 1, currentClassName.length));
    }
}

 // Cross-browser getElementById
function WebForm_GetElementById(elementId) {
    if (document.getElementById) {
        return document.getElementById(elementId);
    }
    else if (document.all) {
        return document.all[elementId];
    }
    else return null;
}

 // Cross-browser getElementByTagName
function WebForm_GetElementByTagName(element, tagName) {
    var elements = WebForm_GetElementsByTagName(element, tagName);
    if (elements && elements.length > 0) {
        return elements[0];
    }
    else return null;
}

 // Cross-browser getElementsByTagName
function WebForm_GetElementsByTagName(element, tagName) {
    if (element && tagName) {
        if (element.getElementsByTagName) {
            return element.getElementsByTagName(tagName);
        }
        if (element.all && element.all.tags) {
            return element.all.tags(tagName);
        }
    }
    return null;
}

 function WebForm_GetElementDir(element) {
    if (element) {
        if (element.dir) {
            return element.dir;
        }
        return WebForm_GetElementDir(element.parentNode);
    }
    return "ltr";
}

 // Cross-browser element position and size.
// Returns an object with x, y, width and height properties
function WebForm_GetElementPosition(element) {
    var result = new Object();
    result.x = 0;
    result.y = 0;
    result.width = 0;
    result.height = 0;

     // Get coordinates
    if (element.offsetParent) {
        result.x = element.offsetLeft;
        result.y = element.offsetTop;
        var parent = element.offsetParent;
        while (parent) {
            result.x += parent.offsetLeft;
            result.y += parent.offsetTop;
            var parentTagName = parent.tagName.toLowerCase();
            if (parentTagName != "table" &&
                parentTagName != "body" && 
                parentTagName != "html" && 
                parentTagName != "div" && 
                parent.clientTop && 
                parent.clientLeft) {

                 result.x += parent.clientLeft;
                result.y += parent.clientTop;
            }
            parent = parent.offsetParent;
        }
    }
    else if (element.left && element.top) {
        result.x = element.left;
        result.y = element.top;
    }
    else {
        if (element.x) {
            result.x = element.x;
        }
        if (element.y) {
            result.y = element.y;
        }
    }
    if (element.offsetWidth && element.offsetHeight) {
        result.width = element.offsetWidth;
        result.height = element.offsetHeight;
    }
    else if (element.style && element.style.pixelWidth && element.style.pixelHeight) {
        result.width = element.style.pixelWidth;
        result.height = element.style.pixelHeight;
    }
    return result;
}

 function WebForm_GetParentByTagName(element, tagName) {
    var parent = element.parentNode;
    var upperTagName = tagName.toUpperCase();
    while (parent && (parent.tagName.toUpperCase() != upperTagName)) {
        parent = parent.parentNode ? parent.parentNode : parent.parentElement;
    }
    return parent;
}

 // Set the height of an element
function WebForm_SetElementHeight(element, height) {
    if (element && element.style) {
        element.style.height = height + "px";
    }
}

 // Set the width of an element
function WebForm_SetElementWidth(element, width) {
    if (element && element.style) {
        element.style.width = width + "px";
    }
}

 // Set the X coordinate of an element
function WebForm_SetElementX(element, x) {
    if (element && element.style) {
        element.style.left = x + "px";
    }
}

 // Set the Y coordinate of an element
function WebForm_SetElementY(element, y) {
    if (element && element.style) {
        element.style.top = y + "px";
    }
}