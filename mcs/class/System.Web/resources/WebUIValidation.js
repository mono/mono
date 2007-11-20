/*
 * WebUIValidation.js
 *
 * Authors:
 *   Chris Toshok (toshok@ximian.com)
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

var have_validation_summaries = false;

function HaveRegexp ()
{
  if (window.RegExp)
    return true;
  return false;
}

function ValidatorOnLoad ()
{
	if (typeof (Page_ValidationSummaries) != 'undefined' && Page_ValidationSummaries != null)
		have_validation_summaries = true;

	for (var v = 0; v < Page_Validators.length; v++) {
		var vo = Page_Validators [v];
		var funcname = vo.getAttribute ("evaluationfunction");

		func = this[funcname];

		vo["evaluationfunction"] = func;

		if (vo.getAttribute ("isvalid") == null)
			vo.setAttribute ("isvalid", "true");

		if (vo.getAttribute ("enabled") == null)
			vo.setAttribute ("enabled", "true");

		func = vo ["evaluationfunction"];
	}

	Page_ValidationActive = true;
}

var validation_result = true;

function ValidatorCommonOnSubmit ()
{
	/* handle validation summaries here */
	if (validation_result == false && have_validation_summaries) {

		for (var vi in Page_ValidationSummaries)  {
			var vs = Page_ValidationSummaries[vi];

			var header = vs.getAttribute ("headertext");
			if (header == null)
				header = "";

			attr = vs.getAttribute ("showsummary");
			if (attr == null || attr.toLowerCase() == "true") {
				var displaymode = vs.getAttribute ("displaymode");
				if (displaymode == null) displaymode = "Bulleted";

				var html = "";

				if (displaymode == "List") {
					list_pre = "";
					list_post = "";
					item_pre = "";
					item_post = "<br>";
				}
				else if (displaymode == "SingleParagraph") {
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
				for (var v = 0; v < Page_Validators.length; v++) {
					var vo = Page_Validators [v];

					if (vo.getAttribute ("isvalid").toLowerCase() == "false") {
						var text = ValidatorGetErrorMessage (vo);
						if (text != null && text != "") {
							html += item_pre + text + item_post;
						}
					}
				}
				html += list_post;

				vs.innerHTML = html;
				vs.style.display = "block";
			}

			attr = vs.getAttribute ("showmessagebox");
			if (attr != null && attr.toLowerCase() == "true") {
				var v_contents = "";

				for (var v = 0; v < Page_Validators.length; v++) {
					var vo = Page_Validators [v];

					if (vo.getAttribute ("isvalid").toLowerCase() == "false") {
						var text = ValidatorGetErrorMessage (vo);
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

	rv = validation_result;
	validation_result = true;
	return rv;
}

function ValidatorGetValue (controlname)
{
	var el = GetElement (controlname);

	/* if the element has a 'value' attribute, return it */
	if (typeof (el.value) != 'undefined' && el.value != null) {
		return el.value;
	}

	/* if it's a select, loop over the options looking for the
	 * selected one. */
	if (typeof (el.selectedIndex) != 'undefined') {
		return el.options[el.selectedIndex].value;
	}
}

function ValidatorTrim (s)
{
	s = s.replace (/^\s+/g, "");
	s = s.replace (/\s+$/g, "");

	return s;
}

function Page_ClientValidate()
{
	validation_result = true;

	/* clear out the existing text from all our summaries */
	if (have_validation_summaries) {
		for (var vi in Page_ValidationSummaries) {
			var vs = Page_ValidationSummaries[vi];
			vs.style.display = "none";
			vs.innerHTML = "";
		}
	}

	for (var v = 0; v < Page_Validators.length; v++) {
		var vo = Page_Validators [v];
		var evalfunc = vo["evaluationfunction"];
		var result = false;

		if (vo.getAttribute ("enabled").toLowerCase() == "false") {
			result = true;
			ValidatorSucceeded (vo);
		}
		else {
			result = evalfunc (vo);
		}

		if (!result)
			validation_result = false;

		vo.setAttribute("isvalid", result ? "true" : "false");
	}

	return validation_result;
}

/*******************/
/* type converters */

function ToInteger (s, validator)
{
	if ((v = parseInt(s, 10)) != s - 0)
		return null;
	else
		return v;
}

function ToString (s, validator)
{
	return s;
}

function ToDouble (s, validator)
{
	if ((v = parseFloat(s)) != s - 0)
		return null;
	else
		return v;
}

function ToDate (s, validator)
{
  if (!HaveRegexp ())
        return null;
    var m, day, month, year;
    var yearFirstExp = new RegExp("^\\s*((\\d{4})|(\\d{2}))([-/]|\\. ?)(\\d{1,2})\\4(\\d{1,2})\\s*$");
    m = s.match(yearFirstExp);
    if (m != null && (m[2].length == 4 || validator.dateorder == "ymd")) {
        day = m[6];
        month = m[5];
        year = (m[2].length == 4) ? m[2] : GetFullYear(parseInt(m[3], 10), validator.cutoffyear)
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
        year = (m[5].length == 4) ? m[5] : GetFullYear(parseInt(m[6], 10), validator.cutoffyear)
    }
    month -= 1;
    var date = new Date(year, month, day);
    return (typeof(date) == "object" && year == date.getFullYear() && month == date.getMonth() && day == date.getDate()) ? date.valueOf() : null;  
}

function ToCurrency (s)
{
  if (!HaveRegexp ())
    return null;
  
	var hasDigits = (validator.digits > 0);
	var beginGroupSize, subsequentGroupSize;
	var groupSizeNum = parseInt(validator.groupsize, 10);
	if (!isNaN(groupSizeNum) && groupSizeNum > 0) {
		beginGroupSize = "{1," + groupSizeNum + "}";
		subsequentGroupSize = "{" + groupSizeNum + "}";
	} else {
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

function GetFullYear(year, maxYear)
{
    var twoDigitMaxYear = maxYear % 100;
    var centure = maxYear - twoDigitMaxYear;
    return ((year > twoDigitMaxYear) ? (centure - 100 + year) : (centure + year));
}

/*******************/
/* validators	  */

function CompareValidatorEvaluateIsValid (validator)
{
	var ControlToCompare = validator.getAttribute ("controltocompare");
	var ValueToCompare = validator.getAttribute ("valuetocompare");
	var Operator = validator.getAttribute ("operator").toLowerCase();
	var ControlToValidate = validator.getAttribute ("controltovalidate");
	var DataType = validator.getAttribute ("datatype");

	var ctrl_value = ValidatorTrim (ValidatorGetValue (ControlToValidate));
	var compare = (ControlToCompare != null && ControlToCompare != "") ? ValidatorTrim (ValidatorGetValue (ControlToCompare)) : ValueToCompare;

	var left = Convert (ctrl_value, DataType, validator);
 	if (left == null) {
		ValidatorFailed (validator);
		return false;
	}
      
	var right = Convert (compare, DataType, validator);
	if (right == null) {
		ValidatorSucceeded (validator);
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
		ValidatorFailed (validator);
		return false;
	}
	else {
		ValidatorSucceeded (validator);
		return true;
	}
}

function RangeValidatorEvaluateIsValid (validator)
{
	var MinimumValue = parseInt (validator.getAttribute ("minimumvalue"));
	var MaximumValue = parseInt (validator.getAttribute ("maximumvalue"));
	var ControlToValidate = validator.getAttribute ("controltovalidate");
	var DataType = validator.getAttribute ("datatype");

	var ctrl_value = ValidatorTrim (ValidatorGetValue (ControlToValidate));

	if (ctrl_value == "") {
		ValidatorSucceeded (validator);
		return true;
	}

	var val = Convert (ctrl_value, DataType, validator);
	if (val == null || val < MinimumValue || val > MaximumValue) {
		ValidatorFailed (validator);
		return false;
	}
	else {
		ValidatorSucceeded (validator);
		return true;
	}
}

function RegularExpressionValidatorEvaluateIsValid (validator)
{
	var ValidationExpression = validator.getAttribute ("validationexpression");
	var ControlToValidate = validator.getAttribute ("controltovalidate");

	var ctrl_value = ValidatorTrim (ValidatorGetValue (ControlToValidate));

	if (ctrl_value == "") {
		ValidatorSucceeded (validator);
		return true;
	}

  if (!HaveRegexp ())
    return false; // better fail than pass invalidated input to the
                  // backend
  
	var r = new RegExp (ValidationExpression);
	match = r.exec (ctrl_value);
	if (match == null || match[0] != ctrl_value) {
		ValidatorFailed (validator);
		return false;
	}
	else {
		ValidatorSucceeded (validator);
		return true;
	}
}

function RequiredFieldValidatorEvaluateIsValid (validator)
{
	var InitialValue = validator.getAttribute ("initialvalue");
	var ControlToValidate = validator.getAttribute ("controltovalidate");

	var ctrl_value = ValidatorTrim (ValidatorGetValue (ControlToValidate));

	if (ctrl_value == ValidatorTrim (InitialValue)) {
		ValidatorFailed (validator);
		return false;
	}
	else {
		ValidatorSucceeded (validator);
		return true;
	}
}

function CustomValidatorEvaluateIsValid (validator)
{
	var InitialValue = validator.getAttribute ("initialvalue");
	var ControlToValidate = validator.getAttribute ("controltovalidate");

	if (!ControlToValidate) {
		ValidatorSucceeded (validator);
		return true;
	}

	var evaluationfunc = validator.getAttribute ("clientvalidationfunction");

	var ctrl_value = ValidatorTrim (ValidatorGetValue (ControlToValidate));

	var result = true;

	if (evaluationfunc && evaluationfunc != "") {
		args = {Value:ctrl_value, IsValid:false};
		eval (evaluationfunc + "(validator, args)");
		result = args.IsValid;
	}

	if (result) {
		ValidatorSucceeded (validator);
		return true;
	}
	else {
		ValidatorFailed (validator);
		return false;
	}
}

/*********************/
/* utility functions */

function Convert (s, ty, validator)
{
	var cvt = this ["To" + ty];
	if (typeof (cvt) == 'function')
		return cvt (s, validator);
	else
		return null;
}

function ValidatorUpdateDisplay (v, valid)
{
	var display = v.getAttribute ("display");

	/* for validators that aren't displayed, do nothing */
	if (display == "None") {
		return;
	}

	v.style.visibility = (valid ? "hidden" : "visible");
	if (display == "Dynamic") {
		v.style.display = (valid ? "none" : "inline");
	}
}

function ValidatorGetErrorMessage (v)
{
	var text = v.getAttribute ("errormessage");
	if (text == null || text == "")
		text = v.getAttribute ("text");	
	if (text == null)
		text = "";
	return text;
}

function ValidatorGetText (v)
{
	var text = v.getAttribute ("text");	
	if (text == null || text == "")
		text = v.getAttribute ("errormessage");
	if (text == null)
		text = "";
	return text;
}

function ValidatorFailed (v)
{
	var text = ValidatorGetText (v);
	v.innerHTML = text;

	ValidatorUpdateDisplay (v, false);
}

function ValidatorSucceeded (v)
{
	v.innerHTML = "";

	ValidatorUpdateDisplay (v, true);
}

function GetElement(id)
{
	var x = document.getElementById ? document.getElementById (id) :
					  ((document.all) ? document.all [id] : null);
	return x;
}
