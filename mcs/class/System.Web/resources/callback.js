function WebForm_DoCallback (id, arg, callback, ctx, errorCallback)
{
	var myForm = WebForm_GetFormFromCtrl (id);
	var qs = WebForm_getFormData (myForm) + "&__CALLBACKTARGET=" + id + "&&__CALLBACKARGUMENT=" + escape(arg);
	// WebForm_httpPost (myForm.serverURL, qs, function (httpPost) { WebForm_ClientCallback (httpPost, ctx, callback, errorCallback); });
	WebForm_httpPost (document.URL, qs, function (httpPost) { WebForm_ClientCallback (httpPost, ctx, callback, errorCallback); });
}

function WebForm_ClientCallback (httpPost, ctx, callback, errorCallback)
{
	try {
		var doc = httpPost.responseText;
		var separatorIndex = doc.indexOf("|");
		if (separatorIndex != -1) {
			var validationFieldLength = parseInt(doc.substring(0, separatorIndex));
			if (!isNaN(validationFieldLength)) {
				var validationField = doc.substring(separatorIndex + 1, separatorIndex + validationFieldLength + 1);
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
				callback (doc.substring(separatorIndex + validationFieldLength + 1), ctx);
				return;
			}
		}
	} catch (e) {
		if (errorCallback != null)
			errorCallback (httpPost.responseText, ctx);
		return;
	}
	callback (httpPost.responseText, ctx);
}

function WebForm_getFormData (theForm)
{
	var qs = "";
	var len = theForm.elements.length;
	for (n=0; n<len; n++) {
		var elem = theForm.elements [n];
		if (qs.length > 0) qs += "&";
		qs += elem.name + "=" + encodeURIComponent (elem.value);
	}
	return qs;
}

var axName = null;
function WebForm_httpPost (url, data, callback)
{
	var httpPost = null;
	
	if (typeof XMLHttpRequest != "undefined") {
		httpPost = new XMLHttpRequest ();
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
	}
	httpPost.onreadystatechange = function () { if (httpPost.readyState == 4) callback (httpPost); };
	
	httpPost.open ("POST", url, true);	// async
	httpPost.setRequestHeader ("Content-Type", "application/x-www-form-urlencoded");
	setTimeout (function () { httpPost.send (data); }, 10);
}
