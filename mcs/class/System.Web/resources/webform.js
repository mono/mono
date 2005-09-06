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
	var x = document.getElementById ? document.getElementById (id) :
					  ((document.all) ? document.all [id] : null);

	if (typeof (x) != 'undefined') {
		x.focus();
	}
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

function WebForm_ReEnableControls()
{
	if (typeof (theForm) == 'undefined')
		return;

	for (var i = 0; i < theForm.childNodes.length; i ++) {
		var node = theForm.childNodes[i];
		if (node.disabled && wasControlEnabled (node.id))
			node.disabled = false;
	}
}

function WebForm_DoPostback (ctrl, par, url, apb, pval, tf, csubm, vg)
{
	if (pval && typeof(Page_ClientValidate) == "function" && !Page_ClientValidate())
		return;

	if (url != null)
		theForm.action = url;
		
	if (csubm)
		__doPostBack (ctrl, par);
}

