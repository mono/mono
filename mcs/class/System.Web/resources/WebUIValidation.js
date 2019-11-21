//<script>
var Page_ValidationVer = "125";
var Page_IsValid = true;
var Page_BlockSubmit = false;
var Page_InvalidControlToBeFocused = null;
var Page_TextTypes = /^(text|password|file|search|tel|url|email|number|range|color|datetime|date|month|week|time|datetime-local)$/i;

 function ValidatorUpdateDisplay(val) {
    if (typeof(val.display) == "string") {
        if (val.display == "None") {
            return;
        }
        if (val.display == "Dynamic") {
            val.style.display = val.isvalid ? "none" : "inline";
            return;
        }
    }

     // VSWhidbey 83164: There is a case that the static error text not shown
    // when the page is browsed on Mac IE that the window is too small to have
    // all controls shown.  The solution is to also set style.display and that
    // seems to get around the problem.
    if ((navigator.userAgent.indexOf("Mac") > -1) &&
        (navigator.userAgent.indexOf("MSIE") > -1)) {
        val.style.display = "inline";
    }
    val.style.visibility = val.isvalid ? "hidden" : "visible";
}

 function ValidatorUpdateIsValid() {
    Page_IsValid = AllValidatorsValid(Page_Validators);
}

 function AllValidatorsValid(validators) {
    if ((typeof(validators) != "undefined") && (validators != null)) {
        var i;
        for (i = 0; i < validators.length; i++) {
            if (!validators[i].isvalid) {
                return false;
            }
        }
    }
    return true;
}

 function ValidatorHookupControlID(controlID, val) {
    if (typeof(controlID) != "string") {
        return;
    }
    var ctrl = document.getElementById(controlID);
    if ((typeof(ctrl) != "undefined") && (ctrl != null)) {
        ValidatorHookupControl(ctrl, val);
    }
    else {
        val.isvalid = true;
        val.enabled = false;
    }
}

 function ValidatorHookupControl(control, val) {
    if (typeof(control.tagName) != "string") {
        return;  // The childNodes collection might contain TextNodes which do not have tagName defined
    }

     if (control.tagName != "INPUT" && control.tagName != "TEXTAREA" && control.tagName != "SELECT") {
        var i;
        for (i = 0; i < control.childNodes.length; i++) {
            ValidatorHookupControl(control.childNodes[i], val);
        }
        return;
    }
    else {
        if (typeof(control.Validators) == "undefined") {
            control.Validators = new Array;
            var eventType;
            if (control.type == "radio") {
                eventType = "onclick";
            } else {
                eventType = "onchange";
                if (typeof(val.focusOnError) == "string" && val.focusOnError == "t") {
                    ValidatorHookupEvent(control, "onblur", "ValidatedControlOnBlur(event); ");
                }
            }

             ValidatorHookupEvent(control, eventType, "ValidatorOnChange(event); ");

             if (Page_TextTypes.test(control.type)) {
                ValidatorHookupEvent(control, "onkeypress", 
                    "event = event || window.event; if (!ValidatedTextBoxOnKeyPress(event)) { event.cancelBubble = true; if (event.stopPropagation) event.stopPropagation(); return false; } ");
            }

         }
        control.Validators[control.Validators.length] = val;
    }
}

 function ValidatorHookupEvent(control, eventType, functionPrefix) {
    var ev = control[eventType];
    if (typeof(ev) == "function") {
        ev = ev.toString();
        ev = ev.substring(ev.indexOf("{") + 1, ev.lastIndexOf("}"));
    }
    else {
        ev = "";
    }
    // in IE the event object is not a parameter, but some libraries pass it in when
    // manually invoking events (jquery), which might be different than the actual window.event.
    control[eventType] = new Function("event", functionPrefix + " " + ev);
}

 function ValidatorGetValue(id) {
    var control;
    control = document.getElementById(id);
    if (typeof(control.value) == "string") {
        return control.value;
    }

     return ValidatorGetValueRecursive(control);
}

 function ValidatorGetValueRecursive(control)
{
    if (typeof(control.value) == "string" && (control.type != "radio" || control.checked == true)) {
        return control.value;
    }
    var i, val;
    for (i = 0; i<control.childNodes.length; i++) {
        val = ValidatorGetValueRecursive(control.childNodes[i]);
        if (val != "") return val;
    }
    return "";
}

 function Page_ClientValidate(validationGroup) {
    Page_InvalidControlToBeFocused = null;

     // On postback login controls can disable all their validators, so Page_Validators can be undefined,
    // this guards against that scenario.
    if (typeof(Page_Validators) == "undefined") {
        return true;
    }
    var i;
    for (i = 0; i < Page_Validators.length; i++) {
        ValidatorValidate(Page_Validators[i], validationGroup, null);
    }
    ValidatorUpdateIsValid();
    ValidationSummaryOnSubmit(validationGroup);
    Page_BlockSubmit = !Page_IsValid;
    return Page_IsValid;
}

 function ValidatorCommonOnSubmit() {
    Page_InvalidControlToBeFocused = null;
    var result = !Page_BlockSubmit;

     // It shouldn't be necessary to set event.returnValue as the function return value should do the job to stop the event.
    // But it was done in V1 so the code is kept here.
    if ((typeof(window.event) != "undefined") && (window.event != null)) {
        window.event.returnValue = result;
    }
    Page_BlockSubmit = false;
    return result;
}

 function ValidatorEnable(val, enable) {
    val.enabled = (enable != false);
    ValidatorValidate(val);
    ValidatorUpdateIsValid();
}

 function ValidatorOnChange(event) {
    event = event || window.event;
    Page_InvalidControlToBeFocused = null;
    var targetedControl;
    if ((typeof(event.srcElement) != "undefined") && (event.srcElement != null)) {
        targetedControl = event.srcElement;
    }
    else {
        targetedControl = event.target;
    }

     var vals;
    if (typeof(targetedControl.Validators) != "undefined") {
        vals = targetedControl.Validators;
    }
    else {
        // On Firefox, the triggered event would be from the label element associated
        // with the input/radio element when the label is clicked.  In this case we
        // need to look for the validators from the associated input element.
        if (targetedControl.tagName.toLowerCase() == "label") {
            targetedControl = document.getElementById(targetedControl.htmlFor);
            vals = targetedControl.Validators;
        }
    }

     if (vals) {
        // Dev10 722166: if vals is undefined, no valdators to validate
        for (var i = 0; i < vals.length; i++) {
            ValidatorValidate(vals[i], null, event);
        }
    }
    ValidatorUpdateIsValid();
}

 function ValidatedTextBoxOnKeyPress(event) {
    event = event || window.event;
    if (event.keyCode == 13) {
        ValidatorOnChange(event);

         // VSWhidbey 284439: The keypress event should return false only when
        // any of the associated validators are invalid.
        var vals;
        if ((typeof(event.srcElement) != "undefined") && (event.srcElement != null)) {
            vals = event.srcElement.Validators;
        }
        else {
            vals = event.target.Validators;
        }
        return AllValidatorsValid(vals);
    }
    return true;
}

 function ValidatedControlOnBlur(event) {
    event = event || window.event;
    var control;
    if ((typeof(event.srcElement) != "undefined") && (event.srcElement != null)) {
        control = event.srcElement;
    }
    else {
        control = event.target;
    }

     if ((typeof(control) != "undefined") && (control != null) && (Page_InvalidControlToBeFocused == control)) {
        control.focus();
        Page_InvalidControlToBeFocused = null;
    }
}

 function ValidatorValidate(val, validationGroup, event) {
    val.isvalid = true;
    if ((typeof(val.enabled) == "undefined" || val.enabled != false) && IsValidationGroupMatch(val, validationGroup)) {
        if (typeof(val.evaluationfunction) == "function") {
            val.isvalid = val.evaluationfunction(val);
            if (!val.isvalid && Page_InvalidControlToBeFocused == null &&
                typeof(val.focusOnError) == "string" && val.focusOnError == "t") {
                ValidatorSetFocus(val, event);
            }
        }
    }
    ValidatorUpdateDisplay(val);
}

 function ValidatorSetFocus(val, event) {
    var ctrl;

     // For CompareValidator that its ControlToCompare triggers the event,
    // we need to correctly identify the control so the focus can be set right.
    if (typeof(val.controlhookup) == "string") {
        var eventCtrl;
        if ((typeof(event) != "undefined") && (event != null)) {
            if ((typeof(event.srcElement) != "undefined") && (event.srcElement != null)) {
                eventCtrl = event.srcElement;
            }
            else {
                eventCtrl = event.target;
            }
        }

         if ((typeof(eventCtrl) != "undefined") && (eventCtrl != null) &&
            (typeof(eventCtrl.id) == "string") &&
            (eventCtrl.id == val.controlhookup)) {
            ctrl = eventCtrl;
        }
    }

     if ((typeof(ctrl) == "undefined") || (ctrl == null)) {
        ctrl = document.getElementById(val.controltovalidate);
    }

     if ((typeof(ctrl) != "undefined") && (ctrl != null) &&
        (ctrl.tagName.toLowerCase() != "table" || (typeof(event) == "undefined") || (event == null)) && // For RadioButtonList, we should not refocus when user selects between radio buttons.
        ((ctrl.tagName.toLowerCase() != "input") || (ctrl.type.toLowerCase() != "hidden")) &&
        (typeof(ctrl.disabled) == "undefined" || ctrl.disabled == null || ctrl.disabled == false) &&
        (typeof(ctrl.visible) == "undefined" || ctrl.visible == null || ctrl.visible != false) &&
        (IsInVisibleContainer(ctrl))) {

         // The focus() method on table only works on IE, but not Firefox and Opera.
        // In those cases for RadioButtonList, we call the focus() method on the last input/radio element.
        // DevDiv Bugs 157647: RadioButtonList renders SPAN in flow mode. The focus() method does not
        // work on spans, in IE or Firefox. Look for last input for spans in all browsers.
        if ((ctrl.tagName.toLowerCase() == "table" && (typeof(__nonMSDOMBrowser) == "undefined" || __nonMSDOMBrowser)) ||
            (ctrl.tagName.toLowerCase() == "span")) {
            var inputElements = ctrl.getElementsByTagName("input");
            var lastInputElement  = inputElements[inputElements.length -1];
            if (lastInputElement != null) {
                ctrl = lastInputElement;
            }
        }

         if (typeof(ctrl.focus) != "undefined" && ctrl.focus != null) {
            ctrl.focus();

             // VSWhidbey 339357: We also need to record the invalid control so the
            // focus can be reset in the onblur event of the validaing control in case
            // that this is simply a tabbing or clicking to a different control.
            Page_InvalidControlToBeFocused = ctrl;
        }
    }
}

 // VSWhidbey 140926
function IsInVisibleContainer(ctrl) {
    if (typeof(ctrl.style) != "undefined" &&
        ( ( typeof(ctrl.style.display) != "undefined" &&
            ctrl.style.display == "none") ||
          ( typeof(ctrl.style.visibility) != "undefined" &&
            ctrl.style.visibility == "hidden") ) ) {
        return false;
    }
    else if (typeof(ctrl.parentNode) != "undefined" &&
             ctrl.parentNode != null &&
             ctrl.parentNode != ctrl) {
        return IsInVisibleContainer(ctrl.parentNode);
    }
    return true;
}

 function IsValidationGroupMatch(control, validationGroup) {

     // When validationGroup is null it means we don't care checking the group
    if ((typeof(validationGroup) == "undefined") || (validationGroup == null)) {
        return true;
    }
    // Default to empty string group
    var controlGroup = "";
    if (typeof(control.validationGroup) == "string") {
        controlGroup = control.validationGroup;
    }
    return (controlGroup == validationGroup);
}

 function ValidatorOnLoad() {
    if (typeof(Page_Validators) == "undefined")
        return;

     var i, val;
    for (i = 0; i < Page_Validators.length; i++) {
        val = Page_Validators[i];
        if (typeof(val.evaluationfunction) == "string") {
            eval("val.evaluationfunction = " + val.evaluationfunction + ";");
        }
        if (typeof(val.isvalid) == "string") {
            if (val.isvalid == "False") {
                val.isvalid = false;
                Page_IsValid = false;
            }
            else {
                val.isvalid = true;
            }
        } else {
            val.isvalid = true;
        }

         if (typeof(val.enabled) == "string") {
            val.enabled = (val.enabled != "False");
        }

         if (typeof(val.controltovalidate) == "string") {
            ValidatorHookupControlID(val.controltovalidate, val);
        }

         if (typeof(val.controlhookup) == "string") {
            ValidatorHookupControlID(val.controlhookup, val);
        }
    }
    Page_ValidationActive = true;
}

 function ValidatorConvert(op, dataType, val) {
    function GetFullYear(year) {
        var twoDigitCutoffYear = val.cutoffyear % 100;
        var cutoffYearCentury = val.cutoffyear - twoDigitCutoffYear;
        return ((year > twoDigitCutoffYear) ? (cutoffYearCentury - 100 + year) : (cutoffYearCentury + year));
    }
    var num, cleanInput, m, exp;
    if (dataType == "Integer") {
        exp = /^\s*[-\+]?\d+\s*$/;
        if (op.match(exp) == null)
            return null;
        num = parseInt(op, 10);
        return (isNaN(num) ? null : num);
    }
    else if(dataType == "Double") {
        exp = new RegExp("^\\s*([-\\+])?(\\d*)\\" + val.decimalchar + "?(\\d*)\\s*$");
        m = op.match(exp);
        if (m == null)
            return null;
        // Make sure there are some valid digits
        if (m[2].length == 0 && m[3].length == 0)
            return null;
        cleanInput = (m[1] != null ? m[1] : "") + (m[2].length>0 ? m[2] : "0") + (m[3].length>0 ? "." + m[3] : "");
        num = parseFloat(cleanInput);
        return (isNaN(num) ? null : num);
    }
    else if (dataType == "Currency") {
        var hasDigits = (val.digits > 0);

         // NDPWhidbey 3352
        var beginGroupSize, subsequentGroupSize;
        var groupSizeNum = parseInt(val.groupsize, 10);
        if (!isNaN(groupSizeNum) && groupSizeNum > 0) {
            beginGroupSize = "{1," + groupSizeNum + "}";
            subsequentGroupSize = "{" + groupSizeNum + "}";
        }
        else {
            beginGroupSize = subsequentGroupSize = "+";
        }

         exp = new RegExp("^\\s*([-\\+])?((\\d" + beginGroupSize + "(\\" + val.groupchar + "\\d" + subsequentGroupSize + ")+)|\\d*)"
                        + (hasDigits ? "\\" + val.decimalchar + "?(\\d{0," + val.digits + "})" : "")
                        + "\\s*$");
        m = op.match(exp);
        if (m == null)
            return null;
        // Make sure there are some valid digits
        if (m[2].length == 0 && hasDigits && m[5].length == 0)
            return null;
        cleanInput = (m[1] != null ? m[1] : "") + m[2].replace(new RegExp("(\\" + val.groupchar + ")", "g"), "") + ((hasDigits && m[5].length > 0) ? "." + m[5] : "");
        num = parseFloat(cleanInput);
        return (isNaN(num) ? null : num);
    }
    else if (dataType == "Date") {
        // ****************************************************************************************************************
        // **                                                                                                            **
        // ** NOTE: When updating the regular expressions in this section, you must also update the regular expressions  **
        // **       in BaseCompareValidator.ConvertDate().  The server and client regular expressions must match.        **
        // **                                                                                                            **
        // ****************************************************************************************************************
        var yearFirstExp = new RegExp("^\\s*((\\d{4})|(\\d{2}))([-/]|\\. ?)(\\d{1,2})\\4(\\d{1,2})\\.?\\s*$");
        m = op.match(yearFirstExp);
        var day, month, year;
        if (m != null && (((typeof(m[2]) != "undefined") && (m[2].length == 4)) || val.dateorder == "ymd")) {
            day = m[6];
            month = m[5];
            year = (m[2].length == 4) ? m[2] : GetFullYear(parseInt(m[3], 10));
        }
        else {
            if (val.dateorder == "ymd"){
                return null;
            }
            var yearLastExp = new RegExp("^\\s*(\\d{1,2})([-/]|\\. ?)(\\d{1,2})(?:\\s|\\2)((\\d{4})|(\\d{2}))(?:\\s\u0433\\.|\\.)?\\s*$");
            m = op.match(yearLastExp);
            if (m == null) {
                return null;
            }
            if (val.dateorder == "mdy") {
                day = m[3];
                month = m[1];
            }
            else {
                day = m[1];
                month = m[3];
            }
            year = ((typeof(m[5]) != "undefined") && (m[5].length == 4)) ? m[5] : GetFullYear(parseInt(m[6], 10));
        }
        month -= 1;
        var date = new Date(year, month, day);

         // If year is 4 digits and older than 100, the Date constructor would
        // automatically add 1900 to it.  In case of it, we use setFullYear
        // method to ensure the expected year is set correctly.
        if (year < 100) {
            date.setFullYear(year);
        }
        return (typeof(date) == "object" && year == date.getFullYear() && month == date.getMonth() && day == date.getDate()) ? date.valueOf() : null;
    }
    else {
        return op.toString();
    }
}

 function ValidatorCompare(operand1, operand2, operator, val) {
    var dataType = val.type;
    var op1, op2;
    if ((op1 = ValidatorConvert(operand1, dataType, val)) == null)
        return false;
    if (operator == "DataTypeCheck")
        return true;
    if ((op2 = ValidatorConvert(operand2, dataType, val)) == null)
        return true;
    switch (operator) {
        case "NotEqual":
            return (op1 != op2);
        case "GreaterThan":
            return (op1 > op2);
        case "GreaterThanEqual":
            return (op1 >= op2);
        case "LessThan":
            return (op1 < op2);
        case "LessThanEqual":
            return (op1 <= op2);
        default:
            return (op1 == op2);
    }
}

 function CompareValidatorEvaluateIsValid(val) {
    var value = ValidatorGetValue(val.controltovalidate);
    if (ValidatorTrim(value).length == 0)
        return true;
    var compareTo = "";
    if ((typeof(val.controltocompare) != "string") ||
        (typeof(document.getElementById(val.controltocompare)) == "undefined") ||
        (null == document.getElementById(val.controltocompare))) {

         if (typeof(val.valuetocompare) == "string") {
            compareTo = val.valuetocompare;
        }
    }
    else {
        compareTo = ValidatorGetValue(val.controltocompare);
    }

     var operator = "Equal";
    if (typeof(val.operator) == "string") {
        operator = val.operator;
    }

     return ValidatorCompare(value, compareTo, operator, val);
}

 function CustomValidatorEvaluateIsValid(val) {
    var value = "";
    if (typeof(val.controltovalidate) == "string") {
        value = ValidatorGetValue(val.controltovalidate);
        // We return if value is empty and validateemptytext is false (default)
        if ((ValidatorTrim(value).length == 0) &&
            ((typeof(val.validateemptytext) != "string") || (val.validateemptytext != "true"))) {
            return true;
        }
    }
    var args = { Value:value, IsValid:true };
    if (typeof(val.clientvalidationfunction) == "string") {
        eval(val.clientvalidationfunction + "(val, args) ;");
    }
    return args.IsValid;
}

 function RegularExpressionValidatorEvaluateIsValid(val) {
    var value = ValidatorGetValue(val.controltovalidate);
    if (ValidatorTrim(value).length == 0)
        return true;
    var rx = new RegExp(val.validationexpression);
    var matches = rx.exec(value);
    return (matches != null && value == matches[0]);
}

 function ValidatorTrim(s) {
    var m = s.match(/^\s*(\S+(\s+\S+)*)\s*$/);
    return (m == null) ? "" : m[1];
}

 function RequiredFieldValidatorEvaluateIsValid(val) {
    return (ValidatorTrim(ValidatorGetValue(val.controltovalidate)) != ValidatorTrim(val.initialvalue))
}

 function RangeValidatorEvaluateIsValid(val) {
    var value = ValidatorGetValue(val.controltovalidate);
    if (ValidatorTrim(value).length == 0)
        return true;
    return (ValidatorCompare(value, val.minimumvalue, "GreaterThanEqual", val) &&
            ValidatorCompare(value, val.maximumvalue, "LessThanEqual", val));
}

 function ValidationSummaryOnSubmit(validationGroup) {
    if (typeof(Page_ValidationSummaries) == "undefined")
        return;
    var summary, sums, s;
    var headerSep, first, pre, post, end;
    for (sums = 0; sums < Page_ValidationSummaries.length; sums++) {
        summary = Page_ValidationSummaries[sums];
        if (!summary) continue;
        summary.style.display = "none";
        if (!Page_IsValid && IsValidationGroupMatch(summary, validationGroup)) {
            var i;
            if (summary.showsummary != "False") {
                summary.style.display = "";
                if (typeof(summary.displaymode) != "string") {
                    summary.displaymode = "BulletList";
                }
                switch (summary.displaymode) {
                    case "List":
                        headerSep = "<br>";
                        first = "";
                        pre = "";
                        post = "<br>";
                        end = "";
                        break;

                     case "BulletList":
                    default:
                        headerSep = "";
                        first = "<ul>";
                        pre = "<li>";
                        post = "</li>";
                        end = "</ul>";
                        break;

                     case "SingleParagraph":
                        headerSep = " ";
                        first = "";
                        pre = "";
                        post = " ";
                        end = "<br>";
                        break;
                }
                s = "";
                if (typeof(summary.headertext) == "string") {
                    s += summary.headertext + headerSep;
                }
                s += first;
                for (i=0; i<Page_Validators.length; i++) {
                    if (!Page_Validators[i].isvalid && typeof(Page_Validators[i].errormessage) == "string") {
                        s += pre + Page_Validators[i].errormessage + post;
                    }
                }
                s += end;
                summary.innerHTML = s;
                window.scrollTo(0,0);
            }
            if (summary.showmessagebox == "True") {
                s = "";
                if (typeof(summary.headertext) == "string") {
                    s += summary.headertext + "\r\n";
                }

                 var lastValIndex = Page_Validators.length - 1;
                for (i=0; i<=lastValIndex; i++) {
                    if (!Page_Validators[i].isvalid && typeof(Page_Validators[i].errormessage) == "string") {
                        switch (summary.displaymode) {
                            case "List":
                                s += Page_Validators[i].errormessage;
                                if (i < lastValIndex) {
                                    s += "\r\n";
                                }
                                break;

                             case "BulletList":
                            default:
                                s += "- " + Page_Validators[i].errormessage;
                                if (i < lastValIndex) {
                                    s += "\r\n";
                                }
                                break;

                             case "SingleParagraph":
                                s += Page_Validators[i].errormessage + " ";
                                break;
                        }
                    }
                }
                alert(s);
            }
        }
    }
}

 if (window.jQuery) {
    (function ($) {
        var dataValidationAttribute = "data-val",
            dataValidationSummaryAttribute = "data-valsummary",
            normalizedAttributes = { validationgroup: "validationGroup", focusonerror: "focusOnError" };

         function getAttributesWithPrefix(element, prefix) {
            // <summary>List all the attributes of an element that starts with the
            // prefix and return them as an object.</summary>
            // <param name="element" type="DOMElement">The element to get the attributes from.</param>
            // <param name="prefix" type="String">The attribute prefix.</param>
            // <returns type="Object" />

             var i,
                attribute,
                list = {},
                attributes = element.attributes,
                length = attributes.length,
                prefixLength = prefix.length;

             prefix = prefix.toLowerCase();
            for (i = 0; i < length; i++) {
                attribute = attributes[i];
                if (attribute.specified && attribute.name.substr(0, prefixLength).toLowerCase() === prefix) {
                    list[attribute.name.substr(prefixLength)] = attribute.value;
                }
            }

             return list;
        }

         function normalizeKey(key) {
            // <summary>Some attributes require to have capital letters.
            // Since the W3C mentions that attribute can potentially be transformed to lower-case,
            // we have to put them back to their original value.</summary>
            // <param name="key" type="string">Key to analyze.</param>
            // <returns type="String" />

             key = key.toLowerCase();
            return normalizedAttributes[key] === undefined ? key : normalizedAttributes[key];
        }

         function addValidationExpando(element) {
            // <summary>Parses the element and add the attributes "nnnn" in data-val-nnnn as expando.</summary>
            // <param name="element" type="DOMElement">The element to add validation expando.</param>

             var attributes = getAttributesWithPrefix(element, dataValidationAttribute + "-");
            $.each(attributes, function (key, value) {
                element[normalizeKey(key)] = value;
            });
        }

         function dispose(element) {
            // <summary>Dispose method used in update panel scenario. Removes the element from the Page_Validators.</summary>
            // <param name="element" type="DOMElement">The element to remove from Page_Validators.</param>

             var index = $.inArray(element, Page_Validators);
            if (index >= 0) {
                Page_Validators.splice(index, 1);
            }
        }

         function addNormalizedAttribute(name, normalizedName) {
            // <summary>Adds a normalized attribute name to the object.</summary>
            // <param name="name" type="String">Attribute name to be normalized.</param>
            // <param name="normalizedName" type="String">Normalized attribute name.</param>

             normalizedAttributes[name.toLowerCase()] = normalizedName;
        }

         function parseSpecificAttribute(selector, attribute, validatorsArray) {
            // <summary>Parses the selector to find validation elements based on the attribute and register them with the validatorsArray.</summary>
            // <param name="selector" type="String">Selector where to search for validation elements.</param>
            // <param name="attribute" type="String">Attribute to look for.</param>
            // <param name="validatorsArray" type="Array">Array to modify.</param>
            // <returns type="Number" />

             return $(selector).find("[" + attribute + "='true']").each(function (index, element) {
                addValidationExpando(element);
                element.dispose = function () { dispose(element); element.dispose = null; };
                if ($.inArray(element, validatorsArray) === -1) {
                    validatorsArray.push(element);
                }
            }).length;
        }

         function parse(selector) {
            // <summary>Parses the selector to find validation elements and register them with the Page_Validators and Page_ValidationSummaries.</summary>
            // <param name="selector" type="String">Selector where to search for validation elements.</param>
            // <returns type="Number" />

             var length = parseSpecificAttribute(selector, dataValidationAttribute, Page_Validators);
            length += parseSpecificAttribute(selector, dataValidationSummaryAttribute, Page_ValidationSummaries);

             return length;
        }

         function loadValidators() {
            // <summary>Initialization of the validators.</summary>
            if (typeof (ValidatorOnLoad) === "function") {
                ValidatorOnLoad();
            }

             if (typeof (ValidatorOnSubmit) === "undefined") {
                window.ValidatorOnSubmit = function () {
                    return Page_ValidationActive ? ValidatorCommonOnSubmit() : true;
                };
            }
        }

         function registerUpdatePanel() {
            // <summary>Registers the script to the PageRequestManager if it exists to handle requests with UpdatePanel.</summary>

             if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                var prm = Sys.WebForms.PageRequestManager.getInstance(),
                    postBackElement, endRequestHandler;

                 // If we are in async postback, it's too late to register the pageLoaded
                // So we will grab the endRequest and we can scan the whole document since
                // we were not previously loaded.
                if (prm.get_isInAsyncPostBack()) {
                    endRequestHandler = function (sender, args) {
                        if (parse(document)) {
                            loadValidators();
                        }

                         prm.remove_endRequest(endRequestHandler);
                        endRequestHandler = null;
                    };
                    prm.add_endRequest(endRequestHandler);
                }

                 prm.add_beginRequest(function (sender, args) {
                    postBackElement = args.get_postBackElement();
                });

                 prm.add_pageLoaded(function (sender, args) {
                    var i, panels, valFound = 0;

                     // Checking if we are doing a postback since pageLoaded is called on PageLoad.
                    if (typeof (postBackElement) === "undefined") {
                        return;
                    }

                     panels = args.get_panelsUpdated();
                    for (i = 0; i < panels.length; i++) {
                        valFound += parse(panels[i]);
                    }

                     panels = args.get_panelsCreated();
                    for (i = 0; i < panels.length; i++) {
                        valFound += parse(panels[i]);
                    }

                     if (valFound) {
                        loadValidators();
                    }
                });
            }
        }

         $(function () {
            // Global variables used by WebUIValidation
            if (typeof (Page_Validators) === "undefined") {
                window.Page_Validators = [];
            }

             if (typeof (Page_ValidationSummaries) === "undefined") {
                window.Page_ValidationSummaries = [];
            }

             if (typeof (Page_ValidationActive) === "undefined") {
                window.Page_ValidationActive = false;
            }

             $.WebFormValidator = {
                addNormalizedAttribute: addNormalizedAttribute,
                parse: parse
            };

             // If we have found something, then we will activate the unobtrusive mode.
            if (parse(document)) {
                loadValidators();
            }

             registerUpdatePanel();
        });
    } (jQuery));
}