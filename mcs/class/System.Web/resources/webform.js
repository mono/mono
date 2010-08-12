/*
 * WebForm.js
 *
 * Authors:
 *   Chris Toshok (toshok@ximian.com)
 *   Lluis Sanchez Gual (lluis@novell.com)
 *   Igor Zelmanovich (igorz@mainsoft.com)
 *
 * (c) 2005 Novell, Inc. (http://www.novell.com)
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

function WebForm_Initialize(webForm) {

webForm.__pendingCallbacks = new Array();
webForm.__synchronousCallBackIndex = -1;
webForm.__theFormPostData = "";
webForm.__theFormPostCollection = new Array();

webForm.WebForm_AutoFocus = function (id)
{
	var x = webForm.WebForm_GetElementById (id);

	if (x && (!webForm.WebForm_CanFocus(x))) {
		x = webForm.WebForm_FindFirstFocusableChild(x);
	}
	if (x) { x.focus(); }
}

webForm.WebForm_CanFocus = function (element) {
	if (!element || !(element.tagName) || element.disabled) {
		return false;
	}
	if (element.type && element.type.toLowerCase() == "hidden") {
		return false;
	}
	var tagName = element.tagName.toLowerCase();
	return (tagName == "input" ||
			tagName == "textarea" ||
			tagName == "select" ||
			tagName == "button" ||
			tagName == "a");
}

webForm.WebForm_FindFirstFocusableChild = function (element) {
	if (!element || !(element.tagName)) {
		return null;
	}
	var tagName = element.tagName.toLowerCase();
	if (tagName == "undefined") {
		return null;
	}
	var children = element.childNodes;
	if (children) {
		for (var i = 0; i < children.length; i++) {
			try {
				if (webForm.WebForm_CanFocus(children[i])) {
					return children[i];
				}
				else {
					var focused = webForm.WebForm_FindFirstFocusableChild(children[i]);
					if (webForm.WebForm_CanFocus(focused)) {
						return focused;
					}
				}
			} catch (e) {
			}
		}
	}
	return null;
}

webForm.WebForm_ReEnableControls = function  ()
{
	if (typeof(webForm.__enabledControlArray) == 'undefined' || webForm.__enabledControlArray == null)
		return false;
	
	webForm.__disabledControlArray = new Array();
	for (var i = 0; i < webForm.__enabledControlArray.length; i++) {
		var c = webForm.WebForm_GetElementById (webForm.__enabledControlArray[i]);
		if ((typeof(c) != "undefined") && (c != null) && (c.disabled == true)) {
			c.disabled = false;
			webForm.__disabledControlArray[webForm.__disabledControlArray.length] = c;
		}
	}
	setTimeout(function () { webForm.WebForm_ReDisableControls (); }, 0);
	return true;
}

webForm.WebForm_ReDisableControls = function  ()
{
	for (var i = 0; i < webForm.__disabledControlArray.length; i++) {
		webForm.__disabledControlArray[i].disabled = true;
	}
}

webForm.WebForm_DoPostback = function (eventTarget, eventArgument, actionUrl, autoPostBack, validation, trackFocus, clientSubmit, validationGroup)
{
	webForm.WebForm_DoPostBackWithOptions({
		"eventTarget" : eventTarget,
		"eventArgument" : eventArgument,
		"validation" : validation,
		"validationGroup" : validationGroup,
		"actionUrl" : actionUrl,
		"trackFocus" : trackFocus,
		"clientSubmit" : clientSubmit,
		"autoPostBack" : autoPostBack
		});
}

webForm.WebForm_DoPostBackWithOptions = function  (options) {
	var validationResult = true;
	if (options.validation && typeof(webForm.Page_ClientValidate) == "function")
		validationResult =  webForm.Page_ClientValidate(options.validationGroup);

	if (validationResult) {
		if ((typeof(options.actionUrl) != "undefined") && (options.actionUrl != null) && (options.actionUrl.length > 0))
			webForm._form.action = options.actionUrl;
		if (options.trackFocus) {
			var lastFocus = webForm._form.elements["__LASTFOCUS"];
			if ((typeof(lastFocus) != "undefined") && (lastFocus != null))
				lastFocus.value = options.eventTarget;
		}
	}		
	if (options.clientSubmit)
		webForm.__doPostBack (options.eventTarget, options.eventArgument);
}

webForm.WebForm_DoCallback = function (id, arg, callback, ctx, errorCallback, useAsync)
{
	var qs = webForm.__theFormPostData + "__CALLBACKID=" + id + "&__CALLBACKPARAM=" + encodeURIComponent(arg);

	if (webForm._form["__EVENTVALIDATION"])
		qs += "&__EVENTVALIDATION=" + encodeURIComponent(webForm._form["__EVENTVALIDATION"].value);

	var httpPost = null;
	
	if (typeof XMLHttpRequest != "undefined") {
		httpPost = new XMLHttpRequest ();
	} else {
		if (window.axName != null)
			httpPost = new ActiveXObject (window.axName);
		else {
			var clsnames = new Array ("MSXML", "MSXML2", "MSXML3", "Microsoft");
			for (n = 0; n < clsnames.length && httpPost == null; n++) {
				window.axName = clsnames [n] + ".XMLHTTP";
				try {
					httpPost = new ActiveXObject (window.axName);
				} catch (e) { window.axName = null; }
			}
			if (httpPost == null)
				throw new Error ("XMLHTTP object could not be created.");
		}
	}

	var i;
	for (i = 0; i < webForm.__pendingCallbacks.length; i++)
		if (!webForm.__pendingCallbacks[i]) break;
	webForm.__pendingCallbacks[i] = {
		"eventCallback" : callback,
		"context" : ctx,
		"errorCallback" : errorCallback,
		"async" : useAsync,
		"xmlRequest" : httpPost
		};

	if (!useAsync) {
		if (webForm.__synchronousCallBackIndex != -1)
			webForm.__pendingCallbacks[webForm.__synchronousCallBackIndex] = null;
		webForm.__synchronousCallBackIndex = i;
	}

	httpPost.onreadystatechange = function () { 
			for (i = 0; i < webForm.__pendingCallbacks.length; i++) {
				var callbackObject = webForm.__pendingCallbacks[i];
				if (callbackObject && callbackObject.xmlRequest && (callbackObject.xmlRequest.readyState == 4)) {
					webForm.WebForm_ClientCallback(
						callbackObject.xmlRequest.responseText, 
						callbackObject.context, 
						callbackObject.eventCallback, 
						callbackObject.errorCallback 
						);
					if (!webForm.__pendingCallbacks[i].async)
						webForm.__synchronousCallBackIndex = -1;
					webForm.__pendingCallbacks[i] = null;
				}
			}
		};
	
	httpPost.open ("POST", webForm._form.serverURL || webForm._form.action, true);
	httpPost.setRequestHeader ("Content-Type", "application/x-www-form-urlencoded");
	setTimeout (function () { httpPost.send (qs); }, 0);
}

webForm.WebForm_ClientCallback = function (doc, ctx, callback, errorCallback)
{
	if (doc.charAt(0) == "e") {
		if ((typeof(errorCallback) != "undefined") && (errorCallback != null))
			errorCallback(doc.substring(1), ctx);
	} else {
		var separatorIndex = doc.indexOf("|");
		if (separatorIndex != -1) {
			var validationFieldLength = parseInt(doc.substring(0, separatorIndex));
			if (!isNaN(validationFieldLength)) {
				var validationField = doc.substring(separatorIndex + 1, separatorIndex + validationFieldLength + 1);
				if (validationField != "") {
					var validationFieldElement = webForm._form["__EVENTVALIDATION"];
					if (!validationFieldElement) {
						validationFieldElement = document.createElement("INPUT");
						validationFieldElement.type = "hidden";
						validationFieldElement.name = "__EVENTVALIDATION";
						validationFieldElement.id = validationFieldElement.name;
						webForm._form.appendChild(validationFieldElement);
					}
					validationFieldElement.value = validationField;
				}
				if ((typeof(callback) != "undefined") && (callback != null))
					callback (doc.substring(separatorIndex + validationFieldLength + 1), ctx);
			}
		} else {
			if ((typeof(callback) != "undefined") && (callback != null))
				callback (doc, ctx);
		}
	}
}

webForm.WebForm_InitCallback = function () {
	var len = webForm._form.elements.length;
	for (n=0; n<len; n++) {
		var elem = webForm._form.elements [n];
		var tagName = elem.tagName.toLowerCase();
		if (tagName == "input") {
			var type = elem.type;
			if ((type == "text" || type == "hidden" || type == "password" ||
				((type == "checkbox" || type == "radio") && elem.checked)) &&
				(elem.id != "__EVENTVALIDATION")) {
					webForm.WebForm_InitCallbackInputField (elem);
			}
		} else if (tagName == "select") {
			var selectCount = elem.options.length;
			for (var j = 0; j < selectCount; j++) {
				var selectChild = elem.options[j];
				if (selectChild.selected == true) {
					webForm.WebForm_InitCallbackInputField (elem);
				}
			}
		}
		else if (tagName == "textarea") {
			webForm.WebForm_InitCallbackInputField (elem);
		}
	}
}

webForm.WebForm_InitCallbackInputField = function (elem) {
	webForm.__theFormPostCollection[webForm.__theFormPostCollection.length] = {"name" : elem.name, "value" : elem.value};
	webForm.__theFormPostData  += elem.name + "=" + encodeURIComponent (elem.value) + "&";
}

webForm.WebForm_GetElementById = function (id)
{
	return document.getElementById ? document.getElementById (id) :
	       document.all ? document.all [id] :
		   document [id];
}

webForm.WebForm_FireDefaultButton = function (event, target)
{
	if (event.keyCode != 13) {
		return true;
	}
	if(event.srcElement && (event.srcElement.tagName.toLowerCase() == "textarea")) {
		return true;
	}
	var defaultButton = webForm.WebForm_GetElementById(target);
	if (!defaultButton)
		return true;
	
	if (typeof(defaultButton.click) != "undefined") {
		defaultButton.click();
		event.cancelBubble = true;
		return false;
	}
	
	if (defaultButton.href && defaultButton.href.match(/^javascript:/i)) {
		var jsCode = defaultButton.href.match(/^javascript:(.*)/i)[1]; 
		eval(jsCode);
		event.cancelBubble = true;
		return false;
	}
	
	return true;
}

webForm.WebForm_SaveScrollPositionSubmit = function ()
{
	var pos = webForm.WebForm_GetElementPosition(webForm._form);
	webForm._form.elements['__SCROLLPOSITIONX'].value = webForm.WebForm_GetScrollX() - pos.x;
	webForm._form.elements['__SCROLLPOSITIONY'].value = webForm.WebForm_GetScrollY() - pos.y;
	if ((typeof(webForm._form.oldSubmit) != "undefined") && (webForm._form.oldSubmit != null)) {
		return webForm._form.oldSubmit();
	}
	return true;
}

webForm.WebForm_SaveScrollPositionOnSubmit = function ()
{
	var pos = webForm.WebForm_GetElementPosition(webForm._form);
	webForm._form.elements['__SCROLLPOSITIONX'].value = webForm.WebForm_GetScrollX() - pos.x;
	webForm._form.elements['__SCROLLPOSITIONY'].value = webForm.WebForm_GetScrollY() - pos.y;
	if ((typeof(webForm._form.oldOnSubmit) != "undefined") && (webForm._form.oldOnSubmit != null)) {
		return webForm._form.oldOnSubmit();
	}
	return true;
}

webForm.WebForm_RestoreScrollPosition = function ()
{
	var pos = webForm.WebForm_GetElementPosition(webForm._form);
	var ScrollX = parseInt(webForm._form.elements['__SCROLLPOSITIONX'].value);
	var ScrollY = parseInt(webForm._form.elements['__SCROLLPOSITIONY'].value);
	ScrollX = (isNaN(ScrollX)) ? pos.x : (ScrollX + pos.x);
	ScrollY = (isNaN(ScrollY)) ? pos.y : (ScrollY + pos.y);
	window.scrollTo(ScrollX, ScrollY);
	if ((typeof(webForm._form.oldOnLoad) != "undefined") && (webForm._form.oldOnLoad != null)) {
		return webForm._form.oldOnLoad();
	}
	return true;
}

webForm.WebForm_GetScrollX = function () {
    if (window.pageXOffset) {
        return window.pageXOffset;
    }
    else if (document.documentElement && document.documentElement.scrollLeft) {
        return document.documentElement.scrollLeft;
    }
    else if (document.body) {
        return document.body.scrollLeft;
    }
    return 0;
}

webForm.WebForm_GetScrollY = function () {
    if (window.pageYOffset) {
        return window.pageYOffset;
    }
    else if (document.documentElement && document.documentElement.scrollTop) {
        return document.documentElement.scrollTop;
    }
    else if (document.body) {
        return document.body.scrollTop;
    }
    return 0;
}

webForm.WebForm_GetElementPosition = function (element)
{
	var result = new Object();
	result.x = 0;
	result.y = 0;
	result.width = 0;
	result.height = 0;
	result.x = element.offsetLeft;
	result.y = element.offsetTop;
	var parent = element.offsetParent;
	while (parent) {
		result.x += parent.offsetLeft;
		result.y += parent.offsetTop;
		parent = parent.offsetParent;
	}
	result.width = element.offsetWidth;
	result.height = element.offsetHeight;
	return result;
}
}

