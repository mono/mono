
function WebForm_DoCallback (id, arg, callback, ctx, errorCallback)
{
	var qs = WebForm_getFormData () + "&__CALLBACKTARGET=" + id + "&&__CALLBACKARGUMENT=" + escape(arg);
	WebForm_httpPost (document.URL, qs, function (httpPost) { WebForm_ClientCallback (httpPost, ctx, callback, errorCallback); });
}

function WebForm_ClientCallback (httpPost, ctx, callback, errorCallback)
{
	try {
		var doc = httpPost.responseText;
	} catch (e) {
		if (errorCallback != null)
			errorCallback (httpPost.responseText, ctx);
		return;
	}
	callback (doc, ctx);
}

function WebForm_getFormData ()
{
	var qs = "";
	var len = theForm.elements.length;
	for (n=0; n<len; n++) {
		var elem = theForm.elements [n];
		if (qs.length > 0) qs += "&";
		qs += elem.name + "=" + escape (elem.value);
	}
	return qs;
}

var axName = null;
function WebForm_httpPost (url, data, callback)
{
	var httpPost = null;
	
	if (typeof XMLHttpRequest != "undefined") {
		httpPost = new XMLHttpRequest ();
		httpPost.addEventListener ("load", function () { callback (httpPost);}, false );
	} else {
		if (axName != null)
			httpPost = new ActiveXObject (axName);
		else {
			var clsnames = new Array ("MSXML", "MSXML2", "MSXML3", "Microsoft");
			for (n = 0; n < clsnames.length && httpPost == null; n++) {
				axName = clsnames [n] + ".XMLHTTP";
				try {
					httpPost = new ActiveXObject (axName);
				} catch (e) { axName = null; }
			}
			if (httpPost == null)
				throw new Error ("XMLHTTP object could not be created.");
		}
		httpPost.onreadystatechange = function () { if (httpPost.readyState == 4) callback (httpPost); };
	}
	
	httpPost.open ("POST", url, true);	// async
	httpPost.setRequestHeader ("Content-Type", "application/x-www-form-urlencoded");
	setTimeout (function () { httpPost.send (data); }, 10);
}
