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

webForm.WebForm_AutoFocus = function (id)
{
	var x = this.WebForm_GetElementById (id);

	if (x && (!this.WebForm_CanFocus(x))) {
		x = this.WebForm_FindFirstFocusableChild(x);
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
				if (this.WebForm_CanFocus(children[i])) {
					return children[i];
				}
				else {
					var focused = this.WebForm_FindFirstFocusableChild(children[i]);
					if (this.WebForm_CanFocus(focused)) {
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
	if (typeof(this._form.__enabledControlArray) != 'undefined' && this._form.__enabledControlArray != null)
		__enabledControlArray = this._form.__enabledControlArray;
	
	if (typeof(__enabledControlArray) == 'undefined' || __enabledControlArray == null)
		return false;
	
	this._form.__disabledControlArray = new Array();
	for (var i = 0; i < __enabledControlArray.length; i++) {
		var c = this.WebForm_GetElementById (__enabledControlArray[i]);
		if ((typeof(c) != "undefined") && (c != null) && (c.disabled == true)) {
			c.disabled = false;
			this._form.__disabledControlArray[this._form.__disabledControlArray.length] = c;
		}
	}
	setTimeout((this._instanceVariableName ? this._instanceVariableName + "." : "") + "WebForm_ReDisableControls ()", 0);
	return true;
}

webForm.WebForm_ReDisableControls = function  ()
{
	for (var i = 0; i < this._form.__disabledControlArray.length; i++) {
		this._form.__disabledControlArray[i].disabled = true;
	}
}

// This function is only used in the context of TARGET_J2EE for portlets
webForm.PortalWebForm_DoPostback = function  (id, par, url, apb, pval, tf, csubm, vg)
{
	if (url != null) {
		if (url.indexOf ("vmw.action.page=") == 0) {
			this._form.__NEXTVMWACTIONPAGE.value = url.substring ("vmw.action.page=".length);
			url = this._form.action;
		}
		else if (url.indexOf ("vmw.render.page=") == 0)
		{
			this._form.__NEXTVMWRENDERPAGE.value = url.substring ("vmw.render.page=".length);
			this._form.submit ();
			return;
		}
	}
	return this.WebForm_DoPostback (id, par, url, apb, pval, tf, csubm, vg);
}
webForm.WebForm_DoPostback = function  (id, par, url, apb, pval, tf, csubm, vg)
{
	var validationResult = true;
	if (pval && typeof(this.Page_ClientValidate) == "function")
		validationResult =  this.Page_ClientValidate(vg);

	if (validationResult) {
		if ((typeof(url) != "undefined") && (url != null) && (url.length > 0))
			this._form.action = url;
		if (tf) {
			var lastFocus = this._form.elements["__LASTFOCUS"];
			if ((typeof(lastFocus) != "undefined") && (lastFocus != null))
				lastFocus.value = id;
		}
	}		
	if (csubm)
		this._form.__doPostBack (id, par);
}

webForm.WebForm_DoCallback = function (id, arg, callback, ctx, errorCallback, useAsync)
{
	var qs = this.WebForm_getFormData () + "__CALLBACKTARGET=" + id + "&__CALLBACKARGUMENT=" + encodeURIComponent(arg);
  
  if (this._form["__EVENTVALIDATION"])
    qs += "&__EVENTVALIDATION=" + encodeURIComponent(this._form["__EVENTVALIDATION"].value);
  
	var This = this;
	this.WebForm_httpPost (this._form.serverURL || document.URL, qs, function (httpPost) { This.WebForm_ClientCallback (httpPost, ctx, callback, errorCallback); });
}

webForm.WebForm_ClientCallback = function (httpPost, ctx, callback, errorCallback)
{
	var doc = httpPost.responseText;
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
					var validationFieldElement = this._form["__EVENTVALIDATION"];
					if (!validationFieldElement) {
						validationFieldElement = document.createElement("INPUT");
						validationFieldElement.type = "hidden";
						validationFieldElement.name = "__EVENTVALIDATION";
						validationFieldElement.id = validationFieldElement.name;
						this._form.appendChild(validationFieldElement);
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

webForm.WebForm_getFormData = function ()
{
	var qs = "";
	var len = this._form.elements.length;
	for (n=0; n<len; n++) {
		var elem = this._form.elements [n];
		var tagName = elem.tagName.toLowerCase();
		if (tagName == "input") {
			var type = elem.type;
			if ((type == "text" || type == "hidden" || type == "password" ||
				((type == "checkbox" || type == "radio") && elem.checked)) &&
          (elem.id != "__EVENTVALIDATION")) {
				qs += elem.name + "=" + encodeURIComponent (elem.value) + "&";
			}
		} else if (tagName == "select") {
			var selectCount = elem.options.length;
			for (var j = 0; j < selectCount; j++) {
				var selectChild = elem.options[j];
				if (selectChild.selected == true) {
					qs += elem.name + "=" + encodeURIComponent (elem.value) + "&";
				}
			}
		}
		else if (tagName == "textarea") {
			qs += elem.name + "=" + encodeURIComponent (elem.value) + "&";
		}
	}
	return qs;
}

webForm.WebForm_httpPost = function (url, data, callback)
{
	var httpPost = null;
	
	if (typeof XMLHttpRequest != "undefined") {
		httpPost = new XMLHttpRequest ();
	} else {
		if (this.axName != null)
			httpPost = new ActiveXObject (this.axName);
		else {
			var clsnames = new Array ("MSXML", "MSXML2", "MSXML3", "Microsoft");
			for (n = 0; n < clsnames.length && httpPost == null; n++) {
				this.axName = clsnames [n] + ".XMLHTTP";
				try {
					httpPost = new ActiveXObject (this.axName);
				} catch (e) { this.axName = null; }
			}
			if (httpPost == null)
				throw new Error ("XMLHTTP object could not be created.");
		}
	}
	httpPost.onreadystatechange = function () { if (httpPost.readyState == 4) callback (httpPost); };
	
	httpPost.open ("POST", url, true);	// async
	httpPost.setRequestHeader ("Content-Type", "application/x-www-form-urlencoded");
	setTimeout (function () { httpPost.send (data); }, 10);
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
	var defaultButton = this.WebForm_GetElementById(target);
	if (defaultButton && typeof(defaultButton.click) != "undefined") {
		defaultButton.click();
		event.cancelBubble = true;
		return false;
	}
	return true;
}

webForm.WebForm_SaveScrollPositionSubmit = function ()
{
	var pos = WebForm_GetElementPosition(this);
	this.elements['__SCROLLPOSITIONX'].value = WebForm_GetScrollX() - pos.x;
	this.elements['__SCROLLPOSITIONY'].value = WebForm_GetScrollY() - pos.y;
	if ((typeof(this.oldSubmit) != "undefined") && (this.oldSubmit != null)) {
		return this.oldSubmit();
	}
	return true;
}

webForm.WebForm_SaveScrollPositionOnSubmit = function ()
{
	var pos = WebForm_GetElementPosition(this);
	this.elements['__SCROLLPOSITIONX'].value = WebForm_GetScrollX() - pos.x;
	this.elements['__SCROLLPOSITIONY'].value = WebForm_GetScrollY() - pos.y;
	if ((typeof(this.oldOnSubmit) != "undefined") && (this.oldOnSubmit != null)) {
		return this.oldOnSubmit();
	}
	return true;
}

webForm.WebForm_RestoreScrollPosition = function (currForm)
{
	currForm = currForm || theForm;
	var pos = WebForm_GetElementPosition(currForm);
	var ScrollX = parseInt(currForm.elements['__SCROLLPOSITIONX'].value);
	var ScrollY = parseInt(currForm.elements['__SCROLLPOSITIONY'].value);
	ScrollX = (isNaN(ScrollX)) ? pos.x : (ScrollX + pos.x);
	ScrollY = (isNaN(ScrollY)) ? pos.y : (ScrollY + pos.y);
	window.scrollTo(ScrollX, ScrollY);
	if ((typeof(currForm.oldOnLoad) != "undefined") && (currForm.oldOnLoad != null)) {
		return currForm.oldOnLoad();
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

