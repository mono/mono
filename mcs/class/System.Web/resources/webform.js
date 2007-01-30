/*
 * WebForm.js
 *
 * Authors:
 *   Chris Toshok (toshok@ximian.com)
 *   Lluis Sanchez Gual (lluis@novell.com)
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


function WebForm_AutoFocus (id)
{
	var x = WebForm_GetElementById (id);

	if (x && (!WebForm_CanFocus(x))) {
		x = WebForm_FindFirstFocusableChild(x);
	}
	if (x) { x.focus(); }
}

function WebForm_CanFocus(element) {
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

function WebForm_FindFirstFocusableChild(element) {
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
				if (WebForm_CanFocus(children[i])) {
					return children[i];
				}
				else {
					var focused = WebForm_FindFirstFocusableChild(children[i]);
					if (WebForm_CanFocus(focused)) {
						return focused;
					}
				}
			} catch (e) {
			}
		}
	}
	return null;
}

function wasControlEnabled (id)
{
	if (typeof (__enabledControlArray) == 'undefined')
		return false;

	for (var i = 0; i < __enabledControlArray.length; i ++) {
		if (id == __enabledControlArray[i])
			return true;
	}

	return false;
}

function WebForm_ReEnableControls (currForm)
{
	currForm = currForm || theForm;
	if (typeof (currForm) == 'undefined')
		return;

	for (var i = 0; i < currForm.childNodes.length; i ++) {
		var node = currForm.childNodes[i];
		if (node.disabled && wasControlEnabled (node.id))
			node.disabled = false;
	}
}

function WebForm_DoPostback (ctrl, par, url, apb, pval, tf, csubm, vg)
{
	if (pval && typeof(Page_ClientValidate) == "function" && !Page_ClientValidate(vg))
		return;

	if (url != null)
		WebForm_GetFormFromCtrl (ctrl).action = url;
		
	if (csubm)
		__doPostBack (ctrl, par);
}

function WebForm_GetFormFromCtrl (id)
{
	// We need to translate the id from ASPX UniqueID to its ClientID.
	var ctrl = WebForm_GetElementById (id.replace(/:/g, "_"));
	while (ctrl != null) {
		if (ctrl.isAspForm)
			return ctrl;
		ctrl = ctrl.parentNode;
	}
	return theForm;
}

function WebForm_GetElementById (id)
{
	return document.getElementById ? document.getElementById (id) :
	       document.all ? document.all [id] :
		   document [id];
}

function WebForm_FireDefaultButton(event, target)
{
	if (event.keyCode != 13) {
		return true;
	}
	if(event.srcElement && (event.srcElement.tagName.toLowerCase() == "textarea")) {
		return true;
	}
	var defaultButton = WebForm_GetElementById(target);
	if (defaultButton && typeof(defaultButton.click) != "undefined") {
		defaultButton.click();
		event.cancelBubble = true;
		return false;
	}
	return true;
}


