/*
 * WebUIValidation.js
 *
 * Authors:
 *   Chris Toshok (toshok@ximian.com)
 *
 * (c) 2005-2009 Novell, Inc. (http://www.novell.com)
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

function WebFormValidation_Initialize(webForm) {

webForm.have_validation_summaries = false;

webForm.HaveRegexp = function ()
{
  if (window.RegExp)
    return true;
  return false;
}

webForm.ValidatorOnLoad = function  ()
{
	if (typeof (webForm.Page_ValidationSummaries) != 'undefined' && webForm.Page_ValidationSummaries != null) {
		webForm.have_validation_summaries = true;
	}

	if (typeof (webForm.Page_Validators) != 'undefined' || webForm.Page_Validators != null) {
		for (var v = 0; v < webForm.Page_Validators.length; v++) {
			var vo = webForm.Page_Validators [v];

			if (typeof(vo.isvalid) == "string" && vo.isvalid == "False")
				vo._isvalid = false;
			else
				vo._isvalid = true;

			if (typeof(vo.enabled) == "string" && vo.enabled == "False")
				vo._enabled = false;
			else
				vo._enabled = true;
			
			if (typeof(vo.evaluationfunction) == "string")
				vo.evaluationfunction = webForm [vo.evaluationfunction];
		}
	}
	
	webForm.Page_ValidationActive = true;
}

webForm.validation_result = true;

webForm.ValidationSummaryOnSubmit = function (group)
{
	/* handle validation summaries here */
	if (webForm.validation_result == false && webForm.have_validation_summaries) {

	  for (var vi = 0; vi < webForm.Page_ValidationSummaries.length; vi++) {
			var vs = webForm.Page_ValidationSummaries[vi];
		    
		    if(webForm.IsValidationGroupMatch(vs, group)) {

			    var header = "";
			    if(typeof(vs.headertext)=="string")
				    header = vs.headertext;

			    if (vs.showsummary != "False") {
					if (typeof(vs.displaymode) != "string")
						vs.displaymode = "BulletList";

				    var html = "";

				    if (vs.displaymode == "List") {
					    list_pre = "";
					    list_post = "";
					    item_pre = "";
					    item_post = "<br>";
				    }
				    else if (vs.displaymode == "SingleParagraph") {
					    list_pre = "";
					    list_post = "<br>";
					    item_pre = "";
					    item_post = " ";
				    }
				    else {
					    list_pre = "<ul>";
					    list_post = "</ul>";
					    item_pre = "\n<li>";
					    item_post = "</li>";
				    }

				    html += header;
				    html += list_pre;
						for (var v = 0; v < webForm.Page_Validators.length; v++) {
				      var vo = webForm.Page_Validators [v];

					    if (!vo._isvalid) {
						    var text = vo.errormessage;
						    if (text != null && text != "") {
							    html += item_pre + text + item_post;
						    }
					    }
				    }
				    html += list_post;

				    vs.innerHTML = html;
				    vs.style.display = "block";
			    }

			    if (vs.showmessagebox == "True") {
				    var v_contents = "";

						for (var v = 0; v < webForm.Page_Validators.length; v++) {
				      var vo = webForm.Page_Validators [v];

					    if (!vo._isvalid) {
						    var text = vo.errormessage;
						    if (text != null && text != "") {
							    v_contents += "-" + text + "\n";
						    }
					    }
				    }

				    var alert_header = header;
				    if (alert_header != "")
					    alert_header += "\n";
				    summary_contents = alert_header + v_contents;
				    alert (summary_contents);
			    }
			}
		}
	}
}

webForm.ValidatorCommonOnSubmit = function ()
{
	var rv = webForm.validation_result;
	webForm.validation_result = true;
	return rv;
}

webForm.ValidatorGetValue = function (controlname)
{
	var el = webForm.GetElement (controlname);
        if (el == null)
	        return null;

	/* if the element has a 'value' attribute, return it */
	if (typeof (el.value) != 'undefined' && el.value != null) {
		return el.value;
	}

	/* if it's a select, loop over the options looking for the
	 * selected one. */
	if (typeof (el.selectedIndex) != 'undefined') {
		return el.options[el.selectedIndex].value;
	}

	return webForm.ValidatorGetValueRecursive(el);
}

webForm.ValidatorGetValueRecursive = function (el)
{
	if (typeof(el.value) == "string") {
		if (el.type != "radio" || el.checked == true) return el.value;
	}
	for (var i = 0; i<el.childNodes.length; i++) {
		var val = webForm.ValidatorGetValueRecursive(el.childNodes[i]);
		if (val != "") return val;
	}
	return "";
}

webForm.ValidatorTrim = function (s)
{
        if (s == null)
	       return null;

	s = s.replace (/^\s+/g, "");
	s = s.replace (/\s+$/g, "");

	return s;
}

webForm.Page_ClientValidate = function (group)
{
	webForm.validation_result = true;

	/* clear out the existing text from all our summaries */
	if (webForm.have_validation_summaries) {
	  for (var vi = 0; vi < webForm.Page_ValidationSummaries.length; vi++) {
			var vs = webForm.Page_ValidationSummaries[vi];
			vs.style.display = "none";
			vs.innerHTML = "";
		}
	}
	
	var invalidControlHasBeenFocused = false;
	if (typeof (webForm.Page_Validators) != 'undefined' || webForm.Page_Validators != null) {
		for (var v = 0; v < webForm.Page_Validators.length; v++) {
			var vo = webForm.Page_Validators [v];
			var evalfunc = vo.evaluationfunction;
			var result = false;
		        var el = webForm.GetElement (vo.controltovalidate);

		        if (el == null) {
			        result = true;
			        webForm.ValidatorSucceeded (vo);
			} else if (!vo._enabled || !webForm.IsValidationGroupMatch(vo, group)) {
				result = true;
				webForm.ValidatorSucceeded (vo);
			} else {
				result = evalfunc.call (this, vo);
			}

			if (!result) {
				webForm.validation_result = false;
				if (!invalidControlHasBeenFocused && typeof(vo.focusOnError) == "string" && vo.focusOnError == "t") {
					invalidControlHasBeenFocused = webForm.ValidatorSetFocus(vo);
				}
			}
		
			vo._isvalid = result;
		}
		webForm.ValidationSummaryOnSubmit(group);
	}
	
	return webForm.validation_result;
}

webForm.IsValidationGroupMatch = function (vo, group) {
    var valGroup = "";
    if (typeof(vo.validationGroup) == "string")
		valGroup = vo.validationGroup;
    if ((typeof(group) == "undefined") || (group == null)) {
        return (valGroup == "");
    }
    return (valGroup == group);
}

webForm.ValidatorSetFocus = function (val) {
    var ctrl = webForm.GetElement(val.controltovalidate);
	if ((typeof(ctrl) != "undefined") && (ctrl != null) &&
		((ctrl.tagName.toLowerCase() != "input") || (ctrl.type.toLowerCase() != "hidden")) &&
		(typeof(ctrl.disabled) == "undefined" || ctrl.disabled == null || ctrl.disabled == false) &&
		(typeof(ctrl.visible) == "undefined" || ctrl.visible == null || ctrl.visible != false) &&
		(webForm.IsInVisibleContainer(ctrl))) {
		if (ctrl.tagName.toLowerCase() == "table") {
			var inputElements = ctrl.getElementsByTagName("input");
			var lastInputElement  = inputElements[inputElements.length -1];
			if (lastInputElement != null) {
				ctrl = lastInputElement;
			}
		}
		if (typeof(ctrl.focus) != "undefined" && ctrl.focus != null) {
			ctrl.focus();
			return true;
		}
    }
    return false;
}

webForm.IsInVisibleContainer = function (ctrl) {
	if (typeof(ctrl.style) != "undefined" && 
		((typeof(ctrl.style.display) != "undefined" &&	ctrl.style.display == "none") ||
		(typeof(ctrl.style.visibility) != "undefined" && ctrl.style.visibility == "hidden"))) {
		return false;
	}
	else if (typeof(ctrl.parentNode) != "undefined" && ctrl.parentNode != null && ctrl.parentNode != ctrl) {
		return webForm.IsInVisibleContainer(ctrl.parentNode);
	}
	return true;
}

/*******************/
/* type converters */

webForm.ToInteger = function  (s, validator)
{
	if (s.match(/^\s*[-\+]?\d+\s*$/) == null)
		return null;
	var v = parseInt(s, 10);
	return (isNaN(v) ? null : v);
}

webForm.ToString = function (s, validator)
{
	return s;
}

webForm.ToDouble = function (s, validator)
{
	if ((v = parseFloat(s)) != s - 0)
		return null;
	else
		return v;
}

webForm.ToDate = function (s, validator)
{
    if (!webForm.HaveRegexp ())
        return null;
    var m, day, month, year;
    var yearFirstExp = new RegExp("^\\s*((\\d{4})|(\\d{2}))([-/]|\\. ?)(\\d{1,2})\\4(\\d{1,2})\\s*$");
    m = s.match(yearFirstExp);
    if (m != null && (m[2].length == 4 || validator.dateorder == "ymd")) {
        day = m[6];
        month = m[5];
        year = (m[2].length == 4) ? m[2] : webForm.GetFullYear(parseInt(m[3], 10), validator.cutoffyear)
    }
    else {
        if (validator.dateorder == "ymd") return null;
        var yearLastExp = new RegExp("^\\s*(\\d{1,2})([-/]|\\. ?)(\\d{1,2})\\2((\\d{4})|(\\d{2}))\\s*$");
        m = s.match(yearLastExp);
        if (m == null) return null;
        if (validator.dateorder == "mdy") {
            day = m[3];
            month = m[1];
        }
        else {
            day = m[1];
            month = m[3];
        }
        year = (m[5].length == 4) ? m[5] : webForm.GetFullYear(parseInt(m[6], 10), validator.cutoffyear)
    }
    month -= 1;
    var date = new Date(year, month, day);
    return (typeof(date) == "object" && year == date.getFullYear() && month == date.getMonth() && day == date.getDate()) ? date.valueOf() : null;
}

webForm.ToCurrency = function (s, validator)
{
  if (!webForm.HaveRegexp ())
    return null;
  
	var hasDigits = (validator.digits > 0);
	var beginGroupSize, subsequentGroupSize;
	var groupSizeNum = parseInt(validator.groupsize, 10);
	if (!isNaN(groupSizeNum) && groupSizeNum > 0) {
		beginGroupSize = "{1," + groupSizeNum + "}";
		subsequentGroupSize = "{" + groupSizeNum + "}";
	}
	else {
		beginGroupSize = subsequentGroupSize = "+";
	}
	var exp = new RegExp("^\\s*([-\\+])?((\\d" + beginGroupSize + "(\\" + validator.groupchar + "\\d" + subsequentGroupSize + ")+)|\\d*)"
					+ (hasDigits ? "\\" + validator.decimalchar + "?(\\d{0," + validator.digits + "})" : "")
					+ "\\s*$");
	var m = s.match(exp);
	if (m == null)
		return null;
	if (m[2].length == 0 && hasDigits && m[5].length == 0)
		return null;
	var cleanInput = (m[1] != null ? m[1] : "") + m[2].replace(new RegExp("(\\" + validator.groupchar + ")", "g"), "") + ((hasDigits && m[5].length > 0) ? "." + m[5] : "");
	var num = parseFloat(cleanInput);
	return (isNaN(num) ? null : num);
}

webForm.GetFullYear = function (year, maxYear)
{
    var twoDigitMaxYear = maxYear % 100;
    var centure = maxYear - twoDigitMaxYear;
    return ((year > twoDigitMaxYear) ? (centure - 100 + year) : (centure + year));
}

/*******************/
/* validators	  */

webForm.CompareValidatorEvaluateIsValid = function (validator)
{
	var Operator = validator.operator.toLowerCase();
	var ControlToValidate = validator.controltovalidate;
	var DataType = validator.type;

	var ctrl_value = webForm.ValidatorGetValue (ControlToValidate);
	if (webForm.ValidatorTrim (ctrl_value) == "") {
		webForm.ValidatorSucceeded (validator);
		return true;
	}
	
	var left = webForm.Convert (ctrl_value, DataType, validator);
 	if (left == null) {
		webForm.ValidatorFailed (validator);
		return false;
	}
	
	if (Operator == "datatypecheck") {
		webForm.ValidatorSucceeded (validator);
		return true;
	}
      
	var compare = "";
	if (typeof(validator.controltocompare) == "string" && document.getElementById(validator.controltocompare))
		compare = webForm.ValidatorGetValue(validator.controltocompare);
	else if (typeof(validator.valuetocompare) == "string")
		compare = validator.valuetocompare;

	var right = compare != null ? webForm.Convert (compare, DataType, validator) : null;
	if (right == null) {
		webForm.ValidatorSucceeded (validator);
		 return true;
	}

	var result = false;
   
	if (Operator == "equal") {
		result = (left == right);
	}
	else if (Operator == "notequal") {
		result = (left != right);
	}
	else if (Operator == "lessthan") {
		result = (left < right);
	}
	else if (Operator == "lessthanequal") {
		result = (left <= right);
	}
	else if (Operator == "greaterthan") {
		result = (left > right);
	}
	else if (Operator == "greaterthanequal") {
		result = (left >= right);
	}

	if (result == false) {
		webForm.ValidatorFailed (validator);
		return false;
	}
	else {
		webForm.ValidatorSucceeded (validator);
		return true;
	}
}

webForm.RangeValidatorEvaluateIsValid = function (validator)
{
	var ControlToValidate = validator.controltovalidate;
	var DataType = validator.type;

	var ctrl_value = webForm.ValidatorTrim (webForm.ValidatorGetValue (ControlToValidate));

	if (ctrl_value == "") {
		webForm.ValidatorSucceeded (validator);
		return true;
	}

	var MinimumValue = webForm.Convert (validator.minimumvalue, DataType, validator);
	var MaximumValue = webForm.Convert (validator.maximumvalue, DataType, validator);
	var val = webForm.Convert (ctrl_value, DataType, validator);
	if (val == null || val < MinimumValue || val > MaximumValue) {
		webForm.ValidatorFailed (validator);
		return false;
	}
	else {
		webForm.ValidatorSucceeded (validator);
		return true;
	}
}

webForm.RegularExpressionValidatorEvaluateIsValid = function (validator)
{
	var ValidationExpression = validator.validationexpression;
	var ControlToValidate = validator.controltovalidate;

	var ctrl_value = webForm.ValidatorTrim (webForm.ValidatorGetValue (ControlToValidate));

	if (ctrl_value == "") {
		webForm.ValidatorSucceeded (validator);
		return true;
	}

  if (!webForm.HaveRegexp ())
    return false;
  
	var r = new RegExp (ValidationExpression);
	match = r.exec (ctrl_value);
	if (match == null || match[0] != ctrl_value) {
		webForm.ValidatorFailed (validator);
		return false;
	}
	else {
		webForm.ValidatorSucceeded (validator);
		return true;
	}
}

webForm.RequiredFieldValidatorEvaluateIsValid = function (validator)
{
	var InitialValue = validator.initialvalue;
	var ControlToValidate = validator.controltovalidate;

	var ctrl_value = webForm.ValidatorTrim (webForm.ValidatorGetValue (ControlToValidate));
        
	if (ctrl_value == webForm.ValidatorTrim (InitialValue)) {
		webForm.ValidatorFailed (validator);
		return false;
	}
	else {
		webForm.ValidatorSucceeded (validator);
		return true;
	}
}

webForm.CustomValidatorEvaluateIsValid = function (validator)
{
	var ControlToValidate = validator.controltovalidate;
	var evaluationfunc = validator.clientvalidationfunction;
	var ctrl_value;

	if (ControlToValidate) {
	    ctrl_value = webForm.ValidatorTrim (webForm.ValidatorGetValue (ControlToValidate));

	    if ((ctrl_value.length == 0) && ((typeof(validator.validateemptytext) != "string") || (validator.validateemptytext != "true"))) {
		webForm.ValidatorSucceeded (validator);
		return true;
	    }
	} else
	    ctrl_value = "";
	var result = true;

	if (evaluationfunc && evaluationfunc != "") {
		args = {Value:ctrl_value, IsValid:true};
		eval (evaluationfunc + "(validator, args)");
		result = args.IsValid;
	}

	if (result) {
		webForm.ValidatorSucceeded (validator);
		return true;
	}
	else {
		webForm.ValidatorFailed (validator);
		return false;
	}
}

/*********************/
/* utility functions */

webForm.Convert = function (s, ty, validator)
{
	var cvt = this ["To" + ty];
	if (typeof (cvt) == 'function')
		return cvt.call (this, s, validator);
	else
		return null;
}

webForm.ValidatorUpdateDisplay = function (v, valid)
{
	var display = v.display;

	/* for validators that aren't displayed, do nothing */
	if (display == "None") {
		return;
	}

	v.style.visibility = (valid ? "hidden" : "visible");
	if (display == "Dynamic") {
		v.style.display = (valid ? "none" : "inline");
	}
}

webForm.ValidatorFailed = function  (v)
{
	webForm.ValidatorUpdateDisplay (v, false);
}

webForm.ValidatorSucceeded = function  (v)
{
	webForm.ValidatorUpdateDisplay (v, true);
}

webForm.GetElement = function (id)
{
	var x = document.getElementById ? document.getElementById (id) :
					  ((document.all) ? document.all [id] : null);
	return x;
}


}
