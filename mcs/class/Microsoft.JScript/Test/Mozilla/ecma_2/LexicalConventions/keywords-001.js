/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/**
 *  File Name:
 *  ECMA Section:
 *  Description:
 *
 *
 *  Author:             christine@netscape.com
 *  Date:               11 August 1998
 */
var SECTION = "";
var VERSION = "ECMA_2";
var TITLE   = "Keywords";

startTest();

writeLineToLog("This test requires option javascript.options.strict enabled");

var prefValue;
if (typeof document == "undefined" && typeof options == 'function')
{
  options("strict", "werror");
}
else
{
  prefValue = setBoolPref("javascript.options.werror", true);
}

var result = "failed";

try {
  eval("super;");
} 
catch (x) {
  if (x instanceof SyntaxError)
    result = x.name;
}

if (typeof prefValue == 'boolean')
{
  setBoolPref("javascript.options.werror", prefValue);
}

AddTestCase(
  "using the expression \"super\" shouldn't cause js to crash",
  "SyntaxError",
  result );

test();
