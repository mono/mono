/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/**
   File Name:          lexical-021.js
   Corresponds To:     7.4.3-14-n.js
   ECMA Section:       7.4.3

   Description:
   The following words are used as keywords in proposed extensions and are
   therefore reserved to allow for the possibility of future adoption of
   those extensions.

   FutureReservedWord :: one of
   case    debugger    export      super
   catch   default     extends     switch
   class   do          finally     throw
   const   enum        import      try

   Author:             christine@netscape.com
   Date:               12 november 1997
*/
var SECTION = "lexical-021.js";
var VERSION = "ECMA_1";
var TITLE   = "Future Reserved Words";

startTest();
writeHeaderToLog( SECTION + " "+ TITLE);

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

var result = "Failed";
var exception = "No exception thrown";
var expect = "Passed";

try {
  eval("enum = true;");
} catch ( e ) {
  result = expect;
  exception = e.toString();
}

if (typeof prefValue == 'boolean')
{
  setBoolPref("javascript.options.werror", prefValue);
}

new TestCase(
  SECTION,
  "enum = true" +
  " (threw " + exception +")",
  expect,
  result );

test();


