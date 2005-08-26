/*
 * jsunit.js
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

/* For the time being, this is a total hack, precariously balanced and
 * ready to tip over if you look at it wrong.  don't look at it
 * wrong. */



var debugging = false;

/* A trace object, that creates a console-esque window and lets us put debugging info there */

Trace = function() { this.Init(); };
Trace.prototype = {
    Init: function () {
	this.w = null;
    },

    ensure_window: function () {
	if (!this.w)
	    this.w = window.open ("", "trace window", "height=300,width=400");
	this.w.focus();

	this.w.document.write ("<head><style>body {color:black; font-size: .7em;}</style></head><body>");
    },

    debug: function (msg){
	if (!debugging) return;

	this.ensure_window ();

	this.w.document.write ( "<p>" + msg + "</p>" );
    }
};

/* An Assert object, to make our tests look a little more like nunit's */
var Assert = {
    IsTrue: function(expr, msg) {
	try {
 	    var result = eval(expr);

	    if (result)
	        test_passed (msg);
	    else
	        test_failed (msg, expr);
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    },

    IsFalse: function(expr, msg) {
	try {
 	    var result = eval(expr);

	    if (!result)
	        test_passed (msg);
	    else
	        test_failed (msg, expr);
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    },

    IsNull: function(expr, msg) {
	try {
 	    var result = eval(expr);

	    if (result == null)
	        test_passed (msg);
	    else
	        test_failed (msg, "'" + encode (expr) + "' returned non-null value '" + result + "'");
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    },

    NotNull: function(expr, msg) {
	try {
 	    var result = eval(expr);

	    if (result != null)
	        test_passed (msg);
	    else
	        test_failed (msg, "'" + encode (expr) + "' returned null");
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    },

    AreEqual: function(expected, expr, msg) {
	try {
	    var result = eval(expr);

	    /* gross hack because mozilla collapses these down to \n, but IE doesn't */
	    result = string_trim (result.replace (/\r\n/g, "\n"));
	    expected = string_trim (expected.replace (/\r\n/g, "\n"));

	    if (result == expected)
	        test_passed (msg);
	    else
                test_failed (msg, "expected (len = " + expected.length + ") &lt;" + encode (expected) + "&gt;, got (len = " + result.length + " ) &lt;" + encode (result) + "&gt; " + string_charcode_diff(expected, result));
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    },

    AreEqualCase: function(expected, expr, msg) {
	try {
	    var result = eval(expr);

	    /* gross hack because mozilla collapses these down to \n, but IE doesn't */
	    result = result.replace (/\r\n/g, "\n");
	    expected = expected.replace (/\r\n/g, "\n");

	    if (string_trim (result.toLowerCase()) == string_trim (expected.toLowerCase()))
	        test_passed (msg);
	    else
                test_failed (msg, "expected (len = " + expected.length + ") &lt;" + encode (expected.toLowerCase()) + "&gt;, got (len = " + result.length + " ) &lt;" + encode (result.toLowerCase()) + "&gt; " + string_charcode_diff(expected.toLowerCase(), result.toLowerCase()));
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    },

    NotEqual: function(expected, expr, msg) {
	try {
	    var result = eval(expr);

	    if (result != expected)
	        test_passed (msg);
	    else
	        test_failed (msg, expr);
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    },

    Contains: function(expected, expr, msg) {
	try {
	    var result = eval(expr);

	    if (result.indexOf (expected) != -1)
	        test_passed (msg);
	    else
	        test_failed (msg, expr);
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    },

    IsFunction: function(expr, msg) {
	try {
	    var result = eval(expr);

	    if (typeof (result) == 'function')
	        test_passed (msg);
	    else
	        test_failed (msg, "expected &lt;function&gt;, got &lt;" + typeof (result) + "&gt;");
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    },

    AttributeHasValue: function (expected, attr, msg) {
	try {
	    var result = JSUnit_GetAttribute (attr);

	    if (result == expected)
	        test_passed (msg);
	    else
	        test_failed (msg, "expected &lt;" + encode (expected) + "&gt;, got &lt;" + (result == null ? "null" : encode (result)) + "&gt;");
	} catch (e) {
	        test_failed (msg, "Exception: " + e.message);
	}
    }
};

/* helper functions for tests */
var element_name;
var bound_element;
var test_causes_page_load = false;

function JSUnit_BindElement (n)
{
    element_name = n;
    bound_element = top.test_run.document.getElementById (element_name);
}

function JSUnit_GetElement(id)
{
    if (typeof (id) == 'undefined')
	return bound_element;
    else
	return top.test_run.document.getElementById (id);
}

function JSUnit_GetAttribute(a, id)
{
    var o = JSUnit_GetElement (id);

    if (o == null)
	return null;
    
    if (o[a])
	return o[a];
    else if (o.getAttribute)
	return o.getAttribute (a);
    else
	return null;
}

function JSUnit_TestCausesPageLoad ()
{
    trace.debug ("in JSUnit_TestCausesPageLoad");
    test_causes_page_load = true;
}

function JSUnit_Click (el)
{
    trace.debug ("in JSUnit_Click");
    if (el == null) {
	trace.debug (" + returning early, element == null");
	return;
    }

    if (test_causes_page_load && !use_onload) {
	top.test_run.waiting = true;
	trace.debug ("adding checkReadState timeout");
	setTimeout ("checkReadyState()", 100);
    }

    if (el.click) {
	trace.debug ("+ using el.click()");
	el.click();
    }
    else if (el.getAttribute ("onClick")) {
	trace.debug ("+ using onClick handler");
	var handler = new Function (el.getAttribute ("onClick"));
	var evt = top.test_run.document.createEvent ("MouseEvents");
	evt.initEvent ("click", true, true);
	handler.call (el, evt);
    }
    else if (el.getAttribute ("href")) {
	var content_window = JSUnit_GetContentWindow (top.test_run.frame);

	trace.debug ("+ setting test_run src = " + el.getAttribute ("href") + " from " + content_window.location.href);

	content_window.location.href = el.getAttribute ("href");
    }
    else {
	alert ("uh oh...");
    }
}

function JSUnit_ExpectFailure(msg)
{
    next_test_expected_failure = true;
    expected_failure_msg = msg;
}

/* the machinery */
var trace = new Trace();
var use_onload = true;
var test_run_loaded = false;
var test_scripts_loaded = false;
var current_testpage = -1;
var current_test = -1;
var current_tests;
var next_test_expected_failure;
var expected_failure_msg;

var current_test_html;
var total_expected_failures = 0;
var total_failed = 0;
var total_tests = 0;
var result_div;

top.test_run = new Object();
top.test_scripts = new Object();
top.test_results = new Object();

function updateStatusText (str)
{
    status_text.innerHTML = "(" + str + ", " + parseInt (((current_testpage + 0.0) / (JSUnit_TestPages.length + 0.0) * 100) + "") + "% completed)";
}

function JSUnit_OnLoad ()
{
    trace.debug ("in JSUnit_Onload");

    top.test_run.frame = document.getElementById ("test-run");

    top.test_scripts.frame = document.getElementById ("test-scripts");

    top.test_results.frame = document.getElementById ("test-results");
    top.test_results.document = JSUnit_GetContentDocument (top.test_results.frame);

    status_text = top.test_results.document.getElementById ("status_text");
    result_div = top.test_results.document.getElementById ("JSUnit_Results");

    if (result_div == null || typeof (result_div) == 'undefined') {
	alert ("Couldn't find result div");
	return;
    }

    result_div.innerHTML = "";

    if (navigator.userAgent.indexOf("MSIE") != -1) {
	use_onload = false;
    }
    else {
	top.test_run.frame.onload = test_run_onload;
	top.test_scripts.frame.onload = test_scripts_onload;
	use_onload = true;
    }

    /* set up the html for the list of pages we're testing */
    for (var i in JSUnit_TestPages) {
	html = "<img style=\"visibility:hidden;\" id=\"spinner-" + JSUnit_TestPages[i].url + "\" src=\"spinner-blue.gif\">";
	html += "<span id=\"indicator-" + JSUnit_TestPages[i].url + "\">&nbsp;</span>";
	html += "<a href=\"javascript:togglediv('results-" + JSUnit_TestPages[i].url + "')\"><b>+</b></a>" +  "<b>" + JSUnit_TestPages[i].url + "</b>: <span id=\"failures-" + JSUnit_TestPages[i].url + "\">&nbsp;</span><br/>\n";
	html += "<div style='margin-left:50px;display:none;' id=\"results-" + JSUnit_TestPages[i].url + "\"></div>\n";

	result_div.innerHTML += html;
    }

    for (var i in JSUnit_TestPages) {
	JSUnit_TestPages[i].results_div = top.test_results.document.getElementById ("results-" + JSUnit_TestPages[i].url);
	JSUnit_TestPages[i].indicator = top.test_results.document.getElementById ("indicator-" + JSUnit_TestPages[i].url);
	JSUnit_TestPages[i].failures_span = top.test_results.document.getElementById ("failures-" + JSUnit_TestPages[i].url);
	JSUnit_TestPages[i].spinner = top.test_results.document.getElementById ("spinner-" + JSUnit_TestPages[i].url);
    }

    jsunit_RunTestPageStep();
}

var query_string_hack = 0;

function checkReadyState ()
{
    var need_timeout = false;

    if ((top.test_run.waiting && JSUnit_GetContentDocument(top.test_run.frame).readyState != "complete")
	|| (top.test_scripts.waiting && JSUnit_GetContentDocument(top.test_scripts.frame).readyState != "complete")) {

	setTimeout("checkReadyState()", 100);
    }

    if (top.test_run.waiting) {
	if (JSUnit_GetContentDocument(top.test_run.frame).readyState == "complete") {
	    top.test_run.waiting = false;
	    test_run_onload();
	}
    }

    if (top.test_scripts.waiting) {
	if (JSUnit_GetContentDocument(top.test_scripts.frame).readyState == "complete") {
	    top.test_run.waiting = false;
	    test_scripts_onload();
	}
    }
}

function jsunit_RunTestPageStep ()
{
    /* first hide the spinner from the old test, if there was one */
    if (current_testpage >= 0) {
	JSUnit_TestPages[current_testpage].spinner.style.visibility="hidden";
    }

    current_testpage ++;
    if (current_testpage >= JSUnit_TestPages.length) {
	jsunit_TestsCompleted();
	return;
    }

    status_text.style.display="inline";
    updateStatusText ("loading " + JSUnit_TestPages[current_testpage].url);
    JSUnit_TestPages[current_testpage].spinner.style.visibility = "visible";

    top.test_run.loaded = false;
    top.test_scripts.loaded = false;

    top.test_run.waiting = true;
    if (JSUnit_TestPages[current_testpage].script) {
	top.test_scripts.waiting = true;
    }
    else {
	top.test_scripts.waiting = false;
    }

    if (!use_onload) {
	setTimeout ("checkReadyState()", 100);
    }

    top.test_run.frame.src = "";
    top.test_scripts.frame.src = "";

    /* start the page loading */
    cw = JSUnit_GetContentWindow (top.test_run.frame);
    cw.location.href = JSUnit_TestPages[current_testpage].url + "?" + query_string_hack;

    /* start the script loading, if there is one */
    if (JSUnit_TestPages[current_testpage].script) {
	cw = JSUnit_GetContentWindow (top.test_scripts.frame);
	cw.location.href = JSUnit_TestPages[current_testpage].script + "?" + query_string_hack;
	query_string_hack++;
    }
}

function jsunit_TestsCompleted ()
{
    trace.debug ("in jsunit_TestsCompleted");
    status_text.style.display="none";

    result_div.innerHTML += "<br/> <b>Totals:</b>  " + total_tests + " tests, " + total_failed + " failure" + (total_failed != 1 ? "s" : "");
    if (total_expected_failures > 0) {
	result_div.innerHTML += " (" + total_expected_failures + " expected)";
    }
    result_div.innerHTML += ".";
}

function jsunit_FindTestFixture ()
{
    var script_context = JSUnit_GetContentWindow (top.test_run.frame);
    if (!script_context['TestFixture']) {
	script_context = JSUnit_GetContentWindow (top.test_scripts.frame);

	if (!script_context['TestFixture'])
	    return;
    }

    top.test_fixture = script_context['TestFixture'];
    top.test_fixture_context = script_context;
}

function jsunit_RunTestsForPage ()
{
    trace.debug ("in jsunit_RunTestsForPage");

    // XXX for now, disable
    //    netscape.security.PrivilegeManager.enablePrivilege("UniversalBrowserRead");

    jsunit_FindTestFixture ();
    current_test = -1;
    current_tests = new Array();
    for (var t in top.test_fixture)
	current_tests.push (t);

    updateStatusText ("testing " + JSUnit_TestPages[current_testpage].url);

    page_total_tests = 0;
    page_total_failed = 0;
    page_total_expected_failures = 0;

    current_test_html = "";

    JSUnit_TestPages[current_testpage].failures_span.innerHTML = "0 tests";

    jsunit_RunTestForPageStep ();
}

function jsunit_RunTestForPageStep ()
{
    trace.debug ("in jsunit_RunTestForPageStep");

    // XXX for now, disable
    //    netscape.security.PrivilegeManager.enablePrivilege("UniversalBrowserRead");

    // need this in case the test fixture was embedded in the test-run
    // page.
    jsunit_FindTestFixture ();

    // add the public api if it's not already there for the page
    if (!top.test_fixture_context['Assert'])
	jsunit_AddPublicAPI (top.test_fixture_context);

    current_test ++;
    if (current_test < current_tests.length) {
	var testfunc = top.test_fixture[current_tests[current_test]];

	if (typeof (testfunc) == 'function') {
	    current_test_html += current_tests[current_test] + "<table width='800' cellpadding='0' cellspacing='0'>\n";
	    try {
		trace.debug ("invoking test: " + current_tests[current_test]);
		testfunc ();
	    } catch (e) {
		test_failed ("test function error", "Exception: " + e.message);
	    }
	    
	    current_test_html += "</table>";
	}

	update_failures_span(false);

	JSUnit_TestPages[current_testpage].results_div.innerHTML = current_test_html;

	if (test_causes_page_load)
	    return;
	else
	    jsunit_RunTestForPageStep ();
    }
    else {
	if (page_total_failed > 0)
	    current_test_html += "test html<br/><textarea cols='100' rows='20'>" + top.test_run.document.body.innerHTML + "</textarea>";

	JSUnit_TestPages[current_testpage].results_div.innerHTML = current_test_html;

	update_failures_span (true);

	/* once we're done with this page, advance to the next */
	jsunit_RunTestPageStep ();
    }
}

function jsunit_AddPublicAPI (ctx)
{
    ctx['JSUnit_ExpectFailure'] = JSUnit_ExpectFailure;
    ctx['JSUnit_Click'] = JSUnit_Click;
    ctx['JSUnit_TestCausesPageLoad'] = JSUnit_TestCausesPageLoad;
    ctx['JSUnit_BindElement'] = JSUnit_BindElement;
    ctx['JSUnit_GetElement'] = JSUnit_GetElement;
    ctx['JSUnit_GetAttribute'] = JSUnit_GetAttribute;
    ctx['Assert'] = Assert;
    ctx['Trace'] = trace;
}

function update_failures_span (finished)
{
	JSUnit_TestPages[current_testpage].failures_span.innerHTML = page_total_tests + " tests, " + page_total_failed + " failure" + (total_failed != 1 ? "s" : "");
	if (page_total_expected_failures > 0) {
	    JSUnit_TestPages[current_testpage].failures_span.innerHTML += " (" + page_total_expected_failures + " expected)";
	}

	if (finished) {
	    /* update the color to either green or red */
            JSUnit_TestPages[current_testpage].failures_span.style.background = (page_total_failed - page_total_expected_failures > 0) ? "#ff0000" : "#00ff00";
        }
        else {
	    /* update the color to red if there's been a failure */
	    if (page_total_failed - page_total_expected_failures > 0)
	        JSUnit_TestPages[current_testpage].failures_span.style.background = "#ff0000";
	}
}

function test_run_onload ()
{
    trace.debug ("in test_run_onload");
    top.test_run.document = JSUnit_GetContentDocument (top.test_run.frame);

    if (test_causes_page_load) {
	trace.debug ("+ resetting causes_page_load");
	test_causes_page_load = false;
	jsunit_RunTestForPageStep ();
	return;
    }

    top.test_run.loaded = true;
    if ((!top.test_scripts.waiting || top.test_scripts.loaded)) {
	trace.debug ("+ starting tests for page");
	/* both the page and its script have been loaded, let's run them */
	jsunit_RunTestsForPage ();
    }
}

function test_scripts_onload ()
{
    trace.debug ("in test_scripts_onload");

    top.test_scripts.document = JSUnit_GetContentDocument (top.test_scripts.frame);
    top.test_scripts.loaded = true;
    if (top.test_run.loaded) {
	trace.debug ("+ starting tests for page");
	/* both the page and its script have been loaded, let's run them */
	jsunit_RunTestsForPage ();
    }
}

/* utility functions */

function JSUnit_GetContentDocument (frame)
{
    if (frame.contentDocument != null)
	return frame.contentDocument;
    else
	return frame.contentWindow.document;
}

function JSUnit_GetContentWindow (frame)
{
    try {
	if (frame.contentDocument && frame.contentDocument.defaultView)
	    return frame.contentDocument.defaultView;
	else
	    return frame.contentWindow;
    } catch (e) {
	trace.debug ("exception getting content window: " + e);
	return null;
    }
}

function test_passed (msg)
{
    next_test_expected_failure = false;
    current_test_html += "<tr class='passed'><td>" + msg + "</td><td width='400'>PASSED</td></tr>";
    page_total_tests ++;
    total_tests ++;
}

function test_failed (msg, extra)
{
    if (next_test_expected_failure) {
	extra += "<br/>Expected failure: " + expected_failure_msg;
	next_test_expected_failure = false;
	page_total_expected_failures ++;
	total_expected_failures ++;
    }
    current_test_html += "<tr class='failed'><td>" + msg + "</td><td width='400'>FAILED " + extra + "</td></tr>";
    page_total_tests ++;
    page_total_failed ++;
    total_tests ++;
    total_failed ++;
}

function update_failures_span (finished)
{
	JSUnit_TestPages[current_testpage].failures_span.innerHTML = page_total_tests + " tests, " + page_total_failed + " failure" + (total_failed != 1 ? "s" : "");
	if (page_total_expected_failures > 0) {
	    JSUnit_TestPages[current_testpage].failures_span.innerHTML += " (" + page_total_expected_failures + " expected)";
	}

	if (finished) {
	    /* update the color to either green or red */
            JSUnit_TestPages[current_testpage].failures_span.style.background = (page_total_failed - page_total_expected_failures > 0) ? "#ff0000" : "#00ff00";
        }
        else {
	    /* update the color to red if there's been a failure */
	    if (page_total_failed - page_total_expected_failures > 0)
	        JSUnit_TestPages[current_testpage].failures_span.style.background = "#ff0000";
	}
}

function encode (str)
{
    return str.replace (/</g, "&lt;").replace (/>/g, "&gt;").replace (/\n/g, "\\n");
}

function string_charcode_diff (str1, str2)
{
    var length = str1.length;
    if (str2.length < str1.length)
	length = str2.length;

    for (i = 0; i < length; i ++) {
	var code1 = str1.charCodeAt (i);
	var code2 = str2.charCodeAt (i);
	if (code1 != code2)
	    return "index = " + i + ", str1 = " + code1 + ", str2 = " + code2;
    }

    return "";
}

function string_trim (s)
{
    s = s.replace (/^\s+/g, "");
    s = s.replace (/\s+$/g, "");

    return s;
}

